using Microsoft.AspNetCore.Mvc;

namespace ProniaAB202.Areas.ProniaAdmin.Controllers
{
    [Area("ProniaAdmin")]   
    public class HomeController : Controller
    {
        public IActionResult Index()
        {
            return View();
        }
    }
}
