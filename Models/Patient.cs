using System.ComponentModel.DataAnnotations;

namespace MedicalRegistration.Models;

/// <summary>
/// Модель пациента
/// </summary>
public class Patient
{
    /// <summary>
    /// Уникальный идентификатор пациента
    /// </summary>
    public int Id { get; set; }

    /// <summary>
    /// Фамилия пациента
    /// </summary>
    [Required(ErrorMessage = "Фамилия обязательна для заполнения")]
    [StringLength(100, ErrorMessage = "Фамилия не может превышать 100 символов")]
    public string LastName { get; set; } = string.Empty;

    /// <summary>
    /// Имя пациента
    /// </summary>
    [Required(ErrorMessage = "Имя обязательно для заполнения")]
    [StringLength(100, ErrorMessage = "Имя не может превышать 100 символов")]
    public string FirstName { get; set; } = string.Empty;

    /// <summary>
    /// Отчество пациента
    /// </summary>
    [StringLength(100, ErrorMessage = "Отчество не может превышать 100 символов")]
    public string? MiddleName { get; set; }

    /// <summary>
    /// Дата рождения
    /// </summary>
    [Required(ErrorMessage = "Дата рождения обязательна для заполнения")]
    [DataType(DataType.Date)]
    public DateTime DateOfBirth { get; set; }

    /// <summary>
    /// Пол пациента
    /// </summary>
    [Required(ErrorMessage = "Пол обязателен для заполнения")]
    public Gender Gender { get; set; }

    /// <summary>
    /// Номер телефона
    /// </summary>
    [Phone(ErrorMessage = "Некорректный номер телефона")]
    [StringLength(15, ErrorMessage = "Номер телефона не может превышать 15 символов")]
    public string? PhoneNumber { get; set; }

    /// <summary>
    /// Email адрес
    /// </summary>
    [EmailAddress(ErrorMessage = "Некорректный email адрес")]
    [StringLength(100, ErrorMessage = "Email не может превышать 100 символов")]
    public string? Email { get; set; }

    /// <summary>
    /// Адрес проживания
    /// </summary>
    [StringLength(300, ErrorMessage = "Адрес не может превышать 300 символов")]
    public string? Address { get; set; }

    /// <summary>
    /// Номер медицинской карты
    /// </summary>
    [Required(ErrorMessage = "Номер медицинской карты обязателен")]
    [StringLength(20, ErrorMessage = "Номер медицинской карты не может превышать 20 символов")]
    public string MedicalCardNumber { get; set; } = string.Empty;

    /// <summary>
    /// Дата создания записи
    /// </summary>
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    /// <summary>
    /// Дата последнего обновления записи
    /// </summary>
    public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;

    /// <summary>
    /// Навигационное свойство для визитов пациента
    /// </summary>
    public virtual ICollection<Visit> Visits { get; set; } = new List<Visit>();

    /// <summary>
    /// Полное имя пациента
    /// </summary>
    public string FullName => $"{LastName} {FirstName} {MiddleName}".Trim();

    /// <summary>
    /// Возраст пациента
    /// </summary>
    public int Age => DateTime.Today.Year - DateOfBirth.Year - 
                     (DateTime.Today.DayOfYear < DateOfBirth.DayOfYear ? 1 : 0);
}

/// <summary>
/// Перечисление пола
/// </summary>
public enum Gender
{
    /// <summary>
    /// Мужской
    /// </summary>
    Male = 1,
    
    /// <summary>
    /// Женский
    /// </summary>
    Female = 2
} 