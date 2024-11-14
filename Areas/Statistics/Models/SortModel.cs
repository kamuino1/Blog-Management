using System.ComponentModel.DataAnnotations;
using App.Models.Blog;
#nullable disable

namespace App.Areas.Statistics.Models
{
    public class SortModel
    {
        [Display(Name = "Từ ngày")]
        public DateTime? DateFrom { get; set; }

        [Display(Name = "Đến ngày")]
        public DateTime? DateTo { get; set; }

        [Display(Name = "Sắp xếp theo")]
        public string Order { get; set; } = "date";
    }
}