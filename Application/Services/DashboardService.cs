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
            // Chạy các task bất đồng bộ song song để tăng hiệu năng
            var totalProductsTask = _productRepository.CountByShopIdAsync(shopId);
            var inStockTask = _productRepository.CountInStockByShopIdAsync(shopId);
            var outOfStockTask = _productRepository.CountOutOfStockByShopIdAsync(shopId);
            var pendingOrdersTask = _orderItemRepository.CountPendingByShopIdAsync(shopId);

            // Đợi tất cả hoàn thành
            await Task.WhenAll(totalProductsTask, inStockTask, outOfStockTask, pendingOrdersTask);

            // Lấy kết quả
            return new ShopDashboardDto
            {
                TotalProducts = await totalProductsTask,
                ProductsInStock = await inStockTask,
                OutOfStockProducts = await outOfStockTask,
                PendingOrderItems = await pendingOrdersTask
            };
        }
    }
}
