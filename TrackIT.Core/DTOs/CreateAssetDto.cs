using System.ComponentModel.DataAnnotations;
using TrackIT.Core.Enums;

namespace TrackIT.Core.DTOs
{
    public class CreateAssetDto
    {
        [Required(ErrorMessage = "Asset Name is required")]
        [MaxLength(100)]
        public string Name { get; set; } = string.Empty;

        [Required(ErrorMessage = "Serial Number is required")]
        public string SerialNumber { get; set; } = string.Empty;

        [Required]
        public int CategoryId { get; set; }

        [Required]
        public DateTime PurchaseDate { get; set; }

        // We use the Enum here so the API expects 1, 2, 3, etc.
        [Required]
        public AssetStatus Status { get; set; } = AssetStatus.Available;
    }
}