using Application.Dtos;
using Application.Interfaces;
using Core.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Application.Services
{
    public class ReviewService : IReviewService
    {
        private readonly IReviewRepository _reviewRepository;
        private readonly IUserRepository _userRepository;
        // ✅ SỬA LỖI #13: Thêm ProductRepository
        private readonly IProductRepository _productRepository;

        public ReviewService(IReviewRepository reviewRepository, IUserRepository userRepository, IProductRepository productRepository) // Thêm vào constructor
        {
            _reviewRepository = reviewRepository;
            _userRepository = userRepository;
            _productRepository = productRepository; // Thêm
        }
        public async Task<IEnumerable<ProductReview>> GetReviewsForProductAsync(int productId)
        {
            return await _reviewRepository.GetByProductIdAsync(productId);
        }
        public async Task<ProductReview> AddReviewAsync(Guid userId, int productId, CreateReviewDto reviewDto)
        {
            var user = await _userRepository.GetByIdAsync(userId);
            if (user == null) throw new Exception("Không tìm thấy người dùng.");

            // ✅ SỬA LỖI #13: Thêm Validation
            if (reviewDto.Rating < 1 || reviewDto.Rating > 5)
                throw new ArgumentException("Rating phải từ 1-5", nameof(reviewDto.Rating));

            var product = await _productRepository.GetByIdAsync(productId);
            if (product == null) throw new Exception("Không tìm thấy sản phẩm.");
            // (Bạn có thể thêm logic kiểm tra đã mua hàng ở đây - Lỗi #29)
            // (Bạn có thể thêm logic kiểm tra đã review ở đây - Lỗi #30)

            var newReview = new ProductReview
            {
                Rating = reviewDto.Rating,
                Comment = reviewDto.Comment,
                ReviewDate = DateTime.UtcNow,
                ProductId = productId,
                UserId = userId,
                UserName = user.FullName // Lấy tên thật từ profile
            };
            await _reviewRepository.AddAsync(newReview);
            await _reviewRepository.SaveChangesAsync();
            return newReview;
        }
    }
}