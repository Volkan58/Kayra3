using Infrastructure.Identity;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using SharedLayer.Model;

namespace AuthService.Infrastructure.Data;


public class AuthDbContext : IdentityDbContext<ApplicationUser, ApplicationRole, Guid>
{
    public AuthDbContext(DbContextOptions<AuthDbContext> options) : base(options)
    {
    }

    public DbSet<RefreshToken> RefreshTokens { get; set; }

    protected override void OnModelCreating(ModelBuilder builder)
    {
        base.OnModelCreating(builder);

        builder.Entity<RefreshToken>(entity =>
        {
            entity.HasKey(rt => rt.Id);
            entity.Property(rt => rt.Token).IsRequired().HasMaxLength(500);
            entity.Property(rt => rt.ExpiresAt).IsRequired();

            entity.HasOne(rt => rt.User)
                  .WithMany(u => u.RefreshTokens)
                  .HasForeignKey(rt => rt.UserId)
                  .OnDelete(DeleteBehavior.Cascade);
        });

        builder.Entity<ApplicationUser>(entity =>
        {
            entity.Property(u => u.FirstName).HasMaxLength(50);
            entity.Property(u => u.LastName).HasMaxLength(50);
            entity.Property(u => u.CreatedAt).IsRequired();
            entity.Property(u => u.IsActive).IsRequired();
        });

        builder.Entity<ApplicationRole>(entity =>
        {
            entity.Property(r => r.Description).HasMaxLength(200);
            entity.Property(r => r.CreatedAt).IsRequired();
            entity.Property(r => r.IsActive).IsRequired();
        });
        SeedData(builder);
    }

    private void SeedData(ModelBuilder builder)
    {
       
        var adminRoleId = new Guid("11111111-1111-1111-1111-111111111111");
        var userRoleId = new Guid("22222222-2222-2222-2222-222222222222");
        var adminUserId = new Guid("33333333-3333-3333-3333-333333333333");

        var fixedDate = new DateTime(2025, 01, 01);

        builder.Entity<ApplicationRole>().HasData(
            new ApplicationRole
            {
                Id = adminRoleId,
                Name = "Admin",
                NormalizedName = "ADMIN",
                Description = "Sistem yöneticisi",
                CreatedAt = fixedDate,
                IsActive = true
            },
            new ApplicationRole
            {
                Id = userRoleId,
                Name = "User",
                NormalizedName = "USER",
                Description = "Standart kullanıcı",
                CreatedAt = fixedDate,
                IsActive = true
            }
        );

        var adminUser = new ApplicationUser
        {
            Id = adminUserId,
            UserName = "admin",
            NormalizedUserName = "ADMIN",
            Email = "admin@kayraexport.com",
            NormalizedEmail = "ADMIN@KAYRAEXPORT.COM",
            EmailConfirmed = true,
            FirstName = "Admin",
            LastName = "User",
            CreatedAt = fixedDate,
            IsActive = true,
            SecurityStamp = "STATIC-SECURITY-STAMP-0001",
            ConcurrencyStamp = "STATIC-CONCURRENCY-STAMP-0001" 
        };

        adminUser.PasswordHash =
            "AQAAAAIAAYagAAAAEH5h3Y7U2zM6M0o0MnOTUQYcPp+0wKuX7PQxz7f0nRZB7uRikH8AkJq2h8Ynl4o2Vw==";
        builder.Entity<ApplicationUser>().HasData(adminUser);

        builder.Entity<IdentityUserRole<Guid>>().HasData(new IdentityUserRole<Guid>
        {
            UserId = adminUserId,
            RoleId = adminRoleId
        });
    }


}
