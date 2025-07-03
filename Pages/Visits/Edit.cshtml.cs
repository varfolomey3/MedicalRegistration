using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.AspNetCore.Mvc.Rendering;
using MedicalRegistration.Models;
using MedicalRegistration.Services;

namespace MedicalRegistration.Pages.Visits
{
    public class EditModel : PageModel
    {
        private readonly IVisitService _visitService;
        private readonly IPatientService _patientService;
        private readonly ILogger<EditModel> _logger;

        public EditModel(IVisitService visitService, IPatientService patientService, ILogger<EditModel> logger)
        {
            _visitService = visitService;
            _patientService = patientService;
            _logger = logger;
        }

        [BindProperty]
        public Visit Visit { get; set; } = new Visit();

        public SelectList? PatientsSelectList { get; set; }
        public SelectList? StatusSelectList { get; set; }

        public async Task<IActionResult> OnGetAsync(int? id)
        {
            if (id == null)
            {
                _logger.LogWarning("Attempt to edit visit without ID");
                return NotFound();
            }

            try
            {
                Visit = await _visitService.GetVisitByIdAsync(id.Value);

                if (Visit == null)
                {
                    _logger.LogWarning("Visit {VisitId} not found for editing", id.Value);
                    return NotFound();
                }

                await LoadSelectListsAsync();
                _logger.LogInformation("Loading edit form for visit {VisitId}", id.Value);
                return Page();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error loading visit {VisitId} for editing", id.Value);
                TempData["ErrorMessage"] = "Произошла ошибка при загрузке информации о визите.";
                return RedirectToPage("./Index");
            }
        }

        public async Task<IActionResult> OnPostAsync()
        {
            if (!ModelState.IsValid)
            {
                await LoadSelectListsAsync();
                return Page();
            }

            try
            {
                await _visitService.UpdateVisitAsync(Visit);
                TempData["SuccessMessage"] = "Визит успешно обновлен.";
                _logger.LogInformation("Visit {VisitId} updated successfully", Visit.Id);
                return RedirectToPage("./Details", new { id = Visit.Id });
            }
            catch (InvalidOperationException ex)
            {
                _logger.LogWarning(ex, "Invalid operation when updating visit {VisitId}", Visit.Id);
                ModelState.AddModelError("", ex.Message);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error updating visit {VisitId}", Visit.Id);
                ModelState.AddModelError("", "Произошла ошибка при обновлении визита. Попробуйте еще раз.");
            }

            await LoadSelectListsAsync();
            return Page();
        }

        public async Task<IActionResult> OnPostDeleteAsync()
        {
            try
            {
                var result = await _visitService.DeleteVisitAsync(Visit.Id);
                if (result)
                {
                    TempData["SuccessMessage"] = "Визит успешно удален.";
                    _logger.LogInformation("Visit {VisitId} deleted successfully", Visit.Id);
                }
                else
                {
                    TempData["ErrorMessage"] = "Визит не найден.";
                    _logger.LogWarning("Attempt to delete non-existent visit {VisitId}", Visit.Id);
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error deleting visit {VisitId}", Visit.Id);
                TempData["ErrorMessage"] = "Произошла ошибка при удалении визита.";
            }

            return RedirectToPage("./Index");
        }

        private async Task LoadSelectListsAsync()
        {
            try
            {
                var patients = await _patientService.GetAllPatientsAsync();
                PatientsSelectList = new SelectList(
                    patients.Select(p => new { 
                        Value = p.Id, 
                        Text = $"{p.LastName} {p.FirstName} {p.MiddleName} - №{p.MedicalCardNumber}" 
                    }),
                    "Value", 
                    "Text"
                );

                StatusSelectList = new SelectList(
                    Enum.GetValues<VisitStatus>().Select(s => new { 
                        Value = s.ToString(), 
                        Text = GetStatusDisplayName(s) 
                    }),
                    "Value", 
                    "Text"
                );
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error loading select lists for visit editing");
                PatientsSelectList = new SelectList(Enumerable.Empty<object>(), "Value", "Text");
                StatusSelectList = new SelectList(Enumerable.Empty<object>(), "Value", "Text");
            }
        }

        private static string GetStatusDisplayName(VisitStatus status)
        {
            return status switch
            {
                VisitStatus.Scheduled => "Запланирован",
                VisitStatus.InProgress => "В процессе",
                VisitStatus.Completed => "Завершен",
                VisitStatus.Cancelled => "Отменен",
                VisitStatus.NoShow => "Не явился",
                _ => status.ToString()
            };
        }
    }
} 