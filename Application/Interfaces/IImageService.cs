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
        /// <summary>
        /// Lưu file ảnh vào nơi lưu trữ (ví dụ: local wwwroot hoặc cloud storage).
        /// </summary>
        /// <param name="imageFile">File ảnh được upload.</param>
        /// <param name="subFolder">Thư mục con để lưu ảnh (ví dụ: "products", "shops").</param>
        /// <returns>Đường dẫn tương đối (hoặc URL đầy đủ) của ảnh đã lưu.</returns>
        Task<string?> SaveImageAsync(IFormFile? imageFile, string subFolder);

        /// <summary>
        /// Chuyển đổi file ảnh thành base64 string để lưu vào database.
        /// </summary>
        /// <param name="imageFile">File ảnh được upload.</param>
        /// <returns>Base64 string (data:image/xxx;base64,...) hoặc null nếu có lỗi.</returns>
        Task<string?> ConvertToBase64Async(IFormFile? imageFile);

        /// <summary>
        /// Xóa file ảnh dựa trên đường dẫn đã lưu.
        /// </summary>
        /// <param name="imagePath">Đường dẫn tương đối (hoặc URL) của ảnh cần xóa.</param>
        void DeleteImage(string? imagePath);
    }
}
