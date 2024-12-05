using Blazored.LocalStorage;
using Microsoft.AspNetCore.Components.Authorization;
using System.Net.Http.Json;
using VinylShop.Shared.Models;

public class AuthService : IAuthService
{
    private readonly HttpClient _http;
    private readonly AuthenticationStateProvider _authStateProvider;
    private readonly ILocalStorageService _localStorage;

    public AuthService(HttpClient http,
                      AuthenticationStateProvider authStateProvider,
                      ILocalStorageService localStorage)
    {
        _http = http;
        _authStateProvider = authStateProvider;
        _localStorage = localStorage;
    }

    public async Task<AuthResult> Login(LoginModel loginModel)
    {
        var response = await _http.PostAsJsonAsync("api/auth/login", loginModel);
        if (response.IsSuccessStatusCode)
        {
            var authResponse = await response.Content.ReadFromJsonAsync<AuthResponse>();
            await ((CustomAuthenticationStateProvider)_authStateProvider)
                .MarkUserAsAuthenticated(authResponse.Token);

            return new AuthResult { Success = true };
        }

        return new AuthResult
        {
            Success = false,
            Message = "Invalid username or password"
        };
    }

    public async Task Logout()
    {
        await ((CustomAuthenticationStateProvider)_authStateProvider).MarkUserAsLoggedOut();
    }
}