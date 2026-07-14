using System;

namespace DoctorAppointmentManagementSystem.ViewModels
{
    public class DashboardViewModel
    {
        public int TotalDoctors { get; set; }
        public int TotalPatients { get; set; }
        public int TotalAppointments { get; set; }
        public int TodaysAppointments { get; set; }
        public int PendingRequests { get; set; }
        public int CompletedAppointments { get; set; }
        // For simple SVG chart (last 7 days)
        public int[] AppointmentsLast7Days { get; set; } = new int[0];
        public string[] LabelsLast7Days { get; set; } = new string[0];
    }
}
