using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace DB.Migrations
{
    public partial class Fix4 : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_TopicCardEntity_Courses_CourseId",
                table: "TopicCardEntity");

            migrationBuilder.DropForeignKey(
                name: "FK_TopicCardEntity_TopicCollections_ParentCourseId_ParentTopic~",
                table: "TopicCardEntity");

            migrationBuilder.DropForeignKey(
                name: "FK_TopicCardEntity_Topics_ParentCourseId_ParentTopicId",
                table: "TopicCardEntity");

            migrationBuilder.DropPrimaryKey(
                name: "PK_TopicCardEntity",
                table: "TopicCardEntity");

            migrationBuilder.DropIndex(
                name: "IX_TopicCardEntity_CourseId",
                table: "TopicCardEntity");

            migrationBuilder.DropColumn(
                name: "CourseId",
                table: "TopicCardEntity");

            migrationBuilder.RenameTable(
                name: "TopicCardEntity",
                newName: "TopicCards");

            migrationBuilder.AddPrimaryKey(
                name: "PK_TopicCards",
                table: "TopicCards",
                columns: new[] { "ParentCourseId", "ParentTopicId", "ParentTopicCollectionId", "Id" });

            migrationBuilder.AddForeignKey(
                name: "FK_TopicCards_Courses_ParentCourseId",
                table: "TopicCards",
                column: "ParentCourseId",
                principalTable: "Courses",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_TopicCards_TopicCollections_ParentCourseId_ParentTopicId_Pa~",
                table: "TopicCards",
                columns: new[] { "ParentCourseId", "ParentTopicId", "ParentTopicCollectionId" },
                principalTable: "TopicCollections",
                principalColumns: new[] { "ParentCourseId", "ParentTopicId", "Id" },
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_TopicCards_Topics_ParentCourseId_ParentTopicId",
                table: "TopicCards",
                columns: new[] { "ParentCourseId", "ParentTopicId" },
                principalTable: "Topics",
                principalColumns: new[] { "ParentCourseId", "Id" },
                onDelete: ReferentialAction.Cascade);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_TopicCards_Courses_ParentCourseId",
                table: "TopicCards");

            migrationBuilder.DropForeignKey(
                name: "FK_TopicCards_TopicCollections_ParentCourseId_ParentTopicId_Pa~",
                table: "TopicCards");

            migrationBuilder.DropForeignKey(
                name: "FK_TopicCards_Topics_ParentCourseId_ParentTopicId",
                table: "TopicCards");

            migrationBuilder.DropPrimaryKey(
                name: "PK_TopicCards",
                table: "TopicCards");

            migrationBuilder.RenameTable(
                name: "TopicCards",
                newName: "TopicCardEntity");

            migrationBuilder.AddColumn<long>(
                name: "CourseId",
                table: "TopicCardEntity",
                type: "bigint",
                nullable: true);

            migrationBuilder.AddPrimaryKey(
                name: "PK_TopicCardEntity",
                table: "TopicCardEntity",
                columns: new[] { "ParentCourseId", "ParentTopicId", "ParentTopicCollectionId", "Id" });

            migrationBuilder.CreateIndex(
                name: "IX_TopicCardEntity_CourseId",
                table: "TopicCardEntity",
                column: "CourseId");

            migrationBuilder.AddForeignKey(
                name: "FK_TopicCardEntity_Courses_CourseId",
                table: "TopicCardEntity",
                column: "CourseId",
                principalTable: "Courses",
                principalColumn: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_TopicCardEntity_TopicCollections_ParentCourseId_ParentTopic~",
                table: "TopicCardEntity",
                columns: new[] { "ParentCourseId", "ParentTopicId", "ParentTopicCollectionId" },
                principalTable: "TopicCollections",
                principalColumns: new[] { "ParentCourseId", "ParentTopicId", "Id" },
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_TopicCardEntity_Topics_ParentCourseId_ParentTopicId",
                table: "TopicCardEntity",
                columns: new[] { "ParentCourseId", "ParentTopicId" },
                principalTable: "Topics",
                principalColumns: new[] { "ParentCourseId", "Id" },
                onDelete: ReferentialAction.Cascade);
        }
    }
}
