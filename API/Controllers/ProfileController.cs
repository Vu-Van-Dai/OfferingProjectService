using Application.Dtos;
using Application.Interfaces;
using Application.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace API.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    [Authorize] // Tất cả các API trong đây đều yêu cầu đăng nhập
    public class ProfileController : ControllerBase
    {
        private readonly IProfileService _profileService;
        private readonly IImageService _imageService;

        public ProfileController(IProfileService profileService, IImageService imageService)
        {
            _profileService = profileService;
            _imageService = imageService;
        }

        // GET: api/profile/me
        [HttpGet("me")]
        public async Task<IActionResult> GetMyProfile()
        {
            var userId = Guid.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier));
            var user = await _profileService.GetProfileAsync(userId);

            if (user == null) return NotFound();

            var avatarUrl = _imageService.ToBase64(user.AvatarData, user.AvatarMimeType);

            // Trả về một đối tượng không chứa mật khẩu
            return Ok(new
            {
                user.FullName,
                user.Email,
                user.PhoneNumber,
                user.Introduction,
                AvatarUrl = avatarUrl
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
