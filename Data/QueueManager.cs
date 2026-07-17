using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.EntityFrameworkCore;
using DoctorAppointmentManagementSystem.Models;

namespace DoctorAppointmentManagementSystem.Data
{
    public static class QueueManager
    {
        public static void EnsureQueueGenerated(ApplicationDbContext context, int doctorId, DateTime date)
        {
            var schedule = context.DoctorSchedules
                .FirstOrDefault(ds => ds.DoctorId == doctorId && ds.AvailableDate.Date == date.Date);

            if (schedule != null && schedule.IsVacation)
            {
                return;
            }

            var appointments = context.Appointments
                .Where(a => a.DoctorId == doctorId 
                         && a.AppointmentDate.Date == date.Date 
                         && (a.AppointmentStatus == "Confirmed" || a.AppointmentStatus == "Approved"))
                .ToList();

            if (!appointments.Any()) return;

            var existingEntries = context.QueueEntries
                .Include(q => q.Appointment)
                .Where(q => q.Appointment.DoctorId == doctorId 
                         && q.Appointment.AppointmentDate.Date == date.Date)
                .ToList();

            var sortedAppointments = appointments
                .OrderBy(a => a.IsEmergency ? 0 : 1)
                .ThenBy(a => ParseTimeSlot(a.AppointmentTime))
                .ToList();

            int tokenSeq = 1;
            int activeSeq = 1;

            var nonWaiting = existingEntries
                .Where(q => q.Status != "Waiting")
                .ToList();

            if (nonWaiting.Any())
            {
                activeSeq = nonWaiting.Max(q => q.SequenceNumber) + 1;
            }

            foreach (var appt in sortedAppointments)
            {
                var existing = existingEntries.FirstOrDefault(q => q.AppointmentId == appt.Id);
                if (existing == null)
                {
                    int tokenNumber = tokenSeq++;
                    while (existingEntries.Any(q => q.TokenNumber == tokenNumber))
                    {
                        tokenNumber = tokenSeq++;
                    }

                    var newEntry = new QueueEntry
                    {
                        AppointmentId = appt.Id,
                        TokenNumber = tokenNumber,
                        SequenceNumber = activeSeq++,
                        Status = "Waiting",
                        CreatedAt = DateTime.Now
                    };

                    context.QueueEntries.Add(newEntry);
                    existingEntries.Add(newEntry);
                }
                else
                {
                    if (existing.TokenNumber >= tokenSeq)
                    {
                        tokenSeq = existing.TokenNumber + 1;
                    }
                }
            }

            context.SaveChanges();

            ReSequenceQueue(context, doctorId, date);
        }

        public static void ReSequenceQueue(ApplicationDbContext context, int doctorId, DateTime date)
        {
            var entries = context.QueueEntries
                .Include(q => q.Appointment)
                .Where(q => q.Appointment.DoctorId == doctorId 
                         && q.Appointment.AppointmentDate.Date == date.Date)
                .ToList();

            if (!entries.Any()) return;

            var activeOrDone = entries
                .Where(q => q.Status != "Waiting")
                .OrderBy(q => q.SequenceNumber)
                .ToList();

            var waiting = entries
                .Where(q => q.Status == "Waiting")
                .OrderBy(q => q.Appointment.IsEmergency ? 0 : 1)
                .ThenBy(q => ParseTimeSlot(q.Appointment.AppointmentTime))
                .ThenBy(q => q.CreatedAt)
                .ToList();

            int seq = 1;
            foreach (var entry in activeOrDone)
            {
                entry.SequenceNumber = seq++;
            }

            foreach (var entry in waiting)
            {
                entry.SequenceNumber = seq++;
            }

            context.SaveChanges();
        }

        public static TimeSpan ParseTimeSlot(string timeSlot)
        {
            if (string.IsNullOrWhiteSpace(timeSlot)) return TimeSpan.Zero;

            var firstPart = timeSlot.Split('-')[0].Trim();
            if (DateTime.TryParse(firstPart, out DateTime parsedTime))
            {
                return parsedTime.TimeOfDay;
            }
            return TimeSpan.Zero;
        }

        public static string GetDoctorStatus(ApplicationDbContext context, int doctorId, DateTime date)
        {
            var schedule = context.DoctorSchedules
                .FirstOrDefault(ds => ds.DoctorId == doctorId && ds.AvailableDate.Date == date.Date);

            if (schedule == null) return "Offline";
            if (schedule.IsVacation) return "On Vacation";

            if (!string.IsNullOrEmpty(schedule.BreakStartTime) && !string.IsNullOrEmpty(schedule.BreakEndTime))
            {
                var now = DateTime.Now.TimeOfDay;
                var breakStart = ParseTimeSlot(schedule.BreakStartTime);
                var breakEnd = ParseTimeSlot(schedule.BreakEndTime);

                if (now >= breakStart && now <= breakEnd)
                {
                    return "On Break";
                }
            }

            var hasActiveEntries = context.QueueEntries
                .Include(q => q.Appointment)
                .Any(q => q.Appointment.DoctorId == doctorId 
                       && q.Appointment.AppointmentDate.Date == date.Date 
                       && (q.Status == "Calling" || q.Status == "InConsultation"));

            if (hasActiveEntries)
            {
                return "Serving Patients";
            }

            return "Available";
        }
    }
}
