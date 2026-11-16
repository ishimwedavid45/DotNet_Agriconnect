using AgriConnect.Models;

namespace AgriConnect.Repositories
{
    public interface IUserRepository
    {
        List<User> GetAll();
        User? GetById(int id);
        User? GetByEmailOrPhone(string emailOrPhone);
        User Create(User user);
        bool Update(User user);
        bool Delete(int id);
        bool UpdateStatus(int userId, UserStatus status);
        bool UpdateBalance(int userId, decimal delta);
    }

    public class UserRepository : IUserRepository
    {
        private readonly AppDatabase _db = AppDatabase.Instance;

        public List<User> GetAll() => _db.Data.Users.ToList();

        public User? GetById(int id) => _db.Data.Users.FirstOrDefault(u => u.UserId == id);

        public User? GetByEmailOrPhone(string emailOrPhone) =>
            _db.Data.Users.FirstOrDefault(u => 
                u.Email.Equals(emailOrPhone, StringComparison.OrdinalIgnoreCase) ||
                u.Phone.Equals(emailOrPhone, StringComparison.OrdinalIgnoreCase));

        public User Create(User user)
        {
            user.UserId = _db.Data.NextUserId++;
            user.CreatedAt = DateTime.UtcNow;
            _db.Data.Users.Add(user);
            _db.Save();
            return user;
        }

        public bool Update(User user)
        {
            var existing = GetById(user.UserId);
            if (existing == null) return false;

            existing.FullName = user.FullName;
            existing.Email = user.Email;
            existing.Phone = user.Phone;
            existing.District = user.District;
            existing.Sector = user.Sector;
            existing.FarmSize = user.FarmSize;
            existing.CropsGrown = user.CropsGrown;
            existing.Cooperative = user.Cooperative;
            existing.DeliveryAddress = user.DeliveryAddress;
            _db.Save();
            return true;
        }

        public bool Delete(int id)
        {
            var user = GetById(id);
            if (user == null) return false;
            _db.Data.Users.Remove(user);
            _db.Save();
            return true;
        }

        public bool UpdateStatus(int userId, UserStatus status)
        {
            var user = GetById(userId);
            if (user == null) return false;
            user.Status = status;
            _db.Save();
            return true;
        }

        public bool UpdateBalance(int userId, decimal delta)
        {
            var user = GetById(userId);
            if (user == null) return false;
            user.WalletBalance += delta;
            _db.Save();
            return true;
        }
    }

    public interface IListingRepository
    {
        List<Listing> GetAll();
        List<Listing> GetActive();
        List<Listing> GetByFarmer(int farmerId);
        Listing? GetById(int id);
        Listing Create(Listing listing);
        bool Update(Listing listing);
        bool Delete(int id);
        bool UpdateStatus(int id, ListingStatus status);
        bool DeductStock(int id, decimal quantity);
    }

    public class ListingRepository : IListingRepository
    {
        private readonly AppDatabase _db = AppDatabase.Instance;

        public List<Listing> GetAll() => _db.Data.Listings.ToList();

        public List<Listing> GetActive() => 
            _db.Data.Listings.Where(l => l.Status == ListingStatus.Active && l.Quantity > 0).ToList();

        public List<Listing> GetByFarmer(int farmerId) =>
            _db.Data.Listings.Where(l => l.FarmerId == farmerId).ToList();

        public Listing? GetById(int id) => _db.Data.Listings.FirstOrDefault(l => l.ListingId == id);

        public Listing Create(Listing listing)
        {
            listing.ListingId = _db.Data.NextListingId++;
            listing.CreatedAt = DateTime.UtcNow;
            listing.OriginalQuantity = listing.Quantity;
            _db.Data.Listings.Add(listing);
            _db.Save();
            return listing;
        }

        public bool Update(Listing listing)
        {
            var existing = GetById(listing.ListingId);
            if (existing == null) return false;

            existing.ProduceName = listing.ProduceName;
            existing.Category = listing.Category;
            existing.Quantity = listing.Quantity;
            existing.Unit = listing.Unit;
            existing.PricePerUnit = listing.PricePerUnit;
            existing.HarvestDate = listing.HarvestDate;
            existing.PickupLocation = listing.PickupLocation;
            existing.District = listing.District;
            existing.Description = listing.Description;
            existing.Status = listing.Quantity <= 0 ? ListingStatus.SoldOut : listing.Status;
            _db.Save();
            return true;
        }

        public bool Delete(int id)
        {
            var item = GetById(id);
            if (item == null) return false;
            _db.Data.Listings.Remove(item);
            _db.Save();
            return true;
        }

        public bool UpdateStatus(int id, ListingStatus status)
        {
            var item = GetById(id);
            if (item == null) return false;
            item.Status = status;
            _db.Save();
            return true;
        }

        public bool DeductStock(int id, decimal quantity)
        {
            var item = GetById(id);
            if (item == null || item.Quantity < quantity) return false;
            item.Quantity -= quantity;
            if (item.Quantity <= 0)
            {
                item.Quantity = 0;
                item.Status = ListingStatus.SoldOut;
            }
            _db.Save();
            return true;
        }
    }

    public interface IOrderRepository
    {
        List<Order> GetAll();
        Order? GetById(int id);
        List<Order> GetByBuyer(int buyerId);
        List<Order> GetByFarmer(int farmerId);
        List<Order> GetByTransporter(int transporterId);
        List<Order> GetAvailableDeliveries();
        Order Create(Order order);
        bool UpdateStatus(int orderId, OrderStatus status);
        bool AssignTransporter(int orderId, int transporterId, string transporterName);
    }

    public class OrderRepository : IOrderRepository
    {
        private readonly AppDatabase _db = AppDatabase.Instance;

        public List<Order> GetAll() => _db.Data.Orders.ToList();

        public Order? GetById(int id) => _db.Data.Orders.FirstOrDefault(o => o.OrderId == id);

        public List<Order> GetByBuyer(int buyerId) =>
            _db.Data.Orders.Where(o => o.BuyerId == buyerId).OrderByDescending(o => o.CreatedAt).ToList();

        public List<Order> GetByFarmer(int farmerId) =>
            _db.Data.Orders.Where(o => o.FarmerId == farmerId).OrderByDescending(o => o.CreatedAt).ToList();

        public List<Order> GetByTransporter(int transporterId) =>
            _db.Data.Orders.Where(o => o.TransporterId == transporterId).OrderByDescending(o => o.CreatedAt).ToList();

        public List<Order> GetAvailableDeliveries() =>
            _db.Data.Orders.Where(o => o.DeliveryType == DeliveryType.TransporterDelivery && 
                                       (o.Status == OrderStatus.Accepted || o.Status == OrderStatus.Ready) && 
                                       o.TransporterId == null).ToList();

        public Order Create(Order order)
        {
            order.OrderId = _db.Data.NextOrderId++;
            order.CreatedAt = DateTime.UtcNow;
            order.DeliveryCode = "AC-" + new Random().Next(1000, 9999);
            _db.Data.Orders.Add(order);
            _db.Save();
            return order;
        }

        public bool UpdateStatus(int orderId, OrderStatus status)
        {
            var order = GetById(orderId);
            if (order == null) return false;
            order.Status = status;
            if (status == OrderStatus.Accepted) order.AcceptedAt = DateTime.UtcNow;
            if (status == OrderStatus.Delivered) order.DeliveredAt = DateTime.UtcNow;
            if (status == OrderStatus.Completed) order.CompletedAt = DateTime.UtcNow;
            _db.Save();
            return true;
        }

        public bool AssignTransporter(int orderId, int transporterId, string transporterName)
        {
            var order = GetById(orderId);
            if (order == null) return false;
            order.TransporterId = transporterId;
            order.TransporterName = transporterName;
            order.Status = OrderStatus.InTransit;
            _db.Save();
            return true;
        }
    }

    public interface IPaymentRepository
    {
        List<Payment> GetAll();
        Payment? GetByOrderId(int orderId);
        Payment Create(Payment payment);
        bool UpdateEscrowStatus(int paymentId, EscrowStatus status);
    }

    public class PaymentRepository : IPaymentRepository
    {
        private readonly AppDatabase _db = AppDatabase.Instance;

        public List<Payment> GetAll() => _db.Data.Payments.ToList();

        public Payment? GetByOrderId(int orderId) => _db.Data.Payments.FirstOrDefault(p => p.OrderId == orderId);

        public Payment Create(Payment payment)
        {
            payment.PaymentId = _db.Data.NextPaymentId++;
            payment.CreatedAt = DateTime.UtcNow;
            _db.Data.Payments.Add(payment);
            _db.Save();
            return payment;
        }

        public bool UpdateEscrowStatus(int paymentId, EscrowStatus status)
        {
            var payment = _db.Data.Payments.FirstOrDefault(p => p.PaymentId == paymentId);
            if (payment == null) return false;
            payment.EscrowStatus = status;
            if (status == EscrowStatus.ReleasedToFarmer) payment.ReleasedAt = DateTime.UtcNow;
            _db.Save();
            return true;
        }
    }

    public interface IMarketPriceRepository
    {
        List<MarketPrice> GetAll();
        List<MarketPrice> GetByDistrict(string district);
        List<MarketPrice> GetByCrop(string cropName);
        MarketPrice AddOrUpdate(MarketPrice price);
    }

    public class MarketPriceRepository : IMarketPriceRepository
    {
        private readonly AppDatabase _db = AppDatabase.Instance;

        public List<MarketPrice> GetAll() => _db.Data.MarketPrices.ToList();

        public List<MarketPrice> GetByDistrict(string district) =>
            _db.Data.MarketPrices.Where(p => p.District.Equals(district, StringComparison.OrdinalIgnoreCase)).ToList();

        public List<MarketPrice> GetByCrop(string cropName) =>
            _db.Data.MarketPrices.Where(p => p.CropName.Contains(cropName, StringComparison.OrdinalIgnoreCase)).ToList();

        public MarketPrice AddOrUpdate(MarketPrice price)
        {
            var existing = _db.Data.MarketPrices.FirstOrDefault(p => 
                p.CropName.Equals(price.CropName, StringComparison.OrdinalIgnoreCase) && 
                p.District.Equals(price.District, StringComparison.OrdinalIgnoreCase));

            if (existing != null)
            {
                existing.PreviousPricePerKg = existing.PricePerKg;
                existing.PricePerKg = price.PricePerKg;
                existing.RecordedDate = DateTime.UtcNow;
            }
            else
            {
                price.PriceId = _db.Data.NextPriceId++;
                price.RecordedDate = DateTime.UtcNow;
                _db.Data.MarketPrices.Add(price);
            }
            _db.Save();
            return price;
        }
    }

    public interface IAdvisoryRepository
    {
        List<AdvisoryArticle> GetArticles(string? language = null);
        AdvisoryArticle AddArticle(AdvisoryArticle article);
        List<FarmerQuestion> GetQuestions(int? farmerId = null);
        FarmerQuestion AskQuestion(FarmerQuestion question);
        bool AnswerQuestion(int questionId, string answer, string answeredBy);
    }

    public class AdvisoryRepository : IAdvisoryRepository
    {
        private readonly AppDatabase _db = AppDatabase.Instance;

        public List<AdvisoryArticle> GetArticles(string? language = null)
        {
            if (string.IsNullOrEmpty(language) || language == "All")
                return _db.Data.AdvisoryArticles.OrderByDescending(a => a.PublishedAt).ToList();
            return _db.Data.AdvisoryArticles.Where(a => a.Language.Equals(language, StringComparison.OrdinalIgnoreCase))
                .OrderByDescending(a => a.PublishedAt).ToList();
        }

        public AdvisoryArticle AddArticle(AdvisoryArticle article)
        {
            article.ArticleId = _db.Data.NextArticleId++;
            article.PublishedAt = DateTime.UtcNow;
            _db.Data.AdvisoryArticles.Add(article);
            _db.Save();
            return article;
        }

        public List<FarmerQuestion> GetQuestions(int? farmerId = null)
        {
            if (farmerId.HasValue)
                return _db.Data.FarmerQuestions.Where(q => q.FarmerId == farmerId.Value).OrderByDescending(q => q.AskedAt).ToList();
            return _db.Data.FarmerQuestions.OrderByDescending(q => q.AskedAt).ToList();
        }

        public FarmerQuestion AskQuestion(FarmerQuestion question)
        {
            question.QuestionId = _db.Data.NextQuestionId++;
            question.AskedAt = DateTime.UtcNow;
            _db.Data.FarmerQuestions.Add(question);
            _db.Save();
            return question;
        }

        public bool AnswerQuestion(int questionId, string answer, string answeredBy)
        {
            var q = _db.Data.FarmerQuestions.FirstOrDefault(x => x.QuestionId == questionId);
            if (q == null) return false;
            q.AnswerText = answer;
            q.AnsweredBy = answeredBy;
            q.AnsweredAt = DateTime.UtcNow;
            _db.Save();
            return true;
        }
    }

    public interface IDisputeRepository
    {
        List<Dispute> GetAll();
        Dispute? GetById(int id);
        Dispute Create(Dispute dispute);
        bool Resolve(int disputeId, DisputeStatus status, string resolutionNotes);
    }

    public class DisputeRepository : IDisputeRepository
    {
        private readonly AppDatabase _db = AppDatabase.Instance;

        public List<Dispute> GetAll() => _db.Data.Disputes.OrderByDescending(d => d.CreatedAt).ToList();

        public Dispute? GetById(int id) => _db.Data.Disputes.FirstOrDefault(d => d.DisputeId == id);

        public Dispute Create(Dispute dispute)
        {
            dispute.DisputeId = _db.Data.NextDisputeId++;
            dispute.CreatedAt = DateTime.UtcNow;
            _db.Data.Disputes.Add(dispute);
            _db.Save();
            return dispute;
        }

        public bool Resolve(int disputeId, DisputeStatus status, string resolutionNotes)
        {
            var dispute = GetById(disputeId);
            if (dispute == null) return false;
            dispute.Status = status;
            dispute.AdminResolutionNotes = resolutionNotes;
            dispute.ResolvedAt = DateTime.UtcNow;
            _db.Save();
            return true;
        }
    }

    public interface IAuditLogRepository
    {
        List<AuditLog> GetAll();
        void Log(string action, string details, int? userId = null, string userName = "System");
    }

    public class AuditLogRepository : IAuditLogRepository
    {
        private readonly AppDatabase _db = AppDatabase.Instance;

        public List<AuditLog> GetAll() => _db.Data.AuditLogs.OrderByDescending(l => l.Timestamp).ToList();

        public void Log(string action, string details, int? userId = null, string userName = "System")
        {
            _db.Data.AuditLogs.Add(new AuditLog
            {
                LogId = _db.Data.NextLogId++,
                Timestamp = DateTime.UtcNow,
                UserId = userId,
                UserName = userName,
                Action = action,
                Details = details
            });
            _db.Save();
        }
    }
}
