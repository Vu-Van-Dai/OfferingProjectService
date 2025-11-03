using Application.Dtos;
using Application.Interfaces;
using Core.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Application.Services
{
    public class UserAddressService : IUserAddressService
    {
        private readonly IUserAddressRepository _addressRepository;

        public UserAddressService(IUserAddressRepository addressRepository)
        {
            _addressRepository = addressRepository;
        }

        public async Task<IEnumerable<AddressResponseDto>> GetMyAddressesAsync(Guid userId)
        {
            var addresses = await _addressRepository.GetByUserIdAsync(userId);
            return addresses.Select(MapToDto); // Map sang DTO
        }

        public async Task<AddressResponseDto?> GetMyAddressByIdAsync(Guid userId, int addressId)
        {
            var address = await _addressRepository.GetByIdAsync(addressId);
            // Kiểm tra xem địa chỉ có thuộc user không
            if (address == null || address.UserId != userId) return null;
            return MapToDto(address);
        }

        public async Task<AddressResponseDto> AddAddressAsync(Guid userId, UpsertAddressDto addressDto)
        {
            var newAddress = MapFromDto(addressDto);
            newAddress.UserId = userId;

            // Nếu người dùng chọn đây là mặc định, bỏ mặc định cũ
            if (newAddress.IsDefault)
            {
                await _addressRepository.UnsetDefaultForUserAsync(userId);
            }
            // Nếu đây là địa chỉ đầu tiên, tự động đặt làm mặc định
            else if (!(await _addressRepository.GetByUserIdAsync(userId)).Any())
            {
                newAddress.IsDefault = true;
            }

            await _addressRepository.AddAsync(newAddress);
            await _addressRepository.SaveChangesAsync();
            return MapToDto(newAddress);
        }

        public async Task<bool> UpdateAddressAsync(Guid userId, int addressId, UpsertAddressDto addressDto)
        {
            var existingAddress = await _addressRepository.GetByIdAsync(addressId);
            if (existingAddress == null || existingAddress.UserId != userId) return false;

            // Bỏ mặc định cũ nếu cần
            if (addressDto.IsDefault && !existingAddress.IsDefault)
            {
                await _addressRepository.UnsetDefaultForUserAsync(userId);
            }

            // Cập nhật thông tin
            existingAddress.FullName = addressDto.FullName;
            existingAddress.PhoneNumber = addressDto.PhoneNumber;
            existingAddress.Street = addressDto.Street;
            existingAddress.Ward = addressDto.Ward;
            existingAddress.District = addressDto.District;
            existingAddress.City = addressDto.City;
            existingAddress.IsDefault = addressDto.IsDefault;

            _addressRepository.Update(existingAddress);
            await _addressRepository.SaveChangesAsync();
            return true;
        }

        public async Task<bool> DeleteAddressAsync(Guid userId, int addressId)
        {
            var address = await _addressRepository.GetByIdAsync(addressId);
            if (address == null || address.UserId != userId) return false;

            _addressRepository.Delete(address);
            await _addressRepository.SaveChangesAsync();
            return true;
        }

        public async Task<bool> SetDefaultAddressAsync(Guid userId, int addressId)
        {
            var addressToSet = await _addressRepository.GetByIdAsync(addressId);
            if (addressToSet == null || addressToSet.UserId != userId) return false;

            // Bỏ mặc định cũ
            await _addressRepository.UnsetDefaultForUserAsync(userId);

            // Đặt mặc định mới
            addressToSet.IsDefault = true;
            _addressRepository.Update(addressToSet);
            await _addressRepository.SaveChangesAsync();
            return true;
        }

        // ✅ SỬA LỖI #20: Triển khai các hàm map
        private AddressResponseDto MapToDto(UserAddress address) => new AddressResponseDto
        {
            Id = address.Id,
            FullName = address.FullName,
            PhoneNumber = address.PhoneNumber,
            Street = address.Street,
            Ward = address.Ward,
            District = address.District,
            City = address.City,
            IsDefault = address.IsDefault
        };
        private UserAddress MapFromDto(UpsertAddressDto dto) => new UserAddress
        {
            FullName = dto.FullName,
            PhoneNumber = dto.PhoneNumber,
            Street = dto.Street,
            Ward = dto.Ward,
            District = dto.District,
            City = dto.City,
            IsDefault = dto.IsDefault
        };
    }
}