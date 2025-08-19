using System.ComponentModel.DataAnnotations;

namespace SharedLayer.Model
{
    public class Role : BaseEntity
    {
        [Required]
        [StringLength(50)]
        public string Name { get; set; } = string.Empty;
        [StringLength(200)]
        public string? Description { get; set; }
        public ICollection<UserRole> UserRoles { get; set; } = new List<UserRole>();
    }
}
