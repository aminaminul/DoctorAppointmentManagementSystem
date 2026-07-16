using DoctorAppointmentManagementSystem.Models;

namespace DoctorAppointmentManagementSystem.Data
{
    public static class SeedData
    {
        public static void Initialize(ApplicationDbContext context)
        {
            SeedRoles(context);
            SeedUsers(context);
            SeedPatients(context);
            SeedDoctors(context);
            SeedAppointments(context);
            SeedDoctorSchedules(context);
            SeedPrescriptions(context);
            SeedMedicalRecords(context);
            SeedPayments(context);
            SeedFeedbacks(context);
            SeedNotifications(context);
            SeedAdminLogs(context);
            SeedQueueEntries(context);
            SeedPrivacyPolicies(context);
        }

        private static void SeedRoles(ApplicationDbContext context)
        {
            if (context.Roles.Any()) return;

            context.Roles.AddRange(
                new Role { Name = "Admin",   Description = "System Administrator" },
                new Role { Name = "Doctor",  Description = "Medical Doctor" },
                new Role { Name = "Patient", Description = "Registered Patient" }
            );
            context.SaveChanges();
        }

        private static void SeedUsers(ApplicationDbContext context)
        {
            if (context.Users.Any()) return;    

            var adminRole   = context.Roles.First(r => r.Name == "Admin");
            var doctorRole  = context.Roles.First(r => r.Name == "Doctor");
            var patientRole = context.Roles.First(r => r.Name == "Patient");

            context.Users.AddRange(
                new User
                {
                    Username = "Rahim Uddin",
                    Email = "admin@dams.com.bd",
                    Password = "1234",
                    PhoneNumber = "01711000001",
                    AccountCreationDateTime = new DateTime(2025, 1, 10),
                    ActiveStatus = true,
                    RoleId = adminRole.Id
                },
                new User
                {
                    Username = "Dr. Kamal Hossain",
                    Email = "drkamal@dams.com.bd",
                    Password = "1234",
                    PhoneNumber = "01712000002",
                    AccountCreationDateTime = new DateTime(2025, 2, 5),
                    ActiveStatus = true,
                    RoleId = doctorRole.Id
                },
                new User
                {
                    Username = "Dr. Nasrin Akter",
                    Email = "drnasrin@dams.com.bd",
                    Password = "1234",
                    PhoneNumber = "01713000003",
                    AccountCreationDateTime = new DateTime(2025, 2, 15),
                    ActiveStatus = true,
                    RoleId = doctorRole.Id
                },
                new User
                {
                    Username = "Dr. Mizanur Rahman",
                    Email = "drmizanur@dams.com.bd",
                    Password = "1234",
                    PhoneNumber = "01714000004",
                    AccountCreationDateTime = new DateTime(2025, 3, 1),
                    ActiveStatus = true,
                    RoleId = doctorRole.Id
                },
                new User
                {
                    Username = "Farida Begum",
                    Email = "farida@gmail.com",
                    Password = "1234",
                    PhoneNumber = "01815000005",
                    AccountCreationDateTime = new DateTime(2025, 4, 10),
                    ActiveStatus = true,
                    RoleId = patientRole.Id
                },
                new User
                {
                    Username = "Md. Jakir Hosen",
                    Email = "jakir@gmail.com",
                    Password = "1234",
                    PhoneNumber = "01816000006",
                    AccountCreationDateTime = new DateTime(2025, 5, 20),
                    ActiveStatus = true,
                    RoleId = patientRole.Id
                },
                new User
                {
                    Username = "Sumaiya Khanam",
                    Email = "sumaiya@gmail.com",
                    Password = "1234",
                    PhoneNumber = "01817000007",
                    AccountCreationDateTime = new DateTime(2025, 6, 8),
                    ActiveStatus = true,
                    RoleId = patientRole.Id
                }
            );
            context.SaveChanges();
        }

        private static void SeedPatients(ApplicationDbContext context)
        {
            if (context.Patients.Any()) return;

            var userFarida  = context.Users.First(u => u.Email == "farida@gmail.com");
            var userJakir   = context.Users.First(u => u.Email == "jakir@gmail.com");
            var userSumaiya = context.Users.First(u => u.Email == "sumaiya@gmail.com");

            context.Patients.AddRange(
                new Patient
                {
                    UserId = userFarida.Id,
                    Gender = "Female",
                    DateOfBirth = new DateTime(1985, 3, 22),
                    BloodGroup = "B+",
                    Address = "House 12, Road 5, Dhanmondi, Dhaka-1205",
                    EmergencyContact = "01911000010",
                    MedicalHistory = "Hypertension, Diabetes (Type 2)",
                    Allergies = "Penicillin allergy",
                    ActiveStatus = true
                },
                new Patient
                {
                    UserId = userJakir.Id,
                    Gender = "Male",
                    DateOfBirth = new DateTime(1992, 7, 15),
                    BloodGroup = "O+",
                    Address = "House 45, Sector 7, Uttara, Dhaka-1230",
                    EmergencyContact = "01911000011",
                    MedicalHistory = "Breathing difficulty, Sinusitis",
                    Allergies = "Dust and pollen allergy",
                    ActiveStatus = true
                },
                new Patient
                {
                    UserId = userSumaiya.Id,
                    Gender = "Female",
                    DateOfBirth = new DateTime(2000, 11, 30),
                    BloodGroup = "A-",
                    Address = "Flat 3B, Nasrin Tower, Chittagong",
                    EmergencyContact = "01911000012",
                    MedicalHistory = "Thyroid disorder (Hypothyroid)",
                    Allergies = "No known allergies",
                    ActiveStatus = true
                }
            );
            context.SaveChanges();
        }

        private static void SeedDoctors(ApplicationDbContext context)
        {
            if (context.Doctors.Any()) return;

            var userKamal   = context.Users.First(u => u.Email == "drkamal@dams.com.bd");
            var userNasrin  = context.Users.First(u => u.Email == "drnasrin@dams.com.bd");
            var userMizanur = context.Users.First(u => u.Email == "drmizanur@dams.com.bd");

            context.Doctors.AddRange(
                new Doctor
                {
                    UserId = userKamal.Id,
                    Specialization = "Cardiologist",
                    Qualification = "MBBS, MD (Cardiology), FCPS",
                    Experience = 14,
                    ConsultationFee = 1200,
                    AvailableDays = "Saturday, Sunday, Monday, Tuesday, Wednesday",
                    AvailableTime = "10:00 AM - 5:00 PM",
                    ActiveStatus = true
                },
                new Doctor
                {
                    UserId = userNasrin.Id,
                    Specialization = "Gynaecologist",
                    Qualification = "MBBS, FCPS (Obs & Gynae)",
                    Experience = 10,
                    ConsultationFee = 1000,
                    AvailableDays = "Sunday, Monday, Tuesday, Thursday",
                    AvailableTime = "9:00 AM - 2:00 PM",
                    ActiveStatus = true
                },
                new Doctor
                {
                    UserId = userMizanur.Id,
                    Specialization = "Medicine Specialist",
                    Qualification = "MBBS, FCPS (Medicine), MD",
                    Experience = 8,
                    ConsultationFee = 800,
                    AvailableDays = "Saturday, Monday, Wednesday, Friday",
                    AvailableTime = "4:00 PM - 8:00 PM",
                    ActiveStatus = true
                }
            );
            context.SaveChanges();
        }

        private static void SeedAppointments(ApplicationDbContext context)
        {
            if (context.Appointments.Any()) return;

            var patientFarida  = context.Patients.First(p => p.User.Email == "farida@gmail.com");
            var patientJakir   = context.Patients.First(p => p.User.Email == "jakir@gmail.com");
            var patientSumaiya = context.Patients.First(p => p.User.Email == "sumaiya@gmail.com");

            var doctorKamal   = context.Doctors.First(d => d.User.Email == "drkamal@dams.com.bd");
            var doctorNasrin  = context.Doctors.First(d => d.User.Email == "drnasrin@dams.com.bd");
            var doctorMizanur = context.Doctors.First(d => d.User.Email == "drmizanur@dams.com.bd");

            context.Appointments.AddRange(
                new Appointment
                {
                    PatientId = patientFarida.Id,
                    DoctorId = doctorKamal.Id,
                    AppointmentDate = new DateTime(2025, 8, 5),
                    AppointmentTime = "10:30 AM",
                    ReasonForVisit = "Chest pain and shortness of breath",
                    AppointmentStatus = "Completed",
                    IsEmergency = false,
                    BookingDateTime = new DateTime(2025, 8, 3, 9, 0, 0)
                },
                new Appointment
                {
                    PatientId = patientJakir.Id,
                    DoctorId = doctorMizanur.Id,
                    AppointmentDate = new DateTime(2025, 8, 10),
                    AppointmentTime = "5:00 PM",
                    ReasonForVisit = "Fever, headache and body ache",
                    AppointmentStatus = "Confirmed",
                    IsEmergency = false,
                    BookingDateTime = new DateTime(2025, 8, 8, 11, 30, 0)
                },
                new Appointment
                {
                    PatientId = patientSumaiya.Id,
                    DoctorId = doctorNasrin.Id,
                    AppointmentDate = new DateTime(2025, 8, 15),
                    AppointmentTime = "11:00 AM",
                    ReasonForVisit = "Thyroid follow-up checkup",
                    AppointmentStatus = "Pending",
                    IsEmergency = false,
                    BookingDateTime = new DateTime(2025, 8, 13, 14, 0, 0)
                }
            );
            context.SaveChanges();
        }

        private static void SeedDoctorSchedules(ApplicationDbContext context)
        {
            if (context.DoctorSchedules.Any()) return;

            var doctorKamal   = context.Doctors.First(d => d.User.Email == "drkamal@dams.com.bd");
            var doctorNasrin  = context.Doctors.First(d => d.User.Email == "drnasrin@dams.com.bd");
            var doctorMizanur = context.Doctors.First(d => d.User.Email == "drmizanur@dams.com.bd");

            context.DoctorSchedules.AddRange(
                new DoctorSchedule
                {
                    DoctorId = doctorKamal.Id,
                    AvailableDate = new DateTime(2025, 8, 5),
                    StartTime = "10:00",
                    EndTime = "17:00",
                    BreakStartTime = "13:00",
                    BreakEndTime = "14:00",
                    SlotStatus = "Available",
                    IsVacation = false,
                    Notes = "Regular chamber session"
                },
                new DoctorSchedule
                {
                    DoctorId = doctorNasrin.Id,
                    AvailableDate = new DateTime(2025, 8, 10),
                    StartTime = "09:00",
                    EndTime = "14:00",
                    BreakStartTime = null,
                    BreakEndTime = null,
                    SlotStatus = "Available",
                    IsVacation = false,
                    Notes = "Existing patients only"
                },
                new DoctorSchedule
                {
                    DoctorId = doctorMizanur.Id,
                    AvailableDate = new DateTime(2025, 8, 14),
                    StartTime = "16:00",
                    EndTime = "20:00",
                    BreakStartTime = null,
                    BreakEndTime = null,
                    SlotStatus = "Booked",
                    IsVacation = false,
                    Notes = "Afternoon session fully booked"
                }
            );
            context.SaveChanges();
        }

        private static void SeedPrescriptions(ApplicationDbContext context)
        {
            if (context.Prescriptions.Any()) return;

            var appt1 = context.Appointments.First(a => a.AppointmentStatus == "Completed");
            var appt2 = context.Appointments.First(a => a.AppointmentStatus == "Confirmed");
            var appt3 = context.Appointments.First(a => a.AppointmentStatus == "Pending");

            context.Prescriptions.AddRange(
                new Prescription
                {
                    AppointmentId = appt1.Id,
                    DoctorId = appt1.DoctorId,
                    PatientId = appt1.PatientId,
                    Diagnosis = "Suspected Ischemic Heart Disease",
                    Medicines = "Aspirin 75mg - 1 tablet after breakfast;\nAtorvastatin 20mg - 1 tablet at bedtime;\nMetoprolol 50mg - 1 tablet morning and night",
                    Instructions = "Reduce salt intake. Avoid heavy physical exertion. Get an ECG after 2 weeks.",
                    PrescriptionDateTime = new DateTime(2025, 8, 5, 11, 30, 0),
                    Status = "Active"
                },
                new Prescription
                {
                    AppointmentId = appt2.Id,
                    DoctorId = appt2.DoctorId,
                    PatientId = appt2.PatientId,
                    Diagnosis = "Viral Fever and Upper Respiratory Tract Infection",
                    Medicines = "Paracetamol 500mg - 1 tablet every 8 hours when feverish;\nFexofenadine 120mg - 1 tablet in the evening;\nORS - 2 to 3 sachets dissolved in water daily",
                    Instructions = "Take rest and drink plenty of water. Return if not improving within 3 days.",
                    PrescriptionDateTime = new DateTime(2025, 8, 10, 17, 45, 0),
                    Status = "Active"
                },
                new Prescription
                {
                    AppointmentId = appt3.Id,
                    DoctorId = appt3.DoctorId,
                    PatientId = appt3.PatientId,
                    Diagnosis = "Hypothyroidism (Controlled)",
                    Medicines = "Levothyroxine 50mcg - 1 tablet every morning on empty stomach",
                    Instructions = "Get TSH test after 3 months. Do not eat anything 30 minutes after taking the tablet.",
                    PrescriptionDateTime = new DateTime(2025, 8, 15, 11, 30, 0),
                    Status = "Active"
                }
            );
            context.SaveChanges();
        }

        private static void SeedMedicalRecords(ApplicationDbContext context)
        {
            if (context.MedicalRecords.Any()) return;

            var appt1 = context.Appointments.First(a => a.AppointmentStatus == "Completed");
            var appt2 = context.Appointments.First(a => a.AppointmentStatus == "Confirmed");
            var appt3 = context.Appointments.First(a => a.AppointmentStatus == "Pending");

            context.MedicalRecords.AddRange(
                new MedicalRecord
                {
                    PatientId = appt1.PatientId,
                    DoctorId = appt1.DoctorId,
                    AppointmentId = appt1.Id,
                    Diagnosis = "Ischemic Heart Disease",
                    TreatmentDetails = "Medication prescribed. Lifestyle modifications advised.",
                    TestReports = "ECG - ST segment abnormality detected; CBC - Normal",
                    Notes = "Echocardiogram recommended at next visit.",
                    RecordDate = new DateTime(2025, 8, 5)
                },
                new MedicalRecord
                {
                    PatientId = appt2.PatientId,
                    DoctorId = appt2.DoctorId,
                    AppointmentId = appt2.Id,
                    Diagnosis = "Viral Fever",
                    TreatmentDetails = "Supportive treatment. Antihistamine and paracetamol prescribed.",
                    TestReports = "CBC - WBC mildly elevated; Dengue NS1 - Negative",
                    Notes = "Patient likely to recover within 3 days.",
                    RecordDate = new DateTime(2025, 8, 10)
                },
                new MedicalRecord
                {
                    PatientId = appt3.PatientId,
                    DoctorId = appt3.DoctorId,
                    AppointmentId = appt3.Id,
                    Diagnosis = "Hypothyroidism - Stable",
                    TreatmentDetails = "Advised to continue Levothyroxine.",
                    TestReports = "TSH - 3.8 mIU/L (within normal range); T4 - Normal",
                    Notes = "Next follow-up in 3 months.",
                    RecordDate = new DateTime(2025, 8, 15)
                }
            );
            context.SaveChanges();
        }

        private static void SeedPayments(ApplicationDbContext context)
        {
            if (context.Payments.Any()) return;

            var appt1 = context.Appointments.First(a => a.AppointmentStatus == "Completed");
            var appt2 = context.Appointments.First(a => a.AppointmentStatus == "Confirmed");
            var appt3 = context.Appointments.First(a => a.AppointmentStatus == "Pending");

            context.Payments.AddRange(
                new Payment
                {
                    AppointmentId = appt1.Id,
                    PatientId = appt1.PatientId,
                    Amount = 1200,
                    PaymentMethod = "bKash",
                    TransactionId = "BK8A2F3D1E",
                    PaymentDateTime = new DateTime(2025, 8, 5, 10, 0, 0),
                    PaymentStatus = "Paid"
                },
                new Payment
                {
                    AppointmentId = appt2.Id,
                    PatientId = appt2.PatientId,
                    Amount = 800,
                    PaymentMethod = "Nagad",
                    TransactionId = "NG5C7E9B2A",
                    PaymentDateTime = new DateTime(2025, 8, 10, 16, 30, 0),
                    PaymentStatus = "Paid"
                },
                new Payment
                {
                    AppointmentId = appt3.Id,
                    PatientId = appt3.PatientId,
                    Amount = 1000,
                    PaymentMethod = "Cash",
                    TransactionId = null,
                    PaymentDateTime = new DateTime(2025, 8, 15, 10, 45, 0),
                    PaymentStatus = "Pending"
                }
            );
            context.SaveChanges();
        }

        private static void SeedFeedbacks(ApplicationDbContext context)
        {
            if (context.Feedbacks.Any()) return;

            var appt1 = context.Appointments.First(a => a.AppointmentStatus == "Completed");
            var appt2 = context.Appointments.First(a => a.AppointmentStatus == "Confirmed");
            var appt3 = context.Appointments.First(a => a.AppointmentStatus == "Pending");

            context.Feedbacks.AddRange(
                new Feedback
                {
                    PatientId = appt1.PatientId,
                    DoctorId = appt1.DoctorId,
                    Rating = 5,
                    FeedbackDateTime = new DateTime(2025, 8, 6, 10, 0, 0),
                    Status = "Active"
                },
                new Feedback
                {
                    PatientId = appt2.PatientId,
                    DoctorId = appt2.DoctorId,
                    Rating = 4,
                    FeedbackDateTime = new DateTime(2025, 8, 11, 9, 30, 0),
                    Status = "Active"
                },
                new Feedback
                {
                    PatientId = appt3.PatientId,
                    DoctorId = appt3.DoctorId,
                    Rating = 5,
                    FeedbackDateTime = new DateTime(2025, 8, 16, 8, 0, 0),
                    Status = "Active"
                }
            );
            context.SaveChanges();
        }

        private static void SeedNotifications(ApplicationDbContext context)
        {
            if (context.Notifications.Any()) return;

            var userFarida  = context.Users.First(u => u.Email == "farida@gmail.com");
            var userJakir   = context.Users.First(u => u.Email == "jakir@gmail.com");
            var userSumaiya = context.Users.First(u => u.Email == "sumaiya@gmail.com");

            context.Notifications.AddRange(
                new Notification
                {
                    UserId = userFarida.Id,
                    NotificationType = "Appointment",
                    Title = "Appointment Confirmed",
                    Message = "Your appointment on August 5, 2025 at 10:30 AM has been confirmed.",
                    SentDateTime = new DateTime(2025, 8, 3, 9, 5, 0),
                    NotificationStatus = "Read"
                },
                new Notification
                {
                    UserId = userJakir.Id,
                    NotificationType = "Reminder",
                    Title = "Appointment Reminder",
                    Message = "You have an appointment with Dr. Mizanur Rahman tomorrow at 5:00 PM.",
                    SentDateTime = new DateTime(2025, 8, 9, 18, 0, 0),
                    NotificationStatus = "Unread"
                },
                new Notification
                {
                    UserId = userSumaiya.Id,
                    NotificationType = "System",
                    Title = "Profile Updated Successfully",
                    Message = "Your profile has been updated successfully.",
                    SentDateTime = new DateTime(2025, 8, 13, 14, 10, 0),
                    NotificationStatus = "Unread"
                }
            );
            context.SaveChanges();
        }

        private static void SeedAdminLogs(ApplicationDbContext context)
        {
            if (context.AdminLogs.Any()) return;

            var admin = context.Users.First(u => u.Email == "admin@dams.com.bd");

            context.AdminLogs.AddRange(
                new AdminLog
                {
                    AdminId = admin.Id,
                    ActionPerformed = "User Created",
                    Description = "New doctor account created: Dr. Kamal Hossain",
                    ActionDateTime = new DateTime(2025, 2, 5, 10, 0, 0)
                },
                new AdminLog
                {
                    AdminId = admin.Id,
                    ActionPerformed = "Appointment Status Updated",
                    Description = "Appointment status changed from Pending to Completed.",
                    ActionDateTime = new DateTime(2025, 8, 5, 12, 0, 0)
                },
                new AdminLog
                {
                    AdminId = admin.Id,
                    ActionPerformed = "Doctor Schedule Set",
                    Description = "Schedule set for Dr. Nasrin Akter on August 10.",
                    ActionDateTime = new DateTime(2025, 8, 8, 9, 30, 0)
                }
            );
            context.SaveChanges();
        }

        private static void SeedQueueEntries(ApplicationDbContext context)
        {
            if (context.QueueEntries.Any()) return;

            var appt1 = context.Appointments.First(a => a.AppointmentStatus == "Completed");
            var appt2 = context.Appointments.First(a => a.AppointmentStatus == "Confirmed");
            var appt3 = context.Appointments.First(a => a.AppointmentStatus == "Pending");

            context.QueueEntries.AddRange(
                new QueueEntry
                {
                    AppointmentId = appt1.Id,
                    TokenNumber = 101,
                    SequenceNumber = 1,
                    Status = "Completed",
                    CreatedAt = new DateTime(2025, 8, 5, 9, 55, 0),
                    CallTime = new DateTime(2025, 8, 5, 10, 28, 0),
                    CompletionTime = new DateTime(2025, 8, 5, 11, 10, 0)
                },
                new QueueEntry
                {
                    AppointmentId = appt2.Id,
                    TokenNumber = 205,
                    SequenceNumber = 5,
                    Status = "Waiting",
                    CreatedAt = new DateTime(2025, 8, 10, 16, 20, 0),
                    CallTime = null,
                    CompletionTime = null
                },
                new QueueEntry
                {
                    AppointmentId = appt3.Id,
                    TokenNumber = 302,
                    SequenceNumber = 2,
                    Status = "Waiting",
                    CreatedAt = new DateTime(2025, 8, 15, 10, 45, 0),
                    CallTime = null,
                    CompletionTime = null
                }
            );
            context.SaveChanges();
        }

        private static void SeedPrivacyPolicies(ApplicationDbContext context)
        {
            if (context.PrivacyPolicies.Any()) return;

            context.PrivacyPolicies.AddRange(
                new PrivacyPolicy
                {
                    Content = "DAMS (Doctor Appointment Management System) is committed to protecting your personal information. We collect your name, address, phone number and health-related data solely for the purpose of providing medical services.",
                    UpdatedAt = new DateTime(2025, 1, 1)
                },
                new PrivacyPolicy
                {
                    Content = "Your personal and health information will not be sold or transferred to third parties. Only the concerned doctor and authorized staff members are permitted to view this information.",
                    UpdatedAt = new DateTime(2025, 3, 15)
                },
                new PrivacyPolicy
                {
                    Content = "Always use a strong password to keep your account secure. If you notice any suspicious activity, contact us immediately at: support@dams.com.bd",
                    UpdatedAt = new DateTime(2025, 6, 1)
                }
            );
            context.SaveChanges();
        }
    }
}
