using Application.Dtos.Application.Dtos;
using Application.Interfaces;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Application.Services
{
    public class DashboardService : IDashboardService
    {
        private readonly IProductRepository _productRepository;
        private readonly IOrderItemRepository _orderItemRepository;

        public DashboardService(IProductRepository productRepository, IOrderItemRepository orderItemRepository)
        {
            _productRepository = productRepository;
            _orderItemRepository = orderItemRepository;
        }

        public async Task<ShopDashboardDto> GetShopDashboardAsync(int shopId)
        {
            var totalProducts = await _productRepository.CountByShopIdAsync(shopId);
            var inStock = await _productRepository.CountInStockByShopIdAsync(shopId);
            var outOfStock = await _productRepository.CountOutOfStockByShopIdAsync(shopId);
            var pendingOrders = await _orderItemRepository.CountPendingByShopIdAsync(shopId);

            return new ShopDashboardDto
            {
                TotalProducts = totalProducts,
                ProductsInStock = inStock,
                OutOfStockProducts = outOfStock,
                PendingOrderItems = pendingOrders
            };
        }
    }
}
