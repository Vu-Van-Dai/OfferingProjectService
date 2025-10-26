using Application.Interfaces;
using Microsoft.AspNetCore.Mvc;

namespace API.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class SearchController : ControllerBase
    {
        private readonly ISearchService _searchService;

        public SearchController(ISearchService searchService)
        {
            _searchService = searchService;
        }

        // API duy nhất: GET /api/search?q=tên_cần_tìm
        [HttpGet]
        public async Task<IActionResult> Search([FromQuery] string q)
        {
            if (string.IsNullOrWhiteSpace(q))
            {
                // Trả về kết quả rỗng nếu không có từ khóa
                return Ok(new Application.Dtos.GlobalSearchResponseDto());
            }

            var results = await _searchService.SearchAsync(q);
            return Ok(results);
        }
    }
}
