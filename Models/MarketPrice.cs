namespace AgriConnect.Models
{
    public class MarketPrice
    {
        public int PriceId { get; set; }
        public string CropName { get; set; } = string.Empty;
        public string Category { get; set; } = string.Empty;
        public string District { get; set; } = string.Empty;
        public string MarketName { get; set; } = string.Empty;
        public decimal PricePerKg { get; set; }
        public decimal PreviousPricePerKg { get; set; }
        public string Unit { get; set; } = "kg";
        public DateTime RecordedDate { get; set; } = DateTime.UtcNow;

        public decimal ChangePercentage => PreviousPricePerKg > 0 
            ? ((PricePerKg - PreviousPricePerKg) / PreviousPricePerKg) * 100 
            : 0;

        public string Trend => ChangePercentage switch
        {
            > 0 => "▲ Rising",
            < 0 => "▼ Falling",
            _ => "▬ Stable"
        };
    }
}
