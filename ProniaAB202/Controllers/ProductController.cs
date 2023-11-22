using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using ProniaAB202.DAL;
using ProniaAB202.Models;

namespace ProniaAB202.Controllers
{
    public class ProductController : Controller
    {

        private readonly AppDbContext _context;
        public ProductController(AppDbContext context)
        {
            _context = context;
        }

        //public IActionResult Index()
        //{
        //    return View();
        //}

        public async Task<IActionResult> Detail(int id)
        {
            if (id <= 0) return BadRequest();
           
            Product product =await _context.Products.Include(p=>p.Category).FirstOrDefaultAsync(p => p.Id == id);
            

            if (product == null) return NotFound();
           


            return View(product);
        }
    }
}
