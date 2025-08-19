using Microsoft.EntityFrameworkCore;
using SharedLayer.Model;

namespace LogService.Infrastructure.Data;


public class LogDbContext : DbContext
{
    public LogDbContext(DbContextOptions<LogDbContext> options) : base(options)
    {
    }
    public DbSet<LogEntry> LogEntries { get; set; }

    protected override void OnModelCreating(ModelBuilder builder)
    {
        base.OnModelCreating(builder);

        builder.Entity<LogEntry>(entity =>
        {
            entity.HasKey(l => l.Id);
            entity.Property(l => l.Level).IsRequired();
            entity.Property(l => l.Message).IsRequired().HasMaxLength(1000);
            entity.Property(l => l.Details).HasMaxLength(4000);
            entity.Property(l => l.Exception).HasMaxLength(4000);
            entity.Property(l => l.Source).IsRequired().HasMaxLength(100);
            entity.Property(l => l.IpAddress).HasMaxLength(45);
            entity.Property(l => l.UserAgent).HasMaxLength(500);
            entity.Property(l => l.RequestPath).HasMaxLength(500);
            entity.Property(l => l.HttpMethod).HasMaxLength(10);
            entity.Property(l => l.CreatedAt).IsRequired();
            entity.Property(l => l.IsActive).IsRequired();

            entity.HasIndex(l => l.Level);
            entity.HasIndex(l => l.Source);
            entity.HasIndex(l => l.CreatedAt);
            entity.HasIndex(l => l.UserId);
            entity.HasIndex(l => l.StatusCode);
            entity.HasIndex(l => l.IpAddress);
            entity.HasIndex(l => new { l.Source, l.Level, l.CreatedAt });
            entity.HasIndex(l => new { l.UserId, l.CreatedAt });
        });
    }
}

    

