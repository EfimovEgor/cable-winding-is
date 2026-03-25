using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using CableWinding.Api.Models;
using Microsoft.IdentityModel.Tokens;

namespace CableWinding.Api.Services;

public interface IAuthService
{
    LoginResponse? TryLogin(string login, string password);
}

public class AuthService : IAuthService
{
    private readonly IConfiguration _config;
    private readonly List<AppUser> _users;

    public AuthService(IConfiguration config)
    {
        _config = config;
        _users = LoadUsers();
    }

    private List<AppUser> LoadUsers()
    {
        var section = _config.GetSection("Users");
        if (!section.Exists())
            return GetDefaultUsers();

        var list = new List<AppUser>();
        foreach (var child in section.GetChildren())
        {
            var login = child["Login"] ?? child.Key;
            var role = child["Role"] ?? "Operator";
            var displayName = child["DisplayName"] ?? role;
            var password = child["Password"] ?? "123"; // В продакшене только хеш!
            list.Add(new AppUser
            {
                Login = login,
                PasswordHash = HashPassword(password),
                Role = role,
                DisplayName = displayName
            });
        }
        return list.Count > 0 ? list : GetDefaultUsers();
    }

    private static List<AppUser> GetDefaultUsers()
    {
        return
        [
            new AppUser { Login = "operator", PasswordHash = HashPassword("operator123"), Role = "Operator", DisplayName = "Оператор АСУТП" },
            new AppUser { Login = "technologist", PasswordHash = HashPassword("technologist123"), Role = "Technologist", DisplayName = "Технолог" },
            new AppUser { Login = "engineer", PasswordHash = HashPassword("engineer123"), Role = "Engineer", DisplayName = "Сервисный инженер" }
        ];
    }

    private static string HashPassword(string password)
    {
        using var sha = System.Security.Cryptography.SHA256.Create();
        var bytes = Encoding.UTF8.GetBytes(password);
        var hash = sha.ComputeHash(bytes);
        return Convert.ToBase64String(hash);
    }

    public LoginResponse? TryLogin(string login, string password)
    {
        var hash = HashPassword(password);
        var user = _users.FirstOrDefault(u =>
            string.Equals(u.Login, login, StringComparison.OrdinalIgnoreCase) &&
            u.PasswordHash == hash);

        if (user == null)
            return null;

        var token = GenerateToken(user);
        return new LoginResponse(token, user.Role, user.DisplayName);
    }

    private string GenerateToken(AppUser user)
    {
        var key = _config["Jwt:Key"] ?? "CableWindingSecretKey_MustBeLongEnough_ForHS256";
        var issuer = _config["Jwt:Issuer"] ?? "CableWinding";
        var audience = _config["Jwt:Audience"] ?? "CableWinding";
        var expireMinutes = int.TryParse(_config["Jwt:ExpireMinutes"], out var m) ? m : 480; // 8 ч

        var securityKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(key));
        var credentials = new SigningCredentials(securityKey, SecurityAlgorithms.HmacSha256);

        var claims = new[]
        {
            new Claim(ClaimTypes.Name, user.Login),
            new Claim(ClaimTypes.Role, user.Role),
            new Claim("displayName", user.DisplayName)
        };

        var token = new JwtSecurityToken(
            issuer,
            audience,
            claims,
            expires: DateTime.UtcNow.AddMinutes(expireMinutes),
            signingCredentials: credentials);

        return new JwtSecurityTokenHandler().WriteToken(token);
    }
}
