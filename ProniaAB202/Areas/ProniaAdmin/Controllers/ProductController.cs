using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using ProniaAB202.Areas.ProniaAdmin.ViewModels;
using ProniaAB202.DAL;
using ProniaAB202.Models;
using ProniaAB202.Utilities.Extentions;

namespace ProniaAB202.Areas.ProniaAdmin.Controllers
{
    [Area("ProniaAdmin")]
    public class ProductController : Controller
    {
        private readonly AppDbContext _context;
        private readonly IWebHostEnvironment _env;
        public ProductController(AppDbContext context,IWebHostEnvironment env)
        {
            _context = context;
            _env = env;
        }
        public async Task<IActionResult> Index()
        {
            List<Product> products=await _context.Products
                .Include(p=>p.Category)
                .Include(p=>p.ProductImages.Where(pi=>pi.IsPrimary==true))
                .Include(p=>p.ProductTags)
                .ThenInclude(pt=>pt.Tag)
                .ToListAsync();
            return View(products);
        }

        public async Task<IActionResult> Create()
        {
            ViewBag.Categories = await _context.Categories.ToListAsync(); 
            ViewBag.Tags = await _context.Tags.ToListAsync();
            return View();
        }
        [HttpPost]
        public async Task<IActionResult> Create(CreateProductVM productVM)
        {
            
            if (!ModelState.IsValid)
            {
                ViewBag.Categories = await _context.Categories.ToListAsync();
                ViewBag.Tags = await _context.Tags.ToListAsync();
                return View();
            }
            bool result = await _context.Categories.AnyAsync(c=>c.Id==productVM.CategoryId);
            if (!result)
            {
                ViewBag.Categories = await _context.Categories.ToListAsync();
                ViewBag.Tags = await _context.Tags.ToListAsync();
                ModelState.AddModelError("CategoryId", $"Bu id li category movcud deyil");
                return View();
            }

            foreach (int tagId in productVM.TagIds)
            {
                bool tagResult = await _context.Tags.AllAsync(t => t.Id == tagId);
                if (!tagResult)
                {
                    ViewBag.Categories = await _context.Categories.ToListAsync();
                    ViewBag.Tags = await _context.Tags.ToListAsync();
                    ModelState.AddModelError("TagIds", "Tag melumatlari sehv dxil edilib");
                    return View();
                }
            }


            if (!productVM.MainPhoto.ValidateType("image/"))
            {
                ViewBag.Categories = await _context.Categories.ToListAsync();
                ViewBag.Tags = await _context.Tags.ToListAsync();
                ModelState.AddModelError("MainPhoto", "File tipi uygun deyil");
                return View();
            }

            if (!productVM.MainPhoto.ValidateSize(600))
            {
                ViewBag.Categories = await _context.Categories.ToListAsync();
                ViewBag.Tags = await _context.Tags.ToListAsync();
                ModelState.AddModelError("MainPhoto", "File olcusu uygun deyil");
                return View();
            }

            if (!productVM.HoverPhoto.ValidateType("image/"))
            {
                ViewBag.Categories = await _context.Categories.ToListAsync();
                ViewBag.Tags = await _context.Tags.ToListAsync();
                ModelState.AddModelError("HoverPhoto", "File tipi uygun deyil");
                return View();
            }

            if (!productVM.HoverPhoto.ValidateSize(600))
            {
                ViewBag.Categories = await _context.Categories.ToListAsync();
                ViewBag.Tags = await _context.Tags.ToListAsync();
                ModelState.AddModelError("HoverPhoto", "File olcusu uygun deyil");
                return View();
            }


            ProductImage main = new ProductImage
            {
                IsPrimary = true,
                Url=await productVM.MainPhoto.CreateFileAsync(_env.WebRootPath,"assets","images","website-images"),
                Alternative=productVM.Name
            };

            ProductImage hover = new ProductImage
            {
                IsPrimary = false,
                Url = await productVM.HoverPhoto.CreateFileAsync(_env.WebRootPath, "assets", "images", "website-images"),
                Alternative = productVM.Name
            };


            Product product = new Product
            {
                SKU = productVM.SKU,
                Description = productVM.Description,
                Name = productVM.Name,
                Price = productVM.Price,
                CategoryId = (int)productVM.CategoryId,
                ProductTags=new List<ProductTag>(),
                ProductImages=new List<ProductImage> { main, hover }
            };


            TempData["Message"] = "";
            foreach (IFormFile photo in productVM.Photos  ?? new List<IFormFile>())
            {
                if (!photo.ValidateType("image/"))
                {
                    TempData["Message"] += $" <p class=\"text-danger\">{photo.FileName} sdli file tipi uygun deyil</p>";
                    continue;
                }
                if (!photo.ValidateSize(600))
                {
                    TempData["Message"] += $"<p class=\"text-danger\">{photo.FileName} sdli file olcusu uygun deyil</p>";
                    continue;
                }
                product.ProductImages.Add(new ProductImage
                {
                    IsPrimary=null,
                    Alternative=productVM.Name,
                    Url = await photo.CreateFileAsync(_env.WebRootPath,"assets","images","website-images")
                });   

            }

           
            foreach (int tagId in productVM.TagIds)
            {
                ProductTag productTag = new ProductTag
                {
                    TagId = tagId                  
                };
                product.ProductTags.Add(productTag);
               
            }
            
                
            await _context.Products.AddAsync(product);
            await _context.SaveChangesAsync();

            return RedirectToAction(nameof(Index));

        }


        public async Task<IActionResult> Update(int id)
        {
            if (id<=0) return BadRequest();            
            Product product = await _context.Products.Include(p=>p.ProductTags).FirstOrDefaultAsync(p=>p.Id==id); 
            if (product != null) return NotFound();

            UpdateProductVM productVM = new UpdateProductVM
            {
                Name = product.Name,
                Price = product.Price,
                Description = product.Description,
                SKU = product.SKU,
                CategoryId= product.CategoryId,
                TagIds=product.ProductTags.Select(pt=>pt.TagId).ToList(),
                Categories = await _context.Categories.ToListAsync(),
                Tags = await _context.Tags.ToListAsync()
            };

            return View(productVM);
        }
        [HttpPost]
        public async Task<IActionResult> Update(int id, UpdateProductVM productVM)
        {
            if (!ModelState.IsValid)
            {
                productVM.Categories = await _context.Categories.ToListAsync();
                productVM.Tags= await _context.Tags.ToListAsync();
                return View(productVM);
            }
            Product existed=await _context.Products.Include(p=>p.ProductTags).FirstOrDefaultAsync(p=>p.Id==id);
            if (existed != null) return NotFound();

            bool result = await _context.Categories.AnyAsync(c => c.Id==productVM.CategoryId);
            if (!result)
            {
                productVM.Categories = await _context.Categories.ToListAsync();
                productVM.Tags = await _context.Tags.ToListAsync();
                ModelState.AddModelError("CategoryId", "Bele bir category movcud deyil");
                return View(productVM);
            }

            //List<ProductTag> removeable = existed.ProductTags.Where(pt=>!productVM.TagIds.Exists(tId=>tId==pt.TagId)).ToList();
            //_context.ProductTags.RemoveRange(removeable);


            existed.ProductTags.RemoveAll(pt => !productVM.TagIds.Exists(tId => tId == pt.TagId));

            //foreach (ProductTag pTag in existed.ProductTags)
            //{
            //    if (!productVM.TagIds.Exists(tId => tId == pTag.TagId))
            //    {
            //        _context.ProductTags.Remove(pTag);
            //    }
            //}


            List<int> creatable = productVM.TagIds.Where(tId=>!existed.ProductTags.Exists(pt=>pt.TagId==tId)).ToList();
            foreach (int tId in creatable)
            {
                bool tagResult = await _context.Tags.AllAsync(t => t.Id == tId);
                if (!tagResult)
                {
                    productVM.Categories = await _context.Categories.ToListAsync();
                    productVM.Tags = await _context.Tags.ToListAsync();
                    ModelState.AddModelError("TagIds", "Bele bir Tag movcud deyil");    
                    return View(productVM);
                }
                existed.ProductTags.Add(new ProductTag
                {
                    TagId = tId,
                });
            }





            //foreach (int tId in productVM.TagIds)
            //{
            //    if (!existed.ProductTags.Any(pt=>pt.TagId==tId))
            //    {                  
            //        existed.ProductTags.Add(new ProductTag
            //        {
            //            TagId = tId
            //        });
            //    }
            //}

            existed.Name = productVM.Name;
            existed.Description = productVM.Description;
            existed.Price = productVM.Price;
            existed.SKU = productVM.SKU;
            existed.CategoryId = productVM.CategoryId;

            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }
    }
}
