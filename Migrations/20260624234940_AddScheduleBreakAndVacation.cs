using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace DoctorAppointmentManagementSystem.Migrations
{
    /// <inheritdoc />
    public partial class AddScheduleBreakAndVacation : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "BreakEndTime",
                table: "DoctorSchedules",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "BreakStartTime",
                table: "DoctorSchedules",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<bool>(
                name: "IsVacation",
                table: "DoctorSchedules",
                type: "bit",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<string>(
                name: "Notes",
                table: "DoctorSchedules",
                type: "nvarchar(max)",
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "BreakEndTime",
                table: "DoctorSchedules");

            migrationBuilder.DropColumn(
                name: "BreakStartTime",
                table: "DoctorSchedules");

            migrationBuilder.DropColumn(
                name: "IsVacation",
                table: "DoctorSchedules");

            migrationBuilder.DropColumn(
                name: "Notes",
                table: "DoctorSchedules");
        }
    }
}
