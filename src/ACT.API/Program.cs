// ACT.API/Program.cs
using ACT.Application.Services.Interfaces;
using ACT.Domain.Interfaces;
using ACT.Infrastructure.Persistence;
using ACT.Infrastructure.Repositories;
using ACT.Infrastructure.Services;
using ACT.Application.Services;
using Microsoft.EntityFrameworkCore;


var builder = WebApplication.CreateBuilder(args);

// ── Database ──────────────────────────────────────────────────────────────────
builder.Services.AddDbContext<AppDbContext>(options =>
    options.UseSqlite(builder.Configuration.GetConnectionString("Default")));

// ── Repositories ──────────────────────────────────────────────────────────────
builder.Services.AddScoped<IClientRepository, ClientRepository>();
builder.Services.AddScoped<ITreatmentRepository, TreatmentRepository>();
builder.Services.AddScoped<ITreatmentTypeRepository, TreatmentTypeRepository>();
builder.Services.AddScoped<IBrandSettingsRepository, BrandSettingsRepository>();
builder.Services.AddScoped<ICompanyRepository, CompanyRepository>();

// ── Background service ────────────────────────────────────────────────────────
// Singleton lifetime is required for IHostedService
// Uses IServiceScopeFactory internally to create scoped DbContext per run
builder.Services.AddHostedService<FollowUpNotificationWorker>();

// ── API + docs ────────────────────────────────────────────────────────────────
builder.Services.AddControllers();
builder.Services.AddSwaggerGen(c =>
{
    c.SwaggerDoc("v1", new() { Title = "ACT — Aesthetic Client Tracker", Version = "v1" });
});

// ── Services ──────────────────────────────────────────────────────────────────
builder.Services.AddScoped<IClientService, ClientService>();
builder.Services.AddScoped<ITreatmentService, TreatmentService>();
builder.Services.AddScoped<ITreatmentTypeService, TreatmentTypeService>();
builder.Services.AddScoped<IBrandSettingsService, BrandSettingsService>();
builder.Services.AddScoped<ICompanyService, CompanyService>();
// ── CORS — allow React PWA running on a different port during development ─────
builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowPwa", policy =>
    {
        policy
            .WithOrigins(
                "http://localhost:5173",  // Vite dev server default
                "https://localhost:5173"
            )
            .AllowAnyHeader()
            .AllowAnyMethod();
    });
});

var app = builder.Build();

// ── Auto-migrate on startup ───────────────────────────────────────────────────
// Runs any pending migrations automatically when the app starts.
// Safe for SQLite — no downtime risk. Remove this for production SQL Server.
using (var scope = app.Services.CreateScope())
{
    var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();
    db.Database.Migrate();
}

// ── Middleware pipeline ───────────────────────────────────────────────────────
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI(c =>
    {
        c.SwaggerEndpoint("/swagger/v1/swagger.json", "ACT — Aesthetic Client Tracker v1");
    });
}

app.UseCors("AllowPwa");

// Only use HTTPS redirection if HTTPS is enabled in the URLs
var hasHttps = app.Urls.Any(url => url.StartsWith("https://", StringComparison.OrdinalIgnoreCase));
if (hasHttps)
{
    app.UseHttpsRedirection();
}

app.UseAuthorization();
app.MapControllers();

app.Run();