using Microsoft.AspNetCore.Http;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Application.Dtos
{
    public class UpdateProfileDto
    {
        [Required]
        public string FullName { get; set; } = string.Empty;
        public string? PhoneNumber { get; set; }
        public string? Introduction { get; set; }

        // Dùng cho upload ảnh avatar
        public IFormFile? AvatarFile { get; set; }
    }
}
