using Core.Entities;
using Microsoft.EntityFrameworkCore;

namespace Infrastructure.Data
{
    // Kế thừa từ DbContext thông thường
    public class ApplicationDbContext : DbContext
    {
        public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options) : base(options)
        {
        }

        // Thêm DbSet cho AppUser để EF Core tạo bảng
        public DbSet<AppUser> AppUsers { get; set; }
        public DbSet<ProductCategory> ProductCategories { get; set; } // Thêm dòng này
        public DbSet<Product> Products { get; set; }
        public DbSet<Shop> Shops { get; set; }
        public DbSet<ProductReview> ProductReviews { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            // Cấu hình để EF Core biết cách lưu danh sách Roles
            // Nó sẽ lưu dưới dạng một chuỗi duy nhất, phân tách bởi dấu phẩy
            modelBuilder.Entity<AppUser>()
                .Property(e => e.Roles)
                .HasConversion(
                    v => string.Join(',', v),
                    v => v.Split(',', StringSplitOptions.RemoveEmptyEntries).ToList());
            // Cấu hình AppUser 1-1 Shop
            modelBuilder.Entity<AppUser>()
                .HasOne(u => u.Shop)
                .WithOne(s => s.OwnerUser)
                .HasForeignKey<Shop>(s => s.OwnerUserId);

            // Cấu hình Shop 1-n Product
            modelBuilder.Entity<Shop>()
                .HasMany(s => s.Products)
                .WithOne(p => p.Shop)
                .HasForeignKey(p => p.ShopId)
                .OnDelete(DeleteBehavior.Cascade); // Xóa shop thì xóa luôn sản phẩm

            // Cấu hình Product 1-n Review
            modelBuilder.Entity<Product>()
                .HasMany(p => p.Reviews)
                .WithOne(r => r.Product)
                .HasForeignKey(r => r.ProductId);

            // Cấu hình AppUser 1-n Review
            modelBuilder.Entity<AppUser>()
                .HasMany(u => u.Reviews)
                .WithOne(r => r.User)
                .HasForeignKey(r => r.UserId)
                .OnDelete(DeleteBehavior.NoAction); // Không cho xóa User nếu còn Review
        }
    }
}
