using System.ComponentModel.DataAnnotations;
using App.Models.Blog;
#nullable disable

namespace App.Areas.Blog.Models
{
    public class CreatePostModel : Post
    {
        [Display(Name = "Chuyên mục")]
        public int[] CategotyIDs { get; set; }
    }
}