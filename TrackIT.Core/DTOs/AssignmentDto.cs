namespace TrackIT.Core.DTOs
{
    public class AssignmentDto
    {
        public int Id { get; set; }
        public string AssetName { get; set; } = string.Empty;
        public string SerialNumber { get; set; } = string.Empty;
        public string UserName { get; set; } = string.Empty;
        public string Department { get; set; } = string.Empty;
        public DateTime AssignedDate { get; set; }
        public DateTime? ReturnDate { get; set; }
        public string Notes { get; set; } = string.Empty;
        public string? ReturnStatus { get; set; }
        public bool IsActive { get; set; } // True if still assigned
    }
}