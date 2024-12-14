using System.Net;
using App.Data;
using App.ExtendMethods;
using App.Models;
using App.Services;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc.ModelBinding.Binders;
using Microsoft.AspNetCore.Mvc.Razor;
using Microsoft.AspNetCore.Routing.Constraints;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.FileProviders;
#nullable disable

var builder = WebApplication.CreateBuilder(args);
var services = builder.Services;
var Configuration = builder.Configuration;


services.AddDbContext<AppDbContext>(option =>
{
    string connectString = Configuration.GetConnectionString("AppMvcConnectionString");
    option.UseSqlServer(connectString);
});
// Add services to the container.
services.AddControllersWithViews();

services.AddRazorPages();

//Dang ky Identity
services.AddIdentity<AppUser, IdentityRole>()
    .AddEntityFrameworkStores<AppDbContext>()
    .AddDefaultTokenProviders();


// Truy cập IdentityOptions
services.Configure<IdentityOptions>(options =>
{
    // Thiết lập về Password
    options.Password.RequireDigit = false; // Không bắt phải có số
    options.Password.RequireLowercase = false; // Không bắt phải có chữ thường
    options.Password.RequireNonAlphanumeric = false; // Không bắt ký tự đặc biệt
    options.Password.RequireUppercase = false; // Không bắt buộc chữ in
    options.Password.RequiredLength = 3; // Số ký tự tối thiểu của password
    options.Password.RequiredUniqueChars = 1; // Số ký tự riêng biệt

    // Cấu hình Lockout - khóa user
    options.Lockout.DefaultLockoutTimeSpan = TimeSpan.FromMinutes(5); // Khóa 5 phút
    options.Lockout.MaxFailedAccessAttempts = 5; // Thất bại 5 lầ thì khóa
    options.Lockout.AllowedForNewUsers = true;

    // Cấu hình về User.
    options.User.AllowedUserNameCharacters = // các ký tự đặt tên user
        "abcdefghijklmnopqrstuvwxyzABCDEFGHIJKLMNOPQRSTUVWXYZ0123456789-._@+";
    options.User.RequireUniqueEmail = true;  // Email là duy nhất

    // Cấu hình đăng nhập.
    options.SignIn.RequireConfirmedEmail = true;            // Cấu hình xác thực địa chỉ email (email phải tồn tại)
    options.SignIn.RequireConfirmedPhoneNumber = false;     // Xác thực số điện thoại
    options.SignIn.RequireConfirmedAccount = true;         // Xác thực confirm email trước khi đăng nhập

});


// //IEmailSender
// services.AddOptions();
// var mailSetting = Configuration.GetSection("MailSettings");
// services.Configure<MailSettings>(mailSetting);
// services.AddTransient<IEmailSender, SendMailService>();

services.ConfigureApplicationCookie(option =>
{
    option.LoginPath = "/login/";
    option.LogoutPath = "/logout/";
    option.AccessDeniedPath = "/Identity/Account/AccessDenied";
});

//Đăng nhập bằng google
services.AddAuthentication()
        .AddGoogle(option =>
        {
            IConfigurationSection gconfig = Configuration.GetSection("Authentication:Google");
            option.ClientId = gconfig["ClientId"];
            option.ClientSecret = gconfig["ClientSecret"];
            //Nếu không config CallbackPath thì đường dẫn mặc định là http://localhost:5279/signin-google
            option.CallbackPath = "/dang-nhap-google";
        })
        // .AddFacebook()
        // .AddTwitter()
        ;


// services.AddSingleton<ProductService>();
// services.AddSingleton<ProductService, ProductService>();
// services.AddSingleton(typeof(ProductService));
services.AddSingleton(typeof(ProductService), typeof(ProductService));

services.AddSingleton<PlanetService>();

//Cấu hình việc tự động tìm file View của Controller
services.Configure<RazorViewEngineOptions>(option =>
{
    /*default: /View/ControllerName/ActionName.cshtml
    {0}: Tên action
    {1}: Tên controller
    {2}: Tên areas
    */
    option.ViewLocationFormats.Add("/MyView/{1}/{0}" + RazorViewEngine.ViewExtension);// hoặc /MyView/{1}/{0}.cshtml
});

//thay thông báo lỗi của IdentityErrorDescriber
services.AddSingleton<IdentityErrorDescriber, AppIdentityErrorDescriber>();

//IEmailSender
services.AddOptions();
var mailSetting = Configuration.GetSection("MailSettings");
services.Configure<MailSettings>(mailSetting);
services.AddTransient<IEmailSender, SendMailService>();

services.AddAuthorization(option =>
{
    option.AddPolicy("ViewManageMenu", builder =>
    {
        builder.RequireAuthenticatedUser();
        builder.RequireRole(RoleName.Administrator);
    });
});

var app = builder.Build();

// Configure the HTTP request pipeline.
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Home/Error");
    // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
    app.UseHsts();
}

app.UseHttpsRedirection();


app.UseStaticFiles();
//Thay đổi đường dẫn file mặc định không dung wwwwroot nữa thay vào đó là Uploads
app.UseStaticFiles(new StaticFileOptions()
{
    FileProvider = new PhysicalFileProvider(
        Path.Combine(Directory.GetCurrentDirectory(), "Uploads")
    ),
    RequestPath = "/contents" //content/1.jpg => /Uploads/1.jpg
});

app.AddStatusCodePage();

app.UseRouting();

app.UseAuthentication();
app.UseAuthorization();

app.MapRazorPages();

//Ví dụ constraint
app.MapControllerRoute(
    name: "first",
    pattern: "{url:regex(^((xemsanpham)|(viewproduct))$)}/{id:range(2,4)?}",
    //"xemsanpham/{id?} khi đặt là {url} thì sẽ không vào được đến middleware name: "default"
    //Do khi nhập chỉ số thứ 2 sẽ gán nhầm vào {url} chứ không được gán vào controller của name: "default"
    defaults: new
    {
        controller = "First",
        action = "ViewProduct"
    },
    constraints: new
    {
        // url = new RegexRouteConstraint(@"^((xemsanpham)|(viewproduct))$"), //"xemsanpham", // new StringRouteConstraint("xemsanpham"),
        // id = new RangeRouteConstraint(2, 4) // ràng buộc id chỉ nhận giá trị từ 2 -4 
    }
);

app.MapAreaControllerRoute(
    name: "product",
    areaName: "ProductManage",
    pattern: "{controller}/{action=Index}/{id?}"
);

app.MapControllerRoute(
    name: "firstroute",
    pattern: "start-here/{controller}/{action}/{id?}"
// defaults: new
// {
//     controller = "First",
//     action = "ViewProduct",
//     id = 3
// }
);

app.MapGet("/sayhi", async context =>
{
    await context.Response.WriteAsync($"Hello Asp.Net {DateTime.Now}");
});

app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=Index}/{id?}"
);







app.Run();