using Microsoft.EntityFrameworkCore;
using MedicalRegistration.Data;
using MedicalRegistration.Models;

namespace MedicalRegistration.Services;

/// <summary>
/// Интерфейс сервиса для работы с диагнозами
/// </summary>
public interface IDiagnosisService
{
    Task<IEnumerable<Diagnosis>> GetAllDiagnosesAsync();
    Task<Diagnosis?> GetDiagnosisByIdAsync(int id);
    Task<IEnumerable<Diagnosis>> GetDiagnosesByVisitIdAsync(int visitId);
    Task<IEnumerable<Diagnosis>> GetDiagnosesByDateRangeAsync(DateTime startDate, DateTime endDate);
    Task<IEnumerable<Diagnosis>> GetDiagnosesByTypeAsync(DiagnosisType type);
    Task<IEnumerable<Diagnosis>> GetDiagnosesByIcdCodeAsync(string icdCode);
    Task<Diagnosis> CreateDiagnosisAsync(Diagnosis diagnosis);
    Task<Diagnosis> UpdateDiagnosisAsync(Diagnosis diagnosis);
    Task<bool> DeleteDiagnosisAsync(int id);
    Task<IEnumerable<Diagnosis>> SearchDiagnosesAsync(string searchTerm);
    Task<bool> HasVisitDiagnosesAsync(int visitId);
    Task<int> GetDiagnosesCountAsync();
    Task<IEnumerable<Diagnosis>> GetRecentDiagnosesAsync(int count = 10);
}

/// <summary>
/// Сервис для работы с диагнозами
/// </summary>
public class DiagnosisService : IDiagnosisService
{
    private readonly MedicalDbContext _context;
    private readonly ILogger<DiagnosisService> _logger;

    public DiagnosisService(MedicalDbContext context, ILogger<DiagnosisService> logger)
    {
        _context = context;
        _logger = logger;
    }

    /// <summary>
    /// Получить все диагнозы
    /// </summary>
    public async Task<IEnumerable<Diagnosis>> GetAllDiagnosesAsync()
    {
        try
        {
            return await _context.Diagnoses
                .Include(d => d.Visit)
                .ThenInclude(v => v.Patient)
                .OrderByDescending(d => d.DiagnosisDate)
                .ToListAsync();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving all diagnoses");
            throw;
        }
    }

    /// <summary>
    /// Получить диагноз по ID
    /// </summary>
    public async Task<Diagnosis?> GetDiagnosisByIdAsync(int id)
    {
        try
        {
            return await _context.Diagnoses
                .Include(d => d.Visit)
                .ThenInclude(v => v.Patient)
                .FirstOrDefaultAsync(d => d.Id == id);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving diagnosis with ID {DiagnosisId}", id);
            throw;
        }
    }

    /// <summary>
    /// Получить диагнозы визита
    /// </summary>
    public async Task<IEnumerable<Diagnosis>> GetDiagnosesByVisitIdAsync(int visitId)
    {
        try
        {
            return await _context.Diagnoses
                .Include(d => d.Visit)
                .ThenInclude(v => v.Patient)
                .Where(d => d.VisitId == visitId)
                .OrderByDescending(d => d.DiagnosisDate)
                .ToListAsync();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving diagnoses for visit {VisitId}", visitId);
            throw;
        }
    }

    /// <summary>
    /// Получить диагнозы за период
    /// </summary>
    public async Task<IEnumerable<Diagnosis>> GetDiagnosesByDateRangeAsync(DateTime startDate, DateTime endDate)
    {
        try
        {
            return await _context.Diagnoses
                .Include(d => d.Visit)
                .ThenInclude(v => v.Patient)
                .Where(d => d.DiagnosisDate.Date >= startDate.Date && d.DiagnosisDate.Date <= endDate.Date)
                .OrderByDescending(d => d.DiagnosisDate)
                .ToListAsync();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving diagnoses for date range {StartDate} - {EndDate}", startDate, endDate);
            throw;
        }
    }

    /// <summary>
    /// Получить диагнозы по типу
    /// </summary>
    public async Task<IEnumerable<Diagnosis>> GetDiagnosesByTypeAsync(DiagnosisType type)
    {
        try
        {
            return await _context.Diagnoses
                .Include(d => d.Visit)
                .ThenInclude(v => v.Patient)
                .Where(d => d.Type == type)
                .OrderByDescending(d => d.DiagnosisDate)
                .ToListAsync();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving diagnoses with type {Type}", type);
            throw;
        }
    }

    /// <summary>
    /// Получить диагнозы по коду МКБ-10
    /// </summary>
    public async Task<IEnumerable<Diagnosis>> GetDiagnosesByIcdCodeAsync(string icdCode)
    {
        try
        {
            return await _context.Diagnoses
                .Include(d => d.Visit)
                .ThenInclude(v => v.Patient)
                .Where(d => d.IcdCode != null && d.IcdCode.Contains(icdCode))
                .OrderByDescending(d => d.DiagnosisDate)
                .ToListAsync();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving diagnoses with ICD code {IcdCode}", icdCode);
            throw;
        }
    }

    /// <summary>
    /// Создать новый диагноз
    /// </summary>
    public async Task<Diagnosis> CreateDiagnosisAsync(Diagnosis diagnosis)
    {
        try
        {
            // Проверяем, существует ли визит
            var visitExists = await _context.Visits.AnyAsync(v => v.Id == diagnosis.VisitId);
            if (!visitExists)
            {
                throw new InvalidOperationException($"Визит с ID {diagnosis.VisitId} не найден");
            }

            // Устанавливаем даты создания и обновления
            var now = DateTime.Now;
            var currentTime = new DateTime(now.Year, now.Month, now.Day, now.Hour, now.Minute, now.Second);
            diagnosis.CreatedAt = currentTime;
            diagnosis.UpdatedAt = currentTime;

            _context.Diagnoses.Add(diagnosis);
            await _context.SaveChangesAsync();

            _logger.LogInformation("Created new diagnosis {DiagnosisId} for visit {VisitId}", diagnosis.Id, diagnosis.VisitId);
            return diagnosis;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error creating diagnosis for visit {VisitId}", diagnosis.VisitId);
            throw;
        }
    }

    /// <summary>
    /// Обновить диагноз
    /// </summary>
    public async Task<Diagnosis> UpdateDiagnosisAsync(Diagnosis diagnosis)
    {
        try
        {
            var existingDiagnosis = await _context.Diagnoses.FindAsync(diagnosis.Id);
            if (existingDiagnosis == null)
            {
                throw new InvalidOperationException($"Диагноз с ID {diagnosis.Id} не найден");
            }

            // Проверяем, существует ли визит
            var visitExists = await _context.Visits.AnyAsync(v => v.Id == diagnosis.VisitId);
            if (!visitExists)
            {
                throw new InvalidOperationException($"Визит с ID {diagnosis.VisitId} не найден");
            }

            // Обновляем поля
            existingDiagnosis.VisitId = diagnosis.VisitId;
            existingDiagnosis.IcdCode = diagnosis.IcdCode;
            existingDiagnosis.Name = diagnosis.Name;
            existingDiagnosis.Description = diagnosis.Description;
            existingDiagnosis.Type = diagnosis.Type;
            existingDiagnosis.Severity = diagnosis.Severity;
            existingDiagnosis.DiagnosisDate = diagnosis.DiagnosisDate;
            existingDiagnosis.Notes = diagnosis.Notes;
            var now = DateTime.Now;
            existingDiagnosis.UpdatedAt = new DateTime(now.Year, now.Month, now.Day, now.Hour, now.Minute, now.Second);

            await _context.SaveChangesAsync();

            _logger.LogInformation("Updated diagnosis {DiagnosisId}", diagnosis.Id);
            return existingDiagnosis;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error updating diagnosis {DiagnosisId}", diagnosis.Id);
            throw;
        }
    }

    /// <summary>
    /// Удалить диагноз
    /// </summary>
    public async Task<bool> DeleteDiagnosisAsync(int id)
    {
        try
        {
            var diagnosis = await _context.Diagnoses.FindAsync(id);
            if (diagnosis == null)
            {
                _logger.LogWarning("Attempt to delete non-existent diagnosis {DiagnosisId}", id);
                return false;
            }

            _context.Diagnoses.Remove(diagnosis);
            await _context.SaveChangesAsync();

            _logger.LogInformation("Deleted diagnosis {DiagnosisId}", id);
            return true;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error deleting diagnosis {DiagnosisId}", id);
            throw;
        }
    }

    /// <summary>
    /// Поиск диагнозов
    /// </summary>
    public async Task<IEnumerable<Diagnosis>> SearchDiagnosesAsync(string searchTerm)
    {
        try
        {
            if (string.IsNullOrWhiteSpace(searchTerm))
            {
                return await GetAllDiagnosesAsync();
            }

            var term = searchTerm.ToLower().Trim();

            return await _context.Diagnoses
                .Include(d => d.Visit)
                .ThenInclude(v => v.Patient)
                .Where(d => 
                    (d.IcdCode != null && d.IcdCode.ToLower().Contains(term)) ||
                    d.Name.ToLower().Contains(term) ||
                    (d.Description != null && d.Description.ToLower().Contains(term)) ||
                    (d.Notes != null && d.Notes.ToLower().Contains(term)))
                .OrderByDescending(d => d.DiagnosisDate)
                .ToListAsync();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error searching diagnoses with term '{SearchTerm}'", searchTerm);
            throw;
        }
    }

    /// <summary>
    /// Проверить наличие диагнозов у визита
    /// </summary>
    public async Task<bool> HasVisitDiagnosesAsync(int visitId)
    {
        try
        {
            return await _context.Diagnoses.AnyAsync(d => d.VisitId == visitId);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error checking diagnoses for visit {VisitId}", visitId);
            throw;
        }
    }

    /// <summary>
    /// Получить количество диагнозов
    /// </summary>
    public async Task<int> GetDiagnosesCountAsync()
    {
        try
        {
            return await _context.Diagnoses.CountAsync();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting diagnoses count");
            throw;
        }
    }

    /// <summary>
    /// Получить последние диагнозы
    /// </summary>
    public async Task<IEnumerable<Diagnosis>> GetRecentDiagnosesAsync(int count = 10)
    {
        try
        {
            return await _context.Diagnoses
                .Include(d => d.Visit)
                .ThenInclude(v => v.Patient)
                .OrderByDescending(d => d.CreatedAt)
                .Take(count)
                .ToListAsync();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving recent diagnoses");
            throw;
        }
    }
} 