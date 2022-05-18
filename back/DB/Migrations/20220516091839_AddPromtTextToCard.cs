using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace DB.Migrations
{
    public partial class AddPromtTextToCard : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.RenameColumn(
                name: "FrontSideText",
                table: "Cards",
                newName: "RememberingText");

            migrationBuilder.RenameColumn(
                name: "BackSideText",
                table: "Cards",
                newName: "MeaningText");

            migrationBuilder.AddColumn<string>(
                name: "PromptText",
                table: "Cards",
                type: "character varying(255)",
                maxLength: 255,
                nullable: false,
                defaultValue: "");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "PromptText",
                table: "Cards");

            migrationBuilder.RenameColumn(
                name: "RememberingText",
                table: "Cards",
                newName: "FrontSideText");

            migrationBuilder.RenameColumn(
                name: "MeaningText",
                table: "Cards",
                newName: "BackSideText");
        }
    }
}
