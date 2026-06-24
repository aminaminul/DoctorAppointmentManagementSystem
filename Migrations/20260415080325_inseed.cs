using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

#pragma warning disable CA1814 // Prefer jagged arrays over multidimensional

namespace DoctorAppointmentManagementSystem.Migrations
{
    /// <inheritdoc />
    public partial class inseed : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.InsertData(
                table: "Users",
                columns: new[] { "Id", "Email", "Name", "Password", "Role" },
                values: new object[,]
                {
                    { 1, "admin@gmail.com", "Admin User", "1234", "Admin" },
                    { 2, "patient@gmail.com", "Patient User", "1234", "Patient" },
                    { 3, "doctor@gmail.com", "Doctor User", "1234", "Doctor" }
                });

            migrationBuilder.InsertData(
                table: "Doctors",
                columns: new[] { "Id", "Availability", "Specialization", "UserId" },
                values: new object[] { 1, "10AM-5PM", "Cardiologist", 3 });

            migrationBuilder.InsertData(
                table: "Patients",
                columns: new[] { "Id", "Age", "Gender", "UserId" },
                values: new object[] { 1, 25, "Male", 2 });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DeleteData(
                table: "Doctors",
                keyColumn: "Id",
                keyValue: 1);

            migrationBuilder.DeleteData(
                table: "Patients",
                keyColumn: "Id",
                keyValue: 1);

            migrationBuilder.DeleteData(
                table: "Users",
                keyColumn: "Id",
                keyValue: 1);

            migrationBuilder.DeleteData(
                table: "Users",
                keyColumn: "Id",
                keyValue: 2);

            migrationBuilder.DeleteData(
                table: "Users",
                keyColumn: "Id",
                keyValue: 3);
        }
    }
}
