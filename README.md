# 🌱 AgriConnect Rwanda — Desktop WinForms Application (MVC)

> **A Smart Farm Produce Marketplace & Agricultural Advisory Platform**  
> Built strictly following the **Model-View-Controller (MVC)** Architecture & **IEEE 830 SRS Specification**.

---

## 📋 Overview

**AgriConnect** directly connects Rwandan smallholder farmers with buyers (households, restaurants, retailers, cooperatives), removing unnecessary middlemen. It empowers farmers with fair market price trends, weather alerts, expert agronomy advice, and guarantees payment safety through an automated **Escrow System**.

This desktop version is implemented as a full-featured, standalone C# .NET Windows Forms desktop application featuring interactive database management via GUI, multi-role dashboards, and escrow settlement.

---

## 🏛️ MVC Architecture Overview

The codebase is organized into clear architectural layers:

```
AgriConnect/
├── Models/                     # Data models, domain entities & business state
│   ├── User.cs                 # Users, roles (Farmer, Buyer, Admin, Transporter, Agronomist)
│   ├── Listing.cs              # Produce listings, categories, stock & harvest details
│   ├── Order.cs                # Order lifecycle (Placed, Accepted, Ready, InTransit, Delivered, Completed)
│   ├── Payment.cs              # Escrow payments (Held, Released, Refunded) & gateways
│   ├── MarketPrice.cs          # National market price intelligence & trends (▲▼)
│   ├── Advisory.cs             # RAB advisory articles & Farmer Q&A
│   └── DisputeAndAudit.cs      # Escrow disputes, reviews, and admin audit logging
│
├── Repositories/               # Data access layer & database persistence
│   ├── AppDatabase.cs          # Singleton persistent transactional JSON database engine
│   └── Repositories.cs         # IUserRepository, IListingRepository, IOrderRepository,
│                               # IPaymentRepository, IMarketPriceRepository,
│                               # IAdvisoryRepository, IDisputeRepository, IAuditLogRepository
│
├── Controllers/                # Business logic & controller mediation (MVC)
│   └── Controllers.cs          # AuthController, FarmerController, BuyerController,
│                               # TransporterController, MarketController, AdvisoryController,
│                               # AdminController & AppSession
│
├── Views/                      # Windows Forms graphical user interface (GUI)
│   ├── LoginForm.cs            # Sign in with instant 1-click role demo switcher
│   ├── RegisterForm.cs         # Account registration + simulated SMS/Email OTP (FR-1.2)
│   ├── MainDashboardForm.cs    # Multi-tab dashboard with responsive sidebar
│   ├── AddEditListingDialog.cs # Produce listing editor with dynamic fair price advisor
│   ├── CheckoutDialog.cs       # Shopping cart, logistics choice & escrow payment
│   ├── RaiseDisputeDialog.cs   # Formal dispute opening & evidence submission
│   └── SimpleInputDialog.cs    # Modal dialog for codes and agronomist inputs
│
├── AgriConnectTests.cs         # Automated verification suite (10 test suites covering all FRs)
├── Program.cs                  # Desktop application entry point
└── AgriConnect.csproj          # .NET Windows Forms project definition
```

---

## ✨ Features Mapped to SRS Requirements

| Feature Category | Related FRs | Implementation in AgriConnect |
|---|---|---|
| **User Authentication & Roles** | `FR-1.1 – FR-1.5` | Role-based authentication (Farmer, Buyer, Admin, Transporter, Agronomist), password hashing, OTP verification. |
| **Produce Listing Management** | `FR-3.1 – FR-3.6` | Add/Edit/Pause/Delete produce, automated stock deduction, fair price suggestion engine based on market data. |
| **Marketplace Search & Cart** | `FR-4.1 – FR-5.1` | Multi-filter search (keyword, category, district, max price, sort), live cart drawer, instant calculation. |
| **Escrow Payment Security** | `FR-6.1 – FR-6.4` | Funds held in escrow via MTN MoMo / Airtel Money / Bank Card until delivery confirmation. Auto-refunds on cancellation. |
| **Logistics & Delivery** | `FR-7.1 – FR-7.4` | Self-pickup or Transporter delivery, unique delivery security code (`AC-XXXX`), transit tracking. |
| **Market Prices Intelligence** | `FR-8.1 – FR-8.4` | Live crop prices across Rwandan markets (Kimironko, Musanze, Nyagatare, Huye, Rubavu) with price trend indicators (`▲ Rising`, `▼ Falling`, `▬ Stable`). |
| **RAB Advisory & Weather** | `FR-9.1 – FR-9.5` | Articles in English, Kinyarwanda, and French; Meteo Rwanda weather advisories; Farmer Q&A board. |
| **Dispute Arbitration** | `FR-10.3 – FR-10.4` | Escrow freeze upon dispute, admin arbitration room to refund buyer or release to farmer. |
| **Administration & Reports** | `FR-12.1 – FR-12.6` | User account suspension/reactivation, system audit logs, 1-click CSV export of order reports. |
| **Database GUI Inspector** | *Core Task* | Direct GUI view into all database tables with real-time record viewer, table stats, JSON export, and backup/restore. |

---

## 👥 Demo User Credentials

The application is pre-seeded with realistic Rwandan agricultural data. You can log in manually or click any of the **Instant Demo Login** buttons on the login screen:

| Role | Name & Location | Email / Phone | Password |
|---|---|---|---|
| **Admin** | System Administrator (Kigali) | `admin@agriconnect.rw` / `+250788111000` | `admin123` |
| **Farmer** | Jean Claude Hakizimana (Musanze) | `farmer.jean@agriconnect.rw` / `+250788222111` | `farmer123` |
| **Farmer** | Marie Claire Uwimana (Nyagatare) | `farmer.marie@agriconnect.rw` / `+250788333222` | `farmer123` |
| **Buyer** | Kigali Fresh Mart (Gasabo) | `buyer.kigali@agriconnect.rw` / `+250788444333` | `buyer123` |
| **Transporter** | Eric Nshimiyimana (Kicukiro) | `transporter.eric@agriconnect.rw` / `+250788555444` | `driver123` |
| **Agronomist** | Dr. Alice Uwase (RAB, Huye) | `agronomist.alice@agriconnect.rw` / `+250788666555` | `agri123` |

---

## 🚀 How to Run

### 1. Requirements
- Windows 10/11
- .NET SDK (8.0, 9.0, or 10.0)

### 2. Launch Desktop GUI
```bash
dotnet run
```

### 3. Run Automated Validation Test Suite
```bash
dotnet run -- --test
```
*Expected Output:*
```
==================================================
    AgriConnect Automated Validation Suite        
==================================================
[PASS] Database Initialization & Seed Verification
[PASS] FR-1.1 to FR-1.5: Authentication & Registration Controller
[PASS] FR-3.1 to FR-3.4: Farmer Produce Listing Management
[PASS] FR-3.6: Fair Price Suggestion Engine
[PASS] FR-4.1 & FR-4.2: Buyer Search & Multi-Criteria Filtering
[PASS] FR-5.1 to FR-6.2: Shopping Cart, Order Placement & Escrow Hold
[PASS] FR-7.3 & FR-7.4: Transporter Job Claim & In-Transit Lifecycle
[PASS] FR-6.3 & FR-7.4: Delivery Confirmation & Escrow Release to Farmer
[PASS] FR-10.3 & FR-10.4: Dispute Arbitration & Buyer Refund Flow
[PASS] FR-12.5 & FR-12.6: Admin CSV Export & Audit Logging
==================================================
RESULT: 10/10 Test Suites Passed (100%)
==================================================
```

---

## 🗄️ Database Implementation & GUI Interaction

The database engine (`AppDatabase`) operates as an embedded, transactional ACID-compliant relational data store with disk persistence in the `Data/agriconnect_db.json` file.

Through the **🗄️ Database GUI** tab, users can:
1. Select and inspect all database tables (`Users`, `Listings`, `Orders`, `Payments`, `MarketPrices`, `AdvisoryArticles`, `FarmerQuestions`, `Disputes`, `AuditLogs`).
2. Search and filter records in real-time.
3. View record counts and table schema.
4. **Backup Database** to an external JSON file.
5. **Restore Database** from any backup file.
6. **Reset Database** back to factory seed defaults.

---

## 📦 Pushing to GitHub

To upload this repository to your GitHub account:
```bash
git init
git add .
git commit -m "feat: complete AgriConnect WinForms MVC implementation with database GUI"
git branch -M main
git remote add origin https://github.com/<YOUR_USERNAME>/<YOUR_REPOSITORY>.git
git push -u origin main
```
