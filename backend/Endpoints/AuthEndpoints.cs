using MinimalApiJwt.Models;
using MinimalApiJwt.Services;

namespace MinimalApiJwt.Endpoints;

public static class AuthEndpoints
{
    // Extension method pattern - Program.cs la clean aa register pannalam
    public static void MapAuthEndpoints(this WebApplication app)
    {
        // Versioned route group - /api/v1/auth
        var v1 = app.NewVersionedApi("Auth");
        var authGroup = v1.MapGroup("/api/v{version:apiVersion}/auth")
                          .HasApiVersion(1.0)
                          .WithTags("Auth");

        // POST /api/v1/auth/login - JWT token generate pannuvom
        authGroup.MapPost("/login", HandleLogin)
            .WithName("Login")
            .WithSummary("Login and get JWT token")
            .AllowAnonymous();  // Authentication vendam - anyone login panlam
    }

    // Handler method - separate pannuvom for clean code
    private static IResult HandleLogin(LoginRequest request, IJwtService jwtService)
    {
        // Hardcoded users - real project la DB check pannuvom
        var users = new Dictionary<string, (string password, string role)>
        {
            { "admin", ("Admin@123", "Admin") },
            { "student", ("Student@123", "Student") },
            { "guru", ("Guru@123", "Admin") }
        };

        // Username check pannuvom
        if (!users.TryGetValue(request.Username.ToLower(), out var userInfo))
        {
            return Results.Unauthorized();
        }

        // Password check pannuvom
        if (userInfo.password != request.Password)
        {
            return Results.Unauthorized();
        }

        // JWT Token generate pannuvom
        var response = jwtService.GenerateToken(request.Username, userInfo.role);

        return Results.Ok(ApiResponse<LoginResponse>.Ok(response, "Login successful"));
    }
}
