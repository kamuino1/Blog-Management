


using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
#nullable disable

namespace App.Models.Blog
{
    [Table("Category")]
    public class Category
    {

        [Key]
        public int Id { get; set; }

        // Tiều đề Category
        [Required(ErrorMessage = "Phải có tên danh mục")]
        [StringLength(100, MinimumLength = 3, ErrorMessage = "{0} dài {1} đến {2}")]
        [Display(Name = "Tên danh mục")]
        public string Title { get; set; }

        // Nội dung, thông tin chi tiết về Category
        [DataType(DataType.Text)]
        [Display(Name = "Nội dung danh mục")]
        public string Descripton { set; get; }

        //chuỗi Url
        [Required(ErrorMessage = "Phải tạo url")]
        [StringLength(100, MinimumLength = 3, ErrorMessage = "{0} dài {1} đến {2}")]
        [RegularExpression(@"^[a-z0-9-]*$", ErrorMessage = "Chỉ dùng các ký tự [a-z0-9-]")]
        [Display(Name = "Url hiện thị")]
        public string Slug { set; get; }

        // Category cha (FKey)
        [Display(Name = "Danh mục cha")]
        public int? ParentCategoryId { get; set; }

        // Các Category con
        public ICollection<Category> CategoryChildren { get; set; }

        [ForeignKey("ParentCategoryId")]
        [Display(Name = "Danh mục cha")]
        public Category ParentCategory { set; get; }

        public void ChildCategoryIDs(List<int> list, ICollection<Category> childcates = null)
        {
            if (childcates == null)
            {
                childcates = this.CategoryChildren;
            }
            foreach (Category child in childcates)
            {
                list.Add(child.Id);
                ChildCategoryIDs(list, child.CategoryChildren);
            }
        }

        public List<Category> ListParents()
        {
            List<Category> list = new List<Category>();
            var parent = this.ParentCategory;
            while (parent != null)
            {
                list.Add(parent);
                parent = parent.ParentCategory;
            }
            list.Reverse();
            return list;
        }
    }
}