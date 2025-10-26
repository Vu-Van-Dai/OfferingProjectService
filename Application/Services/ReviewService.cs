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

        public ReviewService(IReviewRepository reviewRepository, IUserRepository userRepository)
        {
            _reviewRepository = reviewRepository;
            _userRepository = userRepository;
        }
        public async Task<IEnumerable<ProductReview>> GetReviewsForProductAsync(int productId)
        {
            return await _reviewRepository.GetByProductIdAsync(productId);
        }
        public async Task<ProductReview> AddReviewAsync(Guid userId, int productId, CreateReviewDto reviewDto)
        {
            var user = await _userRepository.GetByIdAsync(userId);
            if (user == null) throw new Exception("Không tìm thấy người dùng.");

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
