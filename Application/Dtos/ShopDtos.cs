using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;

namespace Application.Dtos
{
    public class CreateShopDto
    {
        public string ShopName { get; set; }
        public string? Description { get; set; }
    }

    public class GrantShopRoleDto
    {
        public string UserEmail { get; set; }
        public string ShopName { get; set; }
    }
    public class ShopProfileResponseDto
    {
        public int Id { get; set; }
        public string Name { get; set; } = string.Empty;
        public string? Description { get; set; }
        public string? AvatarBase64 { get; set; } // Ảnh dạng Base64
        public string? ContactPhoneNumber { get; set; }
        public DateTime JoinDate { get; set; }
    }

    public class UpdateShopProfileDto
    {
        public string Name { get; set; }
        public string? Description { get; set; }
        public string? ContactPhoneNumber { get; set; }
        public IFormFile? AvatarFile { get; set; }

        // Input cho đường dẫn ảnh (URL hoặc Base64 string) -> BỔ SUNG DÒNG NÀY
        public string? AvatarUrl { get; set; }
    }
}
