using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using MedicalRegistration.Models;

namespace MedicalRegistration.Data;

public static class SeedData
{
    public static async Task SeedAsync(IServiceProvider serviceProvider, ILogger logger)
    {
        using var scope = serviceProvider.CreateScope();
        var context = scope.ServiceProvider.GetRequiredService<MedicalDbContext>();
        var userManager = scope.ServiceProvider.GetRequiredService<UserManager<ApplicationUser>>();
        var roleManager = scope.ServiceProvider.GetRequiredService<RoleManager<IdentityRole>>();
        
        try
        {
            // Создаем роли
            await EnsureRolesAsync(roleManager, logger);
            
            // Создаем тестовых пользователей
            await SeedUsersAsync(userManager, logger);
            
            // Создаем тестовых пациентов
            await SeedPatientsAsync(context, logger);
            
            // Создаем тестовые диагнозы
            await SeedDiagnosesAsync(context, logger);
            
            // Создаем тестовые визиты
            await SeedVisitsAsync(context, logger);
            
            // Создаем тестовые рецепты
            await SeedPrescriptionsAsync(context, logger);
            
            logger.LogInformation("Test data seeded successfully");
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Error seeding test data");
        }
    }
    
    private static async Task EnsureRolesAsync(RoleManager<IdentityRole> roleManager, ILogger logger)
    {
        var roles = new[] { UserRoles.Doctor, UserRoles.Patient };
        
        foreach (var role in roles)
        {
            if (!await roleManager.RoleExistsAsync(role))
            {
                await roleManager.CreateAsync(new IdentityRole(role));
                logger.LogInformation("Created role: {Role}", role);
            }
        }
    }
    
    private static async Task SeedUsersAsync(UserManager<ApplicationUser> userManager, ILogger logger)
    {
        // Создаем врачей
        var doctors = new[]
        {
            new { Email = "doctor1@test.com", FirstName = "Иван", LastName = "Петров", Specialty = "Терапевт", LicenseNumber = "DOC001" },
            new { Email = "doctor2@test.com", FirstName = "Мария", LastName = "Сидорова", Specialty = "Кардиолог", LicenseNumber = "DOC002" },
            new { Email = "doctor3@test.com", FirstName = "Андрей", LastName = "Козлов", Specialty = "Невролог", LicenseNumber = "DOC003" },
            new { Email = "doctor4@test.com", FirstName = "Елена", LastName = "Романова", Specialty = "Хирург", LicenseNumber = "DOC004" },
            new { Email = "doctor5@test.com", FirstName = "Дмитрий", LastName = "Федоров", Specialty = "Эндокринолог", LicenseNumber = "DOC005" }
        };
        
        foreach (var doc in doctors)
        {
            if (await userManager.FindByEmailAsync(doc.Email) == null)
            {
                var user = new ApplicationUser
                {
                    UserName = doc.Email,
                    Email = doc.Email,
                    FirstName = doc.FirstName,
                    LastName = doc.LastName,
                    DateOfBirth = DateTime.Now.AddYears(-40),
                    Gender = Gender.Male,
                    UserType = UserType.Doctor,
                    Specialty = doc.Specialty,
                    LicenseNumber = doc.LicenseNumber,
                    RegisteredAt = new DateTime(DateTime.Now.Year, DateTime.Now.Month, DateTime.Now.Day, DateTime.Now.Hour, DateTime.Now.Minute, 0)
                };
                
                var result = await userManager.CreateAsync(user, "Test123!");
                if (result.Succeeded)
                {
                    await userManager.AddToRoleAsync(user, UserRoles.Doctor);
                    logger.LogInformation("Created doctor: {Email}", doc.Email);
                }
            }
        }
        
        // Создаем пациентов
        var patients = new[]
        {
            new { Email = "patient1@test.com", FirstName = "Анна", LastName = "Иванова", MedicalCard = "PAT001" },
            new { Email = "patient2@test.com", FirstName = "Сергей", LastName = "Петров", MedicalCard = "PAT002" },
            new { Email = "patient3@test.com", FirstName = "Ольга", LastName = "Смирнова", MedicalCard = "PAT003" },
            new { Email = "patient4@test.com", FirstName = "Алексей", LastName = "Волков", MedicalCard = "PAT004" },
            new { Email = "patient5@test.com", FirstName = "Наталья", LastName = "Морозова", MedicalCard = "PAT005" }
        };
        
        foreach (var pat in patients)
        {
            if (await userManager.FindByEmailAsync(pat.Email) == null)
            {
                var user = new ApplicationUser
                {
                    UserName = pat.Email,
                    Email = pat.Email,
                    FirstName = pat.FirstName,
                    LastName = pat.LastName,
                    DateOfBirth = DateTime.Now.AddYears(-30),
                    Gender = Gender.Female,
                    UserType = UserType.Patient,
                    MedicalCardNumber = pat.MedicalCard,
                    RegisteredAt = new DateTime(DateTime.Now.Year, DateTime.Now.Month, DateTime.Now.Day, DateTime.Now.Hour, DateTime.Now.Minute, 0)
                };
                
                var result = await userManager.CreateAsync(user, "Test123!");
                if (result.Succeeded)
                {
                    await userManager.AddToRoleAsync(user, UserRoles.Patient);
                    logger.LogInformation("Created patient: {Email}", pat.Email);
                }
            }
        }
    }
    
    private static async Task SeedPatientsAsync(MedicalDbContext context, ILogger logger)
    {
        if (context.Patients.Any()) return;
        
        var patients = new List<Patient>();
        var random = new Random();
        var firstNames = new[] { "Анна", "Мария", "Елена", "Ольга", "Наталья", "Татьяна", "Светлана", "Ирина", "Людмила", "Галина",
                                "Александр", "Сергей", "Дмитрий", "Андрей", "Алексей", "Владимир", "Игорь", "Николай", "Евгений", "Михаил" };
        var lastNames = new[] { "Иванов", "Петров", "Сидоров", "Смирнов", "Кузнецов", "Попов", "Васильев", "Соколов", "Михайлов", "Новиков",
                               "Федоров", "Морозов", "Волков", "Алексеев", "Лебедев", "Семенов", "Егоров", "Павлов", "Козлов", "Степанов" };
        
        for (int i = 1; i <= 25; i++)
        {
            var firstName = firstNames[random.Next(firstNames.Length)];
            var lastName = lastNames[random.Next(lastNames.Length)];
            var now = new DateTime(DateTime.Now.Year, DateTime.Now.Month, DateTime.Now.Day, DateTime.Now.Hour, DateTime.Now.Minute, 0);
            
            var patient = new Patient
            {
                FirstName = firstName,
                LastName = lastName,
                MiddleName = random.Next(2) == 0 ? "Александрович" : "Владимирович",
                DateOfBirth = DateTime.Now.AddYears(-random.Next(18, 80)),
                Gender = random.Next(2) == 0 ? Gender.Male : Gender.Female,
                PhoneNumber = $"+7(900){random.Next(100, 999)}-{random.Next(10, 99)}-{random.Next(10, 99)}",
                Email = $"{firstName.ToLower()}.{lastName.ToLower()}@email.com",
                Address = $"г. Москва, ул. {lastNames[random.Next(lastNames.Length)]}, д. {random.Next(1, 100)}",
                MedicalCardNumber = $"PAT{i:D3}",
                CreatedAt = now,
                UpdatedAt = now
            };
            
            patients.Add(patient);
        }
        
        context.Patients.AddRange(patients);
        await context.SaveChangesAsync();
        logger.LogInformation("Added {Count} test patients", patients.Count);
    }
    
    private static async Task SeedDiagnosesAsync(MedicalDbContext context, ILogger logger)
    {
        // Диагнозы создаются вместе с визитами, так как у них есть связь с VisitId
        // Поэтому этот метод пока пустой
        await Task.CompletedTask;
        logger.LogInformation("Diagnoses will be created with visits");
    }
    
    private static async Task SeedDiagnosesForVisitsAsync(MedicalDbContext context, ILogger logger)
    {
        if (context.Diagnoses.Any()) return;
        
        var visits = await context.Visits.ToListAsync();
        if (!visits.Any()) return;
        
        var diagnosisData = new[]
        {
            new { Code = "J06.9", Name = "Острая инфекция верхних дыхательных путей неуточненная" },
            new { Code = "I10", Name = "Эссенциальная гипертензия" },
            new { Code = "K29.5", Name = "Хронический гастрит неуточненный" },
            new { Code = "M79.1", Name = "Миалгия" },
            new { Code = "R50.9", Name = "Лихорадка неуточненная" },
            new { Code = "H52.1", Name = "Миопия" },
            new { Code = "J00", Name = "Острый назофарингит" },
            new { Code = "I25.9", Name = "Хроническая ишемическая болезнь сердца неуточненная" },
            new { Code = "E11.9", Name = "Сахарный диабет 2 типа без осложнений" },
            new { Code = "M54.5", Name = "Боль в пояснице" },
            new { Code = "F32.9", Name = "Депрессивный эпизод неуточненный" },
            new { Code = "L20.9", Name = "Атопический дерматит неуточненный" },
            new { Code = "N39.0", Name = "Инфекция мочевыводящих путей" },
            new { Code = "G43.9", Name = "Мигрень неуточненная" },
            new { Code = "J45.9", Name = "Астма неуточненная" },
            new { Code = "K21.9", Name = "Гастроэзофагеальная рефлюксная болезнь" },
            new { Code = "I48", Name = "Фибрилляция и трепетание предсердий" },
            new { Code = "M16.9", Name = "Коксартроз неуточненный" },
            new { Code = "E78.5", Name = "Гиперлипидемия неуточненная" },
            new { Code = "F41.9", Name = "Тревожное расстройство неуточненное" }
        };
        
        var diagnoses = new List<Diagnosis>();
        var random = new Random();
        var now = new DateTime(DateTime.Now.Year, DateTime.Now.Month, DateTime.Now.Day, DateTime.Now.Hour, DateTime.Now.Minute, 0);
        
        // Создаем диагнозы для 20 случайных визитов
        var selectedVisits = visits.OrderBy(x => random.Next()).Take(20).ToList();
        
        foreach (var visit in selectedVisits)
        {
            var diagData = diagnosisData[random.Next(diagnosisData.Length)];
            
            var diagnosis = new Diagnosis
            {
                VisitId = visit.Id,
                IcdCode = diagData.Code,
                Name = diagData.Name,
                Description = $"Подробное описание диагноза {diagData.Name}",
                Type = (DiagnosisType)random.Next(1, 5),
                Severity = (SeverityLevel)random.Next(1, 5),
                DiagnosisDate = new DateTime(visit.VisitDateTime.Year, visit.VisitDateTime.Month, visit.VisitDateTime.Day, 0, 0, 0),
                Notes = "Дополнительные заметки к диагнозу",
                CreatedAt = now,
                UpdatedAt = now
            };
            
            diagnoses.Add(diagnosis);
        }
        
        context.Diagnoses.AddRange(diagnoses);
        await context.SaveChangesAsync();
        logger.LogInformation("Added {Count} test diagnoses for visits", diagnoses.Count);
    }
    
    private static async Task SeedVisitsAsync(MedicalDbContext context, ILogger logger)
    {
        if (context.Visits.Any()) return;
        
        var patients = await context.Patients.ToListAsync();
        
        if (!patients.Any()) return;
        
        var visits = new List<Visit>();
        var random = new Random();
        var doctors = new[] { "Петров И.И.", "Сидорова М.А.", "Козлов А.В.", "Романова Е.С.", "Федоров Д.Н." };
        var specialties = new[] { "Терапевт", "Кардиолог", "Невролог", "Хирург", "Эндокринолог" };
        var complaints = new[]
        {
            "Боль в груди, одышка при физической нагрузке",
            "Головная боль, головокружение",
            "Боль в животе, тошнота",
            "Повышенная температура, общая слабость",
            "Боль в спине, ограничение движений",
            "Кашель, затрудненное дыхание",
            "Боль в суставах, отечность",
            "Нарушение сна, повышенная тревожность"
        };
        
        for (int i = 0; i < 30; i++)
        {
            var patient = patients[random.Next(patients.Count)];
            var visitDate = DateTime.Now.AddDays(-random.Next(1, 90));
            var now = new DateTime(DateTime.Now.Year, DateTime.Now.Month, DateTime.Now.Day, DateTime.Now.Hour, DateTime.Now.Minute, 0);
            
            var visit = new Visit
            {
                PatientId = patient.Id,
                VisitDateTime = new DateTime(visitDate.Year, visitDate.Month, visitDate.Day, visitDate.Hour, visitDate.Minute, 0),
                DoctorName = doctors[random.Next(doctors.Length)],
                Specialty = specialties[random.Next(specialties.Length)],
                Complaints = complaints[random.Next(complaints.Length)],
                Examination = "Проведен осмотр пациента. Выявлены характерные симптомы. Рекомендовано дополнительное обследование.",
                Status = (VisitStatus)random.Next(0, 5),
                Cost = random.Next(1000, 5000),
                Notes = "Дополнительные заметки о визите",
                CreatedAt = now,
                UpdatedAt = now
            };
            
            visits.Add(visit);
        }
        
        context.Visits.AddRange(visits);
        await context.SaveChangesAsync();
        
        // Создаем диагнозы для некоторых визитов
        await SeedDiagnosesForVisitsAsync(context, logger);
        
        logger.LogInformation("Added {Count} test visits", visits.Count);
    }
    
    private static async Task SeedPrescriptionsAsync(MedicalDbContext context, ILogger logger)
    {
        if (context.Prescriptions.Any()) return;
        
        var visits = await context.Visits.Include(v => v.Patient).ToListAsync();
        if (!visits.Any()) return;
        
        var prescriptions = new List<Prescription>();
        var random = new Random();
        var medications = new[]
        {
            "Аспирин 100мг", "Лизиноприл 10мг", "Метформин 850мг", "Амоксициллин 500мг",
            "Ибупрофен 400мг", "Симвастатин 20мг", "Омепразол 20мг", "Лозартан 50мг",
            "Диклофенак 50мг", "Парацетамол 500мг", "Эналаприл 5мг", "Кларитромицин 250мг",
            "Мелоксикам 15мг", "Бисопролол 5мг", "Пантопразол 40мг", "Амлодипин 5мг",
            "Цефтриаксон 1г", "Преднизолон 5мг", "Фуросемид 40мг", "Спиронолактон 25мг"
        };
        
        var dosages = new[] { "1 таб. 1 раз в день", "1 таб. 2 раза в день", "1 таб. 3 раза в день", "2 таб. 2 раза в день" };
        var durations = new[] { "5 дней", "7 дней", "10 дней", "14 дней", "1 месяц" };
        
        for (int i = 0; i < 25; i++)
        {
            var visit = visits[random.Next(visits.Count)];
            var now = new DateTime(DateTime.Now.Year, DateTime.Now.Month, DateTime.Now.Day, DateTime.Now.Hour, DateTime.Now.Minute, 0);
            
            var prescription = new Prescription
            {
                VisitId = visit.Id,
                Type = PrescriptionType.Medication,
                Name = medications[random.Next(medications.Length)],
                Dosage = dosages[random.Next(dosages.Length)],
                Duration = durations[random.Next(durations.Length)],
                Instructions = "Принимать во время еды, запивать водой",
                StartDate = new DateTime(visit.VisitDateTime.Year, visit.VisitDateTime.Month, visit.VisitDateTime.Day, 0, 0, 0),
                EndDate = visit.VisitDateTime.AddDays(random.Next(5, 30)),
                Status = (PrescriptionStatus)random.Next(1, 4),
                Priority = Priority.Normal,
                Notes = "Контролировать самочувствие",
                CreatedAt = now,
                UpdatedAt = now
            };
            
            prescriptions.Add(prescription);
        }
        
        context.Prescriptions.AddRange(prescriptions);
        await context.SaveChangesAsync();
        logger.LogInformation("Added {Count} test prescriptions", prescriptions.Count);
    }
} 