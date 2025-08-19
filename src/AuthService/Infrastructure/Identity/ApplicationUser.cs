using Microsoft.AspNetCore.Identity;
using SharedLayer.Model;

namespace Infrastructure.Identity
{
    public class ApplicationUser : IdentityUser<Guid>
    {
        public string? FirstName { get; set; }

        public string? LastName { get; set; }
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        public DateTime? UpdatedAt { get; set; }

        public bool IsActive { get; set; } = true;

        public DateTime? DeletedAt { get; set; }

        public string FullName => $"{FirstName} {LastName}".Trim();

        public User ToSharedUser()
        {
            return new User
            {
                Id = Id,
                Username = UserName ?? string.Empty,
                Email = Email ?? string.Empty,
                FirstName = FirstName,
                LastName = LastName,
                PhoneNumber = PhoneNumber,
                CreatedAt = CreatedAt,
                UpdatedAt = UpdatedAt,
                IsActive = IsActive,
                DeletedAt = DeletedAt
            };
        }

    }
}
