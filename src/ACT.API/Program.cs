// ACT.API/Program.cs
using ACT.Domain.Interfaces;
using ACT.Infrastructure.Persistence;
using ACT.Infrastructure.Repositories;
using ACT.Infrastructure.Services;
using ACT.Application.Services;
using Microsoft.EntityFrameworkCore;

using Scalar.AspNetCore;

var builder = WebApplication.CreateBuilder(args);

// ── Database ──────────────────────────────────────────────────────────────────
builder.Services.AddDbContext<AppDbContext>(options =>
    options.UseSqlite(builder.Configuration.GetConnectionString("Default")));

// ── Repositories ──────────────────────────────────────────────────────────────
builder.Services.AddScoped<IClientRepository, ClientRepository>();
builder.Services.AddScoped<ITreatmentRepository, TreatmentRepository>();
builder.Services.AddScoped<ITreatmentTypeRepository, TreatmentTypeRepository>();

// ── Background service ────────────────────────────────────────────────────────
// Singleton lifetime is required for IHostedService
// Uses IServiceScopeFactory internally to create scoped DbContext per run
builder.Services.AddHostedService<FollowUpNotificationWorker>();

// ── API + docs ────────────────────────────────────────────────────────────────
builder.Services.AddControllers();
builder.Services.AddOpenApi(); // Scalar uses the built-in OpenAPI doc
builder.Services.AddHostedService<FollowUpNotificationWorker>();

builder.Services.AddScoped<TreatmentService>();
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
    app.MapOpenApi();
    app.MapScalarApiReference(options =>
    {
        options.Title = "ACT — Aesthetic Client Tracker";
        options.Theme = ScalarTheme.Purple; // fits the clinic branding
    });
}

app.UseCors("AllowPwa");
app.UseHttpsRedirection();
app.UseAuthorization();
app.MapControllers();

app.Run();