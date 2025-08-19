using Application.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using SharedLayer.Dtos;

namespace AuthServiceAPI.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class AuthController : ControllerBase
    {
        private readonly IAuthService _authService;
        private readonly ILogger<AuthController> _logger;

        public AuthController(IAuthService authService, ILogger<AuthController> logger)
        {
            _authService = authService;
            _logger = logger;
        }
        [HttpPost("register")]
        [ProducesResponseType(typeof(LoginResponse), 200)]
        [ProducesResponseType(typeof(string), 400)]
        [ProducesResponseType(typeof(string), 409)]
        [ProducesResponseType(typeof(string), 500)]
        public async Task<IActionResult> Register([FromBody] RegisterRequest request)
        {
            try
            {
                _logger.LogInformation("Kullanıcı kaydı API çağrısı: {Username}", request.Username);

                if (!ModelState.IsValid)
                {
                    var errors = ModelState.Values
                        .SelectMany(v => v.Errors)
                        .Select(e => e.ErrorMessage)
                        .ToList();

                    _logger.LogWarning("Geçersiz model state: {Errors}", string.Join(", ", errors));
                    return BadRequest(string.Join(", ", errors));
                }

                var result = await _authService.RegisterAsync(request);

                if (result.IsSuccess)
                {
                    _logger.LogInformation("Kullanıcı kaydı başarılı: {Username}", request.Username);
                    return Ok(result.LoginResponse);
                }

                _logger.LogWarning("Kullanıcı kaydı başarısız: {Username}, Hata: {Error}",
                    request.Username, result.ErrorMessage);

                if (result.ErrorMessage?.Contains("zaten") == true)
                {
                    return Conflict(result.ErrorMessage);
                }

                return BadRequest(result.ErrorMessage);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Kullanıcı kaydı sırasında beklenmeyen hata: {Username}", request.Username);
                return StatusCode(500, "Kullanıcı kaydı sırasında bir hata oluştu.");
            }
        }

        [HttpPost("login")]
        [ProducesResponseType(typeof(LoginResponse), 200)]
        [ProducesResponseType(typeof(string), 400)]
        [ProducesResponseType(typeof(string), 401)]
        [ProducesResponseType(typeof(string), 500)]
        public async Task<IActionResult> Login([FromBody] LoginRequest request)
        {
            try
            {
                _logger.LogInformation("Kullanıcı girişi API çağrısı: {Username}", request.UsernameOrEmail);

                if (!ModelState.IsValid)
                {
                    var errors = ModelState.Values
                        .SelectMany(v => v.Errors)
                        .Select(e => e.ErrorMessage)
                        .ToList();

                    _logger.LogWarning("Geçersiz model state: {Errors}", string.Join(", ", errors));
                    return BadRequest(string.Join(", ", errors));
                }

                var result = await _authService.LoginAsync(request);

                if (result.IsSuccess)
                {
                    _logger.LogInformation("Kullanıcı girişi başarılı: {Username}", request.UsernameOrEmail);
                    return Ok(result.LoginResponse);
                }

                _logger.LogWarning("Kullanıcı girişi başarısız: {Username}, Hata: {Error}",
                    request.UsernameOrEmail, result.ErrorMessage);

                return Unauthorized(result.ErrorMessage);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Kullanıcı girişi sırasında beklenmeyen hata: {Username}", request.UsernameOrEmail);
                return StatusCode(500, "Giriş sırasında bir hata oluştu.");
            }
        }
        [HttpPost("refresh-token")]
        [ProducesResponseType(typeof(LoginResponse), 200)]
        [ProducesResponseType(typeof(string), 400)]
        [ProducesResponseType(typeof(string), 401)]
        [ProducesResponseType(typeof(string), 500)]
        public async Task<IActionResult> RefreshToken([FromBody] RefreshTokenRequest request)
        {
            try
            {
                _logger.LogInformation("Token yenileme API çağrısı");

                if (!ModelState.IsValid)
                {
                    var errors = ModelState.Values
                        .SelectMany(v => v.Errors)
                        .Select(e => e.ErrorMessage)
                        .ToList();

                    _logger.LogWarning("Geçersiz model state: {Errors}", string.Join(", ", errors));
                    return BadRequest(string.Join(", ", errors));
                }

                var result = await _authService.RefreshTokenAsync(request);

                if (result.IsSuccess)
                {
                    _logger.LogInformation("Token yenileme başarılı");
                    return Ok(result.LoginResponse);
                }

                _logger.LogWarning("Token yenileme başarısız: {Error}", result.ErrorMessage);

                return Unauthorized(result.ErrorMessage);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Token yenileme sırasında beklenmeyen hata");
                return StatusCode(500, "Token yenileme sırasında bir hata oluştu.");
            }
        }
        [HttpPost("logout")]
        [Authorize]
        [ProducesResponseType(typeof(bool), 200)]
        [ProducesResponseType(typeof(string), 401)]
        [ProducesResponseType(typeof(string), 500)]
        public async Task<IActionResult> Logout()
        {
            try
            {
                var userIdClaim = User.FindFirst("userId")?.Value;
                if (!Guid.TryParse(userIdClaim, out var userId))
                {
                    _logger.LogWarning("Geçersiz kullanıcı ID claim");
                    return Unauthorized("Geçersiz kullanıcı bilgisi.");
                }

                _logger.LogInformation("Kullanıcı çıkışı API çağrısı: {UserId}", userId);

                var result = await _authService.LogoutAsync(userId);

                if (result)
                {
                    _logger.LogInformation("Kullanıcı çıkışı başarılı: {UserId}", userId);
                    return Ok(true);
                }

                _logger.LogWarning("Kullanıcı çıkışı başarısız: {UserId}", userId);
                return BadRequest("Çıkış işlemi başarısız.");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Kullanıcı çıkışı sırasında beklenmeyen hata");
                return StatusCode(500, "Çıkış sırasında bir hata oluştu.");
            }
        }

        [HttpGet("me")]
        [Authorize]
        [ProducesResponseType(typeof(UserDto), 200)]
        [ProducesResponseType(typeof(string), 401)]
        [ProducesResponseType(typeof(string), 404)]
        [ProducesResponseType(typeof(string), 500)]
        public async Task<IActionResult> GetCurrentUser()
        {
            try
            {
                var userIdClaim = User.FindFirst("userId")?.Value;
                if (!Guid.TryParse(userIdClaim, out var userId))
                {
                    _logger.LogWarning("Geçersiz kullanıcı ID claim");
                    return Unauthorized("Geçersiz kullanıcı bilgisi.");
                }

                _logger.LogInformation("Mevcut kullanıcı bilgileri API çağrısı: {UserId}", userId);

                var user = await _authService.GetUserAsync(userId);

                if (user != null)
                {
                    _logger.LogInformation("Kullanıcı bilgileri başarıyla alındı: {UserId}", userId);
                    return Ok(user);
                }

                _logger.LogWarning("Kullanıcı bulunamadı: {UserId}", userId);
                return NotFound("Kullanıcı bulunamadı.");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Kullanıcı bilgileri alınırken beklenmeyen hata");
                return StatusCode(500, "Kullanıcı bilgileri alınırken bir hata oluştu.");
            }
        }
        [HttpGet("health")]
        [ProducesResponseType(typeof(string), 200)]
        public IActionResult Health()
        {
            return Ok("Auth Service is running");
        }
    }
}
