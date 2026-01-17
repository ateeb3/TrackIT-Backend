using System;
using System.Collections.Generic;
using System.Text;

namespace TrackIT.Core.DTOs
{
    public class RecentActivityDto
    {
        public int Id { get; set; }
        public string AssetName { get; set; }
        public string AssignedTo { get; set; }
        public string Status { get; set; } // Assigned, Available, etc.
        public DateTime Timestamp { get; set; }
    }
}
