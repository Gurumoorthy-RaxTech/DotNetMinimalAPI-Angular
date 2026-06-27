// ============================================================
// MINIMAL API WITH JWT + VERSIONING - COMPLETE EXPLANATION
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

// STEP 1: WebApplication.CreateBuilder
// Builder create pannuvom - dependency injection, configuration load aagum
var builder = WebApplication.CreateBuilder(args);

// ============================================================
// STEP 2: SERVICES REGISTER PANNUVOM (Dependency Injection)
// ============================================================

// JWT Service register - interface -> implementation mapping
builder.Services.AddScoped<IJwtService, JwtService>();

// Student Service register
builder.Services.AddScoped<IStudentService, StudentService>();

// ============================================================
// SIGNALR CONFIGURE PANNUVOM
// Tanglish: Real-time bi-directional communication enable pannuvom
// WebSocket use pannuvom - HTTP unlike oru connection open aa irukum
// ============================================================
builder.Services.AddSignalR(options =>
{
    options.EnableDetailedErrors = true;         // Development - detailed errors show
    options.KeepAliveInterval = TimeSpan.FromSeconds(15); // Connection alive keep pannuvom
    options.ClientTimeoutInterval = TimeSpan.FromSeconds(30);
});

// Background Service - every 2 seconds stats broadcast pannuvom
builder.Services.AddHostedService<DashboardBackgroundService>();
// Singleton aa register - other services inject pannalam
builder.Services.AddSingleton<DashboardBackgroundService>();

// ============================================================
// STEP 3: JWT AUTHENTICATION CONFIGURE PANNUVOM
// ============================================================
var jwtSettings = builder.Configuration.GetSection("Jwt");
var secretKey = jwtSettings["SecretKey"]!;

builder.Services.AddAuthentication(options =>
{
    // Default scheme - JWT Bearer use pannuvom
    options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
    options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
})
.AddJwtBearer(options =>
{
    options.TokenValidationParameters = new TokenValidationParameters
    {
        // Issuer validate pannuvom - token யாரு create pannaru
        ValidateIssuer = true,
        ValidIssuer = jwtSettings["Issuer"],

        // Audience validate pannuvom - token யாருக்காக
        ValidateAudience = true,
        ValidAudience = jwtSettings["Audience"],

        // Token expire aagiruchaa check pannuvom
        ValidateLifetime = true,

        // Secret key validate pannuvom
        ValidateIssuerSigningKey = true,
        IssuerSigningKey = new SymmetricSecurityKey(
            Encoding.UTF8.GetBytes(secretKey))
    };
});

// ============================================================
// STEP 4: AUTHORIZATION POLICIES CONFIGURE PANNUVOM
// ============================================================
builder.Services.AddAuthorization(options =>
{
    // AdminOnly policy - only Admin role access panlam
    options.AddPolicy("AdminOnly", policy =>
        policy.RequireRole("Admin"));

    // StudentOrAdmin policy - both roles access panlam
    options.AddPolicy("StudentOrAdmin", policy =>
        policy.RequireRole("Student", "Admin"));
});

// ============================================================
// STEP 5: API VERSIONING CONFIGURE PANNUVOM
// ============================================================
builder.Services.AddApiVersioning(options =>
{
    // Default version - client specify pannavidaal v1 use aagum
    options.DefaultApiVersion = new ApiVersion(1, 0);
    options.AssumeDefaultVersionWhenUnspecified = true;

    // Version read pannura ways:
    // URL path: /api/v1/students
    // Header: Api-Version: 1.0
    // Query: ?api-version=1.0
    options.ReportApiVersions = true; // Response header la version info show aagum
    options.ApiVersionReader = ApiVersionReader.Combine(
        new UrlSegmentApiVersionReader(),   // /api/v1/ from URL
        new HeaderApiVersionReader("Api-Version"),  // from Header
        new QueryStringApiVersionReader("api-version")  // from Query string
    );
})
.AddApiExplorer(options =>
{
    options.GroupNameFormat = "'v'VVV";
    options.SubstituteApiVersionInUrl = true;
});

// ============================================================
// STEP 6: SWAGGER CONFIGURE PANNUVOM
// ============================================================
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(options =>
{
    // V1 Swagger doc
    options.SwaggerDoc("v1", new OpenApiInfo
    {
        Title = "MinimalAPI JWT - SmartSchool API",
        Version = "v1",
        Description = "JWT Authentication + API Versioning Demo | By Gurumoorthy M"
    });

    // V2 Swagger doc
    options.SwaggerDoc("v2", new OpenApiInfo
    {
        Title = "MinimalAPI JWT - SmartSchool API",
        Version = "v2",
        Description = "V2 - With Course Filter Feature"
    });

    // Swagger la JWT token enter pannuvom - lock icon show aagum
    options.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme
    {
        Name = "Authorization",
        Type = SecuritySchemeType.Http,
        Scheme = "bearer",
        BearerFormat = "JWT",
        In = ParameterLocation.Header,
        Description = "Enter JWT token: Bearer {your_token}"
    });

    options.AddSecurityRequirement(new OpenApiSecurityRequirement
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

// ============================================================
// STEP 7: CORS CONFIGURE PANNUVOM (Angular access ku)
// ============================================================
builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowAngular", policy =>
    {
        policy.WithOrigins(
                "http://localhost:4200",  // Angular dev server
                "https://localhost:4200"
              )
              .AllowAnyHeader()
              .AllowAnyMethod()
              .AllowCredentials(); // SignalR ku AllowCredentials mandatory!
    });
});

// ============================================================
// STEP 8: APP BUILD PANNUVOM
// ============================================================
var app = builder.Build();

// ============================================================
// STEP 9: MIDDLEWARE PIPELINE CONFIGURE PANNUVOM
// Order matters! - Middleware order correct aa irukkanum
// ============================================================

// Development la Swagger show pannuvom
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI(options =>
    {
        // Both versions show aagum
        options.SwaggerEndpoint("/swagger/v1/swagger.json", "API V1");
        options.SwaggerEndpoint("/swagger/v2/swagger.json", "API V2");
        options.RoutePrefix = "swagger";
    });
}

// HTTPS redirect
app.UseHttpsRedirection();

// CORS - Angular request allow pannuvom (Authentication before CORS important!)
app.UseCors("AllowAngular");

// Authentication - "யாரு nee?" check pannuvom (JWT token validate)
app.UseAuthentication();

// Authorization - "permission irukka?" check pannuvom
app.UseAuthorization();

// ============================================================
// STEP 10: ENDPOINTS MAP PANNUVOM
// ============================================================
app.MapAuthEndpoints();     // /api/v1/auth/login
app.MapStudentEndpoints();  // /api/v1/students, /api/v2/students

// ============================================================
// SIGNALR HUB MAP PANNUVOM
// Tanglish: /hubs/dashboard URL la SignalR connection accept pannuvom
// Angular la: new HubConnectionBuilder().withUrl("/hubs/dashboard")
// ============================================================
app.MapHub<DashboardHub>("/hubs/dashboard");

// Health check endpoint
app.MapGet("/health", () => Results.Ok(new
{
    Status = "Healthy",
    Timestamp = DateTime.UtcNow,
    Version = "1.0.0",
    Service = "MinimalAPI JWT - SmartSchool + SignalR"
})).AllowAnonymous().WithTags("Health");

// ============================================================
// STEP 11: APP RUN PANNUVOM
// ============================================================
app.Run();
