// ============================================================
// MINIMAL API WITH JWT + VERSIONING + SIGNALR
// Author: Gurumoorthy M | Interview Preparation Project
// ============================================================

using System.Text;
using Asp.Versioning;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi.Models;
using MinimalApiJwt.Endpoints;
using MinimalApiJwt.Hubs;
using MinimalApiJwt.Services;

var builder = WebApplication.CreateBuilder(args);

// ── SERVICES ─────────────────────────────────────────────────
builder.Services.AddScoped<IJwtService, JwtService>();
builder.Services.AddScoped<IStudentService, StudentService>();

// ── SIGNALR ──────────────────────────────────────────────────
builder.Services.AddSignalR(options =>
{
    options.EnableDetailedErrors = true;
    options.KeepAliveInterval = TimeSpan.FromSeconds(15);
    options.ClientTimeoutInterval = TimeSpan.FromSeconds(30);
});

// BUG FIX 1: AddHostedService + AddSingleton = 2 instances!
// Correct way: Singleton register → same instance as HostedService use pannuvom
builder.Services.AddSingleton<DashboardBackgroundService>();
builder.Services.AddHostedService(sp => sp.GetRequiredService<DashboardBackgroundService>());

// ── JWT AUTHENTICATION ────────────────────────────────────────
var jwtSettings = builder.Configuration.GetSection("Jwt");
var secretKey   = jwtSettings["SecretKey"]!;

builder.Services.AddAuthentication(options =>
{
    options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
    options.DefaultChallengeScheme    = JwtBearerDefaults.AuthenticationScheme;
})
.AddJwtBearer(options =>
{
    options.TokenValidationParameters = new TokenValidationParameters
    {
        ValidateIssuer           = true,
        ValidIssuer              = jwtSettings["Issuer"],
        ValidateAudience         = true,
        ValidAudience            = jwtSettings["Audience"],
        ValidateLifetime         = true,
        ValidateIssuerSigningKey = true,
        IssuerSigningKey         = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(secretKey))
    };

    // BUG FIX 2: SignalR JWT — WebSocket header Bearer token send panna mudiyathu!
    // Token query string la varum: ?access_token=xxx
    // Idha read panni validate pannuvom — otherwise hub always 401 return pannuvom
    options.Events = new JwtBearerEvents
    {
        OnMessageReceived = context =>
        {
            // SignalR hub path check pannuvom
            var path = context.HttpContext.Request.Path;
            if (path.StartsWithSegments("/hubs"))
            {
                // Query string la token irukka padikuvom
                var token = context.Request.Query["access_token"];
                if (!string.IsNullOrEmpty(token))
                    context.Token = token; // JWT middleware ku set pannuvom
            }
            return Task.CompletedTask;
        }
    };
});

// ── AUTHORIZATION POLICIES ───────────────────────────────────
builder.Services.AddAuthorization(options =>
{
    options.AddPolicy("AdminOnly",      policy => policy.RequireRole("Admin"));
    options.AddPolicy("StudentOrAdmin", policy => policy.RequireRole("Student", "Admin"));
});

// ── API VERSIONING ───────────────────────────────────────────
builder.Services.AddApiVersioning(options =>
{
    options.DefaultApiVersion                   = new ApiVersion(1, 0);
    options.AssumeDefaultVersionWhenUnspecified = true;
    options.ReportApiVersions                   = true;
    options.ApiVersionReader = ApiVersionReader.Combine(
        new UrlSegmentApiVersionReader(),
        new HeaderApiVersionReader("Api-Version"),
        new QueryStringApiVersionReader("api-version")
    );
})
.AddApiExplorer(options =>
{
    options.GroupNameFormat        = "'v'VVV";
    options.SubstituteApiVersionInUrl = true;
});

// ── SWAGGER ───────────────────────────────────────────────────
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(options =>
{
    options.SwaggerDoc("v1", new OpenApiInfo
    {
        Title       = "MinimalAPI JWT - SmartSchool API",
        Version     = "v1",
        Description = "JWT + Versioning + SignalR | By Gurumoorthy M"
    });
    options.SwaggerDoc("v2", new OpenApiInfo
    {
        Title   = "MinimalAPI JWT - SmartSchool API",
        Version = "v2",
        Description = "V2 - With Course Filter Feature"
    });

    options.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme
    {
        Name        = "Authorization",
        Type        = SecuritySchemeType.Http,
        Scheme      = "bearer",
        BearerFormat = "JWT",
        In          = ParameterLocation.Header,
        Description = "Enter JWT token"
    });
    options.AddSecurityRequirement(new OpenApiSecurityRequirement
    {
        {
            new OpenApiSecurityScheme
            {
                Reference = new OpenApiReference { Type = ReferenceType.SecurityScheme, Id = "Bearer" }
            },
            Array.Empty<string>()
        }
    });
});

// ── CORS ─────────────────────────────────────────────────────
// AllowCredentials() — SignalR ku mandatory!
builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowAngular", policy =>
        policy.WithOrigins(
                  "http://localhost:4200", "https://localhost:4200",  // Angular
                  "http://localhost:5173", "https://localhost:5173"   // React (Vite)
              )
              .AllowAnyHeader()
              .AllowAnyMethod()
              .AllowCredentials());
});

// ── BUILD ────────────────────────────────────────────────────
var app = builder.Build();

// ── MIDDLEWARE PIPELINE ───────────────────────────────────────
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI(options =>
    {
        options.SwaggerEndpoint("/swagger/v1/swagger.json", "API V1");
        options.SwaggerEndpoint("/swagger/v2/swagger.json", "API V2");
        options.RoutePrefix = "swagger";
    });
}

// NOTE: Remove UseHttpsRedirection — http://localhost:5260 use pannrom
// app.UseHttpsRedirection();

app.UseCors("AllowAngular");       // CORS first — SignalR preflight handle pannuvom
app.UseAuthentication();           // JWT validate pannuvom
app.UseAuthorization();            // Role/policy check pannuvom

// ── ENDPOINTS ────────────────────────────────────────────────
app.MapAuthEndpoints();
app.MapStudentEndpoints();
app.MapHub<DashboardHub>("/hubs/dashboard");  // SignalR hub

app.MapGet("/health", () => Results.Ok(new
{
    Status    = "Healthy",
    Timestamp = DateTime.UtcNow,
    Service   = "MinimalAPI + SignalR"
})).AllowAnonymous().WithTags("Health");

app.Run();
