using System.Runtime.Intrinsics.X86;
using App.Models;
using App.Models.Blog;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace App.Areas.Blog.Controllers
{
    [Area("Blog")]
    public class ViewPostController : Controller
    {
        private readonly AppDbContext _appDbContext;
        private readonly ILogger<ViewPostController> _logger;
        public ViewPostController(AppDbContext appDbContext, ILogger<ViewPostController> logger)
        {
            _appDbContext = appDbContext;
            _logger = logger;
        }

        // GET: ViewPostController
        [Route("/post/{categoryslug?}")]
        public ActionResult Index(string categoryslug, [FromQuery(Name = "p")] int currentPage, int pageSize)
        {
            var categories = GetCategories();
            ViewBag.categories = categories;
            ViewBag.categoryslug = categoryslug;
            Category category = null;
            if (!string.IsNullOrEmpty(categoryslug))
            {
                category = _appDbContext.Categories.Where(c => c.Slug == categoryslug)
                                                    .Include(c => c.CategoryChildren)
                                                    .FirstOrDefault();
                if (category == null)
                {
                    return NotFound();
                }
            }

            var posts = _appDbContext.Posts
                                    .Include(p => p.Author)
                                    .Include(p => p.PostCategories)
                                    .ThenInclude(p => p.Category)
                                    .AsQueryable();

            if (category != null)
            {
                var ids = new List<int>();
                category.ChildCategoryIDs(ids, null);
                ids.Add(category.Id);

                posts = posts.Where(p => p.PostCategories.Where(pc => ids.Contains(pc.CategoryID)).Any());
            }
            posts = posts.OrderByDescending(p => p.DateUpdated);


            int totalPosts = posts.Count();
            if (pageSize <= 0) pageSize = 7;
            int countPages = (int)Math.Ceiling((double)totalPosts / pageSize);
            if (currentPage > countPages) currentPage = countPages;
            if (currentPage < 1) currentPage = 1;

            var pagingmodel = new PagingModel()
            {
                currentpage = currentPage,
                countpages = countPages,
                generateUrl = (int? pageNumber) => Url.Action("Index", new { p = pageNumber, pageSize = pageSize })
            };
            ViewBag.pagingmodel = pagingmodel;
            ViewBag.totalPosts = totalPosts;

            var postsInPage = posts.Skip((currentPage - 1) * pageSize).Take(pageSize);

            ViewBag.category = category;
            return View(postsInPage.ToList());
        }

        [Route("/post/{postslug}.html")]
        public IActionResult Detail(string postslug)
        {
            var categories = GetCategories();
            ViewBag.categories = categories;

            var post = _appDbContext.Posts.Where(p => p.Slug == postslug)
                                    .Include(p => p.Author)
                                    .Include(p => p.PostCategories)
                                    .ThenInclude(pc => pc.Category)
                                    .FirstOrDefault();
            if (post == null)
            {
                return NotFound("Không tìm thấy bài viết");
            }

            post.Views++;
            _appDbContext.Update(post);
            _appDbContext.SaveChanges();
            var category = post.PostCategories.FirstOrDefault()?.Category;
            ViewBag.category = category;

            var otherPosts = _appDbContext.Posts.Where(p => p.PostCategories.Any(pc => pc.CategoryID == category.Id))
                                                .Where(p => p.PostId != post.PostId)
                                                .OrderByDescending(p => p.DateUpdated)
                                                .Take(5);
            ViewBag.otherPosts = otherPosts;
            return View(post);
        }



        public List<Category> GetCategories()
        {
            var categories = _appDbContext.Categories
                            .Include(c => c.CategoryChildren)
                            .AsEnumerable()
                            .Where(c => c.ParentCategory == null)
                            .ToList();
            return categories;
        }
    }
}
