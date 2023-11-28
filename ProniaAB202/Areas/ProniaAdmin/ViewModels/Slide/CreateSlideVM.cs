using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace ProniaAB202.Areas.ProniaAdmin.ViewModels
{
    public class CreateSlideVM
    {

        [Required]
        public string Title { get; set; }

        public string Subtitle { get; set; }

        public string Description { get; set; }
        public int Order { get; set; }
    
        [Required]
        public IFormFile Photo { get; set; }
    }
}
