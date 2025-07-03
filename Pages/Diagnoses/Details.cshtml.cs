using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using MedicalRegistration.Models;
using MedicalRegistration.Services;

namespace MedicalRegistration.Pages.Diagnoses
{
    public class DetailsModel : PageModel
    {
        private readonly IDiagnosisService _diagnosisService;
        private readonly ILogger<DetailsModel> _logger;

        public DetailsModel(IDiagnosisService diagnosisService, ILogger<DetailsModel> logger)
        {
            _diagnosisService = diagnosisService;
            _logger = logger;
        }

        public Diagnosis Diagnosis { get; set; } = null!;

        public async Task<IActionResult> OnGetAsync(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            try
            {
                var diagnosis = await _diagnosisService.GetDiagnosisByIdAsync(id.Value);
                if (diagnosis == null)
                {
                    _logger.LogWarning("Diagnosis with ID {DiagnosisId} not found", id);
                    return NotFound();
                }

                Diagnosis = diagnosis;
                return Page();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error loading diagnosis details for ID {DiagnosisId}", id);
                TempData["ErrorMessage"] = "Произошла ошибка при загрузке деталей диагноза.";
                return RedirectToPage("./Index");
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

            return RedirectToPage("./Index");
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

        public string GetSeverityDisplayName(SeverityLevel? severity)
        {
            return severity switch
            {
                SeverityLevel.Mild => "Легкая",
                SeverityLevel.Moderate => "Средняя",
                SeverityLevel.Severe => "Тяжелая",
                SeverityLevel.Critical => "Критическая",
                null => "Не указана",
                _ => severity?.ToString() ?? "Не указана"
            };
        }
    }
} 