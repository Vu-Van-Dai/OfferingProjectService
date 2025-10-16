using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace API.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class WorshipPackagesController : ControllerBase
    {
        // === CÔNG KHAI: AI CŨNG XEM ĐƯỢC ===
        // Endpoint này không có attribute, vì vậy nó là công khai.
        // Khách vãng lai (guest không đăng nhập) vẫn xem được danh sách gói cúng.
        // Endpoint: GET /api/worshippackages
        [HttpGet]
        public IActionResult GetPublicPackages()
        {
            // Logic để lấy các gói cúng từ database...
            return Ok("Đây là danh sách các gói cúng (công khai cho mọi người).");
        }

        // === YÊU CẦU ĐĂNG NHẬP ===
        // Chỉ những ai có token hợp lệ (đã đăng nhập) mới xem được chi tiết.
        // Áp dụng cho cả người dùng có vai trò "Guest" và "Admin".
        // Endpoint: GET /api/worshippackages/details/1
        [HttpGet("details/{id}")]
        [Authorize]
        public IActionResult GetPackageDetails(int id)
        {
            var userEmail = User.FindFirstValue(ClaimTypes.Email);
            // Logic để lấy chi tiết gói cúng cho người dùng đã đăng nhập...
            return Ok($"Người dùng '{userEmail}' đang xem chi tiết gói cúng số {id}.");
        }

        // === CHỈ ADMIN MỚI ĐƯỢC VÀO ===
        // Yêu cầu phải có token hợp lệ VÀ token đó phải chứa claim role="Admin".
        // Endpoint: POST /api/worshippackages
        [HttpPost]
        [Authorize(Roles = "Admin")]
        public IActionResult CreatePackage([FromBody] string packageName)
        {
            var adminEmail = User.FindFirstValue(ClaimTypes.Email);
            // Logic để tạo gói cúng mới trong database...
            return Ok($"Admin '{adminEmail}' đã tạo thành công gói cúng: '{packageName}'.");
        }
        [HttpGet("health")]
        public IActionResult HealthCheck()
        {
            return Ok(new { status = "Healthy" });
        }
    }
}
