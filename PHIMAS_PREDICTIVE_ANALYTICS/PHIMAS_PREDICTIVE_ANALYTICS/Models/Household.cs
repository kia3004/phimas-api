using System.ComponentModel.DataAnnotations;
using System.Text.Json;

namespace PHIMAS_PREDICTIVE_ANALYTICS.Models;

public class Household
{
    private string? _householdMemberOverride;
    private int? _numberOfMembersOverride;

    [Key]
    public int HouseholdID { get; set; }

    [Required]
    [StringLength(200)]
    public string Address { get; set; } = string.Empty;

    public float? RiskScore { get; set; }

    public List<HouseholdMember> Members { get; set; } = [];

    [System.ComponentModel.DataAnnotations.Schema.NotMapped]
    public string HouseholdMember
    {
        get
        {
            if (!string.IsNullOrWhiteSpace(_householdMemberOverride))
            {
                return _householdMemberOverride;
            }

            return GetPrimaryMember()?.FullName ?? $"Household #{HouseholdID}";
        }
        set => _householdMemberOverride = string.IsNullOrWhiteSpace(value) ? null : value.Trim();
    }

    [System.ComponentModel.DataAnnotations.Schema.NotMapped]
    public int NumberOfMembers
    {
        get
        {
            var computedCount = GetOrderedMembers().Count;
            return computedCount > 0 ? computedCount : _numberOfMembersOverride ?? 0;
        }
        set => _numberOfMembersOverride = value;
    }

    [System.ComponentModel.DataAnnotations.Schema.NotMapped]
    public string MembersInput { get; set; } = string.Empty;

    public HouseholdMember? GetPrimaryMember()
    {
        return GetEmergencyContact()
            ?? GetOrderedMembers().FirstOrDefault()
            ?? null;
    }

    public IReadOnlyList<HouseholdMember> GetOrderedMembers()
    {
        if (Members == null || Members.Count == 0)
        {
            return [];
        }

        return Members
            .Where(member => member.HouseholdID == 0 || member.HouseholdID == HouseholdID)
            .OrderByDescending(member => member.IsEmergencyContact)
            .ThenBy(member => member.FullName)
            .ThenBy(member => member.MemberID)
            .ToList();
    }

    public string GetAdditionalMembersText()
    {
        return string.Join(
            Environment.NewLine,
            GetOrderedMembers()
                .Where(member => member != GetPrimaryMember())
                .Select(member => member.FullName));
    }

    public string GetMemberOptionsJson()
    {
        return JsonSerializer.Serialize(
            GetOrderedMembers().Select(member => new
            {
                id = member.MemberID,
                name = member.FullName
            }));
    }

    public bool HasMember(int? memberId)
    {
        return memberId is int resolvedMemberId && GetOrderedMembers().Any(member => member.MemberID == resolvedMemberId);
    }

    public HouseholdMember? GetEmergencyContact(int? patientId = null)
    {
        var members = GetOrderedMembers();
        if (members.Count == 0)
        {
            return null;
        }

        return members.FirstOrDefault(member => member.IsEmergencyContact)
            ?? members.FirstOrDefault(member => !patientId.HasValue || member.MemberID != patientId.Value)
            ?? members.FirstOrDefault();
    }
}
