

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
        public async Task<IActionResult> Index(SortModel sortModel, string categoryslug)
        {
            Category category = null;
            if (!string.IsNullOrEmpty(categoryslug))
            {
                category = await _context.Categories.FirstOrDefaultAsync(c => c.Slug == categoryslug);
            }
            var posts = await GetPostByUserAsync(category?.Id);

            posts = FilterPost(sortModel.DateFrom, sortModel.DateTo, posts);

            if (!string.IsNullOrEmpty(sortModel.Order))
            {
                if (sortModel.Order == "views")
                {
                    posts = posts.OrderByDescending(post => post.Views).ToList();
                }
                else if (sortModel.Order == "title")
                {
                    posts = posts.OrderBy(post => post.Title).ToList();
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


        [Route("/stat/post/{postslug}.html")]
        public IActionResult Detail(string postslug)
        {
            var categories = GetCategories();
            ViewBag.categories = categories;

            var post = _context.Posts.Where(p => p.Slug == postslug)
                                    .Include(p => p.Author)
                                    .Include(p => p.PostCategories)
                                    .ThenInclude(pc => pc.Category)
                                    .FirstOrDefault();
            if (post == null)
            {
                return NotFound("Không tìm thấy bài viết");
            }

            post.Views++;
            _context.Update(post);
            _context.SaveChanges();
            var category = post.PostCategories.FirstOrDefault()?.Category;
            ViewBag.category = category;
            return View(post);
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

        public List<Post> FilterPost(DateTime? dateFrom, DateTime? dateTo, List<Post> posts)
        {
            if (dateFrom != null && dateTo != null)
            {
                posts = posts.Where(post => post.DateCreated >= dateFrom && post.DateCreated <= dateTo).ToList();
            }
            return posts;
        }
    }
}