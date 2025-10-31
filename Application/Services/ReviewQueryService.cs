using Application.Dtos;
using Application.Interfaces;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Application.Services
{
    public class ReviewQueryService : IReviewQueryService
    {
        private readonly IReviewRepository _reviewRepository;

        public ReviewQueryService(IReviewRepository reviewRepository)
        {
            _reviewRepository = reviewRepository;
        }

        public async Task<IEnumerable<ShopReviewDto>> GetReviewsForShopAsync(int shopId)
        {
            var reviews = await _reviewRepository.GetByShopIdAsync(shopId);

            // Map sang DTO
            return reviews.Select(r => new ShopReviewDto
            {
                ReviewId = r.Id,
                ProductId = r.ProductId,
                ProductName = r.Product.Name, // Đảm bảo Repo đã Include Product
                Rating = r.Rating,
                Comment = r.Comment,
                UserName = r.UserName,
                ReviewDate = r.ReviewDate
            });
        }
    }
}
