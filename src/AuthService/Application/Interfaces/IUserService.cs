

using SharedLayer.Dtos;

namespace Application.Interfaces
{
    public interface IUserService
    {
        Task<UserDto?> GetUserAsync(Guid userId);
        Task<IEnumerable<UserDto>> GetUsersAsync(int page = 1, int pageSize = 20);
        Task<bool> UpdateUserAsync(Guid userId, UserDto userDto);
        Task<bool> DeactivateUserAsync(Guid userId);
    }
}
