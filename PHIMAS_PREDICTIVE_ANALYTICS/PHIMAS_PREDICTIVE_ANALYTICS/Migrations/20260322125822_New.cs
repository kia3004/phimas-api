using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace PHIMAS_PREDICTIVE_ANALYTICS.Migrations
{
    /// <inheritdoc />
    public partial class New : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_healthrecords_householdmembers_MemberID",
                table: "healthrecords");

            migrationBuilder.DropForeignKey(
                name: "FK_healthrecords_households_HouseholdID",
                table: "healthrecords");

            migrationBuilder.DropForeignKey(
                name: "FK_healthrecords_users_BHWID",
                table: "healthrecords");

            migrationBuilder.DropForeignKey(
                name: "FK_householdmembers_households_HouseholdID",
                table: "householdmembers");

            migrationBuilder.DropForeignKey(
                name: "FK_reports_householdmembers_MemberID",
                table: "reports");

            migrationBuilder.DropForeignKey(
                name: "FK_reports_households_HouseholdID",
                table: "reports");

            migrationBuilder.DropForeignKey(
                name: "FK_reports_users_GeneratedBy",
                table: "reports");

            migrationBuilder.DropForeignKey(
                name: "FK_taskassignments_households_HouseholdID",
                table: "taskassignments");

            migrationBuilder.DropForeignKey(
                name: "FK_taskassignments_users_BHWID",
                table: "taskassignments");

            migrationBuilder.DropIndex(
                name: "IX_reports_HouseholdID",
                table: "reports");

            migrationBuilder.DropIndex(
                name: "IX_reports_MemberID",
                table: "reports");

            migrationBuilder.DropPrimaryKey(
                name: "PK_taskassignments",
                table: "taskassignments");

            migrationBuilder.DropPrimaryKey(
                name: "PK_predictiveanalyses",
                table: "predictiveanalyses");

            migrationBuilder.DropPrimaryKey(
                name: "PK_householdmembers",
                table: "householdmembers");

            migrationBuilder.DropIndex(
                name: "IX_householdmembers_HouseholdID",
                table: "householdmembers");

            migrationBuilder.DropPrimaryKey(
                name: "PK_healthrecords",
                table: "healthrecords");

            migrationBuilder.DropIndex(
                name: "IX_healthrecords_MemberID",
                table: "healthrecords");

            migrationBuilder.DropColumn(
                name: "HouseholdID",
                table: "reports");

            migrationBuilder.DropColumn(
                name: "MemberID",
                table: "reports");

            migrationBuilder.DropColumn(
                name: "HouseholdMember",
                table: "households");

            migrationBuilder.DropColumn(
                name: "NumberOfMembers",
                table: "households");

            migrationBuilder.DropColumn(
                name: "Title",
                table: "taskassignments");

            migrationBuilder.DropColumn(
                name: "MemberID",
                table: "healthrecords");

            migrationBuilder.RenameTable(
                name: "taskassignments",
                newName: "task_assignments");

            migrationBuilder.RenameTable(
                name: "predictiveanalyses",
                newName: "predictive_analysis");

            migrationBuilder.RenameTable(
                name: "householdmembers",
                newName: "household_members");

            migrationBuilder.RenameTable(
                name: "healthrecords",
                newName: "health_records");

            migrationBuilder.RenameColumn(
                name: "TaskID",
                table: "task_assignments",
                newName: "TaskAssignmentID");

            migrationBuilder.RenameIndex(
                name: "IX_taskassignments_HouseholdID",
                table: "task_assignments",
                newName: "IX_task_assignments_HouseholdID");

            migrationBuilder.RenameIndex(
                name: "IX_taskassignments_BHWID",
                table: "task_assignments",
                newName: "IX_task_assignments_BHWID");

            migrationBuilder.RenameColumn(
                name: "MemberID",
                table: "household_members",
                newName: "PatientID");

            migrationBuilder.RenameColumn(
                name: "HouseholdID",
                table: "health_records",
                newName: "PatientID");

            migrationBuilder.RenameIndex(
                name: "IX_healthrecords_HouseholdID",
                table: "health_records",
                newName: "IX_health_records_PatientID");

            migrationBuilder.RenameIndex(
                name: "IX_healthrecords_BHWID",
                table: "health_records",
                newName: "IX_health_records_BHWID");

            migrationBuilder.AlterColumn<string>(
                name: "ReportType",
                table: "reports",
                type: "varchar(50)",
                maxLength: 50,
                nullable: false,
                oldClrType: typeof(string),
                oldType: "varchar(150)",
                oldMaxLength: 150)
                .Annotation("MySql:CharSet", "utf8mb4")
                .OldAnnotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.AlterColumn<int>(
                name: "GeneratedBy",
                table: "reports",
                type: "int",
                nullable: false,
                defaultValue: 0,
                oldClrType: typeof(int),
                oldType: "int",
                oldNullable: true);

            migrationBuilder.AddColumn<int>(
                name: "PatientID",
                table: "reports",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AlterColumn<string>(
                name: "Address",
                table: "households",
                type: "varchar(200)",
                maxLength: 200,
                nullable: false,
                oldClrType: typeof(string),
                oldType: "varchar(255)",
                oldMaxLength: 255)
                .Annotation("MySql:CharSet", "utf8mb4")
                .OldAnnotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.UpdateData(
                table: "task_assignments",
                keyColumn: "Description",
                keyValue: null,
                column: "Description",
                value: "");

            migrationBuilder.AlterColumn<string>(
                name: "Description",
                table: "task_assignments",
                type: "varchar(200)",
                maxLength: 200,
                nullable: false,
                oldClrType: typeof(string),
                oldType: "varchar(1000)",
                oldMaxLength: 1000,
                oldNullable: true)
                .Annotation("MySql:CharSet", "utf8mb4")
                .OldAnnotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.AlterColumn<string>(
                name: "FullName",
                table: "household_members",
                type: "varchar(100)",
                maxLength: 100,
                nullable: false,
                oldClrType: typeof(string),
                oldType: "varchar(150)",
                oldMaxLength: 150)
                .Annotation("MySql:CharSet", "utf8mb4")
                .OldAnnotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.AlterColumn<string>(
                name: "ContactNumber",
                table: "household_members",
                type: "varchar(15)",
                maxLength: 15,
                nullable: false,
                oldClrType: typeof(string),
                oldType: "varchar(50)",
                oldMaxLength: 50)
                .Annotation("MySql:CharSet", "utf8mb4")
                .OldAnnotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.AlterColumn<string>(
                name: "Symptoms",
                table: "health_records",
                type: "varchar(255)",
                maxLength: 255,
                nullable: false,
                oldClrType: typeof(string),
                oldType: "varchar(1000)",
                oldMaxLength: 1000)
                .Annotation("MySql:CharSet", "utf8mb4")
                .OldAnnotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.AlterColumn<string>(
                name: "Disease",
                table: "health_records",
                type: "varchar(100)",
                maxLength: 100,
                nullable: false,
                oldClrType: typeof(string),
                oldType: "varchar(150)",
                oldMaxLength: 150)
                .Annotation("MySql:CharSet", "utf8mb4")
                .OldAnnotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.AlterColumn<int>(
                name: "BHWID",
                table: "health_records",
                type: "int",
                nullable: false,
                defaultValue: 0,
                oldClrType: typeof(int),
                oldType: "int",
                oldNullable: true);

            migrationBuilder.AddPrimaryKey(
                name: "PK_task_assignments",
                table: "task_assignments",
                column: "TaskAssignmentID");

            migrationBuilder.AddPrimaryKey(
                name: "PK_predictive_analysis",
                table: "predictive_analysis",
                column: "AnalyticsID");

            migrationBuilder.AddPrimaryKey(
                name: "PK_household_members",
                table: "household_members",
                column: "PatientID");

            migrationBuilder.AddPrimaryKey(
                name: "PK_health_records",
                table: "health_records",
                column: "RecordID");

            migrationBuilder.CreateIndex(
                name: "IX_reports_PatientID",
                table: "reports",
                column: "PatientID");

            migrationBuilder.CreateIndex(
                name: "IX_households_Address",
                table: "households",
                column: "Address");

            migrationBuilder.CreateIndex(
                name: "IX_household_members_HouseholdID_FullName_ContactNumber",
                table: "household_members",
                columns: new[] { "HouseholdID", "FullName", "ContactNumber" },
                unique: true);

            migrationBuilder.AddForeignKey(
                name: "FK_health_records_household_members_PatientID",
                table: "health_records",
                column: "PatientID",
                principalTable: "household_members",
                principalColumn: "PatientID",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_health_records_users_BHWID",
                table: "health_records",
                column: "BHWID",
                principalTable: "users",
                principalColumn: "UserID",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_household_members_households_HouseholdID",
                table: "household_members",
                column: "HouseholdID",
                principalTable: "households",
                principalColumn: "HouseholdID",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_reports_household_members_PatientID",
                table: "reports",
                column: "PatientID",
                principalTable: "household_members",
                principalColumn: "PatientID",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_reports_users_GeneratedBy",
                table: "reports",
                column: "GeneratedBy",
                principalTable: "users",
                principalColumn: "UserID",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_task_assignments_households_HouseholdID",
                table: "task_assignments",
                column: "HouseholdID",
                principalTable: "households",
                principalColumn: "HouseholdID",
                onDelete: ReferentialAction.SetNull);

            migrationBuilder.AddForeignKey(
                name: "FK_task_assignments_users_BHWID",
                table: "task_assignments",
                column: "BHWID",
                principalTable: "users",
                principalColumn: "UserID",
                onDelete: ReferentialAction.SetNull);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_health_records_household_members_PatientID",
                table: "health_records");

            migrationBuilder.DropForeignKey(
                name: "FK_health_records_users_BHWID",
                table: "health_records");

            migrationBuilder.DropForeignKey(
                name: "FK_household_members_households_HouseholdID",
                table: "household_members");

            migrationBuilder.DropForeignKey(
                name: "FK_reports_household_members_PatientID",
                table: "reports");

            migrationBuilder.DropForeignKey(
                name: "FK_reports_users_GeneratedBy",
                table: "reports");

            migrationBuilder.DropForeignKey(
                name: "FK_task_assignments_households_HouseholdID",
                table: "task_assignments");

            migrationBuilder.DropForeignKey(
                name: "FK_task_assignments_users_BHWID",
                table: "task_assignments");

            migrationBuilder.DropIndex(
                name: "IX_reports_PatientID",
                table: "reports");

            migrationBuilder.DropIndex(
                name: "IX_households_Address",
                table: "households");

            migrationBuilder.DropPrimaryKey(
                name: "PK_task_assignments",
                table: "task_assignments");

            migrationBuilder.DropPrimaryKey(
                name: "PK_predictive_analysis",
                table: "predictive_analysis");

            migrationBuilder.DropPrimaryKey(
                name: "PK_household_members",
                table: "household_members");

            migrationBuilder.DropIndex(
                name: "IX_household_members_HouseholdID_FullName_ContactNumber",
                table: "household_members");

            migrationBuilder.DropPrimaryKey(
                name: "PK_health_records",
                table: "health_records");

            migrationBuilder.DropColumn(
                name: "PatientID",
                table: "reports");

            migrationBuilder.RenameTable(
                name: "task_assignments",
                newName: "taskassignments");

            migrationBuilder.RenameTable(
                name: "predictive_analysis",
                newName: "predictiveanalyses");

            migrationBuilder.RenameTable(
                name: "household_members",
                newName: "householdmembers");

            migrationBuilder.RenameTable(
                name: "health_records",
                newName: "healthrecords");

            migrationBuilder.RenameColumn(
                name: "TaskAssignmentID",
                table: "taskassignments",
                newName: "TaskID");

            migrationBuilder.RenameIndex(
                name: "IX_task_assignments_HouseholdID",
                table: "taskassignments",
                newName: "IX_taskassignments_HouseholdID");

            migrationBuilder.RenameIndex(
                name: "IX_task_assignments_BHWID",
                table: "taskassignments",
                newName: "IX_taskassignments_BHWID");

            migrationBuilder.RenameColumn(
                name: "PatientID",
                table: "householdmembers",
                newName: "MemberID");

            migrationBuilder.RenameColumn(
                name: "PatientID",
                table: "healthrecords",
                newName: "HouseholdID");

            migrationBuilder.RenameIndex(
                name: "IX_health_records_PatientID",
                table: "healthrecords",
                newName: "IX_healthrecords_HouseholdID");

            migrationBuilder.RenameIndex(
                name: "IX_health_records_BHWID",
                table: "healthrecords",
                newName: "IX_healthrecords_BHWID");

            migrationBuilder.AlterColumn<string>(
                name: "ReportType",
                table: "reports",
                type: "varchar(150)",
                maxLength: 150,
                nullable: false,
                oldClrType: typeof(string),
                oldType: "varchar(50)",
                oldMaxLength: 50)
                .Annotation("MySql:CharSet", "utf8mb4")
                .OldAnnotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.AlterColumn<int>(
                name: "GeneratedBy",
                table: "reports",
                type: "int",
                nullable: true,
                oldClrType: typeof(int),
                oldType: "int");

            migrationBuilder.AddColumn<int>(
                name: "HouseholdID",
                table: "reports",
                type: "int",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "MemberID",
                table: "reports",
                type: "int",
                nullable: true);

            migrationBuilder.AlterColumn<string>(
                name: "Address",
                table: "households",
                type: "varchar(255)",
                maxLength: 255,
                nullable: false,
                oldClrType: typeof(string),
                oldType: "varchar(200)",
                oldMaxLength: 200)
                .Annotation("MySql:CharSet", "utf8mb4")
                .OldAnnotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.AddColumn<string>(
                name: "HouseholdMember",
                table: "households",
                type: "varchar(150)",
                maxLength: 150,
                nullable: false,
                defaultValue: "")
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.AddColumn<int>(
                name: "NumberOfMembers",
                table: "households",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AlterColumn<string>(
                name: "Description",
                table: "taskassignments",
                type: "varchar(1000)",
                maxLength: 1000,
                nullable: true,
                oldClrType: typeof(string),
                oldType: "varchar(200)",
                oldMaxLength: 200)
                .Annotation("MySql:CharSet", "utf8mb4")
                .OldAnnotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.AddColumn<string>(
                name: "Title",
                table: "taskassignments",
                type: "varchar(150)",
                maxLength: 150,
                nullable: true)
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.AlterColumn<string>(
                name: "FullName",
                table: "householdmembers",
                type: "varchar(150)",
                maxLength: 150,
                nullable: false,
                oldClrType: typeof(string),
                oldType: "varchar(100)",
                oldMaxLength: 100)
                .Annotation("MySql:CharSet", "utf8mb4")
                .OldAnnotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.AlterColumn<string>(
                name: "ContactNumber",
                table: "householdmembers",
                type: "varchar(50)",
                maxLength: 50,
                nullable: false,
                oldClrType: typeof(string),
                oldType: "varchar(15)",
                oldMaxLength: 15)
                .Annotation("MySql:CharSet", "utf8mb4")
                .OldAnnotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.AlterColumn<string>(
                name: "Symptoms",
                table: "healthrecords",
                type: "varchar(1000)",
                maxLength: 1000,
                nullable: false,
                oldClrType: typeof(string),
                oldType: "varchar(255)",
                oldMaxLength: 255)
                .Annotation("MySql:CharSet", "utf8mb4")
                .OldAnnotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.AlterColumn<string>(
                name: "Disease",
                table: "healthrecords",
                type: "varchar(150)",
                maxLength: 150,
                nullable: false,
                oldClrType: typeof(string),
                oldType: "varchar(100)",
                oldMaxLength: 100)
                .Annotation("MySql:CharSet", "utf8mb4")
                .OldAnnotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.AlterColumn<int>(
                name: "BHWID",
                table: "healthrecords",
                type: "int",
                nullable: true,
                oldClrType: typeof(int),
                oldType: "int");

            migrationBuilder.AddColumn<int>(
                name: "MemberID",
                table: "healthrecords",
                type: "int",
                nullable: true);

            migrationBuilder.AddPrimaryKey(
                name: "PK_taskassignments",
                table: "taskassignments",
                column: "TaskID");

            migrationBuilder.AddPrimaryKey(
                name: "PK_predictiveanalyses",
                table: "predictiveanalyses",
                column: "AnalyticsID");

            migrationBuilder.AddPrimaryKey(
                name: "PK_householdmembers",
                table: "householdmembers",
                column: "MemberID");

            migrationBuilder.AddPrimaryKey(
                name: "PK_healthrecords",
                table: "healthrecords",
                column: "RecordID");

            migrationBuilder.CreateIndex(
                name: "IX_reports_HouseholdID",
                table: "reports",
                column: "HouseholdID");

            migrationBuilder.CreateIndex(
                name: "IX_reports_MemberID",
                table: "reports",
                column: "MemberID");

            migrationBuilder.CreateIndex(
                name: "IX_householdmembers_HouseholdID",
                table: "householdmembers",
                column: "HouseholdID");

            migrationBuilder.CreateIndex(
                name: "IX_healthrecords_MemberID",
                table: "healthrecords",
                column: "MemberID");

            migrationBuilder.AddForeignKey(
                name: "FK_healthrecords_householdmembers_MemberID",
                table: "healthrecords",
                column: "MemberID",
                principalTable: "householdmembers",
                principalColumn: "MemberID",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_healthrecords_households_HouseholdID",
                table: "healthrecords",
                column: "HouseholdID",
                principalTable: "households",
                principalColumn: "HouseholdID",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_healthrecords_users_BHWID",
                table: "healthrecords",
                column: "BHWID",
                principalTable: "users",
                principalColumn: "UserID",
                onDelete: ReferentialAction.SetNull);

            migrationBuilder.AddForeignKey(
                name: "FK_householdmembers_households_HouseholdID",
                table: "householdmembers",
                column: "HouseholdID",
                principalTable: "households",
                principalColumn: "HouseholdID",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_reports_householdmembers_MemberID",
                table: "reports",
                column: "MemberID",
                principalTable: "householdmembers",
                principalColumn: "MemberID",
                onDelete: ReferentialAction.SetNull);

            migrationBuilder.AddForeignKey(
                name: "FK_reports_households_HouseholdID",
                table: "reports",
                column: "HouseholdID",
                principalTable: "households",
                principalColumn: "HouseholdID",
                onDelete: ReferentialAction.SetNull);

            migrationBuilder.AddForeignKey(
                name: "FK_reports_users_GeneratedBy",
                table: "reports",
                column: "GeneratedBy",
                principalTable: "users",
                principalColumn: "UserID",
                onDelete: ReferentialAction.SetNull);

            migrationBuilder.AddForeignKey(
                name: "FK_taskassignments_households_HouseholdID",
                table: "taskassignments",
                column: "HouseholdID",
                principalTable: "households",
                principalColumn: "HouseholdID",
                onDelete: ReferentialAction.SetNull);

            migrationBuilder.AddForeignKey(
                name: "FK_taskassignments_users_BHWID",
                table: "taskassignments",
                column: "BHWID",
                principalTable: "users",
                principalColumn: "UserID",
                onDelete: ReferentialAction.SetNull);
        }
    }
}
