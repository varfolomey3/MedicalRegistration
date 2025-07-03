using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace MedicalRegistration.Models;

/// <summary>
/// Модель медицинского отчета
/// </summary>
public class MedicalReport
{
    /// <summary>
    /// Уникальный идентификатор отчета
    /// </summary>
    public int Id { get; set; }

    /// <summary>
    /// Название отчета
    /// </summary>
    [Required(ErrorMessage = "Название отчета обязательно для заполнения")]
    [StringLength(200, ErrorMessage = "Название отчета не может превышать 200 символов")]
    public string Title { get; set; } = string.Empty;

    /// <summary>
    /// Тип отчета
    /// </summary>
    [Required(ErrorMessage = "Тип отчета обязателен для заполнения")]
    public ReportType Type { get; set; }

    /// <summary>
    /// Период отчета - начальная дата
    /// </summary>
    [Required(ErrorMessage = "Начальная дата периода обязательна")]
    [DataType(DataType.Date)]
    public DateTime PeriodStart { get; set; }

    /// <summary>
    /// Период отчета - конечная дата
    /// </summary>
    [Required(ErrorMessage = "Конечная дата периода обязательна")]
    [DataType(DataType.Date)]
    public DateTime PeriodEnd { get; set; }

    /// <summary>
    /// Врач, создавший отчет
    /// </summary>
    [Required(ErrorMessage = "Врач обязателен для заполнения")]
    [StringLength(200, ErrorMessage = "Имя врача не может превышать 200 символов")]
    public string DoctorName { get; set; } = string.Empty;

    /// <summary>
    /// Специальность врача
    /// </summary>
    [StringLength(100, ErrorMessage = "Специальность не может превышать 100 символов")]
    public string? DoctorSpecialty { get; set; }

    /// <summary>
    /// Описание отчета
    /// </summary>
    [StringLength(1000, ErrorMessage = "Описание не может превышать 1000 символов")]
    public string? Description { get; set; }

    /// <summary>
    /// Содержание отчета в формате JSON
    /// </summary>
    [Column(TypeName = "TEXT")]
    public string? Content { get; set; }

    /// <summary>
    /// Статистические данные в формате JSON
    /// </summary>
    [Column(TypeName = "TEXT")]
    public string? Statistics { get; set; }

    /// <summary>
    /// Выводы и рекомендации
    /// </summary>
    [StringLength(2000, ErrorMessage = "Выводы не могут превышать 2000 символов")]
    public string? Conclusions { get; set; }

    /// <summary>
    /// Статус отчета
    /// </summary>
    [Required(ErrorMessage = "Статус отчета обязателен")]
    public ReportStatus Status { get; set; } = ReportStatus.Draft;

    /// <summary>
    /// Дата создания отчета
    /// </summary>
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    /// <summary>
    /// Дата последнего обновления отчета
    /// </summary>
    public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;

    /// <summary>
    /// Дата публикации отчета
    /// </summary>
    public DateTime? PublishedAt { get; set; }
}

/// <summary>
/// Тип отчета
/// </summary>
public enum ReportType
{
    /// <summary>
    /// Статистика по пациентам
    /// </summary>
    PatientStatistics = 1,

    /// <summary>
    /// Статистика по диагнозам
    /// </summary>
    DiagnosisStatistics = 2,

    /// <summary>
    /// Статистика по визитам
    /// </summary>
    VisitStatistics = 3,

    /// <summary>
    /// Отчет по назначениям
    /// </summary>
    PrescriptionReport = 4,

    /// <summary>
    /// Финансовый отчет
    /// </summary>
    FinancialReport = 5,

    /// <summary>
    /// Отчет по эффективности лечения
    /// </summary>
    TreatmentEfficiency = 6,

    /// <summary>
    /// Эпидемиологический отчет
    /// </summary>
    EpidemiologyReport = 7,

    /// <summary>
    /// Отчет по качеству медицинской помощи
    /// </summary>
    QualityReport = 8,

    /// <summary>
    /// Пользовательский отчет
    /// </summary>
    CustomReport = 99
}

/// <summary>
/// Статус отчета
/// </summary>
public enum ReportStatus
{
    /// <summary>
    /// Черновик
    /// </summary>
    Draft = 1,

    /// <summary>
    /// В обработке
    /// </summary>
    Processing = 2,

    /// <summary>
    /// Готов к публикации
    /// </summary>
    ReadyToPublish = 3,

    /// <summary>
    /// Опубликован
    /// </summary>
    Published = 4,

    /// <summary>
    /// Архивирован
    /// </summary>
    Archived = 5
} 