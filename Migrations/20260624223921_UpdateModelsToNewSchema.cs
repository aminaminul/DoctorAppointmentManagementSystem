using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

#pragma warning disable CA1814 // Prefer jagged arrays over multidimensional

namespace DoctorAppointmentManagementSystem.Migrations
{
    /// <inheritdoc />
    public partial class UpdateModelsToNewSchema : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Prescriptions_Appointments_AppointmentId",
                table: "Prescriptions");

            migrationBuilder.DropColumn(
                name: "Name",
                table: "Users");

            migrationBuilder.DropColumn(
                name: "Age",
                table: "Patients");

            migrationBuilder.DropColumn(
                name: "Availability",
                table: "Doctors");

            migrationBuilder.RenameColumn(
                name: "Role",
                table: "Users",
                newName: "FullName");

            migrationBuilder.RenameColumn(
                name: "CreatedDate",
                table: "Prescriptions",
                newName: "PrescriptionDateTime");

            migrationBuilder.RenameColumn(
                name: "Advice",
                table: "Prescriptions",
                newName: "Status");

            migrationBuilder.RenameColumn(
                name: "Date",
                table: "Notifications",
                newName: "SentDateTime");

            migrationBuilder.RenameColumn(
                name: "TimeSlot",
                table: "Appointments",
                newName: "AppointmentTime");

            migrationBuilder.RenameColumn(
                name: "Status",
                table: "Appointments",
                newName: "AppointmentStatus");

            migrationBuilder.RenameColumn(
                name: "Date",
                table: "Appointments",
                newName: "BookingDateTime");

            migrationBuilder.AddColumn<DateTime>(
                name: "AccountCreationDateTime",
                table: "Users",
                type: "datetime2",
                nullable: false,
                defaultValue: new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified));

            migrationBuilder.AddColumn<bool>(
                name: "ActiveStatus",
                table: "Users",
                type: "bit",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<string>(
                name: "PhoneNumber",
                table: "Users",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "RoleId",
                table: "Users",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<string>(
                name: "Diagnosis",
                table: "Prescriptions",
                type: "nvarchar(max)",
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<int>(
                name: "DoctorId",
                table: "Prescriptions",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<string>(
                name: "Instructions",
                table: "Prescriptions",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "PatientId",
                table: "Prescriptions",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<bool>(
                name: "ActiveStatus",
                table: "Patients",
                type: "bit",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<string>(
                name: "Address",
                table: "Patients",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "BloodGroup",
                table: "Patients",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "DateOfBirth",
                table: "Patients",
                type: "datetime2",
                nullable: false,
                defaultValue: new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified));

            migrationBuilder.AddColumn<string>(
                name: "EmergencyContact",
                table: "Patients",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "NotificationStatus",
                table: "Notifications",
                type: "nvarchar(max)",
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "NotificationType",
                table: "Notifications",
                type: "nvarchar(max)",
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "Title",
                table: "Notifications",
                type: "nvarchar(max)",
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<bool>(
                name: "ActiveStatus",
                table: "Doctors",
                type: "bit",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<string>(
                name: "AvailableDays",
                table: "Doctors",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "AvailableTime",
                table: "Doctors",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<decimal>(
                name: "ConsultationFee",
                table: "Doctors",
                type: "decimal(18,2)",
                precision: 18,
                scale: 2,
                nullable: false,
                defaultValue: 0m);

            migrationBuilder.AddColumn<int>(
                name: "Experience",
                table: "Doctors",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<string>(
                name: "ProfileImage",
                table: "Doctors",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "Qualification",
                table: "Doctors",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "AppointmentDate",
                table: "Appointments",
                type: "datetime2",
                nullable: false,
                defaultValue: new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified));

            migrationBuilder.AddColumn<string>(
                name: "ReasonForVisit",
                table: "Appointments",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.CreateTable(
                name: "AdminLogs",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    AdminId = table.Column<int>(type: "int", nullable: false),
                    ActionPerformed = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Description = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    ActionDateTime = table.Column<DateTime>(type: "datetime2", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_AdminLogs", x => x.Id);
                    table.ForeignKey(
                        name: "FK_AdminLogs_Users_AdminId",
                        column: x => x.AdminId,
                        principalTable: "Users",
                        principalColumn: "Id");
                });

            migrationBuilder.CreateTable(
                name: "DoctorSchedules",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    DoctorId = table.Column<int>(type: "int", nullable: false),
                    AvailableDate = table.Column<DateTime>(type: "datetime2", nullable: false),
                    StartTime = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    EndTime = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    SlotStatus = table.Column<string>(type: "nvarchar(max)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_DoctorSchedules", x => x.Id);
                    table.ForeignKey(
                        name: "FK_DoctorSchedules_Doctors_DoctorId",
                        column: x => x.DoctorId,
                        principalTable: "Doctors",
                        principalColumn: "Id");
                });

            migrationBuilder.CreateTable(
                name: "Feedbacks",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    PatientId = table.Column<int>(type: "int", nullable: false),
                    DoctorId = table.Column<int>(type: "int", nullable: false),
                    Rating = table.Column<int>(type: "int", nullable: false),
                    FeedbackDateTime = table.Column<DateTime>(type: "datetime2", nullable: false),
                    Status = table.Column<string>(type: "nvarchar(max)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Feedbacks", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Feedbacks_Doctors_DoctorId",
                        column: x => x.DoctorId,
                        principalTable: "Doctors",
                        principalColumn: "Id");
                    table.ForeignKey(
                        name: "FK_Feedbacks_Patients_PatientId",
                        column: x => x.PatientId,
                        principalTable: "Patients",
                        principalColumn: "Id");
                });

            migrationBuilder.CreateTable(
                name: "MedicalRecords",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    PatientId = table.Column<int>(type: "int", nullable: false),
                    DoctorId = table.Column<int>(type: "int", nullable: false),
                    Diagnosis = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    TreatmentDetails = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    Notes = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    RecordDate = table.Column<DateTime>(type: "datetime2", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_MedicalRecords", x => x.Id);
                    table.ForeignKey(
                        name: "FK_MedicalRecords_Doctors_DoctorId",
                        column: x => x.DoctorId,
                        principalTable: "Doctors",
                        principalColumn: "Id");
                    table.ForeignKey(
                        name: "FK_MedicalRecords_Patients_PatientId",
                        column: x => x.PatientId,
                        principalTable: "Patients",
                        principalColumn: "Id");
                });

            migrationBuilder.CreateTable(
                name: "Payments",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    AppointmentId = table.Column<int>(type: "int", nullable: false),
                    PatientId = table.Column<int>(type: "int", nullable: false),
                    Amount = table.Column<decimal>(type: "decimal(18,2)", precision: 18, scale: 2, nullable: false),
                    PaymentMethod = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    TransactionId = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    PaymentDateTime = table.Column<DateTime>(type: "datetime2", nullable: false),
                    PaymentStatus = table.Column<string>(type: "nvarchar(max)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Payments", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Payments_Appointments_AppointmentId",
                        column: x => x.AppointmentId,
                        principalTable: "Appointments",
                        principalColumn: "Id");
                    table.ForeignKey(
                        name: "FK_Payments_Patients_PatientId",
                        column: x => x.PatientId,
                        principalTable: "Patients",
                        principalColumn: "Id");
                });

            migrationBuilder.CreateTable(
                name: "Roles",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    RoleName = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Description = table.Column<string>(type: "nvarchar(max)", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Roles", x => x.Id);
                });

            migrationBuilder.UpdateData(
                table: "Doctors",
                keyColumn: "Id",
                keyValue: 1,
                columns: new[] { "ActiveStatus", "AvailableDays", "AvailableTime", "ConsultationFee", "Experience", "ProfileImage", "Qualification" },
                values: new object[] { true, "Saturday, Sunday, Monday, Tuesday, Wednesday, Thursday", "10AM-5PM", 800m, 8, "doctor_default.png", "MBBS, FCPS" });

            migrationBuilder.UpdateData(
                table: "Patients",
                keyColumn: "Id",
                keyValue: 1,
                columns: new[] { "ActiveStatus", "Address", "BloodGroup", "DateOfBirth", "EmergencyContact" },
                values: new object[] { true, "Dhaka, Bangladesh", "O+", new DateTime(2001, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), "01900000002" });

            migrationBuilder.InsertData(
                table: "Roles",
                columns: new[] { "Id", "Description", "RoleName" },
                values: new object[,]
                {
                    { 1, "Administrator role", "Admin" },
                    { 2, "Doctor role", "Doctor" },
                    { 3, "Patient role", "Patient" }
                });

            migrationBuilder.UpdateData(
                table: "Users",
                keyColumn: "Id",
                keyValue: 1,
                columns: new[] { "AccountCreationDateTime", "ActiveStatus", "FullName", "PhoneNumber", "RoleId" },
                values: new object[] { new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), true, "Admin User", "01700000001", 1 });

            migrationBuilder.UpdateData(
                table: "Users",
                keyColumn: "Id",
                keyValue: 2,
                columns: new[] { "AccountCreationDateTime", "ActiveStatus", "FullName", "PhoneNumber", "RoleId" },
                values: new object[] { new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), true, "Patient User", "01700000002", 3 });

            migrationBuilder.UpdateData(
                table: "Users",
                keyColumn: "Id",
                keyValue: 3,
                columns: new[] { "AccountCreationDateTime", "ActiveStatus", "FullName", "PhoneNumber", "RoleId" },
                values: new object[] { new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), true, "Doctor User", "01700000003", 2 });

            migrationBuilder.CreateIndex(
                name: "IX_Users_RoleId",
                table: "Users",
                column: "RoleId");

            migrationBuilder.CreateIndex(
                name: "IX_Prescriptions_DoctorId",
                table: "Prescriptions",
                column: "DoctorId");

            migrationBuilder.CreateIndex(
                name: "IX_Prescriptions_PatientId",
                table: "Prescriptions",
                column: "PatientId");

            migrationBuilder.CreateIndex(
                name: "IX_AdminLogs_AdminId",
                table: "AdminLogs",
                column: "AdminId");

            migrationBuilder.CreateIndex(
                name: "IX_DoctorSchedules_DoctorId",
                table: "DoctorSchedules",
                column: "DoctorId");

            migrationBuilder.CreateIndex(
                name: "IX_Feedbacks_DoctorId",
                table: "Feedbacks",
                column: "DoctorId");

            migrationBuilder.CreateIndex(
                name: "IX_Feedbacks_PatientId",
                table: "Feedbacks",
                column: "PatientId");

            migrationBuilder.CreateIndex(
                name: "IX_MedicalRecords_DoctorId",
                table: "MedicalRecords",
                column: "DoctorId");

            migrationBuilder.CreateIndex(
                name: "IX_MedicalRecords_PatientId",
                table: "MedicalRecords",
                column: "PatientId");

            migrationBuilder.CreateIndex(
                name: "IX_Payments_AppointmentId",
                table: "Payments",
                column: "AppointmentId");

            migrationBuilder.CreateIndex(
                name: "IX_Payments_PatientId",
                table: "Payments",
                column: "PatientId");

            migrationBuilder.AddForeignKey(
                name: "FK_Prescriptions_Appointments_AppointmentId",
                table: "Prescriptions",
                column: "AppointmentId",
                principalTable: "Appointments",
                principalColumn: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_Prescriptions_Doctors_DoctorId",
                table: "Prescriptions",
                column: "DoctorId",
                principalTable: "Doctors",
                principalColumn: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_Prescriptions_Patients_PatientId",
                table: "Prescriptions",
                column: "PatientId",
                principalTable: "Patients",
                principalColumn: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_Users_Roles_RoleId",
                table: "Users",
                column: "RoleId",
                principalTable: "Roles",
                principalColumn: "Id");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Prescriptions_Appointments_AppointmentId",
                table: "Prescriptions");

            migrationBuilder.DropForeignKey(
                name: "FK_Prescriptions_Doctors_DoctorId",
                table: "Prescriptions");

            migrationBuilder.DropForeignKey(
                name: "FK_Prescriptions_Patients_PatientId",
                table: "Prescriptions");

            migrationBuilder.DropForeignKey(
                name: "FK_Users_Roles_RoleId",
                table: "Users");

            migrationBuilder.DropTable(
                name: "AdminLogs");

            migrationBuilder.DropTable(
                name: "DoctorSchedules");

            migrationBuilder.DropTable(
                name: "Feedbacks");

            migrationBuilder.DropTable(
                name: "MedicalRecords");

            migrationBuilder.DropTable(
                name: "Payments");

            migrationBuilder.DropTable(
                name: "Roles");

            migrationBuilder.DropIndex(
                name: "IX_Users_RoleId",
                table: "Users");

            migrationBuilder.DropIndex(
                name: "IX_Prescriptions_DoctorId",
                table: "Prescriptions");

            migrationBuilder.DropIndex(
                name: "IX_Prescriptions_PatientId",
                table: "Prescriptions");

            migrationBuilder.DropColumn(
                name: "AccountCreationDateTime",
                table: "Users");

            migrationBuilder.DropColumn(
                name: "ActiveStatus",
                table: "Users");

            migrationBuilder.DropColumn(
                name: "PhoneNumber",
                table: "Users");

            migrationBuilder.DropColumn(
                name: "RoleId",
                table: "Users");

            migrationBuilder.DropColumn(
                name: "Diagnosis",
                table: "Prescriptions");

            migrationBuilder.DropColumn(
                name: "DoctorId",
                table: "Prescriptions");

            migrationBuilder.DropColumn(
                name: "Instructions",
                table: "Prescriptions");

            migrationBuilder.DropColumn(
                name: "PatientId",
                table: "Prescriptions");

            migrationBuilder.DropColumn(
                name: "ActiveStatus",
                table: "Patients");

            migrationBuilder.DropColumn(
                name: "Address",
                table: "Patients");

            migrationBuilder.DropColumn(
                name: "BloodGroup",
                table: "Patients");

            migrationBuilder.DropColumn(
                name: "DateOfBirth",
                table: "Patients");

            migrationBuilder.DropColumn(
                name: "EmergencyContact",
                table: "Patients");

            migrationBuilder.DropColumn(
                name: "NotificationStatus",
                table: "Notifications");

            migrationBuilder.DropColumn(
                name: "NotificationType",
                table: "Notifications");

            migrationBuilder.DropColumn(
                name: "Title",
                table: "Notifications");

            migrationBuilder.DropColumn(
                name: "ActiveStatus",
                table: "Doctors");

            migrationBuilder.DropColumn(
                name: "AvailableDays",
                table: "Doctors");

            migrationBuilder.DropColumn(
                name: "AvailableTime",
                table: "Doctors");

            migrationBuilder.DropColumn(
                name: "ConsultationFee",
                table: "Doctors");

            migrationBuilder.DropColumn(
                name: "Experience",
                table: "Doctors");

            migrationBuilder.DropColumn(
                name: "ProfileImage",
                table: "Doctors");

            migrationBuilder.DropColumn(
                name: "Qualification",
                table: "Doctors");

            migrationBuilder.DropColumn(
                name: "AppointmentDate",
                table: "Appointments");

            migrationBuilder.DropColumn(
                name: "ReasonForVisit",
                table: "Appointments");

            migrationBuilder.RenameColumn(
                name: "FullName",
                table: "Users",
                newName: "Role");

            migrationBuilder.RenameColumn(
                name: "Status",
                table: "Prescriptions",
                newName: "Advice");

            migrationBuilder.RenameColumn(
                name: "PrescriptionDateTime",
                table: "Prescriptions",
                newName: "CreatedDate");

            migrationBuilder.RenameColumn(
                name: "SentDateTime",
                table: "Notifications",
                newName: "Date");

            migrationBuilder.RenameColumn(
                name: "BookingDateTime",
                table: "Appointments",
                newName: "Date");

            migrationBuilder.RenameColumn(
                name: "AppointmentTime",
                table: "Appointments",
                newName: "TimeSlot");

            migrationBuilder.RenameColumn(
                name: "AppointmentStatus",
                table: "Appointments",
                newName: "Status");

            migrationBuilder.AddColumn<string>(
                name: "Name",
                table: "Users",
                type: "nvarchar(max)",
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<int>(
                name: "Age",
                table: "Patients",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<string>(
                name: "Availability",
                table: "Doctors",
                type: "nvarchar(max)",
                nullable: false,
                defaultValue: "");

            migrationBuilder.UpdateData(
                table: "Doctors",
                keyColumn: "Id",
                keyValue: 1,
                column: "Availability",
                value: "10AM-5PM");

            migrationBuilder.UpdateData(
                table: "Patients",
                keyColumn: "Id",
                keyValue: 1,
                column: "Age",
                value: 25);

            migrationBuilder.UpdateData(
                table: "Users",
                keyColumn: "Id",
                keyValue: 1,
                columns: new[] { "Name", "Role" },
                values: new object[] { "Admin User", "Admin" });

            migrationBuilder.UpdateData(
                table: "Users",
                keyColumn: "Id",
                keyValue: 2,
                columns: new[] { "Name", "Role" },
                values: new object[] { "Patient User", "Patient" });

            migrationBuilder.UpdateData(
                table: "Users",
                keyColumn: "Id",
                keyValue: 3,
                columns: new[] { "Name", "Role" },
                values: new object[] { "Doctor User", "Doctor" });

            migrationBuilder.AddForeignKey(
                name: "FK_Prescriptions_Appointments_AppointmentId",
                table: "Prescriptions",
                column: "AppointmentId",
                principalTable: "Appointments",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }
    }
}
