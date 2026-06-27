using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using Microsoft.IdentityModel.Tokens;
using MinimalApiJwt.Models;

namespace MinimalApiJwt.Services;

public interface IJwtService
{
    LoginResponse GenerateToken(string username, string role);
    bool ValidateToken(string token);
}

public class JwtService : IJwtService
{
    private readonly IConfiguration _config;

    // Constructor Injection - IConfiguration inject pannuvom
    public JwtService(IConfiguration config)
    {
        _config = config;
    }

    public LoginResponse GenerateToken(string username, string role)
    {
        // JWT Secret key - appsettings.json la irunthu padikuvom
        var secretKey = _config["Jwt:SecretKey"]!;
        var issuer = _config["Jwt:Issuer"]!;
        var audience = _config["Jwt:Audience"]!;
        var expiryMinutes = int.Parse(_config["Jwt:ExpiryMinutes"]!);

        // Key create pannuvom - HMAC SHA256 algorithm use pannuvom
        var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(secretKey));

        // Signing credentials - token sign pannum algorithm
        var credentials = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);

        // Claims - token la store aagum user information
        var claims = new[]
        {
            new Claim(ClaimTypes.Name, username),           // Username store
            new Claim(ClaimTypes.Role, role),               // Role store (Admin/Student)
            new Claim(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString()), // Unique token ID
            new Claim(JwtRegisteredClaimNames.Iat,
                DateTimeOffset.UtcNow.ToUnixTimeSeconds().ToString()) // Token issued time
        };

        var expiresAt = DateTime.UtcNow.AddMinutes(expiryMinutes);

        // JWT Token create pannuvom
        var token = new JwtSecurityToken(
            issuer: issuer,       // Token யாரு create pannaru
            audience: audience,   // Token யாருக்காக
            claims: claims,       // Token la irukka data
            expires: expiresAt,   // Token expire time
            signingCredentials: credentials  // Token signature
        );

        // Token string aa convert pannuvom
        var tokenString = new JwtSecurityTokenHandler().WriteToken(token);

        return new LoginResponse
        {
            Token = tokenString,
            Username = username,
            Role = role,
            ExpiresAt = expiresAt
        };
    }

    public bool ValidateToken(string token)
    {
        try
        {
            var key = new SymmetricSecurityKey(
                Encoding.UTF8.GetBytes(_config["Jwt:SecretKey"]!));

            var handler = new JwtSecurityTokenHandler();
            handler.ValidateToken(token, new TokenValidationParameters
            {
                ValidateIssuerSigningKey = true,
                IssuerSigningKey = key,
                ValidateIssuer = true,
                ValidIssuer = _config["Jwt:Issuer"],
                ValidateAudience = true,
                ValidAudience = _config["Jwt:Audience"],
                ValidateLifetime = true  // Token expire aagiruchaa check pannuvom
            }, out _);

            return true;
        }
        catch
        {
            return false;
        }
    }
}
