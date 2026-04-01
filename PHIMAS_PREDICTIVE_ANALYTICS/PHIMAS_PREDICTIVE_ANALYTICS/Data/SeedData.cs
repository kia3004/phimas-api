using Microsoft.EntityFrameworkCore;
using PHIMAS_PREDICTIVE_ANALYTICS.Helpers;
using PHIMAS_PREDICTIVE_ANALYTICS.Models;

namespace PHIMAS_PREDICTIVE_ANALYTICS.Data;

public static class SeedData
{
    private static readonly SeedHouseholdDefinition[] HouseholdSeedDefinitions =
    [
        new("Sevilla, Purok 4, San Fernando City, La Union", 74, "Lucia Dela Cruz", ["Marco Dela Cruz", "Angelica Dela Cruz", "Nico Dela Cruz", "Tessa Dela Cruz", "Joshua Dela Cruz"]),
        new("Sevilla, Riverside, San Fernando City, La Union", 63, "Roberto Castillo", ["Lorie Castillo", "Paolo Castillo", "Aira Castillo", "Jared Castillo", "Mika Castillo"]),
        new("Catbangen, Purok 2, San Fernando City, La Union", 58, "Ana Villanueva", ["Miguel Villanueva", "Clara Villanueva", "Sofia Villanueva", "Ethan Villanueva", "Bea Villanueva"]),
        new("Catbangen, Purok 5, San Fernando City, La Union", 68, "Carlos Reyes", ["Luz Reyes", "Paula Reyes", "Ramon Reyes", "Joy Reyes", "Nina Reyes"]),
        new("San Vicente, Zone 1, San Fernando City, La Union", 82, "Pedro Santos", ["Lorna Santos", "Paolo Santos", "Rina Santos", "Mark Santos", "Ella Santos"]),
        new("San Vicente, Zone 3, San Fernando City, La Union", 69, "Marilyn Aquino", ["Joshua Aquino", "Kate Aquino", "Lester Aquino", "Camille Aquino", "Ralph Aquino"]),
        new("Pagdalagan, Sitio Centro, San Fernando City, La Union", 61, "Edna Ramos", ["Noel Ramos", "Bea Ramos", "Rina Ramos", "Cedrick Ramos", "Pam Ramos"]),
        new("Pagdalagan, Purok 6, San Fernando City, La Union", 54, "Joel Mendoza", ["Arvin Mendoza", "Trisha Mendoza", "Kurt Mendoza", "Faith Mendoza", "Janine Mendoza"]),
        new("Biday, Purok 3, San Fernando City, La Union", 47, "Helen Corpuz", ["Cedric Corpuz", "Mae Corpuz", "Janelle Corpuz", "Bryan Corpuz", "Cyril Corpuz"]),
        new("Tanqui, Purok 1, San Fernando City, La Union", 52, "Rodolfo Flores", ["Karen Flores", "Enzo Flores", "Mara Flores", "Patrick Flores", "Celine Flores"]),
        new("Pagdaraoan, Zone 2, San Fernando City, La Union", 57, "Teresita Navarro", ["Miko Navarro", "Jessa Navarro", "Alden Navarro", "Bianca Navarro", "Marlon Navarro"]),
        new("Camansi, Purok 2, San Fernando City, La Union", 49, "Nestor Valdez", ["Rhea Valdez", "Kyle Valdez", "Marvin Valdez", "Shane Valdez", "Lyn Valdez"]),
        new("Lingsat, Riverside, San Fernando City, La Union", 56, "Perla Mercado", ["Alden Mercado", "Jolina Mercado", "Nash Mercado", "Ella Mercado", "Mavi Mercado"]),
        new("Poro, Sitio Proper, San Fernando City, La Union", 53, "Rosario Sampaga", ["Kevin Sampaga", "Mae Sampaga", "John Paul Sampaga", "Ivy Sampaga", "Sean Sampaga"]),
        new("Ilocanos Norte, Zone 1, San Fernando City, La Union", 59, "Domingo Agcaoili", ["Leah Agcaoili", "Carlo Agcaoili", "Patricia Agcaoili", "Renz Agcaoili", "Mina Agcaoili"]),
        new("Dalumpinas Oeste, Purok 3, San Fernando City, La Union", 46, "Nelia Soriano", ["Jerome Soriano", "Janel Soriano", "Alyssa Soriano", "Vince Soriano", "Mitch Soriano"])
    ];

    private static readonly DateTime HealthRecordSeedStartWeek = new(2026, 1, 19, 0, 0, 0, DateTimeKind.Utc);

    private static readonly HealthRecordSeedTrendDefinition[] HealthRecordSeedTrends =
    [
        new("Sevilla", "Dengue", "Sevilla", [4, 5, 7, 10, 13, 15, 17, 16, 14, 12]),
        new("Sevilla", "Flu", "Sevilla", [2, 3, 4, 5, 6, 7, 6, 5, 4, 3]),
        new("Sevilla", "COVID-19", "Sevilla", [1, 2, 3, 4, 5, 6, 7, 7, 6, 5]),
        new("Sevilla", "Leptospirosis", "Sevilla", [1, 1, 2, 2, 3, 4, 5, 4, 3, 2]),

        new("Catbangen", "Dengue", "Catbangen", [2, 3, 4, 6, 8, 10, 11, 10, 8, 6]),
        new("Catbangen", "Flu", "Catbangen", [4, 5, 7, 9, 12, 14, 16, 15, 13, 11]),
        new("Catbangen", "COVID-19", "Catbangen", [1, 2, 3, 4, 5, 6, 7, 7, 6, 5]),
        new("Catbangen", "Leptospirosis", "Catbangen", [1, 1, 2, 2, 3, 4, 5, 4, 3, 2]),

        new("San Vicente", "Dengue", "San Vicente", [2, 3, 4, 5, 7, 9, 10, 9, 8, 6]),
        new("San Vicente", "Flu", "San Vicente", [2, 3, 4, 5, 6, 7, 6, 5, 4, 3]),
        new("San Vicente", "COVID-19", "San Vicente", [3, 4, 5, 7, 9, 11, 13, 15, 14, 12]),
        new("San Vicente", "Leptospirosis", "San Vicente", [1, 2, 3, 4, 6, 8, 10, 11, 9, 7]),

        new("Pagdalagan", "Dengue", "Sevilla", [2, 3, 4, 6, 8, 9, 10, 9, 8, 6]),
        new("Pagdalagan", "Flu", "Sevilla", [2, 3, 4, 5, 6, 7, 7, 6, 5, 4]),
        new("Pagdalagan", "COVID-19", "Sevilla", [1, 2, 3, 4, 5, 6, 7, 8, 7, 5]),
        new("Pagdalagan", "Leptospirosis", "Sevilla", [1, 2, 3, 5, 7, 9, 12, 15, 13, 10]),

        new("Biday", "Dengue", "Catbangen", [1, 2, 3, 4, 5, 6, 7, 6, 5, 4]),
        new("Biday", "Flu", "Catbangen", [2, 3, 4, 5, 7, 8, 9, 8, 6, 5]),
        new("Biday", "COVID-19", "Catbangen", [1, 1, 2, 3, 4, 5, 6, 6, 5, 4]),
        new("Biday", "Leptospirosis", "Catbangen", [1, 1, 2, 2, 3, 4, 5, 5, 4, 3]),

        new("Tanqui", "Dengue", "Sevilla", [2, 3, 4, 6, 7, 9, 10, 9, 7, 6]),
        new("Tanqui", "Flu", "Sevilla", [3, 4, 5, 6, 8, 10, 11, 10, 8, 6]),
        new("Tanqui", "COVID-19", "Sevilla", [1, 2, 2, 3, 4, 5, 6, 6, 5, 4]),
        new("Tanqui", "Leptospirosis", "Sevilla", [1, 1, 2, 3, 4, 5, 6, 6, 5, 3]),

        new("Pagdaraoan", "Dengue", "San Vicente", [2, 3, 5, 6, 8, 10, 12, 11, 9, 7]),
        new("Pagdaraoan", "Flu", "San Vicente", [2, 3, 4, 5, 6, 7, 7, 6, 5, 4]),
        new("Pagdaraoan", "COVID-19", "San Vicente", [2, 3, 4, 5, 6, 8, 9, 8, 7, 5]),
        new("Pagdaraoan", "Leptospirosis", "San Vicente", [1, 1, 2, 3, 4, 5, 6, 6, 5, 4]),

        new("Camansi", "Dengue", "Catbangen", [1, 2, 3, 4, 5, 6, 7, 6, 5, 4]),
        new("Camansi", "Flu", "Catbangen", [2, 3, 4, 6, 7, 9, 10, 9, 7, 5]),
        new("Camansi", "COVID-19", "Catbangen", [1, 2, 2, 3, 4, 5, 6, 6, 5, 4]),
        new("Camansi", "Leptospirosis", "Catbangen", [1, 2, 2, 3, 4, 5, 7, 6, 5, 4])
    ];

    private static readonly IReadOnlyDictionary<string, string[]> SymptomsByDisease =
        new Dictionary<string, string[]>(StringComparer.OrdinalIgnoreCase)
        {
            ["Dengue"] =
            [
                "High fever, severe headache, joint pain",
                "Fever, retro-orbital pain, muscle aches",
                "High fever, nausea, body pain",
                "Persistent fever, rash, fatigue",
                "Fever, abdominal pain, vomiting",
                "Headache, muscle pain, loss of appetite",
                "High fever, petechial rash, weakness",
                "Fever, chills, joint pain, nausea",
                "Body pain, headache, fever",
                "Fever, dizziness, abdominal discomfort"
            ],
            ["Flu"] =
            [
                "Fever, cough, sore throat",
                "Runny nose, body aches, headache",
                "Cough, chills, nasal congestion",
                "Fever, sore throat, fatigue",
                "Dry cough, headache, body pain",
                "Sneezing, cough, low-grade fever",
                "Fever, cough, runny nose",
                "Chills, sore throat, weakness"
            ],
            ["COVID-19"] =
            [
                "Fever, dry cough, fatigue",
                "Sore throat, fever, headache",
                "Cough, loss of taste, body aches",
                "Fever, nasal congestion, fatigue",
                "Dry cough, chills, weakness",
                "Headache, sore throat, loss of smell",
                "Cough, low-grade fever, muscle aches",
                "Fatigue, fever, mild shortness of breath"
            ],
            ["Leptospirosis"] =
            [
                "Fever, severe muscle pain, headache",
                "Fever, calf tenderness, weakness",
                "Chills, myalgia, red eyes",
                "Body pain, nausea, persistent fever",
                "Headache, fever, muscle stiffness",
                "Mild jaundice, fever, muscle pain",
                "Fever, vomiting, body aches",
                "Fatigue, headache, muscle tenderness"
            ]
        };

    private static readonly string[] RequiredTables =
    [
        "users",
        "households",
        "household_members",
        "health_records",
        "inventory",
        "predictive_analysis",
        "reports",
        "task_assignments"
    ];

    public static async Task ValidateSchemaAsync(AppDbContext context)
    {
        var missingTables = new List<string>();

        foreach (var tableName in RequiredTables)
        {
            if (!await TableExistsAsync(context, tableName))
            {
                missingTables.Add(tableName);
            }
        }

        if (missingTables.Count > 0)
        {
            throw new InvalidOperationException(
                $"Required database tables are missing: {string.Join(", ", missingTables)}. Startup schema creation is disabled; provision the database schema before running the app.");
        }
    }

    public static async Task EnsureSchemaAsync(AppDbContext context)
    {
        await context.Database.ExecuteSqlRawAsync(
            """
            CREATE TABLE IF NOT EXISTS users (
                UserID INT NOT NULL AUTO_INCREMENT PRIMARY KEY,
                FullName VARCHAR(100) NOT NULL,
                Role VARCHAR(50) NOT NULL,
                Email VARCHAR(100) NOT NULL UNIQUE,
                Password VARCHAR(255) NOT NULL,
                ContactNumber VARCHAR(15) NOT NULL,
                IsAvailable TINYINT(1) NOT NULL DEFAULT 0,
                ProfilePicture VARCHAR(200) NULL,
                AssignedArea VARCHAR(200) NULL
            );
            """);

        await context.Database.ExecuteSqlRawAsync(
            """
            CREATE TABLE IF NOT EXISTS households (
                HouseholdID INT NOT NULL AUTO_INCREMENT PRIMARY KEY,
                Address VARCHAR(200) NOT NULL,
                RiskScore DECIMAL(5,2) NOT NULL DEFAULT 0.00,
                INDEX idx_households_address (Address)
            );
            """);

        await context.Database.ExecuteSqlRawAsync(
            """
            CREATE TABLE IF NOT EXISTS household_members (
                PatientID INT NOT NULL AUTO_INCREMENT PRIMARY KEY,
                HouseholdID INT NOT NULL,
                FullName VARCHAR(100) NOT NULL,
                ContactNumber VARCHAR(15) NOT NULL,
                IsEmergencyContact TINYINT(1) NOT NULL DEFAULT 0,
                EmergencyContactHouseholdID INT GENERATED ALWAYS AS (CASE WHEN IsEmergencyContact = 1 THEN HouseholdID ELSE NULL END) STORED,
                CONSTRAINT fk_household_members_household FOREIGN KEY (HouseholdID) REFERENCES households(HouseholdID) ON DELETE CASCADE,
                CONSTRAINT uq_household_members_person UNIQUE (HouseholdID, FullName, ContactNumber),
                CONSTRAINT uq_household_members_emergency_contact UNIQUE (EmergencyContactHouseholdID)
            );
            """);

        await context.Database.ExecuteSqlRawAsync(
            """
            CREATE TABLE IF NOT EXISTS health_records (
                RecordID INT NOT NULL AUTO_INCREMENT PRIMARY KEY,
                PatientID INT NOT NULL,
                BHWID INT NOT NULL,
                DateRecorded DATETIME NOT NULL,
                Disease VARCHAR(100) NOT NULL,
                Symptoms VARCHAR(255) NOT NULL,
                Status VARCHAR(50) NOT NULL,
                CONSTRAINT fk_health_records_patient FOREIGN KEY (PatientID) REFERENCES household_members(PatientID),
                CONSTRAINT fk_health_records_bhw FOREIGN KEY (BHWID) REFERENCES users(UserID)
            );
            """);

        await context.Database.ExecuteSqlRawAsync(
            """
            CREATE TABLE IF NOT EXISTS inventory (
                ItemID INT NOT NULL AUTO_INCREMENT PRIMARY KEY,
                ItemName VARCHAR(100) NOT NULL,
                Unit VARCHAR(50) NOT NULL,
                MinimumThreshold INT NOT NULL,
                CurrentStock INT NOT NULL
            );
            """);

        await context.Database.ExecuteSqlRawAsync(
            """
            CREATE TABLE IF NOT EXISTS predictive_analysis (
                AnalyticsID INT NOT NULL AUTO_INCREMENT PRIMARY KEY,
                DateGenerated DATETIME NOT NULL,
                Disease VARCHAR(100) NOT NULL,
                PredictedCases INT NOT NULL,
                HighRiskBarangay VARCHAR(100) NOT NULL,
                ConfidenceScore DECIMAL(5,2) NOT NULL
            );
            """);

        await context.Database.ExecuteSqlRawAsync(
            """
            CREATE TABLE IF NOT EXISTS reports (
                ReportID INT NOT NULL AUTO_INCREMENT PRIMARY KEY,
                DateGenerated DATETIME NOT NULL,
                GeneratedBy INT NOT NULL,
                PatientID INT NOT NULL,
                ReportType VARCHAR(50) NOT NULL,
                Content TEXT NOT NULL,
                CONSTRAINT fk_reports_generated_by FOREIGN KEY (GeneratedBy) REFERENCES users(UserID),
                CONSTRAINT fk_reports_patient FOREIGN KEY (PatientID) REFERENCES household_members(PatientID)
            );
            """);

        await context.Database.ExecuteSqlRawAsync(
            """
            CREATE TABLE IF NOT EXISTS task_assignments (
                TaskAssignmentID INT NOT NULL AUTO_INCREMENT PRIMARY KEY,
                BHWID INT NULL,
                HouseholdID INT NULL,
                Title VARCHAR(150) NULL,
                TaskDate DATETIME NOT NULL,
                Status VARCHAR(50) NOT NULL,
                Priority VARCHAR(20) NOT NULL,
                Description VARCHAR(200) NOT NULL,
                CONSTRAINT fk_task_assignments_bhw FOREIGN KEY (BHWID) REFERENCES users(UserID),
                CONSTRAINT fk_task_assignments_household FOREIGN KEY (HouseholdID) REFERENCES households(HouseholdID)
            );
            """);

        await AlignThreeNormalFormSchemaAsync(context);
        await MigrateLegacyDataAsync(context);
    }

    public static async Task EnsureSeedDataAsync(AppDbContext context)
    {
        await EnsureSeedUsersAsync(context);
        await EnsureSeedHouseholdsAsync(context);
        await EnsureSeedHouseholdMembersAsync(context);

        if (!await context.Inventory.AnyAsync())
        {
            await context.Inventory.AddRangeAsync(
                new Inventory { ItemName = "Dengue NS1 Kits", Unit = "boxes", MinimumThreshold = 15, CurrentStock = 9 },
                new Inventory { ItemName = "Paracetamol", Unit = "bottles", MinimumThreshold = 20, CurrentStock = 36 },
                new Inventory { ItemName = "ORS Packs", Unit = "packs", MinimumThreshold = 25, CurrentStock = 18 },
                new Inventory { ItemName = "Vitamin A", Unit = "boxes", MinimumThreshold = 10, CurrentStock = 14 });
            await context.SaveChangesAsync();
        }

        await EnsureSeedHealthRecordsAsync(context);

        if (!await context.TaskAssignments.AnyAsync())
        {
            var bhws = await context.Users.Where(user => user.Role == "BHW").OrderBy(user => user.UserID).ToListAsync();
            var householdsByBarangay = (await context.Households.ToListAsync())
                .GroupBy(household => ExtractBarangay(household.Address), StringComparer.OrdinalIgnoreCase)
                .ToDictionary(
                    group => group.Key,
                    group => group.OrderByDescending(household => household.RiskScore).First(),
                    StringComparer.OrdinalIgnoreCase);

            await context.TaskAssignments.AddRangeAsync(
                new TaskAssignment { BHWID = bhws[0].UserID, HouseholdID = householdsByBarangay["Sevilla"].HouseholdID, TaskDate = DateTime.Today.AddHours(9), Priority = "High", Status = "Pending", Title = "Dengue validation visit", Description = "Validate recent dengue symptoms and check for warning signs in Sevilla households." },
                new TaskAssignment { BHWID = bhws[1].UserID, HouseholdID = householdsByBarangay["Catbangen"].HouseholdID, TaskDate = DateTime.Today.AddHours(13), Priority = "Medium", Status = "Started", Title = "Flu follow-up", Description = "Review flu recovery progress and reinforce home isolation guidance." },
                new TaskAssignment { BHWID = bhws[2].UserID, HouseholdID = householdsByBarangay["San Vicente"].HouseholdID, TaskDate = DateTime.Today.AddHours(15), Priority = "High", Status = "Ongoing", Title = "COVID household monitoring", Description = "Monitor symptomatic COVID-19 patients and confirm medication and isolation needs." });
            await context.SaveChangesAsync();
        }

        if (!await context.Reports.AnyAsync())
        {
            var bhwId = await context.Users.Where(user => user.Role == "BHW").OrderBy(user => user.UserID).Select(user => user.UserID).FirstAsync();
            var firstPatient = await context.HouseholdMembers.OrderBy(member => member.MemberID).FirstAsync();

            await context.Reports.AddAsync(new Report
            {
                GeneratedBy = bhwId,
                PatientID = firstPatient.PatientID,
                DateGenerated = DateTime.UtcNow.AddHours(-16),
                ReportType = "Consultation Log",
                Content = "Follow-up visit completed. Symptoms were reviewed, hydration guidance reinforced, and CHO escalation advised."
            });
            await context.SaveChangesAsync();
        }

        if (!await context.PredictiveAnalysis.AnyAsync())
        {
            await context.PredictiveAnalysis.AddRangeAsync(
                new PredictiveAnalysis { DateGenerated = DateTime.UtcNow.AddDays(-3), Disease = "Dengue", PredictedCases = 14, HighRiskBarangay = "Sevilla", ConfidenceScore = 0.89f },
                new PredictiveAnalysis { DateGenerated = DateTime.UtcNow.AddDays(-2), Disease = "Flu", PredictedCases = 11, HighRiskBarangay = "Catbangen", ConfidenceScore = 0.78f },
                new PredictiveAnalysis { DateGenerated = DateTime.UtcNow.AddDays(-1), Disease = "COVID-19", PredictedCases = 7, HighRiskBarangay = "San Vicente", ConfidenceScore = 0.72f },
                new PredictiveAnalysis { DateGenerated = DateTime.UtcNow, Disease = "Leptospirosis", PredictedCases = 9, HighRiskBarangay = "Pagdalagan", ConfidenceScore = 0.76f });
            await context.SaveChangesAsync();
        }
    }

    private static async Task EnsureSeedUsersAsync(AppDbContext context)
    {
        var existingEmails = (await context.Users
                .Select(user => user.Email)
                .ToListAsync())
            .ToHashSet(StringComparer.OrdinalIgnoreCase);
        var usersToAdd = BuildRequiredSeedUsers()
            .Where(user => !existingEmails.Contains(user.Email))
            .ToList();

        if (usersToAdd.Count == 0)
        {
            return;
        }

        await context.Users.AddRangeAsync(usersToAdd);
        await context.SaveChangesAsync();
    }

    private static async Task EnsureSeedHouseholdsAsync(AppDbContext context)
    {
        var existingAddresses = (await context.Households
                .Select(household => household.Address)
                .ToListAsync())
            .ToHashSet(StringComparer.OrdinalIgnoreCase);
        var householdsToAdd = HouseholdSeedDefinitions
            .Where(definition => !existingAddresses.Contains(definition.Address))
            .Select(definition => new Household
            {
                Address = definition.Address,
                RiskScore = definition.RiskScore
            })
            .ToList();

        if (householdsToAdd.Count == 0)
        {
            return;
        }

        await context.Households.AddRangeAsync(householdsToAdd);
        await context.SaveChangesAsync();
    }

    private static async Task EnsureSeedHouseholdMembersAsync(AppDbContext context)
    {
        var seedAddresses = HouseholdSeedDefinitions
            .Select(definition => definition.Address)
            .ToList();
        var households = await context.Households
            .Where(household => seedAddresses.Contains(household.Address))
            .OrderBy(household => household.HouseholdID)
            .ToListAsync();
        var householdsByAddress = households.ToDictionary(
            household => household.Address,
            household => household,
            StringComparer.OrdinalIgnoreCase);
        var householdIds = households.Select(household => household.HouseholdID).ToList();
        var existingMembers = await context.HouseholdMembers
            .Where(member => householdIds.Contains(member.HouseholdID))
            .ToListAsync();
        var existingNamesByHousehold = existingMembers
            .GroupBy(member => member.HouseholdID)
            .ToDictionary(
                group => group.Key,
                group => group
                    .Select(member => member.FullName)
                    .ToHashSet(StringComparer.OrdinalIgnoreCase));
        var householdsWithEmergencyContact = existingMembers
            .Where(member => member.IsEmergencyContact)
            .Select(member => member.HouseholdID)
            .ToHashSet();
        var membersToAdd = new List<HouseholdMember>();

        foreach (var definition in HouseholdSeedDefinitions)
        {
            if (!householdsByAddress.TryGetValue(definition.Address, out var household))
            {
                continue;
            }

            if (!existingNamesByHousehold.TryGetValue(household.HouseholdID, out var existingNames))
            {
                existingNames = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
                existingNamesByHousehold[household.HouseholdID] = existingNames;
            }

            if (!existingNames.Contains(definition.EmergencyContactName) &&
                !householdsWithEmergencyContact.Contains(household.HouseholdID))
            {
                membersToAdd.Add(new HouseholdMember
                {
                    HouseholdID = household.HouseholdID,
                    FullName = definition.EmergencyContactName,
                    ContactNumber = BuildSeedContactNumber(household.HouseholdID, 0),
                    IsEmergencyContact = true
                });
                existingNames.Add(definition.EmergencyContactName);
                householdsWithEmergencyContact.Add(household.HouseholdID);
            }

            for (var memberIndex = 0; memberIndex < definition.PatientNames.Length; memberIndex++)
            {
                var patientName = definition.PatientNames[memberIndex];
                if (existingNames.Contains(patientName))
                {
                    continue;
                }

                membersToAdd.Add(new HouseholdMember
                {
                    HouseholdID = household.HouseholdID,
                    FullName = patientName,
                    ContactNumber = BuildSeedContactNumber(household.HouseholdID, memberIndex + 1),
                    IsEmergencyContact = false
                });
                existingNames.Add(patientName);
            }
        }

        if (membersToAdd.Count == 0)
        {
            return;
        }

        await context.HouseholdMembers.AddRangeAsync(membersToAdd);
        await context.SaveChangesAsync();
    }

    private static async Task EnsureSeedHealthRecordsAsync(AppDbContext context)
    {
        var bhws = await context.Users
            .Where(user => user.Role == "BHW")
            .OrderBy(user => user.UserID)
            .ToListAsync();
        var members = await context.HouseholdMembers
            .Include(member => member.Household)
            .OrderBy(member => member.MemberID)
            .ToListAsync();
        var seedRecords = BuildSeedHealthRecords(bhws, members);
        if (seedRecords.Count == 0)
        {
            return;
        }

        var minDate = seedRecords.Min(record => record.DateRecorded);
        var maxDate = seedRecords.Max(record => record.DateRecorded);
        var diseases = seedRecords
            .Select(record => record.Disease)
            .Distinct(StringComparer.OrdinalIgnoreCase)
            .ToList();
        var existingKeys = (await context.HealthRecords
                .Where(record =>
                    record.DateRecorded >= minDate &&
                    record.DateRecorded <= maxDate &&
                    diseases.Contains(record.Disease))
                .Select(record => new
                {
                    record.PatientID,
                    record.BHWID,
                    record.DateRecorded,
                    record.Disease,
                    record.Symptoms,
                    record.Status
                })
                .ToListAsync())
            .Select(record => BuildHealthRecordSeedKey(
                record.PatientID,
                record.BHWID,
                record.DateRecorded,
                record.Disease,
                record.Symptoms,
                record.Status))
            .ToHashSet(StringComparer.Ordinal);
        var recordsToAdd = seedRecords
            .Where(record => existingKeys.Add(BuildHealthRecordSeedKey(record)))
            .ToList();

        if (recordsToAdd.Count == 0)
        {
            return;
        }

        await context.HealthRecords.AddRangeAsync(recordsToAdd);
        await context.SaveChangesAsync();
    }

    private static IEnumerable<User> BuildRequiredSeedUsers()
    {
        yield return new User
        {
            FullName = "System Administrator",
            Role = "ADMIN",
            Email = "admin@phimas.com",
            Password = PasswordHelper.HashPassword("Admin123!"),
            ContactNumber = "09171234567",
            IsAvailable = false
        };
        yield return new User
        {
            FullName = "Dr. Elena Rivera",
            Role = "CHO",
            Email = "cho@phimas.com",
            Password = PasswordHelper.HashPassword("Cho123!"),
            ContactNumber = "09171234568",
            IsAvailable = false
        };
        yield return new User
        {
            FullName = "Maria Clara",
            Role = "BHW",
            Email = "bhw1@phimas.com",
            Password = PasswordHelper.HashPassword("Bhw123!"),
            ContactNumber = "09170000001",
            IsAvailable = true,
            AssignedArea = "Sevilla"
        };
        yield return new User
        {
            FullName = "Roberto Gomez",
            Role = "BHW",
            Email = "bhw2@phimas.com",
            Password = PasswordHelper.HashPassword("Bhw123!"),
            ContactNumber = "09170000002",
            IsAvailable = true,
            AssignedArea = "Catbangen"
        };
        yield return new User
        {
            FullName = "Liza Santos",
            Role = "BHW",
            Email = "bhw3@phimas.com",
            Password = PasswordHelper.HashPassword("Bhw123!"),
            ContactNumber = "09170000003",
            IsAvailable = false,
            AssignedArea = "San Vicente"
        };
    }

    private static async Task MigrateLegacyDataAsync(AppDbContext context)
    {
        if (await TableHasColumnsAsync(
                context,
                "householdmembers",
                "PatientID",
                "HouseholdID",
                "FullName",
                "ContactNumber",
                "IsEmergencyContact"))
        {
            await TrySqlAsync(context, "INSERT IGNORE INTO household_members (PatientID, HouseholdID, FullName, ContactNumber, IsEmergencyContact) SELECT PatientID, HouseholdID, FullName, COALESCE(ContactNumber, ''), COALESCE(IsEmergencyContact, 0) FROM householdmembers;");
        }

        if (await TableHasColumnsAsync(
                context,
                "healthrecords",
                "RecordID",
                "PatientID",
                "BHWID",
                "DateRecorded",
                "Disease",
                "Symptoms",
                "Status"))
        {
            await TrySqlAsync(context, "INSERT IGNORE INTO health_records (RecordID, PatientID, BHWID, DateRecorded, Disease, Symptoms, Status) SELECT RecordID, PatientID, BHWID, DateRecorded, Disease, LEFT(COALESCE(Symptoms, ''), 255), COALESCE(Status, 'Submitted') FROM healthrecords WHERE PatientID IS NOT NULL AND BHWID IS NOT NULL;");
        }

        if (await TableHasColumnsAsync(
                context,
                "taskassignments",
                "TaskAssignmentID",
                "BHWID",
                "HouseholdID",
                "TaskDate",
                "Status",
                "Priority",
                "Description"))
        {
            if (await ColumnExistsAsync(context, "taskassignments", "Title"))
            {
                await TrySqlAsync(context, "INSERT IGNORE INTO task_assignments (TaskAssignmentID, BHWID, HouseholdID, Title, TaskDate, Status, Priority, Description) SELECT TaskAssignmentID, BHWID, HouseholdID, LEFT(NULLIF(Title, ''), 150), TaskDate, COALESCE(Status, 'Pending'), COALESCE(Priority, 'Medium'), LEFT(COALESCE(Description, 'Migrated task'), 200) FROM taskassignments;");
            }
            else
            {
                await TrySqlAsync(context, "INSERT IGNORE INTO task_assignments (TaskAssignmentID, BHWID, HouseholdID, TaskDate, Status, Priority, Description) SELECT TaskAssignmentID, BHWID, HouseholdID, TaskDate, COALESCE(Status, 'Pending'), COALESCE(Priority, 'Medium'), LEFT(COALESCE(Description, 'Migrated task'), 200) FROM taskassignments;");
            }
        }

        if (await TableHasColumnsAsync(
                context,
                "predictiveanalyses",
                "AnalyticsID",
                "DateGenerated",
                "Disease",
                "PredictedCases",
                "HighRiskBarangay",
                "ConfidenceScore"))
        {
            await TrySqlAsync(context, "INSERT IGNORE INTO predictive_analysis (AnalyticsID, DateGenerated, Disease, PredictedCases, HighRiskBarangay, ConfidenceScore) SELECT AnalyticsID, DateGenerated, Disease, PredictedCases, HighRiskBarangay, ConfidenceScore FROM predictiveanalyses;");
        }
    }

    private static async Task TrySqlAsync(AppDbContext context, string sql)
    {
        try
        {
            await context.Database.ExecuteSqlRawAsync(sql);
        }
        catch
        {
        }
    }

    private static async Task AlignThreeNormalFormSchemaAsync(AppDbContext context)
    {
        await DropLegacyColumnAsync(context, "health_records", "HouseholdID");
        await DropLegacyColumnAsync(context, "health_records", "VisitAddress");
        await DropLegacyColumnAsync(context, "health_records", "PatientName");
        await DropLegacyColumnAsync(context, "health_records", "HouseholdAddress");
        await DropLegacyColumnAsync(context, "health_records", "EmergencyContactName");
        await DropLegacyColumnAsync(context, "health_records", "EmergencyContactNumber");

        await DropLegacyColumnAsync(context, "reports", "HouseholdID");
        await DropLegacyColumnAsync(context, "reports", "VisitAddress");
        await DropLegacyColumnAsync(context, "reports", "PatientName");
        await DropLegacyColumnAsync(context, "reports", "HouseholdAddress");
        await DropLegacyColumnAsync(context, "reports", "EmergencyContactName");
        await DropLegacyColumnAsync(context, "reports", "EmergencyContactNumber");

        if (!await IndexExistsAsync(context, "households", "idx_households_address"))
        {
            await TrySqlAsync(context, "ALTER TABLE households ADD KEY idx_households_address (Address);");
        }

        if (!await ColumnExistsAsync(context, "household_members", "EmergencyContactHouseholdID"))
        {
            await TrySqlAsync(
                context,
                "ALTER TABLE household_members ADD COLUMN EmergencyContactHouseholdID INT GENERATED ALWAYS AS (CASE WHEN IsEmergencyContact = 1 THEN HouseholdID ELSE NULL END) STORED;");
        }

        if (!await IndexExistsAsync(context, "household_members", "uq_household_members_emergency_contact"))
        {
            await TrySqlAsync(context, "ALTER TABLE household_members ADD UNIQUE KEY uq_household_members_emergency_contact (EmergencyContactHouseholdID);");
        }

        if (!await ColumnExistsAsync(context, "task_assignments", "Title"))
        {
            await TrySqlAsync(context, "ALTER TABLE task_assignments ADD COLUMN Title VARCHAR(150) NULL AFTER HouseholdID;");
        }

        await TrySqlAsync(context, "ALTER TABLE task_assignments MODIFY Title VARCHAR(150) NULL;");
        await TrySqlAsync(context, "ALTER TABLE health_records MODIFY PatientID INT NOT NULL, MODIFY BHWID INT NOT NULL;");
        await TrySqlAsync(context, "ALTER TABLE reports MODIFY GeneratedBy INT NOT NULL, MODIFY PatientID INT NOT NULL;");
    }

    private static async Task<bool> TableExistsAsync(AppDbContext context, string tableName)
    {
        return await WithOpenConnectionAsync(context, async connection =>
        {
            await using var command = connection.CreateCommand();
            command.CommandText = "SELECT COUNT(*) FROM INFORMATION_SCHEMA.TABLES WHERE TABLE_SCHEMA = DATABASE() AND TABLE_NAME = @tableName;";
            var parameter = command.CreateParameter();
            parameter.ParameterName = "@tableName";
            parameter.Value = tableName;
            command.Parameters.Add(parameter);
            return Convert.ToInt32(await command.ExecuteScalarAsync()) > 0;
        });
    }

    private static async Task<bool> TableHasColumnsAsync(AppDbContext context, string tableName, params string[] columnNames)
    {
        if (!await TableExistsAsync(context, tableName))
        {
            return false;
        }

        foreach (var columnName in columnNames)
        {
            if (!await ColumnExistsAsync(context, tableName, columnName))
            {
                return false;
            }
        }

        return true;
    }

    private static async Task<bool> ColumnExistsAsync(AppDbContext context, string tableName, string columnName)
    {
        return await WithOpenConnectionAsync(context, async connection =>
        {
            await using var command = connection.CreateCommand();
            command.CommandText = "SELECT COUNT(*) FROM INFORMATION_SCHEMA.COLUMNS WHERE TABLE_SCHEMA = DATABASE() AND TABLE_NAME = @tableName AND COLUMN_NAME = @columnName;";

            var tableParameter = command.CreateParameter();
            tableParameter.ParameterName = "@tableName";
            tableParameter.Value = tableName;
            command.Parameters.Add(tableParameter);

            var columnParameter = command.CreateParameter();
            columnParameter.ParameterName = "@columnName";
            columnParameter.Value = columnName;
            command.Parameters.Add(columnParameter);

            return Convert.ToInt32(await command.ExecuteScalarAsync()) > 0;
        });
    }

    private static async Task<bool> IndexExistsAsync(AppDbContext context, string tableName, string indexName)
    {
        return await WithOpenConnectionAsync(context, async connection =>
        {
            await using var command = connection.CreateCommand();
            command.CommandText = "SELECT COUNT(*) FROM INFORMATION_SCHEMA.STATISTICS WHERE TABLE_SCHEMA = DATABASE() AND TABLE_NAME = @tableName AND INDEX_NAME = @indexName;";

            var tableParameter = command.CreateParameter();
            tableParameter.ParameterName = "@tableName";
            tableParameter.Value = tableName;
            command.Parameters.Add(tableParameter);

            var indexParameter = command.CreateParameter();
            indexParameter.ParameterName = "@indexName";
            indexParameter.Value = indexName;
            command.Parameters.Add(indexParameter);

            return Convert.ToInt32(await command.ExecuteScalarAsync()) > 0;
        });
    }

    private static async Task DropLegacyColumnAsync(AppDbContext context, string tableName, string columnName)
    {
        if (!await ColumnExistsAsync(context, tableName, columnName))
        {
            return;
        }

        var foreignKeys = await GetForeignKeysForColumnAsync(context, tableName, columnName);
        foreach (var foreignKey in foreignKeys)
        {
            await TrySqlAsync(context, $"ALTER TABLE `{tableName}` DROP FOREIGN KEY `{foreignKey}`;");
        }

        var indexes = await GetIndexesForColumnAsync(context, tableName, columnName);
        foreach (var index in indexes)
        {
            await TrySqlAsync(context, $"ALTER TABLE `{tableName}` DROP INDEX `{index}`;");
        }

        await TrySqlAsync(context, $"ALTER TABLE `{tableName}` DROP COLUMN `{columnName}`;");
    }

    private static async Task<List<string>> GetForeignKeysForColumnAsync(AppDbContext context, string tableName, string columnName)
    {
        return await WithOpenConnectionAsync(context, async connection =>
        {
            await using var command = connection.CreateCommand();
            command.CommandText =
                """
                SELECT CONSTRAINT_NAME
                FROM INFORMATION_SCHEMA.KEY_COLUMN_USAGE
                WHERE TABLE_SCHEMA = DATABASE()
                  AND TABLE_NAME = @tableName
                  AND COLUMN_NAME = @columnName
                  AND REFERENCED_TABLE_NAME IS NOT NULL;
                """;

            var tableParameter = command.CreateParameter();
            tableParameter.ParameterName = "@tableName";
            tableParameter.Value = tableName;
            command.Parameters.Add(tableParameter);

            var columnParameter = command.CreateParameter();
            columnParameter.ParameterName = "@columnName";
            columnParameter.Value = columnName;
            command.Parameters.Add(columnParameter);

            var foreignKeys = new List<string>();
            await using var reader = await command.ExecuteReaderAsync();
            while (await reader.ReadAsync())
            {
                foreignKeys.Add(reader.GetString(0));
            }

            return foreignKeys;
        });
    }

    private static async Task<List<string>> GetIndexesForColumnAsync(AppDbContext context, string tableName, string columnName)
    {
        return await WithOpenConnectionAsync(context, async connection =>
        {
            await using var command = connection.CreateCommand();
            command.CommandText =
                """
                SELECT DISTINCT INDEX_NAME
                FROM INFORMATION_SCHEMA.STATISTICS
                WHERE TABLE_SCHEMA = DATABASE()
                  AND TABLE_NAME = @tableName
                  AND COLUMN_NAME = @columnName
                  AND INDEX_NAME <> 'PRIMARY';
                """;

            var tableParameter = command.CreateParameter();
            tableParameter.ParameterName = "@tableName";
            tableParameter.Value = tableName;
            command.Parameters.Add(tableParameter);

            var columnParameter = command.CreateParameter();
            columnParameter.ParameterName = "@columnName";
            columnParameter.Value = columnName;
            command.Parameters.Add(columnParameter);

            var indexes = new List<string>();
            await using var reader = await command.ExecuteReaderAsync();
            while (await reader.ReadAsync())
            {
                indexes.Add(reader.GetString(0));
            }

            return indexes;
        });
    }

    private static async Task<T> WithOpenConnectionAsync<T>(
        AppDbContext context,
        Func<System.Data.Common.DbConnection, Task<T>> action)
    {
        var connection = context.Database.GetDbConnection();
        var shouldClose = connection.State != System.Data.ConnectionState.Open;

        if (shouldClose)
        {
            await connection.OpenAsync();
        }

        try
        {
            return await action(connection);
        }
        finally
        {
            if (shouldClose && connection.State == System.Data.ConnectionState.Open)
            {
                await connection.CloseAsync();
            }
        }
    }

    private static IReadOnlyList<HealthRecord> BuildSeedHealthRecords(
        IReadOnlyList<User> bhws,
        IReadOnlyList<HouseholdMember> members)
    {
        var bhwIdsByArea = bhws
            .Where(user => !string.IsNullOrWhiteSpace(user.AssignedArea))
            .GroupBy(user => user.AssignedArea!.Trim(), StringComparer.OrdinalIgnoreCase)
            .ToDictionary(
                group => group.Key,
                group => group.First().UserID,
                StringComparer.OrdinalIgnoreCase);

        var patientsByBarangay = members
            .Where(member => member.Household != null)
            .GroupBy(member => ExtractBarangay(member.Household!.Address), StringComparer.OrdinalIgnoreCase)
            .ToDictionary(
                group => group.Key,
                group => group.OrderBy(member => member.FullName).ThenBy(member => member.PatientID).ToList(),
                StringComparer.OrdinalIgnoreCase);

        var patientOffsetsByBarangay = patientsByBarangay.Keys.ToDictionary(
            barangay => barangay,
            _ => 0,
            StringComparer.OrdinalIgnoreCase);
        var records = new List<HealthRecord>();

        for (var trendIndex = 0; trendIndex < HealthRecordSeedTrends.Length; trendIndex++)
        {
            var trend = HealthRecordSeedTrends[trendIndex];

            if (!patientsByBarangay.TryGetValue(trend.Barangay, out var patients) || patients.Count == 0)
            {
                throw new InvalidOperationException(
                    $"Seed data requires at least one household member in {trend.Barangay}, but none were found.");
            }

            if (!bhwIdsByArea.TryGetValue(trend.BhwArea, out var bhwId))
            {
                throw new InvalidOperationException(
                    $"Seed data requires a BHW assigned to {trend.BhwArea}.");
            }

            var symptoms = SymptomsByDisease[trend.Disease];
            for (var weekIndex = 0; weekIndex < trend.WeeklyCaseCounts.Length; weekIndex++)
            {
                var weekStart = BuildSeedWeekStart(weekIndex);
                var caseCount = trend.WeeklyCaseCounts[weekIndex];
                var startIndex = patientOffsetsByBarangay[trend.Barangay];
                var rotationBase = startIndex + (trendIndex * 2) + weekIndex;

                for (var caseIndex = 0; caseIndex < caseCount; caseIndex++)
                {
                    var patient = patients[(rotationBase + caseIndex) % patients.Count];

                    records.Add(new HealthRecord
                    {
                        PatientID = patient.PatientID,
                        BHWID = bhwId,
                        DateRecorded = BuildSeedDate(weekStart, trend.Disease, weekIndex, caseIndex),
                        Disease = trend.Disease,
                        Symptoms = symptoms[(weekIndex + caseIndex) % symptoms.Length],
                        Status = BuildSeedStatus(weekIndex, trend.WeeklyCaseCounts.Length, caseIndex, caseCount)
                    });
                }

                patientOffsetsByBarangay[trend.Barangay] = (rotationBase + caseCount) % patients.Count;
            }
        }

        return records;
    }

    private static string BuildSeedStatus(int weekIndex, int totalWeeks, int caseIndex, int caseCount)
    {
        if (weekIndex <= 2)
        {
            return caseCount >= 12 && caseIndex >= caseCount - 2
                ? "Critical"
                : "Active";
        }

        if (weekIndex <= totalWeeks - 4)
        {
            if (caseCount >= 14 && caseIndex == caseCount - 1)
            {
                return "Critical";
            }

            return caseIndex % 2 == 0 ? "Active" : "Recovered";
        }

        if (caseCount >= 15 && caseIndex == caseCount - 1)
        {
            return "Critical";
        }

        return caseIndex % 4 == 0 ? "Active" : "Recovered";
    }

    private static DateTime BuildSeedDate(DateTime weekStart, string disease, int weekIndex, int caseIndex)
    {
        var diseaseHourOffset = disease switch
        {
            "Dengue" => 0,
            "Flu" => 1,
            "COVID-19" => 2,
            "Leptospirosis" => 3,
            _ => 0
        };
        var dayOffset = caseIndex % 7;
        var hour = 8 + ((weekIndex + caseIndex + diseaseHourOffset) % 9);
        var minute = caseIndex % 2 == 0 ? 15 : 40;
        return weekStart.AddDays(dayOffset).AddHours(hour).AddMinutes(minute);
    }

    private static DateTime BuildSeedWeekStart(int weekIndex) => HealthRecordSeedStartWeek.AddDays(weekIndex * 7);

    private static string BuildHealthRecordSeedKey(HealthRecord record)
    {
        return BuildHealthRecordSeedKey(
            record.PatientID,
            record.BHWID,
            record.DateRecorded,
            record.Disease,
            record.Symptoms,
            record.Status);
    }

    private static string BuildHealthRecordSeedKey(
        int? patientId,
        int? bhwId,
        DateTime dateRecorded,
        string? disease,
        string? symptoms,
        string? status)
    {
        return $"{patientId.GetValueOrDefault()}|{bhwId.GetValueOrDefault()}|{dateRecorded:yyyy-MM-dd HH:mm:ss}|{disease?.Trim()}|{symptoms?.Trim()}|{status?.Trim()}";
    }

    private static string ExtractBarangay(string address)
    {
        if (string.IsNullOrWhiteSpace(address))
        {
            return string.Empty;
        }

        var separatorIndex = address.IndexOf(',');
        return separatorIndex >= 0 ? address[..separatorIndex].Trim() : address.Trim();
    }

    private static string BuildSeedContactNumber(int householdId, int memberIndex) => $"0917{householdId:000}{memberIndex:0000}";

    private sealed record HealthRecordSeedTrendDefinition(
        string Barangay,
        string Disease,
        string BhwArea,
        int[] WeeklyCaseCounts);

    private sealed record SeedHouseholdDefinition(string Address, float RiskScore, string EmergencyContactName, string[] PatientNames);
}
