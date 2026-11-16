namespace AgriConnect.Models
{
    public enum UserRole
    {
        Farmer,
        Buyer,
        Transporter,
        Agronomist,
        Admin
    }

    public enum UserStatus
    {
        Active,
        Suspended,
        Pending
    }

    public class User
    {
        public int UserId { get; set; }
        public string FullName { get; set; } = string.Empty;
        public string Email { get; set; } = string.Empty;
        public string Phone { get; set; } = string.Empty;
        public string PasswordHash { get; set; } = string.Empty;
        public UserRole Role { get; set; }
        public UserStatus Status { get; set; } = UserStatus.Active;
        public string District { get; set; } = "Kigali";
        public string Sector { get; set; } = string.Empty;
        public decimal WalletBalance { get; set; } = 0.0m;
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        // Farmer specific
        public string? FarmSize { get; set; }
        public string? CropsGrown { get; set; }
        public string? Cooperative { get; set; }

        // Buyer specific
        public string? DeliveryAddress { get; set; }

        public override string ToString() => $"{FullName} ({Role} - {District})";
    }
}
