using App.Services;
using Microsoft.AspNetCore.Mvc;
#nullable disable

namespace App.Controllers
{
    public class FirstController : Controller
    {
        private readonly ILogger<FirstController> _logger;
        private readonly IWebHostEnvironment _env;
        private readonly ProductService _productService;
        public FirstController(ILogger<FirstController> logger, IWebHostEnvironment env, ProductService productService)
        {
            _logger = logger;
            _env = env;
            _productService = productService;
        }
        public string Index() => "FirstController/Index";

        public IActionResult Bird()
        {
            string filePath = Path.Combine(_env.ContentRootPath, "Files", "image.png");
            var bytes = System.IO.File.ReadAllBytes(filePath);

            return File(bytes, "image/png");
        }

        public IActionResult IphonePrice()
        {
            return Json(
                new
                {
                    ProductName = "Iphone X",
                    Price = 1000
                }
            );
        }

        public IActionResult Privacy()
        {
            var url = Url.Action("Privacy", "Home");
            return LocalRedirect(url);//gọi đến 1 url trong local
        }

        public IActionResult Google()
        {
            var url = "https://google.com";
            return Redirect(url);
        }

        public IActionResult HelloView(string username)
        {
            if (string.IsNullOrEmpty(username))
                username = "Khach";
            // return View("/MyView/Xinchao.cshtml");
            // return View("/MyView/Xinchao.cshtml", username);
            // return View("Xinchao2", username);//lấy Xinchao2.cshtml trong /View/First/Xinchao2.cshtml
            return View((object)username); //lấy HelloView trong  /View/First/HelloView.cshtml
        }
        [TempData]
        public string StatusMessage { get; set; }

        [AcceptVerbs("POST", "GET")]//cho phép các kết nối post get
        public IActionResult ViewProduct(int? id)
        {
            var product = _productService.FirstOrDefault(p => p.Id == id);
            if (product == null)
            {
                //TempData được lưu trong session và bị xóa luốn ngay lần đọc đầu tiên
                // TempData["StatusMessage"] = "Sản phẩm bạn yêu cầu không có";
                StatusMessage = "Sản phẩm bạn yêu cầu không có";
                return Redirect(Url.Action("Index", "Home"));
                // return NotFound();
            }

            ViewData["Title"] = product.Name;//Dữ liệu chuyển đến layout
                                             //truyền product qua view bằng model
                                             // return View(product);

            //truyền product qua view bằng ViewData
            // this.ViewData["product"] = product;

            // return View("ViewProduct2");

            //truyền product qua view bằng ViewBag
            ViewBag.product = product;
            return View("ViewProduct3");
        }
    }
}