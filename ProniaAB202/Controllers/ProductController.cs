using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using ProniaAB202.DAL;
using ProniaAB202.Models;
using ProniaAB202.ViewModels;

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
           
            Product product =await _context.Products
                .Include(p=>p.Category)
                .Include(p=>p.ProductImages)
                 .Include(p => p.ProductTags)
                 .ThenInclude(pt=>pt.Tag)
                .FirstOrDefaultAsync(p => p.Id == id);

            if (product == null) return NotFound();

            DetailVM detailVM = new DetailVM
            {
                Product = product,
                RelatedProducts = await _context.Products
                .Where(p => p.CategoryId == product.CategoryId && p.Id != product.Id)
                .Take(12)
                .Include(p => p.ProductImages.Where(pi => pi.IsPrimary != null))
                .ToListAsync()
            };
           


            return View(detailVM);
        }
    }
}
