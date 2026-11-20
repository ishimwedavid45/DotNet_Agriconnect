using System.Security.Cryptography;
using System.Text;
using System.Text.Json;
using AgriConnect.Helpers;
using AgriConnect.Models;
using MySqlConnector;

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
        private bool _isMySqlConnected = false;

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

            // First initialize seed in memory
            _data = InitializeWithSeedData();

            // Try connecting to XAMPP MySQL and initialize tables + sync
            InitializeMySql();
        }

        public string DatabasePath => _dbFilePath;
        public bool IsMySqlConnected => _isMySqlConnected;
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

        // ==================================================================
        //  MySQL Schema and Data Sync for agriconnect_db
        // ==================================================================
        public void InitializeMySql()
        {
            lock (_lock)
            {
                try
                {
                    using var conn = MySqlConnectionFactory.CreateConnection();
                    conn.Open();
                    _isMySqlConnected = true;

                    // 1. Drop irrelevant legacy tables if any
                    using (var dropCmd = new MySqlCommand("DROP TABLE IF EXISTS students;", conn))
                    {
                        dropCmd.ExecuteNonQuery();
                    }

                    // 2. Create the real AgriConnect tables
                    CreateAgriConnectTables(conn);

                    // 3. Seed tables if empty
                    SeedMySqlIfEmpty(conn);

                    // 4. Load latest state from MySQL
                    LoadFromMySql(conn);
                }
                catch (Exception ex)
                {
                    _isMySqlConnected = false;
                    System.Diagnostics.Debug.WriteLine($"MySQL connection note: {ex.Message}");
                    // Fall back to file cache if MySQL isn't reachable
                    var fileData = LoadFromFile();
                    if (fileData != null) _data = fileData;
                }
            }
        }

        private void CreateAgriConnectTables(MySqlConnection conn)
        {
            const string sql = @"
                CREATE TABLE IF NOT EXISTS users (
                    UserId INT AUTO_INCREMENT PRIMARY KEY,
                    FullName VARCHAR(150) NOT NULL,
                    Email VARCHAR(150) NOT NULL UNIQUE,
                    Phone VARCHAR(50) NOT NULL,
                    PasswordHash VARCHAR(255) NOT NULL,
                    Role VARCHAR(50) NOT NULL,
                    Status VARCHAR(50) NOT NULL DEFAULT 'Active',
                    District VARCHAR(100) NOT NULL DEFAULT 'Kigali',
                    Sector VARCHAR(100),
                    WalletBalance DECIMAL(12,2) DEFAULT 0.00,
                    FarmSize VARCHAR(100),
                    CropsGrown VARCHAR(255),
                    Cooperative VARCHAR(150),
                    DeliveryAddress TEXT,
                    CreatedAt DATETIME DEFAULT CURRENT_TIMESTAMP
                );

                CREATE TABLE IF NOT EXISTS listings (
                    ListingId INT AUTO_INCREMENT PRIMARY KEY,
                    FarmerId INT NOT NULL,
                    FarmerName VARCHAR(150) NOT NULL,
                    ProduceName VARCHAR(150) NOT NULL,
                    Category VARCHAR(100) NOT NULL,
                    Quantity DECIMAL(10,2) NOT NULL,
                    OriginalQuantity DECIMAL(10,2) NOT NULL,
                    Unit VARCHAR(50) NOT NULL DEFAULT 'kg',
                    PricePerUnit DECIMAL(10,2) NOT NULL,
                    HarvestDate DATETIME,
                    PickupLocation VARCHAR(255),
                    District VARCHAR(100) NOT NULL DEFAULT 'Musanze',
                    Status VARCHAR(50) NOT NULL DEFAULT 'Active',
                    Description TEXT,
                    ImageUrl VARCHAR(255),
                    CreatedAt DATETIME DEFAULT CURRENT_TIMESTAMP
                );

                CREATE TABLE IF NOT EXISTS orders (
                    OrderId INT AUTO_INCREMENT PRIMARY KEY,
                    BuyerId INT NOT NULL,
                    BuyerName VARCHAR(150) NOT NULL,
                    BuyerPhone VARCHAR(50),
                    FarmerId INT NOT NULL,
                    FarmerName VARCHAR(150) NOT NULL,
                    TransporterId INT,
                    TransporterName VARCHAR(150),
                    ProduceTotal DECIMAL(12,2) NOT NULL,
                    DeliveryFee DECIMAL(12,2) NOT NULL DEFAULT 0.00,
                    PlatformFee DECIMAL(12,2) NOT NULL DEFAULT 0.00,
                    TotalAmount DECIMAL(12,2) NOT NULL,
                    DeliveryType VARCHAR(50) NOT NULL,
                    PickupAddress VARCHAR(255),
                    DeliveryAddress TEXT,
                    DeliveryCode VARCHAR(50),
                    Status VARCHAR(50) NOT NULL DEFAULT 'Placed',
                    CreatedAt DATETIME DEFAULT CURRENT_TIMESTAMP,
                    AcceptedAt DATETIME,
                    DeliveredAt DATETIME,
                    ItemsJson LONGTEXT
                );

                CREATE TABLE IF NOT EXISTS payments (
                    PaymentId INT AUTO_INCREMENT PRIMARY KEY,
                    OrderId INT NOT NULL,
                    BuyerId INT NOT NULL,
                    FarmerId INT NOT NULL,
                    Amount DECIMAL(12,2) NOT NULL,
                    PlatformFee DECIMAL(12,2) DEFAULT 0.00,
                    Method VARCHAR(50) NOT NULL,
                    EscrowStatus VARCHAR(50) NOT NULL,
                    TransactionReference VARCHAR(100) NOT NULL,
                    ReleasedAt DATETIME,
                    CreatedAt DATETIME DEFAULT CURRENT_TIMESTAMP
                );

                CREATE TABLE IF NOT EXISTS market_prices (
                    PriceId INT AUTO_INCREMENT PRIMARY KEY,
                    CropName VARCHAR(100) NOT NULL,
                    Category VARCHAR(100) NOT NULL,
                    District VARCHAR(100) NOT NULL,
                    MarketName VARCHAR(100) NOT NULL,
                    PricePerKg DECIMAL(10,2) NOT NULL,
                    PreviousPricePerKg DECIMAL(10,2) NOT NULL,
                    Unit VARCHAR(50) DEFAULT 'kg',
                    RecordedDate DATETIME DEFAULT CURRENT_TIMESTAMP
                );

                CREATE TABLE IF NOT EXISTS advisory_articles (
                    ArticleId INT AUTO_INCREMENT PRIMARY KEY,
                    Title VARCHAR(255) NOT NULL,
                    Category VARCHAR(100) NOT NULL,
                    AuthorName VARCHAR(150) NOT NULL,
                    Content LONGTEXT NOT NULL,
                    Language VARCHAR(50) DEFAULT 'English',
                    PublishedAt DATETIME DEFAULT CURRENT_TIMESTAMP
                );

                CREATE TABLE IF NOT EXISTS farmer_questions (
                    QuestionId INT AUTO_INCREMENT PRIMARY KEY,
                    FarmerId INT NOT NULL,
                    FarmerName VARCHAR(150) NOT NULL,
                    Subject VARCHAR(255) NOT NULL,
                    QuestionText TEXT NOT NULL,
                    AnswerText TEXT,
                    AnsweredBy VARCHAR(150),
                    AskedAt DATETIME DEFAULT CURRENT_TIMESTAMP,
                    AnsweredAt DATETIME
                );

                CREATE TABLE IF NOT EXISTS disputes (
                    DisputeId INT AUTO_INCREMENT PRIMARY KEY,
                    OrderId INT NOT NULL,
                    RaisedByUserId INT NOT NULL,
                    RaisedByName VARCHAR(150) NOT NULL,
                    Reason VARCHAR(255) NOT NULL,
                    Details TEXT NOT NULL,
                    Status VARCHAR(50) NOT NULL DEFAULT 'Open',
                    ResolutionNotes TEXT,
                    RefundApproved TINYINT(1) DEFAULT 0,
                    CreatedAt DATETIME DEFAULT CURRENT_TIMESTAMP
                );

                CREATE TABLE IF NOT EXISTS audit_logs (
                    LogId INT AUTO_INCREMENT PRIMARY KEY,
                    Timestamp DATETIME DEFAULT CURRENT_TIMESTAMP,
                    UserId INT NOT NULL,
                    UserName VARCHAR(150) NOT NULL,
                    Action VARCHAR(100) NOT NULL,
                    Details TEXT NOT NULL
                );";

            using var cmd = new MySqlCommand(sql, conn);
            cmd.ExecuteNonQuery();
        }

        private void SeedMySqlIfEmpty(MySqlConnection conn)
        {
            using (var checkCmd = new MySqlCommand("SELECT COUNT(*) FROM users;", conn))
            {
                long userCount = Convert.ToInt64(checkCmd.ExecuteScalar());
                if (userCount > 0) return; // already seeded
            }

            // Seed Users
            foreach (var u in _data.Users)
            {
                using var cmd = new MySqlCommand(@"
                    INSERT INTO users (UserId, FullName, Email, Phone, PasswordHash, Role, Status, District, Sector, WalletBalance, FarmSize, CropsGrown, Cooperative, DeliveryAddress, CreatedAt)
                    VALUES (@id, @name, @email, @phone, @pass, @role, @status, @district, @sector, @bal, @farmSize, @crops, @coop, @addr, @created);", conn);
                cmd.Parameters.AddWithValue("@id", u.UserId);
                cmd.Parameters.AddWithValue("@name", u.FullName);
                cmd.Parameters.AddWithValue("@email", u.Email);
                cmd.Parameters.AddWithValue("@phone", u.Phone);
                cmd.Parameters.AddWithValue("@pass", u.PasswordHash);
                cmd.Parameters.AddWithValue("@role", u.Role.ToString());
                cmd.Parameters.AddWithValue("@status", u.Status.ToString());
                cmd.Parameters.AddWithValue("@district", u.District);
                cmd.Parameters.AddWithValue("@sector", u.Sector);
                cmd.Parameters.AddWithValue("@bal", u.WalletBalance);
                cmd.Parameters.AddWithValue("@farmSize", (object?)u.FarmSize ?? DBNull.Value);
                cmd.Parameters.AddWithValue("@crops", (object?)u.CropsGrown ?? DBNull.Value);
                cmd.Parameters.AddWithValue("@coop", (object?)u.Cooperative ?? DBNull.Value);
                cmd.Parameters.AddWithValue("@addr", (object?)u.DeliveryAddress ?? DBNull.Value);
                cmd.Parameters.AddWithValue("@created", u.CreatedAt);
                cmd.ExecuteNonQuery();
            }

            // Seed Listings
            foreach (var l in _data.Listings)
            {
                using var cmd = new MySqlCommand(@"
                    INSERT INTO listings (ListingId, FarmerId, FarmerName, ProduceName, Category, Quantity, OriginalQuantity, Unit, PricePerUnit, HarvestDate, PickupLocation, District, Status, Description, ImageUrl, CreatedAt)
                    VALUES (@id, @farmerId, @farmerName, @produce, @cat, @qty, @origQty, @unit, @price, @harvest, @pickup, @district, @status, @desc, @img, @created);", conn);
                cmd.Parameters.AddWithValue("@id", l.ListingId);
                cmd.Parameters.AddWithValue("@farmerId", l.FarmerId);
                cmd.Parameters.AddWithValue("@farmerName", l.FarmerName);
                cmd.Parameters.AddWithValue("@produce", l.ProduceName);
                cmd.Parameters.AddWithValue("@cat", l.Category);
                cmd.Parameters.AddWithValue("@qty", l.Quantity);
                cmd.Parameters.AddWithValue("@origQty", l.OriginalQuantity);
                cmd.Parameters.AddWithValue("@unit", l.Unit);
                cmd.Parameters.AddWithValue("@price", l.PricePerUnit);
                cmd.Parameters.AddWithValue("@harvest", l.HarvestDate);
                cmd.Parameters.AddWithValue("@pickup", l.PickupLocation);
                cmd.Parameters.AddWithValue("@district", l.District);
                cmd.Parameters.AddWithValue("@status", l.Status.ToString());
                cmd.Parameters.AddWithValue("@desc", l.Description);
                cmd.Parameters.AddWithValue("@img", l.ImageUrl);
                cmd.Parameters.AddWithValue("@created", l.CreatedAt);
                cmd.ExecuteNonQuery();
            }

            // Seed Market Prices
            foreach (var mp in _data.MarketPrices)
            {
                using var cmd = new MySqlCommand(@"
                    INSERT INTO market_prices (PriceId, CropName, Category, District, MarketName, PricePerKg, PreviousPricePerKg, Unit, RecordedDate)
                    VALUES (@id, @crop, @cat, @district, @market, @price, @prev, @unit, @rec);", conn);
                cmd.Parameters.AddWithValue("@id", mp.PriceId);
                cmd.Parameters.AddWithValue("@crop", mp.CropName);
                cmd.Parameters.AddWithValue("@cat", mp.Category);
                cmd.Parameters.AddWithValue("@district", mp.District);
                cmd.Parameters.AddWithValue("@market", mp.MarketName);
                cmd.Parameters.AddWithValue("@price", mp.PricePerKg);
                cmd.Parameters.AddWithValue("@prev", mp.PreviousPricePerKg);
                cmd.Parameters.AddWithValue("@unit", mp.Unit);
                cmd.Parameters.AddWithValue("@rec", mp.RecordedDate);
                cmd.ExecuteNonQuery();
            }

            // Seed Advisory Articles
            foreach (var a in _data.AdvisoryArticles)
            {
                using var cmd = new MySqlCommand(@"
                    INSERT INTO advisory_articles (ArticleId, Title, Category, AuthorName, Content, Language, PublishedAt)
                    VALUES (@id, @title, @cat, @author, @content, @lang, @pub);", conn);
                cmd.Parameters.AddWithValue("@id", a.ArticleId);
                cmd.Parameters.AddWithValue("@title", a.Title);
                cmd.Parameters.AddWithValue("@cat", a.Category);
                cmd.Parameters.AddWithValue("@author", a.AuthorName);
                cmd.Parameters.AddWithValue("@content", a.Content);
                cmd.Parameters.AddWithValue("@lang", a.Language);
                cmd.Parameters.AddWithValue("@pub", a.PublishedAt);
                cmd.ExecuteNonQuery();
            }

            // Seed Questions
            foreach (var q in _data.FarmerQuestions)
            {
                using var cmd = new MySqlCommand(@"
                    INSERT INTO farmer_questions (QuestionId, FarmerId, FarmerName, Subject, QuestionText, AnswerText, AnsweredBy, AskedAt, AnsweredAt)
                    VALUES (@id, @fid, @fname, @subj, @q, @ans, @ansby, @asked, @ansat);", conn);
                cmd.Parameters.AddWithValue("@id", q.QuestionId);
                cmd.Parameters.AddWithValue("@fid", q.FarmerId);
                cmd.Parameters.AddWithValue("@fname", q.FarmerName);
                cmd.Parameters.AddWithValue("@subj", q.Subject);
                cmd.Parameters.AddWithValue("@q", q.QuestionText);
                cmd.Parameters.AddWithValue("@ans", (object?)q.AnswerText ?? DBNull.Value);
                cmd.Parameters.AddWithValue("@ansby", (object?)q.AnsweredBy ?? DBNull.Value);
                cmd.Parameters.AddWithValue("@asked", q.AskedAt);
                cmd.Parameters.AddWithValue("@ansat", (object?)q.AnsweredAt ?? DBNull.Value);
                cmd.ExecuteNonQuery();
            }

            // Seed Orders
            foreach (var o in _data.Orders)
            {
                using var cmd = new MySqlCommand(@"
                    INSERT INTO orders (OrderId, BuyerId, BuyerName, BuyerPhone, FarmerId, FarmerName, TransporterId, TransporterName, ProduceTotal, DeliveryFee, PlatformFee, TotalAmount, DeliveryType, PickupAddress, DeliveryAddress, DeliveryCode, Status, CreatedAt, AcceptedAt, ItemsJson)
                    VALUES (@id, @bid, @bname, @bphone, @fid, @fname, @tid, @tname, @ptotal, @dfee, @pfee, @tot, @dtype, @pickup, @deliv, @code, @status, @created, @accepted, @items);", conn);
                cmd.Parameters.AddWithValue("@id", o.OrderId);
                cmd.Parameters.AddWithValue("@bid", o.BuyerId);
                cmd.Parameters.AddWithValue("@bname", o.BuyerName);
                cmd.Parameters.AddWithValue("@bphone", o.BuyerPhone);
                cmd.Parameters.AddWithValue("@fid", o.FarmerId);
                cmd.Parameters.AddWithValue("@fname", o.FarmerName);
                cmd.Parameters.AddWithValue("@tid", (object?)o.TransporterId ?? DBNull.Value);
                cmd.Parameters.AddWithValue("@tname", (object?)o.TransporterName ?? DBNull.Value);
                cmd.Parameters.AddWithValue("@ptotal", o.ProduceTotal);
                cmd.Parameters.AddWithValue("@dfee", o.DeliveryFee);
                cmd.Parameters.AddWithValue("@pfee", o.PlatformFee);
                cmd.Parameters.AddWithValue("@tot", o.TotalAmount);
                cmd.Parameters.AddWithValue("@dtype", o.DeliveryType.ToString());
                cmd.Parameters.AddWithValue("@pickup", o.PickupAddress);
                cmd.Parameters.AddWithValue("@deliv", o.DeliveryAddress);
                cmd.Parameters.AddWithValue("@code", o.DeliveryCode);
                cmd.Parameters.AddWithValue("@status", o.Status.ToString());
                cmd.Parameters.AddWithValue("@created", o.CreatedAt);
                cmd.Parameters.AddWithValue("@accepted", (object?)o.AcceptedAt ?? DBNull.Value);
                cmd.Parameters.AddWithValue("@items", JsonSerializer.Serialize(o.Items));
                cmd.ExecuteNonQuery();
            }

            // Seed Payments
            foreach (var p in _data.Payments)
            {
                using var cmd = new MySqlCommand(@"
                    INSERT INTO payments (PaymentId, OrderId, BuyerId, FarmerId, Amount, PlatformFee, Method, EscrowStatus, TransactionReference, ReleasedAt, CreatedAt)
                    VALUES (@id, @oid, @bid, @fid, @amount, @pfee, @method, @status, @ref, @rel, @created);", conn);
                cmd.Parameters.AddWithValue("@id", p.PaymentId);
                cmd.Parameters.AddWithValue("@oid", p.OrderId);
                cmd.Parameters.AddWithValue("@bid", p.BuyerId);
                cmd.Parameters.AddWithValue("@fid", p.FarmerId);
                cmd.Parameters.AddWithValue("@amount", p.Amount);
                cmd.Parameters.AddWithValue("@pfee", p.PlatformFee);
                cmd.Parameters.AddWithValue("@method", p.Method.ToString());
                cmd.Parameters.AddWithValue("@status", p.EscrowStatus.ToString());
                cmd.Parameters.AddWithValue("@ref", p.TransactionReference);
                cmd.Parameters.AddWithValue("@rel", (object?)p.ReleasedAt ?? DBNull.Value);
                cmd.Parameters.AddWithValue("@created", p.CreatedAt);
                cmd.ExecuteNonQuery();
            }

            // Seed Audit Logs
            foreach (var al in _data.AuditLogs)
            {
                using var cmd = new MySqlCommand(@"
                    INSERT INTO audit_logs (LogId, Timestamp, UserId, UserName, Action, Details)
                    VALUES (@id, @time, @uid, @uname, @act, @det);", conn);
                cmd.Parameters.AddWithValue("@id", al.LogId);
                cmd.Parameters.AddWithValue("@time", al.Timestamp);
                cmd.Parameters.AddWithValue("@uid", al.UserId);
                cmd.Parameters.AddWithValue("@uname", al.UserName);
                cmd.Parameters.AddWithValue("@act", al.Action);
                cmd.Parameters.AddWithValue("@det", al.Details);
                cmd.ExecuteNonQuery();
            }
        }

        private void LoadFromMySql(MySqlConnection conn)
        {
            var loaded = new DatabaseContainer();

            // Load Users
            using (var cmd = new MySqlCommand("SELECT * FROM users ORDER BY UserId;", conn))
            using (var r = cmd.ExecuteReader())
            {
                while (r.Read())
                {
                    Enum.TryParse<UserRole>(r.GetString("Role"), out var role);
                    Enum.TryParse<UserStatus>(r.GetString("Status"), out var status);
                    loaded.Users.Add(new User
                    {
                        UserId = r.GetInt32("UserId"),
                        FullName = r.GetString("FullName"),
                        Email = r.GetString("Email"),
                        Phone = r.GetString("Phone"),
                        PasswordHash = r.GetString("PasswordHash"),
                        Role = role,
                        Status = status,
                        District = r.GetString("District"),
                        Sector = r.IsDBNull(r.GetOrdinal("Sector")) ? "" : r.GetString("Sector"),
                        WalletBalance = r.GetDecimal("WalletBalance"),
                        FarmSize = r.IsDBNull(r.GetOrdinal("FarmSize")) ? null : r.GetString("FarmSize"),
                        CropsGrown = r.IsDBNull(r.GetOrdinal("CropsGrown")) ? null : r.GetString("CropsGrown"),
                        Cooperative = r.IsDBNull(r.GetOrdinal("Cooperative")) ? null : r.GetString("Cooperative"),
                        DeliveryAddress = r.IsDBNull(r.GetOrdinal("DeliveryAddress")) ? null : r.GetString("DeliveryAddress"),
                        CreatedAt = r.GetDateTime("CreatedAt")
                    });
                }
            }

            // Load Listings
            using (var cmd = new MySqlCommand("SELECT * FROM listings ORDER BY ListingId;", conn))
            using (var r = cmd.ExecuteReader())
            {
                while (r.Read())
                {
                    Enum.TryParse<ListingStatus>(r.GetString("Status"), out var status);
                    loaded.Listings.Add(new Listing
                    {
                        ListingId = r.GetInt32("ListingId"),
                        FarmerId = r.GetInt32("FarmerId"),
                        FarmerName = r.GetString("FarmerName"),
                        ProduceName = r.GetString("ProduceName"),
                        Category = r.GetString("Category"),
                        Quantity = r.GetDecimal("Quantity"),
                        OriginalQuantity = r.GetDecimal("OriginalQuantity"),
                        Unit = r.GetString("Unit"),
                        PricePerUnit = r.GetDecimal("PricePerUnit"),
                        HarvestDate = r.IsDBNull(r.GetOrdinal("HarvestDate")) ? DateTime.UtcNow : r.GetDateTime("HarvestDate"),
                        PickupLocation = r.IsDBNull(r.GetOrdinal("PickupLocation")) ? "" : r.GetString("PickupLocation"),
                        District = r.GetString("District"),
                        Status = status,
                        Description = r.IsDBNull(r.GetOrdinal("Description")) ? "" : r.GetString("Description"),
                        ImageUrl = r.IsDBNull(r.GetOrdinal("ImageUrl")) ? "" : r.GetString("ImageUrl"),
                        CreatedAt = r.GetDateTime("CreatedAt")
                    });
                }
            }

            // Load Market Prices
            using (var cmd = new MySqlCommand("SELECT * FROM market_prices ORDER BY PriceId;", conn))
            using (var r = cmd.ExecuteReader())
            {
                while (r.Read())
                {
                    loaded.MarketPrices.Add(new MarketPrice
                    {
                        PriceId = r.GetInt32("PriceId"),
                        CropName = r.GetString("CropName"),
                        Category = r.GetString("Category"),
                        District = r.GetString("District"),
                        MarketName = r.GetString("MarketName"),
                        PricePerKg = r.GetDecimal("PricePerKg"),
                        PreviousPricePerKg = r.GetDecimal("PreviousPricePerKg"),
                        Unit = r.GetString("Unit"),
                        RecordedDate = r.GetDateTime("RecordedDate")
                    });
                }
            }

            // Load Advisory Articles
            using (var cmd = new MySqlCommand("SELECT * FROM advisory_articles ORDER BY ArticleId;", conn))
            using (var r = cmd.ExecuteReader())
            {
                while (r.Read())
                {
                    loaded.AdvisoryArticles.Add(new AdvisoryArticle
                    {
                        ArticleId = r.GetInt32("ArticleId"),
                        Title = r.GetString("Title"),
                        Category = r.GetString("Category"),
                        AuthorName = r.GetString("AuthorName"),
                        Content = r.GetString("Content"),
                        Language = r.IsDBNull(r.GetOrdinal("Language")) ? "English" : r.GetString("Language"),
                        PublishedAt = r.GetDateTime("PublishedAt")
                    });
                }
            }

            // Load Farmer Questions
            using (var cmd = new MySqlCommand("SELECT * FROM farmer_questions ORDER BY QuestionId;", conn))
            using (var r = cmd.ExecuteReader())
            {
                while (r.Read())
                {
                    loaded.FarmerQuestions.Add(new FarmerQuestion
                    {
                        QuestionId = r.GetInt32("QuestionId"),
                        FarmerId = r.GetInt32("FarmerId"),
                        FarmerName = r.GetString("FarmerName"),
                        Subject = r.GetString("Subject"),
                        QuestionText = r.GetString("QuestionText"),
                        AnswerText = r.IsDBNull(r.GetOrdinal("AnswerText")) ? null : r.GetString("AnswerText"),
                        AnsweredBy = r.IsDBNull(r.GetOrdinal("AnsweredBy")) ? null : r.GetString("AnsweredBy"),
                        AskedAt = r.GetDateTime("AskedAt"),
                        AnsweredAt = r.IsDBNull(r.GetOrdinal("AnsweredAt")) ? null : r.GetDateTime("AnsweredAt")
                    });
                }
            }

            // Load Orders
            using (var cmd = new MySqlCommand("SELECT * FROM orders ORDER BY OrderId;", conn))
            using (var r = cmd.ExecuteReader())
            {
                while (r.Read())
                {
                    Enum.TryParse<OrderStatus>(r.GetString("Status"), out var status);
                    Enum.TryParse<DeliveryType>(r.GetString("DeliveryType"), out var dtype);
                    string itemsJson = r.IsDBNull(r.GetOrdinal("ItemsJson")) ? "[]" : r.GetString("ItemsJson");
                    var items = JsonSerializer.Deserialize<List<OrderItem>>(itemsJson) ?? new();

                    loaded.Orders.Add(new Order
                    {
                        OrderId = r.GetInt32("OrderId"),
                        BuyerId = r.GetInt32("BuyerId"),
                        BuyerName = r.GetString("BuyerName"),
                        BuyerPhone = r.IsDBNull(r.GetOrdinal("BuyerPhone")) ? "" : r.GetString("BuyerPhone"),
                        FarmerId = r.GetInt32("FarmerId"),
                        FarmerName = r.GetString("FarmerName"),
                        TransporterId = r.IsDBNull(r.GetOrdinal("TransporterId")) ? null : r.GetInt32("TransporterId"),
                        TransporterName = r.IsDBNull(r.GetOrdinal("TransporterName")) ? null : r.GetString("TransporterName"),
                        ProduceTotal = r.GetDecimal("ProduceTotal"),
                        DeliveryFee = r.GetDecimal("DeliveryFee"),
                        PlatformFee = r.GetDecimal("PlatformFee"),
                        DeliveryType = dtype,
                        PickupAddress = r.IsDBNull(r.GetOrdinal("PickupAddress")) ? "" : r.GetString("PickupAddress"),
                        DeliveryAddress = r.IsDBNull(r.GetOrdinal("DeliveryAddress")) ? "" : r.GetString("DeliveryAddress"),
                        DeliveryCode = r.IsDBNull(r.GetOrdinal("DeliveryCode")) ? "" : r.GetString("DeliveryCode"),
                        Status = status,
                        CreatedAt = r.GetDateTime("CreatedAt"),
                        AcceptedAt = r.IsDBNull(r.GetOrdinal("AcceptedAt")) ? null : r.GetDateTime("AcceptedAt"),
                        DeliveredAt = r.IsDBNull(r.GetOrdinal("DeliveredAt")) ? null : r.GetDateTime("DeliveredAt"),
                        Items = items
                    });
                }
            }

            // Load Payments
            using (var cmd = new MySqlCommand("SELECT * FROM payments ORDER BY PaymentId;", conn))
            using (var r = cmd.ExecuteReader())
            {
                while (r.Read())
                {
                    Enum.TryParse<PaymentMethod>(r.GetString("Method"), out var method);
                    Enum.TryParse<EscrowStatus>(r.GetString("EscrowStatus"), out var status);
                    loaded.Payments.Add(new Payment
                    {
                        PaymentId = r.GetInt32("PaymentId"),
                        OrderId = r.GetInt32("OrderId"),
                        BuyerId = r.GetInt32("BuyerId"),
                        FarmerId = r.GetInt32("FarmerId"),
                        Amount = r.GetDecimal("Amount"),
                        PlatformFee = r.GetDecimal("PlatformFee"),
                        Method = method,
                        EscrowStatus = status,
                        TransactionReference = r.GetString("TransactionReference"),
                        ReleasedAt = r.IsDBNull(r.GetOrdinal("ReleasedAt")) ? null : r.GetDateTime("ReleasedAt"),
                        CreatedAt = r.GetDateTime("CreatedAt")
                    });
                }
            }

            // Load Audit Logs
            using (var cmd = new MySqlCommand("SELECT * FROM audit_logs ORDER BY LogId DESC LIMIT 200;", conn))
            using (var r = cmd.ExecuteReader())
            {
                while (r.Read())
                {
                    loaded.AuditLogs.Add(new AuditLog
                    {
                        LogId = r.GetInt32("LogId"),
                        Timestamp = r.GetDateTime("Timestamp"),
                        UserId = r.GetInt32("UserId"),
                        UserName = r.GetString("UserName"),
                        Action = r.GetString("Action"),
                        Details = r.GetString("Details")
                    });
                }
            }

            // Set next IDs based on max in MySQL
            loaded.NextUserId = (loaded.Users.Count > 0 ? loaded.Users.Max(u => u.UserId) : 0) + 1;
            loaded.NextListingId = (loaded.Listings.Count > 0 ? loaded.Listings.Max(l => l.ListingId) : 0) + 1;
            loaded.NextOrderId = (loaded.Orders.Count > 0 ? loaded.Orders.Max(o => o.OrderId) : 0) + 1;
            loaded.NextPaymentId = (loaded.Payments.Count > 0 ? loaded.Payments.Max(p => p.PaymentId) : 0) + 1;
            loaded.NextPriceId = (loaded.MarketPrices.Count > 0 ? loaded.MarketPrices.Max(m => m.PriceId) : 0) + 1;
            loaded.NextArticleId = (loaded.AdvisoryArticles.Count > 0 ? loaded.AdvisoryArticles.Max(a => a.ArticleId) : 0) + 1;
            loaded.NextQuestionId = (loaded.FarmerQuestions.Count > 0 ? loaded.FarmerQuestions.Max(q => q.QuestionId) : 0) + 1;
            loaded.NextLogId = (loaded.AuditLogs.Count > 0 ? loaded.AuditLogs.Max(l => l.LogId) : 0) + 1;

            _data = loaded;
        }

        public void Save()
        {
            lock (_lock)
            {
                try
                {
                    // Also maintain local file backup
                    string json = JsonSerializer.Serialize(_data, _jsonOptions);
                    File.WriteAllText(_dbFilePath, json);
                }
                catch (Exception ex)
                {
                    System.Diagnostics.Debug.WriteLine($"File Save error: {ex.Message}");
                }

                // Sync changes to MySQL
                try
                {
                    using var conn = MySqlConnectionFactory.CreateConnection();
                    conn.Open();
                    _isMySqlConnected = true;

                    // Sync Listings to MySQL
                    foreach (var l in _data.Listings)
                    {
                        using var cmd = new MySqlCommand(@"
                            INSERT INTO listings (ListingId, FarmerId, FarmerName, ProduceName, Category, Quantity, OriginalQuantity, Unit, PricePerUnit, HarvestDate, PickupLocation, District, Status, Description, ImageUrl, CreatedAt)
                            VALUES (@id, @farmerId, @farmerName, @produce, @cat, @qty, @origQty, @unit, @price, @harvest, @pickup, @district, @status, @desc, @img, @created)
                            ON DUPLICATE KEY UPDATE 
                                Quantity = VALUES(Quantity),
                                PricePerUnit = VALUES(PricePerUnit),
                                Status = VALUES(Status),
                                Description = VALUES(Description);", conn);
                        cmd.Parameters.AddWithValue("@id", l.ListingId);
                        cmd.Parameters.AddWithValue("@farmerId", l.FarmerId);
                        cmd.Parameters.AddWithValue("@farmerName", l.FarmerName);
                        cmd.Parameters.AddWithValue("@produce", l.ProduceName);
                        cmd.Parameters.AddWithValue("@cat", l.Category);
                        cmd.Parameters.AddWithValue("@qty", l.Quantity);
                        cmd.Parameters.AddWithValue("@origQty", l.OriginalQuantity);
                        cmd.Parameters.AddWithValue("@unit", l.Unit);
                        cmd.Parameters.AddWithValue("@price", l.PricePerUnit);
                        cmd.Parameters.AddWithValue("@harvest", l.HarvestDate);
                        cmd.Parameters.AddWithValue("@pickup", l.PickupLocation);
                        cmd.Parameters.AddWithValue("@district", l.District);
                        cmd.Parameters.AddWithValue("@status", l.Status.ToString());
                        cmd.Parameters.AddWithValue("@desc", l.Description);
                        cmd.Parameters.AddWithValue("@img", l.ImageUrl);
                        cmd.Parameters.AddWithValue("@created", l.CreatedAt);
                        cmd.ExecuteNonQuery();
                    }

                    // Sync Users to MySQL
                    foreach (var u in _data.Users)
                    {
                        using var cmd = new MySqlCommand(@"
                            INSERT INTO users (UserId, FullName, Email, Phone, PasswordHash, Role, Status, District, Sector, WalletBalance, FarmSize, CropsGrown, Cooperative, DeliveryAddress, CreatedAt)
                            VALUES (@id, @name, @email, @phone, @pass, @role, @status, @district, @sector, @bal, @farmSize, @crops, @coop, @addr, @created)
                            ON DUPLICATE KEY UPDATE 
                                Status = VALUES(Status),
                                WalletBalance = VALUES(WalletBalance);", conn);
                        cmd.Parameters.AddWithValue("@id", u.UserId);
                        cmd.Parameters.AddWithValue("@name", u.FullName);
                        cmd.Parameters.AddWithValue("@email", u.Email);
                        cmd.Parameters.AddWithValue("@phone", u.Phone);
                        cmd.Parameters.AddWithValue("@pass", u.PasswordHash);
                        cmd.Parameters.AddWithValue("@role", u.Role.ToString());
                        cmd.Parameters.AddWithValue("@status", u.Status.ToString());
                        cmd.Parameters.AddWithValue("@district", u.District);
                        cmd.Parameters.AddWithValue("@sector", u.Sector);
                        cmd.Parameters.AddWithValue("@bal", u.WalletBalance);
                        cmd.Parameters.AddWithValue("@farmSize", (object?)u.FarmSize ?? DBNull.Value);
                        cmd.Parameters.AddWithValue("@crops", (object?)u.CropsGrown ?? DBNull.Value);
                        cmd.Parameters.AddWithValue("@coop", (object?)u.Cooperative ?? DBNull.Value);
                        cmd.Parameters.AddWithValue("@addr", (object?)u.DeliveryAddress ?? DBNull.Value);
                        cmd.Parameters.AddWithValue("@created", u.CreatedAt);
                        cmd.ExecuteNonQuery();
                    }

                    // Sync Orders to MySQL
                    foreach (var o in _data.Orders)
                    {
                        using var cmd = new MySqlCommand(@"
                            INSERT INTO orders (OrderId, BuyerId, BuyerName, BuyerPhone, FarmerId, FarmerName, TransporterId, TransporterName, ProduceTotal, DeliveryFee, PlatformFee, TotalAmount, DeliveryType, PickupAddress, DeliveryAddress, DeliveryCode, Status, CreatedAt, AcceptedAt, DeliveredAt, ItemsJson)
                            VALUES (@id, @bid, @bname, @bphone, @fid, @fname, @tid, @tname, @ptotal, @dfee, @pfee, @tot, @dtype, @pickup, @deliv, @code, @status, @created, @accepted, @delivAt, @items)
                            ON DUPLICATE KEY UPDATE 
                                Status = VALUES(Status),
                                TransporterId = VALUES(TransporterId),
                                TransporterName = VALUES(TransporterName),
                                DeliveredAt = VALUES(DeliveredAt);", conn);
                        cmd.Parameters.AddWithValue("@id", o.OrderId);
                        cmd.Parameters.AddWithValue("@bid", o.BuyerId);
                        cmd.Parameters.AddWithValue("@bname", o.BuyerName);
                        cmd.Parameters.AddWithValue("@bphone", o.BuyerPhone);
                        cmd.Parameters.AddWithValue("@fid", o.FarmerId);
                        cmd.Parameters.AddWithValue("@fname", o.FarmerName);
                        cmd.Parameters.AddWithValue("@tid", (object?)o.TransporterId ?? DBNull.Value);
                        cmd.Parameters.AddWithValue("@tname", (object?)o.TransporterName ?? DBNull.Value);
                        cmd.Parameters.AddWithValue("@ptotal", o.ProduceTotal);
                        cmd.Parameters.AddWithValue("@dfee", o.DeliveryFee);
                        cmd.Parameters.AddWithValue("@pfee", o.PlatformFee);
                        cmd.Parameters.AddWithValue("@tot", o.TotalAmount);
                        cmd.Parameters.AddWithValue("@dtype", o.DeliveryType.ToString());
                        cmd.Parameters.AddWithValue("@pickup", o.PickupAddress);
                        cmd.Parameters.AddWithValue("@deliv", o.DeliveryAddress);
                        cmd.Parameters.AddWithValue("@code", o.DeliveryCode);
                        cmd.Parameters.AddWithValue("@status", o.Status.ToString());
                        cmd.Parameters.AddWithValue("@created", o.CreatedAt);
                        cmd.Parameters.AddWithValue("@accepted", (object?)o.AcceptedAt ?? DBNull.Value);
                        cmd.Parameters.AddWithValue("@delivAt", (object?)o.DeliveredAt ?? DBNull.Value);
                        cmd.Parameters.AddWithValue("@items", JsonSerializer.Serialize(o.Items));
                        cmd.ExecuteNonQuery();
                    }

                    // Sync Payments
                    foreach (var p in _data.Payments)
                    {
                        using var cmd = new MySqlCommand(@"
                            INSERT INTO payments (PaymentId, OrderId, BuyerId, FarmerId, Amount, PlatformFee, Method, EscrowStatus, TransactionReference, ReleasedAt, CreatedAt)
                            VALUES (@id, @oid, @bid, @fid, @amount, @pfee, @method, @status, @ref, @rel, @created)
                            ON DUPLICATE KEY UPDATE 
                                EscrowStatus = VALUES(EscrowStatus),
                                ReleasedAt = VALUES(ReleasedAt);", conn);
                        cmd.Parameters.AddWithValue("@id", p.PaymentId);
                        cmd.Parameters.AddWithValue("@oid", p.OrderId);
                        cmd.Parameters.AddWithValue("@bid", p.BuyerId);
                        cmd.Parameters.AddWithValue("@fid", p.FarmerId);
                        cmd.Parameters.AddWithValue("@amount", p.Amount);
                        cmd.Parameters.AddWithValue("@pfee", p.PlatformFee);
                        cmd.Parameters.AddWithValue("@method", p.Method.ToString());
                        cmd.Parameters.AddWithValue("@status", p.EscrowStatus.ToString());
                        cmd.Parameters.AddWithValue("@ref", p.TransactionReference);
                        cmd.Parameters.AddWithValue("@rel", (object?)p.ReleasedAt ?? DBNull.Value);
                        cmd.Parameters.AddWithValue("@created", p.CreatedAt);
                        cmd.ExecuteNonQuery();
                    }
                }
                catch (Exception ex)
                {
                    System.Diagnostics.Debug.WriteLine($"MySQL sync note: {ex.Message}");
                }

                OnDatabaseChanged?.Invoke();
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
                try
                {
                    using var conn = MySqlConnectionFactory.CreateConnection();
                    conn.Open();
                    using var cmd = new MySqlCommand("DROP TABLE IF EXISTS users, listings, orders, payments, market_prices, advisory_articles, farmer_questions, disputes, audit_logs, students;", conn);
                    cmd.ExecuteNonQuery();
                    CreateAgriConnectTables(conn);
                    SeedMySqlIfEmpty(conn);
                }
                catch
                {
                }
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
