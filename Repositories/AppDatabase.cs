using System.Security.Cryptography;
using System.Text;
using System.Text.Json;
using AgriConnect.Models;

namespace AgriConnect.Repositories
{
    public class DatabaseContainer
    {
        public List<User> Users { get; set; } = new();
        public List<Listing> Listings { get; set; } = new();
        public List<Order> Orders { get; set; } = new();
        public List<Payment> Payments { get; set; } = new();
        public List<MarketPrice> MarketPrices { get; set; } = new();
        public List<AdvisoryArticle> AdvisoryArticles { get; set; } = new();
        public List<FarmerQuestion> FarmerQuestions { get; set; } = new();
        public List<Dispute> Disputes { get; set; } = new();
        public List<Review> Reviews { get; set; } = new();
        public List<AuditLog> AuditLogs { get; set; } = new();

        public int NextUserId { get; set; } = 1;
        public int NextListingId { get; set; } = 1;
        public int NextOrderId { get; set; } = 1;
        public int NextPaymentId { get; set; } = 1;
        public int NextPriceId { get; set; } = 1;
        public int NextArticleId { get; set; } = 1;
        public int NextQuestionId { get; set; } = 1;
        public int NextDisputeId { get; set; } = 1;
        public int NextReviewId { get; set; } = 1;
        public int NextLogId { get; set; } = 1;
    }

    public class AppDatabase
    {
        private static AppDatabase? _instance;
        private static readonly object _lock = new();

        public static AppDatabase Instance
        {
            get
            {
                if (_instance == null)
                {
                    lock (_lock)
                    {
                        _instance ??= new AppDatabase();
                    }
                }
                return _instance;
            }
        }

        private readonly string _dbFilePath;
        private DatabaseContainer _data;
        private readonly JsonSerializerOptions _jsonOptions;

        public event Action? OnDatabaseChanged;

        private AppDatabase()
        {
            string baseDir = AppDomain.CurrentDomain.BaseDirectory;
            string appDir = Path.Combine(baseDir, "Data");
            Directory.CreateDirectory(appDir);
            _dbFilePath = Path.Combine(appDir, "agriconnect_db.json");

            _jsonOptions = new JsonSerializerOptions
            {
                WriteIndented = true,
                PropertyNameCaseInsensitive = true
            };

            _data = LoadFromFile() ?? InitializeWithSeedData();
        }

        public string DatabasePath => _dbFilePath;

        public DatabaseContainer Data => _data;

        public static string HashPassword(string password)
        {
            using var sha = SHA256.Create();
            byte[] bytes = sha.ComputeHash(Encoding.UTF8.GetBytes(password + "AgriConnectSalt2026"));
            return Convert.ToBase64String(bytes);
        }

        public static bool VerifyPassword(string password, string storedHash)
        {
            return HashPassword(password) == storedHash;
        }

        public void Save()
        {
            lock (_lock)
            {
                try
                {
                    string json = JsonSerializer.Serialize(_data, _jsonOptions);
                    File.WriteAllText(_dbFilePath, json);
                    OnDatabaseChanged?.Invoke();
                }
                catch (Exception ex)
                {
                    System.Diagnostics.Debug.WriteLine($"DB Save error: {ex.Message}");
                }
            }
        }

        private DatabaseContainer? LoadFromFile()
        {
            if (!File.Exists(_dbFilePath)) return null;
            try
            {
                string json = File.ReadAllText(_dbFilePath);
                var data = JsonSerializer.Deserialize<DatabaseContainer>(json, _jsonOptions);
                return data;
            }
            catch
            {
                return null;
            }
        }

        public void ResetDatabase()
        {
            lock (_lock)
            {
                _data = InitializeWithSeedData();
                Save();
            }
        }

        public void BackupTo(string destinationPath)
        {
            lock (_lock)
            {
                string json = JsonSerializer.Serialize(_data, _jsonOptions);
                File.WriteAllText(destinationPath, json);
            }
        }

        public void RestoreFrom(string sourcePath)
        {
            lock (_lock)
            {
                string json = File.ReadAllText(sourcePath);
                var data = JsonSerializer.Deserialize<DatabaseContainer>(json, _jsonOptions);
                if (data != null)
                {
                    _data = data;
                    Save();
                }
            }
        }

        private DatabaseContainer InitializeWithSeedData()
        {
            var data = new DatabaseContainer();

            // Users
            data.Users.Add(new User
            {
                UserId = data.NextUserId++,
                FullName = "Admin User",
                Email = "admin@agriconnect.rw",
                Phone = "+250788111000",
                PasswordHash = HashPassword("admin123"),
                Role = UserRole.Admin,
                Status = UserStatus.Active,
                District = "Kigali",
                Sector = "Nyarugenge",
                WalletBalance = 500000m
            });

            data.Users.Add(new User
            {
                UserId = data.NextUserId++,
                FullName = "Jean Claude Hakizimana",
                Email = "farmer.jean@agriconnect.rw",
                Phone = "+250788222111",
                PasswordHash = HashPassword("farmer123"),
                Role = UserRole.Farmer,
                Status = UserStatus.Active,
                District = "Musanze",
                Sector = "Kinigi",
                FarmSize = "2.5 Hectares",
                CropsGrown = "Irish Potatoes (Kinigi), Carrots, Cabbages",
                Cooperative = "KOPABUMU Cooperative",
                WalletBalance = 125000m
            });

            data.Users.Add(new User
            {
                UserId = data.NextUserId++,
                FullName = "Marie Claire Uwimana",
                Email = "farmer.marie@agriconnect.rw",
                Phone = "+250788333222",
                PasswordHash = HashPassword("farmer123"),
                Role = UserRole.Farmer,
                Status = UserStatus.Active,
                District = "Nyagatare",
                Sector = "Nyagatare",
                FarmSize = "5.0 Hectares",
                CropsGrown = "Maize, Soya Beans, Beans",
                Cooperative = "COOPAMA Nyagatare",
                WalletBalance = 240000m
            });

            data.Users.Add(new User
            {
                UserId = data.NextUserId++,
                FullName = "Kigali Fresh Mart (Buyer)",
                Email = "buyer.kigali@agriconnect.rw",
                Phone = "+250788444333",
                PasswordHash = HashPassword("buyer123"),
                Role = UserRole.Buyer,
                Status = UserStatus.Active,
                District = "Gasabo",
                Sector = "Kimironko",
                DeliveryAddress = "Plot 45, KG 11 Ave, Kimironko Market Plaza",
                WalletBalance = 850000m
            });

            data.Users.Add(new User
            {
                UserId = data.NextUserId++,
                FullName = "Eric Nshimiyimana (Transporter)",
                Email = "transporter.eric@agriconnect.rw",
                Phone = "+250788555444",
                PasswordHash = HashPassword("driver123"),
                Role = UserRole.Transporter,
                Status = UserStatus.Active,
                District = "Kicukiro",
                Sector = "Gahanga",
                WalletBalance = 45000m
            });

            data.Users.Add(new User
            {
                UserId = data.NextUserId++,
                FullName = "Dr. Alice Uwase (Agronomist)",
                Email = "agronomist.alice@agriconnect.rw",
                Phone = "+250788666555",
                PasswordHash = HashPassword("agri123"),
                Role = UserRole.Agronomist,
                Status = UserStatus.Active,
                District = "Huye",
                Sector = "Ngoma",
                WalletBalance = 0m
            });

            // Listings
            data.Listings.Add(new Listing
            {
                ListingId = data.NextListingId++,
                FarmerId = 2,
                FarmerName = "Jean Claude Hakizimana",
                ProduceName = "Irish Potatoes (Kinigi Premium)",
                Category = "Tubers",
                Quantity = 1200m,
                OriginalQuantity = 1500m,
                Unit = "kg",
                PricePerUnit = 480m,
                HarvestDate = DateTime.UtcNow.AddDays(-2),
                PickupLocation = "Kinigi Agro Collection Center, Musanze",
                District = "Musanze",
                Status = ListingStatus.Active,
                Description = "High quality volcanic soil Kinigi red potatoes, freshly harvested and sorted into Grade A."
            });

            data.Listings.Add(new Listing
            {
                ListingId = data.NextListingId++,
                FarmerId = 2,
                FarmerName = "Jean Claude Hakizimana",
                ProduceName = "Fresh Organic Carrots",
                Category = "Vegetables",
                Quantity = 450m,
                OriginalQuantity = 500m,
                Unit = "kg",
                PricePerUnit = 350m,
                HarvestDate = DateTime.UtcNow.AddDays(-1),
                PickupLocation = "Kinigi Center, Musanze",
                District = "Musanze",
                Status = ListingStatus.Active,
                Description = "Sweet, crisp organic carrots washed and packed in 50kg crates."
            });

            data.Listings.Add(new Listing
            {
                ListingId = data.NextListingId++,
                FarmerId = 3,
                FarmerName = "Marie Claire Uwimana",
                ProduceName = "Yellow Hybrid Maize",
                Category = "Cereals",
                Quantity = 3500m,
                OriginalQuantity = 4000m,
                Unit = "kg",
                PricePerUnit = 380m,
                HarvestDate = DateTime.UtcNow.AddDays(-5),
                PickupLocation = "COOPAMA Warehouse, Nyagatare",
                District = "Nyagatare",
                Status = ListingStatus.Active,
                Description = "Dry grade 1 yellow maize grain, moisture content below 13.5%."
            });

            data.Listings.Add(new Listing
            {
                ListingId = data.NextListingId++,
                FarmerId = 3,
                FarmerName = "Marie Claire Uwimana",
                ProduceName = "Red Kidney Beans (Colta)",
                Category = "Legumes",
                Quantity = 800m,
                OriginalQuantity = 1000m,
                Unit = "kg",
                PricePerUnit = 850m,
                HarvestDate = DateTime.UtcNow.AddDays(-4),
                PickupLocation = "Nyagatare Agro Center",
                District = "Nyagatare",
                Status = ListingStatus.Active,
                Description = "Clean, pest-free dried red kidney beans, high protein content."
            });

            data.Listings.Add(new Listing
            {
                ListingId = data.NextListingId++,
                FarmerId = 2,
                FarmerName = "Jean Claude Hakizimana",
                ProduceName = "Hass Avocados (Export Quality)",
                Category = "Fruits",
                Quantity = 600m,
                OriginalQuantity = 600m,
                Unit = "kg",
                PricePerUnit = 700m,
                HarvestDate = DateTime.UtcNow.AddDays(-1),
                PickupLocation = "Musanze Highland Orchards",
                District = "Musanze",
                Status = ListingStatus.Active,
                Description = "Rich buttery Hass avocados, handpicked with stems intact."
            });

            // Market Prices
            string[] districts = { "Kigali (Kimironko)", "Musanze Central", "Nyagatare Market", "Huye Market", "Rubavu Border" };
            (string crop, string cat, decimal p1, decimal p2)[] samplePrices = {
                ("Irish Potatoes (Kinigi)", "Tubers", 500m, 480m),
                ("Yellow Maize Grain", "Cereals", 410m, 390m),
                ("Dry Beans (Colta)", "Legumes", 900m, 920m),
                ("Fresh Tomatoes", "Vegetables", 650m, 600m),
                ("Carrots", "Vegetables", 380m, 400m),
                ("Hass Avocados", "Fruits", 750m, 720m),
                ("Cassava Flour", "Tubers", 600m, 600m),
                ("Bananas (Plantain)", "Fruits", 450m, 430m)
            };

            foreach (var dist in districts)
            {
                foreach (var (crop, cat, p1, p2) in samplePrices)
                {
                    data.MarketPrices.Add(new MarketPrice
                    {
                        PriceId = data.NextPriceId++,
                        CropName = crop,
                        Category = cat,
                        District = dist.Split(' ')[0],
                        MarketName = dist,
                        PricePerKg = p1,
                        PreviousPricePerKg = p2,
                        Unit = "kg",
                        RecordedDate = DateTime.UtcNow.AddDays(-1)
                    });
                }
            }

            // Advisory Articles
            data.AdvisoryArticles.Add(new AdvisoryArticle
            {
                ArticleId = data.NextArticleId++,
                Title = "Best Practices for Late Blight Management in Irish Potatoes",
                Content = "Late blight (Phytophthora infestans) is the most destructive disease of potatoes in Rwanda's volcanic highlands. Ensure fungicides are applied preventively during high-humidity seasons, practice 3-year crop rotation, and plant certified resistant seeds such as Kinigi and Cruza.",
                AuthorName = "Dr. Alice Uwase (RAB)",
                Category = "Pest Management",
                Language = "English",
                PublishedAt = DateTime.UtcNow.AddDays(-10)
            });

            data.AdvisoryArticles.Add(new AdvisoryArticle
            {
                ArticleId = data.NextArticleId++,
                Title = "Uburyo bwo kurwanya udukoko twangiza ibigori (Fall Armyworm)",
                Content = "Mu gihe cy'ihinga, sukura umurima hakiri kare. Mu gihe ubonye ibimenyetso by'inkongoro y'icyonnyi ku mababi y'ibigori, koresha imiti yemejwe na RAB nka Spinosad cyangwa Emamectin benzoate mbere yuko ibigori bibumbura amashereka.",
                AuthorName = "Dr. Alice Uwase (RAB)",
                Category = "Pest Management",
                Language = "Kinyarwanda",
                PublishedAt = DateTime.UtcNow.AddDays(-7)
            });

            data.AdvisoryArticles.Add(new AdvisoryArticle
            {
                ArticleId = data.NextArticleId++,
                Title = "Guide pour la conservation post-récolte du maïs et des haricots",
                Content = "Assurez-vous que le taux d'humidité des grains est inférieur à 13,5% avant le stockage. Utilisez des sacs hermétiques PICS pour protéger les récoltes contre les charançons sans produits chimiques toxiques.",
                AuthorName = "Dr. Alice Uwase (RAB)",
                Category = "Post-Harvest",
                Language = "French",
                PublishedAt = DateTime.UtcNow.AddDays(-5)
            });

            // Sample Questions
            data.FarmerQuestions.Add(new FarmerQuestion
            {
                QuestionId = data.NextQuestionId++,
                FarmerId = 2,
                FarmerName = "Jean Claude Hakizimana",
                Subject = "Yellowing leaves on young potato plants",
                QuestionText = "My potato crop in Kinigi has yellow lower leaves with brown spots after last week's continuous rain. What could this be?",
                AnswerText = "This is early stage Early Blight caused by Alternaria solani. Spray with Mancozeb at 2.5g per litre of water and ensure proper drainage between ridges.",
                AnsweredBy = "Dr. Alice Uwase",
                AskedAt = DateTime.UtcNow.AddDays(-3),
                AnsweredAt = DateTime.UtcNow.AddDays(-2)
            });

            // Sample Order & Escrow Payment
            var sampleOrder = new Order
            {
                OrderId = data.NextOrderId++,
                BuyerId = 4,
                BuyerName = "Kigali Fresh Mart (Buyer)",
                BuyerPhone = "+250788444333",
                FarmerId = 2,
                FarmerName = "Jean Claude Hakizimana",
                TransporterId = 5,
                TransporterName = "Eric Nshimiyimana (Transporter)",
                ProduceTotal = 144000m,
                DeliveryFee = 15000m,
                PlatformFee = 4320m, // 3%
                DeliveryType = DeliveryType.TransporterDelivery,
                PickupAddress = "Kinigi Agro Collection Center, Musanze",
                DeliveryAddress = "Plot 45, KG 11 Ave, Kimironko Market Plaza, Kigali",
                DeliveryCode = "AC-8821",
                Status = OrderStatus.InTransit,
                CreatedAt = DateTime.UtcNow.AddDays(-1),
                AcceptedAt = DateTime.UtcNow.AddHours(-18)
            };

            sampleOrder.Items.Add(new OrderItem
            {
                OrderItemId = 1,
                OrderId = sampleOrder.OrderId,
                ListingId = 1,
                ProduceName = "Irish Potatoes (Kinigi Premium)",
                Quantity = 300m,
                Unit = "kg",
                UnitPrice = 480m
            });

            data.Orders.Add(sampleOrder);

            data.Payments.Add(new Payment
            {
                PaymentId = data.NextPaymentId++,
                OrderId = sampleOrder.OrderId,
                BuyerId = 4,
                FarmerId = 2,
                Amount = sampleOrder.TotalAmount,
                PlatformFee = sampleOrder.PlatformFee,
                Method = PaymentMethod.MTN_MoMo,
                EscrowStatus = EscrowStatus.HeldInEscrow,
                TransactionReference = "MOMO-RW-20261115-9941",
                CreatedAt = DateTime.UtcNow.AddDays(-1)
            });

            // Sample Audit Log
            data.AuditLogs.Add(new AuditLog
            {
                LogId = data.NextLogId++,
                Timestamp = DateTime.UtcNow.AddDays(-1),
                UserId = 1,
                UserName = "Admin User",
                Action = "System Initialization",
                Details = "AgriConnect database initialized with default Rwanda agricultural data."
            });

            return data;
        }
    }
}
