using Microsoft.EntityFrameworkCore;
using MedicalRegistration.Data;
using MedicalRegistration.Models;

namespace MedicalRegistration.Services;

/// <summary>
/// Интерфейс сервиса для работы с пациентами
/// </summary>
public interface IPatientService
{
    Task<IEnumerable<Patient>> GetAllPatientsAsync();
    Task<Patient?> GetPatientByIdAsync(int id);
    Task<Patient?> GetPatientByMedicalCardNumberAsync(string medicalCardNumber);
    Task<Patient> CreatePatientAsync(Patient patient);
    Task<Patient> UpdatePatientAsync(Patient patient);
    Task<bool> DeletePatientAsync(int id);
    Task<IEnumerable<Patient>> SearchPatientsAsync(string searchTerm);
    Task<bool> IsMedicalCardNumberUniqueAsync(string medicalCardNumber, int? excludePatientId = null);
    
    // Алиасы для совместимости
    Task<Patient?> GetByIdAsync(int id);
    Task<Patient?> GetByMedicalCardNumberAsync(string medicalCardNumber);
    Task<Patient> UpdateAsync(Patient patient);
    Task DeleteAsync(int id);
}

/// <summary>
/// Сервис для работы с пациентами
/// </summary>
public class PatientService : IPatientService
{
    private readonly MedicalDbContext _context;

    public PatientService(MedicalDbContext context)
    {
        _context = context;
    }

    /// <summary>
    /// Получить всех пациентов
    /// </summary>
    public async Task<IEnumerable<Patient>> GetAllPatientsAsync()
    {
        return await _context.Patients
            .OrderBy(p => p.LastName)
            .ThenBy(p => p.FirstName)
            .ToListAsync();
    }

    /// <summary>
    /// Получить пациента по ID
    /// </summary>
    public async Task<Patient?> GetPatientByIdAsync(int id)
    {
        return await _context.Patients
            .Include(p => p.Visits)
                .ThenInclude(v => v.Diagnoses)
            .Include(p => p.Visits)
                .ThenInclude(v => v.Prescriptions)
            .FirstOrDefaultAsync(p => p.Id == id);
    }

    /// <summary>
    /// Получить пациента по номеру медицинской карты
    /// </summary>
    public async Task<Patient?> GetPatientByMedicalCardNumberAsync(string medicalCardNumber)
    {
        return await _context.Patients
            .Include(p => p.Visits)
            .FirstOrDefaultAsync(p => p.MedicalCardNumber == medicalCardNumber);
    }

    /// <summary>
    /// Создать нового пациента
    /// </summary>
    public async Task<Patient> CreatePatientAsync(Patient patient)
    {
        // Проверяем уникальность номера медицинской карты
        var existingPatient = await _context.Patients
            .FirstOrDefaultAsync(p => p.MedicalCardNumber == patient.MedicalCardNumber);

        if (existingPatient != null)
        {
            throw new InvalidOperationException($"Пациент с номером медицинской карты {patient.MedicalCardNumber} уже существует");
        }

        // Устанавливаем даты создания и обновления
        patient.CreatedAt = DateTime.UtcNow;
        patient.UpdatedAt = DateTime.UtcNow;

        _context.Patients.Add(patient);
        await _context.SaveChangesAsync();

        return patient;
    }

    /// <summary>
    /// Обновить информацию о пациенте
    /// </summary>
    public async Task<Patient> UpdatePatientAsync(Patient patient)
    {
        var existingPatient = await _context.Patients.FindAsync(patient.Id);
        if (existingPatient == null)
        {
            throw new InvalidOperationException($"Пациент с ID {patient.Id} не найден");
        }

        // Проверяем уникальность номера медицинской карты (исключая текущего пациента)
        var duplicateCardNumber = await _context.Patients
            .AnyAsync(p => p.MedicalCardNumber == patient.MedicalCardNumber && p.Id != patient.Id);

        if (duplicateCardNumber)
        {
            throw new InvalidOperationException($"Пациент с номером медицинской карты {patient.MedicalCardNumber} уже существует");
        }

        // Обновляем поля
        existingPatient.LastName = patient.LastName;
        existingPatient.FirstName = patient.FirstName;
        existingPatient.MiddleName = patient.MiddleName;
        existingPatient.DateOfBirth = patient.DateOfBirth;
        existingPatient.Gender = patient.Gender;
        existingPatient.PhoneNumber = patient.PhoneNumber;
        existingPatient.Email = patient.Email;
        existingPatient.Address = patient.Address;
        existingPatient.MedicalCardNumber = patient.MedicalCardNumber;
        existingPatient.UpdatedAt = DateTime.UtcNow;

        await _context.SaveChangesAsync();

        return existingPatient;
    }

    /// <summary>
    /// Удалить пациента
    /// </summary>
    public async Task<bool> DeletePatientAsync(int id)
    {
        var patient = await _context.Patients.FindAsync(id);
        if (patient == null)
        {
            return false;
        }

        // Проверяем, есть ли связанные визиты
        var hasVisits = await _context.Visits.AnyAsync(v => v.PatientId == id);
        if (hasVisits)
        {
            throw new InvalidOperationException("Невозможно удалить пациента, у которого есть записи о визитах. Сначала удалите все связанные визиты.");
        }

        _context.Patients.Remove(patient);
        await _context.SaveChangesAsync();

        return true;
    }

    /// <summary>
    /// Поиск пациентов по имени, фамилии или номеру медицинской карты
    /// </summary>
    public async Task<IEnumerable<Patient>> SearchPatientsAsync(string searchTerm)
    {
        if (string.IsNullOrWhiteSpace(searchTerm))
        {
            return await GetAllPatientsAsync();
        }

        var lowerSearchTerm = searchTerm.ToLower();

        return await _context.Patients
            .Where(p => 
                p.LastName.ToLower().Contains(lowerSearchTerm) ||
                p.FirstName.ToLower().Contains(lowerSearchTerm) ||
                (p.MiddleName != null && p.MiddleName.ToLower().Contains(lowerSearchTerm)) ||
                p.MedicalCardNumber.ToLower().Contains(lowerSearchTerm))
            .OrderBy(p => p.LastName)
            .ThenBy(p => p.FirstName)
            .ToListAsync();
    }

    /// <summary>
    /// Проверить уникальность номера медицинской карты
    /// </summary>
    public async Task<bool> IsMedicalCardNumberUniqueAsync(string medicalCardNumber, int? excludePatientId = null)
    {
        var query = _context.Patients.Where(p => p.MedicalCardNumber == medicalCardNumber);
        
        if (excludePatientId.HasValue)
        {
            query = query.Where(p => p.Id != excludePatientId.Value);
        }

        return !await query.AnyAsync();
    }
    
    // Реализация алиасов для совместимости
    public async Task<Patient?> GetByIdAsync(int id) => await GetPatientByIdAsync(id);
    
    public async Task<Patient?> GetByMedicalCardNumberAsync(string medicalCardNumber) => 
        await GetPatientByMedicalCardNumberAsync(medicalCardNumber);
    
    public async Task<Patient> UpdateAsync(Patient patient) => await UpdatePatientAsync(patient);
    
    public async Task DeleteAsync(int id)
    {
        var result = await DeletePatientAsync(id);
        if (!result)
        {
            throw new InvalidOperationException($"Пациент с ID {id} не найден");
        }
    }
} 