using Application.Interfaces;
using Core.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using static Application.Dtos.CartDtos;

namespace Application.Services
{
    public class CartService : ICartService
    {
        private readonly ICartRepository _cartRepository;
        private readonly IProductRepository _productRepository; // Cần để lấy thông tin SP khi thêm

        public CartService(ICartRepository cartRepository, IProductRepository productRepository)
        {
            _cartRepository = cartRepository;
            _productRepository = productRepository;
        }

        // Hàm trợ giúp lấy hoặc tạo giỏ hàng
        private async Task<Cart> GetOrCreateCartAsync(Guid userId)
        {
            var cart = await _cartRepository.GetByUserIdAsync(userId);
            if (cart == null)
            {
                cart = await _cartRepository.CreateForUserAsync(userId);
                await _cartRepository.SaveChangesAsync();
            }
            return cart;
        }

        // Lấy giỏ hàng
        public async Task<CartResponseDto> GetCartAsync(Guid userId)
        {
            var cart = await GetOrCreateCartAsync(userId);
            return MapCartToDto(cart);
        }

        // Thêm sản phẩm
        public async Task<CartResponseDto> AddItemToCartAsync(Guid userId, AddItemToCartDto itemDto)
        {
            var cart = await GetOrCreateCartAsync(userId);
            var product = await _productRepository.GetByIdAsync(itemDto.ProductId); // Lấy sản phẩm (đã include Shop)
            if (product == null) throw new Exception("Không tìm thấy sản phẩm.");

            // ✅ SỬA LỖI #19: Kiểm tra tồn kho
            if (product.StockQuantity < itemDto.Quantity)
            {
                throw new InvalidOperationException(
                    $"Sản phẩm '{product.Name}' chỉ còn {product.StockQuantity} sản phẩm."
                );
            }
            // (Bạn cũng có thể kiểm tra tổng số lượng trong giỏ + số lượng thêm mới so với tồn kho)

            var existingItem = await _cartRepository.GetItemInCartAsync(cart.Id, itemDto.ProductId);
            if (existingItem != null)
            {
                // ✅ SỬA LỖI #19 (Logic bổ sung): Kiểm tra tồn kho khi tăng số lượng
                int newQuantity = existingItem.Quantity + itemDto.Quantity;
                if (product.StockQuantity < newQuantity)
                {
                    throw new InvalidOperationException(
                        $"Sản phẩm '{product.Name}' không đủ hàng (Bạn đã có {existingItem.Quantity} trong giỏ)."
                    );
                }
                existingItem.Quantity = newQuantity;
            }
            else
            {
                var newItem = new CartItem
                {
                    CartId = cart.Id,
                    ProductId = itemDto.ProductId,
                    Quantity = itemDto.Quantity,
                    ShopId = product.ShopId, // Lấy ShopId từ sản phẩm
                    IsSelected = true
                };
                await _cartRepository.AddItemAsync(newItem);
            }
            await _cartRepository.SaveChangesAsync();

            // Tải lại giỏ hàng đầy đủ để trả về
            cart = await _cartRepository.GetByUserIdAsync(userId);
            return MapCartToDto(cart!);
        }

        // Cập nhật số lượng
        public async Task<CartResponseDto> UpdateItemQuantityAsync(Guid userId, int cartItemId, int quantity)
        {
            var cart = await GetOrCreateCartAsync(userId);
            var item = await _cartRepository.GetItemByIdAsync(cartItemId);
            // Kiểm tra item có thuộc giỏ hàng của user không (bảo mật)
            if (item == null || item.CartId != cart.Id) throw new Exception("Không tìm thấy món hàng trong giỏ.");

            if (quantity <= 0)
            {
                _cartRepository.RemoveItem(item); // Nếu số lượng <= 0 thì xóa
            }
            else
            {
                // ✅ SỬA LỖI #19 (Logic bổ sung): Kiểm tra tồn kho khi cập nhật
                var product = await _productRepository.GetByIdAsync(item.ProductId);
                if (product == null) throw new Exception("Sản phẩm không còn tồn tại.");
                if (product.StockQuantity < quantity)
                {
                    throw new InvalidOperationException(
                       $"Sản phẩm '{product.Name}' chỉ còn {product.StockQuantity} sản phẩm."
                   );
                }
                item.Quantity = quantity;
            }
            await _cartRepository.SaveChangesAsync();

            cart = await _cartRepository.GetByUserIdAsync(userId);
            return MapCartToDto(cart!);
        }

        // Xóa sản phẩm
        public async Task<CartResponseDto> RemoveItemAsync(Guid userId, int cartItemId)
        {
            var cart = await GetOrCreateCartAsync(userId);
            var item = await _cartRepository.GetItemByIdAsync(cartItemId);
            if (item == null || item.CartId != cart.Id) throw new Exception("Không tìm thấy món hàng trong giỏ.");

            _cartRepository.RemoveItem(item);
            await _cartRepository.SaveChangesAsync();

            cart = await _cartRepository.GetByUserIdAsync(userId);
            return MapCartToDto(cart!);
        }

        // Chọn/Bỏ chọn sản phẩm
        public async Task<CartResponseDto> SetItemSelectionAsync(Guid userId, int cartItemId, bool isSelected)
        {
            var cart = await GetOrCreateCartAsync(userId);
            var item = await _cartRepository.GetItemByIdAsync(cartItemId);
            if (item == null || item.CartId != cart.Id) throw new Exception("Không tìm thấy món hàng trong giỏ.");

            item.IsSelected = isSelected;
            await _cartRepository.SaveChangesAsync();

            cart = await _cartRepository.GetByUserIdAsync(userId);
            return MapCartToDto(cart!);
        }


        // Hàm map dữ liệu sang DTO (nhóm theo shop, tính tổng tiền đã chọn)
        private CartResponseDto MapCartToDto(Cart cart)
        {
            var shopGroups = cart.Items
                .GroupBy(item => item.Shop) // Nhóm theo đối tượng Shop đã Include
                .Select(group => new ShopInCartDto
                {
                    ShopId = group.Key.Id,
                    ShopName = group.Key.Name,
                    Items = group.Select(item => new CartItemDto
                    {
                        Id = item.Id,
                        ProductId = item.ProductId,
                        ProductName = item.Product.Name,
                        ImageUrl = item.Product.Images?.FirstOrDefault()?.ImageUrl ?? null,
                        Price = item.Product.BasePrice,
                        Quantity = item.Quantity,
                        IsSelected = item.IsSelected
                    }).ToList()
                }).ToList();

            var totalPrice = cart.Items
                .Where(item => item.IsSelected) // Chỉ tính tiền các món được chọn
                .Sum(item => item.Product.BasePrice * item.Quantity);

            return new CartResponseDto
            {
                Id = cart.Id,
                Shops = shopGroups,
                TotalPrice = totalPrice
            };
        }
    }
}