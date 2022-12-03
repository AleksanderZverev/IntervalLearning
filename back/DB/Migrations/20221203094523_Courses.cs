using System.Collections.Generic;
using Microsoft.EntityFrameworkCore.Migrations;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;

#nullable disable

namespace DB.Migrations
{
    public partial class Courses : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "Courses",
                columns: table => new
                {
                    Id = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    Name = table.Column<string>(type: "character varying(255)", maxLength: 255, nullable: false),
                    Description = table.Column<string>(type: "text", nullable: false),
                    IsPrivate = table.Column<bool>(type: "boolean", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Courses", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "Topics",
                columns: table => new
                {
                    Id = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    ParentCourseId = table.Column<long>(type: "bigint", nullable: false),
                    Name = table.Column<string>(type: "character varying(255)", maxLength: 255, nullable: false),
                    Theory = table.Column<string>(type: "text", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Topics", x => new { x.ParentCourseId, x.Id });
                    table.ForeignKey(
                        name: "FK_Topics_Courses_ParentCourseId",
                        column: x => x.ParentCourseId,
                        principalTable: "Courses",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "UsersGroups",
                columns: table => new
                {
                    Id = table.Column<long>(type: "bigint", nullable: false),
                    ParentCourseId = table.Column<long>(type: "bigint", nullable: false),
                    Name = table.Column<string>(type: "text", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_UsersGroups", x => new { x.ParentCourseId, x.Id });
                    table.ForeignKey(
                        name: "FK_UsersGroups_Courses_ParentCourseId",
                        column: x => x.ParentCourseId,
                        principalTable: "Courses",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "TopicCollectionEntity",
                columns: table => new
                {
                    Id = table.Column<long>(type: "bigint", nullable: false),
                    ParentTopicId = table.Column<long>(type: "bigint", nullable: false),
                    Name = table.Column<string>(type: "text", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_TopicCollectionEntity", x => new { x.ParentTopicId, x.Id });
                    table.ForeignKey(
                        name: "FK_TopicCollectionEntity_Topics_ParentTopicId_Id",
                        columns: x => new { x.ParentTopicId, x.Id },
                        principalTable: "Topics",
                        principalColumns: new[] { "ParentCourseId", "Id" },
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "UserToCourseGroupEntity",
                columns: table => new
                {
                    ParentCourseId = table.Column<long>(type: "bigint", nullable: false),
                    UserId = table.Column<long>(type: "bigint", nullable: false),
                    UserGroupId = table.Column<long>(type: "bigint", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_UserToCourseGroupEntity", x => new { x.ParentCourseId, x.UserGroupId, x.UserId });
                    table.ForeignKey(
                        name: "FK_UserToCourseGroupEntity_Courses_ParentCourseId",
                        column: x => x.ParentCourseId,
                        principalTable: "Courses",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_UserToCourseGroupEntity_Users_UserId",
                        column: x => x.UserId,
                        principalTable: "Users",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_UserToCourseGroupEntity_UsersGroups_ParentCourseId_UserGrou~",
                        columns: x => new { x.ParentCourseId, x.UserGroupId },
                        principalTable: "UsersGroups",
                        principalColumns: new[] { "ParentCourseId", "Id" },
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "TopicCardEntity",
                columns: table => new
                {
                    Id = table.Column<long>(type: "bigint", nullable: false),
                    ParentTopicCollectionId = table.Column<long>(type: "bigint", nullable: false),
                    RememberingText = table.Column<string>(type: "character varying(255)", maxLength: 255, nullable: false),
                    PromptText = table.Column<string>(type: "character varying(255)", maxLength: 255, nullable: false),
                    MeaningText = table.Column<string>(type: "character varying(255)", maxLength: 255, nullable: false),
                    Description = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: true),
                    Examples = table.Column<List<string>>(type: "text[]", maxLength: 255, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_TopicCardEntity", x => new { x.ParentTopicCollectionId, x.Id });
                    table.ForeignKey(
                        name: "FK_TopicCardEntity_TopicCollectionEntity_ParentTopicCollection~",
                        columns: x => new { x.ParentTopicCollectionId, x.Id },
                        principalTable: "TopicCollectionEntity",
                        principalColumns: new[] { "ParentTopicId", "Id" },
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_UserToCourseGroupEntity_UserId",
                table: "UserToCourseGroupEntity",
                column: "UserId");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "TopicCardEntity");

            migrationBuilder.DropTable(
                name: "UserToCourseGroupEntity");

            migrationBuilder.DropTable(
                name: "TopicCollectionEntity");

            migrationBuilder.DropTable(
                name: "UsersGroups");

            migrationBuilder.DropTable(
                name: "Topics");

            migrationBuilder.DropTable(
                name: "Courses");
        }
    }
}
