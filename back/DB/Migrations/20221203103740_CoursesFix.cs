using Microsoft.EntityFrameworkCore.Migrations;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;

#nullable disable

namespace DB.Migrations
{
    public partial class CoursesFix : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_TopicCardEntity_TopicCollectionEntity_ParentTopicCollection~",
                table: "TopicCardEntity");

            migrationBuilder.DropForeignKey(
                name: "FK_TopicCollectionEntity_Topics_ParentTopicId_Id",
                table: "TopicCollectionEntity");

            migrationBuilder.DropPrimaryKey(
                name: "PK_TopicCollectionEntity",
                table: "TopicCollectionEntity");

            migrationBuilder.DropPrimaryKey(
                name: "PK_TopicCardEntity",
                table: "TopicCardEntity");

            migrationBuilder.AlterColumn<long>(
                name: "Id",
                table: "TopicCollectionEntity",
                type: "bigint",
                nullable: false,
                oldClrType: typeof(long),
                oldType: "bigint")
                .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn);

            migrationBuilder.AddColumn<long>(
                name: "ParentCourseId",
                table: "TopicCollectionEntity",
                type: "bigint",
                nullable: false,
                defaultValue: 0L);

            migrationBuilder.AddColumn<long>(
                name: "CourseId",
                table: "TopicCollectionEntity",
                type: "bigint",
                nullable: true);

            migrationBuilder.AlterColumn<long>(
                name: "Id",
                table: "TopicCardEntity",
                type: "bigint",
                nullable: false,
                oldClrType: typeof(long),
                oldType: "bigint")
                .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn);

            migrationBuilder.AddColumn<long>(
                name: "ParentCourseId",
                table: "TopicCardEntity",
                type: "bigint",
                nullable: false,
                defaultValue: 0L);

            migrationBuilder.AddColumn<long>(
                name: "ParentTopicId",
                table: "TopicCardEntity",
                type: "bigint",
                nullable: false,
                defaultValue: 0L);

            migrationBuilder.AddColumn<long>(
                name: "CourseId",
                table: "TopicCardEntity",
                type: "bigint",
                nullable: true);

            migrationBuilder.AddColumn<long>(
                name: "TopicId",
                table: "TopicCardEntity",
                type: "bigint",
                nullable: true);

            migrationBuilder.AddColumn<long>(
                name: "TopicParentCourseId",
                table: "TopicCardEntity",
                type: "bigint",
                nullable: true);

            migrationBuilder.AddPrimaryKey(
                name: "PK_TopicCollectionEntity",
                table: "TopicCollectionEntity",
                columns: new[] { "ParentCourseId", "ParentTopicId", "Id" });

            migrationBuilder.AddPrimaryKey(
                name: "PK_TopicCardEntity",
                table: "TopicCardEntity",
                columns: new[] { "ParentCourseId", "ParentTopicId", "ParentTopicCollectionId", "Id" });

            migrationBuilder.CreateIndex(
                name: "IX_TopicCollectionEntity_CourseId",
                table: "TopicCollectionEntity",
                column: "CourseId");

            migrationBuilder.CreateIndex(
                name: "IX_TopicCardEntity_CourseId",
                table: "TopicCardEntity",
                column: "CourseId");

            migrationBuilder.CreateIndex(
                name: "IX_TopicCardEntity_TopicParentCourseId_TopicId",
                table: "TopicCardEntity",
                columns: new[] { "TopicParentCourseId", "TopicId" });

            migrationBuilder.AddForeignKey(
                name: "FK_TopicCardEntity_Courses_CourseId",
                table: "TopicCardEntity",
                column: "CourseId",
                principalTable: "Courses",
                principalColumn: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_TopicCardEntity_TopicCollectionEntity_ParentCourseId_Parent~",
                table: "TopicCardEntity",
                columns: new[] { "ParentCourseId", "ParentTopicId", "ParentTopicCollectionId" },
                principalTable: "TopicCollectionEntity",
                principalColumns: new[] { "ParentCourseId", "ParentTopicId", "Id" },
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_TopicCardEntity_Topics_TopicParentCourseId_TopicId",
                table: "TopicCardEntity",
                columns: new[] { "TopicParentCourseId", "TopicId" },
                principalTable: "Topics",
                principalColumns: new[] { "ParentCourseId", "Id" });

            migrationBuilder.AddForeignKey(
                name: "FK_TopicCollectionEntity_Courses_CourseId",
                table: "TopicCollectionEntity",
                column: "CourseId",
                principalTable: "Courses",
                principalColumn: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_TopicCollectionEntity_Topics_ParentCourseId_ParentTopicId",
                table: "TopicCollectionEntity",
                columns: new[] { "ParentCourseId", "ParentTopicId" },
                principalTable: "Topics",
                principalColumns: new[] { "ParentCourseId", "Id" },
                onDelete: ReferentialAction.Cascade);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_TopicCardEntity_Courses_CourseId",
                table: "TopicCardEntity");

            migrationBuilder.DropForeignKey(
                name: "FK_TopicCardEntity_TopicCollectionEntity_ParentCourseId_Parent~",
                table: "TopicCardEntity");

            migrationBuilder.DropForeignKey(
                name: "FK_TopicCardEntity_Topics_TopicParentCourseId_TopicId",
                table: "TopicCardEntity");

            migrationBuilder.DropForeignKey(
                name: "FK_TopicCollectionEntity_Courses_CourseId",
                table: "TopicCollectionEntity");

            migrationBuilder.DropForeignKey(
                name: "FK_TopicCollectionEntity_Topics_ParentCourseId_ParentTopicId",
                table: "TopicCollectionEntity");

            migrationBuilder.DropPrimaryKey(
                name: "PK_TopicCollectionEntity",
                table: "TopicCollectionEntity");

            migrationBuilder.DropIndex(
                name: "IX_TopicCollectionEntity_CourseId",
                table: "TopicCollectionEntity");

            migrationBuilder.DropPrimaryKey(
                name: "PK_TopicCardEntity",
                table: "TopicCardEntity");

            migrationBuilder.DropIndex(
                name: "IX_TopicCardEntity_CourseId",
                table: "TopicCardEntity");

            migrationBuilder.DropIndex(
                name: "IX_TopicCardEntity_TopicParentCourseId_TopicId",
                table: "TopicCardEntity");

            migrationBuilder.DropColumn(
                name: "ParentCourseId",
                table: "TopicCollectionEntity");

            migrationBuilder.DropColumn(
                name: "CourseId",
                table: "TopicCollectionEntity");

            migrationBuilder.DropColumn(
                name: "ParentCourseId",
                table: "TopicCardEntity");

            migrationBuilder.DropColumn(
                name: "ParentTopicId",
                table: "TopicCardEntity");

            migrationBuilder.DropColumn(
                name: "CourseId",
                table: "TopicCardEntity");

            migrationBuilder.DropColumn(
                name: "TopicId",
                table: "TopicCardEntity");

            migrationBuilder.DropColumn(
                name: "TopicParentCourseId",
                table: "TopicCardEntity");

            migrationBuilder.AlterColumn<long>(
                name: "Id",
                table: "TopicCollectionEntity",
                type: "bigint",
                nullable: false,
                oldClrType: typeof(long),
                oldType: "bigint")
                .OldAnnotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn);

            migrationBuilder.AlterColumn<long>(
                name: "Id",
                table: "TopicCardEntity",
                type: "bigint",
                nullable: false,
                oldClrType: typeof(long),
                oldType: "bigint")
                .OldAnnotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn);

            migrationBuilder.AddPrimaryKey(
                name: "PK_TopicCollectionEntity",
                table: "TopicCollectionEntity",
                columns: new[] { "ParentTopicId", "Id" });

            migrationBuilder.AddPrimaryKey(
                name: "PK_TopicCardEntity",
                table: "TopicCardEntity",
                columns: new[] { "ParentTopicCollectionId", "Id" });

            migrationBuilder.AddForeignKey(
                name: "FK_TopicCardEntity_TopicCollectionEntity_ParentTopicCollection~",
                table: "TopicCardEntity",
                columns: new[] { "ParentTopicCollectionId", "Id" },
                principalTable: "TopicCollectionEntity",
                principalColumns: new[] { "ParentTopicId", "Id" },
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_TopicCollectionEntity_Topics_ParentTopicId_Id",
                table: "TopicCollectionEntity",
                columns: new[] { "ParentTopicId", "Id" },
                principalTable: "Topics",
                principalColumns: new[] { "ParentCourseId", "Id" },
                onDelete: ReferentialAction.Cascade);
        }
    }
}
