using System.Security.Claims;
using System.Text.Json;
using Blazored.LocalStorage;
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Authorization;
using static System.Net.WebRequestMethods;
using VinylShop.Shared.Models;

public class CustomAuthenticationStateProvider : AuthenticationStateProvider
{
    private readonly ILocalStorageService _localStorage;
    private readonly HttpClient _http;

    public CustomAuthenticationStateProvider(ILocalStorageService localStorage, HttpClient http)
    {
        _localStorage = localStorage;
        _http = http;
    }

    public override async Task<AuthenticationState> GetAuthenticationStateAsync()
    {
        var token = await _localStorage.GetItemAsync<string>("authToken");

        var identity = new ClaimsIdentity();

        if (!string.IsNullOrEmpty(token))
        {
            identity = new ClaimsIdentity(ParseClaimsFromJwt(token), "jwt");
        }

        var user = new ClaimsPrincipal(identity);
        return new AuthenticationState(user);
    }

    public async Task MarkUserAsAuthenticated(string token)
    {
        await _localStorage.SetItemAsync("authToken", token);

        var identity = new ClaimsIdentity(ParseClaimsFromJwt(token), "jwt");
        var user = new ClaimsPrincipal(identity);

        NotifyAuthenticationStateChanged(Task.FromResult(new AuthenticationState(user)));
    }

    public async Task MarkUserAsLoggedOut()
    {
        await _localStorage.RemoveItemAsync("authToken");

        var identity = new ClaimsIdentity();
        var user = new ClaimsPrincipal(identity);

        NotifyAuthenticationStateChanged(Task.FromResult(new AuthenticationState(user)));
    }

    private IEnumerable<Claim> ParseClaimsFromJwt(string jwt)
    {
        var claims = new List<Claim>();
        var payload = jwt.Split('.')[1];
        var jsonBytes = ParseBase64WithoutPadding(payload);
        var keyValuePairs = JsonSerializer.Deserialize<Dictionary<string, object>>(jsonBytes);

        if (keyValuePairs != null)
        {
            keyValuePairs.TryGetValue(ClaimTypes.Role, out object roles);

            if (roles != null)
            {
                if (roles.ToString().Trim().StartsWith("["))
                {
                    var parsedRoles = JsonSerializer.Deserialize<string[]>(roles.ToString());
                    foreach (var parsedRole in parsedRoles)
                    {
                        claims.Add(new Claim(ClaimTypes.Role, parsedRole));
                    }
                }
                else
                {
                    claims.Add(new Claim(ClaimTypes.Role, roles.ToString()));
                }
            }

            foreach (var kvp in keyValuePairs)
            {
                if (kvp.Key != ClaimTypes.Role)
                    claims.Add(new Claim(kvp.Key, kvp.Value.ToString()));
            }
        }

        return claims;
    }

    private byte[] ParseBase64WithoutPadding(string base64)
    {
        switch (base64.Length % 4)
        {
            case 2: base64 += "=="; break;
            case 3: base64 += "="; break;
        }
        return Convert.FromBase64String(base64);
    }
}
//1. Начальное состояние:
//- Пользователь открывает сайт
//- Приложение проверяет localStorage через CustomAuthenticationStateProvider
//- Так как токена еще нет, пользователь не аутентифицирован

//2. Попытка доступа к защищенной странице 
//@page "/admin"
//@attribute [Authorize] // Этот атрибут проверяет аутентификацию
//Blazor видит атрибут [Authorize] и проверяет состояние аутентификации. Так как пользователь не аутентифицирован, его перенаправляет на страницу логина.

//3. Процесс логина:
//// Пример метода логина
//public async Task Login(LoginModel loginModel)
//{
//    // Отправляем запрос на сервер
//    var response = await _http.PostAsJsonAsync("api/auth/login", loginModel);

//    if (response.IsSuccessStatusCode)
//    {
//        var authResponse = await response.Content.ReadFromJsonAsync<AuthResponse>();

//        // Сохраняем токен
//        await ((CustomAuthenticationStateProvider)AuthenticationStateProvider)
//            .MarkUserAsAuthenticated(authResponse.Token);

//        // Перенаправляем на защищенную страницу
//        NavigationManager.NavigateTo("/admin");
//    }
//}

//4. Сервер обрабатывает запрос (AuthController)
//    [HttpPost("login")]
//public ActionResult<AuthResponse> Login([FromBody] LoginModel loginModel)
//{
//    // Проверяет credentials
//    // Генерирует JWT токен
//    // Генерирует refresh token
//    return new AuthResponse
//    {
//        Token = token,
//        RefreshToken = refreshToken,
//        TokenExpires = expires
//    };
//}

//5.Сохранение токена;
//// В CustomAuthenticationStateProvider
//public async Task MarkUserAsAuthenticated(string token)
//{
//    // Сохраняем в localStorage
//    await _localStorage.SetItemAsync("authToken", token);

//    // Создаем ClaimsIdentity из токена
//    var identity = new ClaimsIdentity(ParseClaimsFromJwt(token), "jwt");
//    var user = new ClaimsPrincipal(identity);

//    // Уведомляем систему об изменении состояния аутентификации
//    NotifyAuthenticationStateChanged(Task.FromResult(new AuthenticationState(user)));
//}

//6. Последующие запросы к API:
//// При каждом запросе к API токен автоматически добавляется в заголовки
//protected override async Task OnInitializedAsync()
//{
//    // Токен автоматически берется из localStorage и добавляется в заголовок
//    _albums = await Http.GetFromJsonAsync<IEnumerable<Album>>("api/albums");
//}


//7. Проверка токена
//// В CustomAuthenticationStateProvider
//public override async Task<AuthenticationState> GetAuthenticationStateAsync()
//{
//    // Получаем токен из localStorage
//    var token = await _localStorage.GetItemAsync<string>("authToken");

//    var identity = new ClaimsIdentity();

//    if (!string.IsNullOrEmpty(token))
//    {
//        // Создаем ClaimsIdentity из токена
//        identity = new ClaimsIdentity(ParseClaimsFromJwt(token), "jwt");
//    }

//    return new AuthenticationState(new ClaimsPrincipal(identity));
//}

//8.Когда тьокен истекает;
//-Сервер возвращает 401 Unauthorized
//- Клиент использует refresh token для получения нового токена
//- Новый токен сохраняется в localStorage
//- Процесс повторяется

//9. При выходе:
//public async Task Logout()
//{
//    // Удаляем токен из localStorage
//    await ((CustomAuthenticationStateProvider)AuthenticationStateProvider)
//        .MarkUserAsLoggedOut();

//    // Перенаправляем на главную страницу
//    NavigationManager.NavigateTo("/");
//}

//Основные преимущества этого подхода:
//Токен хранится в localStorage, что позволяет сохранять сессию даже после перезагрузки страницы
//JWT токен содержит всю необходимую информацию о пользователе (claims)
//Система автоматически защищает страницы с атрибутом [Authorize]
//Refresh token позволяет автоматически обновлять сессию без необходимости повторного входа