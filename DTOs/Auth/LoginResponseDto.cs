namespace backend.DTOs.Auth;

public class LoginResponseDto
{
    public UserDto? User { get; set; }
    public TokenDto? AccessToken { get; set; }
    public TokenDto? RefreshToken { get; set; }
}

public class TokenDto
{
    public string? Value { get; set; }
    public DateTime? ExpiresAt { get; set; }
}

public class UserDto
{
    public string? UserName { get; set; }
}