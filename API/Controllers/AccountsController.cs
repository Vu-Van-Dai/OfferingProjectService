using API.Services;
using Application.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace API.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class AccountsController : ControllerBase
    {
        private readonly IAccountService _accountService;
        private readonly TokenService _tokenService;

        // Tiêm các service cần thiết vào constructor
        public AccountsController(IAccountService accountService, TokenService tokenService)
        {
            _accountService = accountService;
            _tokenService = tokenService;
        }

        // Endpoint: POST /api/accounts/register
        [HttpPost("register")]
        public async Task<IActionResult> Register(RegisterDto registerDto)
        {
            var user = await _accountService.RegisterAsync(registerDto.FullName, registerDto.Email, registerDto.Password);

            if (user == null)
            {
                // AccountService trả về null nghĩa là email đã tồn tại
                return BadRequest(new { message = "Email already exists." });
            }

            return Ok(new { message = "Registration successful." });
        }

        // Endpoint: POST /api/accounts/login
        [HttpPost("login")]
        public async Task<IActionResult> Login(LoginDto loginDto)
        {
            var user = await _accountService.ValidateCredentialsAsync(loginDto.Email, loginDto.Password);

            if (user == null)
            {
                // AccountService trả về null nghĩa là email hoặc mật khẩu không hợp lệ
                return Unauthorized(new { message = "Invalid email or password." });
            }

            // Nếu thông tin hợp lệ, tạo token
            var token = _tokenService.GenerateToken(user);

            // Trả về token cho client
            return Ok(new { token = token });
        }
        // POST: api/accounts/change-password
        [HttpPost("changepassword")]
        [Authorize]
        public async Task<IActionResult> ChangePassword([FromBody] ChangePasswordDto passwordDto)
        {
            var userId = Guid.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier));
            var result = await _accountService.ChangePasswordAsync(userId, passwordDto.OldPassword, passwordDto.NewPassword);

            if (!result) return BadRequest(new { message = "Mật khẩu cũ không đúng hoặc đã có lỗi xảy ra." });

            return Ok(new { message = "Đổi mật khẩu thành công." });
        }

        // DELETE: api/accounts/delete-me
        [HttpDelete("deleteme")]
        [Authorize]
        public async Task<IActionResult> DeleteAccount([FromBody] DeleteAccountDto deleteDto)
        {
            var userId = Guid.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier));
            var result = await _accountService.DeleteAccountAsync(userId, deleteDto.Password);

            if (!result) return BadRequest(new { message = "Mật khẩu không đúng hoặc đã có lỗi xảy ra." });

            return Ok(new { message = "Tài khoản đã được xóa." });
        }
    }

    // --- DTOs (Data Transfer Objects) ---
    // Các lớp đơn giản chỉ dùng để truyền dữ liệu từ request
    public class RegisterDto
    {
        public string FullName { get; set; }
        public string Email { get; set; }
        public string Password { get; set; }
    }

    public class LoginDto
    {
        public string Email { get; set; }
        public string Password { get; set; }
    }

    public class ChangePasswordDto
    {
        public string OldPassword { get; set; }
        public string NewPassword { get; set; }
    }

    public class DeleteAccountDto
    {
        public string Password { get; set; }
    }
}
