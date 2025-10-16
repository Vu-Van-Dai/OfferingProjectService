using API.Services;
using Application.Interfaces;
using Microsoft.AspNetCore.Mvc;

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
}
