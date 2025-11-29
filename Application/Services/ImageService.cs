using Application.Interfaces;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace Application.Services
{
    /// <summary>
    /// Triển khai ImageService đơn giản, lưu ảnh vào thư mục wwwroot.
    /// </summary>
    public class ImageService : IImageService
    {
        private readonly IWebHostEnvironment _webHostEnvironment;
        private readonly ILogger<ImageService> _logger;
        private readonly string[] _allowedExtensions = { ".jpg", ".jpeg", ".png", ".gif", ".webp" };
        private readonly HttpClient _httpClient; // Dùng để tải ảnh từ URL
        private const long _maxFileSize = 5 * 1024 * 1024; // 5MB


        public ImageService(IWebHostEnvironment webHostEnvironment, ILogger<ImageService> logger, HttpClient httpClient)
        {
            _webHostEnvironment = webHostEnvironment;
            _logger = logger;
            _httpClient = httpClient;
        }

        public async Task<(byte[] Data, string MimeType)?> ProcessStringImageAsync(string imageString)
        {
            if (string.IsNullOrWhiteSpace(imageString)) return null;

            // TRƯỜNG HỢP 1: Xử lý Base64 Data URI (data:image/jpeg;base64,...)
            var base64Pattern = new Regex(@"^data:(image\/[a-zA-Z]+);base64,(.+)$");
            var match = base64Pattern.Match(imageString);

            if (match.Success)
            {
                var mimeType = match.Groups[1].Value; // vd: image/jpeg
                var base64Data = match.Groups[2].Value;
                try
                {
                    var bytes = Convert.FromBase64String(base64Data);
                    return (bytes, mimeType);
                }
                catch { return null; } // Lỗi format base64
            }

            // TRƯỜNG HỢP 2: Xử lý URL Online (https://...)
            if (Uri.TryCreate(imageString, UriKind.Absolute, out var uriResult)
                && (uriResult.Scheme == Uri.UriSchemeHttp || uriResult.Scheme == Uri.UriSchemeHttps))
            {
                try
                {
                    // Tải ảnh về
                    var response = await _httpClient.GetAsync(uriResult);
                    if (response.IsSuccessStatusCode)
                    {
                        var contentType = response.Content.Headers.ContentType?.MediaType ?? "image/jpeg";
                        var bytes = await response.Content.ReadAsByteArrayAsync();
                        return (bytes, contentType);
                    }
                }
                catch { return null; } // Lỗi mạng hoặc link chết
            }

            return null;
        }
        public async Task<(byte[] Data, string MimeType)> ProcessImageAsync(IFormFile imageFile)
        {
            if (imageFile == null || imageFile.Length == 0)
                throw new ArgumentException("File không hợp lệ.");

            if (imageFile.Length > _maxFileSize)
                throw new ArgumentException("Dung lượng file quá lớn (Max 5MB).");

            var extension = Path.GetExtension(imageFile.FileName).ToLowerInvariant();
            if (!_allowedExtensions.Contains(extension))
                throw new ArgumentException("Định dạng file không được hỗ trợ.");

            using var memoryStream = new MemoryStream();
            await imageFile.CopyToAsync(memoryStream);

            return (memoryStream.ToArray(), imageFile.ContentType);
        }

        public string ToBase64(byte[]? data, string? mimeType)
        {
            if (data == null || data.Length == 0) return string.Empty;
            // Nếu thiếu mimeType trong DB cũ, mặc định là image/jpeg
            var mime = string.IsNullOrEmpty(mimeType) ? "image/jpeg" : mimeType;
            return $"data:{mime};base64,{Convert.ToBase64String(data)}";
        }
    }
}
