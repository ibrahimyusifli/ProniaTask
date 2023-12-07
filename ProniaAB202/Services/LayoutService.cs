using Azure.Core;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Newtonsoft.Json;
using ProniaAB202.DAL;
using ProniaAB202.Models;
using ProniaAB202.ViewModels;
using System.Security.Claims;

namespace ProniaAB202.Services
{
    public class LayoutService
    {
        private readonly AppDbContext _context;
        private readonly IHttpContextAccessor _http;
        private readonly UserManager<AppUser> _userManager;
        public LayoutService(AppDbContext context,IHttpContextAccessor http,UserManager<AppUser> userManager)
        {
            _context = context;
            _http = http;
            _userManager = userManager;
        }
        public async Task<Dictionary<string,string>> GetSettingsAsync()
        {
            Dictionary<string, string> settings = await _context.Settings.ToDictionaryAsync(s => s.Key, s => s.Value);

            return settings;
        }
        public async Task<List<BasketItemVM>> GetBasketsAsync()
        {
            List<BasketItemVM> items = new List<BasketItemVM>();

            if (_http.HttpContext.User.Identity.IsAuthenticated)
            {
                AppUser? user = await _userManager.Users
                   .Include(u => u.BasketItems)
                   .ThenInclude(bi => bi.Product)
                   .ThenInclude(p => p.ProductImages.Where(pi => pi.IsPrimary == true))
                   .FirstOrDefaultAsync(u => u.Id == _http.HttpContext.User.FindFirstValue(ClaimTypes.NameIdentifier));

                foreach (BasketItem basketItem in user.BasketItems)
                {
                    items.Add(new BasketItemVM
                    {
                        Id = basketItem.ProductId,
                        Price = basketItem.Product.Price,
                        Count = basketItem.Count,
                        Name = basketItem.Product.Name,
                        SubTotal = basketItem.Count * basketItem.Product.Price,
                        Image = basketItem.Product.ProductImages.FirstOrDefault()?.Url
                    });
                }
            }
            else
            {
                if (_http.HttpContext.Request.Cookies["Basket"] is not null)
                {
                    List<BasketCookieItemVM> cookies = JsonConvert.DeserializeObject<List<BasketCookieItemVM>>(_http.HttpContext.Request.Cookies["Basket"]);

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
           
            return items;
        }
    }
}
