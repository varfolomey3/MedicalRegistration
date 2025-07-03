using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.AspNetCore.Mvc.ModelBinding.Validation;

namespace MedicalRegistration.Models;

/// <summary>
/// Модель назначения/рецепта
/// </summary>
public class Prescription
{
    /// <summary>
    /// Уникальный идентификатор назначения
    /// </summary>
    public int Id { get; set; }

    /// <summary>
    /// Идентификатор визита
    /// </summary>
    [Required(ErrorMessage = "Визит обязателен для заполнения")]
    public int VisitId { get; set; }

    /// <summary>
    /// Тип назначения
    /// </summary>
    [Required(ErrorMessage = "Тип назначения обязателен для заполнения")]
    public PrescriptionType Type { get; set; }

    /// <summary>
    /// Название препарата/процедуры
    /// </summary>
    [Required(ErrorMessage = "Название обязательно для заполнения")]
    [StringLength(300, ErrorMessage = "Название не может превышать 300 символов")]
    public string Name { get; set; } = string.Empty;

    /// <summary>
    /// Дозировка
    /// </summary>
    [StringLength(100, ErrorMessage = "Дозировка не может превышать 100 символов")]
    public string? Dosage { get; set; }

    /// <summary>
    /// Частота приема
    /// </summary>
    [StringLength(200, ErrorMessage = "Частота приема не может превышать 200 символов")]
    public string? Frequency { get; set; }

    /// <summary>
    /// Продолжительность лечения
    /// </summary>
    [StringLength(100, ErrorMessage = "Продолжительность не может превышать 100 символов")]
    public string? Duration { get; set; }

    /// <summary>
    /// Инструкции по применению
    /// </summary>
    [StringLength(1000, ErrorMessage = "Инструкции не могут превышать 1000 символов")]
    public string? Instructions { get; set; }

    /// <summary>
    /// Дата начала приема
    /// </summary>
    [Required(ErrorMessage = "Дата начала обязательна для заполнения")]
    public DateTime StartDate { get; set; }

    /// <summary>
    /// Дата окончания приема
    /// </summary>
    public DateTime? EndDate { get; set; }

    /// <summary>
    /// Статус назначения
    /// </summary>
    [Required(ErrorMessage = "Статус назначения обязателен")]
    public PrescriptionStatus Status { get; set; } = PrescriptionStatus.Active;

    /// <summary>
    /// Приоритет выполнения
    /// </summary>
    public Priority Priority { get; set; } = Priority.Normal;

    /// <summary>
    /// Примечания к назначению
    /// </summary>
    [StringLength(500, ErrorMessage = "Примечания не могут превышать 500 символов")]
    public string? Notes { get; set; }

    /// <summary>
    /// Дата создания записи
    /// </summary>
    public DateTime CreatedAt { get; set; }

    /// <summary>
    /// Дата последнего обновления записи
    /// </summary>
    public DateTime UpdatedAt { get; set; }

    /// <summary>
    /// Навигационное свойство для визита
    /// </summary>
    [ForeignKey(nameof(VisitId))]
    [ValidateNever]
    public virtual Visit Visit { get; set; } = null!;
}

/// <summary>
/// Тип назначения
/// </summary>
public enum PrescriptionType
{
    /// <summary>
    /// Медикаментозное лечение
    /// </summary>
    Medication = 1,

    /// <summary>
    /// Медицинская процедура
    /// </summary>
    Procedure = 2,

    /// <summary>
    /// Анализы
    /// </summary>
    LabTest = 3,

    /// <summary>
    /// Диагностическое исследование
    /// </summary>
    Diagnostic = 4,

    /// <summary>
    /// Физиотерапия
    /// </summary>
    Physiotherapy = 5,

    /// <summary>
    /// Консультация специалиста
    /// </summary>
    Consultation = 6,

    /// <summary>
    /// Хирургическое вмешательство
    /// </summary>
    Surgery = 7,

    /// <summary>
    /// Реабилитация
    /// </summary>
    Rehabilitation = 8
}

/// <summary>
/// Статус назначения
/// </summary>
public enum PrescriptionStatus
{
    /// <summary>
    /// Активно
    /// </summary>
    Active = 1,

    /// <summary>
    /// Выполнено
    /// </summary>
    Completed = 2,

    /// <summary>
    /// Отменено
    /// </summary>
    Cancelled = 3,

    /// <summary>
    /// Приостановлено
    /// </summary>
    Suspended = 4,

    /// <summary>
    /// Просрочено
    /// </summary>
    Expired = 5
}

/// <summary>
/// Приоритет назначения
/// </summary>
public enum Priority
{
    /// <summary>
    /// Низкий
    /// </summary>
    Low = 1,

    /// <summary>
    /// Обычный
    /// </summary>
    Normal = 2,

    /// <summary>
    /// Высокий
    /// </summary>
    High = 3,

    /// <summary>
    /// Критический
    /// </summary>
    Critical = 4
} 