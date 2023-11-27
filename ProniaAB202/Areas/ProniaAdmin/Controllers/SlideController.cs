using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using ProniaAB202.Areas.ProniaAdmin.ViewModels;
using ProniaAB202.DAL;
using ProniaAB202.Models;
using ProniaAB202.Utilities.Extentions;

namespace ProniaAB202.Areas.ProniaAdmin.Controllers
{
    [Area("ProniaAdmin")] //Yazmasaydiq sehife gorunmeyeseiydi
    
    public class SlideController : Controller
    {
        private readonly AppDbContext _context;
        private readonly IWebHostEnvironment _env;
        public SlideController(AppDbContext context, IWebHostEnvironment env)
        {
            _context = context;
            _env = env;
        }
        public async Task<IActionResult> Index()
        {
            List<Slide> slides = await _context.Slides.ToListAsync();

            return View(slides);
        }
        public IActionResult Create()
        {
            return View();
        }
        [HttpPost]
        public async Task<IActionResult> Create(CreateSlideVM slideVM)
        {
           
            //if(slideVM.Photo is null)
            //{
            //    ModelState.AddModelError("Photo","Shekil mutleq secilmelidir");
            //    return View();
            //}
            if (!ModelState.IsValid)
            {
                return View();
            }

            if (!slideVM.Photo.ValidateType("image/"))
            {
                ModelState.AddModelError("Photo", "File tipi uygun deyil");
                return View();
            }

            if (!slideVM.Photo.ValidateSize(2*1024))
            {
                ModelState.AddModelError("Photo", "File olcusu 2 mb dan boyuk ola bilmez");
                return View();
            }



          string fileName =await  slideVM.Photo.CreateFileAsync(_env.WebRootPath, "assets", "image", "slider");

            Slide slide = new Slide
            {
                Image=fileName,
                Title=slideVM.Title,
                Subtitle=slideVM.Subtitle,
                Description=slideVM.Description,
                Order=slideVM.Order
            };
            await _context.Slides.AddAsync(slide);
            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }

        public async Task<IActionResult> Update(int id)
        {
            if (id <= 0) return BadRequest();

            Slide existed = await _context.Slides.FirstOrDefaultAsync(s => s.Id == id);

            if (existed == null) return NotFound();

            UpdateSlideVM slideVM = new UpdateSlideVM
            {
                Description = existed.Description,
                Subtitle = existed.Subtitle,
                Order = existed.Order,
                Image = existed.Image,
                Title = existed.Title
            };

            return View(slideVM);

        }
        [HttpPost]
        public async Task<IActionResult> Update(int id,UpdateSlideVM slideVM)
        {
          

            if(!ModelState.IsValid)
            {
                return View(slideVM);
            }
            Slide existed = _context.Slides.FirstOrDefault(s => s.Id == id);
            if (existed == null) return NotFound();

            if (slideVM.Photo is not null)
            {
                if (!slideVM.Photo.ValidateType("image/"))
                {
                    ModelState.AddModelError("Photo", "File tipi uygun deyil");
                    return View(slideVM);
                }

                if (!slideVM.Photo.ValidateSize(2 * 1024))
                {
                    ModelState.AddModelError("Photo", "File olcusu 2 mb dan boyuk ola bilmez");
                    return View(slideVM);
                }
                string fileName =await slideVM.Photo.CreateFileAsync(_env.WebRootPath,"assets","images","slider");
                existed.Image.DeleteFile(_env.WebRootPath, "assets", "images", "slider");

                existed.Image = fileName;
            }

            existed.Title = slideVM.Title;
            existed.Subtitle = slideVM.Subtitle;
            existed.Order = slideVM.Order;
            existed.Description = slideVM.Description;
            await _context.SaveChangesAsync();

            return RedirectToAction(nameof(Index));
        }
        

        public async Task<IActionResult> Delete(int id)
        {
            if (id <= 0) return BadRequest();

            Slide existed = await _context.Slides.FirstOrDefaultAsync(s => s.Id == id);
            if (existed == null) return NotFound();


            existed.Image.DeleteFile(_env.WebRootPath,"assets","images","slider");
            

            _context.Slides.Remove(existed);

            await _context.SaveChangesAsync();

            return RedirectToAction(nameof(Index));
          
        }
    }
}

