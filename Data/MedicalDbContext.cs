using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using MedicalRegistration.Models;

namespace MedicalRegistration.Data;

/// <summary>
/// Контекст базы данных для медицинской системы
/// </summary>
public class MedicalDbContext : IdentityDbContext<ApplicationUser>
{
    public MedicalDbContext(DbContextOptions<MedicalDbContext> options) : base(options)
    {
    }

    /// <summary>
    /// Таблица пациентов
    /// </summary>
    public DbSet<Patient> Patients { get; set; }

    /// <summary>
    /// Таблица визитов
    /// </summary>
    public DbSet<Visit> Visits { get; set; }

    /// <summary>
    /// Таблица диагнозов
    /// </summary>
    public DbSet<Diagnosis> Diagnoses { get; set; }

    /// <summary>
    /// Таблица назначений
    /// </summary>
    public DbSet<Prescription> Prescriptions { get; set; }

    /// <summary>
    /// Таблица медицинских отчетов
    /// </summary>
    public DbSet<MedicalReport> MedicalReports { get; set; }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        // Конфигурация модели Patient
        modelBuilder.Entity<Patient>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.Property(e => e.LastName).IsRequired().HasMaxLength(100);
            entity.Property(e => e.FirstName).IsRequired().HasMaxLength(100);
            entity.Property(e => e.MiddleName).HasMaxLength(100);
            entity.Property(e => e.PhoneNumber).HasMaxLength(15);
            entity.Property(e => e.Email).HasMaxLength(100);
            entity.Property(e => e.Address).HasMaxLength(300);
            entity.Property(e => e.MedicalCardNumber).IsRequired().HasMaxLength(20);
            
            // Уникальный индекс для номера медицинской карты
            entity.HasIndex(e => e.MedicalCardNumber).IsUnique();
            
            // Индексы для быстрого поиска
            entity.HasIndex(e => e.LastName);
            entity.HasIndex(e => new { e.LastName, e.FirstName });
            
            // Конфигурация связей
            entity.HasMany(e => e.Visits)
                  .WithOne(e => e.Patient)
                  .HasForeignKey(e => e.PatientId)
                  .OnDelete(DeleteBehavior.Cascade);
        });

        // Конфигурация модели Visit
        modelBuilder.Entity<Visit>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.Property(e => e.DoctorName).IsRequired().HasMaxLength(200);
            entity.Property(e => e.Specialty).IsRequired().HasMaxLength(100);
            entity.Property(e => e.Complaints).HasMaxLength(1000);
            entity.Property(e => e.Examination).HasMaxLength(2000);
            entity.Property(e => e.Notes).HasMaxLength(500);
            entity.Property(e => e.Cost).HasColumnType("decimal(10,2)");
            
            // Индексы
            entity.HasIndex(e => e.VisitDateTime);
            entity.HasIndex(e => e.PatientId);
            entity.HasIndex(e => e.DoctorName);
            entity.HasIndex(e => e.Status);
            
            // Конфигурация связей
            entity.HasMany(e => e.Diagnoses)
                  .WithOne(e => e.Visit)
                  .HasForeignKey(e => e.VisitId)
                  .OnDelete(DeleteBehavior.Cascade);
                  
            entity.HasMany(e => e.Prescriptions)
                  .WithOne(e => e.Visit)
                  .HasForeignKey(e => e.VisitId)
                  .OnDelete(DeleteBehavior.Cascade);
        });

        // Конфигурация модели Diagnosis
        modelBuilder.Entity<Diagnosis>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.Property(e => e.IcdCode).HasMaxLength(10);
            entity.Property(e => e.Name).IsRequired().HasMaxLength(500);
            entity.Property(e => e.Description).HasMaxLength(2000);
            entity.Property(e => e.Notes).HasMaxLength(1000);
            
            // Индексы
            entity.HasIndex(e => e.VisitId);
            entity.HasIndex(e => e.IcdCode);
            entity.HasIndex(e => e.Type);
            entity.HasIndex(e => e.DiagnosisDate);
        });

        // Конфигурация модели Prescription
        modelBuilder.Entity<Prescription>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.Property(e => e.Name).IsRequired().HasMaxLength(300);
            entity.Property(e => e.Dosage).HasMaxLength(100);
            entity.Property(e => e.Frequency).HasMaxLength(200);
            entity.Property(e => e.Duration).HasMaxLength(100);
            entity.Property(e => e.Instructions).HasMaxLength(1000);
            entity.Property(e => e.Notes).HasMaxLength(500);
            
            // Индексы
            entity.HasIndex(e => e.VisitId);
            entity.HasIndex(e => e.Type);
            entity.HasIndex(e => e.Status);
            entity.HasIndex(e => e.StartDate);
            entity.HasIndex(e => e.Priority);
        });

        // Конфигурация модели MedicalReport
        modelBuilder.Entity<MedicalReport>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.Property(e => e.Title).IsRequired().HasMaxLength(200);
            entity.Property(e => e.DoctorName).IsRequired().HasMaxLength(200);
            entity.Property(e => e.DoctorSpecialty).HasMaxLength(100);
            entity.Property(e => e.Description).HasMaxLength(1000);
            entity.Property(e => e.Content).HasColumnType("TEXT");
            entity.Property(e => e.Statistics).HasColumnType("TEXT");
            entity.Property(e => e.Conclusions).HasMaxLength(2000);
            
            // Индексы
            entity.HasIndex(e => e.Type);
            entity.HasIndex(e => e.Status);
            entity.HasIndex(e => e.CreatedAt);
            entity.HasIndex(e => e.PeriodStart);
            entity.HasIndex(e => e.PeriodEnd);
            entity.HasIndex(e => e.DoctorName);
        });

        // Настройка автоматического обновления полей аудита
        foreach (var entityType in modelBuilder.Model.GetEntityTypes())
        {
            var properties = entityType.ClrType.GetProperties()
                .Where(p => p.Name == "CreatedAt" || p.Name == "UpdatedAt")
                .Where(p => p.PropertyType == typeof(DateTime) || p.PropertyType == typeof(DateTime?));

            foreach (var property in properties)
            {
                modelBuilder.Entity(entityType.ClrType)
                    .Property(property.Name)
                    .HasDefaultValueSql("datetime('now')");
            }
        }
    }

    /// <summary>
    /// Переопределение SaveChanges для автоматического обновления аудитных полей
    /// </summary>
    public override int SaveChanges()
    {
        UpdateAuditFields();
        return base.SaveChanges();
    }

    /// <summary>
    /// Переопределение SaveChangesAsync для автоматического обновления аудитных полей
    /// </summary>
    public override Task<int> SaveChangesAsync(CancellationToken cancellationToken = default)
    {
        UpdateAuditFields();
        return base.SaveChangesAsync(cancellationToken);
    }

    /// <summary>
    /// Обновление аудитных полей при сохранении
    /// </summary>
    private void UpdateAuditFields()
    {
        var entries = ChangeTracker.Entries()
            .Where(e => e.State == EntityState.Added || e.State == EntityState.Modified);

        foreach (var entry in entries)
        {
            var now = DateTime.UtcNow;

            if (entry.State == EntityState.Added)
            {
                if (entry.Entity.GetType().GetProperty("CreatedAt") != null)
                {
                    entry.Property("CreatedAt").CurrentValue = now;
                }
            }

            if (entry.Entity.GetType().GetProperty("UpdatedAt") != null)
            {
                entry.Property("UpdatedAt").CurrentValue = now;
            }
        }
    }
} 