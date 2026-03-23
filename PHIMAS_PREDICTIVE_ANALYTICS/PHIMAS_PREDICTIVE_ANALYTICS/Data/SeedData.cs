using Microsoft.EntityFrameworkCore;
using PHIMAS_PREDICTIVE_ANALYTICS.Helpers;
using PHIMAS_PREDICTIVE_ANALYTICS.Models;

namespace PHIMAS_PREDICTIVE_ANALYTICS.Data;

public static class SeedData
{
    private static readonly SeedHouseholdDefinition[] HouseholdSeedDefinitions =
    [
        new("Sevilla, Zone 4, San Fernando City", 72, "Juan Dela Cruz", ["Maria Dela Cruz", "Jose Dela Cruz", "Anna Dela Cruz", "Carl Dela Cruz"]),
        new("Catbangen, Purok 2, San Fernando City", 58, "Ana Villanueva", ["Miguel Villanueva", "Clara Villanueva", "Sofia Villanueva"]),
        new("San Vicente, Zone 1, San Fernando City", 84, "Pedro Santos", ["Lorna Santos", "Paolo Santos", "Rina Santos", "Mark Santos", "Ella Santos"]),
        new("Sevilla, Zone 2, San Fernando City", 35, "Maricel Ramos", ["Noel Ramos", "Bea Ramos"]),
        new("Catbangen, Purok 5, San Fernando City", 66, "Carlos Reyes", ["Luz Reyes", "Paula Reyes", "Ramon Reyes", "Joy Reyes", "Nina Reyes", "Leo Reyes"])
    ];

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
        if (!await context.Users.AnyAsync())
        {
            await context.Users.AddRangeAsync(
                new User
                {
                    FullName = "System Administrator",
                    Role = "ADMIN",
                    Email = "admin@phimas.com",
                    Password = PasswordHelper.HashPassword("Admin123!"),
                    ContactNumber = "09171234567",
                    IsAvailable = false
                },
                new User
                {
                    FullName = "Dr. Elena Rivera",
                    Role = "CHO",
                    Email = "cho@phimas.com",
                    Password = PasswordHelper.HashPassword("Cho123!"),
                    ContactNumber = "09171234568",
                    IsAvailable = false
                },
                new User
                {
                    FullName = "Maria Clara",
                    Role = "BHW",
                    Email = "bhw1@phimas.com",
                    Password = PasswordHelper.HashPassword("Bhw123!"),
                    ContactNumber = "09170000001",
                    IsAvailable = true,
                    AssignedArea = "Sevilla"
                },
                new User
                {
                    FullName = "Roberto Gomez",
                    Role = "BHW",
                    Email = "bhw2@phimas.com",
                    Password = PasswordHelper.HashPassword("Bhw123!"),
                    ContactNumber = "09170000002",
                    IsAvailable = true,
                    AssignedArea = "Catbangen"
                },
                new User
                {
                    FullName = "Liza Santos",
                    Role = "BHW",
                    Email = "bhw3@phimas.com",
                    Password = PasswordHelper.HashPassword("Bhw123!"),
                    ContactNumber = "09170000003",
                    IsAvailable = false,
                    AssignedArea = "San Vicente"
                });
            await context.SaveChangesAsync();
        }

        if (!await context.Households.AnyAsync())
        {
            await context.Households.AddRangeAsync(
                HouseholdSeedDefinitions.Select(definition => new Household
                {
                    Address = definition.Address,
                    RiskScore = definition.RiskScore
                }));
            await context.SaveChangesAsync();
        }

        if (!await context.HouseholdMembers.AnyAsync())
        {
            var households = await context.Households.OrderBy(household => household.HouseholdID).ToListAsync();
            var members = new List<HouseholdMember>();

            for (var index = 0; index < households.Count && index < HouseholdSeedDefinitions.Length; index++)
            {
                var household = households[index];
                var definition = HouseholdSeedDefinitions[index];

                members.Add(new HouseholdMember
                {
                    HouseholdID = household.HouseholdID,
                    FullName = definition.EmergencyContactName,
                    ContactNumber = BuildSeedContactNumber(household.HouseholdID, 0),
                    IsEmergencyContact = true
                });

                for (var memberIndex = 0; memberIndex < definition.PatientNames.Length; memberIndex++)
                {
                    members.Add(new HouseholdMember
                    {
                        HouseholdID = household.HouseholdID,
                        FullName = definition.PatientNames[memberIndex],
                        ContactNumber = BuildSeedContactNumber(household.HouseholdID, memberIndex + 1),
                        IsEmergencyContact = false
                    });
                }
            }

            await context.HouseholdMembers.AddRangeAsync(members);
            await context.SaveChangesAsync();
        }

        if (!await context.Inventory.AnyAsync())
        {
            await context.Inventory.AddRangeAsync(
                new Inventory { ItemName = "Dengue NS1 Kits", Unit = "boxes", MinimumThreshold = 15, CurrentStock = 9 },
                new Inventory { ItemName = "Paracetamol", Unit = "bottles", MinimumThreshold = 20, CurrentStock = 36 },
                new Inventory { ItemName = "ORS Packs", Unit = "packs", MinimumThreshold = 25, CurrentStock = 18 },
                new Inventory { ItemName = "Vitamin A", Unit = "boxes", MinimumThreshold = 10, CurrentStock = 14 });
            await context.SaveChangesAsync();
        }

        if (!await context.HealthRecords.AnyAsync())
        {
            var bhwIds = await context.Users.Where(user => user.Role == "BHW").OrderBy(user => user.UserID).Select(user => user.UserID).ToListAsync();
            var members = await context.HouseholdMembers.OrderBy(member => member.MemberID).ToListAsync();
            var templates = new[]
            {
                new { Disease = "Dengue", Symptoms = "High fever, body pain, petechiae", Status = "Active", Offset = -10 },
                new { Disease = "Influenza", Symptoms = "Cough, fever, sore throat", Status = "Monitoring", Offset = -7 },
                new { Disease = "Leptospirosis", Symptoms = "Fever, headache, muscle pain", Status = "Active", Offset = -3 },
                new { Disease = "Dengue", Symptoms = "Persistent fever, abdominal pain", Status = "Escalated", Offset = -1 },
                new { Disease = "Acute Gastroenteritis", Symptoms = "Vomiting, diarrhea", Status = "Monitoring", Offset = 0 }
            };

            await context.HealthRecords.AddRangeAsync(
                templates.Select((template, index) => new HealthRecord
                {
                    PatientID = members[index % members.Count].PatientID,
                    BHWID = bhwIds[index % bhwIds.Count],
                    DateRecorded = DateTime.UtcNow.AddDays(template.Offset),
                    Disease = template.Disease,
                    Symptoms = template.Symptoms,
                    Status = template.Status
                }));
            await context.SaveChangesAsync();
        }

        if (!await context.TaskAssignments.AnyAsync())
        {
            var bhws = await context.Users.Where(user => user.Role == "BHW").OrderBy(user => user.UserID).ToListAsync();
            var households = await context.Households.OrderByDescending(household => household.RiskScore).ToListAsync();

            await context.TaskAssignments.AddRangeAsync(
                new TaskAssignment { BHWID = bhws[0].UserID, HouseholdID = households[0].HouseholdID, TaskDate = DateTime.Today.AddHours(9), Priority = "High", Status = "Pending", Description = "Validate symptoms and escalate possible severe dengue cases." },
                new TaskAssignment { BHWID = bhws[1].UserID, HouseholdID = households[1].HouseholdID, TaskDate = DateTime.Today.AddHours(13), Priority = "Medium", Status = "Started", Description = "Review influenza symptoms and reinforce isolation guidance." },
                new TaskAssignment { BHWID = bhws[2].UserID, HouseholdID = households[2].HouseholdID, TaskDate = DateTime.Today.AddHours(15), Priority = "High", Status = "Ongoing", Description = "Coordinate with CHO on the household's leptospirosis risk." });
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
                new PredictiveAnalysis { DateGenerated = DateTime.UtcNow.AddDays(-1), Disease = "Dengue", PredictedCases = 12, HighRiskBarangay = "Sevilla", ConfidenceScore = 0.88f },
                new PredictiveAnalysis { DateGenerated = DateTime.UtcNow, Disease = "Influenza", PredictedCases = 8, HighRiskBarangay = "Catbangen", ConfidenceScore = 0.74f });
            await context.SaveChangesAsync();
        }
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
            await TrySqlAsync(context, "INSERT IGNORE INTO task_assignments (TaskAssignmentID, BHWID, HouseholdID, TaskDate, Status, Priority, Description) SELECT TaskAssignmentID, BHWID, HouseholdID, TaskDate, COALESCE(Status, 'Pending'), COALESCE(Priority, 'Medium'), LEFT(COALESCE(Description, 'Migrated task'), 200) FROM taskassignments;");
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

    private static string BuildSeedContactNumber(int householdId, int memberIndex) => $"0917{householdId:000}{memberIndex:0000}";

    private sealed record SeedHouseholdDefinition(string Address, float RiskScore, string EmergencyContactName, string[] PatientNames);
}
