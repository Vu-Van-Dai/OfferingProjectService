using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Application.Dtos
{
    // DTO để Frontend gửi lên khi thêm hoặc sửa địa chỉ
    public class UpsertAddressDto
    {
        [Required] public string FullName { get; set; }
        [Required] public string PhoneNumber { get; set; }
        [Required] public string Street { get; set; }
        [Required] public string Ward { get; set; }
        [Required] public string District { get; set; }
        [Required] public string City { get; set; }
        public bool IsDefault { get; set; }
    }

    // DTO để Backend trả về danh sách địa chỉ
    public class AddressResponseDto
    {
        public int Id { get; set; }
        public string FullName { get; set; }
        public string PhoneNumber { get; set; }
        public string Street { get; set; }
        public string Ward { get; set; }
        public string District { get; set; }
        public string City { get; set; }
        public bool IsDefault { get; set; }
    }
}
