using Application.Dtos;
using Core.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Application.Interfaces
{
    public interface IShopService
    {
        Task<Shop> CreateShopAsync(Guid ownerUserId, CreateShopDto shopDto);
    }
}
