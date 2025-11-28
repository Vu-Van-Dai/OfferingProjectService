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

        public async Task<string?> SaveImageAsync(IFormFile? imageFile, string subFolder)
        {
            if (imageFile == null || imageFile.Length == 0)
            {
                _logger.LogWarning("Không có file ảnh nào được cung cấp để lưu.");
                return null; // Hoặc trả về URL ảnh mặc định nếu bạn muốn
            }

            // ✅ SỬA LỖI #16: Thêm Validation
            var allowedExtensions = new[] { ".jpg", ".jpeg", ".png", ".gif", ".webp" };
            var extension = Path.GetExtension(imageFile.FileName).ToLowerInvariant();

            if (!allowedExtensions.Contains(extension))
            {
                throw new ArgumentException($"File extension '{extension}' không được phép. " +
                    $"Chỉ chấp nhận: {string.Join(", ", allowedExtensions)}");
            }

            const long maxFileSize = 5 * 1024 * 1024; // 5MB
            if (imageFile.Length > maxFileSize)
            {
                throw new ArgumentException($"File quá lớn. Tối đa: {maxFileSize / 1024 / 1024}MB");
            }
            // (Kết thúc Lỗi #16)

            // Đường dẫn tuyệt đối đến thư mục lưu ảnh (ví dụ: C:\project\wwwroot\images\products)
            var uploadsFolder = Path.Combine(_webHostEnvironment.WebRootPath, "images", subFolder);

            // Tạo thư mục nếu chưa tồn tại
            if (!Directory.Exists(uploadsFolder))
            {
                try
                {
                    Directory.CreateDirectory(uploadsFolder);
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Không thể tạo thư mục lưu ảnh: {FolderPath}", uploadsFolder);
                    return null; // Không thể lưu
                }
            }

            // Tạo tên file duy nhất để tránh trùng lặp
            // ✅ SỬA LỖI #16: Dùng 'extension' đã được validate
            var uniqueFileName = Guid.NewGuid().ToString() + extension;
            var filePath = Path.Combine(uploadsFolder, uniqueFileName);

            try
            {
                // Lưu file ảnh vào đường dẫn đã tạo
                using (var fileStream = new FileStream(filePath, FileMode.Create))
                {
                    await imageFile.CopyToAsync(fileStream);
                }

                // Trả về đường dẫn tương đối để lưu vào database (ví dụ: /images/products/xxxxx.jpg)
                var relativePath = $"/images/{subFolder}/{uniqueFileName}";
                _logger.LogInformation("Đã lưu ảnh thành công: {ImagePath}", relativePath);
                return relativePath;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Lỗi khi lưu file ảnh: {FileName}", imageFile.FileName);
                return null; // Không thể lưu
            }
        }

        public void DeleteImage(string? imagePath)
        {
            if (string.IsNullOrWhiteSpace(imagePath))
            {
                return; // Không có gì để xóa
            }

            // Chỉ xóa file nếu đó là file lưu trữ cục bộ (bắt đầu bằng /images/)
            if (!imagePath.StartsWith("/images/"))
            {
                _logger.LogWarning("Bỏ qua việc xóa ảnh không phải file cục bộ: {ImagePath}", imagePath);
                return;
            }

            // Chuyển đường dẫn tương đối thành đường dẫn tuyệt đối
            var fullPath = Path.Combine(_webHostEnvironment.WebRootPath, imagePath.TrimStart('/'));

            if (File.Exists(fullPath))
            {
                try
                {
                    File.Delete(fullPath);
                    _logger.LogInformation("Đã xóa file ảnh: {ImagePath}", imagePath);
                }
                catch (Exception ex)
                {
                    // Ghi log lỗi nhưng không dừng chương trình
                    _logger.LogError(ex, "Lỗi khi xóa file ảnh: {ImagePath}", imagePath);
                }
            }
            else
            {
                _logger.LogWarning("Không tìm thấy file ảnh để xóa: {ImagePath}", imagePath);
            }
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
