using Microsoft.AspNetCore.Mvc;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using VinylShop.Api.Service;
using VinylShop.Shared.Models;

namespace VinylShop.Api.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class AuthController : ControllerBase
    {
        private readonly IConfiguration _configuration;
        private readonly ITokenService _tokenService;
        private readonly Dictionary<string, RefreshToken> _refreshTokens = new();

        public AuthController(IConfiguration configuration, ITokenService tokenService)
        {
            _configuration = configuration;
            _tokenService = tokenService;
        }

        [HttpPost("login")]
        public ActionResult<AuthResponse> Login([FromBody] LoginModel loginModel)
        {
            if (loginModel.Username != _configuration["LoginModel:Username"] ||
                loginModel.Password != _configuration["LoginModel:Password"])
                return Unauthorized();

            var claims = new List<Claim>
        {
            new(ClaimTypes.Name, loginModel.Username),
            new(ClaimTypes.Role, "Admin")
        };

            var token = CreateToken(claims);
            var refreshToken = _tokenService.GenerateRefreshToken();

            // Сохраняем refresh token (в реальном приложении это должно быть в базе данных)
            _refreshTokens[refreshToken] = new RefreshToken
            {
                Token = refreshToken,
                Created = DateTime.Now,
                Expires = DateTime.Now.AddDays(7)
            };

            return new AuthResponse
            {
                Token = new JwtSecurityTokenHandler().WriteToken(token),
                RefreshToken = refreshToken,
                TokenExpires = token.ValidTo
            };
        }

        [HttpPost("refresh")]
        public ActionResult<AuthResponse> Refresh([FromBody] RefreshRequest request)
        {
            if (!_refreshTokens.TryGetValue(request.RefreshToken, out var refreshToken))
                return Unauthorized("Invalid refresh token.");

            if (refreshToken.Expires < DateTime.Now)
            {
                _refreshTokens.Remove(request.RefreshToken);
                return Unauthorized("Refresh token expired.");
            }

            // В реальном приложении здесь нужно получить claims из базы данных
            var claims = new List<Claim>
        {
            new(ClaimTypes.Name, _configuration["LoginModel:Username"]!),
            new(ClaimTypes.Role, "Admin")
        };

            var token = CreateToken(claims);
            var newRefreshToken = _tokenService.GenerateRefreshToken();

            _refreshTokens.Remove(request.RefreshToken);
            _refreshTokens[newRefreshToken] = new RefreshToken
            {
                Token = newRefreshToken,
                Created = DateTime.Now,
                Expires = DateTime.Now.AddDays(7)
            };

            return new AuthResponse
            {
                Token = new JwtSecurityTokenHandler().WriteToken(token),
                RefreshToken = newRefreshToken,
                TokenExpires = token.ValidTo
            };
        }

        private JwtSecurityToken CreateToken(List<Claim> claims)
        {
            var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(
                _configuration["Jwt:Key"]!));

            var creds = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);

            return _tokenService.GenerateTokenOptions(creds, claims);
        }
    }

    public class RefreshToken
    {
        public string Token { get; set; } = string.Empty;
        public DateTime Created { get; set; }
        public DateTime Expires { get; set; }
    }
}
