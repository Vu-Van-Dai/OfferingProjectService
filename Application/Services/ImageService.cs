using Application.Interfaces;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
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

        public ImageService(IWebHostEnvironment webHostEnvironment, ILogger<ImageService> logger)
        {
            _webHostEnvironment = webHostEnvironment;
            _logger = logger;
        }

        public async Task<string?> SaveImageAsync(IFormFile? imageFile, string subFolder)
        {
            if (imageFile == null || imageFile.Length == 0)
            {
                _logger.LogWarning("Không có file ảnh nào được cung cấp để lưu.");
                return null; // Hoặc trả về URL ảnh mặc định nếu bạn muốn
            }

            // ✅ SỬA LỖI #16: Thêm Validation
            if (string.IsNullOrWhiteSpace(imageFile.FileName))
            {
                throw new ArgumentException("Tên file không hợp lệ.");
            }

            var allowedExtensions = new[] { ".jpg", ".jpeg", ".png", ".gif", ".webp" };
            var extension = Path.GetExtension(imageFile.FileName).ToLowerInvariant();

            if (string.IsNullOrEmpty(extension) || !allowedExtensions.Contains(extension))
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

            // Xác định base path - ưu tiên WebRootPath, fallback sang ContentRootPath/wwwroot
            string basePath;
            if (!string.IsNullOrWhiteSpace(_webHostEnvironment.WebRootPath))
            {
                basePath = _webHostEnvironment.WebRootPath;
            }
            else
            {
                // Fallback: tạo wwwroot trong ContentRootPath nếu chưa có
                basePath = Path.Combine(_webHostEnvironment.ContentRootPath, "wwwroot");
                if (!Directory.Exists(basePath))
                {
                    Directory.CreateDirectory(basePath);
                    _logger.LogInformation("Đã tạo thư mục wwwroot tại: {BasePath}", basePath);
                }
            }

            // Đường dẫn tuyệt đối đến thư mục lưu ảnh (ví dụ: C:\project\wwwroot\images\products)
            var uploadsFolder = Path.Combine(basePath, "images", subFolder);

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

        /// <summary>
        /// Chuyển đổi file ảnh thành base64 string để lưu vào database.
        /// Không tạo file trên server, chỉ convert và trả về string.
        /// </summary>
        public async Task<string?> ConvertToBase64Async(IFormFile? imageFile)
        {
            if (imageFile == null || imageFile.Length == 0)
            {
                _logger.LogWarning("Không có file ảnh nào được cung cấp để convert.");
                return null;
            }

            // Validation
            if (string.IsNullOrWhiteSpace(imageFile.FileName))
            {
                throw new ArgumentException("Tên file không hợp lệ.");
            }

            var allowedExtensions = new[] { ".jpg", ".jpeg", ".png", ".gif", ".webp" };
            var extension = Path.GetExtension(imageFile.FileName).ToLowerInvariant();

            if (string.IsNullOrEmpty(extension) || !allowedExtensions.Contains(extension))
            {
                throw new ArgumentException($"File extension '{extension}' không được phép. " +
                    $"Chỉ chấp nhận: {string.Join(", ", allowedExtensions)}");
            }

            const long maxFileSize = 5 * 1024 * 1024; // 5MB
            if (imageFile.Length > maxFileSize)
            {
                throw new ArgumentException($"File quá lớn. Tối đa: {maxFileSize / 1024 / 1024}MB");
            }

            try
            {
                // Đọc file vào memory stream
                using (var memoryStream = new MemoryStream())
                {
                    await imageFile.CopyToAsync(memoryStream);
                    var imageBytes = memoryStream.ToArray();

                    // Xác định MIME type dựa trên extension
                    var mimeType = extension switch
                    {
                        ".jpg" or ".jpeg" => "image/jpeg",
                        ".png" => "image/png",
                        ".gif" => "image/gif",
                        ".webp" => "image/webp",
                        _ => "image/jpeg"
                    };

                    // Convert sang base64
                    var base64String = Convert.ToBase64String(imageBytes);
                    var dataUrl = $"data:{mimeType};base64,{base64String}";

                    _logger.LogInformation("Đã convert ảnh thành base64 thành công. Size: {Size} bytes", imageBytes.Length);
                    return dataUrl;
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Lỗi khi convert file ảnh sang base64: {FileName}", imageFile.FileName);
                return null;
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

            // Xác định base path - ưu tiên WebRootPath, fallback sang ContentRootPath/wwwroot
            string basePath;
            if (!string.IsNullOrWhiteSpace(_webHostEnvironment.WebRootPath))
            {
                basePath = _webHostEnvironment.WebRootPath;
            }
            else
            {
                basePath = Path.Combine(_webHostEnvironment.ContentRootPath, "wwwroot");
                if (!Directory.Exists(basePath))
                {
                    // Không có wwwroot thì không thể xóa
                    _logger.LogWarning("WebRootPath is null and wwwroot folder not found. Cannot delete image: {ImagePath}", imagePath);
                    return;
                }
            }

            // Chuyển đường dẫn tương đối thành đường dẫn tuyệt đối
            var fullPath = Path.Combine(basePath, imagePath.TrimStart('/'));

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
    }
}