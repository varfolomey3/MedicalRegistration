using Microsoft.EntityFrameworkCore;
using MedicalRegistration.Data;
using MedicalRegistration.Models;

namespace MedicalRegistration.Services;

/// <summary>
/// Интерфейс сервиса для работы с визитами
/// </summary>
public interface IVisitService
{
    Task<IEnumerable<Visit>> GetAllVisitsAsync();
    Task<Visit?> GetVisitByIdAsync(int id);
    Task<IEnumerable<Visit>> GetVisitsByPatientIdAsync(int patientId);
    Task<IEnumerable<Visit>> GetVisitsByDateRangeAsync(DateTime startDate, DateTime endDate);
    Task<IEnumerable<Visit>> GetVisitsByStatusAsync(VisitStatus status);
    Task<IEnumerable<Visit>> GetVisitsByDoctorAsync(string doctorName);
    Task<Visit> CreateVisitAsync(Visit visit);
    Task<Visit> UpdateVisitAsync(Visit visit);
    Task<bool> DeleteVisitAsync(int id);
    Task<IEnumerable<Visit>> SearchVisitsAsync(string searchTerm);
    Task<bool> HasPatientVisitsAsync(int patientId);
    Task<int> GetActiveVisitsCountAsync();
    Task<IEnumerable<Visit>> GetRecentVisitsAsync(int count = 10);
}

/// <summary>
/// Сервис для работы с визитами
/// </summary>
public class VisitService : IVisitService
{
    private readonly MedicalDbContext _context;
    private readonly ILogger<VisitService> _logger;

    public VisitService(MedicalDbContext context, ILogger<VisitService> logger)
    {
        _context = context;
        _logger = logger;
    }

    /// <summary>
    /// Получить все визиты
    /// </summary>
    public async Task<IEnumerable<Visit>> GetAllVisitsAsync()
    {
        try
        {
            return await _context.Visits
                .Include(v => v.Patient)
                .Include(v => v.Diagnoses)
                .Include(v => v.Prescriptions)
                .OrderByDescending(v => v.VisitDateTime)
                .ToListAsync();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving all visits");
            throw;
        }
    }

    /// <summary>
    /// Получить визит по ID
    /// </summary>
    public async Task<Visit?> GetVisitByIdAsync(int id)
    {
        try
        {
            return await _context.Visits
                .Include(v => v.Patient)
                .Include(v => v.Diagnoses)
                .Include(v => v.Prescriptions)
                .FirstOrDefaultAsync(v => v.Id == id);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving visit with ID {VisitId}", id);
            throw;
        }
    }

    /// <summary>
    /// Получить визиты пациента
    /// </summary>
    public async Task<IEnumerable<Visit>> GetVisitsByPatientIdAsync(int patientId)
    {
        try
        {
            return await _context.Visits
                .Include(v => v.Diagnoses)
                .Include(v => v.Prescriptions)
                .Where(v => v.PatientId == patientId)
                .OrderByDescending(v => v.VisitDateTime)
                .ToListAsync();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving visits for patient {PatientId}", patientId);
            throw;
        }
    }

    /// <summary>
    /// Получить визиты за период
    /// </summary>
    public async Task<IEnumerable<Visit>> GetVisitsByDateRangeAsync(DateTime startDate, DateTime endDate)
    {
        try
        {
            return await _context.Visits
                .Include(v => v.Patient)
                .Include(v => v.Diagnoses)
                .Include(v => v.Prescriptions)
                .Where(v => v.VisitDateTime.Date >= startDate.Date && v.VisitDateTime.Date <= endDate.Date)
                .OrderByDescending(v => v.VisitDateTime)
                .ToListAsync();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving visits for date range {StartDate} - {EndDate}", startDate, endDate);
            throw;
        }
    }

    /// <summary>
    /// Получить визиты по статусу
    /// </summary>
    public async Task<IEnumerable<Visit>> GetVisitsByStatusAsync(VisitStatus status)
    {
        try
        {
            return await _context.Visits
                .Include(v => v.Patient)
                .Include(v => v.Diagnoses)
                .Include(v => v.Prescriptions)
                .Where(v => v.Status == status)
                .OrderByDescending(v => v.VisitDateTime)
                .ToListAsync();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving visits with status {Status}", status);
            throw;
        }
    }

    /// <summary>
    /// Получить визиты врача
    /// </summary>
    public async Task<IEnumerable<Visit>> GetVisitsByDoctorAsync(string doctorName)
    {
        try
        {
            return await _context.Visits
                .Include(v => v.Patient)
                .Include(v => v.Diagnoses)
                .Include(v => v.Prescriptions)
                .Where(v => v.DoctorName.Contains(doctorName))
                .OrderByDescending(v => v.VisitDateTime)
                .ToListAsync();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving visits for doctor {DoctorName}", doctorName);
            throw;
        }
    }

    /// <summary>
    /// Создать новый визит
    /// </summary>
    public async Task<Visit> CreateVisitAsync(Visit visit)
    {
        try
        {
            // Проверяем, существует ли пациент
            var patientExists = await _context.Patients.AnyAsync(p => p.Id == visit.PatientId);
            if (!patientExists)
            {
                throw new InvalidOperationException($"Пациент с ID {visit.PatientId} не найден");
            }

            // Устанавливаем даты создания и обновления
            visit.CreatedAt = DateTime.UtcNow;
            visit.UpdatedAt = DateTime.UtcNow;

            _context.Visits.Add(visit);
            await _context.SaveChangesAsync();

            _logger.LogInformation("Created new visit {VisitId} for patient {PatientId}", visit.Id, visit.PatientId);
            return visit;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error creating visit for patient {PatientId}", visit.PatientId);
            throw;
        }
    }

    /// <summary>
    /// Обновить визит
    /// </summary>
    public async Task<Visit> UpdateVisitAsync(Visit visit)
    {
        try
        {
            var existingVisit = await _context.Visits.FindAsync(visit.Id);
            if (existingVisit == null)
            {
                throw new InvalidOperationException($"Визит с ID {visit.Id} не найден");
            }

            // Обновляем поля
            existingVisit.VisitDateTime = visit.VisitDateTime;
            existingVisit.DoctorName = visit.DoctorName;
            existingVisit.Specialty = visit.Specialty;
            existingVisit.Complaints = visit.Complaints;
            existingVisit.Examination = visit.Examination;
            existingVisit.Status = visit.Status;
            existingVisit.Cost = visit.Cost;
            existingVisit.Notes = visit.Notes;
            existingVisit.UpdatedAt = DateTime.UtcNow;

            await _context.SaveChangesAsync();

            _logger.LogInformation("Updated visit {VisitId}", visit.Id);
            return existingVisit;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error updating visit {VisitId}", visit.Id);
            throw;
        }
    }

    /// <summary>
    /// Удалить визит
    /// </summary>
    public async Task<bool> DeleteVisitAsync(int id)
    {
        try
        {
            var visit = await _context.Visits
                .Include(v => v.Diagnoses)
                .Include(v => v.Prescriptions)
                .FirstOrDefaultAsync(v => v.Id == id);

            if (visit == null)
            {
                return false;
            }

            // Удаляем связанные диагнозы и назначения (каскадное удаление настроено в контексте)
            _context.Visits.Remove(visit);
            await _context.SaveChangesAsync();

            _logger.LogInformation("Deleted visit {VisitId}", id);
            return true;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error deleting visit {VisitId}", id);
            throw;
        }
    }

    /// <summary>
    /// Поиск визитов
    /// </summary>
    public async Task<IEnumerable<Visit>> SearchVisitsAsync(string searchTerm)
    {
        try
        {
            if (string.IsNullOrWhiteSpace(searchTerm))
            {
                return await GetAllVisitsAsync();
            }

            var lowerSearchTerm = searchTerm.ToLower();

            return await _context.Visits
                .Include(v => v.Patient)
                .Include(v => v.Diagnoses)
                .Include(v => v.Prescriptions)
                .Where(v => 
                    v.DoctorName.ToLower().Contains(lowerSearchTerm) ||
                    (v.Complaints != null && v.Complaints.ToLower().Contains(lowerSearchTerm)) ||
                    v.Patient.LastName.ToLower().Contains(lowerSearchTerm) ||
                    v.Patient.FirstName.ToLower().Contains(lowerSearchTerm) ||
                    (v.Notes != null && v.Notes.ToLower().Contains(lowerSearchTerm)))
                .OrderByDescending(v => v.VisitDateTime)
                .ToListAsync();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error searching visits with term {SearchTerm}", searchTerm);
            throw;
        }
    }

    /// <summary>
    /// Проверить, есть ли у пациента визиты
    /// </summary>
    public async Task<bool> HasPatientVisitsAsync(int patientId)
    {
        try
        {
            return await _context.Visits.AnyAsync(v => v.PatientId == patientId);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error checking if patient {PatientId} has visits", patientId);
            throw;
        }
    }

    /// <summary>
    /// Получить количество активных визитов
    /// </summary>
    public async Task<int> GetActiveVisitsCountAsync()
    {
        try
        {
            return await _context.Visits
                .CountAsync(v => v.Status == VisitStatus.Scheduled || v.Status == VisitStatus.InProgress);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting active visits count");
            throw;
        }
    }

    /// <summary>
    /// Получить последние визиты
    /// </summary>
    public async Task<IEnumerable<Visit>> GetRecentVisitsAsync(int count = 10)
    {
        try
        {
            return await _context.Visits
                .Include(v => v.Patient)
                .OrderByDescending(v => v.VisitDateTime)
                .Take(count)
                .ToListAsync();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting recent visits");
            throw;
        }
    }
} 