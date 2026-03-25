namespace CableWinding.Api.Models;

public record LoginRequest(string Login, string Password);

public record LoginResponse(string Token, string Role, string DisplayName);

public class AppUser
{
    public string Login { get; set; } = "";
    public string PasswordHash { get; set; } = "";
    public string Role { get; set; } = "";
    public string DisplayName { get; set; } = "";
}
