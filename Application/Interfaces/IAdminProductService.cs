using Application.Dtos;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Application.Interfaces
{
    public interface IAdminProductService
    {
        Task<IEnumerable<AdminProductListResponseDto>> GetAllProductsAsync();
        Task<bool> ToggleProductVisibilityAsync(int productId, bool isHidden);
        Task<bool> ChangeProductCategoryAsync(int productId, int newCategoryId);
    }
}
