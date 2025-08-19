using System.ComponentModel.DataAnnotations;

namespace SharedLayer.Model
{
    public class RefreshToken : BaseEntity
    {
        public Guid UserId { get; set; }
        [Required]
        public string Token { get; set; } = string.Empty;
        public DateTime ExpiresAt { get; set; }
        public User User { get; set; } = null!;
    }
}
