using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Http;
using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel.DataAnnotations;

namespace EjustLostAndFoundHub.Models
{

    public abstract class ItemModel
    {
        // Universal properties for all items
        [Key]
        public int Id { get; set; }
        public Guid PublicId { get; set; } = Guid.NewGuid();
        public string? UserId { get; set; }
        [Required]
        public string? Category { get; set; }
        public DateTime DateReported { get; set; }
        public string? ContactNumber { get; set; }
        public string? ContactEmail { get; set; }

        [Required(ErrorMessage = "You must specify where the item was found.")]
        public string? LocationFound { get; set; }
        public string? ItemLocation { get; set; }
        [NotMapped]
        public IFormFile? ItemPhoto { get; set; }
        public string? PhotoPath { get; set; }
        public string Status { get; set; } = "Active";
    }
}
