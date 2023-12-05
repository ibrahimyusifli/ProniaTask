using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using ProniaAB202.Models;
using ProniaAB202.ViewModels;

namespace ProniaAB202.Controllers
{
    public class AccountController : Controller
    {
        private readonly UserManager<AppUser> _userManager;
        private readonly SignInManager<AppUser> _signInManager;
        public AccountController(UserManager<AppUser> userManager,SignInManager<AppUser> signInManager)
        {
            _userManager = userManager;
            _signInManager = signInManager;
        }


        public IActionResult Register()
        {
            return View();
        }
        [HttpPost]
        public async Task<IActionResult> Register(RegisterVM UserVM)
        {
            if(!ModelState.IsValid) return View();

            AppUser user = new AppUser
            {
                Name = UserVM.Name,
                Surname = UserVM.Surname,
                Email = UserVM.Email,
                UserName = UserVM.Name,
            };


            IdentityResult result =await _userManager.CreateAsync(user, UserVM.Password);

            if (result.Succeeded)
            {
                foreach (IdentityError error in result.Errors)
                {
                    ModelState.AddModelError(String.Empty, error.Description);
                }
                return View();
            }

           await _signInManager.SignInAsync(user,false);

            return RedirectToAction("Index","Home");
        }
        public async Task<IActionResult> Logout()
        {
            await _signInManager.SignOutAsync();
            return RedirectToAction("Index", "Home");
        }
    }
}
