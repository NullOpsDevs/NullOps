using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace NullOps.DAL.Migrations
{
    /// <inheritdoc />
    public partial class FixedUsersconstraint : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateIndex(
                name: "IX_users_Username",
                table: "users",
                column: "Username",
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_users_Username",
                table: "users");
        }
    }
}
