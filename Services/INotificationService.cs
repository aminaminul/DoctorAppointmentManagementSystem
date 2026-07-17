using DoctorAppointmentManagementSystem.Models;

namespace DoctorAppointmentManagementSystem.Services
{
    public interface INotificationService
    {
        Task SendAppointmentConfirmationAsync(Appointment appointment);

        Task SendAppointmentApprovedAsync(Appointment appointment);

        Task SendAppointmentCancelledAsync(Appointment appointment);

        Task SendAppointmentDelayedAsync(Appointment appointment);

        Task SendPrescriptionReadyAsync(Prescription prescription);

        Task SendAppointmentReminderAsync(Appointment appointment);
    }
}
