namespace AgriConnect.Models
{
    public enum DisputeStatus
    {
        Open,
        UnderReview,
        ResolvedRefundBuyer,
        ResolvedReleaseFarmer,
        Dismissed
    }

    public class Dispute
    {
        public int DisputeId { get; set; }
        public int OrderId { get; set; }
        public int RaisedByUserId { get; set; }
        public string RaisedByName { get; set; } = string.Empty;
        public string Reason { get; set; } = string.Empty;
        public string Evidence { get; set; } = string.Empty;
        public DisputeStatus Status { get; set; } = DisputeStatus.Open;
        public string? AdminResolutionNotes { get; set; }
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
        public DateTime? ResolvedAt { get; set; }
    }

    public class Review
    {
        public int ReviewId { get; set; }
        public int OrderId { get; set; }
        public int ReviewerId { get; set; }
        public string ReviewerName { get; set; } = string.Empty;
        public int RevieweeId { get; set; }
        public int Rating { get; set; } = 5; // 1-5
        public string Comment { get; set; } = string.Empty;
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    }

    public class AuditLog
    {
        public int LogId { get; set; }
        public DateTime Timestamp { get; set; } = DateTime.UtcNow;
        public int? UserId { get; set; }
        public string UserName { get; set; } = "System";
        public string Action { get; set; } = string.Empty;
        public string Details { get; set; } = string.Empty;
    }
}
