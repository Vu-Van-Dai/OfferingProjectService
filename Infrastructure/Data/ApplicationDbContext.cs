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
        public DbSet<Cart> Carts { get; set; }
        public DbSet<CartItem> CartItems { get; set; }
        public DbSet<Order> Orders { get; set; }
        public DbSet<OrderItem> OrderItems { get; set; }
        public DbSet<UserAddress> UserAddresses { get; set; }

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
            // Cấu hình AppUser 1-1 Cart (Bắt buộc)
            modelBuilder.Entity<AppUser>()
                .HasOne(u => u.Cart)
                .WithOne(c => c.OwnerUser)
                .HasForeignKey<Cart>(c => c.OwnerUserId);

            // Cấu hình Cart 1-n CartItem
            modelBuilder.Entity<Cart>()
                .HasMany(c => c.Items)
                .WithOne(ci => ci.Cart)
                .HasForeignKey(ci => ci.CartId);

            // Cấu hình CartItem n-1 Product
            modelBuilder.Entity<CartItem>()
                .HasOne(ci => ci.Product)
                .WithMany()
                .HasForeignKey(ci => ci.ProductId)
                .OnDelete(DeleteBehavior.NoAction);

            // Cấu hình CartItem n-1 Shop
            modelBuilder.Entity<CartItem>()
                .HasOne(ci => ci.Shop)
                .WithMany()
                .HasForeignKey(ci => ci.ShopId)
                .OnDelete(DeleteBehavior.NoAction);
            modelBuilder.Entity<Order>().OwnsOne(o => o.ShippingAddress, sa =>
            {
                sa.WithOwner(); // Chỉ định đây là owned type
            });

            // Cấu hình AppUser 1-n Order
            modelBuilder.Entity<AppUser>()
                .HasMany<Order>() // Một User có nhiều Order
                .WithOne(o => o.Buyer)
                .HasForeignKey(o => o.BuyerUserId);

            // Cấu hình Order 1-n OrderItem
            modelBuilder.Entity<Order>()
                .HasMany(o => o.Items)
                .WithOne(oi => oi.Order)
                .HasForeignKey(oi => oi.OrderId)
                .OnDelete(DeleteBehavior.Cascade); // Xóa Order thì xóa luôn Item

            // Cấu hình OrderItem n-1 Product
            modelBuilder.Entity<OrderItem>()
                .HasOne(oi => oi.ProductOrdered)
                .WithMany() // Product không cần biết nó nằm trong bao nhiêu Order
                .HasForeignKey(oi => oi.ProductId)
                .OnDelete(DeleteBehavior.Restrict); // Không cho xóa Product nếu còn OrderItem

            // Cấu hình OrderItem n-1 Shop
            modelBuilder.Entity<OrderItem>()
                .HasOne(oi => oi.Shop)
                .WithMany() // Shop không cần biết
                .HasForeignKey(oi => oi.ShopId)
                .OnDelete(DeleteBehavior.Restrict); // Không cho xóa Shop nếu còn OrderItem
            modelBuilder.Entity<AppUser>()
                .HasMany(u => u.Addresses) // Một User có nhiều Address
                .WithOne(a => a.User) // Một Address thuộc về một User
                .HasForeignKey(a => a.UserId)
                .OnDelete(DeleteBehavior.Cascade); // Xóa User thì xóa luôn Address

            // Đảm bảo mỗi User chỉ có tối đa 1 địa chỉ mặc định (IsDefault = true)
            modelBuilder.Entity<UserAddress>()
                .HasIndex(a => new { a.UserId, a.IsDefault })
                .HasFilter("[IsDefault] = 1") // Chỉ áp dụng index cho các bản ghi có IsDefault = true
                .IsUnique(); // Đảm bảo tính duy nhất
            modelBuilder.Entity<Product>()
                .HasMany(p => p.Images) // Một Product có nhiều Image
                .WithOne(i => i.Product) // Một Image thuộc về một Product
                .HasForeignKey(i => i.ProductId)
                .OnDelete(DeleteBehavior.Cascade); // Xóa Product thì xóa luôn các ảnh liên quan
        }
    }
}
