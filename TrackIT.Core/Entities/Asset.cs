using System;
using System.ComponentModel.DataAnnotations;
using TrackIT.Core.Enums;

namespace TrackIT.Core.Entities
{
    public class Asset : BaseEntity
    {
        [Required]
        [MaxLength(100)]
        public string Name { get; set; } = string.Empty; // e.g., "MacBook Pro 16"

        [Required]
        [MaxLength(50)]
        public string SerialNumber { get; set; } = string.Empty;

        public AssetStatus Status { get; set; } = AssetStatus.Available;

        public DateTime PurchaseDate { get; set; }

        // Foreign Key: Category
        public int CategoryId { get; set; }
        public Category? Category { get; set; }
        public bool IsDeleted { get; set; } = false;
        // Foreign Key: Current Employee (Optional - null if unassigned)
        public string? UserId { get; set; }
        public AppUser? User { get; set; }
    }
}