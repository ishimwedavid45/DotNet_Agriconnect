namespace AgriConnect.Models
{
    public enum ListingStatus
    {
        Active,
        Paused,
        SoldOut,
        RemovedByAdmin
    }

    public class Listing
    {
        public int ListingId { get; set; }
        public int FarmerId { get; set; }
        public string FarmerName { get; set; } = string.Empty;
        public string ProduceName { get; set; } = string.Empty;
        public string Category { get; set; } = "Vegetables"; // Cereals, Tubers, Fruits, Vegetables, Legumes, Coffee/Tea
        public decimal Quantity { get; set; }
        public decimal OriginalQuantity { get; set; }
        public string Unit { get; set; } = "kg"; // kg, sack (50kg), crate, bunch
        public decimal PricePerUnit { get; set; } // in RWF
        public DateTime HarvestDate { get; set; } = DateTime.UtcNow;
        public string PickupLocation { get; set; } = string.Empty;
        public string District { get; set; } = "Musanze";
        public ListingStatus Status { get; set; } = ListingStatus.Active;
        public string Description { get; set; } = string.Empty;
        public string ImageUrl { get; set; } = string.Empty;
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        public bool IsAvailable => Status == ListingStatus.Active && Quantity > 0;
    }
}
