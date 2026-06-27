using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Security.Cryptography;
using System.Text;
using Microsoft.IdentityModel.Tokens;
using MinimalApiJwt.Models;

namespace MinimalApiJwt.Services;

public interface IJwtService
{
    LoginResponse GenerateTokens(string username, string role);
    LoginResponse? RefreshTokens(string accessToken, string refreshToken);
    bool RevokeRefreshToken(string refreshToken);
    ClaimsPrincipal? GetPrincipalFromExpiredToken(string token);
}

// ============================================================
// JWT SERVICE - Access Token + Refresh Token
//
// FLOW (Interview la explain pannuvom):
// 1. Login → Access Token (15 min) + Refresh Token (7 days)
// 2. API call → Access Token header la attach pannuvom
// 3. Access Token expire → Refresh Token use panni new tokens get pannuvom
// 4. Refresh Token expire → Re-login mandatory
// 5. Logout → Refresh Token revoke pannuvom
//
// SECURITY:
// - Refresh Token Rotation: Har refresh call la new refresh token issue pannuvom
//   (old one invalidate aagum) — token theft detect pannalam
// - Access Token short-lived (15 min) — compromise aana also damage limited
// ============================================================
public class JwtService : IJwtService
{
    private readonly IConfiguration _config;

    // In-memory refresh token store — real project: SQL/Redis use pannuvom
    // ConcurrentDictionary — thread-safe (multiple requests same time handle)
    private static readonly System.Collections.Concurrent.ConcurrentDictionary<string, RefreshTokenRecord>
        _refreshTokens = new();

    public JwtService(IConfiguration config)
    {
        _config = config;
    }

    // ============================================================
    // GENERATE BOTH TOKENS - Login success aana call pannuvom
    // ============================================================
    public LoginResponse GenerateTokens(string username, string role)
    {
        var accessToken = GenerateAccessToken(username, role);
        var refreshToken = GenerateRefreshToken();

        var refreshExpiryDays = int.Parse(_config["Jwt:RefreshTokenExpiryDays"]!);
        var refreshExpiry = DateTime.UtcNow.AddDays(refreshExpiryDays);

        // Refresh token store pannuvom
        _refreshTokens[refreshToken] = new RefreshTokenRecord
        {
            Token = refreshToken,
            Username = username,
            Role = role,
            ExpiresAt = refreshExpiry,
            IsRevoked = false
        };

        var expiryMinutes = int.Parse(_config["Jwt:ExpiryMinutes"]!);

        return new LoginResponse
        {
            Token = accessToken,
            RefreshToken = refreshToken,
            Username = username,
            Role = role,
            ExpiresAt = DateTime.UtcNow.AddMinutes(expiryMinutes),
            RefreshTokenExpiresAt = refreshExpiry
        };
    }

    // ============================================================
    // REFRESH TOKENS - Access token expire aana call pannuvom
    // ============================================================
    public LoginResponse? RefreshTokens(string accessToken, string refreshToken)
    {
        // Step 1: Expired access token la irunthu claims extract pannuvom
        var principal = GetPrincipalFromExpiredToken(accessToken);
        if (principal == null) return null;

        var username = principal.Identity?.Name;
        var role = principal.FindFirst(ClaimTypes.Role)?.Value;
        if (username == null || role == null) return null;

        // Step 2: Refresh token validate pannuvom
        if (!_refreshTokens.TryGetValue(refreshToken, out var storedToken))
            return null; // Token illai

        if (storedToken.IsRevoked)
            return null; // Already revoked

        if (storedToken.Username != username)
            return null; // Token username mismatch — possible theft!

        if (storedToken.ExpiresAt < DateTime.UtcNow)
            return null; // Refresh token also expired — re-login mandatory

        // Step 3: REFRESH TOKEN ROTATION
        // Old token revoke pannuvom, new token generate pannuvom
        // Idhu security feature — stolen token reuse pannala
        storedToken.IsRevoked = true;
        storedToken.ReplacedByToken = "new-token-issued"; // tracking ku

        // Step 4: New tokens generate pannuvom
        var newResponse = GenerateTokens(username, role);
        return newResponse;
    }

    // ============================================================
    // REVOKE - Logout aana refresh token invalid pannuvom
    // ============================================================
    public bool RevokeRefreshToken(string refreshToken)
    {
        if (!_refreshTokens.TryGetValue(refreshToken, out var token))
            return false;

        token.IsRevoked = true;
        return true;
    }

    // ============================================================
    // EXTRACT CLAIMS FROM EXPIRED TOKEN
    // ValidateLifetime = false — expired token la irunthu also claims padikuvom
    // ============================================================
    public ClaimsPrincipal? GetPrincipalFromExpiredToken(string token)
    {
        try
        {
            var key = new SymmetricSecurityKey(
                Encoding.UTF8.GetBytes(_config["Jwt:SecretKey"]!));

            var tokenValidationParams = new TokenValidationParameters
            {
                ValidateIssuerSigningKey = true,
                IssuerSigningKey = key,
                ValidateIssuer = true,
                ValidIssuer = _config["Jwt:Issuer"],
                ValidateAudience = true,
                ValidAudience = _config["Jwt:Audience"],
                ValidateLifetime = false  // KEY: Expired token also accept pannuvom!
            };

            var handler = new JwtSecurityTokenHandler();
            var principal = handler.ValidateToken(token, tokenValidationParams, out var validatedToken);

            // Algorithm validate pannuvom
            if (validatedToken is not JwtSecurityToken jwtToken ||
                !jwtToken.Header.Alg.Equals(SecurityAlgorithms.HmacSha256,
                    StringComparison.InvariantCultureIgnoreCase))
                return null;

            return principal;
        }
        catch
        {
            return null;
        }
    }

    // ============================================================
    // PRIVATE HELPERS
    // ============================================================

    private string GenerateAccessToken(string username, string role)
    {
        var secretKey = _config["Jwt:SecretKey"]!;
        var issuer = _config["Jwt:Issuer"]!;
        var audience = _config["Jwt:Audience"]!;
        var expiryMinutes = int.Parse(_config["Jwt:ExpiryMinutes"]!);

        var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(secretKey));
        var credentials = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);

        var claims = new[]
        {
            new Claim(ClaimTypes.Name, username),
            new Claim(ClaimTypes.Role, role),
            new Claim(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString()),
            new Claim(JwtRegisteredClaimNames.Iat,
                DateTimeOffset.UtcNow.ToUnixTimeSeconds().ToString())
        };

        var token = new JwtSecurityToken(
            issuer: issuer,
            audience: audience,
            claims: claims,
            expires: DateTime.UtcNow.AddMinutes(expiryMinutes),
            signingCredentials: credentials
        );

        return new JwtSecurityTokenHandler().WriteToken(token);
    }

    // Cryptographically secure random token generate pannuvom
    // JWT unlike ithil signature illai — DB validate panum
    private static string GenerateRefreshToken()
    {
        var randomBytes = new byte[64];
        using var rng = RandomNumberGenerator.Create();
        rng.GetBytes(randomBytes);
        return Convert.ToBase64String(randomBytes);
    }
}
