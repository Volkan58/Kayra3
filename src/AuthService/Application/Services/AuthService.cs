using Application.Interfaces;
using Infrastructure.Identity;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Logging;
using SharedLayer.Dtos;


namespace Application.Services
{
    public class AuthService : IAuthService
    {
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly SignInManager<ApplicationUser> _signInManager;
        private readonly ITokenService _tokenService;
        private readonly ILogger<AuthService> _logger;

        public AuthService(UserManager<ApplicationUser> userManager, SignInManager<ApplicationUser> signInManager, ITokenService tokenService, ILogger<AuthService> logger)
        {
            _userManager = userManager;
            _signInManager = signInManager;
            _tokenService = tokenService;
            _logger = logger;
        }

        public async Task<AuthResult> RegisterAsync(RegisterRequest request)
        {
            try
            {
                _logger.LogInformation("Kullanıcı kaydı başlatıldı: {Username}", request.Username);

                var existingUser = await _userManager.FindByNameAsync(request.Username);
                if (existingUser != null)
                {
                    _logger.LogWarning("Kullanıcı adı zaten mevcut: {Username}", request.Username);
                    return AuthResult.Failure("Kullanıcı adı zaten kullanılıyor.");
                }

                var existingEmail = await _userManager.FindByEmailAsync(request.Email);
                if (existingEmail != null)
                {
                    _logger.LogWarning("E-posta adresi zaten mevcut: {Email}", request.Email);
                    return AuthResult.Failure("E-posta adresi zaten kullanılıyor.");
                }

                var user = new ApplicationUser
                {
                    UserName = request.Username,
                    Email = request.Email,
                    FirstName = request.FirstName,
                    LastName = request.LastName,
                    PhoneNumber = request.PhoneNumber,
                    CreatedAt = DateTime.UtcNow,
                    IsActive = true
                };

                var result = await _userManager.CreateAsync(user, request.Password);
                if (!result.Succeeded)
                {
                    var errors = string.Join(", ", result.Errors);
                    _logger.LogError("Kullanıcı oluşturulamadı: {Errors}", errors);
                    return AuthResult.Failure($"Kullanıcı oluşturulamadı: {errors}");
                }

                await _userManager.AddToRoleAsync(user, "User");

                _logger.LogInformation("Kullanıcı başarıyla oluşturuldu: {Username}", request.Username);

                return await LoginAsync(new LoginRequest
                {
                    UsernameOrEmail = request.Username,
                    Password = request.Password
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Kullanıcı kaydı sırasında hata oluştu: {Username}", request.Username);
                return AuthResult.Failure("Kullanıcı kaydı sırasında bir hata oluştu.");
            }
        }
        public async Task<AuthResult> LoginAsync(LoginRequest request)
        {
            try
            {
                _logger.LogInformation("Kullanıcı girişi başlatıldı: {Username}", request.UsernameOrEmail);
                var user = await _userManager.FindByNameAsync(request.UsernameOrEmail) ??
                          await _userManager.FindByEmailAsync(request.UsernameOrEmail);

                if (user == null)
                {
                    _logger.LogWarning("Kullanıcı bulunamadı: {Username}", request.UsernameOrEmail);
                    return AuthResult.Failure("Kullanıcı adı veya şifre hatalı.");
                }

                if (!user.IsActive)
                {
                    _logger.LogWarning("Pasif kullanıcı giriş denemesi: {Username}", request.UsernameOrEmail);
                    return AuthResult.Failure("Hesabınız aktif değil.");
                }
                var signInResult = await _signInManager.CheckPasswordSignInAsync(user, request.Password, false);
                if (!signInResult.Succeeded)
                {
                    _logger.LogWarning("Hatalı şifre girişi: {Username}", request.UsernameOrEmail);
                    return AuthResult.Failure("Kullanıcı adı veya şifre hatalı.");
                }
                var roles = await _userManager.GetRolesAsync(user);
                var accessToken = _tokenService.GenerateAccessToken(user);
                var refreshToken = await _tokenService.GenerateRefreshTokenAsync(user);

                var loginResponse = new LoginResponse
                {
                    AccessToken = accessToken,
                    RefreshToken = refreshToken,
                    ExpiresAt = DateTime.UtcNow.AddMinutes(60), 
                    User = new UserDto
                    {
                        Id = user.Id,
                        Username = user.UserName ?? string.Empty,
                        Email = user.Email ?? string.Empty,
                        FirstName = user.FirstName,
                        LastName = user.LastName,
                        PhoneNumber = user.PhoneNumber,
                        Roles = roles.ToList()
                    }
                };

                _logger.LogInformation("Kullanıcı başarıyla giriş yaptı: {Username}", request.UsernameOrEmail);

                return AuthResult.Success(loginResponse);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Kullanıcı girişi sırasında hata oluştu: {Username}", request.UsernameOrEmail);
                return AuthResult.Failure("Giriş sırasında bir hata oluştu.");
            }
        }

        public async Task<AuthResult> RefreshTokenAsync(RefreshTokenRequest request)
        {
            try
            {
                _logger.LogInformation("Token yenileme başlatıldı");

                await _tokenService.InvalidateRefreshTokenAsync(request.RefreshToken);

                var userId = _tokenService.GetUserIdFromToken(request.RefreshToken);
                if (!userId.HasValue)
                {
                    _logger.LogWarning("Geçersiz refresh token");
                    return AuthResult.Failure("Geçersiz refresh token.");
                }
                var user = await _userManager.FindByIdAsync(userId.Value.ToString());
                if (user == null || !user.IsActive)
                {
                    _logger.LogWarning("Kullanıcı bulunamadı veya pasif: {UserId}", userId);
                    return AuthResult.Failure("Kullanıcı bulunamadı.");
                }

                var accessToken = _tokenService.GenerateAccessToken(user);
                var refreshToken = await _tokenService.GenerateRefreshTokenAsync(user);

                var loginResponse = new LoginResponse
                {
                    AccessToken = accessToken,
                    RefreshToken = refreshToken,
                    ExpiresAt = DateTime.UtcNow.AddMinutes(60),
                    User = new UserDto
                    {
                        Id = user.Id,
                        Username = user.UserName ?? string.Empty,
                        Email = user.Email ?? string.Empty,
                        FirstName = user.FirstName,
                        LastName = user.LastName,
                        PhoneNumber = user.PhoneNumber,
                        Roles = (await _userManager.GetRolesAsync(user)).ToList()
                    }
                };

                _logger.LogInformation("Token başarıyla yenilendi: {UserId}", userId);

                return AuthResult.Success(loginResponse);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Token yenileme sırasında hata oluştu");
                return AuthResult.Failure("Token yenileme sırasında bir hata oluştu.");
            }
        }
        public async Task<bool> LogoutAsync(Guid userId)
        {
            try
            {
                _logger.LogInformation("Kullanıcı çıkışı başlatıldı: {UserId}", userId);

                var user = await _userManager.FindByIdAsync(userId.ToString());
                if (user != null)
                {
                    await _signInManager.SignOutAsync();
                    _logger.LogInformation("Kullanıcı başarıyla çıkış yaptı: {UserId}", userId);
                    return true;
                }

                return false;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Kullanıcı çıkışı sırasında hata oluştu: {UserId}", userId);
                return false;
            }
        }

        /// <summary>
        /// Kullanıcı bilgilerini getir
        /// </summary>
        public async Task<UserDto?> GetUserAsync(Guid userId)
        {
            try
            {
                var user = await _userManager.FindByIdAsync(userId.ToString());
                if (user == null || !user.IsActive)
                {
                    return null;
                }

                var roles = await _userManager.GetRolesAsync(user);

                return new UserDto
                {
                    Id = user.Id,
                    Username = user.UserName ?? string.Empty,
                    Email = user.Email ?? string.Empty,
                    FirstName = user.FirstName,
                    LastName = user.LastName,
                    PhoneNumber = user.PhoneNumber,
                    Roles = roles.ToList()
                };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Kullanıcı bilgileri alınırken hata oluştu: {UserId}", userId);
                return null;
            }
        }
    }
}
