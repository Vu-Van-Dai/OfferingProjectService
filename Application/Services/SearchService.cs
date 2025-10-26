using Application.Dtos;
using Application.Interfaces;
using Application.Utils;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Application.Services
{
    public class SearchService : ISearchService
    {
        private readonly IProductRepository _productRepository;
        private readonly IShopRepository _shopRepository;

        public SearchService(IProductRepository productRepository, IShopRepository shopRepository)
        {
            _productRepository = productRepository;
            _shopRepository = shopRepository;
        }

        public async Task<GlobalSearchResponseDto> SearchAsync(string query)
        {
            var normalizedQuery = StringUtils.RemoveAccents(query);
            // 1. Thực hiện tìm kiếm song song sản phẩm và cửa hàng
            var productTask = _productRepository.SearchByNameAsync(normalizedQuery); // Gửi từ đã chuẩn hóa
            var shopTask = _shopRepository.SearchByNameAsync(normalizedQuery); // Gửi từ đã chuẩn hóa

            await Task.WhenAll(productTask, shopTask);

            var foundProducts = await productTask;
            var foundShops = await shopTask;

            var response = new GlobalSearchResponseDto();

            // 2. Map kết quả tìm kiếm sản phẩm
            response.Products = foundProducts.Select(p => new ProductSearchResultDto
            {
                Id = p.Id,
                Name = p.Name,
                ImageUrl = p.ImageUrl,
                BasePrice = p.BasePrice,
                ShopName = p.Shop.Name // Lấy tên shop từ đối tượng Shop đã Include
            }).ToList();

            // 3. Map kết quả tìm kiếm cửa hàng
            response.Shops = foundShops.Select(s => new ShopSearchResultDto
            {
                Id = s.Id,
                Name = s.Name,
                ImageUrl = s.ImageUrl,
                // Map các sản phẩm nổi bật đã được tải kèm
                PopularProducts = s.Products.Select(p => new ProductSearchResultDto
                {
                    Id = p.Id,
                    Name = p.Name,
                    ImageUrl = p.ImageUrl,
                    BasePrice = p.BasePrice,
                    ShopName = s.Name
                }).ToList()
            }).ToList();

            return response;
        }
    }
}
