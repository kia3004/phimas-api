using Microsoft.EntityFrameworkCore.Metadata;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace PHIMAS_PREDICTIVE_ANALYTICS.Migrations
{
    /// <inheritdoc />
    public partial class Updated : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropPrimaryKey(
                name: "PK_predictiveanalysis",
                table: "predictiveanalysis");

            migrationBuilder.DropPrimaryKey(
                name: "PK_household",
                table: "household");

            migrationBuilder.RenameTable(
                name: "predictiveanalysis",
                newName: "predictiveanalyses");

            migrationBuilder.RenameTable(
                name: "household",
                newName: "households");

            migrationBuilder.AlterColumn<string>(
                name: "Role",
                table: "users",
                type: "varchar(30)",
                maxLength: 30,
                nullable: false,
                oldClrType: typeof(string),
                oldType: "longtext")
                .Annotation("MySql:CharSet", "utf8mb4")
                .OldAnnotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.AlterColumn<string>(
                name: "ProfilePicture",
                table: "users",
                type: "varchar(255)",
                maxLength: 255,
                nullable: true,
                oldClrType: typeof(string),
                oldType: "longtext",
                oldNullable: true)
                .Annotation("MySql:CharSet", "utf8mb4")
                .OldAnnotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.AlterColumn<string>(
                name: "Password",
                table: "users",
                type: "varchar(255)",
                maxLength: 255,
                nullable: false,
                oldClrType: typeof(string),
                oldType: "longtext")
                .Annotation("MySql:CharSet", "utf8mb4")
                .OldAnnotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.AlterColumn<string>(
                name: "FullName",
                table: "users",
                type: "varchar(150)",
                maxLength: 150,
                nullable: false,
                oldClrType: typeof(string),
                oldType: "longtext")
                .Annotation("MySql:CharSet", "utf8mb4")
                .OldAnnotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.AlterColumn<string>(
                name: "Email",
                table: "users",
                type: "varchar(150)",
                maxLength: 150,
                nullable: false,
                oldClrType: typeof(string),
                oldType: "longtext")
                .Annotation("MySql:CharSet", "utf8mb4")
                .OldAnnotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.AlterColumn<string>(
                name: "ContactNumber",
                table: "users",
                type: "varchar(50)",
                maxLength: 50,
                nullable: false,
                oldClrType: typeof(string),
                oldType: "longtext")
                .Annotation("MySql:CharSet", "utf8mb4")
                .OldAnnotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.AddColumn<string>(
                name: "AssignedArea",
                table: "users",
                type: "varchar(120)",
                maxLength: 120,
                nullable: true)
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.AlterColumn<string>(
                name: "Status",
                table: "taskassignments",
                type: "varchar(50)",
                maxLength: 50,
                nullable: false,
                oldClrType: typeof(string),
                oldType: "longtext")
                .Annotation("MySql:CharSet", "utf8mb4")
                .OldAnnotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.AlterColumn<string>(
                name: "Priority",
                table: "taskassignments",
                type: "varchar(50)",
                maxLength: 50,
                nullable: false,
                oldClrType: typeof(string),
                oldType: "longtext")
                .Annotation("MySql:CharSet", "utf8mb4")
                .OldAnnotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.AddColumn<string>(
                name: "Description",
                table: "taskassignments",
                type: "varchar(1000)",
                maxLength: 1000,
                nullable: true)
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.AddColumn<string>(
                name: "Title",
                table: "taskassignments",
                type: "varchar(150)",
                maxLength: 150,
                nullable: true)
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.AlterColumn<string>(
                name: "ReportType",
                table: "reports",
                type: "varchar(150)",
                maxLength: 150,
                nullable: false,
                oldClrType: typeof(string),
                oldType: "longtext")
                .Annotation("MySql:CharSet", "utf8mb4")
                .OldAnnotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.AlterColumn<string>(
                name: "Content",
                table: "reports",
                type: "varchar(4000)",
                maxLength: 4000,
                nullable: false,
                oldClrType: typeof(string),
                oldType: "longtext")
                .Annotation("MySql:CharSet", "utf8mb4")
                .OldAnnotation("MySql:CharSet", "utf8mb4");

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
                name: "Unit",
                table: "inventory",
                type: "varchar(50)",
                maxLength: 50,
                nullable: false,
                oldClrType: typeof(string),
                oldType: "longtext")
                .Annotation("MySql:CharSet", "utf8mb4")
                .OldAnnotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.AlterColumn<string>(
                name: "ItemName",
                table: "inventory",
                type: "varchar(150)",
                maxLength: 150,
                nullable: false,
                oldClrType: typeof(string),
                oldType: "longtext")
                .Annotation("MySql:CharSet", "utf8mb4")
                .OldAnnotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.AddColumn<int>(
                name: "CurrentStock",
                table: "inventory",
                type: "int",
                nullable: true);

            migrationBuilder.AlterColumn<string>(
                name: "Symptoms",
                table: "healthrecords",
                type: "varchar(1000)",
                maxLength: 1000,
                nullable: false,
                oldClrType: typeof(string),
                oldType: "longtext")
                .Annotation("MySql:CharSet", "utf8mb4")
                .OldAnnotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.AlterColumn<string>(
                name: "Status",
                table: "healthrecords",
                type: "varchar(50)",
                maxLength: 50,
                nullable: false,
                oldClrType: typeof(string),
                oldType: "longtext")
                .Annotation("MySql:CharSet", "utf8mb4")
                .OldAnnotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.AlterColumn<string>(
                name: "Disease",
                table: "healthrecords",
                type: "varchar(150)",
                maxLength: 150,
                nullable: false,
                oldClrType: typeof(string),
                oldType: "longtext")
                .Annotation("MySql:CharSet", "utf8mb4")
                .OldAnnotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.AddColumn<int>(
                name: "MemberID",
                table: "healthrecords",
                type: "int",
                nullable: true);

            migrationBuilder.AlterColumn<string>(
                name: "HighRiskBarangay",
                table: "predictiveanalyses",
                type: "varchar(150)",
                maxLength: 150,
                nullable: false,
                oldClrType: typeof(string),
                oldType: "longtext")
                .Annotation("MySql:CharSet", "utf8mb4")
                .OldAnnotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.AlterColumn<string>(
                name: "Disease",
                table: "predictiveanalyses",
                type: "varchar(150)",
                maxLength: 150,
                nullable: false,
                oldClrType: typeof(string),
                oldType: "longtext")
                .Annotation("MySql:CharSet", "utf8mb4")
                .OldAnnotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.AlterColumn<string>(
                name: "HouseholdMember",
                table: "households",
                type: "varchar(150)",
                maxLength: 150,
                nullable: false,
                oldClrType: typeof(string),
                oldType: "longtext")
                .Annotation("MySql:CharSet", "utf8mb4")
                .OldAnnotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.AlterColumn<string>(
                name: "Address",
                table: "households",
                type: "varchar(255)",
                maxLength: 255,
                nullable: false,
                oldClrType: typeof(string),
                oldType: "longtext")
                .Annotation("MySql:CharSet", "utf8mb4")
                .OldAnnotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.AddPrimaryKey(
                name: "PK_predictiveanalyses",
                table: "predictiveanalyses",
                column: "AnalyticsID");

            migrationBuilder.AddPrimaryKey(
                name: "PK_households",
                table: "households",
                column: "HouseholdID");

            migrationBuilder.CreateTable(
                name: "householdmembers",
                columns: table => new
                {
                    MemberID = table.Column<int>(type: "int", nullable: false)
                        .Annotation("MySql:ValueGenerationStrategy", MySqlValueGenerationStrategy.IdentityColumn),
                    HouseholdID = table.Column<int>(type: "int", nullable: false),
                    FullName = table.Column<string>(type: "varchar(150)", maxLength: 150, nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    ContactNumber = table.Column<string>(type: "varchar(50)", maxLength: 50, nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    IsEmergencyContact = table.Column<bool>(type: "tinyint(1)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_householdmembers", x => x.MemberID);
                    table.ForeignKey(
                        name: "FK_householdmembers_households_HouseholdID",
                        column: x => x.HouseholdID,
                        principalTable: "households",
                        principalColumn: "HouseholdID",
                        onDelete: ReferentialAction.Cascade);
                })
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.CreateIndex(
                name: "IX_users_Email",
                table: "users",
                column: "Email",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_taskassignments_BHWID",
                table: "taskassignments",
                column: "BHWID");

            migrationBuilder.CreateIndex(
                name: "IX_taskassignments_HouseholdID",
                table: "taskassignments",
                column: "HouseholdID");

            migrationBuilder.CreateIndex(
                name: "IX_reports_GeneratedBy",
                table: "reports",
                column: "GeneratedBy");

            migrationBuilder.CreateIndex(
                name: "IX_reports_HouseholdID",
                table: "reports",
                column: "HouseholdID");

            migrationBuilder.CreateIndex(
                name: "IX_reports_MemberID",
                table: "reports",
                column: "MemberID");

            migrationBuilder.CreateIndex(
                name: "IX_healthrecords_BHWID",
                table: "healthrecords",
                column: "BHWID");

            migrationBuilder.CreateIndex(
                name: "IX_healthrecords_HouseholdID",
                table: "healthrecords",
                column: "HouseholdID");

            migrationBuilder.CreateIndex(
                name: "IX_healthrecords_MemberID",
                table: "healthrecords",
                column: "MemberID");

            migrationBuilder.CreateIndex(
                name: "IX_householdmembers_HouseholdID",
                table: "householdmembers",
                column: "HouseholdID");

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

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
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

            migrationBuilder.DropTable(
                name: "householdmembers");

            migrationBuilder.DropIndex(
                name: "IX_users_Email",
                table: "users");

            migrationBuilder.DropIndex(
                name: "IX_taskassignments_BHWID",
                table: "taskassignments");

            migrationBuilder.DropIndex(
                name: "IX_taskassignments_HouseholdID",
                table: "taskassignments");

            migrationBuilder.DropIndex(
                name: "IX_reports_GeneratedBy",
                table: "reports");

            migrationBuilder.DropIndex(
                name: "IX_reports_HouseholdID",
                table: "reports");

            migrationBuilder.DropIndex(
                name: "IX_reports_MemberID",
                table: "reports");

            migrationBuilder.DropIndex(
                name: "IX_healthrecords_BHWID",
                table: "healthrecords");

            migrationBuilder.DropIndex(
                name: "IX_healthrecords_HouseholdID",
                table: "healthrecords");

            migrationBuilder.DropIndex(
                name: "IX_healthrecords_MemberID",
                table: "healthrecords");

            migrationBuilder.DropPrimaryKey(
                name: "PK_predictiveanalyses",
                table: "predictiveanalyses");

            migrationBuilder.DropPrimaryKey(
                name: "PK_households",
                table: "households");

            migrationBuilder.DropColumn(
                name: "AssignedArea",
                table: "users");

            migrationBuilder.DropColumn(
                name: "Description",
                table: "taskassignments");

            migrationBuilder.DropColumn(
                name: "Title",
                table: "taskassignments");

            migrationBuilder.DropColumn(
                name: "HouseholdID",
                table: "reports");

            migrationBuilder.DropColumn(
                name: "MemberID",
                table: "reports");

            migrationBuilder.DropColumn(
                name: "CurrentStock",
                table: "inventory");

            migrationBuilder.DropColumn(
                name: "MemberID",
                table: "healthrecords");

            migrationBuilder.RenameTable(
                name: "predictiveanalyses",
                newName: "predictiveanalysis");

            migrationBuilder.RenameTable(
                name: "households",
                newName: "household");

            migrationBuilder.AlterColumn<string>(
                name: "Role",
                table: "users",
                type: "longtext",
                nullable: false,
                oldClrType: typeof(string),
                oldType: "varchar(30)",
                oldMaxLength: 30)
                .Annotation("MySql:CharSet", "utf8mb4")
                .OldAnnotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.AlterColumn<string>(
                name: "ProfilePicture",
                table: "users",
                type: "longtext",
                nullable: true,
                oldClrType: typeof(string),
                oldType: "varchar(255)",
                oldMaxLength: 255,
                oldNullable: true)
                .Annotation("MySql:CharSet", "utf8mb4")
                .OldAnnotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.AlterColumn<string>(
                name: "Password",
                table: "users",
                type: "longtext",
                nullable: false,
                oldClrType: typeof(string),
                oldType: "varchar(255)",
                oldMaxLength: 255)
                .Annotation("MySql:CharSet", "utf8mb4")
                .OldAnnotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.AlterColumn<string>(
                name: "FullName",
                table: "users",
                type: "longtext",
                nullable: false,
                oldClrType: typeof(string),
                oldType: "varchar(150)",
                oldMaxLength: 150)
                .Annotation("MySql:CharSet", "utf8mb4")
                .OldAnnotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.AlterColumn<string>(
                name: "Email",
                table: "users",
                type: "longtext",
                nullable: false,
                oldClrType: typeof(string),
                oldType: "varchar(150)",
                oldMaxLength: 150)
                .Annotation("MySql:CharSet", "utf8mb4")
                .OldAnnotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.AlterColumn<string>(
                name: "ContactNumber",
                table: "users",
                type: "longtext",
                nullable: false,
                oldClrType: typeof(string),
                oldType: "varchar(50)",
                oldMaxLength: 50)
                .Annotation("MySql:CharSet", "utf8mb4")
                .OldAnnotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.AlterColumn<string>(
                name: "Status",
                table: "taskassignments",
                type: "longtext",
                nullable: false,
                oldClrType: typeof(string),
                oldType: "varchar(50)",
                oldMaxLength: 50)
                .Annotation("MySql:CharSet", "utf8mb4")
                .OldAnnotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.AlterColumn<string>(
                name: "Priority",
                table: "taskassignments",
                type: "longtext",
                nullable: false,
                oldClrType: typeof(string),
                oldType: "varchar(50)",
                oldMaxLength: 50)
                .Annotation("MySql:CharSet", "utf8mb4")
                .OldAnnotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.AlterColumn<string>(
                name: "ReportType",
                table: "reports",
                type: "longtext",
                nullable: false,
                oldClrType: typeof(string),
                oldType: "varchar(150)",
                oldMaxLength: 150)
                .Annotation("MySql:CharSet", "utf8mb4")
                .OldAnnotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.AlterColumn<string>(
                name: "Content",
                table: "reports",
                type: "longtext",
                nullable: false,
                oldClrType: typeof(string),
                oldType: "varchar(4000)",
                oldMaxLength: 4000)
                .Annotation("MySql:CharSet", "utf8mb4")
                .OldAnnotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.AlterColumn<string>(
                name: "Unit",
                table: "inventory",
                type: "longtext",
                nullable: false,
                oldClrType: typeof(string),
                oldType: "varchar(50)",
                oldMaxLength: 50)
                .Annotation("MySql:CharSet", "utf8mb4")
                .OldAnnotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.AlterColumn<string>(
                name: "ItemName",
                table: "inventory",
                type: "longtext",
                nullable: false,
                oldClrType: typeof(string),
                oldType: "varchar(150)",
                oldMaxLength: 150)
                .Annotation("MySql:CharSet", "utf8mb4")
                .OldAnnotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.AlterColumn<string>(
                name: "Symptoms",
                table: "healthrecords",
                type: "longtext",
                nullable: false,
                oldClrType: typeof(string),
                oldType: "varchar(1000)",
                oldMaxLength: 1000)
                .Annotation("MySql:CharSet", "utf8mb4")
                .OldAnnotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.AlterColumn<string>(
                name: "Status",
                table: "healthrecords",
                type: "longtext",
                nullable: false,
                oldClrType: typeof(string),
                oldType: "varchar(50)",
                oldMaxLength: 50)
                .Annotation("MySql:CharSet", "utf8mb4")
                .OldAnnotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.AlterColumn<string>(
                name: "Disease",
                table: "healthrecords",
                type: "longtext",
                nullable: false,
                oldClrType: typeof(string),
                oldType: "varchar(150)",
                oldMaxLength: 150)
                .Annotation("MySql:CharSet", "utf8mb4")
                .OldAnnotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.AlterColumn<string>(
                name: "HighRiskBarangay",
                table: "predictiveanalysis",
                type: "longtext",
                nullable: false,
                oldClrType: typeof(string),
                oldType: "varchar(150)",
                oldMaxLength: 150)
                .Annotation("MySql:CharSet", "utf8mb4")
                .OldAnnotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.AlterColumn<string>(
                name: "Disease",
                table: "predictiveanalysis",
                type: "longtext",
                nullable: false,
                oldClrType: typeof(string),
                oldType: "varchar(150)",
                oldMaxLength: 150)
                .Annotation("MySql:CharSet", "utf8mb4")
                .OldAnnotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.AlterColumn<string>(
                name: "HouseholdMember",
                table: "household",
                type: "longtext",
                nullable: false,
                oldClrType: typeof(string),
                oldType: "varchar(150)",
                oldMaxLength: 150)
                .Annotation("MySql:CharSet", "utf8mb4")
                .OldAnnotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.AlterColumn<string>(
                name: "Address",
                table: "household",
                type: "longtext",
                nullable: false,
                oldClrType: typeof(string),
                oldType: "varchar(255)",
                oldMaxLength: 255)
                .Annotation("MySql:CharSet", "utf8mb4")
                .OldAnnotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.AddPrimaryKey(
                name: "PK_predictiveanalysis",
                table: "predictiveanalysis",
                column: "AnalyticsID");

            migrationBuilder.AddPrimaryKey(
                name: "PK_household",
                table: "household",
                column: "HouseholdID");
        }
    }
}
