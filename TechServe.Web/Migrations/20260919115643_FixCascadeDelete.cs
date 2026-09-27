using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace TechServe.Web.Migrations
{
    /// <inheritdoc />
    public partial class FixCascadeDelete : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Devices_Customers_CustomerId",
                table: "Devices");

            migrationBuilder.DropForeignKey(
                name: "FK_RepairJobs_Customers_CustomerId",
                table: "RepairJobs");

            migrationBuilder.DropForeignKey(
                name: "FK_RepairJobs_Devices_DeviceId",
                table: "RepairJobs");

            migrationBuilder.AddForeignKey(
                name: "FK_Devices_Customers_CustomerId",
                table: "Devices",
                column: "CustomerId",
                principalTable: "Customers",
                principalColumn: "CustomerId",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_RepairJobs_Customers_CustomerId",
                table: "RepairJobs",
                column: "CustomerId",
                principalTable: "Customers",
                principalColumn: "CustomerId",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_RepairJobs_Devices_DeviceId",
                table: "RepairJobs",
                column: "DeviceId",
                principalTable: "Devices",
                principalColumn: "DeviceId",
                onDelete: ReferentialAction.Restrict);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Devices_Customers_CustomerId",
                table: "Devices");

            migrationBuilder.DropForeignKey(
                name: "FK_RepairJobs_Customers_CustomerId",
                table: "RepairJobs");

            migrationBuilder.DropForeignKey(
                name: "FK_RepairJobs_Devices_DeviceId",
                table: "RepairJobs");

            migrationBuilder.AddForeignKey(
                name: "FK_Devices_Customers_CustomerId",
                table: "Devices",
                column: "CustomerId",
                principalTable: "Customers",
                principalColumn: "CustomerId",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_RepairJobs_Customers_CustomerId",
                table: "RepairJobs",
                column: "CustomerId",
                principalTable: "Customers",
                principalColumn: "CustomerId",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_RepairJobs_Devices_DeviceId",
                table: "RepairJobs",
                column: "DeviceId",
                principalTable: "Devices",
                principalColumn: "DeviceId",
                onDelete: ReferentialAction.Cascade);
        }
    }
}
