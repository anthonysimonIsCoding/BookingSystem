using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace BookingSystem.Migrations
{
    /// <inheritdoc />
    public partial class UsedVoucherUniqueFalse : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_UsedVouchers_UserId_BookingId",
                table: "UsedVouchers");

            migrationBuilder.CreateIndex(
                name: "IX_UsedVouchers_UserId_BookingId",
                table: "UsedVouchers",
                columns: new[] { "UserId", "BookingId" });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_UsedVouchers_UserId_BookingId",
                table: "UsedVouchers");

            migrationBuilder.CreateIndex(
                name: "IX_UsedVouchers_UserId_BookingId",
                table: "UsedVouchers",
                columns: new[] { "UserId", "BookingId" },
                unique: true);
        }
    }
}
