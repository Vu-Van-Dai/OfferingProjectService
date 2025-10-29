using Application.Dtos;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Application.Interfaces
{
    public interface IUserAddressService
    {
        Task<IEnumerable<AddressResponseDto>> GetMyAddressesAsync(Guid userId);
        Task<AddressResponseDto?> GetMyAddressByIdAsync(Guid userId, int addressId);
        Task<AddressResponseDto> AddAddressAsync(Guid userId, UpsertAddressDto addressDto);
        Task<bool> UpdateAddressAsync(Guid userId, int addressId, UpsertAddressDto addressDto);
        Task<bool> DeleteAddressAsync(Guid userId, int addressId);
        Task<bool> SetDefaultAddressAsync(Guid userId, int addressId);
    }
}
