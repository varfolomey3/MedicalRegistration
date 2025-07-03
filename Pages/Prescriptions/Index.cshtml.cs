using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.AspNetCore.Mvc.Rendering;
using MedicalRegistration.Models;
using MedicalRegistration.Services;

namespace MedicalRegistration.Pages.Prescriptions
{
    public class IndexModel : PageModel
    {
        private readonly IPrescriptionService _prescriptionService;
        private readonly ILogger<IndexModel> _logger;

        public IndexModel(IPrescriptionService prescriptionService, ILogger<IndexModel> logger)
        {
            _prescriptionService = prescriptionService;
            _logger = logger;
        }

        public IEnumerable<Prescription> Prescriptions { get; set; } = new List<Prescription>();
        public PrescriptionStatistics? Statistics { get; set; }

        [BindProperty(SupportsGet = true)]
        public string? SearchTerm { get; set; }

        [BindProperty(SupportsGet = true)]
        public int? SelectedType { get; set; }

        [BindProperty(SupportsGet = true)]
        public int? SelectedStatus { get; set; }

        [BindProperty(SupportsGet = true)]
        public int? SelectedPriority { get; set; }

        [BindProperty(SupportsGet = true)]
        public DateTime? StartDate { get; set; }

        [BindProperty(SupportsGet = true)]
        public DateTime? EndDate { get; set; }

        public SelectList TypeSelectList { get; set; } = default!;
        public SelectList StatusSelectList { get; set; } = default!;
        public SelectList PrioritySelectList { get; set; } = default!;

        public async Task<IActionResult> OnGetAsync()
        {
            try
            {
                await LoadPrescriptionsAsync();
                await LoadStatisticsAsync();
                LoadSelectLists();
                return Page();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error loading prescriptions index page");
                TempData["ErrorMessage"] = "Произошла ошибка при загрузке списка назначений.";
                return Page();
            }
        }

        public async Task<IActionResult> OnPostDeleteAsync(int id)
        {
            try
            {
                var success = await _prescriptionService.DeletePrescriptionAsync(id);
                if (success)
                {
                    TempData["SuccessMessage"] = "Назначение успешно удалено.";
                    _logger.LogInformation("Prescription {PrescriptionId} deleted successfully", id);
                }
                else
                {
                    TempData["ErrorMessage"] = "Назначение не найдено.";
                    _logger.LogWarning("Attempted to delete non-existent prescription {PrescriptionId}", id);
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error deleting prescription {PrescriptionId}", id);
                TempData["ErrorMessage"] = "Произошла ошибка при удалении назначения.";
            }

            return RedirectToPage();
        }

        public async Task<IActionResult> OnPostUpdateStatusAsync(int id, PrescriptionStatus status)
        {
            try
            {
                var success = await _prescriptionService.UpdatePrescriptionStatusAsync(id, status);
                if (success)
                {
                    TempData["SuccessMessage"] = $"Статус назначения обновлен на '{GetStatusDisplayName(status)}'.";
                    _logger.LogInformation("Prescription {PrescriptionId} status updated to {Status}", id, status);
                }
                else
                {
                    TempData["ErrorMessage"] = "Назначение не найдено.";
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error updating prescription {PrescriptionId} status", id);
                TempData["ErrorMessage"] = "Произошла ошибка при обновлении статуса назначения.";
            }

            return RedirectToPage();
        }

        private async Task LoadPrescriptionsAsync()
        {
            try
            {
                // Apply filters and search
                if (!string.IsNullOrEmpty(SearchTerm))
                {
                    Prescriptions = await _prescriptionService.SearchPrescriptionsAsync(SearchTerm);
                }
                else if (SelectedType.HasValue && SelectedStatus.HasValue && SelectedPriority.HasValue)
                {
                    // Multiple filters
                    var allPrescriptions = await _prescriptionService.GetAllPrescriptionsAsync();
                    Prescriptions = allPrescriptions.Where(p => 
                        p.Type == (PrescriptionType)SelectedType.Value &&
                        p.Status == (PrescriptionStatus)SelectedStatus.Value &&
                        p.Priority == (Priority)SelectedPriority.Value);
                }
                else if (SelectedType.HasValue && StartDate.HasValue && EndDate.HasValue)
                {
                    // Filter by type and date range
                    var prescriptionsByDate = await _prescriptionService.GetPrescriptionsByDateRangeAsync(StartDate.Value, EndDate.Value);
                    Prescriptions = prescriptionsByDate.Where(p => p.Type == (PrescriptionType)SelectedType.Value);
                }
                else if (SelectedType.HasValue)
                {
                    // Filter by type only
                    Prescriptions = await _prescriptionService.GetPrescriptionsByTypeAsync((PrescriptionType)SelectedType.Value);
                }
                else if (SelectedStatus.HasValue)
                {
                    // Filter by status only
                    Prescriptions = await _prescriptionService.GetPrescriptionsByStatusAsync((PrescriptionStatus)SelectedStatus.Value);
                }
                else if (SelectedPriority.HasValue)
                {
                    // Filter by priority only
                    Prescriptions = await _prescriptionService.GetPrescriptionsByPriorityAsync((Priority)SelectedPriority.Value);
                }
                else if (StartDate.HasValue && EndDate.HasValue)
                {
                    // Filter by date range only
                    Prescriptions = await _prescriptionService.GetPrescriptionsByDateRangeAsync(StartDate.Value, EndDate.Value);
                }
                else
                {
                    // Load all prescriptions
                    Prescriptions = await _prescriptionService.GetAllPrescriptionsAsync();
                }

                // Additional filtering if multiple criteria are set
                ApplyAdditionalFilters();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error loading prescriptions with filters");
                Prescriptions = new List<Prescription>();
            }
        }

        private void ApplyAdditionalFilters()
        {
            if (!string.IsNullOrEmpty(SearchTerm) && SelectedType.HasValue)
            {
                Prescriptions = Prescriptions.Where(p => p.Type == (PrescriptionType)SelectedType.Value);
            }

            if (!string.IsNullOrEmpty(SearchTerm) && SelectedStatus.HasValue)
            {
                Prescriptions = Prescriptions.Where(p => p.Status == (PrescriptionStatus)SelectedStatus.Value);
            }

            if (!string.IsNullOrEmpty(SearchTerm) && StartDate.HasValue && EndDate.HasValue)
            {
                Prescriptions = Prescriptions.Where(p => p.StartDate.Date >= StartDate.Value.Date && 
                                                        p.StartDate.Date <= EndDate.Value.Date);
            }
        }

        private async Task LoadStatisticsAsync()
        {
            try
            {
                Statistics = await _prescriptionService.GetPrescriptionStatisticsAsync();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error loading prescription statistics");
                Statistics = null;
            }
        }

        private void LoadSelectLists()
        {
            TypeSelectList = new SelectList(
                Enum.GetValues<PrescriptionType>().Select(t => new { 
                    Value = (int)t, 
                    Text = GetTypeDisplayName(t) 
                }),
                "Value", 
                "Text"
            );

            StatusSelectList = new SelectList(
                Enum.GetValues<PrescriptionStatus>().Select(s => new { 
                    Value = (int)s, 
                    Text = GetStatusDisplayName(s) 
                }),
                "Value", 
                "Text"
            );

            PrioritySelectList = new SelectList(
                Enum.GetValues<Priority>().Select(p => new { 
                    Value = (int)p, 
                    Text = GetPriorityDisplayName(p) 
                }),
                "Value", 
                "Text"
            );
        }

        public static string GetTypeDisplayName(PrescriptionType type)
        {
            return type switch
            {
                PrescriptionType.Medication => "Медикаменты",
                PrescriptionType.Procedure => "Процедуры",
                PrescriptionType.LabTest => "Анализы",
                PrescriptionType.Diagnostic => "Диагностика",
                PrescriptionType.Physiotherapy => "Физиотерапия",
                PrescriptionType.Consultation => "Консультации",
                PrescriptionType.Surgery => "Хирургия",
                PrescriptionType.Rehabilitation => "Реабилитация",
                _ => type.ToString()
            };
        }

        public static string GetStatusDisplayName(PrescriptionStatus status)
        {
            return status switch
            {
                PrescriptionStatus.Active => "Активно",
                PrescriptionStatus.Completed => "Выполнено",
                PrescriptionStatus.Cancelled => "Отменено",
                PrescriptionStatus.Suspended => "Приостановлено",
                PrescriptionStatus.Expired => "Просрочено",
                _ => status.ToString()
            };
        }

        public static string GetPriorityDisplayName(Priority priority)
        {
            return priority switch
            {
                Priority.Low => "Низкий",
                Priority.Normal => "Обычный",
                Priority.High => "Высокий",
                Priority.Critical => "Критический",
                _ => priority.ToString()
            };
        }

        public static string GetStatusCssClass(PrescriptionStatus status)
        {
            return status switch
            {
                PrescriptionStatus.Active => "bg-success",
                PrescriptionStatus.Completed => "bg-primary",
                PrescriptionStatus.Cancelled => "bg-danger",
                PrescriptionStatus.Suspended => "bg-warning",
                PrescriptionStatus.Expired => "bg-secondary",
                _ => "bg-secondary"
            };
        }

        public static string GetPriorityCssClass(Priority priority)
        {
            return priority switch
            {
                Priority.Low => "text-muted",
                Priority.Normal => "text-primary",
                Priority.High => "text-warning",
                Priority.Critical => "text-danger",
                _ => "text-secondary"
            };
        }

        public static string GetTypeCssClass(PrescriptionType type)
        {
            return type switch
            {
                PrescriptionType.Medication => "bg-info",
                PrescriptionType.Procedure => "bg-primary",
                PrescriptionType.LabTest => "bg-warning",
                PrescriptionType.Diagnostic => "bg-secondary",
                PrescriptionType.Physiotherapy => "bg-success",
                PrescriptionType.Consultation => "bg-info",
                PrescriptionType.Surgery => "bg-danger",
                PrescriptionType.Rehabilitation => "bg-dark",
                _ => "bg-secondary"
            };
        }
    }
} 