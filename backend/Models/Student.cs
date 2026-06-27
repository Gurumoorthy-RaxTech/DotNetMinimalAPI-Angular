namespace MinimalApiJwt.Models;

// Student entity - database la store aagum data
public class Student
{
    public int Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public string Course { get; set; } = string.Empty;
    public int Age { get; set; }
}

// Login request body - user send pannum data
public class LoginRequest
{
    public string Username { get; set; } = string.Empty;
    public string Password { get; set; } = string.Empty;
}

// Login response - server return pannum data (Access + Refresh token)
public class LoginResponse
{
    public string Token { get; set; } = string.Empty;           // Access token - 15 min
    public string RefreshToken { get; set; } = string.Empty;    // Refresh token - 7 days
    public string Username { get; set; } = string.Empty;
    public string Role { get; set; } = string.Empty;
    public DateTime ExpiresAt { get; set; }
    public DateTime RefreshTokenExpiresAt { get; set; }
}

// Refresh token request - client sends expired access token + refresh token
public class RefreshTokenRequest
{
    public string AccessToken { get; set; } = string.Empty;
    public string RefreshToken { get; set; } = string.Empty;
}

// Stored refresh token - in-memory (real app: database la store pannuvom)
public class RefreshTokenRecord
{
    public string Token { get; set; } = string.Empty;
    public string Username { get; set; } = string.Empty;
    public string Role { get; set; } = string.Empty;
    public DateTime ExpiresAt { get; set; }
    public bool IsRevoked { get; set; }
    public string? ReplacedByToken { get; set; }   // Rotation track pannuvom
}

// API response wrapper - consistent response format
public class ApiResponse<T>
{
    public bool Success { get; set; }
    public string Message { get; set; } = string.Empty;
    public T? Data { get; set; }

    public static ApiResponse<T> Ok(T data, string message = "Success") =>
        new() { Success = true, Message = message, Data = data };

    public static ApiResponse<T> Fail(string message) =>
        new() { Success = false, Message = message };
}
