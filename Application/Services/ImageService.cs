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
            var uniqueFileName = Guid.NewGuid().ToString() + Path.GetExtension(imageFile.FileName);
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
    }
}
