using Microsoft.EntityFrameworkCore;
using MedicalRegistration.Data;
using MedicalRegistration.Models;

namespace MedicalRegistration.Services;

/// <summary>
/// Сервис для работы с назначениями/рецептами
/// </summary>
public class PrescriptionService : IPrescriptionService
{
    private readonly MedicalDbContext _context;
    private readonly ILogger<PrescriptionService> _logger;

    public PrescriptionService(MedicalDbContext context, ILogger<PrescriptionService> logger)
    {
        _context = context;
        _logger = logger;
    }

    /// <summary>
    /// Получить все назначения
    /// </summary>
    public async Task<IEnumerable<Prescription>> GetAllPrescriptionsAsync()
    {
        try
        {
            return await _context.Prescriptions
                .Include(p => p.Visit)
                .ThenInclude(v => v.Patient)
                .OrderByDescending(p => p.StartDate)
                .ToListAsync();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving all prescriptions");
            throw;
        }
    }

    /// <summary>
    /// Получить назначение по ID
    /// </summary>
    public async Task<Prescription?> GetPrescriptionByIdAsync(int id)
    {
        try
        {
            return await _context.Prescriptions
                .Include(p => p.Visit)
                .ThenInclude(v => v.Patient)
                .FirstOrDefaultAsync(p => p.Id == id);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving prescription with ID {PrescriptionId}", id);
            throw;
        }
    }

    /// <summary>
    /// Получить назначения пациента
    /// </summary>
    public async Task<IEnumerable<Prescription>> GetPrescriptionsByPatientIdAsync(int patientId)
    {
        try
        {
            return await _context.Prescriptions
                .Include(p => p.Visit)
                .ThenInclude(v => v.Patient)
                .Where(p => p.Visit.PatientId == patientId)
                .OrderByDescending(p => p.StartDate)
                .ToListAsync();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving prescriptions for patient {PatientId}", patientId);
            throw;
        }
    }

    /// <summary>
    /// Получить назначения по визиту
    /// </summary>
    public async Task<IEnumerable<Prescription>> GetPrescriptionsByVisitIdAsync(int visitId)
    {
        try
        {
            return await _context.Prescriptions
                .Include(p => p.Visit)
                .ThenInclude(v => v.Patient)
                .Where(p => p.VisitId == visitId)
                .OrderByDescending(p => p.StartDate)
                .ToListAsync();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving prescriptions for visit {VisitId}", visitId);
            throw;
        }
    }

    /// <summary>
    /// Получить назначения по типу
    /// </summary>
    public async Task<IEnumerable<Prescription>> GetPrescriptionsByTypeAsync(PrescriptionType type)
    {
        try
        {
            return await _context.Prescriptions
                .Include(p => p.Visit)
                .ThenInclude(v => v.Patient)
                .Where(p => p.Type == type)
                .OrderByDescending(p => p.StartDate)
                .ToListAsync();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving prescriptions with type {Type}", type);
            throw;
        }
    }

    /// <summary>
    /// Получить назначения по статусу
    /// </summary>
    public async Task<IEnumerable<Prescription>> GetPrescriptionsByStatusAsync(PrescriptionStatus status)
    {
        try
        {
            return await _context.Prescriptions
                .Include(p => p.Visit)
                .ThenInclude(v => v.Patient)
                .Where(p => p.Status == status)
                .OrderByDescending(p => p.StartDate)
                .ToListAsync();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving prescriptions with status {Status}", status);
            throw;
        }
    }

    /// <summary>
    /// Получить назначения по приоритету
    /// </summary>
    public async Task<IEnumerable<Prescription>> GetPrescriptionsByPriorityAsync(Priority priority)
    {
        try
        {
            return await _context.Prescriptions
                .Include(p => p.Visit)
                .ThenInclude(v => v.Patient)
                .Where(p => p.Priority == priority)
                .OrderByDescending(p => p.StartDate)
                .ToListAsync();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving prescriptions with priority {Priority}", priority);
            throw;
        }
    }

    /// <summary>
    /// Получить активные назначения пациента
    /// </summary>
    public async Task<IEnumerable<Prescription>> GetActivePrescriptionsByPatientIdAsync(int patientId)
    {
        try
        {
            return await _context.Prescriptions
                .Include(p => p.Visit)
                .ThenInclude(v => v.Patient)
                .Where(p => p.Visit.PatientId == patientId && p.Status == PrescriptionStatus.Active)
                .OrderByDescending(p => p.StartDate)
                .ToListAsync();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving active prescriptions for patient {PatientId}", patientId);
            throw;
        }
    }

    /// <summary>
    /// Получить назначения за период
    /// </summary>
    public async Task<IEnumerable<Prescription>> GetPrescriptionsByDateRangeAsync(DateTime startDate, DateTime endDate)
    {
        try
        {
            return await _context.Prescriptions
                .Include(p => p.Visit)
                .ThenInclude(v => v.Patient)
                .Where(p => p.StartDate.Date >= startDate.Date && p.StartDate.Date <= endDate.Date)
                .OrderByDescending(p => p.StartDate)
                .ToListAsync();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving prescriptions for date range {StartDate} - {EndDate}", startDate, endDate);
            throw;
        }
    }

    /// <summary>
    /// Получить просроченные назначения
    /// </summary>
    public async Task<IEnumerable<Prescription>> GetExpiredPrescriptionsAsync()
    {
        try
        {
            var currentDate = DateTime.Now.Date;
            return await _context.Prescriptions
                .Include(p => p.Visit)
                .ThenInclude(v => v.Patient)
                .Where(p => p.EndDate.HasValue && p.EndDate.Value.Date < currentDate && p.Status == PrescriptionStatus.Active)
                .OrderByDescending(p => p.EndDate)
                .ToListAsync();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving expired prescriptions");
            throw;
        }
    }

    /// <summary>
    /// Создать новое назначение
    /// </summary>
    public async Task<Prescription> CreatePrescriptionAsync(Prescription prescription)
    {
        try
        {
            // Проверяем валидность назначения
            var isValid = await ValidatePrescriptionAsync(prescription);
            if (!isValid)
            {
                throw new InvalidOperationException("Данные назначения не прошли валидацию");
            }

            // Проверяем, существует ли визит
            var visitExists = await _context.Visits.AnyAsync(v => v.Id == prescription.VisitId);
            if (!visitExists)
            {
                throw new InvalidOperationException($"Визит с ID {prescription.VisitId} не найден");
            }

            // Устанавливаем даты создания и обновления
            var now = DateTime.Now;
            var currentTime = new DateTime(now.Year, now.Month, now.Day, now.Hour, now.Minute, now.Second);
            prescription.CreatedAt = currentTime;
            prescription.UpdatedAt = currentTime;

            _context.Prescriptions.Add(prescription);
            await _context.SaveChangesAsync();

            _logger.LogInformation("Created new prescription {PrescriptionId} for visit {VisitId}", prescription.Id, prescription.VisitId);
            return prescription;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error creating prescription for visit {VisitId}", prescription.VisitId);
            throw;
        }
    }

    /// <summary>
    /// Обновить назначение
    /// </summary>
    public async Task<Prescription> UpdatePrescriptionAsync(Prescription prescription)
    {
        try
        {
            var existingPrescription = await _context.Prescriptions.FindAsync(prescription.Id);
            if (existingPrescription == null)
            {
                throw new InvalidOperationException($"Назначение с ID {prescription.Id} не найдено");
            }

            // Проверяем валидность обновленных данных
            var isValid = await ValidatePrescriptionAsync(prescription);
            if (!isValid)
            {
                throw new InvalidOperationException("Обновленные данные назначения не прошли валидацию");
            }

            // Обновляем поля
            existingPrescription.Type = prescription.Type;
            existingPrescription.Name = prescription.Name;
            existingPrescription.Dosage = prescription.Dosage;
            existingPrescription.Frequency = prescription.Frequency;
            existingPrescription.Duration = prescription.Duration;
            existingPrescription.Instructions = prescription.Instructions;
            existingPrescription.StartDate = prescription.StartDate;
            existingPrescription.EndDate = prescription.EndDate;
            existingPrescription.Status = prescription.Status;
            existingPrescription.Priority = prescription.Priority;
            existingPrescription.Notes = prescription.Notes;
            var now = DateTime.Now;
            existingPrescription.UpdatedAt = new DateTime(now.Year, now.Month, now.Day, now.Hour, now.Minute, now.Second);

            await _context.SaveChangesAsync();

            _logger.LogInformation("Updated prescription {PrescriptionId}", prescription.Id);
            return existingPrescription;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error updating prescription {PrescriptionId}", prescription.Id);
            throw;
        }
    }

    /// <summary>
    /// Удалить назначение
    /// </summary>
    public async Task<bool> DeletePrescriptionAsync(int id)
    {
        try
        {
            var prescription = await _context.Prescriptions.FindAsync(id);
            if (prescription == null)
            {
                _logger.LogWarning("Attempt to delete non-existent prescription {PrescriptionId}", id);
                return false;
            }

            _context.Prescriptions.Remove(prescription);
            await _context.SaveChangesAsync();

            _logger.LogInformation("Deleted prescription {PrescriptionId}", id);
            return true;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error deleting prescription {PrescriptionId}", id);
            throw;
        }
    }

    /// <summary>
    /// Поиск назначений
    /// </summary>
    public async Task<IEnumerable<Prescription>> SearchPrescriptionsAsync(string searchTerm)
    {
        try
        {
            if (string.IsNullOrWhiteSpace(searchTerm))
            {
                return await GetAllPrescriptionsAsync();
            }

            var term = searchTerm.ToLower().Trim();

            return await _context.Prescriptions
                .Include(p => p.Visit)
                .ThenInclude(v => v.Patient)
                .Where(p => 
                    p.Name.ToLower().Contains(term) ||
                    (p.Dosage != null && p.Dosage.ToLower().Contains(term)) ||
                    (p.Frequency != null && p.Frequency.ToLower().Contains(term)) ||
                    (p.Instructions != null && p.Instructions.ToLower().Contains(term)) ||
                    (p.Notes != null && p.Notes.ToLower().Contains(term)) ||
                    p.Visit.Patient.LastName.ToLower().Contains(term) ||
                    p.Visit.Patient.FirstName.ToLower().Contains(term))
                .OrderByDescending(p => p.StartDate)
                .ToListAsync();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error searching prescriptions with term '{SearchTerm}'", searchTerm);
            throw;
        }
    }

    /// <summary>
    /// Обновить статус назначения
    /// </summary>
    public async Task<bool> UpdatePrescriptionStatusAsync(int id, PrescriptionStatus status)
    {
        try
        {
            var prescription = await _context.Prescriptions.FindAsync(id);
            if (prescription == null)
            {
                _logger.LogWarning("Attempt to update status of non-existent prescription {PrescriptionId}", id);
                return false;
            }

            prescription.Status = status;
            var now = DateTime.Now;
            prescription.UpdatedAt = new DateTime(now.Year, now.Month, now.Day, now.Hour, now.Minute, now.Second);

            await _context.SaveChangesAsync();

            _logger.LogInformation("Updated prescription {PrescriptionId} status to {Status}", id, status);
            return true;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error updating prescription {PrescriptionId} status", id);
            throw;
        }
    }

    /// <summary>
    /// Получить статистику назначений
    /// </summary>
    public async Task<PrescriptionStatistics> GetPrescriptionStatisticsAsync()
    {
        try
        {
            var prescriptions = await _context.Prescriptions.ToListAsync();

            var statistics = new PrescriptionStatistics
            {
                TotalPrescriptions = prescriptions.Count,
                ActivePrescriptions = prescriptions.Count(p => p.Status == PrescriptionStatus.Active),
                CompletedPrescriptions = prescriptions.Count(p => p.Status == PrescriptionStatus.Completed),
                CancelledPrescriptions = prescriptions.Count(p => p.Status == PrescriptionStatus.Cancelled),
                ExpiredPrescriptions = prescriptions.Count(p => p.Status == PrescriptionStatus.Expired)
            };

            // Статистика по типам
            foreach (PrescriptionType type in Enum.GetValues<PrescriptionType>())
            {
                statistics.PrescriptionsByType[type] = prescriptions.Count(p => p.Type == type);
            }

            // Статистика по приоритетам
            foreach (Priority priority in Enum.GetValues<Priority>())
            {
                statistics.PrescriptionsByPriority[priority] = prescriptions.Count(p => p.Priority == priority);
            }

            _logger.LogInformation("Generated prescription statistics: {TotalCount} total prescriptions", statistics.TotalPrescriptions);
            return statistics;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error generating prescription statistics");
            throw;
        }
    }

    /// <summary>
    /// Проверить валидность назначения
    /// </summary>
    public async Task<bool> ValidatePrescriptionAsync(Prescription prescription)
    {
        try
        {
            // Базовая валидация
            if (string.IsNullOrWhiteSpace(prescription.Name))
                return false;

            if (prescription.VisitId <= 0)
                return false;

            // Проверяем, что дата начала не в будущем более чем на год
            if (prescription.StartDate > DateTime.Now.AddYears(1))
                return false;

            // Проверяем, что дата окончания больше даты начала
            if (prescription.EndDate.HasValue && prescription.EndDate <= prescription.StartDate)
                return false;

            // Проверяем существование визита
            var visitExists = await _context.Visits.AnyAsync(v => v.Id == prescription.VisitId);
            if (!visitExists)
                return false;

            _logger.LogInformation("Prescription validation passed for visit {VisitId}", prescription.VisitId);
            return true;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error validating prescription for visit {VisitId}", prescription.VisitId);
            return false;
        }
    }
} 