using System.ComponentModel.DataAnnotations;

namespace TrackIT.Core.DTOs
{
    public class CreateCategoryDto
    {
        [Required]
        public string Name { get; set; } = string.Empty;
    }
}