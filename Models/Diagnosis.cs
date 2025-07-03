using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.AspNetCore.Mvc.ModelBinding.Validation;

namespace MedicalRegistration.Models;

/// <summary>
/// Модель диагноза
/// </summary>
public class Diagnosis
{
    /// <summary>
    /// Уникальный идентификатор диагноза
    /// </summary>
    public int Id { get; set; }

    /// <summary>
    /// Идентификатор визита
    /// </summary>
    [Required(ErrorMessage = "Визит обязателен для заполнения")]
    public int VisitId { get; set; }

    /// <summary>
    /// Код диагноза по МКБ-10
    /// </summary>
    [StringLength(10, ErrorMessage = "Код МКБ-10 не может превышать 10 символов")]
    public string? IcdCode { get; set; }

    /// <summary>
    /// Название диагноза
    /// </summary>
    [Required(ErrorMessage = "Название диагноза обязательно для заполнения")]
    [StringLength(500, ErrorMessage = "Название диагноза не может превышать 500 символов")]
    public string Name { get; set; } = string.Empty;

    /// <summary>
    /// Описание диагноза
    /// </summary>
    [StringLength(2000, ErrorMessage = "Описание диагноза не может превышать 2000 символов")]
    public string? Description { get; set; }

    /// <summary>
    /// Тип диагноза
    /// </summary>
    [Required(ErrorMessage = "Тип диагноза обязателен для заполнения")]
    public DiagnosisType Type { get; set; }

    /// <summary>
    /// Степень тяжести
    /// </summary>
    public SeverityLevel? Severity { get; set; }

    /// <summary>
    /// Дата установления диагноза
    /// </summary>
    [Required(ErrorMessage = "Дата диагноза обязательна для заполнения")]
    public DateTime DiagnosisDate { get; set; }

    /// <summary>
    /// Примечания к диагнозу
    /// </summary>
    [StringLength(1000, ErrorMessage = "Примечания не могут превышать 1000 символов")]
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
/// Тип диагноза
/// </summary>
public enum DiagnosisType
{
    /// <summary>
    /// Основной диагноз
    /// </summary>
    Primary = 1,

    /// <summary>
    /// Сопутствующий диагноз
    /// </summary>
    Secondary = 2,

    /// <summary>
    /// Предварительный диагноз
    /// </summary>
    Preliminary = 3,

    /// <summary>
    /// Дифференциальный диагноз
    /// </summary>
    Differential = 4
}

/// <summary>
/// Степень тяжести
/// </summary>
public enum SeverityLevel
{
    /// <summary>
    /// Легкая
    /// </summary>
    Mild = 1,

    /// <summary>
    /// Средняя
    /// </summary>
    Moderate = 2,

    /// <summary>
    /// Тяжелая
    /// </summary>
    Severe = 3,

    /// <summary>
    /// Критическая
    /// </summary>
    Critical = 4
} 