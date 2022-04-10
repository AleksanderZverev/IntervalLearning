using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace DB.Migrations
{
    public partial class AddUserMetadata : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<bool>(
                name: "IsFinished",
                table: "Collections",
                type: "boolean",
                nullable: true);

            migrationBuilder.CreateTable(
                name: "UserMetadata",
                columns: table => new
                {
                    ParentUserId = table.Column<long>(type: "bigint", nullable: false),
                    NotStartedCollections = table.Column<short>(type: "smallint", nullable: false),
                    StartedCollections = table.Column<short>(type: "smallint", nullable: false),
                    FinishedCollections = table.Column<short>(type: "smallint", nullable: false),
                    NotStartedCards = table.Column<short>(type: "smallint", nullable: false),
                    StartedCards = table.Column<short>(type: "smallint", nullable: false),
                    FinishedCards = table.Column<short>(type: "smallint", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_UserMetadata", x => x.ParentUserId);
                    table.ForeignKey(
                        name: "FK_UserMetadata_Users_ParentUserId",
                        column: x => x.ParentUserId,
                        principalTable: "Users",
                        principalColumn: "Id");
                });
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "UserMetadata");

            migrationBuilder.DropColumn(
                name: "IsFinished",
                table: "Collections");
        }
    }
}
