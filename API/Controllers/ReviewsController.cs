using Application.Dtos;
using Application.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace API.Controllers
{
    [ApiController]
    [Route("api/products/{productId}/reviews")]
    public class ReviewsController : ControllerBase
    {
        private readonly IReviewService _reviewService;

        public ReviewsController(IReviewService reviewService)
        {
            _reviewService = reviewService;
        }

        // GET: /api/products/5/reviews
        [HttpGet]
        public async Task<IActionResult> GetReviews(int productId)
        {
            var reviews = await _reviewService.GetReviewsForProductAsync(productId);
            return Ok(reviews);
        }

        // POST: /api/products/5/reviews
        [HttpPost]
        [Authorize] // Yêu cầu đăng nhập (Guest, Shop, Admin đều có thể review)
        public async Task<IActionResult> PostReview(int productId, [FromBody] CreateReviewDto reviewDto)
        {
            var userId = Guid.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier));
            try
            {
                var newReview = await _reviewService.AddReviewAsync(userId, productId, reviewDto);
                return CreatedAtAction(nameof(GetReviews), new { productId = productId }, newReview);
            }
            catch (Exception ex)
            {
                return BadRequest(new { message = ex.Message });
            }
        }
    }
}
