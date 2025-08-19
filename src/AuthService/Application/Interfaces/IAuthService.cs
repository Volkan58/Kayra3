

using SharedLayer.Dtos;

namespace Application.Interfaces
{
    public interface IAuthService
    {
        Task<AuthResult> RegisterAsync(RegisterRequest request);
        Task<AuthResult> LoginAsync(LoginRequest request);
        Task<AuthResult> RefreshTokenAsync(RefreshTokenRequest request);
        Task<bool> LogoutAsync(Guid userId);
        Task<UserDto?> GetUserAsync(Guid userId);
    }

    public class AuthResult
    {
        public bool IsSuccess { get; set; }
        public string? ErrorMessage { get; set; }
        public LoginResponse? LoginResponse { get; set; }
        public static AuthResult Success(LoginResponse loginResponse)
        {
            return new AuthResult
            {
                IsSuccess = true,
                LoginResponse = loginResponse
            };
        }
        public static AuthResult Failure(string errorMessage)
        {
            return new AuthResult
            {
                IsSuccess = false,
                ErrorMessage = errorMessage
            };
        }
    }
}
