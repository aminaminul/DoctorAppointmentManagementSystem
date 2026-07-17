using System;
using System.Collections.Generic;
using DoctorAppointmentManagementSystem.Models;

namespace DoctorAppointmentManagementSystem.ViewModels
{
    public class RecentAppointmentItem
    {
        public int Id { get; set; }
        public string PatientName { get; set; } = "";
        public string DoctorName { get; set; } = "";
        public string Specialization { get; set; } = "";
        public DateTime AppointmentDate { get; set; }
        public string AppointmentTime { get; set; } = "";
        public string Status { get; set; } = "";
        public bool IsEmergency { get; set; }
    }

    public class DashboardViewModel
    {
        public int TotalDoctors { get; set; }
        public int TotalPatients { get; set; }
        public int TotalAppointments { get; set; }
        public int TodaysAppointments { get; set; }
        public int PendingRequests { get; set; }
        public int CompletedAppointments { get; set; }
        public int CancelledAppointments { get; set; }
        public int ConfirmedAppointments { get; set; }

        public decimal TotalRevenue { get; set; }
        public decimal TodaysRevenue { get; set; }
        public decimal MonthlyRevenue { get; set; }

        public int ActiveDoctors { get; set; }
        public int ActivePatients { get; set; }
        public int EmergencyCases { get; set; }
        public int NewPatientsThisMonth { get; set; }
        public int NewDoctorsThisMonth { get; set; }
        public int PendingLeaveRequests { get; set; }
        public int OpenComplaints { get; set; }

        public int[] AppointmentsLast7Days { get; set; } = new int[0];
        public string[] LabelsLast7Days { get; set; } = new string[0];

        public int[] MonthlyAppointments { get; set; } = new int[6];
        public string[] MonthlyLabels { get; set; } = new string[6];
        public decimal[] MonthlyRevenueData { get; set; } = new decimal[6];

        public int[] StatusCounts { get; set; } = new int[4]; // Pending, Confirmed, Completed, Cancelled

        public List<RecentAppointmentItem> RecentAppointments { get; set; } = new();

        public List<(string Specialization, int Count)> TopSpecializations { get; set; } = new();
    }
}
