namespace AgriConnect.Models
{
    public enum OrderStatus
    {
        Placed,
        Accepted,
        Ready,
        InTransit,
        Delivered,
        Completed,
        Cancelled
    }

    public enum DeliveryType
    {
        SelfPickup,
        TransporterDelivery
    }

    public class OrderItem
    {
        public int OrderItemId { get; set; }
        public int OrderId { get; set; }
        public int ListingId { get; set; }
        public string ProduceName { get; set; } = string.Empty;
        public decimal Quantity { get; set; }
        public string Unit { get; set; } = "kg";
        public decimal UnitPrice { get; set; }
        public decimal Subtotal => Quantity * UnitPrice;
    }

    public class Order
    {
        public int OrderId { get; set; }
        public int BuyerId { get; set; }
        public string BuyerName { get; set; } = string.Empty;
        public string BuyerPhone { get; set; } = string.Empty;
        public int FarmerId { get; set; }
        public string FarmerName { get; set; } = string.Empty;
        public int? TransporterId { get; set; }
        public string? TransporterName { get; set; }

        public List<OrderItem> Items { get; set; } = new();
        public decimal ProduceTotal { get; set; }
        public decimal DeliveryFee { get; set; }
        public decimal PlatformFee { get; set; }
        public decimal TotalAmount => ProduceTotal + DeliveryFee + PlatformFee;

        public DeliveryType DeliveryType { get; set; } = DeliveryType.TransporterDelivery;
        public string DeliveryAddress { get; set; } = string.Empty;
        public string PickupAddress { get; set; } = string.Empty;
        public string DeliveryCode { get; set; } = string.Empty;

        public OrderStatus Status { get; set; } = OrderStatus.Placed;
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
        public DateTime? AcceptedAt { get; set; }
        public DateTime? DeliveredAt { get; set; }
        public DateTime? CompletedAt { get; set; }
    }
}
