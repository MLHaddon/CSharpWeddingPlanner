using Microsoft.EntityFrameworkCore.Migrations;

namespace WeddingPlanner.Migrations
{
    public partial class ThirdM : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_ManyToMany_Users_UserId",
                table: "ManyToMany");

            migrationBuilder.DropForeignKey(
                name: "FK_ManyToMany_Weddings_WeddingId",
                table: "ManyToMany");

            migrationBuilder.DropPrimaryKey(
                name: "PK_ManyToMany",
                table: "ManyToMany");

            migrationBuilder.RenameTable(
                name: "ManyToMany",
                newName: "Associations");

            migrationBuilder.RenameIndex(
                name: "IX_ManyToMany_WeddingId",
                table: "Associations",
                newName: "IX_Associations_WeddingId");

            migrationBuilder.RenameIndex(
                name: "IX_ManyToMany_UserId",
                table: "Associations",
                newName: "IX_Associations_UserId");

            migrationBuilder.AddPrimaryKey(
                name: "PK_Associations",
                table: "Associations",
                column: "ManyToManyId");

            migrationBuilder.AddForeignKey(
                name: "FK_Associations_Users_UserId",
                table: "Associations",
                column: "UserId",
                principalTable: "Users",
                principalColumn: "UserID",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_Associations_Weddings_WeddingId",
                table: "Associations",
                column: "WeddingId",
                principalTable: "Weddings",
                principalColumn: "WeddingID",
                onDelete: ReferentialAction.Cascade);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Associations_Users_UserId",
                table: "Associations");

            migrationBuilder.DropForeignKey(
                name: "FK_Associations_Weddings_WeddingId",
                table: "Associations");

            migrationBuilder.DropPrimaryKey(
                name: "PK_Associations",
                table: "Associations");

            migrationBuilder.RenameTable(
                name: "Associations",
                newName: "ManyToMany");

            migrationBuilder.RenameIndex(
                name: "IX_Associations_WeddingId",
                table: "ManyToMany",
                newName: "IX_ManyToMany_WeddingId");

            migrationBuilder.RenameIndex(
                name: "IX_Associations_UserId",
                table: "ManyToMany",
                newName: "IX_ManyToMany_UserId");

            migrationBuilder.AddPrimaryKey(
                name: "PK_ManyToMany",
                table: "ManyToMany",
                column: "ManyToManyId");

            migrationBuilder.AddForeignKey(
                name: "FK_ManyToMany_Users_UserId",
                table: "ManyToMany",
                column: "UserId",
                principalTable: "Users",
                principalColumn: "UserID",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_ManyToMany_Weddings_WeddingId",
                table: "ManyToMany",
                column: "WeddingId",
                principalTable: "Weddings",
                principalColumn: "WeddingID",
                onDelete: ReferentialAction.Cascade);
        }
    }
}
