using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using ProniaAB202.Areas.ProniaAdmin.ViewModels;
using ProniaAB202.DAL;
using ProniaAB202.Models;

namespace ProniaAB202.Areas.ProniaAdmin.Controllers
{
    [Area("ProniaAdmin")]
    public class ProductController : Controller
    {
        private readonly AppDbContext _context;
        public ProductController(AppDbContext context)
        {
            _context = context;
        }
        public async Task<IActionResult> Index()
        {
            List<Product> products=await _context.Products
                .Include(p=>p.Category)
                .Include(p=>p.ProductImages.Where(pi=>pi.IsPrimary==true))
                .ToListAsync();
            return View(products);
        }

        public async Task<IActionResult> Create()
        {
            ViewBag.Categories = await _context.Categories.ToListAsync();    
            return View();
        }
        [HttpPost]
        public async Task<IActionResult> Create(CreateProductVM productVM)
        {
            
            if (!ModelState.IsValid)
            {
                ViewBag.Categories = await _context.Categories.ToListAsync();
                return View();
            }
            bool result = await _context.Categories.AnyAsync(c=>c.Id==productVM.CategoryId);
            if (!result)
            {
                ViewBag.Categories = await _context.Categories.ToListAsync();
                ModelState.AddModelError("CategoryId", "Bu id li category movcud deyil");
                return View();
            }

            Product product = new Product
            {
                SKU = productVM.SKU,
                Description = productVM.Description,
                Name = productVM.Name,
                Price = productVM.Price,
                CategoryId = (int)productVM.CategoryId,
            };
            await _context.Products.AddAsync(product);
            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));

        }
    }
}
