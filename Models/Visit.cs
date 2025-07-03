using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace MedicalRegistration.Models;

/// <summary>
/// Модель визита пациента
/// </summary>
public class Visit
{
    /// <summary>
    /// Уникальный идентификатор визита
    /// </summary>
    public int Id { get; set; }

    /// <summary>
    /// Идентификатор пациента
    /// </summary>
    [Required(ErrorMessage = "Пациент обязателен для заполнения")]
    public int PatientId { get; set; }

    /// <summary>
    /// Дата и время визита
    /// </summary>
    [Required(ErrorMessage = "Дата визита обязательна для заполнения")]
    public DateTime VisitDateTime { get; set; }

    /// <summary>
    /// Врач, принимавший пациента
    /// </summary>
    [Required(ErrorMessage = "Врач обязателен для заполнения")]
    [StringLength(200, ErrorMessage = "Имя врача не может превышать 200 символов")]
    public string DoctorName { get; set; } = string.Empty;

    /// <summary>
    /// Специальность врача
    /// </summary>
    [Required(ErrorMessage = "Специальность врача обязательна для заполнения")]
    [StringLength(100, ErrorMessage = "Специальность не может превышать 100 символов")]
    public string Specialty { get; set; } = string.Empty;

    /// <summary>
    /// Жалобы пациента
    /// </summary>
    [StringLength(1000, ErrorMessage = "Жалобы не могут превышать 1000 символов")]
    public string? Complaints { get; set; }

    /// <summary>
    /// Осмотр врача
    /// </summary>
    [StringLength(2000, ErrorMessage = "Описание осмотра не может превышать 2000 символов")]
    public string? Examination { get; set; }

    /// <summary>
    /// Статус визита
    /// </summary>
    [Required(ErrorMessage = "Статус визита обязателен")]
    public VisitStatus Status { get; set; } = VisitStatus.Scheduled;

    /// <summary>
    /// Стоимость визита
    /// </summary>
    [Column(TypeName = "decimal(10,2)")]
    [Range(0, 999999.99, ErrorMessage = "Стоимость должна быть от 0 до 999999.99")]
    public decimal? Cost { get; set; }

    /// <summary>
    /// Примечания к визиту
    /// </summary>
    [StringLength(500, ErrorMessage = "Примечания не могут превышать 500 символов")]
    public string? Notes { get; set; }

    /// <summary>
    /// Дата создания записи
    /// </summary>
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    /// <summary>
    /// Дата последнего обновления записи
    /// </summary>
    public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;

    /// <summary>
    /// Навигационное свойство для пациента
    /// </summary>
    [ForeignKey(nameof(PatientId))]
    [Microsoft.AspNetCore.Mvc.ModelBinding.Validation.ValidateNever] // Исключаем из валидации модели
    public virtual Patient? Patient { get; set; }

    /// <summary>
    /// Навигационное свойство для диагнозов
    /// </summary>
    [Microsoft.AspNetCore.Mvc.ModelBinding.Validation.ValidateNever]
    public virtual ICollection<Diagnosis> Diagnoses { get; set; } = new List<Diagnosis>();

    /// <summary>
    /// Навигационное свойство для назначений
    /// </summary>
    [Microsoft.AspNetCore.Mvc.ModelBinding.Validation.ValidateNever]
    public virtual ICollection<Prescription> Prescriptions { get; set; } = new List<Prescription>();
}

/// <summary>
/// Статус визита
/// </summary>
public enum VisitStatus
{
    /// <summary>
    /// Запланирован
    /// </summary>
    Scheduled = 1,

    /// <summary>
    /// В процессе
    /// </summary>
    InProgress = 2,

    /// <summary>
    /// Завершен
    /// </summary>
    Completed = 3,

    /// <summary>
    /// Отменен
    /// </summary>
    Cancelled = 4,

    /// <summary>
    /// Пациент не явился
    /// </summary>
    NoShow = 5
} 