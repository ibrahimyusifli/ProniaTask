using System.ComponentModel.DataAnnotations;

namespace ProniaAB202.ViewModels
{

    public class LoginVM
    {
        [Required]

        public string UsernameOrEmail { get; set; }
        [Required]
        [MinLength(8)]
        [DataType(DataType.Password)]
        public string Password { get; set; }
        public bool IsRemembered { get; set; }
    }
}
