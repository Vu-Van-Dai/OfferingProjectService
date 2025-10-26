using Application.Dtos;
using Application.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace API.Controllers
{
    [ApiController]
    [Route("api/admin")]
    [Authorize(Roles = "Admin")]
    public class AdminController : ControllerBase
    {
        private readonly IShopService _shopService;
        private readonly IUserRepository _userRepository;

        public AdminController(IShopService shopService, IUserRepository userRepository)
        {
            _shopService = shopService;
            _userRepository = userRepository;
        }

        // POST: /api/admin/grant-shop-role
        [HttpPost("grantshoprole")]
        public async Task<IActionResult> GrantShopRole([FromBody] GrantShopRoleDto dto)
        {
            var user = await _userRepository.GetByEmailAsync(dto.UserEmail);
            if (user == null) return NotFound("Không tìm thấy người dùng.");

            try
            {
                var shop = await _shopService.CreateShopAsync(user.Id, new CreateShopDto { ShopName = dto.ShopName });
                return Ok(new { message = "Tạo shop và cấp quyền thành công.", shopId = shop.Id });
            }
            catch (Exception ex)
            {
                return BadRequest(new { message = ex.Message });
            }
        }
    }
}
