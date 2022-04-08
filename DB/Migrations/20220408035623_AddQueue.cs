using Microsoft.EntityFrameworkCore.Migrations;
using NodaTime;

#nullable disable

namespace DB.Migrations
{
    public partial class AddQueue : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "PassedSecondsFromLastStep",
                table: "RememberWeights");

            migrationBuilder.AddColumn<Instant>(
                name: "RepeatedDate",
                table: "RememberWeights",
                type: "timestamp with time zone",
                nullable: false,
                defaultValue: NodaTime.Instant.FromUnixTimeTicks(0L));

            migrationBuilder.CreateTable(
                name: "Queue",
                columns: table => new
                {
                    Id = table.Column<byte>(type: "smallint", nullable: false),
                    ParentUserId = table.Column<long>(type: "bigint", nullable: false),
                    ParentCollectionId = table.Column<short>(type: "smallint", nullable: false),
                    ParentCardId = table.Column<short>(type: "smallint", nullable: false),
                    PhaseStep = table.Column<byte>(type: "smallint", nullable: false),
                    Date = table.Column<Instant>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Queue", x => new { x.ParentUserId, x.ParentCollectionId, x.ParentCardId, x.Id });
                    table.ForeignKey(
                        name: "FK_Queue_Cards_ParentUserId_ParentCollectionId_ParentCardId",
                        columns: x => new { x.ParentUserId, x.ParentCollectionId, x.ParentCardId },
                        principalTable: "Cards",
                        principalColumns: new[] { "ParentUserId", "ParentCollectionId", "Id" });
                    table.ForeignKey(
                        name: "FK_Queue_Collections_ParentUserId_ParentCollectionId",
                        columns: x => new { x.ParentUserId, x.ParentCollectionId },
                        principalTable: "Collections",
                        principalColumns: new[] { "ParentUserId", "Id" });
                    table.ForeignKey(
                        name: "FK_Queue_Users_ParentUserId",
                        column: x => x.ParentUserId,
                        principalTable: "Users",
                        principalColumn: "Id");
                });
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "Queue");

            migrationBuilder.DropColumn(
                name: "RepeatedDate",
                table: "RememberWeights");

            migrationBuilder.AddColumn<int>(
                name: "PassedSecondsFromLastStep",
                table: "RememberWeights",
                type: "integer",
                nullable: false,
                defaultValue: 0);
        }
    }
}
