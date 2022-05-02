using System;
using System.Collections.Generic;
using Microsoft.EntityFrameworkCore.Migrations;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;

#nullable disable

namespace DB.Migrations
{
    public partial class InitialMigration : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "Themes",
                columns: table => new
                {
                    Id = table.Column<short>(type: "smallint", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    Name = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Themes", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "Users",
                columns: table => new
                {
                    Id = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    FirstName = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    LastName = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    Email = table.Column<string>(type: "character varying(255)", maxLength: 255, nullable: false),
                    EmailConfirmed = table.Column<bool>(type: "boolean", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Users", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "Collections",
                columns: table => new
                {
                    Id = table.Column<short>(type: "smallint", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    ParentUserId = table.Column<long>(type: "bigint", nullable: false),
                    Title = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    IsDefaultBackSide = table.Column<bool>(type: "boolean", nullable: false),
                    ThemeId = table.Column<short>(type: "smallint", nullable: false),
                    CreatedDate = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    CardsCount = table.Column<short>(type: "smallint", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Collections", x => new { x.ParentUserId, x.Id });
                    table.ForeignKey(
                        name: "FK_Collections_Themes_ThemeId",
                        column: x => x.ThemeId,
                        principalTable: "Themes",
                        principalColumn: "Id");
                    table.ForeignKey(
                        name: "FK_Collections_Users_ParentUserId",
                        column: x => x.ParentUserId,
                        principalTable: "Users",
                        principalColumn: "Id");
                });

            migrationBuilder.CreateTable(
                name: "RefreshTokens",
                columns: table => new
                {
                    Id = table.Column<short>(type: "smallint", nullable: false),
                    ParentUserId = table.Column<long>(type: "bigint", nullable: false),
                    Token = table.Column<string>(type: "text", nullable: false),
                    Expires = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    Created = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    CreatedByIp = table.Column<string>(type: "character varying(15)", maxLength: 15, nullable: false),
                    Revoked = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    RevokedByIp = table.Column<string>(type: "character varying(15)", maxLength: 15, nullable: true),
                    ReplacedByToken = table.Column<string>(type: "text", nullable: true),
                    ReasonRevoked = table.Column<string>(type: "text", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_RefreshTokens", x => new { x.ParentUserId, x.Id });
                    table.ForeignKey(
                        name: "FK_RefreshTokens_Users_ParentUserId",
                        column: x => x.ParentUserId,
                        principalTable: "Users",
                        principalColumn: "Id");
                });

            migrationBuilder.CreateTable(
                name: "RepeatsSchedules",
                columns: table => new
                {
                    Id = table.Column<short>(type: "smallint", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    ParentUserId = table.Column<long>(type: "bigint", nullable: false),
                    Title = table.Column<string>(type: "character varying(255)", maxLength: 255, nullable: false),
                    Description = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: true),
                    CardsCountPerPhase = table.Column<short>(type: "smallint", nullable: false),
                    ForgottenBehavior = table.Column<int>(type: "integer", nullable: false),
                    IsArchived = table.Column<bool>(type: "boolean", nullable: false),
                    IsRecommended = table.Column<bool>(type: "boolean", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_RepeatsSchedules", x => new { x.ParentUserId, x.Id });
                    table.ForeignKey(
                        name: "FK_RepeatsSchedules_Users_ParentUserId",
                        column: x => x.ParentUserId,
                        principalTable: "Users",
                        principalColumn: "Id");
                });

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

            migrationBuilder.CreateTable(
                name: "UsersPasswords",
                columns: table => new
                {
                    ParentUserId = table.Column<long>(type: "bigint", nullable: false),
                    PasswordHash = table.Column<string>(type: "varchar(60)", maxLength: 60, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_UsersPasswords", x => x.ParentUserId);
                    table.ForeignKey(
                        name: "FK_UsersPasswords_Users_ParentUserId",
                        column: x => x.ParentUserId,
                        principalTable: "Users",
                        principalColumn: "Id");
                });

            migrationBuilder.CreateTable(
                name: "Cards",
                columns: table => new
                {
                    Id = table.Column<short>(type: "smallint", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    ParentUserId = table.Column<long>(type: "bigint", nullable: false),
                    ParentCollectionId = table.Column<short>(type: "smallint", nullable: false),
                    FrontSideText = table.Column<string>(type: "character varying(255)", maxLength: 255, nullable: false),
                    BackSideText = table.Column<string>(type: "character varying(255)", maxLength: 255, nullable: false),
                    CreatedDate = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    Description = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: true),
                    Examples = table.Column<List<string>>(type: "text[]", maxLength: 255, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Cards", x => new { x.ParentUserId, x.ParentCollectionId, x.Id });
                    table.ForeignKey(
                        name: "FK_Cards_Collections_ParentUserId_ParentCollectionId",
                        columns: x => new { x.ParentUserId, x.ParentCollectionId },
                        principalTable: "Collections",
                        principalColumns: new[] { "ParentUserId", "Id" });
                    table.ForeignKey(
                        name: "FK_Cards_Users_ParentUserId",
                        column: x => x.ParentUserId,
                        principalTable: "Users",
                        principalColumn: "Id");
                });

            migrationBuilder.CreateTable(
                name: "SchedulePhases",
                columns: table => new
                {
                    Id = table.Column<short>(type: "smallint", nullable: false),
                    ParentRepeatsScheduleId = table.Column<short>(type: "smallint", nullable: false),
                    ParentUserId = table.Column<long>(type: "bigint", nullable: false),
                    SecondsFromLastPhase = table.Column<long>(type: "bigint", nullable: false),
                    Description = table.Column<string>(type: "character varying(150)", maxLength: 150, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_SchedulePhases", x => new { x.ParentUserId, x.ParentRepeatsScheduleId, x.Id });
                    table.ForeignKey(
                        name: "FK_SchedulePhases_RepeatsSchedules_ParentUserId_ParentRepeatsS~",
                        columns: x => new { x.ParentUserId, x.ParentRepeatsScheduleId },
                        principalTable: "RepeatsSchedules",
                        principalColumns: new[] { "ParentUserId", "Id" });
                    table.ForeignKey(
                        name: "FK_SchedulePhases_Users_ParentUserId",
                        column: x => x.ParentUserId,
                        principalTable: "Users",
                        principalColumn: "Id");
                });

            migrationBuilder.CreateTable(
                name: "PhaseRememberEntities",
                columns: table => new
                {
                    Id = table.Column<short>(type: "smallint", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    RepeatedUserId = table.Column<long>(type: "bigint", nullable: false),
                    ParentUserId = table.Column<long>(type: "bigint", nullable: false),
                    ParentRepeatsScheduleId = table.Column<short>(type: "smallint", nullable: false),
                    ParentPhaseId = table.Column<short>(type: "smallint", nullable: false),
                    Weight = table.Column<float>(type: "real", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_PhaseRememberEntities", x => new { x.ParentUserId, x.ParentRepeatsScheduleId, x.ParentPhaseId, x.RepeatedUserId, x.Id });
                    table.ForeignKey(
                        name: "FK_PhaseRememberEntities_RepeatsSchedules_ParentUserId_ParentR~",
                        columns: x => new { x.ParentUserId, x.ParentRepeatsScheduleId },
                        principalTable: "RepeatsSchedules",
                        principalColumns: new[] { "ParentUserId", "Id" });
                    table.ForeignKey(
                        name: "FK_PhaseRememberEntities_SchedulePhases_ParentUserId_ParentRep~",
                        columns: x => new { x.ParentUserId, x.ParentRepeatsScheduleId, x.ParentPhaseId },
                        principalTable: "SchedulePhases",
                        principalColumns: new[] { "ParentUserId", "ParentRepeatsScheduleId", "Id" });
                    table.ForeignKey(
                        name: "FK_PhaseRememberEntities_Users_ParentUserId",
                        column: x => x.ParentUserId,
                        principalTable: "Users",
                        principalColumn: "Id");
                    table.ForeignKey(
                        name: "FK_PhaseRememberEntities_Users_RepeatedUserId",
                        column: x => x.RepeatedUserId,
                        principalTable: "Users",
                        principalColumn: "Id");
                });

            migrationBuilder.CreateTable(
                name: "Queue",
                columns: table => new
                {
                    PhaseId = table.Column<short>(type: "smallint", nullable: false),
                    ParentUserId = table.Column<long>(type: "bigint", nullable: false),
                    ParentCollectionId = table.Column<short>(type: "smallint", nullable: false),
                    ParentCardId = table.Column<short>(type: "smallint", nullable: false),
                    ParentRepeatsScheduleUserId = table.Column<long>(type: "bigint", nullable: false),
                    ParentRepeatsScheduleId = table.Column<short>(type: "smallint", nullable: false),
                    Date = table.Column<DateTime>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Queue", x => new { x.ParentUserId, x.ParentCollectionId, x.ParentCardId, x.ParentRepeatsScheduleUserId, x.ParentRepeatsScheduleId, x.PhaseId });
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
                        name: "FK_Queue_RepeatsSchedules_ParentRepeatsScheduleUserId_ParentRe~",
                        columns: x => new { x.ParentRepeatsScheduleUserId, x.ParentRepeatsScheduleId },
                        principalTable: "RepeatsSchedules",
                        principalColumns: new[] { "ParentUserId", "Id" });
                    table.ForeignKey(
                        name: "FK_Queue_SchedulePhases_ParentRepeatsScheduleUserId_ParentRepe~",
                        columns: x => new { x.ParentRepeatsScheduleUserId, x.ParentRepeatsScheduleId, x.PhaseId },
                        principalTable: "SchedulePhases",
                        principalColumns: new[] { "ParentUserId", "ParentRepeatsScheduleId", "Id" });
                    table.ForeignKey(
                        name: "FK_Queue_Users_ParentUserId",
                        column: x => x.ParentUserId,
                        principalTable: "Users",
                        principalColumn: "Id");
                });

            migrationBuilder.CreateTable(
                name: "RememberWeights",
                columns: table => new
                {
                    Id = table.Column<short>(type: "smallint", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    PhaseId = table.Column<short>(type: "smallint", nullable: false),
                    ParentUserId = table.Column<long>(type: "bigint", nullable: false),
                    ParentCollectionId = table.Column<short>(type: "smallint", nullable: false),
                    ParentCardId = table.Column<short>(type: "smallint", nullable: false),
                    ParentRepeatsScheduleUserId = table.Column<long>(type: "bigint", nullable: false),
                    ParentRepeatsScheduleId = table.Column<short>(type: "smallint", nullable: false),
                    Weight = table.Column<float>(type: "real", nullable: false),
                    RepeatedDate = table.Column<DateTime>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_RememberWeights", x => new { x.ParentUserId, x.ParentCollectionId, x.ParentCardId, x.ParentRepeatsScheduleUserId, x.ParentRepeatsScheduleId, x.PhaseId, x.Id });
                    table.ForeignKey(
                        name: "FK_RememberWeights_Cards_ParentUserId_ParentCollectionId_Paren~",
                        columns: x => new { x.ParentUserId, x.ParentCollectionId, x.ParentCardId },
                        principalTable: "Cards",
                        principalColumns: new[] { "ParentUserId", "ParentCollectionId", "Id" });
                    table.ForeignKey(
                        name: "FK_RememberWeights_Collections_ParentUserId_ParentCollectionId",
                        columns: x => new { x.ParentUserId, x.ParentCollectionId },
                        principalTable: "Collections",
                        principalColumns: new[] { "ParentUserId", "Id" });
                    table.ForeignKey(
                        name: "FK_RememberWeights_RepeatsSchedules_ParentRepeatsScheduleUserI~",
                        columns: x => new { x.ParentRepeatsScheduleUserId, x.ParentRepeatsScheduleId },
                        principalTable: "RepeatsSchedules",
                        principalColumns: new[] { "ParentUserId", "Id" });
                    table.ForeignKey(
                        name: "FK_RememberWeights_SchedulePhases_ParentRepeatsScheduleUserId_~",
                        columns: x => new { x.ParentRepeatsScheduleUserId, x.ParentRepeatsScheduleId, x.PhaseId },
                        principalTable: "SchedulePhases",
                        principalColumns: new[] { "ParentUserId", "ParentRepeatsScheduleId", "Id" });
                    table.ForeignKey(
                        name: "FK_RememberWeights_Users_ParentUserId",
                        column: x => x.ParentUserId,
                        principalTable: "Users",
                        principalColumn: "Id");
                });

            migrationBuilder.CreateIndex(
                name: "IX_Collections_ThemeId",
                table: "Collections",
                column: "ThemeId");

            migrationBuilder.CreateIndex(
                name: "IX_PhaseRememberEntities_RepeatedUserId",
                table: "PhaseRememberEntities",
                column: "RepeatedUserId");

            migrationBuilder.CreateIndex(
                name: "IX_Queue_ParentRepeatsScheduleUserId_ParentRepeatsScheduleId_P~",
                table: "Queue",
                columns: new[] { "ParentRepeatsScheduleUserId", "ParentRepeatsScheduleId", "PhaseId" });

            migrationBuilder.CreateIndex(
                name: "IX_RememberWeights_ParentRepeatsScheduleUserId_ParentRepeatsSc~",
                table: "RememberWeights",
                columns: new[] { "ParentRepeatsScheduleUserId", "ParentRepeatsScheduleId", "PhaseId" });
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "PhaseRememberEntities");

            migrationBuilder.DropTable(
                name: "Queue");

            migrationBuilder.DropTable(
                name: "RefreshTokens");

            migrationBuilder.DropTable(
                name: "RememberWeights");

            migrationBuilder.DropTable(
                name: "UserMetadata");

            migrationBuilder.DropTable(
                name: "UsersPasswords");

            migrationBuilder.DropTable(
                name: "Cards");

            migrationBuilder.DropTable(
                name: "SchedulePhases");

            migrationBuilder.DropTable(
                name: "Collections");

            migrationBuilder.DropTable(
                name: "RepeatsSchedules");

            migrationBuilder.DropTable(
                name: "Themes");

            migrationBuilder.DropTable(
                name: "Users");
        }
    }
}
