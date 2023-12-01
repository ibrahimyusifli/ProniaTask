using ProniaAB202.Models;

namespace ProniaAB202.ViewModels
{
    public class DetailVM
    {
        public Product Product { get; set; }
        public List<Product> RelatedProducts { get; set; }
    }
}
