using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;

namespace Application.Interfaces
{
    /// <summary>
    /// Interface cho dịch vụ xử lý lưu trữ và xóa ảnh.
    /// </summary>
    public interface IImageService
    {
        // Xử lý file upload
        Task<(byte[] Data, string MimeType)> ProcessImageAsync(IFormFile imageFile);

        // Xử lý chuỗi (Base64 hoặc URL)
        Task<(byte[] Data, string MimeType)?> ProcessStringImageAsync(string imageString);

        string ToBase64(byte[]? data, string? mimeType);
    }
}
