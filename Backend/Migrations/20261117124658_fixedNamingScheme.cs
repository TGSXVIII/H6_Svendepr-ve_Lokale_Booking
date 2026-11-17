using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace MercBooking.Api.Migrations
{
    /// <inheritdoc />
    public partial class fixedNamingScheme : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.RenameColumn(
                name: "RequestedStart",
                table: "Bookings",
                newName: "BookingStart");

            migrationBuilder.RenameColumn(
                name: "RequestedEnd",
                table: "Bookings",
                newName: "BookingEnd");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.RenameColumn(
                name: "BookingStart",
                table: "Bookings",
                newName: "RequestedStart");

            migrationBuilder.RenameColumn(
                name: "BookingEnd",
                table: "Bookings",
                newName: "RequestedEnd");
        }
    }
}
