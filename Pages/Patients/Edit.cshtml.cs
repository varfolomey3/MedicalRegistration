using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using MedicalRegistration.Models;
using MedicalRegistration.Services;
using Microsoft.EntityFrameworkCore;

namespace MedicalRegistration.Pages.Patients
{
    public class EditModel : PageModel
    {
        private readonly IPatientService _patientService;
        private readonly ILogger<EditModel> _logger;

        public EditModel(IPatientService patientService, ILogger<EditModel> logger)
        {
            _patientService = patientService;
            _logger = logger;
        }

        [BindProperty]
        public Patient Patient { get; set; } = default!;

        public async Task<IActionResult> OnGetAsync(int? id)
        {
            if (id == null)
            {
                _logger.LogWarning("Edit page accessed without patient ID");
                return NotFound();
            }

            try
            {
                var patient = await _patientService.GetByIdAsync(id.Value);
                if (patient == null)
                {
                    _logger.LogWarning("Patient with ID {PatientId} not found for editing", id.Value);
                    TempData["ErrorMessage"] = "Пациент не найден.";
                    return RedirectToPage("./Index");
                }

                Patient = patient;
                _logger.LogInformation("Editing patient {PatientId}: {PatientName}", patient.Id, patient.FullName);
                return Page();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error loading patient {PatientId} for editing", id.Value);
                TempData["ErrorMessage"] = "Произошла ошибка при загрузке данных пациента.";
                return RedirectToPage("./Index");
            }
        }

        public async Task<IActionResult> OnPostAsync()
        {
            if (!ModelState.IsValid)
            {
                _logger.LogWarning("Invalid model state when updating patient {PatientId}", Patient?.Id);
                return Page();
            }

            try
            {
                // Проверяем, существует ли пациент
                var existingPatient = await _patientService.GetByIdAsync(Patient.Id);
                if (existingPatient == null)
                {
                    _logger.LogWarning("Attempt to update non-existent patient {PatientId}", Patient.Id);
                    TempData["ErrorMessage"] = "Пациент не найден.";
                    return RedirectToPage("./Index");
                }

                // Проверяем уникальность номера медицинской карты (если изменился)
                if (existingPatient.MedicalCardNumber != Patient.MedicalCardNumber)
                {
                    var patientWithSameCard = await _patientService.GetByMedicalCardNumberAsync(Patient.MedicalCardNumber);
                    if (patientWithSameCard != null)
                    {
                        ModelState.AddModelError("Patient.MedicalCardNumber", 
                            "Пациент с таким номером медицинской карты уже существует.");
                        _logger.LogWarning("Attempt to update patient {PatientId} with duplicate medical card number {CardNumber}", 
                            Patient.Id, Patient.MedicalCardNumber);
                        return Page();
                    }
                }

                // Обновляем пациента
                await _patientService.UpdateAsync(Patient);
                
                _logger.LogInformation("Patient {PatientId} updated successfully by user", Patient.Id);
                TempData["SuccessMessage"] = $"Информация о пациенте {Patient.FullName} успешно обновлена.";
                
                return RedirectToPage("./Details", new { id = Patient.Id });
            }
            catch (DbUpdateConcurrencyException ex)
            {
                _logger.LogError(ex, "Concurrency error updating patient {PatientId}", Patient?.Id);
                
                // Проверяем, существует ли пациент еще
                var exists = Patient?.Id != null && await _patientService.GetByIdAsync(Patient.Id) != null;
                if (!exists)
                {
                    TempData["ErrorMessage"] = "Пациент был удален другим пользователем.";
                    return RedirectToPage("./Index");
                }
                
                TempData["ErrorMessage"] = "Данные пациента были изменены другим пользователем. Пожалуйста, обновите страницу и повторите попытку.";
                return RedirectToPage("./Edit", new { id = Patient.Id });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error updating patient {PatientId}", Patient?.Id);
                TempData["ErrorMessage"] = "Произошла ошибка при обновлении данных пациента. Пожалуйста, попробуйте еще раз.";
                return Page();
            }
        }

        public async Task<IActionResult> OnPostDeleteAsync()
        {
            if (Patient?.Id == null)
            {
                _logger.LogWarning("Delete attempted without patient ID");
                return RedirectToPage("./Index");
            }

            try
            {
                var patient = await _patientService.GetByIdAsync(Patient.Id);
                if (patient == null)
                {
                    _logger.LogWarning("Attempt to delete non-existent patient {PatientId}", Patient.Id);
                    TempData["ErrorMessage"] = "Пациент не найден.";
                    return RedirectToPage("./Index");
                }

                await _patientService.DeleteAsync(Patient.Id);
                
                _logger.LogInformation("Patient {PatientId} ({PatientName}) deleted successfully", 
                    Patient.Id, patient.FullName);
                TempData["SuccessMessage"] = $"Пациент {patient.FullName} успешно удален из системы.";
                
                return RedirectToPage("./Index");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error deleting patient {PatientId}", Patient?.Id);
                TempData["ErrorMessage"] = "Произошла ошибка при удалении пациента. Пожалуйста, попробуйте еще раз.";
                return RedirectToPage("./Details", new { id = Patient.Id });
            }
        }
    }
} 