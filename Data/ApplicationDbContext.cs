using DoctorAppointmentManagementSystem.Models;
using Microsoft.EntityFrameworkCore;


public class ApplicationDbContext : DbContext
{
    public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options)
        : base(options)
    {
    }

    // Tables
    public DbSet<User> Users { get; set; }
    public DbSet<Patient> Patients { get; set; }
    public DbSet<Doctor> Doctors { get; set; }
    public DbSet<Appointment> Appointments { get; set; }
    public DbSet<Prescription> Prescriptions { get; set; }
    public DbSet<Notification> Notifications { get; set; }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {

        base.OnModelCreating(modelBuilder);

        modelBuilder.Entity<User>().HasData(
            new User
            {
                Id = 1,
                Name = "Admin User",
                Email = "admin@gmail.com",
                Password = "1234",
                Role = "Admin"
            },
            new User
            {
                Id = 2,
                Name = "Patient User",
                Email = "patient@gmail.com",
                Password = "1234",
                Role = "Patient"
            },
            new User
            {
                Id = 3,
                Name = "Doctor User",
                Email = "doctor@gmail.com",
                Password = "1234",
                Role = "Doctor"
            }
        );

        modelBuilder.Entity<Patient>().HasData(
            new Patient
            {
                Id = 1,
                UserId = 2,
                Age = 25,
                Gender = "Male"
            }
        );

        modelBuilder.Entity<Doctor>().HasData(
            new Doctor
            {
                Id = 1,
                UserId = 3,
                Specialization = "Cardiologist",
                Availability = "10AM-5PM"
            }
        );

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

        // Notification → User
        modelBuilder.Entity<Notification>()
            .HasOne(n => n.User)
            .WithMany()
            .HasForeignKey(n => n.UserId)
            .OnDelete(DeleteBehavior.NoAction);
    }
}