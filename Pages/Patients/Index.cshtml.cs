using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using MedicalRegistration.Models;
using MedicalRegistration.Services;

namespace MedicalRegistration.Pages.Patients;

public class IndexModel : PageModel
{
    private readonly IPatientService _patientService;

    public IndexModel(IPatientService patientService)
    {
        _patientService = patientService;
    }

    public IEnumerable<Patient> Patients { get; set; } = new List<Patient>();

    [BindProperty(SupportsGet = true)]
    public string? SearchTerm { get; set; }

    public async Task OnGetAsync()
    {
        try
        {
            if (!string.IsNullOrWhiteSpace(SearchTerm))
            {
                Patients = await _patientService.SearchPatientsAsync(SearchTerm);
            }
            else
            {
                Patients = await _patientService.GetAllPatientsAsync();
            }
        }
        catch (Exception ex)
        {
            TempData["ErrorMessage"] = $"Ошибка при получении списка пациентов: {ex.Message}";
            Patients = new List<Patient>();
        }
    }

    public async Task<IActionResult> OnPostDeleteAsync(int id)
    {
        try
        {
            var success = await _patientService.DeletePatientAsync(id);
            
            if (success)
            {
                TempData["SuccessMessage"] = "Пациент успешно удален";
            }
            else
            {
                TempData["ErrorMessage"] = "Пациент не найден";
            }
        }
        catch (InvalidOperationException ex)
        {
            TempData["ErrorMessage"] = ex.Message;
        }
        catch (Exception ex)
        {
            TempData["ErrorMessage"] = $"Ошибка при удалении пациента: {ex.Message}";
        }

        return RedirectToPage();
    }
} 