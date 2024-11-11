#nullable disable

using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace App.Models.Contacts
{
    public class Contact
    {
        [Key]
        public int Id { get; set; }

        [Required(ErrorMessage = "Phải nhập {0}")]
        [StringLength(50)]
        [Column(TypeName = "nvarchar")]
        [Display(Name = "Họ tên")]
        public string FullName { get; set; }

        [Required(ErrorMessage = "Phải là {0}")]
        [StringLength(100)]
        [Display(Name = "Địa chỉ email")]
        public string Email { get; set; }

        public DateTime DateSent { get; set; }

        [Display(Name = "Nội dung")]
        public string Message { get; set; }

        [StringLength(50)]
        [Phone(ErrorMessage = "Phải là {0}")]
        [Display(Name = "Số điện thoại")]
        public string PhoneNumber { get; set; }
    }
}