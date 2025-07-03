using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using MedicalRegistration.Models;
using MedicalRegistration.Services;

namespace MedicalRegistration.Pages.Visits
{
    public class IndexModel : PageModel
    {
        private readonly IVisitService _visitService;
        private readonly ILogger<IndexModel> _logger;

        public IndexModel(IVisitService visitService, ILogger<IndexModel> logger)
        {
            _visitService = visitService;
            _logger = logger;
        }

        public IEnumerable<Visit> Visits { get; set; } = new List<Visit>();
        
        [BindProperty(SupportsGet = true)]
        public string? SearchTerm { get; set; }
        
        [BindProperty(SupportsGet = true)]
        public int? SelectedStatus { get; set; }
        
        [BindProperty(SupportsGet = true)]
        public DateTime? StartDate { get; set; }
        
        [BindProperty(SupportsGet = true)]
        public DateTime? EndDate { get; set; }

        // Statistics
        public int TotalVisits { get; set; }
        public int ScheduledVisits { get; set; }
        public int InProgressVisits { get; set; }
        public int CompletedVisits { get; set; }

        public async Task OnGetAsync()
        {
            try
            {
                await LoadVisitsAsync();
                await LoadStatisticsAsync();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error loading visits page");
                TempData["ErrorMessage"] = "Произошла ошибка при загрузке списка визитов.";
                Visits = new List<Visit>();
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

            return RedirectToPage();
        }

        private async Task LoadVisitsAsync()
        {
            // Apply filters and search
            if (!string.IsNullOrEmpty(SearchTerm))
            {
                Visits = await _visitService.SearchVisitsAsync(SearchTerm);
            }
            else if (SelectedStatus.HasValue && StartDate.HasValue && EndDate.HasValue)
            {
                // Filter by status and date range
                var allVisits = await _visitService.GetVisitsByDateRangeAsync(StartDate.Value, EndDate.Value);
                Visits = allVisits.Where(v => v.Status == (VisitStatus)SelectedStatus.Value);
            }
            else if (SelectedStatus.HasValue)
            {
                // Filter by status only
                Visits = await _visitService.GetVisitsByStatusAsync((VisitStatus)SelectedStatus.Value);
            }
            else if (StartDate.HasValue && EndDate.HasValue)
            {
                // Filter by date range only
                Visits = await _visitService.GetVisitsByDateRangeAsync(StartDate.Value, EndDate.Value);
            }
            else
            {
                // Load all visits
                Visits = await _visitService.GetAllVisitsAsync();
            }

            // Additional filtering if multiple criteria are set
            if (!string.IsNullOrEmpty(SearchTerm) && SelectedStatus.HasValue)
            {
                Visits = Visits.Where(v => v.Status == (VisitStatus)SelectedStatus.Value);
            }

            if (!string.IsNullOrEmpty(SearchTerm) && StartDate.HasValue && EndDate.HasValue)
            {
                Visits = Visits.Where(v => v.VisitDateTime.Date >= StartDate.Value.Date && 
                                         v.VisitDateTime.Date <= EndDate.Value.Date);
            }
        }

        private async Task LoadStatisticsAsync()
        {
            try
            {
                var allVisits = await _visitService.GetAllVisitsAsync();
                
                TotalVisits = allVisits.Count();
                ScheduledVisits = allVisits.Count(v => v.Status == VisitStatus.Scheduled);
                InProgressVisits = allVisits.Count(v => v.Status == VisitStatus.InProgress);
                CompletedVisits = allVisits.Count(v => v.Status == VisitStatus.Completed);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error loading visit statistics");
                // Set default values if statistics loading fails
                TotalVisits = ScheduledVisits = InProgressVisits = CompletedVisits = 0;
            }
        }
    }
} 