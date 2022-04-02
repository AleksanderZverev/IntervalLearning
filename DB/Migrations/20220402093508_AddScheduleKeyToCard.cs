using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace DB.Migrations
{
    public partial class AddScheduleKeyToCard : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Cards_RepeatsSchedules_ParentUserId_ParentRepeatsScheduleId",
                table: "Cards");

            migrationBuilder.DropIndex(
                name: "IX_Cards_ParentUserId_ParentRepeatsScheduleId",
                table: "Cards");

            migrationBuilder.AddColumn<long>(
                name: "ParentRepeatsScheduleUserId",
                table: "Cards",
                type: "bigint",
                nullable: false,
                defaultValue: 0L);

            migrationBuilder.CreateIndex(
                name: "IX_Cards_ParentRepeatsScheduleUserId_ParentRepeatsScheduleId",
                table: "Cards",
                columns: new[] { "ParentRepeatsScheduleUserId", "ParentRepeatsScheduleId" });

            migrationBuilder.AddForeignKey(
                name: "FK_Cards_RepeatsSchedules_ParentRepeatsScheduleUserId_ParentRe~",
                table: "Cards",
                columns: new[] { "ParentRepeatsScheduleUserId", "ParentRepeatsScheduleId" },
                principalTable: "RepeatsSchedules",
                principalColumns: new[] { "ParentUserId", "Id" });
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Cards_RepeatsSchedules_ParentRepeatsScheduleUserId_ParentRe~",
                table: "Cards");

            migrationBuilder.DropIndex(
                name: "IX_Cards_ParentRepeatsScheduleUserId_ParentRepeatsScheduleId",
                table: "Cards");

            migrationBuilder.DropColumn(
                name: "ParentRepeatsScheduleUserId",
                table: "Cards");

            migrationBuilder.CreateIndex(
                name: "IX_Cards_ParentUserId_ParentRepeatsScheduleId",
                table: "Cards",
                columns: new[] { "ParentUserId", "ParentRepeatsScheduleId" });

            migrationBuilder.AddForeignKey(
                name: "FK_Cards_RepeatsSchedules_ParentUserId_ParentRepeatsScheduleId",
                table: "Cards",
                columns: new[] { "ParentUserId", "ParentRepeatsScheduleId" },
                principalTable: "RepeatsSchedules",
                principalColumns: new[] { "ParentUserId", "Id" });
        }
    }
}
