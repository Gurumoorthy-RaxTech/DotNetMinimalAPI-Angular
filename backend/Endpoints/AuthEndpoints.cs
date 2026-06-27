using System.Security.Claims;
using MinimalApiJwt.Models;
using MinimalApiJwt.Services;

namespace MinimalApiJwt.Endpoints;

public static class AuthEndpoints
{
    // Hardcoded users - real project la DB + password hash use pannuvom
    private static readonly Dictionary<string, (string password, string role)> Users = new()
    {
        { "admin",   ("Admin@123",   "Admin") },
        { "student", ("Student@123", "Student") },
        { "guru",    ("Guru@123",    "Admin") }
    };

    public static void MapAuthEndpoints(this WebApplication app)
    {
        var v1 = app.NewVersionedApi("Auth");
        var authGroup = v1.MapGroup("/api/v{version:apiVersion}/auth")
                          .HasApiVersion(1.0)
                          .WithTags("Auth");

        // ── POST /api/v1/auth/login ──────────────────────────────
        // Login → Access Token (15 min) + Refresh Token (7 days) return pannuvom
        authGroup.MapPost("/login", HandleLogin)
            .WithName("Login")
            .WithSummary("Login → returns Access Token (15min) + Refresh Token (7days)")
            .AllowAnonymous();

        // ── POST /api/v1/auth/refresh ────────────────────────────
        // Access Token expire aana → Refresh Token use panni new tokens get pannuvom
        authGroup.MapPost("/refresh", HandleRefresh)
            .WithName("RefreshToken")
            .WithSummary("Access token expired? Use refresh token to get new tokens")
            .AllowAnonymous();

        // ── POST /api/v1/auth/revoke ─────────────────────────────
        // Logout → Refresh Token blacklist pannuvom
        authGroup.MapPost("/revoke", HandleRevoke)
            .WithName("RevokeToken")
            .WithSummary("Logout - revoke refresh token")
            .RequireAuthorization();

        // ── GET /api/v1/auth/me ───────────────────────────────────
        // Current logged-in user info return pannuvom
        authGroup.MapGet("/me", HandleMe)
            .WithName("GetMe")
            .WithSummary("Get current logged-in user info from JWT claims")
            .RequireAuthorization();
    }

    // ── LOGIN HANDLER ────────────────────────────────────────────
    private static IResult HandleLogin(LoginRequest request, IJwtService jwtService)
    {
        if (!Users.TryGetValue(request.Username.ToLower(), out var userInfo))
            return Results.Unauthorized();

        if (userInfo.password != request.Password)
            return Results.Unauthorized();

        // Both access + refresh token generate pannuvom
        var response = jwtService.GenerateTokens(request.Username, userInfo.role);

        return Results.Ok(ApiResponse<LoginResponse>.Ok(response,
            "Login successful. Access token: 15min | Refresh token: 7 days"));
    }

    // ── REFRESH HANDLER ──────────────────────────────────────────
    // Interview Q: "How do you handle token expiry without forcing re-login?"
    // A: Refresh token use pannuvom — Angular interceptor auto call pannuvom
    private static IResult HandleRefresh(RefreshTokenRequest request, IJwtService jwtService)
    {
        if (string.IsNullOrEmpty(request.AccessToken) || string.IsNullOrEmpty(request.RefreshToken))
            return Results.BadRequest(ApiResponse<object>.Fail("Access token and refresh token required"));

        // New tokens generate pannuvom (rotation happens inside)
        var newTokens = jwtService.RefreshTokens(request.AccessToken, request.RefreshToken);

        if (newTokens == null)
            return Results.Unauthorized(); // Refresh token invalid/expired/revoked

        return Results.Ok(ApiResponse<LoginResponse>.Ok(newTokens,
            "Tokens refreshed successfully (old refresh token revoked)"));
    }

    // ── REVOKE HANDLER (LOGOUT) ──────────────────────────────────
    private static IResult HandleRevoke(HttpContext context, IJwtService jwtService)
    {
        // Request body la refresh token expect pannuvom
        // Simple implementation: header la irunthu read pannuvom
        var refreshToken = context.Request.Headers["X-Refresh-Token"].ToString();

        if (string.IsNullOrEmpty(refreshToken))
            return Results.BadRequest(ApiResponse<object>.Fail("Refresh token required in X-Refresh-Token header"));

        var revoked = jwtService.RevokeRefreshToken(refreshToken);

        if (!revoked)
            return Results.NotFound(ApiResponse<object>.Fail("Refresh token not found"));

        return Results.Ok(ApiResponse<object>.Ok(new { }, "Logout successful — refresh token revoked"));
    }

    // ── ME HANDLER ───────────────────────────────────────────────
    // JWT claims la irunthu user info read pannuvom — DB call vendam!
    private static IResult HandleMe(HttpContext context)
    {
        var user = context.User;
        return Results.Ok(ApiResponse<object>.Ok(new
        {
            Username = user.Identity?.Name,
            Role     = user.FindFirst(ClaimTypes.Role)?.Value,
            IssuedAt = user.FindFirst("iat")?.Value
        }));
    }
}
