using MedicalRegistration.Models;

namespace MedicalRegistration.Services;

/// <summary>
/// Интерфейс сервиса для работы с назначениями/рецептами
/// </summary>
public interface IPrescriptionService
{
    /// <summary>
    /// Получить все назначения
    /// </summary>
    Task<IEnumerable<Prescription>> GetAllPrescriptionsAsync();

    /// <summary>
    /// Получить назначение по ID
    /// </summary>
    Task<Prescription?> GetPrescriptionByIdAsync(int id);

    /// <summary>
    /// Получить назначения пациента
    /// </summary>
    Task<IEnumerable<Prescription>> GetPrescriptionsByPatientIdAsync(int patientId);

    /// <summary>
    /// Получить назначения по визиту
    /// </summary>
    Task<IEnumerable<Prescription>> GetPrescriptionsByVisitIdAsync(int visitId);

    /// <summary>
    /// Получить назначения по типу
    /// </summary>
    Task<IEnumerable<Prescription>> GetPrescriptionsByTypeAsync(PrescriptionType type);

    /// <summary>
    /// Получить назначения по статусу
    /// </summary>
    Task<IEnumerable<Prescription>> GetPrescriptionsByStatusAsync(PrescriptionStatus status);

    /// <summary>
    /// Получить назначения по приоритету
    /// </summary>
    Task<IEnumerable<Prescription>> GetPrescriptionsByPriorityAsync(Priority priority);

    /// <summary>
    /// Получить активные назначения пациента
    /// </summary>
    Task<IEnumerable<Prescription>> GetActivePrescriptionsByPatientIdAsync(int patientId);

    /// <summary>
    /// Получить назначения за период
    /// </summary>
    Task<IEnumerable<Prescription>> GetPrescriptionsByDateRangeAsync(DateTime startDate, DateTime endDate);

    /// <summary>
    /// Получить просроченные назначения
    /// </summary>
    Task<IEnumerable<Prescription>> GetExpiredPrescriptionsAsync();

    /// <summary>
    /// Создать новое назначение
    /// </summary>
    Task<Prescription> CreatePrescriptionAsync(Prescription prescription);

    /// <summary>
    /// Обновить назначение
    /// </summary>
    Task<Prescription> UpdatePrescriptionAsync(Prescription prescription);

    /// <summary>
    /// Удалить назначение
    /// </summary>
    Task<bool> DeletePrescriptionAsync(int id);

    /// <summary>
    /// Поиск назначений
    /// </summary>
    Task<IEnumerable<Prescription>> SearchPrescriptionsAsync(string searchTerm);

    /// <summary>
    /// Обновить статус назначения
    /// </summary>
    Task<bool> UpdatePrescriptionStatusAsync(int id, PrescriptionStatus status);

    /// <summary>
    /// Получить статистику назначений
    /// </summary>
    Task<PrescriptionStatistics> GetPrescriptionStatisticsAsync();

    /// <summary>
    /// Проверить валидность назначения
    /// </summary>
    Task<bool> ValidatePrescriptionAsync(Prescription prescription);
}

/// <summary>
/// Статистика назначений
/// </summary>
public class PrescriptionStatistics
{
    public int TotalPrescriptions { get; set; }
    public int ActivePrescriptions { get; set; }
    public int CompletedPrescriptions { get; set; }
    public int CancelledPrescriptions { get; set; }
    public int ExpiredPrescriptions { get; set; }
    public Dictionary<PrescriptionType, int> PrescriptionsByType { get; set; } = new();
    public Dictionary<Priority, int> PrescriptionsByPriority { get; set; } = new();
} 