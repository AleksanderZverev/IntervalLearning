using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace DB.Migrations
{
    public partial class AddCardCascadeDelete : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Queue_Cards_ParentUserId_ParentCollectionId_ParentCardId",
                table: "Queue");

            migrationBuilder.DropForeignKey(
                name: "FK_RememberWeights_Cards_ParentUserId_ParentCollectionId_Paren~",
                table: "RememberWeights");

            migrationBuilder.AddForeignKey(
                name: "FK_Queue_Cards_ParentUserId_ParentCollectionId_ParentCardId",
                table: "Queue",
                columns: new[] { "ParentUserId", "ParentCollectionId", "ParentCardId" },
                principalTable: "Cards",
                principalColumns: new[] { "ParentUserId", "ParentCollectionId", "Id" },
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_RememberWeights_Cards_ParentUserId_ParentCollectionId_Paren~",
                table: "RememberWeights",
                columns: new[] { "ParentUserId", "ParentCollectionId", "ParentCardId" },
                principalTable: "Cards",
                principalColumns: new[] { "ParentUserId", "ParentCollectionId", "Id" },
                onDelete: ReferentialAction.Cascade);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Queue_Cards_ParentUserId_ParentCollectionId_ParentCardId",
                table: "Queue");

            migrationBuilder.DropForeignKey(
                name: "FK_RememberWeights_Cards_ParentUserId_ParentCollectionId_Paren~",
                table: "RememberWeights");

            migrationBuilder.AddForeignKey(
                name: "FK_Queue_Cards_ParentUserId_ParentCollectionId_ParentCardId",
                table: "Queue",
                columns: new[] { "ParentUserId", "ParentCollectionId", "ParentCardId" },
                principalTable: "Cards",
                principalColumns: new[] { "ParentUserId", "ParentCollectionId", "Id" });

            migrationBuilder.AddForeignKey(
                name: "FK_RememberWeights_Cards_ParentUserId_ParentCollectionId_Paren~",
                table: "RememberWeights",
                columns: new[] { "ParentUserId", "ParentCollectionId", "ParentCardId" },
                principalTable: "Cards",
                principalColumns: new[] { "ParentUserId", "ParentCollectionId", "Id" });
        }
    }
}
