using System;
using System.Collections.Generic;
using System.Text;

namespace TrackIT.Core.DTOs
{
    public class ChangePasswordDto
    {
        public string Email { get; set; }
        public string CurrentPassword { get; set; }
        public string NewPassword { get; set; }
    }
}
