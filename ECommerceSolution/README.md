# ECommerce .NET Windows Forms Application

## Three-Tier Architecture

```
ECommerceSolution/
├── ECommerce.Models/       # Data models and enums
├── ECommerce.Utilities/    # Helpers (Email, OTP, Password, Validation, Config, Logger)
├── ECommerce.DL/           # Data Layer (MySQL database access)
├── ECommerce.BL/           # Business Logic Layer
├── ECommerce.UI/           # Windows Forms UI (Presentation Layer)
└── Database/               # SQL schema and backup scripts
```

## Features

### User Management
- Three user roles: **Customer**, **Seller**, **Administrator**
- Email OTP verification on registration
- Role-based access control (each role sees only their dashboard)
- Ban/unban users (admin only)

### Customer Features
- Browse and search products
- Add to cart, update quantities, remove items
- Place orders with shipping address
- Write reviews for delivered orders (1-5 star rating)
- View order history and status updates
- Notification center

### Seller Features
- Add, edit, delete products
- View incoming orders
- Update order status (Pending → Confirmed → Dispatched → Shipped → Delivered)
- Notification center for new orders and reviews

### Administrator Features
- Manage all users (ban/unban)
- Manage all products (ban/unban)
- View all orders
- Manage categories
- Approve/reject admin role requests (temporary with time limit)
- Full audit log viewer
- Expired admin access auto-revocation

### Notification System
- In-app notifications for all user types
- Email notifications (configurable per user)
- Notification settings (toggle order updates, promotions, alerts, etc.)

### Security & Audit
- SHA-256 password hashing with salt (10,000 iterations)
- OTP-based email verification
- Comprehensive audit logging (all CRUD actions)
- MySQL triggers for automatic change tracking
- Input validation on all forms

### Database
- MySQL with InnoDB engine
- Full referential integrity with foreign keys
- Indexes for performance
- Full-text search on products
- Views for reporting
- Stored procedures for admin dashboard stats and monthly reports
- Scheduled events for OTP cleanup and admin access expiry
- Backup script templates (Windows Task Scheduler / Linux cron)

## Setup

### Prerequisites
- .NET 8.0 SDK
- MySQL 8.0+
- Visual Studio 2022 (or VS Code with C# extension)

### Database Setup
1. Open MySQL client
2. Run: `source Database/ecommerce_schema.sql`
3. This creates the database, tables, triggers, events, views, stored procedures, and seed data

### Application Setup
1. Open `ECommerceSolution.sln` in Visual Studio
2. Update connection settings in `ECommerce.Utilities/AppConfig.cs`:
   - `DbServer`, `DbPort`, `DbName`, `DbUser`, `DbPassword`
3. Update SMTP settings for email OTP:
   - `SmtpHost`, `SmtpPort`, `SmtpUser`, `SmtpPassword`
4. Build and run

### Default Admin Login
- Email: `admin@ecommerce.com`
- Password: `Admin@123`

> **Note:** The default admin password hash is a placeholder. On first run,
> register a new admin or update the hash via the application.

## Database Backups

### Windows (Task Scheduler)
```
mysqldump --single-transaction --routines --triggers --events --databases ecommerce_db > "C:\ECommerceBackups\backup_%date%.sql"
```

### Linux (Cron - daily at 2 AM)
```
0 2 * * * mysqldump --single-transaction --routines --triggers --events --databases ecommerce_db > /backups/ecommerce_$(date +%Y%m%d).sql
```

## NuGet Packages Used
- **MySql.Data** (8.3.0) - MySQL ADO.NET connector
- **MailKit** (4.3.0) - SMTP email sending
- **MimeKit** (4.3.0) - Email message construction

## License
MIT
