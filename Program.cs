using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Identity;
using MedicalRegistration.Data;
using MedicalRegistration.Models;
using MedicalRegistration.Services;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddRazorPages();

// Add Entity Framework and SQLite
builder.Services.AddDbContext<MedicalDbContext>(options =>
    options.UseSqlite(builder.Configuration.GetConnectionString("DefaultConnection")));

// Добавляем Identity
builder.Services.AddDefaultIdentity<ApplicationUser>(options => 
{
    // Упрощенные настройки пароля для тестирования
    options.Password.RequireDigit = false;
    options.Password.RequireLowercase = false;
    options.Password.RequireNonAlphanumeric = false;
    options.Password.RequireUppercase = false;
    options.Password.RequiredLength = 3;
    options.Password.RequiredUniqueChars = 1;
    
    // Настройки пользователя
    options.User.RequireUniqueEmail = true;
    options.User.AllowedUserNameCharacters = "abcdefghijklmnopqrstuvwxyzABCDEFGHIJKLMNOPQRSTUVWXYZ0123456789-._@+";
    options.SignIn.RequireConfirmedEmail = false;
    options.SignIn.RequireConfirmedAccount = false;
})
.AddRoles<IdentityRole>()
.AddEntityFrameworkStores<MedicalDbContext>();

// Настройки авторизации
builder.Services.AddAuthorization(options =>
{
    options.AddPolicy("DoctorOnly", policy => policy.RequireRole(UserRoles.Doctor));
    options.AddPolicy("PatientOnly", policy => policy.RequireRole(UserRoles.Patient));
    options.AddPolicy("DoctorOrAdmin", policy => policy.RequireRole(UserRoles.Doctor, UserRoles.Admin));
});

// Register services (business logic layer)
builder.Services.AddScoped<IPatientService, PatientService>();
builder.Services.AddScoped<IVisitService, VisitService>();
builder.Services.AddScoped<IDiagnosisService, DiagnosisService>();
builder.Services.AddScoped<IPrescriptionService, PrescriptionService>();

var app = builder.Build();

// Инициализация тестовых данных
using (var scope = app.Services.CreateScope())
{
    var logger = scope.ServiceProvider.GetRequiredService<ILogger<Program>>();
    try
    {
        logger.LogInformation("Starting test data seeding...");
        await SeedData.SeedAsync(scope.ServiceProvider, logger);
        logger.LogInformation("Test data seeding completed successfully");
    }
    catch (Exception ex)
    {
        logger.LogError(ex, "Error occurred while seeding test data");
    }
}

// Configure the HTTP request pipeline.
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Error");
    // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
    app.UseHsts();
}

app.UseHttpsRedirection();

app.UseRouting();
app.UseAuthentication();
app.UseAuthorization();

app.MapStaticAssets();
app.MapRazorPages()
   .WithStaticAssets();

app.Run();
