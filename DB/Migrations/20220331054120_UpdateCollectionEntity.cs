using Microsoft.EntityFrameworkCore.Migrations;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;

#nullable disable

namespace DB.Migrations
{
    public partial class UpdateCollectionEntity : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Collections_RepeatsSchedules_ParentUserId_DefaultRepeatsSch~",
                table: "Collections");

            migrationBuilder.DropIndex(
                name: "IX_Collections_ParentUserId_DefaultRepeatsScheduleId",
                table: "Collections");

            migrationBuilder.AlterColumn<byte>(
                name: "Id",
                table: "SchedulePhases",
                type: "smallint",
                nullable: false,
                oldClrType: typeof(byte),
                oldType: "smallint")
                .OldAnnotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn);

            migrationBuilder.AddColumn<long>(
                name: "DefaultRepeatsScheduleParentUserId",
                table: "Collections",
                type: "bigint",
                nullable: false,
                defaultValue: 0L);

            migrationBuilder.CreateIndex(
                name: "IX_Collections_DefaultRepeatsScheduleParentUserId_DefaultRepea~",
                table: "Collections",
                columns: new[] { "DefaultRepeatsScheduleParentUserId", "DefaultRepeatsScheduleId" });

            migrationBuilder.AddForeignKey(
                name: "FK_Collections_RepeatsSchedules_DefaultRepeatsScheduleParentUs~",
                table: "Collections",
                columns: new[] { "DefaultRepeatsScheduleParentUserId", "DefaultRepeatsScheduleId" },
                principalTable: "RepeatsSchedules",
                principalColumns: new[] { "ParentUserId", "Id" });
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Collections_RepeatsSchedules_DefaultRepeatsScheduleParentUs~",
                table: "Collections");

            migrationBuilder.DropIndex(
                name: "IX_Collections_DefaultRepeatsScheduleParentUserId_DefaultRepea~",
                table: "Collections");

            migrationBuilder.DropColumn(
                name: "DefaultRepeatsScheduleParentUserId",
                table: "Collections");

            migrationBuilder.AlterColumn<byte>(
                name: "Id",
                table: "SchedulePhases",
                type: "smallint",
                nullable: false,
                oldClrType: typeof(byte),
                oldType: "smallint")
                .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn);

            migrationBuilder.CreateIndex(
                name: "IX_Collections_ParentUserId_DefaultRepeatsScheduleId",
                table: "Collections",
                columns: new[] { "ParentUserId", "DefaultRepeatsScheduleId" });

            migrationBuilder.AddForeignKey(
                name: "FK_Collections_RepeatsSchedules_ParentUserId_DefaultRepeatsSch~",
                table: "Collections",
                columns: new[] { "ParentUserId", "DefaultRepeatsScheduleId" },
                principalTable: "RepeatsSchedules",
                principalColumns: new[] { "ParentUserId", "Id" });
        }
    }
}
