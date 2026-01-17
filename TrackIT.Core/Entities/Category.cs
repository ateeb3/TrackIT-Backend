using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace TrackIT.Core.Entities
{
    public class Category : BaseEntity
    {
        [Required]
        [MaxLength(50)]
        public string Name { get; set; } = string.Empty;
        public string? Description { get; set; } = string.Empty;

        // ADD THIS property
        public bool IsDeleted { get; set; } = false;
        // Navigation Property: One Category has many Assets
        public ICollection<Asset> Assets { get; set; } = new List<Asset>();
    }
}