using System.Text;
using AgriConnect.Models;
using AgriConnect.Repositories;

namespace AgriConnect.Controllers
{
    public static class AppSession
    {
        public static User? CurrentUser { get; set; }
        public static string CurrentLanguage { get; set; } = "English"; // English, Kinyarwanda, French
        public static List<OrderItem> Cart { get; } = new();

        public static bool IsLoggedIn => CurrentUser != null;
        public static bool IsAdmin => CurrentUser?.Role == UserRole.Admin;
        public static bool IsFarmer => CurrentUser?.Role == UserRole.Farmer;
        public static bool IsBuyer => CurrentUser?.Role == UserRole.Buyer;
        public static bool IsTransporter => CurrentUser?.Role == UserRole.Transporter;
        public static bool IsAgronomist => CurrentUser?.Role == UserRole.Agronomist;

        public static void Logout()
        {
            CurrentUser = null;
            Cart.Clear();
        }
    }

    public class AuthController
    {
        private readonly IUserRepository _userRepo = new UserRepository();
        private readonly IAuditLogRepository _auditRepo = new AuditLogRepository();

        public (bool success, string message, User? user) Login(string emailOrPhone, string password)
        {
            if (string.IsNullOrWhiteSpace(emailOrPhone) || string.IsNullOrWhiteSpace(password))
                return (false, "Please provide both Email/Phone and Password.", null);

            var user = _userRepo.GetByEmailOrPhone(emailOrPhone.Trim());
            if (user == null)
                return (false, "User account not found with this email or phone.", null);

            if (user.Status == UserStatus.Suspended)
                return (false, "Your account has been suspended by the platform administrator.", null);

            if (!AppDatabase.VerifyPassword(password, user.PasswordHash))
                return (false, "Invalid password. Please check your credentials.", null);

            AppSession.CurrentUser = user;
            _auditRepo.Log("User Login", $"User {user.FullName} ({user.Role}) logged in.", user.UserId, user.FullName);
            return (true, $"Welcome back, {user.FullName}!", user);
        }

        public (bool success, string message, User? user) Register(
            string fullName, string email, string phone, string password, 
            UserRole role, string district, string sector, 
            string? farmSize = null, string? crops = null, string? coop = null, string? deliveryAddress = null)
        {
            if (string.IsNullOrWhiteSpace(fullName) || string.IsNullOrWhiteSpace(phone) || string.IsNullOrWhiteSpace(password))
                return (false, "Name, Phone and Password are required.", null);

            var existing = _userRepo.GetByEmailOrPhone(email) ?? _userRepo.GetByEmailOrPhone(phone);
            if (existing != null)
                return (false, "A user with this email or phone already exists.", null);

            var user = new User
            {
                FullName = fullName.Trim(),
                Email = email.Trim(),
                Phone = phone.Trim(),
                PasswordHash = AppDatabase.HashPassword(password),
                Role = role,
                Status = UserStatus.Active,
                District = district,
                Sector = sector,
                FarmSize = farmSize,
                CropsGrown = crops,
                Cooperative = coop,
                DeliveryAddress = deliveryAddress,
                WalletBalance = role == UserRole.Buyer ? 100000m : 0m // starter balance for testing
            };

            var created = _userRepo.Create(user);
            AppSession.CurrentUser = created;
            _auditRepo.Log("User Registration", $"New {role} registered: {fullName} ({phone})", created.UserId, created.FullName);
            return (true, "Registration successful!", created);
        }
    }

    public class FarmerController
    {
        private readonly IListingRepository _listingRepo = new ListingRepository();
        private readonly IOrderRepository _orderRepo = new OrderRepository();
        private readonly IMarketPriceRepository _marketRepo = new MarketPriceRepository();
        private readonly IAuditLogRepository _auditRepo = new AuditLogRepository();

        public List<Listing> GetMyListings(int farmerId) => _listingRepo.GetByFarmer(farmerId);

        public (bool success, string message) CreateListing(Listing listing)
        {
            if (string.IsNullOrWhiteSpace(listing.ProduceName) || listing.Quantity <= 0 || listing.PricePerUnit <= 0)
                return (false, "Please provide valid produce name, positive quantity and price.");

            var created = _listingRepo.Create(listing);
            _auditRepo.Log("Create Listing", $"Farmer {listing.FarmerName} listed {listing.Quantity} {listing.Unit} of {listing.ProduceName} @ RWF {listing.PricePerUnit}", listing.FarmerId, listing.FarmerName);
            return (true, "Produce listing created successfully!");
        }

        public (bool success, string message) UpdateListing(Listing listing)
        {
            if (_listingRepo.Update(listing))
            {
                _auditRepo.Log("Update Listing", $"Updated listing #{listing.ListingId} ({listing.ProduceName})", listing.FarmerId, listing.FarmerName);
                return (true, "Listing updated successfully!");
            }
            return (false, "Failed to update listing.");
        }

        public (bool success, string message) TogglePauseListing(int listingId)
        {
            var listing = _listingRepo.GetById(listingId);
            if (listing == null) return (false, "Listing not found.");

            var newStatus = listing.Status == ListingStatus.Active ? ListingStatus.Paused : ListingStatus.Active;
            _listingRepo.UpdateStatus(listingId, newStatus);
            return (true, $"Listing is now {newStatus}.");
        }

        public (bool success, string message) DeleteListing(int listingId)
        {
            if (_listingRepo.Delete(listingId))
                return (true, "Listing removed.");
            return (false, "Could not delete listing.");
        }

        public List<Order> GetIncomingOrders(int farmerId) => _orderRepo.GetByFarmer(farmerId);

        public (bool success, string message) AcceptOrder(int orderId)
        {
            if (_orderRepo.UpdateStatus(orderId, OrderStatus.Accepted))
            {
                _auditRepo.Log("Order Accepted", $"Farmer accepted Order #{orderId}", AppSession.CurrentUser?.UserId, AppSession.CurrentUser?.FullName ?? "Farmer");
                return (true, $"Order #{orderId} accepted! Prepare produce for delivery/pickup.");
            }
            return (false, "Order could not be accepted.");
        }

        public (bool success, string message) RejectOrder(int orderId)
        {
            if (_orderRepo.UpdateStatus(orderId, OrderStatus.Cancelled))
            {
                _auditRepo.Log("Order Rejected", $"Farmer rejected Order #{orderId}", AppSession.CurrentUser?.UserId, AppSession.CurrentUser?.FullName ?? "Farmer");
                return (true, $"Order #{orderId} rejected.");
            }
            return (false, "Order rejection failed.");
        }

        public (bool success, string message) MarkOrderReady(int orderId)
        {
            if (_orderRepo.UpdateStatus(orderId, OrderStatus.Ready))
            {
                return (true, $"Order #{orderId} marked as Ready for pickup/dispatch.");
            }
            return (false, "Failed to update order status.");
        }

        public (decimal min, decimal max, decimal avg) SuggestFairPrice(string cropName, string district)
        {
            var prices = _marketRepo.GetByCrop(cropName);
            if (!prices.Any())
            {
                return (400m, 600m, 500m); // default baseline
            }
            decimal min = prices.Min(p => p.PricePerKg);
            decimal max = prices.Max(p => p.PricePerKg);
            decimal avg = prices.Average(p => p.PricePerKg);
            return (min * 0.9m, max * 1.05m, avg);
        }

        public (decimal today, decimal thisWeek, decimal thisMonth, decimal allTime) GetEarningsSummary(int farmerId)
        {
            var completedOrders = _orderRepo.GetByFarmer(farmerId)
                .Where(o => o.Status == OrderStatus.Completed || o.Status == OrderStatus.Delivered)
                .ToList();

            DateTime now = DateTime.UtcNow;
            decimal today = completedOrders.Where(o => o.CreatedAt.Date == now.Date).Sum(o => o.ProduceTotal * 0.97m);
            decimal thisWeek = completedOrders.Where(o => o.CreatedAt >= now.AddDays(-7)).Sum(o => o.ProduceTotal * 0.97m);
            decimal thisMonth = completedOrders.Where(o => o.CreatedAt >= now.AddDays(-30)).Sum(o => o.ProduceTotal * 0.97m);
            decimal allTime = completedOrders.Sum(o => o.ProduceTotal * 0.97m);

            return (today, thisWeek, thisMonth, allTime);
        }
    }

    public class BuyerController
    {
        private readonly IListingRepository _listingRepo = new ListingRepository();
        private readonly IOrderRepository _orderRepo = new OrderRepository();
        private readonly IPaymentRepository _paymentRepo = new PaymentRepository();
        private readonly IUserRepository _userRepo = new UserRepository();
        private readonly IAuditLogRepository _auditRepo = new AuditLogRepository();

        public List<Listing> SearchProduce(string? keyword = null, string? category = null, string? district = null, decimal? maxPrice = null, string? sortBy = null)
        {
            var query = _listingRepo.GetActive().AsEnumerable();

            if (!string.IsNullOrWhiteSpace(keyword))
            {
                query = query.Where(l => l.ProduceName.Contains(keyword, StringComparison.OrdinalIgnoreCase) ||
                                         l.Description.Contains(keyword, StringComparison.OrdinalIgnoreCase) ||
                                         l.FarmerName.Contains(keyword, StringComparison.OrdinalIgnoreCase));
            }

            if (!string.IsNullOrWhiteSpace(category) && category != "All Categories")
            {
                query = query.Where(l => l.Category.Equals(category, StringComparison.OrdinalIgnoreCase));
            }

            if (!string.IsNullOrWhiteSpace(district) && district != "All Districts")
            {
                query = query.Where(l => l.District.Equals(district, StringComparison.OrdinalIgnoreCase));
            }

            if (maxPrice.HasValue && maxPrice.Value > 0)
            {
                query = query.Where(l => l.PricePerUnit <= maxPrice.Value);
            }

            query = sortBy switch
            {
                "Price: Low to High" => query.OrderBy(l => l.PricePerUnit),
                "Price: High to Low" => query.OrderByDescending(l => l.PricePerUnit),
                "Freshness (Newest)" => query.OrderByDescending(l => l.HarvestDate),
                "Quantity Available" => query.OrderByDescending(l => l.Quantity),
                _ => query.OrderByDescending(l => l.CreatedAt)
            };

            return query.ToList();
        }

        public (bool success, string message) AddToCart(Listing listing, decimal quantity)
        {
            if (quantity <= 0) return (false, "Quantity must be greater than zero.");
            if (quantity > listing.Quantity) return (false, $"Only {listing.Quantity} {listing.Unit} available.");

            var existing = AppSession.Cart.FirstOrDefault(c => c.ListingId == listing.ListingId);
            if (existing != null)
            {
                if (existing.Quantity + quantity > listing.Quantity)
                    return (false, $"Cannot add more. Max stock is {listing.Quantity} {listing.Unit}.");
                existing.Quantity += quantity;
            }
            else
            {
                AppSession.Cart.Add(new OrderItem
                {
                    ListingId = listing.ListingId,
                    ProduceName = listing.ProduceName,
                    Quantity = quantity,
                    Unit = listing.Unit,
                    UnitPrice = listing.PricePerUnit
                });
            }

            return (true, $"Added {quantity} {listing.Unit} of {listing.ProduceName} to cart.");
        }

        public void RemoveFromCart(int listingId)
        {
            AppSession.Cart.RemoveAll(c => c.ListingId == listingId);
        }

        public void ClearCart() => AppSession.Cart.Clear();

        public (decimal produceTotal, decimal deliveryFee, decimal platformFee, decimal grandTotal) CalculateTotals(DeliveryType deliveryType)
        {
            decimal produceTotal = AppSession.Cart.Sum(i => i.Subtotal);
            decimal deliveryFee = deliveryType == DeliveryType.TransporterDelivery ? 5000m : 0m;
            decimal platformFee = Math.Round(produceTotal * 0.03m, 0); // 3% escrow platform fee
            decimal grandTotal = produceTotal + deliveryFee + platformFee;
            return (produceTotal, deliveryFee, platformFee, grandTotal);
        }

        public (bool success, string message, Order? order) Checkout(
            User buyer, DeliveryType deliveryType, string deliveryAddress, PaymentMethod paymentMethod)
        {
            if (!AppSession.Cart.Any())
                return (false, "Your cart is empty.", null);

            var (produceTotal, deliveryFee, platformFee, grandTotal) = CalculateTotals(deliveryType);

            // Deduct inventory
            foreach (var item in AppSession.Cart)
            {
                var listing = _listingRepo.GetById(item.ListingId);
                if (listing == null || listing.Quantity < item.Quantity)
                    return (false, $"Stock changed for {item.ProduceName}. Please review your cart.", null);
            }

            // Group by farmer or take first farmer for simplicity
            var firstItemListing = _listingRepo.GetById(AppSession.Cart[0].ListingId);
            int farmerId = firstItemListing?.FarmerId ?? 2;
            string farmerName = firstItemListing?.FarmerName ?? "Farmer";

            foreach (var item in AppSession.Cart)
            {
                _listingRepo.DeductStock(item.ListingId, item.Quantity);
            }

            var order = new Order
            {
                BuyerId = buyer.UserId,
                BuyerName = buyer.FullName,
                BuyerPhone = buyer.Phone,
                FarmerId = farmerId,
                FarmerName = farmerName,
                ProduceTotal = produceTotal,
                DeliveryFee = deliveryFee,
                PlatformFee = platformFee,
                DeliveryType = deliveryType,
                DeliveryAddress = deliveryAddress,
                PickupAddress = firstItemListing?.PickupLocation ?? "Farmer Station",
                Status = OrderStatus.Placed,
                Items = new List<OrderItem>(AppSession.Cart)
            };

            var createdOrder = _orderRepo.Create(order);

            // Create Escrow Payment
            var payment = new Payment
            {
                OrderId = createdOrder.OrderId,
                BuyerId = buyer.UserId,
                FarmerId = farmerId,
                Amount = grandTotal,
                PlatformFee = platformFee,
                Method = paymentMethod,
                EscrowStatus = EscrowStatus.HeldInEscrow,
                TransactionReference = $"{paymentMethod}-RW-" + DateTime.UtcNow.Ticks.ToString()[^8..]
            };
            _paymentRepo.Create(payment);

            _auditRepo.Log("Order Placed", $"Buyer {buyer.FullName} placed Order #{createdOrder.OrderId} (RWF {grandTotal:N0}) via {paymentMethod} [Escrow Secured]", buyer.UserId, buyer.FullName);

            ClearCart();
            return (true, $"Order #{createdOrder.OrderId} placed successfully! Funds held securely in Escrow.", createdOrder);
        }

        public List<Order> GetMyOrders(int buyerId) => _orderRepo.GetByBuyer(buyerId);

        public (bool success, string message) ConfirmDeliveryAndReleaseEscrow(int orderId, string code)
        {
            var order = _orderRepo.GetById(orderId);
            if (order == null) return (false, "Order not found.");

            if (!string.Equals(order.DeliveryCode.Trim(), code.Trim(), StringComparison.OrdinalIgnoreCase))
                return (false, "Invalid delivery confirmation code.");

            _orderRepo.UpdateStatus(orderId, OrderStatus.Completed);

            var payment = _paymentRepo.GetByOrderId(orderId);
            if (payment != null)
            {
                _paymentRepo.UpdateEscrowStatus(payment.PaymentId, EscrowStatus.ReleasedToFarmer);
                _userRepo.UpdateBalance(order.FarmerId, payment.FarmerPayout);
            }

            _auditRepo.Log("Escrow Released", $"Order #{orderId} completed. Escrow released to Farmer #{order.FarmerId}.", AppSession.CurrentUser?.UserId, AppSession.CurrentUser?.FullName ?? "Buyer");
            return (true, $"Delivery confirmed! Escrow payment released to farmer {order.FarmerName}.");
        }

        public (bool success, string message) CancelOrder(int orderId)
        {
            var order = _orderRepo.GetById(orderId);
            if (order == null) return (false, "Order not found.");
            if (order.Status != OrderStatus.Placed) return (false, "Only orders with 'Placed' status can be cancelled.");

            _orderRepo.UpdateStatus(orderId, OrderStatus.Cancelled);

            // Refund payment
            var payment = _paymentRepo.GetByOrderId(orderId);
            if (payment != null)
            {
                _paymentRepo.UpdateEscrowStatus(payment.PaymentId, EscrowStatus.RefundedToBuyer);
                _userRepo.UpdateBalance(order.BuyerId, payment.Amount);
            }

            _auditRepo.Log("Order Cancelled", $"Buyer cancelled Order #{orderId}. Funds refunded.", AppSession.CurrentUser?.UserId, AppSession.CurrentUser?.FullName ?? "Buyer");
            return (true, $"Order #{orderId} cancelled and payment refunded.");
        }
    }

    public class TransporterController
    {
        private readonly IOrderRepository _orderRepo = new OrderRepository();
        private readonly IAuditLogRepository _auditRepo = new AuditLogRepository();

        public List<Order> GetAvailableJobs() => _orderRepo.GetAvailableDeliveries();
        public List<Order> GetMyJobs(int transporterId) => _orderRepo.GetByTransporter(transporterId);

        public (bool success, string message) AcceptDeliveryJob(int orderId, int transporterId, string transporterName)
        {
            if (_orderRepo.AssignTransporter(orderId, transporterId, transporterName))
            {
                _auditRepo.Log("Job Claimed", $"Transporter {transporterName} claimed delivery for Order #{orderId}", transporterId, transporterName);
                return (true, $"Delivery job #{orderId} claimed! Proceed to pickup location.");
            }
            return (false, "Could not claim delivery job.");
        }

        public (bool success, string message) ConfirmPickup(int orderId)
        {
            if (_orderRepo.UpdateStatus(orderId, OrderStatus.InTransit))
            {
                return (true, $"Produce picked up for Order #{orderId}. Now In Transit!");
            }
            return (false, "Failed to update pickup status.");
        }

        public (bool success, string message) ConfirmDropOff(int orderId, string enteredCode)
        {
            var order = _orderRepo.GetById(orderId);
            if (order == null) return (false, "Order not found.");

            if (!string.Equals(order.DeliveryCode.Trim(), enteredCode.Trim(), StringComparison.OrdinalIgnoreCase))
                return (false, "Incorrect delivery code provided by recipient.");

            _orderRepo.UpdateStatus(orderId, OrderStatus.Delivered);
            _auditRepo.Log("Order Delivered", $"Transporter confirmed drop-off for Order #{orderId}", AppSession.CurrentUser?.UserId, AppSession.CurrentUser?.FullName ?? "Transporter");
            return (true, $"Order #{orderId} successfully delivered! Awaiting buyer completion.");
        }
    }

    public class MarketController
    {
        private readonly IMarketPriceRepository _marketRepo = new MarketPriceRepository();

        public List<MarketPrice> GetAllPrices() => _marketRepo.GetAll();
        public List<MarketPrice> GetByDistrict(string district) => _marketRepo.GetByDistrict(district);
        public List<MarketPrice> GetByCrop(string crop) => _marketRepo.GetByCrop(crop);

        public void RecordPrice(MarketPrice price)
        {
            _marketRepo.AddOrUpdate(price);
        }
    }

    public class AdvisoryController
    {
        private readonly IAdvisoryRepository _advisoryRepo = new AdvisoryRepository();

        public List<AdvisoryArticle> GetArticles(string? language = null) => _advisoryRepo.GetArticles(language);

        public (bool success, string message) PublishArticle(AdvisoryArticle article)
        {
            if (string.IsNullOrWhiteSpace(article.Title) || string.IsNullOrWhiteSpace(article.Content))
                return (false, "Title and Content are required.");

            _advisoryRepo.AddArticle(article);
            return (true, "Advisory article published successfully!");
        }

        public List<FarmerQuestion> GetQuestions(int? farmerId = null) => _advisoryRepo.GetQuestions(farmerId);

        public (bool success, string message) AskQuestion(FarmerQuestion question)
        {
            if (string.IsNullOrWhiteSpace(question.Subject) || string.IsNullOrWhiteSpace(question.QuestionText))
                return (false, "Subject and Question are required.");

            _advisoryRepo.AskQuestion(question);
            return (true, "Your question has been submitted to the agronomists!");
        }

        public (bool success, string message) AnswerQuestion(int questionId, string answer, string agronomist)
        {
            if (string.IsNullOrWhiteSpace(answer))
                return (false, "Answer cannot be empty.");

            if (_advisoryRepo.AnswerQuestion(questionId, answer, agronomist))
                return (true, "Answer posted successfully.");
            return (false, "Failed to post answer.");
        }
    }

    public class AdminController
    {
        private readonly IUserRepository _userRepo = new UserRepository();
        private readonly IListingRepository _listingRepo = new ListingRepository();
        private readonly IOrderRepository _orderRepo = new OrderRepository();
        private readonly IPaymentRepository _paymentRepo = new PaymentRepository();
        private readonly IDisputeRepository _disputeRepo = new DisputeRepository();
        private readonly IAuditLogRepository _auditRepo = new AuditLogRepository();

        public List<User> GetAllUsers() => _userRepo.GetAll();

        public bool SetUserStatus(int userId, UserStatus status)
        {
            bool res = _userRepo.UpdateStatus(userId, status);
            if (res)
            {
                _auditRepo.Log("User Status Changed", $"User #{userId} status changed to {status}", AppSession.CurrentUser?.UserId, AppSession.CurrentUser?.FullName ?? "Admin");
            }
            return res;
        }

        public bool ModerateListing(int listingId, ListingStatus newStatus)
        {
            return _listingRepo.UpdateStatus(listingId, newStatus);
        }

        public List<Dispute> GetAllDisputes() => _disputeRepo.GetAll();

        public (bool success, string message) ResolveDispute(int disputeId, DisputeStatus resolution, string notes)
        {
            var dispute = _disputeRepo.GetById(disputeId);
            if (dispute == null) return (false, "Dispute not found.");

            _disputeRepo.Resolve(disputeId, resolution, notes);

            var order = _orderRepo.GetById(dispute.OrderId);
            var payment = _paymentRepo.GetByOrderId(dispute.OrderId);

            if (resolution == DisputeStatus.ResolvedRefundBuyer && payment != null && order != null)
            {
                _paymentRepo.UpdateEscrowStatus(payment.PaymentId, EscrowStatus.RefundedToBuyer);
                _userRepo.UpdateBalance(order.BuyerId, payment.Amount);
                _orderRepo.UpdateStatus(order.OrderId, OrderStatus.Cancelled);
            }
            else if (resolution == DisputeStatus.ResolvedReleaseFarmer && payment != null && order != null)
            {
                _paymentRepo.UpdateEscrowStatus(payment.PaymentId, EscrowStatus.ReleasedToFarmer);
                _userRepo.UpdateBalance(order.FarmerId, payment.FarmerPayout);
                _orderRepo.UpdateStatus(order.OrderId, OrderStatus.Completed);
            }

            _auditRepo.Log("Dispute Resolved", $"Dispute #{disputeId} resolved: {resolution}. Notes: {notes}", AppSession.CurrentUser?.UserId, AppSession.CurrentUser?.FullName ?? "Admin");
            return (true, $"Dispute #{disputeId} marked as {resolution}.");
        }

        public List<AuditLog> GetAuditLogs() => _auditRepo.GetAll();

        public string ExportReportsCsv()
        {
            var sb = new StringBuilder();
            sb.AppendLine("OrderId,BuyerName,FarmerName,ProduceTotal,DeliveryFee,PlatformFee,TotalAmount,Status,Date");
            foreach (var o in _orderRepo.GetAll())
            {
                sb.AppendLine($"{o.OrderId},\"{o.BuyerName}\",\"{o.FarmerName}\",{o.ProduceTotal},{o.DeliveryFee},{o.PlatformFee},{o.TotalAmount},{o.Status},{o.CreatedAt:yyyy-MM-dd HH:mm}");
            }
            return sb.ToString();
        }

        public (int totalUsers, int activeListings, int totalOrders, decimal totalVolume, decimal escrowHeld) GetPlatformStats()
        {
            int totalUsers = _userRepo.GetAll().Count;
            int activeListings = _listingRepo.GetActive().Count;
            var orders = _orderRepo.GetAll();
            int totalOrders = orders.Count;
            decimal totalVolume = orders.Where(o => o.Status != OrderStatus.Cancelled).Sum(o => o.TotalAmount);
            decimal escrowHeld = _paymentRepo.GetAll()
                .Where(p => p.EscrowStatus == EscrowStatus.HeldInEscrow)
                .Sum(p => p.Amount);

            return (totalUsers, activeListings, totalOrders, totalVolume, escrowHeld);
        }
    }
}
