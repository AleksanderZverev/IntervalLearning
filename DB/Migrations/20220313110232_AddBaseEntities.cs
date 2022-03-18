using Microsoft.EntityFrameworkCore.Migrations;
using NodaTime;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;

#nullable disable

namespace DB.Migrations
{
    public partial class AddBaseEntities : Migration
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
                name: "RefreshTokens",
                columns: table => new
                {
                    Id = table.Column<byte>(type: "smallint", nullable: false),
                    Token = table.Column<string>(type: "text", nullable: false),
                    Expires = table.Column<Instant>(type: "timestamp with time zone", nullable: false),
                    Created = table.Column<Instant>(type: "timestamp with time zone", nullable: false),
                    CreatedByIp = table.Column<string>(type: "character varying(15)", maxLength: 15, nullable: false),
                    Revoked = table.Column<Instant>(type: "timestamp with time zone", nullable: true),
                    RevokedByIp = table.Column<string>(type: "character varying(15)", maxLength: 15, nullable: false),
                    ReplacedByToken = table.Column<string>(type: "text", nullable: false),
                    ReasonRevoked = table.Column<string>(type: "text", nullable: false),
                    ParentUserId = table.Column<long>(type: "bigint", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_RefreshTokens", x => x.Id);
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
                    Id = table.Column<short>(type: "smallint", nullable: false),
                    ParentUserId = table.Column<long>(type: "bigint", nullable: false),
                    Title = table.Column<string>(type: "character varying(255)", maxLength: 255, nullable: false),
                    Description = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: true),
                    CardsCountPerPhase = table.Column<short>(type: "smallint", nullable: false),
                    ForgottenBehavior = table.Column<int>(type: "integer", nullable: false),
                    IsArchived = table.Column<bool>(type: "boolean", nullable: false)
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
                name: "UsersPasswords",
                columns: table => new
                {
                    ParentUserId = table.Column<long>(type: "bigint", nullable: false),
                    PasswordHash = table.Column<string>(type: "varchar(255)", maxLength: 255, nullable: true)
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
                name: "Collections",
                columns: table => new
                {
                    Id = table.Column<short>(type: "smallint", nullable: false),
                    ParentUserId = table.Column<long>(type: "bigint", nullable: false),
                    Title = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    IsDefaultBackSide = table.Column<bool>(type: "boolean", nullable: false),
                    ThemeId = table.Column<short>(type: "smallint", nullable: false),
                    CreatedDate = table.Column<Instant>(type: "timestamp with time zone", nullable: false),
                    DefaultRepeatsScheduleId = table.Column<short>(type: "smallint", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Collections", x => new { x.ParentUserId, x.Id });
                    table.ForeignKey(
                        name: "FK_Collections_RepeatsSchedules_ParentUserId_DefaultRepeatsSch~",
                        columns: x => new { x.ParentUserId, x.DefaultRepeatsScheduleId },
                        principalTable: "RepeatsSchedules",
                        principalColumns: new[] { "ParentUserId", "Id" });
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
                name: "SchedulePhases",
                columns: table => new
                {
                    Id = table.Column<byte>(type: "smallint", nullable: false),
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
                name: "Cards",
                columns: table => new
                {
                    Id = table.Column<short>(type: "smallint", nullable: false),
                    ParentUserId = table.Column<long>(type: "bigint", nullable: false),
                    ParentCollectionId = table.Column<short>(type: "smallint", nullable: false),
                    FrontSideText = table.Column<string>(type: "character varying(255)", maxLength: 255, nullable: false),
                    BackSideText = table.Column<string>(type: "character varying(255)", maxLength: 255, nullable: false),
                    CreatedDate = table.Column<Instant>(type: "timestamp with time zone", nullable: false),
                    Description = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: true),
                    Examples = table.Column<List<string>>(type: "text[]", maxLength: 255, nullable: true),
                    IsFinished = table.Column<bool>(type: "boolean", nullable: true),
                    ParentRepeatsScheduleId = table.Column<short>(type: "smallint", nullable: false)
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
                        name: "FK_Cards_RepeatsSchedules_ParentUserId_ParentRepeatsScheduleId",
                        columns: x => new { x.ParentUserId, x.ParentRepeatsScheduleId },
                        principalTable: "RepeatsSchedules",
                        principalColumns: new[] { "ParentUserId", "Id" });
                    table.ForeignKey(
                        name: "FK_Cards_Users_ParentUserId",
                        column: x => x.ParentUserId,
                        principalTable: "Users",
                        principalColumn: "Id");
                });

            migrationBuilder.CreateTable(
                name: "RememberWeights",
                columns: table => new
                {
                    Id = table.Column<byte>(type: "smallint", nullable: false),
                    ParentUserId = table.Column<long>(type: "bigint", nullable: false),
                    ParentCollectionId = table.Column<short>(type: "smallint", nullable: false),
                    ParentCardId = table.Column<short>(type: "smallint", nullable: false),
                    Weight = table.Column<float>(type: "real", nullable: false),
                    PhaseStep = table.Column<byte>(type: "smallint", nullable: false),
                    PassedSecondsFromLastStep = table.Column<int>(type: "integer", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_RememberWeights", x => new { x.ParentUserId, x.ParentCollectionId, x.ParentCardId, x.Id });
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
                        name: "FK_RememberWeights_Users_ParentUserId",
                        column: x => x.ParentUserId,
                        principalTable: "Users",
                        principalColumn: "Id");
                });

            migrationBuilder.CreateIndex(
                name: "IX_Cards_ParentUserId_ParentRepeatsScheduleId",
                table: "Cards",
                columns: new[] { "ParentUserId", "ParentRepeatsScheduleId" });

            migrationBuilder.CreateIndex(
                name: "IX_Collections_ParentUserId_DefaultRepeatsScheduleId",
                table: "Collections",
                columns: new[] { "ParentUserId", "DefaultRepeatsScheduleId" });

            migrationBuilder.CreateIndex(
                name: "IX_Collections_ThemeId",
                table: "Collections",
                column: "ThemeId");

            migrationBuilder.CreateIndex(
                name: "IX_RefreshTokens_ParentUserId",
                table: "RefreshTokens",
                column: "ParentUserId");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "RefreshTokens");

            migrationBuilder.DropTable(
                name: "RememberWeights");

            migrationBuilder.DropTable(
                name: "SchedulePhases");

            migrationBuilder.DropTable(
                name: "UsersPasswords");

            migrationBuilder.DropTable(
                name: "Cards");

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
