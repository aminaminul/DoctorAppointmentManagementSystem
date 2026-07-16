using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

#pragma warning disable CA1814 // Prefer jagged arrays over multidimensional

namespace DoctorAppointmentManagementSystem.Migrations
{
    /// <inheritdoc />
    public partial class M24 : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
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
                table: "Roles",
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

            migrationBuilder.DeleteData(
                table: "Roles",
                keyColumn: "Id",
                keyValue: 2);

            migrationBuilder.DeleteData(
                table: "Roles",
                keyColumn: "Id",
                keyValue: 3);

            migrationBuilder.DropColumn(
                name: "ProfileImage",
                table: "Doctors");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "ProfileImage",
                table: "Doctors",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.InsertData(
                table: "Roles",
                columns: new[] { "Id", "Description", "Name" },
                values: new object[,]
                {
                    { 1, "Administrator role", "Admin" },
                    { 2, "Doctor role", "Doctor" },
                    { 3, "Patient role", "Patient" }
                });

            migrationBuilder.InsertData(
                table: "Users",
                columns: new[] { "Id", "AccountCreationDateTime", "ActiveStatus", "Email", "Password", "PhoneNumber", "RoleId", "Username" },
                values: new object[,]
                {
                    { 1, new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), true, "admin@gmail.com", "1234", "01700000001", 1, "Admin User" },
                    { 2, new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), true, "patient@gmail.com", "1234", "01700000002", 3, "Patient User" },
                    { 3, new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), true, "doctor@gmail.com", "1234", "01700000003", 2, "Doctor User" }
                });

            migrationBuilder.InsertData(
                table: "Doctors",
                columns: new[] { "Id", "ActiveStatus", "AvailableDays", "AvailableTime", "ConsultationFee", "Experience", "ProfileImage", "Qualification", "Specialization", "UserId" },
                values: new object[] { 1, true, "Saturday, Sunday, Monday, Tuesday, Wednesday, Thursday", "10AM-5PM", 800m, 8, "doctor_default.png", "MBBS, FCPS", "Cardiologist", 3 });

            migrationBuilder.InsertData(
                table: "Patients",
                columns: new[] { "Id", "ActiveStatus", "Address", "Allergies", "BloodGroup", "DateOfBirth", "EmergencyContact", "Gender", "MedicalHistory", "UserId" },
                values: new object[] { 1, true, "Dhaka, Bangladesh", "Dust and pollen allergy", "O+", new DateTime(2001, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), "01900000002", "Male", "No major past medical illnesses. Regular checkups.", 2 });
        }
    }
}
