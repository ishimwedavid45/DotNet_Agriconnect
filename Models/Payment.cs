namespace AgriConnect.Models
{
    public enum PaymentMethod
    {
        MTN_MoMo,
        AirtelMoney,
        BankCard
    }

    public enum EscrowStatus
    {
        HeldInEscrow,
        ReleasedToFarmer,
        RefundedToBuyer
    }

    public class Payment
    {
        public int PaymentId { get; set; }
        public int OrderId { get; set; }
        public int BuyerId { get; set; }
        public int FarmerId { get; set; }
        public decimal Amount { get; set; }
        public decimal PlatformFee { get; set; }
        public decimal FarmerPayout => Amount - PlatformFee;
        public PaymentMethod Method { get; set; } = PaymentMethod.MTN_MoMo;
        public EscrowStatus EscrowStatus { get; set; } = EscrowStatus.HeldInEscrow;
        public string TransactionReference { get; set; } = string.Empty;
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
        public DateTime? ReleasedAt { get; set; }
    }
}
