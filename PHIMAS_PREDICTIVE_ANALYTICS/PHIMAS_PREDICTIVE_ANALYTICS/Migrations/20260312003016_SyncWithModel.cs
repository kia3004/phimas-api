using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace PHIMAS_PREDICTIVE_ANALYTICS.Migrations
{
    /// <inheritdoc />
    public partial class SyncWithModel : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropPrimaryKey(
                name: "PK_Users",
                table: "Users");

            migrationBuilder.DropPrimaryKey(
                name: "PK_TaskAssignments",
                table: "TaskAssignments");

            migrationBuilder.DropPrimaryKey(
                name: "PK_Reports",
                table: "Reports");

            migrationBuilder.DropPrimaryKey(
                name: "PK_PredictiveAnalysis",
                table: "PredictiveAnalysis");

            migrationBuilder.DropPrimaryKey(
                name: "PK_Inventory",
                table: "Inventory");

            migrationBuilder.DropPrimaryKey(
                name: "PK_HealthRecords",
                table: "HealthRecords");

            migrationBuilder.DropPrimaryKey(
                name: "PK_Households",
                table: "Households");

            migrationBuilder.RenameTable(
                name: "Users",
                newName: "users");

            migrationBuilder.RenameTable(
                name: "TaskAssignments",
                newName: "taskassignments");

            migrationBuilder.RenameTable(
                name: "Reports",
                newName: "reports");

            migrationBuilder.RenameTable(
                name: "PredictiveAnalysis",
                newName: "predictiveanalysis");

            migrationBuilder.RenameTable(
                name: "Inventory",
                newName: "inventory");

            migrationBuilder.RenameTable(
                name: "HealthRecords",
                newName: "healthrecords");

            migrationBuilder.RenameTable(
                name: "Households",
                newName: "household");

            migrationBuilder.AddPrimaryKey(
                name: "PK_users",
                table: "users",
                column: "UserID");

            migrationBuilder.AddPrimaryKey(
                name: "PK_taskassignments",
                table: "taskassignments",
                column: "TaskID");

            migrationBuilder.AddPrimaryKey(
                name: "PK_reports",
                table: "reports",
                column: "ReportID");

            migrationBuilder.AddPrimaryKey(
                name: "PK_predictiveanalysis",
                table: "predictiveanalysis",
                column: "AnalyticsID");

            migrationBuilder.AddPrimaryKey(
                name: "PK_inventory",
                table: "inventory",
                column: "ItemID");

            migrationBuilder.AddPrimaryKey(
                name: "PK_healthrecords",
                table: "healthrecords",
                column: "RecordID");

            migrationBuilder.AddPrimaryKey(
                name: "PK_household",
                table: "household",
                column: "HouseholdID");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropPrimaryKey(
                name: "PK_users",
                table: "users");

            migrationBuilder.DropPrimaryKey(
                name: "PK_taskassignments",
                table: "taskassignments");

            migrationBuilder.DropPrimaryKey(
                name: "PK_reports",
                table: "reports");

            migrationBuilder.DropPrimaryKey(
                name: "PK_predictiveanalysis",
                table: "predictiveanalysis");

            migrationBuilder.DropPrimaryKey(
                name: "PK_inventory",
                table: "inventory");

            migrationBuilder.DropPrimaryKey(
                name: "PK_healthrecords",
                table: "healthrecords");

            migrationBuilder.DropPrimaryKey(
                name: "PK_household",
                table: "household");

            migrationBuilder.RenameTable(
                name: "users",
                newName: "Users");

            migrationBuilder.RenameTable(
                name: "taskassignments",
                newName: "TaskAssignments");

            migrationBuilder.RenameTable(
                name: "reports",
                newName: "Reports");

            migrationBuilder.RenameTable(
                name: "predictiveanalysis",
                newName: "PredictiveAnalysis");

            migrationBuilder.RenameTable(
                name: "inventory",
                newName: "Inventory");

            migrationBuilder.RenameTable(
                name: "healthrecords",
                newName: "HealthRecords");

            migrationBuilder.RenameTable(
                name: "household",
                newName: "Households");

            migrationBuilder.AddPrimaryKey(
                name: "PK_Users",
                table: "Users",
                column: "UserID");

            migrationBuilder.AddPrimaryKey(
                name: "PK_TaskAssignments",
                table: "TaskAssignments",
                column: "TaskID");

            migrationBuilder.AddPrimaryKey(
                name: "PK_Reports",
                table: "Reports",
                column: "ReportID");

            migrationBuilder.AddPrimaryKey(
                name: "PK_PredictiveAnalysis",
                table: "PredictiveAnalysis",
                column: "AnalyticsID");

            migrationBuilder.AddPrimaryKey(
                name: "PK_Inventory",
                table: "Inventory",
                column: "ItemID");

            migrationBuilder.AddPrimaryKey(
                name: "PK_HealthRecords",
                table: "HealthRecords",
                column: "RecordID");

            migrationBuilder.AddPrimaryKey(
                name: "PK_Households",
                table: "Households",
                column: "HouseholdID");
        }
    }
}
