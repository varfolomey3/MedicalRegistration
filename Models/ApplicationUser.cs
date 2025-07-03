using Microsoft.AspNetCore.Identity;
using System.ComponentModel.DataAnnotations;

namespace MedicalRegistration.Models;

/// <summary>
/// Пользователь системы (врач или пациент)
/// </summary>
public class ApplicationUser : IdentityUser
{
    /// <summary>
    /// Имя пользователя
    /// </summary>
    [Required]
    [StringLength(50)]
    public string FirstName { get; set; } = string.Empty;

    /// <summary>
    /// Фамилия пользователя
    /// </summary>
    [Required]
    [StringLength(50)]
    public string LastName { get; set; } = string.Empty;

    /// <summary>
    /// Отчество пользователя
    /// </summary>
    [StringLength(50)]
    public string? MiddleName { get; set; }

    /// <summary>
    /// Дата рождения
    /// </summary>
    public DateTime DateOfBirth { get; set; }

    /// <summary>
    /// Пол
    /// </summary>
    public Gender Gender { get; set; }

    /// <summary>
    /// Тип пользователя (врач или пациент)
    /// </summary>
    [Required]
    public UserType UserType { get; set; }

    /// <summary>
    /// Специальность (только для врачей)
    /// </summary>
    [StringLength(100)]
    public string? Specialty { get; set; }

    /// <summary>
    /// Номер лицензии (только для врачей)
    /// </summary>
    [StringLength(50)]
    public string? LicenseNumber { get; set; }

    /// <summary>
    /// Номер медицинской карты (только для пациентов)
    /// </summary>
    [StringLength(20)]
    public string? MedicalCardNumber { get; set; }

    /// <summary>
    /// Адрес
    /// </summary>
    [StringLength(200)]
    public string? Address { get; set; }

    /// <summary>
    /// Дата регистрации в системе
    /// </summary>
    public DateTime RegisteredAt { get; set; } = DateTime.Now;

    /// <summary>
    /// Полное имя
    /// </summary>
    public string FullName => $"{LastName} {FirstName} {MiddleName}".Trim();

    /// <summary>
    /// Проверка, является ли пользователь врачом
    /// </summary>
    public bool IsDoctor => UserType == UserType.Doctor;

    /// <summary>
    /// Проверка, является ли пользователь пациентом
    /// </summary>
    public bool IsPatient => UserType == UserType.Patient;
}

/// <summary>
/// Тип пользователя в системе
/// </summary>
public enum UserType
{
    /// <summary>
    /// Пациент
    /// </summary>
    Patient = 1,

    /// <summary>
    /// Врач
    /// </summary>
    Doctor = 2
}

/// <summary>
/// Роли пользователей в системе
/// </summary>
public static class UserRoles
{
    public const string Doctor = "Doctor";
    public const string Patient = "Patient";
    public const string Admin = "Admin";
} 