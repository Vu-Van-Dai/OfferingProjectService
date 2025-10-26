using Application.Dtos;
using Core.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Application.Interfaces
{
    public interface IReviewService
    {
        Task<IEnumerable<ProductReview>> GetReviewsForProductAsync(int productId);
        Task<ProductReview> AddReviewAsync(Guid userId, int productId, CreateReviewDto reviewDto);
    }
}
