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
                new Role { Name = "Admin",        Description = "System Administrator" },
                new Role { Name = "Doctor",       Description = "Medical Doctor" },
                new Role { Name = "Patient",      Description = "Registered Patient" },
                new Role { Name = "Receptionist", Description = "Hospital Receptionist" },
                new Role { Name = "LabStaff",     Description = "Laboratory Staff" }
            );
            context.SaveChanges();
        }

        private static void SeedUsers(ApplicationDbContext context)
        {
            if (context.Users.Any()) return;
            var adminRole   = context.Roles.First(r => r.Name == "Admin");
            var doctorRole  = context.Roles.First(r => r.Name == "Doctor");
            var patientRole = context.Roles.First(r => r.Name == "Patient");

            context.Users.Add(new User
            {
                Username = "Rahim Uddin",
                Email = "admin@dams.com.bd",
                Password = "1234",
                PhoneNumber = "01711000001",
                AccountCreationDateTime = new DateTime(2025, 1, 10),
                ActiveStatus = true,
                RoleId = adminRole.Id
            });

            context.Users.AddRange(
                new User { Username = "Dr. Kamal Hossain",  Email = "drkamal@dams.com.bd",    Password = "1234", PhoneNumber = "01712000002", AccountCreationDateTime = new DateTime(2025, 2, 5),  ActiveStatus = true, RoleId = doctorRole.Id },
                new User { Username = "Dr. Nasrin Akter",   Email = "drnasrin@dams.com.bd",   Password = "1234", PhoneNumber = "01713000003", AccountCreationDateTime = new DateTime(2025, 2, 15), ActiveStatus = true, RoleId = doctorRole.Id },
                new User { Username = "Dr. Mizanur Rahman", Email = "drmizanur@dams.com.bd",  Password = "1234", PhoneNumber = "01714000004", AccountCreationDateTime = new DateTime(2025, 3, 1),  ActiveStatus = true, RoleId = doctorRole.Id },
                new User { Username = "Dr. Tahmina Begum",  Email = "drtahmina@dams.com.bd",  Password = "1234", PhoneNumber = "01715000005", AccountCreationDateTime = new DateTime(2025, 3, 20), ActiveStatus = true, RoleId = doctorRole.Id },
                new User { Username = "Dr. Ashraful Islam", Email = "drashraful@dams.com.bd", Password = "1234", PhoneNumber = "01716000006", AccountCreationDateTime = new DateTime(2025, 4, 1),  ActiveStatus = true, RoleId = doctorRole.Id }
            );

            context.Users.AddRange(
                new User { Username = "Farida Begum",    Email = "farida@gmail.com",  Password = "1234", PhoneNumber = "01815000010", AccountCreationDateTime = new DateTime(2025, 4, 10), ActiveStatus = true, RoleId = patientRole.Id },
                new User { Username = "Md. Jakir Hosen", Email = "jakir@gmail.com",   Password = "1234", PhoneNumber = "01816000011", AccountCreationDateTime = new DateTime(2025, 5, 20), ActiveStatus = true, RoleId = patientRole.Id },
                new User { Username = "Sumaiya Khanam",  Email = "sumaiya@gmail.com", Password = "1234", PhoneNumber = "01817000012", AccountCreationDateTime = new DateTime(2025, 6, 8),  ActiveStatus = true, RoleId = patientRole.Id },
                new User { Username = "Arif Hossain",    Email = "arif@gmail.com",    Password = "1234", PhoneNumber = "01818000013", AccountCreationDateTime = new DateTime(2025, 6, 25), ActiveStatus = true, RoleId = patientRole.Id },
                new User { Username = "Nusrat Jahan",    Email = "nusrat@gmail.com",  Password = "1234", PhoneNumber = "01819000014", AccountCreationDateTime = new DateTime(2025, 7, 5),  ActiveStatus = true, RoleId = patientRole.Id }
            );
            context.SaveChanges();
        }

        private static void SeedPatients(ApplicationDbContext context)
        {
            if (context.Patients.Any()) return;
            var uFarida  = context.Users.First(u => u.Email == "farida@gmail.com");
            var uJakir   = context.Users.First(u => u.Email == "jakir@gmail.com");
            var uSumaiya = context.Users.First(u => u.Email == "sumaiya@gmail.com");
            var uArif    = context.Users.First(u => u.Email == "arif@gmail.com");
            var uNusrat  = context.Users.First(u => u.Email == "nusrat@gmail.com");

            context.Patients.AddRange(
                new Patient { UserId = uFarida.Id,  Gender = "Female", DateOfBirth = new DateTime(1985, 3, 22),  BloodGroup = "B+",  Address = "House 12, Road 5, Dhanmondi, Dhaka-1205",       EmergencyContact = "01911000020", MedicalHistory = "Hypertension, Diabetes (Type 2)",       Allergies = "Penicillin allergy",     ActiveStatus = true },
                new Patient { UserId = uJakir.Id,   Gender = "Male",   DateOfBirth = new DateTime(1992, 7, 15),  BloodGroup = "O+",  Address = "House 45, Sector 7, Uttara, Dhaka-1230",         EmergencyContact = "01911000021", MedicalHistory = "Breathing difficulty, Sinusitis",        Allergies = "Dust and pollen allergy", ActiveStatus = true },
                new Patient { UserId = uSumaiya.Id, Gender = "Female", DateOfBirth = new DateTime(2000, 11, 30), BloodGroup = "A-",  Address = "Flat 3B, Nasrin Tower, Chittagong",              EmergencyContact = "01911000022", MedicalHistory = "Thyroid disorder (Hypothyroid)",          Allergies = "No known allergies",     ActiveStatus = true },
                new Patient { UserId = uArif.Id,    Gender = "Male",   DateOfBirth = new DateTime(1988, 5, 10),  BloodGroup = "AB+", Address = "Road 3, Block C, Mirpur, Dhaka-1216",            EmergencyContact = "01911000023", MedicalHistory = "Asthma, Mild depression",                 Allergies = "Aspirin, NSAIDs",        ActiveStatus = true },
                new Patient { UserId = uNusrat.Id,  Gender = "Female", DateOfBirth = new DateTime(1995, 9, 18),  BloodGroup = "O-",  Address = "Holding 7, Sutrapur, Dhaka Old City",            EmergencyContact = "01911000024", MedicalHistory = "Anemia, Iron deficiency",                 Allergies = "Sulfa drugs",            ActiveStatus = true }
            );
            context.SaveChanges();
        }

        private static void SeedDoctors(ApplicationDbContext context)
        {
            if (context.Doctors.Any()) return;
            var uKamal    = context.Users.First(u => u.Email == "drkamal@dams.com.bd");
            var uNasrin   = context.Users.First(u => u.Email == "drnasrin@dams.com.bd");
            var uMizanur  = context.Users.First(u => u.Email == "drmizanur@dams.com.bd");
            var uTahmina  = context.Users.First(u => u.Email == "drtahmina@dams.com.bd");
            var uAshraful = context.Users.First(u => u.Email == "drashraful@dams.com.bd");

            context.Doctors.AddRange(
                new Doctor { UserId = uKamal.Id,    Specialization = "Cardiologist",       Qualification = "MBBS, MD (Cardiology), FCPS",    Experience = 14, ConsultationFee = 1200, AvailableDays = "Saturday, Sunday, Monday, Tuesday, Wednesday",  AvailableTime = "10:00 AM - 5:00 PM", ActiveStatus = true },
                new Doctor { UserId = uNasrin.Id,   Specialization = "Gynaecologist",      Qualification = "MBBS, FCPS (Obs & Gynae)",        Experience = 10, ConsultationFee = 1000, AvailableDays = "Sunday, Monday, Tuesday, Thursday",             AvailableTime = "9:00 AM - 2:00 PM",  ActiveStatus = true },
                new Doctor { UserId = uMizanur.Id,  Specialization = "Medicine Specialist", Qualification = "MBBS, FCPS (Medicine), MD",      Experience = 8,  ConsultationFee = 800,  AvailableDays = "Saturday, Monday, Wednesday, Friday",           AvailableTime = "4:00 PM - 8:00 PM",  ActiveStatus = true },
                new Doctor { UserId = uTahmina.Id,  Specialization = "Neurologist",        Qualification = "MBBS, MD (Neurology), FCPS",     Experience = 12, ConsultationFee = 1500, AvailableDays = "Sunday, Tuesday, Thursday",                     AvailableTime = "11:00 AM - 4:00 PM", ActiveStatus = true },
                new Doctor { UserId = uAshraful.Id, Specialization = "Orthopedic Surgeon", Qualification = "MBBS, MS (Orthopedics), FCPS",   Experience = 9,  ConsultationFee = 1100, AvailableDays = "Saturday, Sunday, Wednesday, Thursday",         AvailableTime = "9:00 AM - 1:00 PM",  ActiveStatus = true }
            );
            context.SaveChanges();
        }

        private static void SeedAppointments(ApplicationDbContext context)
        {
            if (context.Appointments.Any()) return;
            var pFarida  = context.Patients.First(p => p.User.Email == "farida@gmail.com");
            var pJakir   = context.Patients.First(p => p.User.Email == "jakir@gmail.com");
            var pSumaiya = context.Patients.First(p => p.User.Email == "sumaiya@gmail.com");
            var pArif    = context.Patients.First(p => p.User.Email == "arif@gmail.com");
            var pNusrat  = context.Patients.First(p => p.User.Email == "nusrat@gmail.com");
            var dKamal    = context.Doctors.First(d => d.User.Email == "drkamal@dams.com.bd");
            var dNasrin   = context.Doctors.First(d => d.User.Email == "drnasrin@dams.com.bd");
            var dMizanur  = context.Doctors.First(d => d.User.Email == "drmizanur@dams.com.bd");
            var dTahmina  = context.Doctors.First(d => d.User.Email == "drtahmina@dams.com.bd");
            var dAshraful = context.Doctors.First(d => d.User.Email == "drashraful@dams.com.bd");

            context.Appointments.AddRange(
                new Appointment { PatientId = pFarida.Id,  DoctorId = dKamal.Id,    AppointmentDate = new DateTime(2025, 8, 5),  AppointmentTime = "10:30 AM", ReasonForVisit = "Chest pain and shortness of breath",      AppointmentStatus = "Completed", IsEmergency = false, BookingDateTime = new DateTime(2025, 8, 3,  9,  0, 0) },
                new Appointment { PatientId = pJakir.Id,   DoctorId = dMizanur.Id,  AppointmentDate = new DateTime(2025, 8, 10), AppointmentTime = "5:00 PM",  ReasonForVisit = "Fever, headache and body ache",            AppointmentStatus = "Completed", IsEmergency = false, BookingDateTime = new DateTime(2025, 8, 8,  11, 30, 0) },
                new Appointment { PatientId = pSumaiya.Id, DoctorId = dNasrin.Id,   AppointmentDate = new DateTime(2025, 8, 15), AppointmentTime = "11:00 AM", ReasonForVisit = "Thyroid follow-up checkup",                AppointmentStatus = "Completed", IsEmergency = false, BookingDateTime = new DateTime(2025, 8, 13, 14, 0,  0) },
                new Appointment { PatientId = pArif.Id,    DoctorId = dTahmina.Id,  AppointmentDate = new DateTime(2025, 9, 2),  AppointmentTime = "11:30 AM", ReasonForVisit = "Persistent headache and dizziness",        AppointmentStatus = "Confirmed", IsEmergency = false, BookingDateTime = new DateTime(2025, 8, 30, 10, 0,  0) },
                new Appointment { PatientId = pNusrat.Id,  DoctorId = dAshraful.Id, AppointmentDate = new DateTime(2025, 9, 5),  AppointmentTime = "10:00 AM", ReasonForVisit = "Knee pain and difficulty in walking",      AppointmentStatus = "Pending",   IsEmergency = false, BookingDateTime = new DateTime(2025, 9, 3,  8,  30, 0) }
            );
            context.SaveChanges();
        }

        private static void SeedDoctorSchedules(ApplicationDbContext context)
        {
            if (context.DoctorSchedules.Any()) return;
            var dKamal    = context.Doctors.First(d => d.User.Email == "drkamal@dams.com.bd");
            var dNasrin   = context.Doctors.First(d => d.User.Email == "drnasrin@dams.com.bd");
            var dMizanur  = context.Doctors.First(d => d.User.Email == "drmizanur@dams.com.bd");
            var dTahmina  = context.Doctors.First(d => d.User.Email == "drtahmina@dams.com.bd");
            var dAshraful = context.Doctors.First(d => d.User.Email == "drashraful@dams.com.bd");

            context.DoctorSchedules.AddRange(
                new DoctorSchedule { DoctorId = dKamal.Id,    AvailableDate = new DateTime(2025, 8, 5),  StartTime = "10:00", EndTime = "17:00", BreakStartTime = "13:00", BreakEndTime = "14:00", SlotStatus = "Available", IsVacation = false, Notes = "Regular chamber session" },
                new DoctorSchedule { DoctorId = dNasrin.Id,   AvailableDate = new DateTime(2025, 8, 10), StartTime = "09:00", EndTime = "14:00", BreakStartTime = null,    BreakEndTime = null,    SlotStatus = "Available", IsVacation = false, Notes = "Existing patients only" },
                new DoctorSchedule { DoctorId = dMizanur.Id,  AvailableDate = new DateTime(2025, 8, 14), StartTime = "16:00", EndTime = "20:00", BreakStartTime = null,    BreakEndTime = null,    SlotStatus = "Booked",    IsVacation = false, Notes = "Afternoon session fully booked" },
                new DoctorSchedule { DoctorId = dTahmina.Id,  AvailableDate = new DateTime(2025, 9, 2),  StartTime = "11:00", EndTime = "16:00", BreakStartTime = "13:30", BreakEndTime = "14:00", SlotStatus = "Available", IsVacation = false, Notes = "New and follow-up patients" },
                new DoctorSchedule { DoctorId = dAshraful.Id, AvailableDate = new DateTime(2025, 9, 5),  StartTime = "09:00", EndTime = "13:00", BreakStartTime = null,    BreakEndTime = null,    SlotStatus = "Available", IsVacation = false, Notes = "Morning OPD session" }
            );
            context.SaveChanges();
        }

        private static void SeedPrescriptions(ApplicationDbContext context)
        {
            if (context.Prescriptions.Any()) return;
            var appts = context.Appointments.OrderBy(a => a.AppointmentDate).ToList();

            context.Prescriptions.AddRange(
                new Prescription
                {
                    AppointmentId = appts[0].Id, DoctorId = appts[0].DoctorId, PatientId = appts[0].PatientId,
                    Diagnosis = "Suspected Ischemic Heart Disease",
                    Medicines = "Aspirin 75mg - 1 tablet after breakfast;\nAtorvastatin 20mg - 1 tablet at bedtime;\nMetoprolol 50mg - 1 tablet morning and night",
                    Instructions = "Reduce salt intake. Avoid heavy physical exertion. ECG after 2 weeks.",
                    PrescriptionDateTime = new DateTime(2025, 8, 5, 11, 30, 0), Status = "Active"
                },
                new Prescription
                {
                    AppointmentId = appts[1].Id, DoctorId = appts[1].DoctorId, PatientId = appts[1].PatientId,
                    Diagnosis = "Viral Fever and Upper Respiratory Tract Infection",
                    Medicines = "Paracetamol 500mg - 1 tablet every 8 hours;\nFexofenadine 120mg - 1 tablet evening;\nORS - 2 to 3 sachets daily",
                    Instructions = "Rest and drink plenty of water. Return if not improving within 3 days.",
                    PrescriptionDateTime = new DateTime(2025, 8, 10, 17, 45, 0), Status = "Active"
                },
                new Prescription
                {
                    AppointmentId = appts[2].Id, DoctorId = appts[2].DoctorId, PatientId = appts[2].PatientId,
                    Diagnosis = "Hypothyroidism (Controlled)",
                    Medicines = "Levothyroxine 50mcg - 1 tablet every morning on empty stomach",
                    Instructions = "TSH test after 3 months. Do not eat 30 min after the tablet.",
                    PrescriptionDateTime = new DateTime(2025, 8, 15, 11, 30, 0), Status = "Active"
                },
                new Prescription
                {
                    AppointmentId = appts[3].Id, DoctorId = appts[3].DoctorId, PatientId = appts[3].PatientId,
                    Diagnosis = "Tension-Type Headache with Mild Vertigo",
                    Medicines = "Naproxen 500mg - 1 tablet twice daily;\nFlunarizine 5mg - 1 tablet at bedtime;\nVitamin B Complex - 1 tablet daily",
                    Instructions = "Avoid screen time before sleep. Adequate hydration. MRI brain if not improving.",
                    PrescriptionDateTime = new DateTime(2025, 9, 2, 12, 15, 0), Status = "Active"
                },
                new Prescription
                {
                    AppointmentId = appts[4].Id, DoctorId = appts[4].DoctorId, PatientId = appts[4].PatientId,
                    Diagnosis = "Right Knee Osteoarthritis (Grade II)",
                    Medicines = "Etoricoxib 60mg - 1 tablet daily after meals;\nCalcium + Vitamin D3 - 1 tablet twice daily;\nDiclofenac gel - apply on knee twice daily",
                    Instructions = "Knee physiotherapy 3x per week. Avoid climbing stairs. X-ray after 4 weeks.",
                    PrescriptionDateTime = new DateTime(2025, 9, 5, 10, 45, 0), Status = "Active"
                }
            );
            context.SaveChanges();
        }

        private static void SeedMedicalRecords(ApplicationDbContext context)
        {
            if (context.MedicalRecords.Any()) return;
            var appts = context.Appointments.OrderBy(a => a.AppointmentDate).ToList();

            context.MedicalRecords.AddRange(
                new MedicalRecord { PatientId = appts[0].PatientId, DoctorId = appts[0].DoctorId, AppointmentId = appts[0].Id, Diagnosis = "Ischemic Heart Disease",             TreatmentDetails = "Medication prescribed. Lifestyle modifications advised.",            TestReports = "ECG - ST segment abnormality; CBC - Normal",                Notes = "Echocardiogram at next visit.",                      RecordDate = new DateTime(2025, 8, 5)  },
                new MedicalRecord { PatientId = appts[1].PatientId, DoctorId = appts[1].DoctorId, AppointmentId = appts[1].Id, Diagnosis = "Viral Fever",                        TreatmentDetails = "Supportive treatment. Antihistamine and paracetamol prescribed.",  TestReports = "CBC - WBC mildly elevated; Dengue NS1 - Negative",          Notes = "Expected recovery within 3 days.",                   RecordDate = new DateTime(2025, 8, 10) },
                new MedicalRecord { PatientId = appts[2].PatientId, DoctorId = appts[2].DoctorId, AppointmentId = appts[2].Id, Diagnosis = "Hypothyroidism - Stable",            TreatmentDetails = "Advised to continue Levothyroxine.",                               TestReports = "TSH - 3.8 mIU/L (normal); T4 - Normal",                     Notes = "Next follow-up in 3 months.",                        RecordDate = new DateTime(2025, 8, 15) },
                new MedicalRecord { PatientId = appts[3].PatientId, DoctorId = appts[3].DoctorId, AppointmentId = appts[3].Id, Diagnosis = "Tension-Type Headache with Vertigo", TreatmentDetails = "Analgesic and vestibular suppressant prescribed.",                 TestReports = "BP - 128/82 mmHg; Blood Sugar - Normal; Neuro exam - Normal", Notes = "Brain MRI if symptoms persist beyond 2 weeks.",      RecordDate = new DateTime(2025, 9, 2)  },
                new MedicalRecord { PatientId = appts[4].PatientId, DoctorId = appts[4].DoctorId, AppointmentId = appts[4].Id, Diagnosis = "Right Knee Osteoarthritis (Grade II)", TreatmentDetails = "NSAID prescribed. Physiotherapy referral given.",                TestReports = "X-ray Right Knee - Grade II OA changes; BMI - 27",           Notes = "Weight management counseled. Knee brace advised.",    RecordDate = new DateTime(2025, 9, 5)  }
            );
            context.SaveChanges();
        }

        private static void SeedPayments(ApplicationDbContext context)
        {
            if (context.Payments.Any()) return;
            var appts = context.Appointments.OrderBy(a => a.AppointmentDate).ToList();

            context.Payments.AddRange(
                new Payment { AppointmentId = appts[0].Id, PatientId = appts[0].PatientId, Amount = 1200, PaymentMethod = "bKash",  TransactionId = "BK8A2F3D1E", PaymentDateTime = new DateTime(2025, 8, 5,  10, 0,  0), PaymentStatus = "Paid"    },
                new Payment { AppointmentId = appts[1].Id, PatientId = appts[1].PatientId, Amount = 800,  PaymentMethod = "Nagad",  TransactionId = "NG5C7E9B2A", PaymentDateTime = new DateTime(2025, 8, 10, 16, 30, 0), PaymentStatus = "Paid"    },
                new Payment { AppointmentId = appts[2].Id, PatientId = appts[2].PatientId, Amount = 1000, PaymentMethod = "Cash",   TransactionId = null,         PaymentDateTime = new DateTime(2025, 8, 15, 10, 45, 0), PaymentStatus = "Paid"    },
                new Payment { AppointmentId = appts[3].Id, PatientId = appts[3].PatientId, Amount = 1500, PaymentMethod = "Card",   TransactionId = "CRD3F9A7B4", PaymentDateTime = new DateTime(2025, 9, 2,  11, 0,  0), PaymentStatus = "Paid"    },
                new Payment { AppointmentId = appts[4].Id, PatientId = appts[4].PatientId, Amount = 1100, PaymentMethod = "bKash",  TransactionId = null,         PaymentDateTime = new DateTime(2025, 9, 5,  9,  30, 0), PaymentStatus = "Pending" }
            );
            context.SaveChanges();
        }

        private static void SeedFeedbacks(ApplicationDbContext context)
        {
            if (context.Feedbacks.Any()) return;
            var appts = context.Appointments.OrderBy(a => a.AppointmentDate).ToList();

            context.Feedbacks.AddRange(
                new Feedback { PatientId = appts[0].PatientId, DoctorId = appts[0].DoctorId, Rating = 5, FeedbackDateTime = new DateTime(2025, 8, 6,  10, 0,  0), Status = "Active" },
                new Feedback { PatientId = appts[1].PatientId, DoctorId = appts[1].DoctorId, Rating = 4, FeedbackDateTime = new DateTime(2025, 8, 11, 9,  30, 0), Status = "Active" },
                new Feedback { PatientId = appts[2].PatientId, DoctorId = appts[2].DoctorId, Rating = 5, FeedbackDateTime = new DateTime(2025, 8, 16, 8,  0,  0), Status = "Active" },
                new Feedback { PatientId = appts[3].PatientId, DoctorId = appts[3].DoctorId, Rating = 4, FeedbackDateTime = new DateTime(2025, 9, 3,  14, 0,  0), Status = "Active" },
                new Feedback { PatientId = appts[4].PatientId, DoctorId = appts[4].DoctorId, Rating = 3, FeedbackDateTime = new DateTime(2025, 9, 6,  11, 0,  0), Status = "Active" }
            );
            context.SaveChanges();
        }

        private static void SeedNotifications(ApplicationDbContext context)
        {
            if (context.Notifications.Any()) return;
            var uFarida  = context.Users.First(u => u.Email == "farida@gmail.com");
            var uJakir   = context.Users.First(u => u.Email == "jakir@gmail.com");
            var uSumaiya = context.Users.First(u => u.Email == "sumaiya@gmail.com");
            var uArif    = context.Users.First(u => u.Email == "arif@gmail.com");
            var uNusrat  = context.Users.First(u => u.Email == "nusrat@gmail.com");

            context.Notifications.AddRange(
                new Notification { UserId = uFarida.Id,  NotificationType = "Appointment", Title = "Appointment Confirmed",  Message = "Your appointment on August 5, 2025 at 10:30 AM with Dr. Kamal Hossain has been confirmed.",       SentDateTime = new DateTime(2025, 8, 3,  9,  5, 0),  NotificationStatus = "Read"   },
                new Notification { UserId = uJakir.Id,   NotificationType = "Reminder",    Title = "Appointment Reminder",   Message = "You have an appointment with Dr. Mizanur Rahman tomorrow at 5:00 PM. Please be on time.",          SentDateTime = new DateTime(2025, 8, 9,  18, 0, 0),  NotificationStatus = "Read"   },
                new Notification { UserId = uSumaiya.Id, NotificationType = "System",      Title = "Profile Updated",        Message = "Your profile information has been updated successfully.",                                          SentDateTime = new DateTime(2025, 8, 13, 14, 10, 0), NotificationStatus = "Unread" },
                new Notification { UserId = uArif.Id,    NotificationType = "Appointment", Title = "Appointment Booked",     Message = "Your appointment with Dr. Tahmina Begum on September 2, 2025 at 11:30 AM is successfully booked.", SentDateTime = new DateTime(2025, 8, 30, 10, 5, 0),  NotificationStatus = "Unread" },
                new Notification { UserId = uNusrat.Id,  NotificationType = "Payment",     Title = "Payment Pending",        Message = "Your consultation fee of BDT 1100 for September 5, 2025 appointment is pending.",                  SentDateTime = new DateTime(2025, 9, 3,  8,  35, 0), NotificationStatus = "Unread" }
            );
            context.SaveChanges();
        }

        private static void SeedAdminLogs(ApplicationDbContext context)
        {
            if (context.AdminLogs.Any()) return;
            var admin = context.Users.First(u => u.Email == "admin@dams.com.bd");

            context.AdminLogs.AddRange(
                new AdminLog { AdminId = admin.Id, ActionPerformed = "User Created",               Description = "New doctor account created: Dr. Kamal Hossain (Cardiologist)",                                    ActionDateTime = new DateTime(2025, 2, 5,  10, 0,  0) },
                new AdminLog { AdminId = admin.Id, ActionPerformed = "Doctor Account Approved",    Description = "Dr. Nasrin Akter's account activated and schedule configured.",                                    ActionDateTime = new DateTime(2025, 2, 15, 11, 30, 0) },
                new AdminLog { AdminId = admin.Id, ActionPerformed = "Appointment Status Updated", Description = "Appointment of Farida Begum (Aug 5) status changed from Pending to Completed.",                   ActionDateTime = new DateTime(2025, 8, 5,  12, 0,  0) },
                new AdminLog { AdminId = admin.Id, ActionPerformed = "Doctor Schedule Set",        Description = "Weekly schedule configured for Dr. Tahmina Begum (Neurologist).",                                 ActionDateTime = new DateTime(2025, 8, 28, 9,  0,  0) },
                new AdminLog { AdminId = admin.Id, ActionPerformed = "Payment Verified",           Description = "Payment of BDT 1500 by Arif Hossain verified and marked as Paid.",                                ActionDateTime = new DateTime(2025, 9, 2,  12, 30, 0) }
            );
            context.SaveChanges();
        }

        private static void SeedQueueEntries(ApplicationDbContext context)
        {
            if (context.QueueEntries.Any()) return;
            var appts = context.Appointments.OrderBy(a => a.AppointmentDate).ToList();

            context.QueueEntries.AddRange(
                new QueueEntry { AppointmentId = appts[0].Id, TokenNumber = 101, SequenceNumber = 1, Status = "Completed", CreatedAt = new DateTime(2025, 8, 5,  9,  55, 0), CallTime = new DateTime(2025, 8, 5,  10, 28, 0), CompletionTime = new DateTime(2025, 8, 5,  11, 10, 0) },
                new QueueEntry { AppointmentId = appts[1].Id, TokenNumber = 205, SequenceNumber = 5, Status = "Completed", CreatedAt = new DateTime(2025, 8, 10, 16, 20, 0), CallTime = new DateTime(2025, 8, 10, 17, 0,  0), CompletionTime = new DateTime(2025, 8, 10, 17, 40, 0) },
                new QueueEntry { AppointmentId = appts[2].Id, TokenNumber = 302, SequenceNumber = 2, Status = "Completed", CreatedAt = new DateTime(2025, 8, 15, 10, 45, 0), CallTime = new DateTime(2025, 8, 15, 11, 0,  0), CompletionTime = new DateTime(2025, 8, 15, 11, 45, 0) },
                new QueueEntry { AppointmentId = appts[3].Id, TokenNumber = 403, SequenceNumber = 3, Status = "Waiting",   CreatedAt = new DateTime(2025, 9, 2,  11, 0,  0), CallTime = null,                                 CompletionTime = null                                  },
                new QueueEntry { AppointmentId = appts[4].Id, TokenNumber = 501, SequenceNumber = 1, Status = "Waiting",   CreatedAt = new DateTime(2025, 9, 5,  9,  30, 0), CallTime = null,                                 CompletionTime = null                                  }
            );
            context.SaveChanges();
        }

        private static void SeedPrivacyPolicies(ApplicationDbContext context)
        {
            if (context.PrivacyPolicies.Any()) return;

            context.PrivacyPolicies.AddRange(
                new PrivacyPolicy { Content = "DAMS is committed to protecting your personal information. We collect your name, address, phone number, and health-related data solely for providing medical services and maintaining your health records securely.",                                                                  UpdatedAt = new DateTime(2025, 1, 1) },
                new PrivacyPolicy { Content = "Your personal and health information will not be sold or transferred to any third party. Only the concerned doctor and authorized DAMS staff members are permitted to view your data.",                                                                                              UpdatedAt = new DateTime(2025, 3, 15) },
                new PrivacyPolicy { Content = "We use industry-standard encryption and SSL/TLS security protocols to safeguard all your data and transactions made through DAMS.",                                                                                                                                                 UpdatedAt = new DateTime(2025, 5, 1) },
                new PrivacyPolicy { Content = "You have the right to request access, correction, or deletion of your personal data at any time. Contact our support team at support@dams.com.bd to exercise these rights.",                                                                                                        UpdatedAt = new DateTime(2025, 6, 1) },
                new PrivacyPolicy { Content = "Always use a strong, unique password to keep your DAMS account secure. Do not share your login credentials with anyone. If you notice suspicious activity, contact us immediately at support@dams.com.bd",                                                                          UpdatedAt = new DateTime(2025, 7, 1) }
            );
            context.SaveChanges();
        }
    }
}

