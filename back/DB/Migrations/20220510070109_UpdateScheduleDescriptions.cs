using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace DB.Migrations
{
    public partial class UpdateScheduleDescriptions : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.RenameColumn(
                name: "Description",
                table: "SchedulePhases",
                newName: "OnLearnDescription");

            migrationBuilder.RenameColumn(
                name: "Description",
                table: "RepeatsSchedules",
                newName: "OnStartLearningDescription");

            migrationBuilder.AddColumn<string>(
                name: "ShortDescription",
                table: "SchedulePhases",
                type: "character varying(100)",
                maxLength: 100,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "DefaultPhaseDescription",
                table: "RepeatsSchedules",
                type: "text",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "DefaultPhaseShortDescription",
                table: "RepeatsSchedules",
                type: "character varying(100)",
                maxLength: 100,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "DefaultRepeatPhaseDescription",
                table: "RepeatsSchedules",
                type: "text",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "DefaultRepeatPhaseShortDescription",
                table: "RepeatsSchedules",
                type: "character varying(100)",
                maxLength: 100,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "ShortDescription",
                table: "RepeatsSchedules",
                type: "character varying(100)",
                maxLength: 100,
                nullable: true);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "ShortDescription",
                table: "SchedulePhases");

            migrationBuilder.DropColumn(
                name: "DefaultPhaseDescription",
                table: "RepeatsSchedules");

            migrationBuilder.DropColumn(
                name: "DefaultPhaseShortDescription",
                table: "RepeatsSchedules");

            migrationBuilder.DropColumn(
                name: "DefaultRepeatPhaseDescription",
                table: "RepeatsSchedules");

            migrationBuilder.DropColumn(
                name: "DefaultRepeatPhaseShortDescription",
                table: "RepeatsSchedules");

            migrationBuilder.DropColumn(
                name: "ShortDescription",
                table: "RepeatsSchedules");

            migrationBuilder.RenameColumn(
                name: "OnLearnDescription",
                table: "SchedulePhases",
                newName: "Description");

            migrationBuilder.RenameColumn(
                name: "OnStartLearningDescription",
                table: "RepeatsSchedules",
                newName: "Description");
        }
    }
}
