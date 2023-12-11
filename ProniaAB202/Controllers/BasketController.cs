using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Internal;
using Newtonsoft.Json;
using ProniaAB202.DAL;
using ProniaAB202.Intefaces;
using ProniaAB202.Models;
using ProniaAB202.ViewModels;
using System.Diagnostics.Metrics;
using System.Security.Claims;

namespace ProniaAB202.Controllers
{
    public class BasketController : Controller
    {
        public readonly AppDbContext _context;
        public readonly UserManager<AppUser> _userManager;
        private readonly IEmailService _emailService;

        public BasketController(AppDbContext context,UserManager<AppUser> userManager,IEmailService emailService)
        {
            _context = context;
            _userManager = userManager;
            _emailService = emailService;
        }

        public async Task<IActionResult> Index()
        {
            List<BasketItemVM> items = new List<BasketItemVM>();

            if(User.Identity.IsAuthenticated)
            {
                AppUser? user = await _userManager.Users
                    .Include(u => u.BasketItems.Where(bi => bi.OrederId == null))
                    .ThenInclude(bi => bi.Product)
                    .ThenInclude(p => p.ProductImages.Where(pi => pi.IsPrimary == true))
                    .FirstOrDefaultAsync(u => u.Id == User.FindFirstValue(ClaimTypes.NameIdentifier));

                foreach (BasketItem basketItem in user.BasketItems)
                {
                    items.Add(new BasketItemVM
                    {
                        Id=basketItem.ProductId,
                        Price=basketItem.Product.Price,
                        Count=basketItem.Count,
                        Name=basketItem.Product.Name,
                        SubTotal= basketItem.Count* basketItem.Product.Price,
                        Image=basketItem.Product.ProductImages.FirstOrDefault()?.Url
                    });
                }
            }
            else
            {
                if (Request.Cookies["Basket"] is not null)
                {
                    List<BasketCookieItemVM> cookies = JsonConvert.DeserializeObject<List<BasketCookieItemVM>>(Request.Cookies["Basket"]);

                    foreach (var cookie in cookies)
                    {
                        Product product = await _context.Products
                            .Include(p => p.ProductImages.Where(pi => pi.IsPrimary == true))
                            .FirstOrDefaultAsync(p => p.Id == cookie.Id);
                        if (product != null)
                        {
                            BasketItemVM item = new BasketItemVM
                            {
                                Id = product.Id,
                                Name = product.Name,
                                Image = product.ProductImages.FirstOrDefault().Url,
                                Price = product.Price,
                                Count = cookie.Count,
                                SubTotal = product.Price * cookie.Count,
                            };
                            items.Add(item);
                        }
                    }
                }
            }
            
            return View(items);
        }

        public async Task<IActionResult> AddBasket(int id)
        {
            if (id <= 0) return BadRequest();
            Product product = await _context.Products.FirstOrDefaultAsync(p => p.Id == id);
            if (product == null) return NotFound();

            if (User.Identity.IsAuthenticated)
            {
                // AppUser user = await _userManager.FindByNameAsync(User.Identity.Name);
               // _userManager.GetUserId()
               // AppUser user = await _userManager.Users.Include(u => u.BasketItems).FirstOrDefaultAsync(u=>u.UserName==User.Identity.Name);
                
                AppUser? user = await _userManager.Users
                    .Include(u => u.BasketItems.Where(bi => bi.OrederId == null))
                    .FirstOrDefaultAsync(u => u.Id==User.FindFirstValue(ClaimTypes.NameIdentifier));

                if(user == null) return NotFound();

               BasketItem item = user.BasketItems.FirstOrDefault(bi => bi.ProductId == product.Id);

                if(item is null)
                {
                    item = new BasketItem
                    {
                        AppUserId = user.Id,
                        ProductId = product.Id,
                        Count = 1,
                        Price = product.Price,
                    };  
                    user.BasketItems.Add(item);
                }
                else
                {
                    item.Count++;
                }

                await _context.SaveChangesAsync();

               
              
            }
            else
            {
                List<BasketCookieItemVM> basket;
                if (Request.Cookies["Basket"] is null)
                {
                    basket = new List<BasketCookieItemVM>();
                    BasketCookieItemVM item = new BasketCookieItemVM
                    {
                        Id = id,
                        Count = 1
                    };
                    basket.Add(item);
                }
                else
                {
                    basket = JsonConvert.DeserializeObject<List<BasketCookieItemVM>>(Request.Cookies["Basket"]);

                    BasketCookieItemVM existed = basket.FirstOrDefault(b => b.Id == id);

                    if (existed == null)
                    {
                        BasketCookieItemVM item = new BasketCookieItemVM
                        {
                            Id = id,
                            Count = 1
                        };
                        basket.Add(item);
                    }
                    else
                    {
                        existed.Count++;
                    }
                }



                string json = JsonConvert.SerializeObject(basket);

                Response.Cookies.Append("Basket", json);
            }


            
            return RedirectToAction(nameof(Index),"Home");
        }
        [Authorize(Roles ="Member")]
        public async Task<IActionResult> CheckOut()
        {
            AppUser user = await _userManager.Users
                .Include(u => u.BasketItems.Where(bi => bi.OrederId == null))
                .ThenInclude(bi => bi.Product)
                .FirstOrDefaultAsync(u => u.Id == User.FindFirstValue(ClaimTypes.NameIdentifier));

            OrderVM orderVM = new OrderVM
            {
                BasketItems=user.BasketItems
            };
            return View(orderVM);
        }
        [HttpPost]
        public async Task<IActionResult> CheckOut(OrderVM orderVM)
        {
            AppUser user = await _userManager.Users
               .Include(u => u.BasketItems.Where(bi=>bi.OrederId==null))
               .ThenInclude(bi => bi.Product)
               .FirstOrDefaultAsync(u => u.Id == User.FindFirstValue(ClaimTypes.NameIdentifier));
            if (!ModelState.IsValid)
            {
                orderVM.BasketItems = user.BasketItems;
                return View(orderVM);
            }

            decimal total = 0;
            foreach (var item in user.BasketItems)
            {
                item.Price = item.Product.Price;

                total += item.Price * item.Count;
            }

            Order order = new Order
            {
                Status = null,
                Address = orderVM.Address,
                AppUserId=user.Id,
                PurchasedAt= DateTime.Now,
                BasketItems=user.BasketItems,
                TotalPrice=total
            };



            await _context.Orders.AddAsync(order);
            await _context.SaveChangesAsync();

            string body = @"
                        Your order successfully placed:
                          <table border=""1"">
                           <thead>
                               <tr>
                                   <th> Name </th>
                                  <th> Price </th>
                                   <th> Count </th>
                               </tr>
                           </thead>
                           <tbody>";
            foreach (var item in order.BasketItems)
            {
                body += @$" <tr>
                              <td>{item.Product.Name}</td>
                              <td>{item.Price}</td>
                              <td>{item.Count}</td> 
                          </tr>";
            }
            body += @"</tbody>
                    </table>";


            await _emailService.SendEmailAsync(user.Email, "Your order", body, true);

            return RedirectToAction("Index","Home");
        }

    }
}

//<table>
//    <thead>
//        <tr>
//            <th> Name </th>
//            <th> Price </th>
//            <th> Count </th>
//        </tr>
//    </thead>
//    <tbody>
//        <tr>
//            <td></td>
//            <td></td>
//            <td></td>
//        </tr>
//    </tbody>
//</table>

