using System.ComponentModel.DataAnnotations;
namespace BTL_LTW.Models
{
    public class Reservation
    {
        public string? Id { get; set; } = Guid.NewGuid().ToString();
        [Required(ErrorMessage = "Vui lòng nhập họ tên.")]
        [StringLength(100, ErrorMessage = "Họ tên tối đa 100 ký tự.")]
        [RegularExpression(@"^[\p{L}\s]+$", ErrorMessage = "Họ tên chỉ chứa chữ cái và khoảng trắng.")]
        public string CustomerName { get; set; } = "";
        [Required(ErrorMessage = "Vui lòng nhập số điện thoại.")]
        [RegularExpression(@"^0\d{9}$", ErrorMessage = "Số điện thoại không hợp lệ.")]
        public string Phone { get; set; } = "";
        public string? Email { get; set; } = "";
        public DateTime DateTime { get; set; } = DateTime.UtcNow;
        public int People { get; set; } = 2;
        public string? Note { get; set; } = "";
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow; 

        public string? AssignedTable { get; set; }
        public string Status { set; get; } = "Pending";
        public string? LinkOrderId { get; set; }
    }
}
