using System.ComponentModel.DataAnnotations;

namespace SharedLayer.Model
{
    public class LogEntry : BaseEntity
    {
        public LogLevel Level { get; set; }
        [Required]
        [StringLength(1000)]
        public string Message { get; set; } = string.Empty;
        [StringLength(4000)]
        public string? Details { get; set; }
        [StringLength(4000)]
        public string? Exception { get; set; }
        [Required]
        [StringLength(100)]
        public string Source { get; set; } = string.Empty;
        public Guid? UserId { get; set; }
        [StringLength(45)]
        public string? IpAddress { get; set; }
        [StringLength(500)]
        public string? UserAgent { get; set; }
        [StringLength(500)]
        public string? RequestPath { get; set; }
        [StringLength(10)]
        public string? HttpMethod { get; set; }
        public int? StatusCode { get; set; }

        public long? ResponseTime { get; set; }
        public enum LogLevel
        {
            Info = 1,
            Warning = 2,
            Error = 3,
            Critical = 4
        }
    }
}
