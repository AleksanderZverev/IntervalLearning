using Microsoft.EntityFrameworkCore.Migrations;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;

#nullable disable

namespace DB.Migrations
{
    public partial class AddTranslations : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<short>(
                name: "SuggestTranslationLanguageId",
                table: "UserMetadata",
                type: "smallint",
                nullable: false,
                defaultValue: (short)2);

            migrationBuilder.AddColumn<string>(
                name: "NativeLanguageName",
                table: "Languages",
                type: "character varying(50)",
                maxLength: 50,
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "TranslationLink",
                table: "Languages",
                type: "text",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "TranslationLinkTitle",
                table: "Languages",
                type: "character varying(50)",
                maxLength: 50,
                nullable: true);

            migrationBuilder.CreateTable(
                name: "Translations",
                columns: table => new
                {
                    WordId = table.Column<int>(type: "integer", nullable: false),
                    LanguageId = table.Column<short>(type: "smallint", nullable: false),
                    Id = table.Column<short>(type: "smallint", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    Translation = table.Column<string>(type: "character varying(255)", maxLength: 255, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Translations", x => new { x.WordId, x.LanguageId, x.Id });
                    table.ForeignKey(
                        name: "FK_Translations_Languages_LanguageId",
                        column: x => x.LanguageId,
                        principalTable: "Languages",
                        principalColumn: "Id");
                    table.ForeignKey(
                        name: "FK_Translations_Words_WordId",
                        column: x => x.WordId,
                        principalTable: "Words",
                        principalColumn: "Id");
                });

            migrationBuilder.InsertData(
                table: "Languages",
                columns: new[] { "Id", "Name", "NativeLanguageName", "TranslationLink", "TranslationLinkTitle" },
                values: new object[,]
                {
                    { (short)1, "English", "English", null, null },
                    { (short)2, "Russian", "Русский", null, null },
                    { (short)3, "Japanese", "日本語", null, null }
                });

            migrationBuilder.CreateIndex(
                name: "IX_Words_Word",
                table: "Words",
                column: "Word");

            migrationBuilder.CreateIndex(
                name: "IX_UserMetadata_SuggestTranslationLanguageId",
                table: "UserMetadata",
                column: "SuggestTranslationLanguageId");

            migrationBuilder.CreateIndex(
                name: "IX_Translations_LanguageId",
                table: "Translations",
                column: "LanguageId");

            migrationBuilder.AddForeignKey(
                name: "FK_UserMetadata_Languages_SuggestTranslationLanguageId",
                table: "UserMetadata",
                column: "SuggestTranslationLanguageId",
                principalTable: "Languages",
                principalColumn: "Id");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_UserMetadata_Languages_SuggestTranslationLanguageId",
                table: "UserMetadata");

            migrationBuilder.DropTable(
                name: "Translations");

            migrationBuilder.DropIndex(
                name: "IX_Words_Word",
                table: "Words");

            migrationBuilder.DropIndex(
                name: "IX_UserMetadata_SuggestTranslationLanguageId",
                table: "UserMetadata");

            migrationBuilder.DeleteData(
                table: "Languages",
                keyColumn: "Id",
                keyValue: (short)1);

            migrationBuilder.DeleteData(
                table: "Languages",
                keyColumn: "Id",
                keyValue: (short)2);

            migrationBuilder.DeleteData(
                table: "Languages",
                keyColumn: "Id",
                keyValue: (short)3);

            migrationBuilder.DropColumn(
                name: "SuggestTranslationLanguageId",
                table: "UserMetadata");

            migrationBuilder.DropColumn(
                name: "NativeLanguageName",
                table: "Languages");

            migrationBuilder.DropColumn(
                name: "TranslationLink",
                table: "Languages");

            migrationBuilder.DropColumn(
                name: "TranslationLinkTitle",
                table: "Languages");
        }
    }
}
