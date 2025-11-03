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
            var userId = Guid.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier));
            var result = await _profileService.UpdateProfileAsync(userId, profileDto);

            if (!result) return NotFound();

            return Ok(new { message = "Cập nhật thông tin thành công." });
        }
    }
}
