using Application.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;
using Application.Dtos;

namespace API.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    [Authorize] // Tất cả các API trong đây đều yêu cầu đăng nhập
    public class ProfileController : ControllerBase
    {
        private readonly IProfileService _profileService;

        public ProfileController(IProfileService profileService)
        {
            _profileService = profileService;
        }

        // GET: api/profile/me
        [HttpGet("me")]
        public async Task<IActionResult> GetMyProfile()
        {
            var userId = Guid.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier));
            var user = await _profileService.GetProfileAsync(userId);

            if (user == null) return NotFound();

            // Trả về một đối tượng không chứa mật khẩu
            return Ok(new
            {
                user.FullName,
                user.Email,
                user.PhoneNumber,
                user.Introduction,
                user.AvatarUrl
            });
        }

        // PUT: api/profile/me
        [HttpPut("me")]
        public async Task<IActionResult> UpdateMyProfile([FromForm] UpdateProfileDto profileDto)
        {
            try
            {
                // Validate model state
                if (!ModelState.IsValid)
                {
                    return BadRequest(new { message = "Dữ liệu không hợp lệ.", errors = ModelState });
                }

                var userId = Guid.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier));
                var result = await _profileService.UpdateProfileAsync(userId, profileDto);

                if (!result) return NotFound(new { message = "Không tìm thấy người dùng." });

                return Ok(new { message = "Cập nhật thông tin thành công." });
            }
            catch (ArgumentException ex)
            {
                // Lỗi validation file (extension, size)
                return BadRequest(new { message = ex.Message });
            }
            catch (Exception ex)
            {
                // Log error và trả về 500 với message
                return StatusCode(500, new { message = "Lỗi server khi cập nhật profile.", error = ex.Message });
            }
        }
    }
}
