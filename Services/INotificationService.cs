using DoctorAppointmentManagementSystem.Models;

namespace DoctorAppointmentManagementSystem.Services
{
    public interface INotificationService
    {
        /// <summary>Patient books an appointment — sends "Booking Received" notification.</summary>
        Task SendAppointmentConfirmationAsync(Appointment appointment);

        /// <summary>Doctor approves/confirms an appointment.</summary>
        Task SendAppointmentApprovedAsync(Appointment appointment);

        /// <summary>Doctor rejects/cancels an appointment.</summary>
        Task SendAppointmentCancelledAsync(Appointment appointment);

        /// <summary>Doctor marks appointment as Delayed.</summary>
        Task SendAppointmentDelayedAsync(Appointment appointment);

        /// <summary>Doctor writes a prescription — patient is notified it's available.</summary>
        Task SendPrescriptionReadyAsync(Prescription prescription);

        /// <summary>24-hour reminder before the appointment date.</summary>
        Task SendAppointmentReminderAsync(Appointment appointment);
    }
}
