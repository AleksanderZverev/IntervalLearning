using System;
using Microsoft.EntityFrameworkCore.Migrations;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;

#nullable disable

namespace DB.Migrations
{
    public partial class AddPublicationModels : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AlterColumn<short>(
                name: "Id",
                table: "Translations",
                type: "smallint",
                nullable: false,
                oldClrType: typeof(short),
                oldType: "smallint")
                .OldAnnotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn);

            migrationBuilder.AddColumn<bool>(
                name: "IsPublic",
                table: "Collections",
                type: "boolean",
                nullable: false,
                defaultValue: false);

            migrationBuilder.CreateTable(
                name: "CollectionPublications",
                columns: table => new
                {
                    ParentUserId = table.Column<long>(type: "bigint", nullable: false),
                    ParentCollectionId = table.Column<short>(type: "smallint", nullable: false),
                    PublishDate = table.Column<DateOnly>(type: "date", nullable: false),
                    SubscribersCount = table.Column<long>(type: "bigint", nullable: false),
                    LikesCount = table.Column<long>(type: "bigint", nullable: false),
                    DislikesCount = table.Column<long>(type: "bigint", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_CollectionPublications", x => new { x.ParentUserId, x.ParentCollectionId });
                    table.ForeignKey(
                        name: "FK_CollectionPublications_Collections_ParentUserId_ParentColle~",
                        columns: x => new { x.ParentUserId, x.ParentCollectionId },
                        principalTable: "Collections",
                        principalColumns: new[] { "ParentUserId", "Id" },
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_CollectionPublications_Users_ParentUserId",
                        column: x => x.ParentUserId,
                        principalTable: "Users",
                        principalColumn: "Id");
                });

            migrationBuilder.CreateTable(
                name: "PublicCollectionSubscriber",
                columns: table => new
                {
                    ParentUserId = table.Column<long>(type: "bigint", nullable: false),
                    ParentCollectionId = table.Column<short>(type: "smallint", nullable: false),
                    SubscriberUserId = table.Column<long>(type: "bigint", nullable: false),
                    IsLiked = table.Column<bool>(type: "boolean", nullable: false),
                    IsDisliked = table.Column<bool>(type: "boolean", nullable: false),
                    IsAdded = table.Column<bool>(type: "boolean", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_PublicCollectionSubscriber", x => new { x.ParentUserId, x.ParentCollectionId, x.SubscriberUserId });
                    table.ForeignKey(
                        name: "FK_PublicCollectionSubscriber_CollectionPublications_ParentUse~",
                        columns: x => new { x.ParentUserId, x.ParentCollectionId },
                        principalTable: "CollectionPublications",
                        principalColumns: new[] { "ParentUserId", "ParentCollectionId" },
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_PublicCollectionSubscriber_Collections_ParentUserId_ParentC~",
                        columns: x => new { x.ParentUserId, x.ParentCollectionId },
                        principalTable: "Collections",
                        principalColumns: new[] { "ParentUserId", "Id" });
                    table.ForeignKey(
                        name: "FK_PublicCollectionSubscriber_Users_ParentUserId",
                        column: x => x.ParentUserId,
                        principalTable: "Users",
                        principalColumn: "Id");
                    table.ForeignKey(
                        name: "FK_PublicCollectionSubscriber_Users_SubscriberUserId",
                        column: x => x.SubscriberUserId,
                        principalTable: "Users",
                        principalColumn: "Id");
                });

            migrationBuilder.CreateIndex(
                name: "IX_PublicCollectionSubscriber_SubscriberUserId",
                table: "PublicCollectionSubscriber",
                column: "SubscriberUserId");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "PublicCollectionSubscriber");

            migrationBuilder.DropTable(
                name: "CollectionPublications");

            migrationBuilder.DropColumn(
                name: "IsPublic",
                table: "Collections");

            migrationBuilder.AlterColumn<short>(
                name: "Id",
                table: "Translations",
                type: "smallint",
                nullable: false,
                oldClrType: typeof(short),
                oldType: "smallint")
                .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn);
        }
    }
}
