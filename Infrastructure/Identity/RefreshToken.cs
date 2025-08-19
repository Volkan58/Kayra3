using System.ComponentModel.DataAnnotations;
using SharedLayer.Model;

namespace Infrastructure.Identity
{
    public class RefreshToken : BaseEntity
    {
        public Guid UserId { get; set; }
        [Required]
        public string Token { get; set; } = string.Empty;
        public DateTime ExpiresAt { get; set; }
        public ApplicationUser User { get; set; } = null!;
    }
}
