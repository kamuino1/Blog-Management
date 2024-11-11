using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using App.Models;
using App.Models.Blog;
using Microsoft.AspNetCore.Authorization;
using App.Data;
using Microsoft.AspNetCore.Identity;
using App.Areas.Blog.Models;
using App.Utilities;
#nullable disable

namespace App.Areas.Blog.Controllers
{
    [Route("admin/blog/post/[action]/{id?}")]
    [Area("Blog")]
    [Authorize(Roles = RoleName.Administrator + "," + RoleName.Editor)]
    public class PostController : Controller
    {
        private readonly AppDbContext _context;
        private readonly UserManager<AppUser> _userManager;

        public PostController(AppDbContext context, UserManager<AppUser> userManager)
        {
            _context = context;
            _userManager = userManager;
        }

        [TempData]
        public string StatusMessage { get; set; }

        // GET: Post
        public async Task<IActionResult> Index([FromQuery(Name = "p")] int currentPage, int pageSize)
        {
            var posts = _context.Posts
                        .Include(p => p.Author)
                        .OrderByDescending(p => p.DateUpdated);

            int totalPosts = await posts.CountAsync();
            if (pageSize <= 0) pageSize = 10;
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
            ViewBag.postIndex = (currentPage - 1) * pageSize;

            var postsInPage = await posts.Skip((currentPage - 1) * pageSize)
                                    .Take(pageSize)
                                    .Include(p => p.PostCategories)
                                    .ThenInclude(pc => pc.Category)
                                    .ToListAsync();


            return View(postsInPage);
        }

        // GET: Post/Details/5
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var post = await _context.Posts
                .Include(p => p.Author)
                .FirstOrDefaultAsync(m => m.PostId == id);
            if (post == null)
            {
                return NotFound();
            }
            post.Views++;
            _context.Update(post);
            await _context.SaveChangesAsync();

            return View(post);
        }

        // GET: Post/Create
        public async Task<IActionResult> Create()
        {
            var categories = await _context.Categories.ToListAsync();
            ViewData["categories"] = new MultiSelectList(categories, "Id", "Title");
            return View();
        }

        // POST: Post/Create
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("Title,Description,Slug,Content,Published, CategotyIDs")] CreatePostModel post)
        {
            var categories = await _context.Categories.ToListAsync();
            ViewData["categories"] = new MultiSelectList(categories, "Id", "Title");

            if (post.Slug == null)
            {
                post.Slug = AppUtilities.GenerateSlug(post.Title);
            }

            if (await _context.Posts.AnyAsync(p => p.Slug == post.Slug))
            {
                ModelState.AddModelError("Slug", "Input another url");
                return View(post);
            }

            if (ModelState.IsValid)
            {
                var user = await _userManager.GetUserAsync(this.User);

                post.DateCreated = post.DateUpdated = DateTime.Now;
                post.AuthorId = user.Id;

                if (post.CategotyIDs != null)
                {
                    foreach (var CateID in post.CategotyIDs)
                    {
                        _context.Add(new PostCategory()
                        {
                            CategoryID = CateID,
                            Post = post
                        });
                    }
                }

                _context.Add(post);
                await _context.SaveChangesAsync();
                StatusMessage = "Just created a post";
                return RedirectToAction(nameof(Index));
            }
            return View(post);
        }

        // GET: Post/Edit/5
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var post = await _context.Posts.Include(pc => pc.PostCategories).FirstOrDefaultAsync(p => p.PostId == id);
            if (post == null)
            {
                return NotFound();
            }

            var postEdit = new CreatePostModel()
            {
                PostId = post.PostId,
                Title = post.Title,
                Description = post.Description,
                Content = post.Content,
                Slug = post.Slug,
                Published = post.Published,
                CategotyIDs = post.PostCategories.Select(c => c.CategoryID).ToArray()

            };

            var categories = await _context.Categories.ToListAsync();
            ViewData["categories"] = new MultiSelectList(categories, "Id", "Title");

            return View(postEdit);
        }

        // POST: Post/Edit/5
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, [Bind("PostId,Title,Description,Slug,Content,Published, CategotyIDs")] CreatePostModel post)
        {

            if (id != post.PostId)
            {
                return NotFound();
            }

            var categories = await _context.Categories.ToListAsync();
            ViewData["categories"] = new MultiSelectList(categories, "Id", "Title");

            if (post.Slug == null)
            {
                post.Slug = AppUtilities.GenerateSlug(post.Title);
            }

            if (await _context.Posts.AnyAsync(p => p.Slug == post.Slug && p.PostId != id))
            {
                ModelState.AddModelError("Slug", "Input another url");
                return View(post);
            }

            if (ModelState.IsValid)
            {
                try
                {
                    var postUpdate = await _context.Posts.Include(pc => pc.PostCategories).FirstOrDefaultAsync(p => p.PostId == id);
                    if (post == null)
                    {
                        return NotFound();
                    }
                    postUpdate.Title = post.Title;
                    postUpdate.Description = post.Description;
                    postUpdate.Content = post.Content;
                    postUpdate.Slug = post.Slug;
                    postUpdate.Published = post.Published;
                    postUpdate.DateUpdated = DateTime.Now;
                    //Category
                    if (post.CategotyIDs == null) post.CategotyIDs = new int[] { };

                    var oldCateIds = postUpdate.PostCategories.Select(c => c.CategoryID).ToArray();
                    var newCateIds = post.CategotyIDs;
                    var removeCatePosts = from postCate in postUpdate.PostCategories
                                          where (!newCateIds.Contains(postCate.CategoryID))
                                          select postCate;
                    _context.PostCategories.RemoveRange(removeCatePosts);

                    var addCateIds = from CateId in newCateIds
                                     where !oldCateIds.Contains(CateId)
                                     select CateId;

                    foreach (var CateId in addCateIds)
                    {
                        _context.PostCategories.Add(new PostCategory()
                        {
                            PostID = id,
                            CategoryID = CateId
                        });
                    }
                    _context.Update(postUpdate);
                    await _context.SaveChangesAsync();
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!PostExists(post.PostId))
                    {
                        return NotFound();
                    }
                    else
                    {
                        throw;
                    }
                }
                StatusMessage = "Vừa cập nhật bài viết";
                return RedirectToAction(nameof(Index));
            }
            return View(post);
        }

        // GET: Post/Delete/5
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var post = await _context.Posts
                .Include(p => p.Author)
                .FirstOrDefaultAsync(m => m.PostId == id);
            if (post == null)
            {
                return NotFound();
            }

            return View(post);
        }

        // POST: Post/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var post = await _context.Posts.FindAsync(id);
            if (post != null)
            {
                _context.Posts.Remove(post);
            }
            else
            {
                return NotFound();
            }
            StatusMessage = "You just delete post: " + post.Title;

            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }

        private bool PostExists(int id)
        {
            return _context.Posts.Any(e => e.PostId == id);
        }
    }
}
