using AgriConnect.Controllers;
using AgriConnect.Models;
using AgriConnect.Repositories;

namespace AgriConnect
{
    public static class AgriConnectTests
    {
        public static int RunAllTests()
        {
            Console.WriteLine("==================================================");
            Console.WriteLine("    AgriConnect Automated Validation Suite        ");
            Console.WriteLine("==================================================");

            int passed = 0;
            int total = 0;

            void RunTest(string testName, Action testAction)
            {
                total++;
                try
                {
                    testAction();
                    Console.WriteLine($"[PASS] {testName}");
                    passed++;
                }
                catch (Exception ex)
                {
                    Console.ForegroundColor = ConsoleColor.Red;
                    Console.WriteLine($"[FAIL] {testName}: {ex.Message}");
                    Console.ResetColor();
                }
            }

            var db = AppDatabase.Instance;
            db.ResetDatabase();

            RunTest("Database Initialization & Seed Verification", () =>
            {
                Assert(db.Data.Users.Count >= 5, "Users not seeded");
                Assert(db.Data.Listings.Count >= 5, "Listings not seeded");
                Assert(db.Data.MarketPrices.Count >= 20, "Market prices not seeded");
                Assert(db.Data.AdvisoryArticles.Count >= 3, "Advisory articles not seeded");
            });

            RunTest("FR-1.1 to FR-1.5: Authentication & Registration Controller", () =>
            {
                var auth = new AuthController();
                var (succ, _, user) = auth.Login("farmer.jean@agriconnect.rw", "farmer123");
                Assert(succ && user != null, "Farmer login failed");
                Assert(user?.Role == UserRole.Farmer, "Role mismatch");

                var (regSucc, _, newUser) = auth.Register(
                    "Test Buyer David", "david.test@agriconnect.rw", "+250789999888", "pass123",
                    UserRole.Buyer, "Gasabo", "Remera", deliveryAddress: "KG 500 St");
                Assert(regSucc && newUser != null, "Registration failed");
            });

            RunTest("FR-3.1 to FR-3.4: Farmer Produce Listing Management", () =>
            {
                var farmerCtrl = new FarmerController();
                var newListing = new Listing
                {
                    FarmerId = 2,
                    FarmerName = "Jean Claude Hakizimana",
                    ProduceName = "Test Organic Sweet Potatoes",
                    Category = "Tubers",
                    Quantity = 200,
                    Unit = "kg",
                    PricePerUnit = 400,
                    District = "Musanze",
                    PickupLocation = "Kinigi Station"
                };

                var (succ, _) = farmerCtrl.CreateListing(newListing);
                Assert(succ, "Create listing failed");
                Assert(newListing.ListingId > 0, "ListingId not assigned");

                var (pauseSucc, _) = farmerCtrl.TogglePauseListing(newListing.ListingId);
                Assert(pauseSucc, "Pause listing failed");
                Assert(newListing.Status == ListingStatus.Paused, "Status not paused");
            });

            RunTest("FR-3.6: Fair Price Suggestion Engine", () =>
            {
                var farmerCtrl = new FarmerController();
                var (min, max, avg) = farmerCtrl.SuggestFairPrice("Irish Potatoes (Kinigi)", "Musanze");
                Assert(min > 0 && max > min && avg > 0, "Invalid suggested price range");
            });

            RunTest("FR-4.1 & FR-4.2: Buyer Search & Multi-Criteria Filtering", () =>
            {
                var buyerCtrl = new BuyerController();
                var results = buyerCtrl.SearchProduce("Potatoes", "Tubers", "Musanze", 600, "Price: Low to High");
                Assert(results.Any(), "Search returned empty results");
                Assert(results.All(r => r.PricePerUnit <= 600), "Price filter failed");
            });

            RunTest("FR-5.1 to FR-6.2: Shopping Cart, Order Placement & Escrow Hold", () =>
            {
                var buyerCtrl = new BuyerController();
                var listing = db.Data.Listings.First(l => l.Quantity >= 20);
                decimal initialStock = listing.Quantity;

                buyerCtrl.ClearCart();
                var (addSucc, _) = buyerCtrl.AddToCart(listing, 10);
                Assert(addSucc, "Add to cart failed");

                var buyer = db.Data.Users.First(u => u.Role == UserRole.Buyer);
                var (checkSucc, _, order) = buyerCtrl.Checkout(buyer, DeliveryType.TransporterDelivery, "Kigali Downtown", PaymentMethod.MTN_MoMo);
                Assert(checkSucc && order != null, "Checkout failed");

                var payment = db.Data.Payments.FirstOrDefault(p => p.OrderId == order!.OrderId);
                Assert(payment != null, "Escrow payment not generated");
                Assert(payment!.EscrowStatus == EscrowStatus.HeldInEscrow, "Payment not held in escrow");
                Assert(listing.Quantity == initialStock - 10, "Stock was not deducted");
            });

            RunTest("FR-7.3 & FR-7.4: Transporter Job Claim & In-Transit Lifecycle", () =>
            {
                var transCtrl = new TransporterController();
                var openJob = db.Data.Orders.First(o => o.DeliveryType == DeliveryType.TransporterDelivery && o.TransporterId == null);

                var (claimSucc, _) = transCtrl.AcceptDeliveryJob(openJob.OrderId, 5, "Eric Nshimiyimana");
                Assert(claimSucc, "Transporter job claim failed");
                Assert(openJob.Status == OrderStatus.InTransit, "Order status not set to InTransit");

                var (pickupSucc, _) = transCtrl.ConfirmPickup(openJob.OrderId);
                Assert(pickupSucc, "Pickup confirmation failed");
            });

            RunTest("FR-6.3 & FR-7.4: Delivery Confirmation & Escrow Release to Farmer", () =>
            {
                var buyerCtrl = new BuyerController();
                var inTransitOrder = db.Data.Orders.First(o => o.Status == OrderStatus.InTransit);
                int farmerId = inTransitOrder.FarmerId;
                var farmer = db.Data.Users.First(u => u.UserId == farmerId);
                decimal oldBalance = farmer.WalletBalance;

                var (relSucc, _) = buyerCtrl.ConfirmDeliveryAndReleaseEscrow(inTransitOrder.OrderId, inTransitOrder.DeliveryCode);
                Assert(relSucc, "Escrow release failed");
                Assert(inTransitOrder.Status == OrderStatus.Completed, "Order not marked Completed");
                Assert(farmer.WalletBalance > oldBalance, "Farmer did not receive escrow payout");
            });

            RunTest("FR-10.3 & FR-10.4: Dispute Arbitration & Buyer Refund Flow", () =>
            {
                var adminCtrl = new AdminController();
                var disputeRepo = new DisputeRepository();

                // Create a test order and escrow
                var order = new Order
                {
                    BuyerId = 4,
                    FarmerId = 2,
                    ProduceTotal = 20000,
                    DeliveryFee = 0,
                    PlatformFee = 600,
                    Status = OrderStatus.Placed
                };
                db.Data.Orders.Add(order);
                var payment = new Payment
                {
                    OrderId = order.OrderId,
                    BuyerId = 4,
                    FarmerId = 2,
                    Amount = 20600,
                    EscrowStatus = EscrowStatus.HeldInEscrow
                };
                db.Data.Payments.Add(payment);

                var dispute = disputeRepo.Create(new Dispute
                {
                    OrderId = order.OrderId,
                    RaisedByUserId = 4,
                    RaisedByName = "Buyer",
                    Reason = "Produce arrived spoiled",
                    Status = DisputeStatus.Open
                });

                var (resSucc, _) = adminCtrl.ResolveDispute(dispute.DisputeId, DisputeStatus.ResolvedRefundBuyer, "Refund approved after photo evidence inspection");
                Assert(resSucc, "Dispute resolution failed");
                Assert(payment.EscrowStatus == EscrowStatus.RefundedToBuyer, "Payment not refunded to buyer");
                Assert(order.Status == OrderStatus.Cancelled, "Order not cancelled upon dispute refund");
            });

            RunTest("FR-12.5 & FR-12.6: Admin CSV Export & Audit Logging", () =>
            {
                var adminCtrl = new AdminController();
                string csv = adminCtrl.ExportReportsCsv();
                Assert(!string.IsNullOrWhiteSpace(csv), "CSV report export empty");
                Assert(csv.Contains("OrderId,BuyerName,FarmerName"), "CSV header missing");

                var logs = adminCtrl.GetAuditLogs();
                Assert(logs.Any(), "No audit logs recorded");
            });

            Console.WriteLine("==================================================");
            Console.WriteLine($"RESULT: {passed}/{total} Test Suites Passed ({passed * 100 / total}%)");
            Console.WriteLine("==================================================");

            return passed == total ? 0 : 1;
        }

        private static void Assert(bool condition, string message)
        {
            if (!condition) throw new InvalidOperationException(message);
        }
    }
}
