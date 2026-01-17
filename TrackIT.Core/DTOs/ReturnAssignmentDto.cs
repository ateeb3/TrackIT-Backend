using TrackIT.Core.Enums;

namespace TrackIT.Core.DTOs
{
    public class ReturnAssignmentDto
    {
        public int AssignmentId { get; set; }

        // This captures the condition (e.g., AssetStatus.Broken or AssetStatus.Available)
        public AssetStatus ReturnCondition { get; set; }

        public string Notes { get; set; } = string.Empty;
    }
}