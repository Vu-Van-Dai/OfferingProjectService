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
        }
    }
}
