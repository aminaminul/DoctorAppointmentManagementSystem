using DoctorAppointmentManagementSystem.Models;
using Microsoft.EntityFrameworkCore;
using System;

public class ApplicationDbContext : DbContext
{
    public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options)
        : base(options)
    {
    }

    // Tables
    public DbSet<Role> Roles { get; set; }
    public DbSet<User> Users { get; set; }
    public DbSet<Patient> Patients { get; set; }
    public DbSet<Doctor> Doctors { get; set; }
    public DbSet<Appointment> Appointments { get; set; }
    public DbSet<Prescription> Prescriptions { get; set; }
    public DbSet<DoctorSchedule> DoctorSchedules { get; set; }
    public DbSet<MedicalRecord> MedicalRecords { get; set; }
    public DbSet<Payment> Payments { get; set; }
    public DbSet<Notification> Notifications { get; set; }
    public DbSet<Feedback> Feedbacks { get; set; }
    public DbSet<AdminLog> AdminLogs { get; set; }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        // Configure decimal precision
        modelBuilder.Entity<Doctor>()
            .Property(d => d.ConsultationFee)
            .HasPrecision(18, 2);

        modelBuilder.Entity<Payment>()
            .Property(p => p.Amount)
            .HasPrecision(18, 2);

        // Seed Roles
        modelBuilder.Entity<Role>().HasData(
            new Role { Id = 1, RoleName = "Admin", Description = "Administrator role" },
            new Role { Id = 2, RoleName = "Doctor", Description = "Doctor role" },
            new Role { Id = 3, RoleName = "Patient", Description = "Patient role" }
        );

        // Seed Users
        modelBuilder.Entity<User>().HasData(
            new User
            {
                Id = 1,
                FullName = "Admin User",
                Email = "admin@gmail.com",
                Password = "1234",
                PhoneNumber = "01700000001",
                AccountCreationDateTime = new DateTime(2026, 1, 1),
                ActiveStatus = true,
                RoleId = 1
            },
            new User
            {
                Id = 2,
                FullName = "Patient User",
                Email = "patient@gmail.com",
                Password = "1234",
                PhoneNumber = "01700000002",
                AccountCreationDateTime = new DateTime(2026, 1, 1),
                ActiveStatus = true,
                RoleId = 3
            },
            new User
            {
                Id = 3,
                FullName = "Doctor User",
                Email = "doctor@gmail.com",
                Password = "1234",
                PhoneNumber = "01700000003",
                AccountCreationDateTime = new DateTime(2026, 1, 1),
                ActiveStatus = true,
                RoleId = 2
            }
        );

        // Seed Patients
        modelBuilder.Entity<Patient>().HasData(
            new Patient
            {
                Id = 1,
                UserId = 2,
                Gender = "Male",
                DateOfBirth = new DateTime(2001, 1, 1),
                BloodGroup = "O+",
                Address = "Dhaka, Bangladesh",
                EmergencyContact = "01900000002",
                MedicalHistory = "No major past medical illnesses. Regular checkups.",
                Allergies = "Dust and pollen allergy",
                ActiveStatus = true
            }
        );

        // Seed Doctors
        modelBuilder.Entity<Doctor>().HasData(
            new Doctor
            {
                Id = 1,
                UserId = 3,
                Specialization = "Cardiologist",
                Qualification = "MBBS, FCPS",
                Experience = 8,
                ConsultationFee = 800,
                AvailableDays = "Saturday, Sunday, Monday, Tuesday, Wednesday, Thursday",
                AvailableTime = "10AM-5PM",
                ProfileImage = "doctor_default.png",
                ActiveStatus = true
            }
        );

        // Define Foreign Keys and disable cascading deletes to prevent cycles

        // User → Role
        modelBuilder.Entity<User>()
            .HasOne(u => u.Role)
            .WithMany()
            .HasForeignKey(u => u.RoleId)
            .OnDelete(DeleteBehavior.NoAction);

        // Doctor → User
        modelBuilder.Entity<Doctor>()
            .HasOne(d => d.User)
            .WithMany()
            .HasForeignKey(d => d.UserId)
            .OnDelete(DeleteBehavior.NoAction);

        // Patient → User
        modelBuilder.Entity<Patient>()
            .HasOne(p => p.User)
            .WithMany()
            .HasForeignKey(p => p.UserId)
            .OnDelete(DeleteBehavior.NoAction);

        // Appointment → Doctor
        modelBuilder.Entity<Appointment>()
            .HasOne(a => a.Doctor)
            .WithMany()
            .HasForeignKey(a => a.DoctorId)
            .OnDelete(DeleteBehavior.NoAction);

        // Appointment → Patient
        modelBuilder.Entity<Appointment>()
            .HasOne(a => a.Patient)
            .WithMany()
            .HasForeignKey(a => a.PatientId)
            .OnDelete(DeleteBehavior.NoAction);

        // Prescription → Appointment
        modelBuilder.Entity<Prescription>()
            .HasOne(p => p.Appointment)
            .WithMany()
            .HasForeignKey(p => p.AppointmentId)
            .OnDelete(DeleteBehavior.NoAction);

        // Prescription → Doctor
        modelBuilder.Entity<Prescription>()
            .HasOne(p => p.Doctor)
            .WithMany()
            .HasForeignKey(p => p.DoctorId)
            .OnDelete(DeleteBehavior.NoAction);

        // Prescription → Patient
        modelBuilder.Entity<Prescription>()
            .HasOne(p => p.Patient)
            .WithMany()
            .HasForeignKey(p => p.PatientId)
            .OnDelete(DeleteBehavior.NoAction);

        // DoctorSchedule → Doctor
        modelBuilder.Entity<DoctorSchedule>()
            .HasOne(ds => ds.Doctor)
            .WithMany()
            .HasForeignKey(ds => ds.DoctorId)
            .OnDelete(DeleteBehavior.NoAction);

        // MedicalRecord → Patient
        modelBuilder.Entity<MedicalRecord>()
            .HasOne(mr => mr.Patient)
            .WithMany()
            .HasForeignKey(mr => mr.PatientId)
            .OnDelete(DeleteBehavior.NoAction);

        // MedicalRecord → Doctor
        modelBuilder.Entity<MedicalRecord>()
            .HasOne(mr => mr.Doctor)
            .WithMany()
            .HasForeignKey(mr => mr.DoctorId)
            .OnDelete(DeleteBehavior.NoAction);

        // Payment → Appointment
        modelBuilder.Entity<Payment>()
            .HasOne(p => p.Appointment)
            .WithMany()
            .HasForeignKey(p => p.AppointmentId)
            .OnDelete(DeleteBehavior.NoAction);

        // Payment → Patient
        modelBuilder.Entity<Payment>()
            .HasOne(p => p.Patient)
            .WithMany()
            .HasForeignKey(p => p.PatientId)
            .OnDelete(DeleteBehavior.NoAction);

        // Notification → User
        modelBuilder.Entity<Notification>()
            .HasOne(n => n.User)
            .WithMany()
            .HasForeignKey(n => n.UserId)
            .OnDelete(DeleteBehavior.NoAction);

        // Feedback → Patient
        modelBuilder.Entity<Feedback>()
            .HasOne(f => f.Patient)
            .WithMany()
            .HasForeignKey(f => f.PatientId)
            .OnDelete(DeleteBehavior.NoAction);

        // Feedback → Doctor
        modelBuilder.Entity<Feedback>()
            .HasOne(f => f.Doctor)
            .WithMany()
            .HasForeignKey(f => f.DoctorId)
            .OnDelete(DeleteBehavior.NoAction);

        // AdminLog → User (Admin)
        modelBuilder.Entity<AdminLog>()
            .HasOne(al => al.Admin)
            .WithMany()
            .HasForeignKey(al => al.AdminId)
            .OnDelete(DeleteBehavior.NoAction);
    }
}