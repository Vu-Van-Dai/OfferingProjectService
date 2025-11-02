using API.Services;
using Application.Interfaces;
using Application.Services;
using Core.Entities;
using Infrastructure.Data;
using Infrastructure.Repositorie;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Mvc.Infrastructure;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using System.Text;
using HealthChecks.UI.Client;
using Microsoft.AspNetCore.Diagnostics.HealthChecks;

var builder = WebApplication.CreateBuilder(args);

// 1. Dịch vụ của Infrastructure Layer (Truy cập dữ liệu)
builder.Services.AddDbContext<ApplicationDbContext>(options =>
{
    options.UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnection"));
});
builder.Services.AddScoped<IUserRepository, UserRepository>();
builder.Services.AddScoped<IAccountService, AccountService>();
builder.Services.AddScoped<IProfileService, ProfileService>();
builder.Services.AddScoped<ICategoryRepository, CategoryRepository>();
builder.Services.AddScoped<IProductRepository, ProductRepository>();
builder.Services.AddScoped<ICategoryService, CategoryService>();
builder.Services.AddScoped<IProductService, ProductService>();
builder.Services.AddScoped<IShopRepository, ShopRepository>();
builder.Services.AddScoped<IReviewRepository, ReviewRepository>();
builder.Services.AddScoped<IReviewService, ReviewService>();
builder.Services.AddScoped<ISearchService, SearchService>();
builder.Services.AddScoped<ICartRepository, CartRepository>();
builder.Services.AddScoped<ICartService, CartService>();
builder.Services.AddScoped<IOrderRepository, OrderRepository>();
builder.Services.AddScoped<IOrderService, OrderService>();
builder.Services.AddScoped<IUserAddressRepository, UserAddressRepository>();
builder.Services.AddScoped<IUserAddressService, UserAddressService>();
builder.Services.AddScoped<IOrderQueryRepository, OrderQueryRepository>();
builder.Services.AddScoped<IImageService, ImageService>();
builder.Services.AddScoped<IShopProfileService, ShopProfileService>();
builder.Services.AddScoped<IDashboardService, DashboardService>();
builder.Services.AddScoped<IOrderItemService, OrderItemService>();
builder.Services.AddScoped<IReviewQueryService, ReviewQueryService>();
builder.Services.AddScoped<IStatisticsService, StatisticsService>();
builder.Services.AddScoped<IOrderItemRepository, OrderItemRepository>();
builder.Services.AddScoped<IActivityLogRepository, ActivityLogRepository>();
builder.Services.AddScoped<IAppSettingRepository, AppSettingRepository>();
builder.Services.AddScoped<IAdminShopService, AdminShopService>();
builder.Services.AddScoped<IAdminProductService, AdminProductService>();
builder.Services.AddScoped<IActivityLogService, ActivityLogService>();
builder.Services.AddScoped<IAdminCategoryService, AdminCategoryService>();
builder.Services.AddScoped<IAdminDashboardService, AdminDashboardService>();
builder.Services.AddScoped<IAdminConfigService, AdminConfigService>();

// Add services to the container.
builder.Services.AddScoped<TokenService>();
builder.Services.AddControllers();

builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowLocalVite", policy =>
    {
        policy.WithOrigins("http://localhost:8080",
                "https://localhost:8080",  // Add HTTPS support
                "http://localhost:8081",
                "https://localhost:8081",  // Add HTTPS support
                "http://localhost:3000",
                "https://localhost:3000")  // Add HTTPS support)
              .AllowAnyMethod()
              .AllowAnyHeader();
    });
});

builder.Services.AddAuthentication(options =>
{
    options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
    options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
})
.AddJwtBearer(options =>
{
    options.TokenValidationParameters = new TokenValidationParameters
    {
        ValidateIssuer = true, // Kiểm tra người phát hành
        ValidateAudience = true, // Kiểm tra người nhận
        ValidateLifetime = true, // Kiểm tra token còn hạn hay không
        ValidateIssuerSigningKey = true, // Quan trọng: Kiểm tra chữ ký bí mật
        ValidIssuer = builder.Configuration["Jwt:Issuer"],
        ValidAudience = builder.Configuration["Jwt:Audience"],
        IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(builder.Configuration["Jwt:Key"]))
    };
});

// === KẾT THÚC CẤU HÌNH AUTHENTICATION ===

// Đăng ký TokenService (sẽ tạo ở bước 4)


// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

builder.Services.AddHealthChecks()
    // Thêm một kiểm tra cho kết nối Database
    .AddDbContextCheck<ApplicationDbContext>();

var app = builder.Build();

using (var scope = app.Services.CreateScope())
{
    var services = scope.ServiceProvider;
    var logger = services.GetRequiredService<ILogger<Program>>();
    try
    {
        var context = services.GetRequiredService<ApplicationDbContext>();
        await context.Database.MigrateAsync();

        var adminEmail = "admin@worship.com";
        if (!await context.AppUsers.AnyAsync(u => u.Email == adminEmail))
        {
            var adminUser = new AppUser
            {
                Id = Guid.NewGuid(),
                FullName = "Administrator",
                Email = adminEmail,
                PasswordHash = BCrypt.Net.BCrypt.HashPassword("Admin@123!"),
                Roles = new List<string> { "Admin" }
            };
            await context.AppUsers.AddAsync(adminUser);
            await context.SaveChangesAsync();
            logger.LogInformation("Đã tạo tài khoản Admin mặc định.");
        }
        await Infrastructure.Data.DataSeeder.SeedShopAndProductsAsync(services, logger);
    }
    catch (Exception ex)
    {
        logger.LogError(ex, "Đã xảy ra lỗi khi khởi tạo dữ liệu.");
    }
}

if (app.Environment.IsDevelopment())
{
    var logger = app.Services.GetRequiredService<ILogger<Program>>();
    var endpoints = app.Services.GetRequiredService<IActionDescriptorCollectionProvider>();

    // Lấy URL https mà ứng dụng đang chạy
    var launchUrl = app.Urls.FirstOrDefault(url => url.StartsWith("https://")) ?? "https://localhost:5001";

    logger.LogInformation("==================================================");
    logger.LogInformation("                    Endpoints                     ");
    logger.LogInformation("==================================================");

    foreach (var endpoint in endpoints.ActionDescriptors.Items.OfType<Microsoft.AspNetCore.Mvc.Controllers.ControllerActionDescriptor>())
    {
        var controller = endpoint.ControllerName;
        var action = endpoint.ActionName;
        // Lấy template route từ attribute, ví dụ: "api/[controller]"
        var routeTemplate = endpoint.AttributeRouteInfo?.Template;

        if (routeTemplate != null)
        {
            var fullRoute = routeTemplate.Replace("[controller]", controller).Replace("[action]", action);
            var httpMethods = string.Join(", ", endpoint.EndpointMetadata.OfType<HttpMethodMetadata>().SelectMany(m => m.HttpMethods));

            if (!string.IsNullOrEmpty(httpMethods))
            {
                logger.LogInformation($"[{httpMethods}] {launchUrl}/{fullRoute}");
            }
        }
    }
    logger.LogInformation("==================================================");
}
// === KẾT THÚC KHỐI CODE ===


// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();

app.UseStaticFiles();

app.UseCors("AllowLocalVite");

app.UseAuthentication();
app.UseAuthorization();

app.MapControllers();

app.MapHealthChecks("/api/health", new HealthCheckOptions
{
    // Cấu hình để trả về JSON (frontend dễ đọc hơn)
    Predicate = _ => true,
    ResponseWriter = UIResponseWriter.WriteHealthCheckUIResponse
});

app.Run();
