namespace TrackIT.Core.DTOs
{
    public class AssetDto
    {
        public int Id { get; set; }
        public string Name { get; set; } = string.Empty;
        public string SerialNumber { get; set; } = string.Empty;

        // Frontend prefers text ("Assigned") over numbers (2)
        public string Status { get; set; } = string.Empty;

        public DateTime PurchaseDate { get; set; }

        // The flattened category name (Eager Loaded)
        public string CategoryName { get; set; } = string.Empty;
    }
}