using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using ProniaAB202.DAL;
using ProniaAB202.Models;
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


        public IActionResult Index()
        {




            //_context.Slides.AddRange(slides);
            //_context.SaveChanges();

           List<Slide> slides= _context.Slides.OrderBy(s=>s.Order).Take(2).ToList();
           List<Product> products= _context.Products.Include(p=>p.ProductImages).ToList();

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
    }
}
