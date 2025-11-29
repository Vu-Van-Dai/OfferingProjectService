using Application.Dtos;
using Application.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace API.Controllers
{
    [ApiController]
    [Route("api/useraddresses")]
    [Authorize] // Yêu cầu đăng nhập cho tất cả
    public class UserAddressesController : ControllerBase
    {
        private readonly IUserAddressService _addressService;
        public UserAddressesController(IUserAddressService addressService) { _addressService = addressService; }

        private Guid GetUserId() => Guid.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier));

        // GET /api/useraddresses (Lấy danh sách địa chỉ)
        [HttpGet]
        public async Task<IActionResult> GetMyAddresses()
        {
            return Ok(await _addressService.GetMyAddressesAsync(GetUserId()));
        }

        // GET /api/useraddresses/{id} (Lấy 1 địa chỉ)
        [HttpGet("{id}")]
        public async Task<IActionResult> GetAddressById(int id)
        {
            var address = await _addressService.GetMyAddressByIdAsync(GetUserId(), id);
            if (address == null) return NotFound();
            return Ok(address);
        }

        // POST /api/useraddresses (Thêm địa chỉ mới)
        [HttpPost]
        public async Task<IActionResult> AddAddress([FromBody] UpsertAddressDto addressDto)
        {
            var newAddress = await _addressService.AddAddressAsync(GetUserId(), addressDto);
            return CreatedAtAction(nameof(GetAddressById), new { id = newAddress.Id }, newAddress);
        }

        // PUT /api/useraddresses/{id} (Sửa địa chỉ)
        [HttpPut("{id}")]
        public async Task<IActionResult> UpdateAddress(int id, [FromBody] UpsertAddressDto addressDto)
        {
            var result = await _addressService.UpdateAddressAsync(GetUserId(), id, addressDto);
            if (!result) return NotFound();
            return Ok(new { message = "Cập nhật thành công", success = true });
        }

        // DELETE /api/useraddresses/{id} (Xóa địa chỉ)
        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteAddress(int id)
        {
            var result = await _addressService.DeleteAddressAsync(GetUserId(), id);
            if (!result) return NotFound();
            return Ok(new { message = "Xóa thành công" });
        }

        // POST /api/useraddresses/{id}/set-default (Đặt làm mặc định)
        [HttpPost("{id}/set-default")]
        public async Task<IActionResult> SetDefault(int id)
        {
            var result = await _addressService.SetDefaultAddressAsync(GetUserId(), id);
            if (!result) return NotFound();
            return Ok(new { message = "Đặt địa chỉ mặc định thành công." });
        }
    }
}
