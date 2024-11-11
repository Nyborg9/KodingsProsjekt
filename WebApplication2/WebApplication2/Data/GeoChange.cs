using System.ComponentModel.DataAnnotations;

namespace WebApplication2.Data
{
    public class GeoChange
    {
        public int Id { get; set; }

        [Required]
        public string? Description { get; set; }

        [Required]
        public string? GeoJson { get; set; }

        [Required]
        public string UserId { get; set; } // Foreign key to associate with a user

        // Navigation property
        public virtual ApplicationUser User { get; set; }
    }
}