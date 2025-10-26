using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Application.Dtos
{
    public class CreateShopDto
    {
        public string ShopName { get; set; }
        public string? Description { get; set; }
    }

    public class GrantShopRoleDto
    {
        public string UserEmail { get; set; }
        public string ShopName { get; set; }
    }
}
