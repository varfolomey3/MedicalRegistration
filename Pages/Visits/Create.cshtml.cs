using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using MedicalRegistration.Data;
using MedicalRegistration.Models;
using MedicalRegistration.Services;
using Microsoft.AspNetCore.Identity;

namespace MedicalRegistration.Pages.Visits
{
    public class CreateModel : PageModel
    {
        private readonly IVisitService _visitService;
        private readonly IPatientService _patientService;
        private readonly ILogger<CreateModel> _logger;
        private readonly UserManager<ApplicationUser> _userManager;

        public CreateModel(IVisitService visitService, IPatientService patientService, 
                          ILogger<CreateModel> logger, UserManager<ApplicationUser> userManager)
        {
            _visitService = visitService;
            _patientService = patientService;
            _logger = logger;
            _userManager = userManager;
        }

        [BindProperty]
        public Visit Visit { get; set; } = new Visit();

        public SelectList? PatientsSelectList { get; set; }
        public SelectList? StatusSelectList { get; set; }
        public bool IsPatientUser { get; set; }

        public async Task<IActionResult> OnGetAsync(int? patientId)
        {
            // Проверяем, является ли текущий пользователь пациентом
            IsPatientUser = User.IsInRole(UserRoles.Patient);
            
            if (IsPatientUser)
            {
                // Для пациентов находим их запись в таблице пациентов
                var currentUser = await _userManager.GetUserAsync(User);
                if (currentUser?.MedicalCardNumber != null)
                {
                    var patient = await _patientService.GetPatientByMedicalCardNumberAsync(currentUser.MedicalCardNumber);
                    if (patient != null)
                    {
                        Visit.PatientId = patient.Id;
                        _logger.LogInformation("Auto-selected patient {PatientId} for current user", patient.Id);
                    }
                    else
                    {
                        _logger.LogWarning("Patient record not found for user with medical card {MedicalCardNumber}", 
                                         currentUser.MedicalCardNumber);
                        TempData["ErrorMessage"] = "Не найдена запись пациента. Обратитесь к администратору.";
                        return RedirectToPage("/Index");
                    }
                }
            }
            else
            {
                await LoadSelectListsAsync();
            }
            
            // Устанавливаем значения по умолчанию без миллисекунд
            var nextHour = DateTime.Now.AddHours(1);
            Visit.VisitDateTime = new DateTime(nextHour.Year, nextHour.Month, nextHour.Day, 
                                              nextHour.Hour, 0, 0); // Убираем минуты, секунды и миллисекунды
            Visit.Status = VisitStatus.Scheduled;
            
            // Если передан ID пациента (только для врачей), предварительно выбираем его
            if (patientId.HasValue && !IsPatientUser)
            {
                Visit.PatientId = patientId.Value;
                _logger.LogInformation("Pre-selecting patient {PatientId} for visit creation", patientId.Value);
            }
            
            return Page();
        }

        public async Task<IActionResult> OnPostAsync()
        {
            // Проверяем, является ли текущий пользователь пациентом
            IsPatientUser = User.IsInRole(UserRoles.Patient);
            
            _logger.LogInformation("Attempting to create visit for patient {PatientId}", Visit.PatientId);
            
            if (!ModelState.IsValid)
            {
                _logger.LogWarning("ModelState is invalid. Errors: {Errors}", 
                    string.Join(", ", ModelState.SelectMany(x => x.Value?.Errors ?? new Microsoft.AspNetCore.Mvc.ModelBinding.ModelErrorCollection()).Select(e => e.ErrorMessage)));
                if (!IsPatientUser)
                {
                    await LoadSelectListsAsync();
                }
                return Page();
            }

            try
            {
                _logger.LogInformation("Creating visit: PatientId={PatientId}, DateTime={DateTime}, Doctor={Doctor}, Specialty={Specialty}", 
                    Visit.PatientId, Visit.VisitDateTime, Visit.DoctorName, Visit.Specialty);
                
                await _visitService.CreateVisitAsync(Visit);
                TempData["SuccessMessage"] = "Визит успешно создан.";
                _logger.LogInformation("Visit created successfully for patient {PatientId}", Visit.PatientId);
                return RedirectToPage("./Index");
            }
            catch (InvalidOperationException ex)
            {
                _logger.LogWarning(ex, "Invalid operation when creating visit");
                ModelState.AddModelError("", ex.Message);
                TempData["ErrorMessage"] = ex.Message;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error creating visit");
                ModelState.AddModelError("", "Произошла ошибка при создании визита. Попробуйте еще раз.");
                TempData["ErrorMessage"] = "Произошла ошибка при создании визита: " + ex.Message;
            }

            if (!IsPatientUser)
            {
                await LoadSelectListsAsync();
            }
            return Page();
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

                // Исключаем NoShow при создании визита - этот статус устанавливается только после неявки
                StatusSelectList = new SelectList(
                    Enum.GetValues<VisitStatus>()
                        .Where(s => s != VisitStatus.NoShow)
                        .Select(s => new { 
                            Value = s.ToString(), 
                            Text = GetStatusDisplayName(s) 
                        }),
                    "Value", 
                    "Text"
                );
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error loading select lists for visit creation");
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