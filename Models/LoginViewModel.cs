using System.ComponentModel.DataAnnotations;

namespace EjustLostAndFoundHub.Models
{
    public class LoginViewModel
    {
        [Required(ErrorMessage = "National ID or Passport Number is required")]
        [Display(Name = "National ID / Passport Number")]
        public string NationalId { get; set; }  
    }
}
