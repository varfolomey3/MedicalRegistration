using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using MedicalRegistration.Models;
using MedicalRegistration.Services;

namespace MedicalRegistration.Pages.Diagnoses
{
    public class IndexModel : PageModel
    {
        private readonly IDiagnosisService _diagnosisService;
        private readonly IVisitService _visitService;
        private readonly ILogger<IndexModel> _logger;

        public IndexModel(IDiagnosisService diagnosisService, IVisitService visitService, ILogger<IndexModel> logger)
        {
            _diagnosisService = diagnosisService;
            _visitService = visitService;
            _logger = logger;
        }

        public IEnumerable<Diagnosis> Diagnoses { get; set; } = new List<Diagnosis>();
        
        [BindProperty(SupportsGet = true)]
        public string? SearchTerm { get; set; }
        
        [BindProperty(SupportsGet = true)]
        public int? SelectedType { get; set; }
        
        [BindProperty(SupportsGet = true)]
        public DateTime? StartDate { get; set; }
        
        [BindProperty(SupportsGet = true)]
        public DateTime? EndDate { get; set; }

        // Statistics
        public int TotalDiagnoses { get; set; }
        public int PrimaryDiagnoses { get; set; }
        public int SecondaryDiagnoses { get; set; }
        public int DifferentialDiagnoses { get; set; }

        public async Task OnGetAsync()
        {
            try
            {
                await LoadDiagnosesAsync();
                await LoadStatisticsAsync();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error loading diagnoses page");
                TempData["ErrorMessage"] = "Произошла ошибка при загрузке списка диагнозов.";
                Diagnoses = new List<Diagnosis>();
            }
        }

        public async Task<IActionResult> OnPostDeleteAsync(int id)
        {
            try
            {
                var result = await _diagnosisService.DeleteDiagnosisAsync(id);
                if (result)
                {
                    TempData["SuccessMessage"] = "Диагноз успешно удален.";
                    _logger.LogInformation("Diagnosis {DiagnosisId} deleted successfully", id);
                }
                else
                {
                    TempData["ErrorMessage"] = "Диагноз не найден.";
                    _logger.LogWarning("Attempt to delete non-existent diagnosis {DiagnosisId}", id);
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error deleting diagnosis {DiagnosisId}", id);
                TempData["ErrorMessage"] = "Произошла ошибка при удалении диагноза.";
            }

            return RedirectToPage();
        }

        private async Task LoadDiagnosesAsync()
        {
            // Apply filters and search
            if (!string.IsNullOrEmpty(SearchTerm))
            {
                Diagnoses = await _diagnosisService.SearchDiagnosesAsync(SearchTerm);
            }
            else if (SelectedType.HasValue && StartDate.HasValue && EndDate.HasValue)
            {
                // Filter by type and date range
                var allDiagnoses = await _diagnosisService.GetDiagnosesByDateRangeAsync(StartDate.Value, EndDate.Value);
                Diagnoses = allDiagnoses.Where(d => d.Type == (DiagnosisType)SelectedType.Value);
            }
            else if (SelectedType.HasValue)
            {
                // Filter by type only
                Diagnoses = await _diagnosisService.GetDiagnosesByTypeAsync((DiagnosisType)SelectedType.Value);
            }
            else if (StartDate.HasValue && EndDate.HasValue)
            {
                // Filter by date range only
                Diagnoses = await _diagnosisService.GetDiagnosesByDateRangeAsync(StartDate.Value, EndDate.Value);
            }
            else
            {
                // Load all diagnoses
                Diagnoses = await _diagnosisService.GetAllDiagnosesAsync();
            }

            // Additional filtering if multiple criteria are set
            if (!string.IsNullOrEmpty(SearchTerm) && SelectedType.HasValue)
            {
                Diagnoses = Diagnoses.Where(d => d.Type == (DiagnosisType)SelectedType.Value);
            }

            if (!string.IsNullOrEmpty(SearchTerm) && StartDate.HasValue && EndDate.HasValue)
            {
                Diagnoses = Diagnoses.Where(d => d.DiagnosisDate.Date >= StartDate.Value.Date && 
                                               d.DiagnosisDate.Date <= EndDate.Value.Date);
            }
        }

        private async Task LoadStatisticsAsync()
        {
            try
            {
                var allDiagnoses = await _diagnosisService.GetAllDiagnosesAsync();
                
                TotalDiagnoses = allDiagnoses.Count();
                PrimaryDiagnoses = allDiagnoses.Count(d => d.Type == DiagnosisType.Primary);
                SecondaryDiagnoses = allDiagnoses.Count(d => d.Type == DiagnosisType.Secondary);
                DifferentialDiagnoses = allDiagnoses.Count(d => d.Type == DiagnosisType.Differential);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error loading diagnosis statistics");
                // Set default values if statistics loading fails
                TotalDiagnoses = PrimaryDiagnoses = SecondaryDiagnoses = DifferentialDiagnoses = 0;
            }
        }

        public string GetTypeDisplayName(DiagnosisType type)
        {
            return type switch
            {
                DiagnosisType.Primary => "Основной",
                DiagnosisType.Secondary => "Сопутствующий",
                DiagnosisType.Preliminary => "Предварительный",
                DiagnosisType.Differential => "Дифференциальный",
                _ => type.ToString()
            };
        }

        public string GetTypeBadgeClass(DiagnosisType type)
        {
            return type switch
            {
                DiagnosisType.Primary => "bg-danger",
                DiagnosisType.Secondary => "bg-warning",
                DiagnosisType.Preliminary => "bg-success",
                DiagnosisType.Differential => "bg-info",
                _ => "bg-secondary"
            };
        }
    }
} 