// ACT.API/Program.cs
using ACT.API.Middleware;
using ACT.API.Services;
using ACT.Application.Services.Interfaces;
using ACT.Domain.Interfaces;
using ACT.Infrastructure.Persistence;
using ACT.Infrastructure.Repositories;
using ACT.Infrastructure.Services;
using ACT.Application.Services;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.RateLimiting;
using Microsoft.IdentityModel.Tokens;
using System.Text;
using System.IdentityModel.Tokens.Jwt;
using System.Threading.RateLimiting;
using Microsoft.OpenApi.Models;


var builder = WebApplication.CreateBuilder(args);

// ── Global exception handling ─────────────────────────────────────────────────
builder.Services.AddExceptionHandler<GlobalExceptionHandler>();
builder.Services.AddProblemDetails();

// ── Tenant context — resolves the caller's company scope from the JWT for EF Core's
// global query filters (defense-in-depth tenant isolation, see AppDbContext) ───────
builder.Services.AddHttpContextAccessor();
builder.Services.AddScoped<ITenantContext, HttpContextTenantContext>();

// ── Database ──────────────────────────────────────────────────────────────────
builder.Services.AddDbContext<AppDbContext>(options =>
    options.UseSqlite(builder.Configuration.GetConnectionString("Default")));

// ── Repositories ──────────────────────────────────────────────────────────────
builder.Services.AddScoped<IClientRepository, ClientRepository>();
builder.Services.AddScoped<ITreatmentRepository, TreatmentRepository>();
builder.Services.AddScoped<ITreatmentTypeRepository, TreatmentTypeRepository>();
builder.Services.AddScoped<IBrandSettingsRepository, BrandSettingsRepository>();
builder.Services.AddScoped<ICompanyRepository, CompanyRepository>();
builder.Services.AddScoped<IUserRepository, UserRepository>();
builder.Services.AddScoped<IAuditLogRepository, AuditLogRepository>();
builder.Services.AddScoped<ILoginHistoryRepository, LoginHistoryRepository>();

// ── Background service ────────────────────────────────────────────────────────
// Singleton lifetime is required for IHostedService
// Uses IServiceScopeFactory internally to create scoped DbContext per run
builder.Services.AddHostedService<FollowUpNotificationWorker>();

// ── API + docs ────────────────────────────────────────────────────────────────
builder.Services.AddControllers()
    .AddJsonOptions(options =>
    {
        options.JsonSerializerOptions.Converters.Add(
            new System.Text.Json.Serialization.JsonStringEnumConverter());
    });
builder.Services.AddSwaggerGen(c =>
{
    c.SwaggerDoc("v1", new() { Title = "ACT — Aesthetic Client Tracker", Version = "v1" });
    c.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme
    {
        Name = "Authorization",
        Type = SecuritySchemeType.Http,
        Scheme = "bearer",
        BearerFormat = "JWT",
        In = ParameterLocation.Header,
        Description = "Enter your JWT token"
    });
    c.AddSecurityRequirement(new OpenApiSecurityRequirement
    {
        {
            new OpenApiSecurityScheme
            {
                Reference = new OpenApiReference
                {
                    Type = ReferenceType.SecurityScheme,
                    Id = "Bearer"
                }
            },
            Array.Empty<string>()
        }
    });
});

// ── Services ──────────────────────────────────────────────────────────────────
builder.Services.AddScoped<IClientService, ClientService>();
builder.Services.AddScoped<ITreatmentService, TreatmentService>();
builder.Services.AddScoped<ITreatmentTypeService, TreatmentTypeService>();
builder.Services.AddScoped<IBrandSettingsService, BrandSettingsService>();
builder.Services.AddScoped<ICompanyService, CompanyService>();
builder.Services.AddScoped<IJwtService, JwtService>();
builder.Services.AddScoped<IPasswordHasher, PasswordHasher>();
builder.Services.AddScoped<IAuthService, AuthService>();
builder.Services.AddScoped<IUserService, UserService>();
builder.Services.AddScoped<IAuditService, AuditService>();

// ── Authentication ────────────────────────────────────────────────────────────
JwtSecurityTokenHandler.DefaultInboundClaimTypeMap.Clear();
// Set via `dotnet user-secrets set "JwtSettings:Secret" "..."` locally, or the
// JwtSettings__Secret environment variable in every other environment — never in appsettings*.json.
var jwtSecret = builder.Configuration["JwtSettings:Secret"];
if (string.IsNullOrWhiteSpace(jwtSecret))
{
    throw new InvalidOperationException(
        "JwtSettings:Secret is not configured. Set it via 'dotnet user-secrets set \"JwtSettings:Secret\" \"<value>\"' " +
        "(local dev) or the JwtSettings__Secret environment variable (all other environments).");
}
builder.Services.AddAuthentication(options =>
{
    options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
    options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
})
.AddJwtBearer(options =>
{
    options.MapInboundClaims = false;
    options.TokenValidationParameters = new TokenValidationParameters
    {
        ValidateIssuer = true,
        ValidateAudience = true,
        ValidateLifetime = true,
        ValidateIssuerSigningKey = true,
        ValidIssuer = builder.Configuration["JwtSettings:Issuer"],
        ValidAudience = builder.Configuration["JwtSettings:Audience"],
        IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwtSecret)),
        RoleClaimType = "role",
        NameClaimType = "email"
    };
});
builder.Services.AddAuthorization();
// ── CORS — origins come from config so prod can lock this down without a code change ─────
var allowedOrigins = builder.Configuration.GetSection("Cors:AllowedOrigins").Get<string[]>()
    ?? ["http://localhost:5173", "https://localhost:5173"]; // Vite dev server default
builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowPwa", policy =>
    {
        policy
            .WithOrigins(allowedOrigins)
            .AllowAnyHeader()
            .AllowAnyMethod();
    });
});

// ── Rate limiting — throttle brute-force login attempts per client IP ─────────
builder.Services.AddRateLimiter(options =>
{
    options.RejectionStatusCode = StatusCodes.Status429TooManyRequests;
    options.OnRejected = async (context, cancellationToken) =>
    {
        context.HttpContext.Response.Headers.RetryAfter = "60";
        await context.HttpContext.Response.WriteAsJsonAsync(
            new { message = "Too many login attempts. Please try again in a minute." },
            cancellationToken);
    };

    options.AddPolicy("login", httpContext =>
        RateLimitPartition.GetFixedWindowLimiter(
            partitionKey: httpContext.Connection.RemoteIpAddress?.ToString() ?? "unknown",
            factory: _ => new FixedWindowRateLimiterOptions
            {
                PermitLimit = 5,
                Window = TimeSpan.FromMinutes(1),
                QueueLimit = 0
            }));
});

var app = builder.Build();

// ── Auto-migrate on startup ───────────────────────────────────────────────────
// Runs any pending migrations automatically when the app starts.
// Safe for SQLite — no downtime risk. Remove this for production SQL Server.
using (var scope = app.Services.CreateScope())
{
    var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();
    db.Database.Migrate();

    var hasher = scope.ServiceProvider.GetRequiredService<IPasswordHasher>();
    var seedLogger = scope.ServiceProvider.GetRequiredService<ILoggerFactory>().CreateLogger("AdminSeeder");
    await AdminSeeder.SeedSuperAdminAsync(db, hasher, seedLogger);
}

// ── Middleware pipeline ───────────────────────────────────────────────────────
app.UseExceptionHandler();

// ── Security headers — applied to every response, ahead of any terminal middleware ────
app.Use(async (context, next) =>
{
    context.Response.Headers["X-Content-Type-Options"] = "nosniff";
    context.Response.Headers["X-Frame-Options"] = "DENY";
    context.Response.Headers["Referrer-Policy"] = "no-referrer";
    await next();
});

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
    if (!app.Environment.IsDevelopment())
    {
        app.UseHsts();
    }
}

app.UseRateLimiter();
app.UseAuthentication();
app.UseAuthorization();
app.MapControllers();

app.Run();