using Microsoft.EntityFrameworkCore.Migrations;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;

#nullable disable

namespace DB.Migrations
{
    public partial class RememberEntityFix : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Queue_SchedulePhases_ParentRepeatsScheduleUserId_ParentRepe~",
                table: "Queue");

            migrationBuilder.DropForeignKey(
                name: "FK_RememberWeights_SchedulePhases_ParentRepeatsScheduleUserId_~",
                table: "RememberWeights");

            migrationBuilder.DropIndex(
                name: "IX_RememberWeights_ParentRepeatsScheduleUserId_ParentRepeatsSc~",
                table: "RememberWeights");

            migrationBuilder.DropPrimaryKey(
                name: "PK_Queue",
                table: "Queue");

            migrationBuilder.DropIndex(
                name: "IX_Queue_ParentRepeatsScheduleUserId_ParentRepeatsScheduleId_P~",
                table: "Queue");

            migrationBuilder.RenameColumn(
                name: "PhaseId",
                table: "RememberWeights",
                newName: "PhaseIndex");

            migrationBuilder.RenameColumn(
                name: "PhaseId",
                table: "Queue",
                newName: "PhaseIndex");

            migrationBuilder.AddColumn<short>(
                name: "Id",
                table: "Queue",
                type: "smallint",
                nullable: false,
                defaultValue: (short)0)
                .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn);

            migrationBuilder.AddPrimaryKey(
                name: "PK_Queue",
                table: "Queue",
                columns: new[] { "ParentUserId", "ParentCollectionId", "ParentCardId", "ParentRepeatsScheduleUserId", "ParentRepeatsScheduleId", "Id" });

            migrationBuilder.CreateIndex(
                name: "IX_RememberWeights_ParentRepeatsScheduleUserId_ParentRepeatsSc~",
                table: "RememberWeights",
                columns: new[] { "ParentRepeatsScheduleUserId", "ParentRepeatsScheduleId" });

            migrationBuilder.CreateIndex(
                name: "IX_Queue_ParentRepeatsScheduleUserId_ParentRepeatsScheduleId",
                table: "Queue",
                columns: new[] { "ParentRepeatsScheduleUserId", "ParentRepeatsScheduleId" });
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_RememberWeights_ParentRepeatsScheduleUserId_ParentRepeatsSc~",
                table: "RememberWeights");

            migrationBuilder.DropPrimaryKey(
                name: "PK_Queue",
                table: "Queue");

            migrationBuilder.DropIndex(
                name: "IX_Queue_ParentRepeatsScheduleUserId_ParentRepeatsScheduleId",
                table: "Queue");

            migrationBuilder.DropColumn(
                name: "Id",
                table: "Queue");

            migrationBuilder.RenameColumn(
                name: "PhaseIndex",
                table: "RememberWeights",
                newName: "PhaseId");

            migrationBuilder.RenameColumn(
                name: "PhaseIndex",
                table: "Queue",
                newName: "PhaseId");

            migrationBuilder.AddPrimaryKey(
                name: "PK_Queue",
                table: "Queue",
                columns: new[] { "ParentUserId", "ParentCollectionId", "ParentCardId", "ParentRepeatsScheduleUserId", "ParentRepeatsScheduleId", "PhaseId" });

            migrationBuilder.CreateIndex(
                name: "IX_RememberWeights_ParentRepeatsScheduleUserId_ParentRepeatsSc~",
                table: "RememberWeights",
                columns: new[] { "ParentRepeatsScheduleUserId", "ParentRepeatsScheduleId", "PhaseId" });

            migrationBuilder.CreateIndex(
                name: "IX_Queue_ParentRepeatsScheduleUserId_ParentRepeatsScheduleId_P~",
                table: "Queue",
                columns: new[] { "ParentRepeatsScheduleUserId", "ParentRepeatsScheduleId", "PhaseId" });

            migrationBuilder.AddForeignKey(
                name: "FK_Queue_SchedulePhases_ParentRepeatsScheduleUserId_ParentRepe~",
                table: "Queue",
                columns: new[] { "ParentRepeatsScheduleUserId", "ParentRepeatsScheduleId", "PhaseId" },
                principalTable: "SchedulePhases",
                principalColumns: new[] { "ParentUserId", "ParentRepeatsScheduleId", "Id" });

            migrationBuilder.AddForeignKey(
                name: "FK_RememberWeights_SchedulePhases_ParentRepeatsScheduleUserId_~",
                table: "RememberWeights",
                columns: new[] { "ParentRepeatsScheduleUserId", "ParentRepeatsScheduleId", "PhaseId" },
                principalTable: "SchedulePhases",
                principalColumns: new[] { "ParentUserId", "ParentRepeatsScheduleId", "Id" });
        }
    }
}
