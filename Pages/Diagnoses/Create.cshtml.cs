using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.AspNetCore.Mvc.Rendering;
using MedicalRegistration.Models;
using MedicalRegistration.Services;

namespace MedicalRegistration.Pages.Diagnoses
{
    [Authorize(Roles = UserRoles.Doctor)]
    public class CreateModel : PageModel
    {
        private readonly IDiagnosisService _diagnosisService;
        private readonly IVisitService _visitService;
        private readonly IPatientService _patientService;
        private readonly ILogger<CreateModel> _logger;

        public CreateModel(IDiagnosisService diagnosisService, IVisitService visitService, 
                          IPatientService patientService, ILogger<CreateModel> logger)
        {
            _diagnosisService = diagnosisService;
            _visitService = visitService;
            _patientService = patientService;
            _logger = logger;
        }

        [BindProperty]
        public Diagnosis Diagnosis { get; set; } = new Diagnosis();

        public SelectList? VisitsSelectList { get; set; }
        public SelectList? TypeSelectList { get; set; }
        public SelectList? SeveritySelectList { get; set; }

        public async Task<IActionResult> OnGetAsync(int? patientId, int? visitId)
        {
            try
            {
                await LoadSelectListsAsync(patientId);
                
                // Устанавливаем значения по умолчанию
                var now = DateTime.Now;
                Diagnosis.DiagnosisDate = new DateTime(now.Year, now.Month, now.Day, now.Hour, now.Minute, 0);
                Diagnosis.Type = DiagnosisType.Primary;
                
                // Если передан ID визита, предварительно выбираем его
                if (visitId.HasValue)
                {
                    Diagnosis.VisitId = visitId.Value;
                    _logger.LogInformation("Pre-selecting visit {VisitId} for diagnosis creation", visitId.Value);
                }
                // Если передан только ID пациента, попробуем найти последний визит
                else if (patientId.HasValue)
                {
                    var patientVisits = await _visitService.GetVisitsByPatientIdAsync(patientId.Value);
                    var lastVisit = patientVisits.OrderByDescending(v => v.VisitDateTime).FirstOrDefault();
                    if (lastVisit != null)
                    {
                        Diagnosis.VisitId = lastVisit.Id;
                        _logger.LogInformation("Pre-selecting last visit {VisitId} for patient {PatientId}", lastVisit.Id, patientId.Value);
                    }
                }
                
                return Page();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error loading diagnosis creation page");
                TempData["ErrorMessage"] = "Произошла ошибка при загрузке страницы создания диагноза.";
                return RedirectToPage("./Index");
            }
        }

        public async Task<IActionResult> OnPostAsync()
        {
            _logger.LogInformation("Attempting to create diagnosis for visit {VisitId}", Diagnosis.VisitId);
            
            if (!ModelState.IsValid)
            {
                _logger.LogWarning("ModelState is invalid for diagnosis creation");
                await LoadSelectListsAsync();
                return Page();
            }

            try
            {
                await _diagnosisService.CreateDiagnosisAsync(Diagnosis);
                TempData["SuccessMessage"] = "Диагноз успешно создан.";
                _logger.LogInformation("Diagnosis created successfully for visit {VisitId}", Diagnosis.VisitId);
                return RedirectToPage("./Index");
            }
            catch (InvalidOperationException ex)
            {
                _logger.LogWarning(ex, "Invalid operation when creating diagnosis");
                ModelState.AddModelError("", ex.Message);
                TempData["ErrorMessage"] = ex.Message;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error creating diagnosis");
                ModelState.AddModelError("", "Произошла ошибка при создании диагноза. Попробуйте еще раз.");
                TempData["ErrorMessage"] = "Произошла ошибка при создании диагноза: " + ex.Message;
            }

            await LoadSelectListsAsync();
            return Page();
        }

        private async Task LoadSelectListsAsync(int? patientId = null)
        {
            try
            {
                // Загружаем визиты, фильтруя по пациенту если нужно
                IEnumerable<Visit> visits;
                if (patientId.HasValue)
                {
                    visits = await _visitService.GetVisitsByPatientIdAsync(patientId.Value);
                }
                else
                {
                    visits = await _visitService.GetAllVisitsAsync();
                }

                VisitsSelectList = new SelectList(
                    visits.Select(v => new { 
                        Value = v.Id, 
                        Text = $"{v.Patient?.FullName} - {v.VisitDateTime:dd.MM.yyyy HH:mm} ({v.DoctorName})" 
                    }).OrderByDescending(v => v.Text),
                    "Value", 
                    "Text"
                );

                TypeSelectList = new SelectList(
                    Enum.GetValues<DiagnosisType>().Select(t => new { 
                        Value = t.ToString(), 
                        Text = GetTypeDisplayName(t) 
                    }),
                    "Value", 
                    "Text"
                );

                SeveritySelectList = new SelectList(
                    Enum.GetValues<SeverityLevel>().Select(s => new { 
                        Value = s.ToString(), 
                        Text = GetSeverityDisplayName(s) 
                    }),
                    "Value", 
                    "Text"
                );
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error loading select lists for diagnosis creation");
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