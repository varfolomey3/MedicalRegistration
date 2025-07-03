using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using MedicalRegistration.Models;
using MedicalRegistration.Services;

namespace MedicalRegistration.Pages.Patients;

public class CreateModel : PageModel
{
    private readonly IPatientService _patientService;

    public CreateModel(IPatientService patientService)
    {
        _patientService = patientService;
    }

    [BindProperty]
    public Patient Patient { get; set; } = new Patient();

    public IActionResult OnGet()
    {
        // Инициализируем пустого пациента
        Patient = new Patient
        {
            DateOfBirth = DateTime.Today.AddYears(-30) // Значение по умолчанию
        };
        
        return Page();
    }

    public async Task<IActionResult> OnPostAsync()
    {
        if (!ModelState.IsValid)
        {
            return Page();
        }

        try
        {
            // Дополнительная валидация возраста
            if (Patient.DateOfBirth > DateTime.Today)
            {
                ModelState.AddModelError("Patient.DateOfBirth", "Дата рождения не может быть в будущем");
                return Page();
            }

            if (Patient.Age > 150)
            {
                ModelState.AddModelError("Patient.DateOfBirth", "Возраст не может превышать 150 лет");
                return Page();
            }

            // Проверка уникальности номера медицинской карты
            var isUnique = await _patientService.IsMedicalCardNumberUniqueAsync(Patient.MedicalCardNumber);
            if (!isUnique)
            {
                ModelState.AddModelError("Patient.MedicalCardNumber", 
                    "Пациент с таким номером медицинской карты уже существует");
                return Page();
            }

            // Создание пациента
            var createdPatient = await _patientService.CreatePatientAsync(Patient);
            
            TempData["SuccessMessage"] = $"Пациент {createdPatient.FullName} успешно добавлен в систему";
            
            return RedirectToPage("Details", new { id = createdPatient.Id });
        }
        catch (InvalidOperationException ex)
        {
            ModelState.AddModelError(string.Empty, ex.Message);
            return Page();
        }
        catch (Exception ex)
        {
            ModelState.AddModelError(string.Empty, $"Ошибка при создании пациента: {ex.Message}");
            return Page();
        }
    }
} 