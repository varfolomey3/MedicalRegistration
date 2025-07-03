using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.AspNetCore.Mvc.Rendering;
using System.ComponentModel.DataAnnotations;
using MedicalRegistration.Models;
using MedicalRegistration.Services;

namespace MedicalRegistration.Pages.Account
{
    public class RegisterModel : PageModel
    {
        private readonly SignInManager<ApplicationUser> _signInManager;
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly RoleManager<IdentityRole> _roleManager;
        private readonly ILogger<RegisterModel> _logger;
        private readonly IPatientService _patientService;

        public RegisterModel(
            UserManager<ApplicationUser> userManager,
            SignInManager<ApplicationUser> signInManager,
            RoleManager<IdentityRole> roleManager,
            ILogger<RegisterModel> logger,
            IPatientService patientService)
        {
            _userManager = userManager;
            _signInManager = signInManager;
            _roleManager = roleManager;
            _logger = logger;
            _patientService = patientService;
        }

        [BindProperty]
        public InputModel Input { get; set; } = new InputModel();

        public string? ReturnUrl { get; set; }

        public SelectList? UserTypeList { get; set; }
        public SelectList? GenderList { get; set; }

        public class InputModel
        {
            [Required(ErrorMessage = "Email обязателен для заполнения")]
            [EmailAddress(ErrorMessage = "Некорректный email")]
            public string Email { get; set; } = string.Empty;

            [Required(ErrorMessage = "Пароль обязателен для заполнения")]
            [StringLength(100, ErrorMessage = "Пароль должен содержать минимум {2} и максимум {1} символов.", MinimumLength = 6)]
            [DataType(DataType.Password)]
            public string Password { get; set; } = string.Empty;

            [DataType(DataType.Password)]
            [Compare("Password", ErrorMessage = "Пароли не совпадают")]
            public string ConfirmPassword { get; set; } = string.Empty;

            [Required(ErrorMessage = "Имя обязательно для заполнения")]
            [StringLength(50, ErrorMessage = "Имя не может превышать 50 символов")]
            public string FirstName { get; set; } = string.Empty;

            [Required(ErrorMessage = "Фамилия обязательна для заполнения")]
            [StringLength(50, ErrorMessage = "Фамилия не может превышать 50 символов")]
            public string LastName { get; set; } = string.Empty;

            [StringLength(50, ErrorMessage = "Отчество не может превышать 50 символов")]
            public string? MiddleName { get; set; }

            [Required(ErrorMessage = "Дата рождения обязательна")]
            [DataType(DataType.Date)]
            public DateTime DateOfBirth { get; set; } = DateTime.Now.AddYears(-18);

            [Required(ErrorMessage = "Пол обязателен для заполнения")]
            public Gender Gender { get; set; }

            [Required(ErrorMessage = "Тип пользователя обязателен")]
            public UserType UserType { get; set; }

            [RegularExpression(@"^[\+]?[0-9\-\(\)\s]*$", ErrorMessage = "Некорректный номер телефона")]
            public string? PhoneNumber { get; set; }

            [StringLength(200, ErrorMessage = "Адрес не может превышать 200 символов")]
            public string? Address { get; set; }

            // Поля для врачей и пациентов - валидация в коде
            public string? Specialty { get; set; }
            public string? LicenseNumber { get; set; }
            public string? MedicalCardNumber { get; set; }
        }

        public async Task OnGetAsync(string? returnUrl = null)
        {
            ReturnUrl = returnUrl;
            await LoadSelectListsAsync();
        }

        public async Task<IActionResult> OnPostAsync(string? returnUrl = null)
        {
            returnUrl ??= Url.Content("~/");
            
            // Убираем из валидации поля, которые не относятся к выбранному типу пользователя
            if (Input.UserType == UserType.Doctor)
            {
                // Убираем ошибки для полей пациента
                ModelState.Remove("Input.MedicalCardNumber");
                Input.MedicalCardNumber = null;
                
                // Проверяем поля врача
                if (string.IsNullOrWhiteSpace(Input.Specialty))
                {
                    ModelState.AddModelError("Input.Specialty", "Специальность обязательна для врачей");
                }
                if (string.IsNullOrWhiteSpace(Input.LicenseNumber))
                {
                    ModelState.AddModelError("Input.LicenseNumber", "Номер лицензии обязателен для врачей");
                }
            }
            else if (Input.UserType == UserType.Patient)
            {
                // Убираем ошибки для полей врача
                ModelState.Remove("Input.Specialty");
                ModelState.Remove("Input.LicenseNumber");
                Input.Specialty = null;
                Input.LicenseNumber = null;
                
                // Проверяем поля пациента
                if (string.IsNullOrWhiteSpace(Input.MedicalCardNumber))
                {
                    ModelState.AddModelError("Input.MedicalCardNumber", "Номер медицинской карты обязателен для пациентов");
                }
            }

            if (ModelState.IsValid)
            {
                _logger.LogInformation("Creating user: {Email}, Type: {UserType}", Input.Email, Input.UserType);
                
                // Создаем роли если они не существуют
                await EnsureRolesCreatedAsync();
                
                var now = new DateTime(DateTime.Now.Year, DateTime.Now.Month, DateTime.Now.Day, 
                                     DateTime.Now.Hour, DateTime.Now.Minute, 0);
                
                var user = new ApplicationUser
                {
                    UserName = Input.Email,
                    Email = Input.Email,
                    FirstName = Input.FirstName,
                    LastName = Input.LastName,
                    MiddleName = Input.MiddleName,
                    DateOfBirth = Input.DateOfBirth,
                    Gender = Input.Gender,
                    UserType = Input.UserType,
                    PhoneNumber = Input.PhoneNumber,
                    Address = Input.Address,
                    Specialty = Input.Specialty,
                    LicenseNumber = Input.LicenseNumber,
                    MedicalCardNumber = Input.MedicalCardNumber,
                    RegisteredAt = now
                };

                var result = await _userManager.CreateAsync(user, Input.Password);

                if (result.Succeeded)
                {
                    _logger.LogInformation("User created successfully: {Email}", user.Email);

                    // Назначаем роль
                    var roleName = Input.UserType == UserType.Doctor ? UserRoles.Doctor : UserRoles.Patient;
                    var roleResult = await _userManager.AddToRoleAsync(user, roleName);
                    
                    if (roleResult.Succeeded)
                    {
                        // Если это пациент, создаем запись в таблице пациентов
                        if (Input.UserType == UserType.Patient)
                        {
                            try
                            {
                                var patient = new Patient
                                {
                                    FirstName = Input.FirstName,
                                    LastName = Input.LastName,
                                    MiddleName = Input.MiddleName,
                                    DateOfBirth = Input.DateOfBirth,
                                    Gender = Input.Gender,
                                    PhoneNumber = Input.PhoneNumber,
                                    Email = Input.Email,
                                    Address = Input.Address,
                                    MedicalCardNumber = Input.MedicalCardNumber!,
                                    CreatedAt = now,
                                    UpdatedAt = now
                                };

                                await _patientService.CreatePatientAsync(patient);
                                _logger.LogInformation("Patient record created for user {Email}", user.Email);
                            }
                            catch (Exception ex)
                            {
                                _logger.LogError(ex, "Failed to create patient record for user {Email}", user.Email);
                                // Не останавливаем процесс регистрации из-за этой ошибки
                            }
                        }

                        _logger.LogInformation("Role {Role} assigned to user {Email}", roleName, user.Email);
                        TempData["SuccessMessage"] = "Регистрация прошла успешно! Теперь вы можете войти в систему.";
                        return RedirectToPage("./Login");
                    }
                    else
                    {
                        _logger.LogError("Failed to assign role: {Errors}", 
                            string.Join(", ", roleResult.Errors.Select(e => e.Description)));
                        ModelState.AddModelError(string.Empty, "Ошибка при назначении роли пользователя");
                    }
                }
                else
                {
                    _logger.LogError("User creation failed: {Errors}", 
                        string.Join(", ", result.Errors.Select(e => e.Description)));
                    
                    foreach (var error in result.Errors)
                    {
                        if (error.Code == "DuplicateUserName" || error.Code == "DuplicateEmail")
                        {
                            ModelState.AddModelError(string.Empty, "Пользователь с таким email уже существует");
                        }
                        else
                        {
                            ModelState.AddModelError(string.Empty, error.Description);
                        }
                    }
                }
            }
            else
            {
                var errors = ModelState.Values.SelectMany(v => v.Errors).Select(e => e.ErrorMessage);
                _logger.LogWarning("Validation failed: {Errors}", string.Join(", ", errors));
            }

            await LoadSelectListsAsync();
            return Page();
        }

        private async Task LoadSelectListsAsync()
        {
            UserTypeList = new SelectList(Enum.GetValues(typeof(UserType))
                .Cast<UserType>()
                .Select(ut => new { Value = (int)ut, Text = GetUserTypeDisplayName(ut) }), 
                "Value", "Text");

            GenderList = new SelectList(Enum.GetValues(typeof(Gender))
                .Cast<Gender>()
                .Select(g => new { Value = (int)g, Text = GetGenderDisplayName(g) }), 
                "Value", "Text");
        }

        private static string GetUserTypeDisplayName(UserType userType)
        {
            return userType switch
            {
                UserType.Patient => "Пациент",
                UserType.Doctor => "Врач",
                _ => userType.ToString()
            };
        }

        private static string GetGenderDisplayName(Gender gender)
        {
            return gender switch
            {
                Gender.Male => "Мужской",
                Gender.Female => "Женский",
                _ => gender.ToString()
            };
        }

        private async Task EnsureRolesCreatedAsync()
        {
            if (!await _roleManager.RoleExistsAsync(UserRoles.Doctor))
            {
                await _roleManager.CreateAsync(new IdentityRole(UserRoles.Doctor));
            }

            if (!await _roleManager.RoleExistsAsync(UserRoles.Patient))
            {
                await _roleManager.CreateAsync(new IdentityRole(UserRoles.Patient));
            }
        }
    }
} 