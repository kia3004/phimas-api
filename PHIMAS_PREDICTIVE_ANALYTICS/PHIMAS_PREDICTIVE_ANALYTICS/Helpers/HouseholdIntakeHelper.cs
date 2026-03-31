using Microsoft.EntityFrameworkCore;
using PHIMAS_PREDICTIVE_ANALYTICS.Data;
using PHIMAS_PREDICTIVE_ANALYTICS.Models;

namespace PHIMAS_PREDICTIVE_ANALYTICS.Helpers;

public sealed record HouseholdIntakeRequest(
    string PatientName,
    string ContactNumber,
    string Address,
    string? EmergencyContactName,
    string? EmergencyContactNumber);

public sealed record HouseholdIntakeResult(
    Household Household,
    HouseholdMember Patient,
    HouseholdMember EmergencyContact);

public static class HouseholdIntakeHelper
{
    public static async Task<HouseholdIntakeResult> ResolveOrCreateAsync(
        AppDbContext context,
        HouseholdIntakeRequest request,
        CancellationToken cancellationToken = default)
    {
        var patientName = NormalizeRequired(request.PatientName, nameof(request.PatientName));
        var patientContactNumber = NormalizeRequiredContactNumber(request.ContactNumber, nameof(request.ContactNumber));
        var address = NormalizeRequired(request.Address, nameof(request.Address));
        var emergencyContactName = NormalizeOptional(request.EmergencyContactName);
        var emergencyContactNumber = NormalizeOptionalContactNumber(request.EmergencyContactNumber);

        var household = await context.Households
            .Include(item => item.Members)
            .FirstOrDefaultAsync(
                item => item.Address.ToLower() == address.ToLower(),
                cancellationToken);

        if (household == null)
        {
            household = new Household
            {
                Address = address,
                RiskScore = 0f,
                Members = []
            };

            context.Households.Add(household);
        }
        else
        {
            household.Address = address;
            household.RiskScore ??= 0f;
            household.Members ??= [];
        }

        var members = household.Members
            .Where(member => member.HouseholdID == 0 || member.HouseholdID == household.HouseholdID)
            .OrderBy(member => member.MemberID)
            .ToList();

        var patient = members.FirstOrDefault(member =>
                          NamesMatch(member.FullName, patientName) &&
                          ContactNumbersMatch(member.ContactNumber, patientContactNumber))
                      ?? members.FirstOrDefault(member =>
                          NamesMatch(member.FullName, patientName) &&
                          string.IsNullOrWhiteSpace(member.ContactNumber));

        if (patient == null)
        {
            patient = new HouseholdMember
            {
                Household = household,
                FullName = patientName,
                ContactNumber = patientContactNumber
            };

            household.Members.Add(patient);
            context.HouseholdMembers.Add(patient);
            members.Add(patient);
        }
        else
        {
            patient.FullName = patientName;
            patient.ContactNumber = patientContactNumber;
            patient.Household ??= household;
        }

        var hasExplicitEmergencyContact = !string.IsNullOrWhiteSpace(emergencyContactName)
            || !string.IsNullOrWhiteSpace(emergencyContactNumber);

        HouseholdMember emergencyContact;
        if (!hasExplicitEmergencyContact)
        {
            emergencyContact = members.FirstOrDefault(member => member.IsEmergencyContact)
                ?? patient;
        }
        else
        {
            var resolvedEmergencyName = string.IsNullOrWhiteSpace(emergencyContactName)
                ? patientName
                : emergencyContactName!;

            var usePatientAsEmergencyContact = NamesMatch(patient.FullName, resolvedEmergencyName)
                && (string.IsNullOrWhiteSpace(emergencyContactNumber)
                    || ContactNumbersMatch(patient.ContactNumber, emergencyContactNumber));

            if (usePatientAsEmergencyContact)
            {
                emergencyContact = patient;

                if (!string.IsNullOrWhiteSpace(emergencyContactNumber))
                {
                    emergencyContact.ContactNumber = emergencyContactNumber;
                }
            }
            else
            {
                emergencyContact = members.FirstOrDefault(member =>
                                       NamesMatch(member.FullName, resolvedEmergencyName) &&
                                       (string.IsNullOrWhiteSpace(emergencyContactNumber)
                                        || ContactNumbersMatch(member.ContactNumber, emergencyContactNumber)))
                                   ?? members.FirstOrDefault(member =>
                                       NamesMatch(member.FullName, resolvedEmergencyName) &&
                                       string.IsNullOrWhiteSpace(member.ContactNumber))
                                   ?? new HouseholdMember
                                   {
                                       Household = household,
                                       FullName = resolvedEmergencyName,
                                       ContactNumber = emergencyContactNumber ?? string.Empty
                                   };

                if (emergencyContact.MemberID == 0)
                {
                    household.Members.Add(emergencyContact);
                    context.HouseholdMembers.Add(emergencyContact);
                    members.Add(emergencyContact);
                }
                else
                {
                    emergencyContact.Household ??= household;
                }

                emergencyContact.FullName = resolvedEmergencyName;
                if (!string.IsNullOrWhiteSpace(emergencyContactNumber))
                {
                    emergencyContact.ContactNumber = emergencyContactNumber;
                }
            }
        }

        foreach (var member in members)
        {
            member.IsEmergencyContact = ReferenceEquals(member, emergencyContact);
        }

        return new HouseholdIntakeResult(household, patient, emergencyContact);
    }

    public static string NormalizeRequired(string value, string paramName)
    {
        if (string.IsNullOrWhiteSpace(value))
        {
            throw new ArgumentException("Value is required.", paramName);
        }

        return CollapseWhitespace(value);
    }

    public static string? NormalizeOptional(string? value)
    {
        return string.IsNullOrWhiteSpace(value)
            ? null
            : CollapseWhitespace(value);
    }

    public static string NormalizeRequiredContactNumber(string value, string paramName)
    {
        var normalized = NormalizeContactNumber(value);
        if (string.IsNullOrWhiteSpace(normalized))
        {
            throw new ArgumentException("Contact number is required.", paramName);
        }

        return normalized;
    }

    public static string? NormalizeOptionalContactNumber(string? value)
    {
        var normalized = NormalizeContactNumber(value);
        return string.IsNullOrWhiteSpace(normalized) ? null : normalized;
    }

    public static bool NamesMatch(string? left, string? right)
    {
        return string.Equals(
            NormalizeOptional(left),
            NormalizeOptional(right),
            StringComparison.OrdinalIgnoreCase);
    }

    public static bool ContactNumbersMatch(string? left, string? right)
    {
        return string.Equals(
            NormalizeOptionalContactNumber(left),
            NormalizeOptionalContactNumber(right),
            StringComparison.Ordinal);
    }

    private static string CollapseWhitespace(string value)
    {
        return string.Join(
            ' ',
            value
                .Trim()
                .Split(' ', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries));
    }

    private static string? NormalizeContactNumber(string? value)
    {
        if (string.IsNullOrWhiteSpace(value))
        {
            return null;
        }

        var digits = new string(value.Where(char.IsDigit).ToArray());
        return string.IsNullOrWhiteSpace(digits) ? null : digits;
    }
}
