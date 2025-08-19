using Infrastructure.Identity;

namespace Application.Interfaces
{
    public interface ITokenService
    {
        string GenerateAccessToken(ApplicationUser user);
        Task<string> GenerateRefreshTokenAsync(ApplicationUser user);
        Guid? GetUserIdFromToken(string token);
        bool ValidateToken(string token);
        Task<bool> InvalidateRefreshTokenAsync(string token);
    }
}
