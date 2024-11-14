

using App.Data;
using App.Models;
using App.Models.Blog;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using App.Areas.Statistics.Models;
#nullable disable

namespace App.Areas.Statistics.Controllers
{

    [Area("Statistics")]
    [Authorize(Roles = RoleName.Editor + "," + RoleName.Administrator)]
    public class PostStatController : Controller
    {
        private readonly AppDbContext _context;
        private readonly UserManager<AppUser> _userManager;

        public PostStatController(AppDbContext context, UserManager<AppUser> userManager)
        {
            _context = context;
            _userManager = userManager;
        }

        [HttpGet]
        [Route("/stat/post/{categoryslug?}")]
        public async Task<IActionResult> Index(string categoryslug)
        {
            Category category = null;
            if (!string.IsNullOrEmpty(categoryslug))
            {
                category = await _context.Categories.FirstOrDefaultAsync(c => c.Slug == categoryslug);
            }
            var posts = await GetPostByUserAsync(category?.Id);

            int totalPosts = posts.Count;
            var totalViews = posts.Sum(post => post.Views);
            var categories = GetCategories();

            ViewBag.categories = categories;
            ViewBag.TotalViews = totalViews;
            ViewBag.category = category;
            ViewBag.categoryslug = categoryslug;
            ViewBag.sortModel = new SortModel()
            {
                DateFrom = new DateTime(2023, 1, 1),
                DateTo = DateTime.Now,
                Order = "date"
            };

            return View(posts);
        }

        [HttpPost]
        [Route("/stat/post/{categoryslug?}")]
        public async Task<IActionResult> Index(SortModel sortModel, string categoryslug)
        {
            Category category = null;
            if (!string.IsNullOrEmpty(categoryslug))
            {
                category = await _context.Categories.FirstOrDefaultAsync(c => c.Slug == categoryslug);
            }
            var posts = await GetPostByUserAsync(category?.Id);

            if (sortModel.DateFrom != null && sortModel.DateTo != null)
            {
                posts = posts.Where(post => post.DateCreated >= sortModel.DateFrom && post.DateCreated <= sortModel.DateTo).ToList();
            }

            if (!string.IsNullOrEmpty(sortModel.Order))
            {
                if (sortModel.Order == "views")
                {
                    posts = posts.OrderByDescending(post => post.Views).ToList();
                }
                else if (sortModel.Order == "title")
                {
                    posts = posts.OrderByDescending(post => post.Title).ToList();
                }
                else
                {
                    posts = posts.OrderByDescending(post => post.DateCreated).ToList();
                }
            }

            int totalPosts = posts.Count;
            var totalViews = posts.Sum(post => post.Views);
            var categories = GetCategories();

            ViewBag.categories = categories;
            ViewBag.TotalViews = totalViews;
            ViewBag.category = category;
            ViewBag.categoryslug = categoryslug;
            ViewBag.sortModel = sortModel;

            return View(posts);
        }


        public async Task<List<Post>> GetPostByUserAsync(int? categoryId)
        {
            var user = await _userManager.GetUserAsync(this.User);

            var posts = await _context.Posts
                                    .Where(p => p.AuthorId == user.Id)
                                    .Include(p => p.PostCategories)
                                    .ThenInclude(p => p.Category)
                                    .ToListAsync();
            if (categoryId != null)
            {

                posts = posts.Where(post => post.PostCategories.Any(pc => pc.CategoryID == categoryId)).ToList();

            }
            return posts;
        }

        public List<Category> GetCategories()
        {
            var categories = _context.Categories
                            .Include(c => c.CategoryChildren)
                            .AsEnumerable()
                            .Where(c => c.ParentCategory == null)
                            .ToList();
            return categories;
        }
    }
}