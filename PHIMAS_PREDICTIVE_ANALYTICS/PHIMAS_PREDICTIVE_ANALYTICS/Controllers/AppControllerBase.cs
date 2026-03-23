using System.Security.Claims;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using PHIMAS_PREDICTIVE_ANALYTICS.Data;
using PHIMAS_PREDICTIVE_ANALYTICS.Models;
using PHIMAS_PREDICTIVE_ANALYTICS.Models.ViewModels;

namespace PHIMAS_PREDICTIVE_ANALYTICS.Controllers;

public abstract class AppControllerBase : Controller
{
    protected readonly AppDbContext Context;

    protected AppControllerBase(AppDbContext context)
    {
        Context = context;
    }

    protected int? CurrentUserId
    {
        get
        {
            var claim = User.FindFirstValue(ClaimTypes.NameIdentifier);
            return int.TryParse(claim, out var userId) ? userId : null;
        }
    }

    protected async Task<User?> GetCurrentUserAsync()
    {
        if (CurrentUserId is int userId)
        {
            return await Context.Users.FindAsync(userId);
        }

        return null;
    }

    protected static List<PatientLookupViewModel> BuildPatientDirectory(IEnumerable<Household> households)
    {
        var directory = new List<PatientLookupViewModel>();
        foreach (var household in households.OrderBy(item => item.HouseholdMember).ThenBy(item => item.HouseholdID))
        {
            foreach (var patient in household.GetOrderedMembers().Where(member => member.PatientID > 0))
            {
                var emergencyContact = household.GetEmergencyContact(patient.PatientID);
                directory.Add(new PatientLookupViewModel
                {
                    PatientID = patient.PatientID,
                    PatientName = patient.FullName,
                    HouseholdID = household.HouseholdID,
                    HouseholdName = household.HouseholdMember,
                    HouseholdAddress = household.Address,
                    EmergencyContactName = emergencyContact?.FullName ?? string.Empty,
                    EmergencyContactNumber = emergencyContact?.ContactNumber?.Trim() ?? string.Empty,
                    SearchText = string.Join(
                        " ",
                        new[]
                        {
                            patient.FullName,
                            household.HouseholdMember,
                            household.Address,
                            emergencyContact?.FullName ?? string.Empty,
                            emergencyContact?.ContactNumber ?? string.Empty
                        }.Where(value => !string.IsNullOrWhiteSpace(value)))
                });
            }
        }

        return directory
            .OrderBy(item => item.PatientName)
            .ThenBy(item => item.HouseholdAddress)
            .ToList();
    }

    protected async Task<HouseholdMember?> FindPatientAsync(int? patientId)
    {
        if (patientId is not int resolvedPatientId || resolvedPatientId <= 0)
        {
            return null;
        }

        return await Context.HouseholdMembers
            .Include(patient => patient.Household!)
            .ThenInclude(household => household.Members)
            .FirstOrDefaultAsync(patient => patient.MemberID == resolvedPatientId);
    }

    protected async Task SyncHouseholdMembersAsync(Household household)
    {
        if (household.HouseholdID <= 0)
        {
            return;
        }

        var desiredMembers = BuildDesiredMembers(household.HouseholdMember, household.MembersInput);
        if (desiredMembers.Count == 0)
        {
            household.NumberOfMembers = 0;
            return;
        }

        household.NumberOfMembers = desiredMembers.Count;
        var existingMembers = await Context.HouseholdMembers
            .Where(member => member.HouseholdID == household.HouseholdID)
            .OrderBy(member => member.MemberID)
            .ToListAsync();

        var matchedExistingIds = new HashSet<int>();
        foreach (var desiredMember in desiredMembers)
        {
            var existingMember = existingMembers.FirstOrDefault(member =>
                !matchedExistingIds.Contains(member.MemberID) &&
                member.FullName.Equals(desiredMember.FullName, StringComparison.OrdinalIgnoreCase));

            if (existingMember == null)
            {
                Context.HouseholdMembers.Add(new HouseholdMember
                {
                    HouseholdID = household.HouseholdID,
                    FullName = desiredMember.FullName,
                    ContactNumber = desiredMember.ContactNumber,
                    IsEmergencyContact = desiredMember.IsEmergencyContact
                });
                continue;
            }

            matchedExistingIds.Add(existingMember.MemberID);
            existingMember.FullName = desiredMember.FullName;
            existingMember.IsEmergencyContact = desiredMember.IsEmergencyContact;
            existingMember.ContactNumber = string.IsNullOrWhiteSpace(existingMember.ContactNumber)
                ? desiredMember.ContactNumber
                : existingMember.ContactNumber.Trim();
        }

        var removableMembers = existingMembers
            .Where(member => desiredMembers.All(desired =>
                !desired.FullName.Equals(member.FullName, StringComparison.OrdinalIgnoreCase)))
            .ToList();

        if (removableMembers.Count == 0)
        {
            return;
        }

        var removableIds = removableMembers.Select(member => member.MemberID).ToList();
        var referencedIds = await Context.HealthRecords
            .Where(record => record.PatientID.HasValue && removableIds.Contains(record.PatientID.Value))
            .Select(record => record.PatientID!.Value)
            .Union(Context.Reports
                .Where(report => report.PatientID.HasValue && removableIds.Contains(report.PatientID.Value))
                .Select(report => report.PatientID!.Value))
            .ToListAsync();

        Context.HouseholdMembers.RemoveRange(removableMembers.Where(member => !referencedIds.Contains(member.MemberID)));
    }

    protected static string ResolveVisitAddress(string? submittedAddress, string? householdAddress, string? fallbackAddress = null)
    {
        if (!string.IsNullOrWhiteSpace(submittedAddress))
        {
            return submittedAddress.Trim();
        }

        if (!string.IsNullOrWhiteSpace(householdAddress))
        {
            return householdAddress.Trim();
        }

        return fallbackAddress?.Trim() ?? string.Empty;
    }

    protected async Task UpsertEmergencyContactAsync(HouseholdMember patient, string? submittedName, string? submittedNumber)
    {
        var normalizedName = submittedName?.Trim() ?? string.Empty;
        var normalizedNumber = submittedNumber?.Trim() ?? string.Empty;
        if (string.IsNullOrWhiteSpace(normalizedName) && string.IsNullOrWhiteSpace(normalizedNumber))
        {
            return;
        }

        var household = patient.Household;
        if (household == null)
        {
            household = await Context.Households
                .Include(item => item.Members)
                .FirstOrDefaultAsync(item => item.HouseholdID == patient.HouseholdID);
        }

        if (household == null)
        {
            return;
        }

        var members = household.Members
            .Where(member => member.HouseholdID == household.HouseholdID)
            .OrderBy(member => member.MemberID)
            .ToList();

        var emergencyContact = members.FirstOrDefault(member => member.IsEmergencyContact && member.MemberID != patient.MemberID)
            ?? members.FirstOrDefault(member => member.MemberID != patient.MemberID);

        if (emergencyContact == null)
        {
            emergencyContact = new HouseholdMember
            {
                HouseholdID = household.HouseholdID,
                FullName = string.IsNullOrWhiteSpace(normalizedName) ? "Emergency Contact" : normalizedName,
                ContactNumber = string.IsNullOrWhiteSpace(normalizedNumber) ? string.Empty : normalizedNumber,
                IsEmergencyContact = true
            };

            Context.HouseholdMembers.Add(emergencyContact);
            household.Members.Add(emergencyContact);
            household.NumberOfMembers = Math.Max(household.NumberOfMembers, members.Count + 1);
        }
        else
        {
            if (!string.IsNullOrWhiteSpace(normalizedName))
            {
                emergencyContact.FullName = normalizedName;
            }

            if (!string.IsNullOrWhiteSpace(normalizedNumber))
            {
                emergencyContact.ContactNumber = normalizedNumber;
            }

            emergencyContact.IsEmergencyContact = true;
        }

        foreach (var member in members.Where(member =>
                     member.MemberID != emergencyContact.MemberID &&
                     member.IsEmergencyContact))
        {
            member.IsEmergencyContact = false;
        }
    }

    private static List<HouseholdMember> BuildDesiredMembers(string householdHead, string? additionalMembersInput)
    {
        var members = new List<HouseholdMember>();
        AddMember(householdHead, isEmergencyContact: true);

        if (!string.IsNullOrWhiteSpace(additionalMembersInput))
        {
            foreach (var memberName in additionalMembersInput.Split(
                         ["\r\n", "\n", ";", ","],
                         StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries))
            {
                AddMember(memberName, isEmergencyContact: false);
            }
        }

        return members;

        void AddMember(string? fullName, bool isEmergencyContact)
        {
            if (string.IsNullOrWhiteSpace(fullName))
            {
                return;
            }

            var normalizedName = fullName.Trim();
            if (members.Any(member => member.FullName.Equals(normalizedName, StringComparison.OrdinalIgnoreCase)))
            {
                var existingMember = members.First(member => member.FullName.Equals(normalizedName, StringComparison.OrdinalIgnoreCase));
                existingMember.IsEmergencyContact = existingMember.IsEmergencyContact || isEmergencyContact;
                return;
            }

            members.Add(new HouseholdMember
            {
                FullName = normalizedName,
                ContactNumber = string.Empty,
                IsEmergencyContact = isEmergencyContact
            });
        }
    }
}
