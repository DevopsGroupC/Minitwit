using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace csharp_minitwit.Migrations
{
    /// <inheritdoc />
    public partial class Indexedentities : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateIndex(
                name: "IX_user_user_id",
                table: "user",
                column: "user_id");

            migrationBuilder.CreateIndex(
                name: "IX_message_flagged",
                table: "message",
                column: "flagged");

            migrationBuilder.CreateIndex(
                name: "IX_message_pub_date",
                table: "message",
                column: "pub_date");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_user_user_id",
                table: "user");

            migrationBuilder.DropIndex(
                name: "IX_message_flagged",
                table: "message");

            migrationBuilder.DropIndex(
                name: "IX_message_pub_date",
                table: "message");
        }
    }
}
