using VinylShop.Shared.Models;

public interface IAuthService
{
    Task<AuthResult> Login(LoginModel loginModel);
    Task Logout();
}

public class AuthResult
{
    public bool Success { get; set; }
    public string Message { get; set; } = string.Empty;
}