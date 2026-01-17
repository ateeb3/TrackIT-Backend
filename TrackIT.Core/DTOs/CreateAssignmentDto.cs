using System.ComponentModel.DataAnnotations;

namespace TrackIT.Core.DTOs
{
    public class CreateAssignmentDto
    {
        [Required]
        public int AssetId { get; set; }

        [Required]
        public string UserId { get; set; } = string.Empty; // The Employee

        public string Notes { get; set; } = string.Empty;

        public DateTime AssignedDate { get; set; } = DateTime.UtcNow;
    }
}