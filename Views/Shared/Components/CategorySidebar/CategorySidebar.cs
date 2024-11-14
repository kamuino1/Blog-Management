

using App.Models.Blog;
using Microsoft.AspNetCore.Mvc;
#nullable disable 

namespace App.Components
{
    [ViewComponent]
    public class CategorySidebar : ViewComponent
    {
        public class CategorySidebarData
        {
            public List<Category> Categories { get; set; }
            public int level { get; set; }
            public string Categoryslug { get; set; }
            public string Controller { get; set; }
        }

        public IViewComponentResult Invoke(CategorySidebarData data)
        {
            return View(data);
        }
    }
}