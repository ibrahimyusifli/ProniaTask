using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using ProniaAB202.DAL;
using ProniaAB202.Models;

namespace ProniaAB202.Areas.ProniaAdmin.Controllers
{
    [Area("ProniaAdmin")] //Yazmasaydiq sehife gorunmeyeseiydi
    
    public class SlideController1 : Controller
    {
        private readonly AppDbContext _context;
        public SlideController1(AppDbContext context)
        {
            _context = context;
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
        public async Task<IActionResult> Create(Slide slide)
        {
            if(slide.Photo is null)
            {
                ModelState.AddModelError("Photo","Shekil mutleq secilmelidir");
                return View();
            }

            if (!slide.Photo.ContentType.Contains("image/"))
            {
                ModelState.AddModelError("Photo", "File tipi uygun deyil");
                return View();
            }

            if (slide.Photo.Length>2*1024*1024)
            {
                ModelState.AddModelError("Photo", "File olcusu 2 mb dan boyuk ola bilmez");
                return View();
            }

            FileStream file = new FileStream(@"C:\Users\Sheki Komp\Desktop\Task22.11.23\ProniaTask\ProniaAB202\wwwroot\assets\images\slider\" + slide.Photo.FileName, FileMode.Create);
            await slide.Photo.CopyToAsync(file);
            slide.Image = slide.Photo.FileName;


           

            //return Content(slide.Photo.FileName + " " + slide.Photo.ContentType + " " + slide.Photo.Length);
            await _context.Slides.AddAsync(slide);
            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }
    }
}

