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
        private readonly IImageService _imageService;

        public SearchService(IProductRepository productRepository, IShopRepository shopRepository, IImageService imageService)
        {
            _productRepository = productRepository;
            _shopRepository = shopRepository;
            _imageService = imageService;
        }

        public async Task<GlobalSearchResponseDto> SearchAsync(string query)
        {
            if (string.IsNullOrWhiteSpace(query)) return new GlobalSearchResponseDto();

            var normalizedQuery = StringUtils.RemoveAccents(query);
            var productTask = _productRepository.SearchByNameAsync(normalizedQuery);
            var shopTask = _shopRepository.SearchByNameAsync(normalizedQuery);

            await Task.WhenAll(productTask, shopTask);

            var foundProducts = await productTask;
            var foundShops = await shopTask;

            var response = new GlobalSearchResponseDto();

            response.Products = foundProducts.Select(p =>
            {
                // Lấy ảnh đầu tiên trong list
                var firstImg = p.Images?.FirstOrDefault();
                return new ProductSearchResultDto
                {
                    Id = p.Id,
                    Name = p.Name,
                    // SỬA LỖI IMAGE URL
                    ImageUrl = firstImg != null ? _imageService.ToBase64(firstImg.ImageData, firstImg.ImageMimeType) : null,
                    BasePrice = p.BasePrice,
                    ShopName = p.Shop.Name
                };
            }).ToList();

            response.Shops = foundShops.Select(s => new ShopSearchResultDto
            {
                Id = s.Id,
                Name = s.Name,
                // SỬA LỖI SHOP IMAGE URL
                ImageUrl = _imageService.ToBase64(s.AvatarData, s.AvatarMimeType),

                PopularProducts = s.Products.Select(p =>
                {
                    var pImg = p.Images?.FirstOrDefault();
                    return new ProductSearchResultDto
                    {
                        Id = p.Id,
                        Name = p.Name,
                        // SỬA LỖI IMAGE URL
                        ImageUrl = pImg != null ? _imageService.ToBase64(pImg.ImageData, pImg.ImageMimeType) : null,
                        BasePrice = p.BasePrice,
                        ShopName = s.Name
                    };
                }).ToList()
            }).ToList();

            return response;
        }
    }
}