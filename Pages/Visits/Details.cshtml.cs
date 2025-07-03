using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using MedicalRegistration.Models;
using MedicalRegistration.Services;

namespace MedicalRegistration.Pages.Visits
{
    public class DetailsModel : PageModel
    {
        private readonly IVisitService _visitService;
        private readonly ILogger<DetailsModel> _logger;

        public DetailsModel(IVisitService visitService, ILogger<DetailsModel> logger)
        {
            _visitService = visitService;
            _logger = logger;
        }

        public Visit? Visit { get; set; }

        public async Task<IActionResult> OnGetAsync(int? id)
        {
            if (id == null)
            {
                _logger.LogWarning("Attempt to access visit details without ID");
                return NotFound();
            }

            try
            {
                Visit = await _visitService.GetVisitByIdAsync(id.Value);

                if (Visit == null)
                {
                    _logger.LogWarning("Visit {VisitId} not found", id.Value);
                    return NotFound();
                }

                _logger.LogInformation("Loading details for visit {VisitId}", id.Value);
                return Page();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error loading visit {VisitId} details", id.Value);
                TempData["ErrorMessage"] = "Произошла ошибка при загрузке информации о визите.";
                return RedirectToPage("./Index");
            }
        }

        public async Task<IActionResult> OnPostDeleteAsync(int id)
        {
            try
            {
                var result = await _visitService.DeleteVisitAsync(id);
                if (result)
                {
                    TempData["SuccessMessage"] = "Визит успешно удален.";
                    _logger.LogInformation("Visit {VisitId} deleted successfully", id);
                }
                else
                {
                    TempData["ErrorMessage"] = "Визит не найден.";
                    _logger.LogWarning("Attempt to delete non-existent visit {VisitId}", id);
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error deleting visit {VisitId}", id);
                TempData["ErrorMessage"] = "Произошла ошибка при удалении визита.";
            }

            return RedirectToPage("./Index");
        }

        public string GetStatusBadgeClass(VisitStatus status)
        {
            return status switch
            {
                VisitStatus.Scheduled => "bg-warning",
                VisitStatus.InProgress => "bg-primary",
                VisitStatus.Completed => "bg-success",
                VisitStatus.Cancelled => "bg-danger",
                VisitStatus.NoShow => "bg-dark",
                _ => "bg-secondary"
            };
        }

        public string GetStatusDisplayName(VisitStatus status)
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