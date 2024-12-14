using App.Data;
using App.Models;
using App.Models.Blog;
using Bogus;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
#nullable disable

namespace App.Areas.Database.Controllers
{
    [Area("Database")]
    [Route("/database-manage/[action]")]
    public class DbManage : Controller
    {
        private readonly AppDbContext _appDbContext;
        private readonly UserManager<AppUser> _userManager;
        private readonly RoleManager<IdentityRole> _roleManager;

        public DbManage(AppDbContext appDbContext, UserManager<AppUser> userManager, RoleManager<IdentityRole> roleManager)
        {
            _appDbContext = appDbContext;
            _userManager = userManager;
            _roleManager = roleManager;
        }

        [TempData]
        public string StatusMessage { get; set; }

        // GET: DbManage
        public ActionResult Index()
        {
            return View();
        }

        [HttpGet]
        public IActionResult DeleteDb()
        {
            return View();
        }

        [HttpPost]
        public async Task<IActionResult> DeleteDbAsync()
        {
            var success = await _appDbContext.Database.EnsureDeletedAsync();
            StatusMessage = success ? "Xóa DB thành công" : "Không xóa được DB";
            return RedirectToAction(nameof(Index));

        }

        [HttpPost]
        public async Task<IActionResult> Migrate()
        {
            await _appDbContext.Database.MigrateAsync();
            StatusMessage = "Cập nhật DB thành công";
            return RedirectToAction(nameof(Index));

        }


        public async Task<IActionResult> SeedDataAsync()
        {
            var rolenames = typeof(RoleName).GetFields().ToList();
            foreach (var r in rolenames)
            {
                var rolename = (string)r.GetRawConstantValue();
                var rfound = await _roleManager.FindByNameAsync(rolename);
                if (rfound == null)
                {
                    await _roleManager.CreateAsync(new IdentityRole(rolename));
                }
            }

            //admin, pass=tien012369, admin@example.com
            var useradmin = await _userManager.FindByEmailAsync("admin@example.com");
            if (useradmin == null)
            {
                useradmin = new AppUser()
                {
                    UserName = "admin",
                    Email = "admin@example.com",
                    EmailConfirmed = true
                };
                await _userManager.CreateAsync(useradmin, "admin");
                await _userManager.AddToRoleAsync(useradmin, RoleName.Administrator);
            }

            SeedPostCategory();

            StatusMessage = "Đã seed data thành công";
            return RedirectToAction(nameof(Index));
        }
        private void SeedPostCategory()
        {
            _appDbContext.Categories.RemoveRange(_appDbContext.Categories.Where(c => c.Descripton.Contains("[fakeData]")));
            _appDbContext.Posts.RemoveRange(_appDbContext.Posts.Where(c => c.Content.Contains("[fakeData]")));

            var fakerCategory = new Faker<Category>();
            int cm = 1;
            fakerCategory.RuleFor(c => c.Title, fk => $"CM{cm++}: " + fk.Lorem.Sentence(1, 2).Trim('.'));
            fakerCategory.RuleFor(c => c.Descripton, fk => fk.Lorem.Sentences(5) + "[fakeData]");
            fakerCategory.RuleFor(c => c.Slug, fk => fk.Lorem.Slug());

            var cate1 = fakerCategory.Generate();
            var cate11 = fakerCategory.Generate();
            var cate12 = fakerCategory.Generate();
            var cate2 = fakerCategory.Generate();
            var cate21 = fakerCategory.Generate();
            var cate211 = fakerCategory.Generate();

            cate11.ParentCategory = cate1;
            cate12.ParentCategory = cate1;
            cate21.ParentCategory = cate2;
            cate211.ParentCategory = cate21;

            var categories = new Category[] { cate1, cate2, cate12, cate11, cate21, cate211 };
            _appDbContext.Categories.AddRange(categories);

            //Fake Post
            var rCateInded = new Random();
            int bv = 1;

            var user = _userManager.GetUserAsync(this.User).Result;
            var fakePost = new Faker<Post>();
            fakePost.RuleFor(p => p.AuthorId, f => user.Id);
            fakePost.RuleFor(p => p.Content, f => f.Lorem.Paragraphs(7) + "[fakeData]");
            fakePost.RuleFor(p => p.DateCreated, f => f.Date.Between(new DateTime(2024, 1, 1), new DateTime(2024, 10, 10)));
            fakePost.RuleFor(p => p.Description, f => f.Lorem.Sentences(3));
            fakePost.RuleFor(p => p.Published, f => true);
            fakePost.RuleFor(p => p.Slug, f => f.Lorem.Slug());
            fakePost.RuleFor(p => p.Title, f => $"Bài {bv++} " + f.Lorem.Sentence(3, 4).Trim('.'));

            List<Post> posts = new List<Post>();
            List<PostCategory> postCategories = new List<PostCategory>();

            for (int i = 0; i < 40; i++)
            {
                var post = fakePost.Generate();
                post.DateUpdated = post.DateCreated;
                posts.Add(post);
                postCategories.Add(new PostCategory()
                {
                    Post = post,
                    Category = categories[rCateInded.Next(5)]
                });
            }
            _appDbContext.AddRange(posts);
            _appDbContext.AddRange(postCategories);


            _appDbContext.SaveChanges();

        }
    }
}
