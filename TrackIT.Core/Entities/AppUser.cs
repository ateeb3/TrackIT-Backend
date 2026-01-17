using Microsoft.AspNetCore.Identity;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace TrackIT.Core.Entities
{
    
    public class AppUser : IdentityUser
    {
        [Required]
        [MaxLength(100)]
        public string FullName { get; set; } = string.Empty;

        [MaxLength(50)]
        public string Department { get; set; } = string.Empty;

       
        public bool IsDeleted { get; set; } = false;

       
        public ICollection<AssignmentHistory> AssignmentHistory { get; set; } = new List<AssignmentHistory>();
    }
}