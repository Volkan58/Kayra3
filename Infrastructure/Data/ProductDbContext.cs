using Microsoft.EntityFrameworkCore;
using SharedLayer.Model;

namespace Infrastructure.Data
{
    public class ProductDbContext : DbContext
    {
        public ProductDbContext(DbContextOptions<ProductDbContext> options)
        : base(options)
        {
        }
        public DbSet<Product> Products { get; set; }

        protected override void OnModelCreating(ModelBuilder builder)
        {
            base.OnModelCreating(builder);
            builder.Entity<Product>(entity =>
            {
                entity.HasKey(p => p.Id);
                entity.Property(p => p.Name).IsRequired().HasMaxLength(200);
                entity.Property(p => p.Description).HasMaxLength(1000);
                entity.Property(p => p.Price).IsRequired().HasPrecision(18, 2);
                entity.Property(p => p.StockQuantity).IsRequired();
                entity.Property(p => p.CreatedAt).IsRequired();
                entity.Property(p => p.IsActive).IsRequired();
                entity.HasIndex(p => p.Name);
                entity.HasIndex(p => p.CreatedAt);
            });

        }
    }
}
