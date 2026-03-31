using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace PHIMAS_PREDICTIVE_ANALYTICS.Models;

public class HealthRecord
{
    private static readonly IReadOnlyDictionary<string, string> SyntheticSymptomsByDisease = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase)
    {
        ["Dengue"] = "Fever, headache, retro-orbital pain",
        ["Influenza"] = "Cough, fever, sore throat",
        ["Leptospirosis"] = "Fever, myalgia, headache"
    };

    [Key]
    public int RecordID { get; set; }

    public int? BHWID { get; set; }

    public int? PatientID { get; set; }

    public DateTime DateRecorded { get; set; } = DateTime.UtcNow;

    [Required]
    [StringLength(100)]
    public string Disease { get; set; } = string.Empty;

    [Required]
    [StringLength(255)]
    public string Symptoms { get; set; } = string.Empty;

    [NotMapped]
    public string DisplaySymptoms => NormalizeSymptomsForDisplay(Symptoms, Disease);

    [NotMapped]
    public bool IsSyntheticVerificationRecord => IsSyntheticVerificationMarker(Symptoms);

    [Required]
    [StringLength(50)]
    public string Status { get; set; } = "Submitted";

    [ForeignKey(nameof(BHWID))]
    public User? BHW { get; set; }

    [ForeignKey(nameof(PatientID))]
    public HouseholdMember? Patient { get; set; }

    [NotMapped]
    public Household? Household => Patient?.Household;

    [NotMapped]
    public HouseholdMember? Member
    {
        get => Patient;
        set => Patient = value;
    }

    [NotMapped]
    public string DisplayAddress => Household?.Address ?? string.Empty;

    [NotMapped]
    public string DisplayPatientName => Patient?.FullName ?? string.Empty;

    [NotMapped]
    public string DisplayHouseholdName => Household?.HouseholdMember ?? string.Empty;

    [NotMapped]
    public string DisplayEmergencyContactName
    {
        get
        {
            return Household?.GetEmergencyContact(PatientID)?.FullName ?? string.Empty;
        }
    }

    [NotMapped]
    public string DisplayEmergencyContactNumber
    {
        get
        {
            return Household?.GetEmergencyContact(PatientID)?.ContactNumber ?? string.Empty;
        }
    }

    public static string NormalizeSymptomsForDisplay(string? symptoms, string? diseaseHint)
    {
        var normalizedSymptoms = symptoms?.Trim();
        if (string.IsNullOrWhiteSpace(normalizedSymptoms))
        {
            return string.Empty;
        }

        if (!IsSyntheticVerificationMarker(normalizedSymptoms))
        {
            return normalizedSymptoms;
        }

        var resolvedDisease = ResolveSyntheticDiseaseName(diseaseHint)
            ?? ResolveSyntheticDiseaseName(ExtractDiseaseFromSyntheticMarker(normalizedSymptoms));

        return resolvedDisease != null && SyntheticSymptomsByDisease.TryGetValue(resolvedDisease, out var mappedSymptoms)
            ? mappedSymptoms
            : "Symptoms recorded in verification batch.";
    }

    public static bool IsSyntheticVerificationMarker(string? symptoms)
    {
        return !string.IsNullOrWhiteSpace(symptoms) &&
               symptoms.Trim().StartsWith("VERIFYPRED-", StringComparison.OrdinalIgnoreCase);
    }

    private static string? ExtractDiseaseFromSyntheticMarker(string symptoms)
    {
        var tokens = symptoms.Split('|', StringSplitOptions.TrimEntries | StringSplitOptions.RemoveEmptyEntries);
        return tokens.Length >= 4 ? tokens[3] : null;
    }

    private static string? ResolveSyntheticDiseaseName(string? value)
    {
        if (string.IsNullOrWhiteSpace(value))
        {
            return null;
        }

        foreach (var disease in SyntheticSymptomsByDisease.Keys)
        {
            if (value.Contains(disease, StringComparison.OrdinalIgnoreCase))
            {
                return disease;
            }
        }

        return null;
    }
}
