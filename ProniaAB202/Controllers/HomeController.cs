using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using ProniaAB202.DAL;
using ProniaAB202.Models;
using ProniaAB202.Services;
using ProniaAB202.ViewModels;

namespace ProniaAB202.Controllers
{
    public class HomeController : Controller
    {
        private readonly AppDbContext _context;
      

        public HomeController(AppDbContext context)
        {
            _context = context;
        }


        public async Task<IActionResult> Index()
        {
 
           
            //_context.Slides.AddRange(slides);
            //_context.SaveChanges();

           List<Slide> slides=await _context.Slides.OrderBy(s=>s.Order).Take(2).ToListAsync();
           List<Product> products=await _context.Products
                .Include(p=>p.ProductImages .Where(pi=>pi.IsPrimary!=null))
                .ToListAsync();

            HomeVM home = new HomeVM
            {
                Slides = slides,
                Products = products,
            };


            return View(home);
        }
        public IActionResult About()
        {
            return View();
        }

        public IActionResult ErrorPage(string error="xeta bas verdi")
        {
            return View(model:error);
        }

        //public ActionResult Test()
        //{
        //    Response.Cookies.Append("Score", "5-0",new CookieOptions
        //    {
        //        MaxAge=TimeSpan.FromSeconds(10)
        //    });


        //    HttpContext.Session.SetString("Score2","1-0");
           
        //    return Ok();
        //}

        //public IActionResult GetCookie()
        //{
        //    string score = Request.Cookies["Score"];

        //    string score2 = HttpContext.Session.GetString("Score2");
        //    return Content(score+" "+score2);
        //}
    }
}   
