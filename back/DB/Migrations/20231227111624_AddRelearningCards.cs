using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace DB.Migrations
{
    /// <inheritdoc />
    public partial class AddRelearningCards : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "RelearningCards",
                columns: table => new
                {
                    UserId = table.Column<long>(type: "bigint", nullable: false),
                    CollectionId = table.Column<short>(type: "smallint", nullable: false),
                    CardId = table.Column<short>(type: "smallint", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_RelearningCards", x => new { x.UserId, x.CollectionId, x.CardId });
                });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "RelearningCards");
        }
    }
}
