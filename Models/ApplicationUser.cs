using Microsoft.AspNetCore.Identity;

namespace EjustLostAndFoundHub.Models
{
    public class ApplicationUser : IdentityUser
    {
        public string NationalId { get; set; }
    }
}