using System;
using TrackIT.Core.Enums;

namespace TrackIT.Core.Entities
{
    public class AssignmentHistory : BaseEntity
    {
        public int? AssetId { get; set; }
        public Asset? Asset { get; set; }

        public string? UserId { get; set; }
        public AppUser? User { get; set; }
        public AssetStatus? ReturnStatus { get; set; }

        public DateTime AssignedDate { get; set; }
        public DateTime? ReturnDate { get; set; }

        public string Notes { get; set; } = string.Empty; // e.g. "Screen was scratched on return"
    }
}