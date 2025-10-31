using Core.Entities;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;

namespace API.Services
{
    public class TokenService
    {
        private readonly IConfiguration _config;
        public TokenService(IConfiguration config)
        {
            _config = config;
        }

        public string GenerateToken(AppUser user)
        {
            // 1. Tạo danh sách các "thông tin" (claims) sẽ chứa trong token
            var claims = new List<Claim>
            {
                new Claim(ClaimTypes.Email, user.Email),
                new Claim(ClaimTypes.NameIdentifier, user.Id.ToString())
            };

            // Thêm các vai trò của người dùng vào claims
            foreach (var role in user.Roles)
            {
                claims.Add(new Claim(ClaimTypes.Role, role));
            }

            if (user.Roles.Contains("Shop") && user.ShopId.HasValue)
            {
                claims.Add(new Claim("ShopId", user.ShopId.Value.ToString()));
            }

            // 2. Lấy khóa bí mật từ appsettings.json
            var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_config["Jwt:Key"]));
            // 3. Tạo thông tin chữ ký
            var creds = new SigningCredentials(key, SecurityAlgorithms.HmacSha256Signature);

            // 4. Tạo token object chứa đầy đủ thông tin
            var tokenDescriptor = new SecurityTokenDescriptor
            {
                Subject = new ClaimsIdentity(claims),
                Expires = DateTime.Now.AddDays(7), // Token hết hạn sau 7 ngày
                SigningCredentials = creds,
                Issuer = _config["Jwt:Issuer"],
                Audience = _config["Jwt:Audience"]
            };

            // 5. Dùng handler để tạo ra chuỗi token cuối cùng
            var tokenHandler = new JwtSecurityTokenHandler();
            var token = tokenHandler.CreateToken(tokenDescriptor);
            return tokenHandler.WriteToken(token);
        }
    }
}
