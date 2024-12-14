using App.Services;
using Microsoft.AspNetCore.Mvc;
#nullable disable

namespace App.Controllers
{
    [Route("he-mat-troi/[action]")]
    //Nếu đã thiết lập route cho controller thì phải thiết lập route cho tất cả action
    //Nếu không thì phải thiết lập /[action]
    // [HttpGet] [HttpPost] tương tự như route

    public class PlanetController : Controller
    {
        private readonly PlanetService _planetService;
        private readonly ILogger<PlanetController> _logger;

        public PlanetController(PlanetService planetService, ILogger<PlanetController> logger)
        {
            _planetService = planetService;
            _logger = logger;
        }

        // GET: Planet
        public IActionResult Index()
        {
            return View();
        }

        [BindProperty(SupportsGet = true, Name = "action")]
        public string Name { get; set; }//action ~ planetmodel
        public IActionResult Mercury()
        {
            var planet = _planetService.FirstOrDefault(p => p.Name == Name);
            return View("Detail", planet);
        }

        [Route("sao/[controller]/[action]", Order = 2, Name = "TraiDat1")] //sao/Planet/Earth
        [Route("[controller]-[action].html", Order = 1, Name = "TraiDat2")] //Planet-Earth.html
        //có thể khai báo nhiều route cho 1 action
        public IActionResult Earth()
        {
            var planet = _planetService.FirstOrDefault(p => p.Name == Name);
            return View("Detail", planet);
        }

        [Route("hanhtinh/{id:int}")]
        public IActionResult PlanetInfor(int? id)
        {
            var planet = _planetService.FirstOrDefault(p => p.Id == id);
            return View("Detail", planet);
        }

    }
}
