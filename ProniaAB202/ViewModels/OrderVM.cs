using ProniaAB202.Models;

namespace ProniaAB202.ViewModels
{
    public class OrderVM
    {
        public string Address { get; set; }
        public List<BasketItem>? BasketItems { get; set; }
    }
}
