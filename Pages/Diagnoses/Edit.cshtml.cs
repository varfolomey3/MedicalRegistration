using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.AspNetCore.Mvc.Rendering;
using MedicalRegistration.Models;
using MedicalRegistration.Services;

namespace MedicalRegistration.Pages.Diagnoses
{
    public class EditModel : PageModel
    {
        private readonly IDiagnosisService _diagnosisService;
        private readonly IVisitService _visitService;
        private readonly ILogger<EditModel> _logger;

        public EditModel(IDiagnosisService diagnosisService, IVisitService visitService, ILogger<EditModel> logger)
        {
            _diagnosisService = diagnosisService;
            _visitService = visitService;
            _logger = logger;
        }

        [BindProperty]
        public Diagnosis Diagnosis { get; set; } = null!;

        public SelectList? VisitsSelectList { get; set; }
        public SelectList? TypeSelectList { get; set; }
        public SelectList? SeveritySelectList { get; set; }

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
                await LoadSelectListsAsync();
                return Page();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error loading diagnosis for editing with ID {DiagnosisId}", id);
                TempData["ErrorMessage"] = "Произошла ошибка при загрузке диагноза для редактирования.";
                return RedirectToPage("./Index");
            }
        }

        public async Task<IActionResult> OnPostAsync()
        {
            _logger.LogInformation("Attempting to update diagnosis {DiagnosisId}", Diagnosis.Id);

            if (!ModelState.IsValid)
            {
                _logger.LogWarning("ModelState is invalid for diagnosis update");
                await LoadSelectListsAsync();
                return Page();
            }

            try
            {
                await _diagnosisService.UpdateDiagnosisAsync(Diagnosis);
                TempData["SuccessMessage"] = "Диагноз успешно обновлен.";
                _logger.LogInformation("Diagnosis {DiagnosisId} updated successfully", Diagnosis.Id);
                return RedirectToPage("./Details", new { id = Diagnosis.Id });
            }
            catch (InvalidOperationException ex)
            {
                _logger.LogWarning(ex, "Invalid operation when updating diagnosis");
                ModelState.AddModelError("", ex.Message);
                TempData["ErrorMessage"] = ex.Message;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error updating diagnosis");
                ModelState.AddModelError("", "Произошла ошибка при обновлении диагноза. Попробуйте еще раз.");
                TempData["ErrorMessage"] = "Произошла ошибка при обновлении диагноза: " + ex.Message;
            }

            await LoadSelectListsAsync();
            return Page();
        }

        private async Task LoadSelectListsAsync()
        {
            try
            {
                var visits = await _visitService.GetAllVisitsAsync();

                VisitsSelectList = new SelectList(
                    visits.Select(v => new { 
                        Value = v.Id, 
                        Text = $"{v.Patient?.FullName} - {v.VisitDateTime:dd.MM.yyyy HH:mm} ({v.DoctorName})" 
                    }).OrderByDescending(v => v.Text),
                    "Value", 
                    "Text",
                    Diagnosis?.VisitId
                );

                TypeSelectList = new SelectList(
                    Enum.GetValues<DiagnosisType>().Select(t => new { 
                        Value = t.ToString(), 
                        Text = GetTypeDisplayName(t) 
                    }),
                    "Value", 
                    "Text",
                    Diagnosis?.Type.ToString()
                );

                SeveritySelectList = new SelectList(
                    new[] { new { Value = "", Text = "Не указана" } }
                    .Concat(Enum.GetValues<SeverityLevel>().Select(s => new { 
                        Value = s.ToString(), 
                        Text = GetSeverityDisplayName(s) 
                    })),
                    "Value", 
                    "Text",
                    Diagnosis?.Severity?.ToString() ?? ""
                );
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error loading select lists for diagnosis editing");
                VisitsSelectList = new SelectList(Enumerable.Empty<object>(), "Value", "Text");
                TypeSelectList = new SelectList(Enumerable.Empty<object>(), "Value", "Text");
                SeveritySelectList = new SelectList(Enumerable.Empty<object>(), "Value", "Text");
            }
        }

        private static string GetTypeDisplayName(DiagnosisType type)
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

        private static string GetSeverityDisplayName(SeverityLevel severity)
        {
            return severity switch
            {
                SeverityLevel.Mild => "Легкая",
                SeverityLevel.Moderate => "Средняя",
                SeverityLevel.Severe => "Тяжелая",
                SeverityLevel.Critical => "Критическая",
                _ => severity.ToString()
            };
        }
    }
} 