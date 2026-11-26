using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace MercBooking.Api.Migrations
{
    /// <inheritdoc />
    public partial class lastEditedAdded : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "LastEditedById",
                table: "Users",
                type: "integer",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "LastEditedById",
                table: "Rooms",
                type: "integer",
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_Users_LastEditedById",
                table: "Users",
                column: "LastEditedById");

            migrationBuilder.CreateIndex(
                name: "IX_Rooms_LastEditedById",
                table: "Rooms",
                column: "LastEditedById");

            migrationBuilder.AddForeignKey(
                name: "FK_Rooms_Users_LastEditedById",
                table: "Rooms",
                column: "LastEditedById",
                principalTable: "Users",
                principalColumn: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_Users_Users_LastEditedById",
                table: "Users",
                column: "LastEditedById",
                principalTable: "Users",
                principalColumn: "Id");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Rooms_Users_LastEditedById",
                table: "Rooms");

            migrationBuilder.DropForeignKey(
                name: "FK_Users_Users_LastEditedById",
                table: "Users");

            migrationBuilder.DropIndex(
                name: "IX_Users_LastEditedById",
                table: "Users");

            migrationBuilder.DropIndex(
                name: "IX_Rooms_LastEditedById",
                table: "Rooms");

            migrationBuilder.DropColumn(
                name: "LastEditedById",
                table: "Users");

            migrationBuilder.DropColumn(
                name: "LastEditedById",
                table: "Rooms");
        }
    }
}
