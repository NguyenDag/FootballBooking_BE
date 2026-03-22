using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

#pragma warning disable CA1814 // Prefer jagged arrays over multidimensional

namespace FootballBooking_BE.Migrations
{
    /// <inheritdoc />
    public partial class AllowRejectedStatus : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropCheckConstraint(
                name: "CK_BookingDetails_Status",
                table: "BookingDetails");

            migrationBuilder.AddCheckConstraint(
                name: "CK_BookingDetails_Status",
                table: "BookingDetails",
                sql: "[DetailStatus] IN ('PENDING', 'CONFIRMED', 'CANCELLED', 'COMPLETED', 'REJECTED')");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropCheckConstraint(
                name: "CK_BookingDetails_Status",
                table: "BookingDetails");

            migrationBuilder.AddCheckConstraint(
                name: "CK_BookingDetails_Status",
                table: "BookingDetails",
                sql: "[DetailStatus] IN ('PENDING', 'CONFIRMED', 'CANCELLED', 'COMPLETED')");
        }
    }
}
