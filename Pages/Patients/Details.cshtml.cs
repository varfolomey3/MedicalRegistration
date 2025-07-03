using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using MedicalRegistration.Models;
using MedicalRegistration.Services;

namespace MedicalRegistration.Pages.Patients;

public class DetailsModel : PageModel
{
    private readonly IPatientService _patientService;

    public DetailsModel(IPatientService patientService)
    {
        _patientService = patientService;
    }

    public Patient Patient { get; set; } = null!;

    public async Task<IActionResult> OnGetAsync(int? id)
    {
        if (id == null)
        {
            return NotFound();
        }

        try
        {
            var patient = await _patientService.GetPatientByIdAsync(id.Value);
            
            if (patient == null)
            {
                return NotFound();
            }
            
            Patient = patient;

            return Page();
        }
        catch (Exception ex)
        {
            TempData["ErrorMessage"] = $"Ошибка при загрузке данных пациента: {ex.Message}";
            return RedirectToPage("Index");
        }
    }
} 