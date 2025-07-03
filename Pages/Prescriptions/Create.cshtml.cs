using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.AspNetCore.Mvc.Rendering;
using MedicalRegistration.Models;
using MedicalRegistration.Services;

namespace MedicalRegistration.Pages.Prescriptions
{
    [Authorize(Roles = UserRoles.Doctor)]
    public class CreateModel : PageModel
    {
        private readonly IPrescriptionService _prescriptionService;
        private readonly IVisitService _visitService;
        private readonly IPatientService _patientService;
        private readonly ILogger<CreateModel> _logger;

        public CreateModel(IPrescriptionService prescriptionService, IVisitService visitService,
                          IPatientService patientService, ILogger<CreateModel> logger)
        {
            _prescriptionService = prescriptionService;
            _visitService = visitService;
            _patientService = patientService;
            _logger = logger;
        }

        [BindProperty]
        public Prescription Prescription { get; set; } = new Prescription();

        public SelectList? VisitsSelectList { get; set; }
        public SelectList? TypeSelectList { get; set; }
        public SelectList? StatusSelectList { get; set; }
        public SelectList? PrioritySelectList { get; set; }

        public async Task<IActionResult> OnGetAsync(int? patientId, int? visitId)
        {
            try
            {
                await LoadSelectListsAsync(patientId);
                
                // Устанавливаем значения по умолчанию
                var now = DateTime.Now;
                Prescription.StartDate = new DateTime(now.Year, now.Month, now.Day, now.Hour, now.Minute, 0);
                Prescription.Status = PrescriptionStatus.Active;
                Prescription.Priority = Priority.Normal;
                Prescription.Type = PrescriptionType.Medication;
                
                // Если передан ID визита, предварительно выбираем его
                if (visitId.HasValue)
                {
                    Prescription.VisitId = visitId.Value;
                    _logger.LogInformation("Pre-selecting visit {VisitId} for prescription creation", visitId.Value);
                }
                // Если передан только ID пациента, попробуем найти последний визит
                else if (patientId.HasValue)
                {
                    var patientVisits = await _visitService.GetVisitsByPatientIdAsync(patientId.Value);
                    var lastVisit = patientVisits.OrderByDescending(v => v.VisitDateTime).FirstOrDefault();
                    if (lastVisit != null)
                    {
                        Prescription.VisitId = lastVisit.Id;
                        _logger.LogInformation("Pre-selecting last visit {VisitId} for patient {PatientId}", lastVisit.Id, patientId.Value);
                    }
                }
                
                return Page();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error loading prescription creation page");
                TempData["ErrorMessage"] = "Произошла ошибка при загрузке страницы создания назначения.";
                return RedirectToPage("./Index");
            }
        }

        public async Task<IActionResult> OnPostAsync()
        {
            _logger.LogInformation("Attempting to create prescription for visit {VisitId}", Prescription.VisitId);
            
            if (!ModelState.IsValid)
            {
                _logger.LogWarning("ModelState is invalid for prescription creation");
                await LoadSelectListsAsync();
                return Page();
            }

            try
            {
                await _prescriptionService.CreatePrescriptionAsync(Prescription);
                TempData["SuccessMessage"] = "Назначение успешно создано.";
                _logger.LogInformation("Prescription created successfully for visit {VisitId}", Prescription.VisitId);
                return RedirectToPage("./Index");
            }
            catch (InvalidOperationException ex)
            {
                _logger.LogWarning(ex, "Invalid operation when creating prescription");
                ModelState.AddModelError("", ex.Message);
                TempData["ErrorMessage"] = ex.Message;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error creating prescription");
                ModelState.AddModelError("", "Произошла ошибка при создании назначения. Попробуйте еще раз.");
                TempData["ErrorMessage"] = "Произошла ошибка при создании назначения: " + ex.Message;
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
                    Enum.GetValues<PrescriptionType>().Select(t => new { 
                        Value = t.ToString(), 
                        Text = GetTypeDisplayName(t) 
                    }),
                    "Value", 
                    "Text"
                );

                StatusSelectList = new SelectList(
                    Enum.GetValues<PrescriptionStatus>().Select(s => new { 
                        Value = s.ToString(), 
                        Text = GetStatusDisplayName(s) 
                    }),
                    "Value", 
                    "Text"
                );

                PrioritySelectList = new SelectList(
                    Enum.GetValues<Priority>().Select(p => new { 
                        Value = p.ToString(), 
                        Text = GetPriorityDisplayName(p) 
                    }),
                    "Value", 
                    "Text"
                );
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error loading select lists for prescription creation");
                VisitsSelectList = new SelectList(Enumerable.Empty<object>(), "Value", "Text");
                TypeSelectList = new SelectList(Enumerable.Empty<object>(), "Value", "Text");
                StatusSelectList = new SelectList(Enumerable.Empty<object>(), "Value", "Text");
                PrioritySelectList = new SelectList(Enumerable.Empty<object>(), "Value", "Text");
            }
        }

        private static string GetTypeDisplayName(PrescriptionType type)
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

        private static string GetStatusDisplayName(PrescriptionStatus status)
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

        private static string GetPriorityDisplayName(Priority priority)
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
    }
} 