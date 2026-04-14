-- ============================================================
-- ECommerce Database Schema for MySQL
-- Generated for Three-Tier .NET WinForms Application
-- ============================================================

CREATE DATABASE IF NOT EXISTS ecommerce_db
    CHARACTER SET utf8mb4
    COLLATE utf8mb4_unicode_ci;

USE ecommerce_db;

-- ============================================================
-- USERS TABLE
-- ============================================================
CREATE TABLE IF NOT EXISTS users (
    user_id INT AUTO_INCREMENT PRIMARY KEY,
    full_name VARCHAR(100) NOT NULL,
    email VARCHAR(255) NOT NULL UNIQUE,
    password_hash VARCHAR(500) NOT NULL,
    phone VARCHAR(20),
    role INT NOT NULL DEFAULT 1 COMMENT '1=Customer, 2=Seller, 3=Administrator',
    is_email_verified BOOLEAN NOT NULL DEFAULT FALSE,
    is_banned BOOLEAN NOT NULL DEFAULT FALSE,
    ban_reason VARCHAR(500),
    is_active BOOLEAN NOT NULL DEFAULT TRUE,
    profile_image_url VARCHAR(500),
    created_at DATETIME NOT NULL DEFAULT CURRENT_TIMESTAMP,
    updated_at DATETIME ON UPDATE CURRENT_TIMESTAMP,
    last_login_at DATETIME,
    INDEX idx_users_email (email),
    INDEX idx_users_role (role)
) ENGINE=InnoDB;

-- ============================================================
-- OTP VERIFICATIONS
-- ============================================================
CREATE TABLE IF NOT EXISTS otp_verifications (
    otp_id INT AUTO_INCREMENT PRIMARY KEY,
    user_id INT NOT NULL,
    otp_code VARCHAR(10) NOT NULL,
    email VARCHAR(255) NOT NULL,
    purpose VARCHAR(50) NOT NULL DEFAULT 'EmailVerification',
    is_used BOOLEAN NOT NULL DEFAULT FALSE,
    created_at DATETIME NOT NULL DEFAULT CURRENT_TIMESTAMP,
    expires_at DATETIME NOT NULL,
    FOREIGN KEY (user_id) REFERENCES users(user_id) ON DELETE CASCADE,
    INDEX idx_otp_email (email, purpose)
) ENGINE=InnoDB;

-- ============================================================
-- CATEGORIES
-- ============================================================
CREATE TABLE IF NOT EXISTS categories (
    category_id INT AUTO_INCREMENT PRIMARY KEY,
    category_name VARCHAR(100) NOT NULL,
    description VARCHAR(500),
    parent_category_id INT,
    is_active BOOLEAN NOT NULL DEFAULT TRUE,
    created_at DATETIME NOT NULL DEFAULT CURRENT_TIMESTAMP,
    FOREIGN KEY (parent_category_id) REFERENCES categories(category_id) ON DELETE SET NULL,
    INDEX idx_cat_parent (parent_category_id)
) ENGINE=InnoDB;

-- ============================================================
-- PRODUCTS
-- ============================================================
CREATE TABLE IF NOT EXISTS products (
    product_id INT AUTO_INCREMENT PRIMARY KEY,
    seller_id INT NOT NULL,
    category_id INT NOT NULL,
    product_name VARCHAR(200) NOT NULL,
    description TEXT,
    price DECIMAL(12,2) NOT NULL,
    discount_price DECIMAL(12,2),
    stock_quantity INT NOT NULL DEFAULT 0,
    image_url VARCHAR(500),
    is_active BOOLEAN NOT NULL DEFAULT TRUE,
    is_banned BOOLEAN NOT NULL DEFAULT FALSE,
    ban_reason VARCHAR(500),
    average_rating DOUBLE NOT NULL DEFAULT 0,
    total_reviews INT NOT NULL DEFAULT 0,
    created_at DATETIME NOT NULL DEFAULT CURRENT_TIMESTAMP,
    updated_at DATETIME ON UPDATE CURRENT_TIMESTAMP,
    FOREIGN KEY (seller_id) REFERENCES users(user_id) ON DELETE CASCADE,
    FOREIGN KEY (category_id) REFERENCES categories(category_id) ON DELETE RESTRICT,
    INDEX idx_prod_seller (seller_id),
    INDEX idx_prod_category (category_id),
    INDEX idx_prod_active (is_active, is_banned),
    FULLTEXT INDEX idx_prod_search (product_name, description)
) ENGINE=InnoDB;

-- ============================================================
-- CART ITEMS
-- ============================================================
CREATE TABLE IF NOT EXISTS cart_items (
    cart_item_id INT AUTO_INCREMENT PRIMARY KEY,
    customer_id INT NOT NULL,
    product_id INT NOT NULL,
    quantity INT NOT NULL DEFAULT 1,
    added_at DATETIME NOT NULL DEFAULT CURRENT_TIMESTAMP,
    FOREIGN KEY (customer_id) REFERENCES users(user_id) ON DELETE CASCADE,
    FOREIGN KEY (product_id) REFERENCES products(product_id) ON DELETE CASCADE,
    UNIQUE KEY uk_cart_customer_product (customer_id, product_id)
) ENGINE=InnoDB;

-- ============================================================
-- ORDERS
-- ============================================================
CREATE TABLE IF NOT EXISTS orders (
    order_id INT AUTO_INCREMENT PRIMARY KEY,
    customer_id INT NOT NULL,
    total_amount DECIMAL(12,2) NOT NULL,
    shipping_address VARCHAR(500) NOT NULL,
    payment_method VARCHAR(50) DEFAULT 'COD',
    status INT NOT NULL DEFAULT 1 COMMENT '1=Pending,2=Confirmed,3=Dispatched,4=Shipped,5=Delivered,6=Cancelled,7=Returned',
    order_date DATETIME NOT NULL DEFAULT CURRENT_TIMESTAMP,
    updated_at DATETIME ON UPDATE CURRENT_TIMESTAMP,
    FOREIGN KEY (customer_id) REFERENCES users(user_id) ON DELETE CASCADE,
    INDEX idx_order_customer (customer_id),
    INDEX idx_order_status (status),
    INDEX idx_order_date (order_date)
) ENGINE=InnoDB;

-- ============================================================
-- ORDER ITEMS
-- ============================================================
CREATE TABLE IF NOT EXISTS order_items (
    order_item_id INT AUTO_INCREMENT PRIMARY KEY,
    order_id INT NOT NULL,
    product_id INT NOT NULL,
    seller_id INT NOT NULL,
    quantity INT NOT NULL,
    unit_price DECIMAL(12,2) NOT NULL,
    total_price DECIMAL(12,2) NOT NULL,
    FOREIGN KEY (order_id) REFERENCES orders(order_id) ON DELETE CASCADE,
    FOREIGN KEY (product_id) REFERENCES products(product_id) ON DELETE RESTRICT,
    FOREIGN KEY (seller_id) REFERENCES users(user_id) ON DELETE RESTRICT,
    INDEX idx_oi_order (order_id),
    INDEX idx_oi_seller (seller_id)
) ENGINE=InnoDB;

-- ============================================================
-- REVIEWS
-- ============================================================
CREATE TABLE IF NOT EXISTS reviews (
    review_id INT AUTO_INCREMENT PRIMARY KEY,
    product_id INT NOT NULL,
    customer_id INT NOT NULL,
    order_id INT NOT NULL,
    rating INT NOT NULL CHECK (rating BETWEEN 1 AND 5),
    comment TEXT,
    created_at DATETIME NOT NULL DEFAULT CURRENT_TIMESTAMP,
    FOREIGN KEY (product_id) REFERENCES products(product_id) ON DELETE CASCADE,
    FOREIGN KEY (customer_id) REFERENCES users(user_id) ON DELETE CASCADE,
    FOREIGN KEY (order_id) REFERENCES orders(order_id) ON DELETE CASCADE,
    UNIQUE KEY uk_review_customer_product_order (customer_id, product_id, order_id),
    INDEX idx_review_product (product_id)
) ENGINE=InnoDB;

-- ============================================================
-- NOTIFICATIONS
-- ============================================================
CREATE TABLE IF NOT EXISTS notifications (
    notification_id INT AUTO_INCREMENT PRIMARY KEY,
    user_id INT NOT NULL,
    title VARCHAR(200) NOT NULL,
    message TEXT NOT NULL,
    type INT NOT NULL DEFAULT 1 COMMENT '1=OrderUpdate,2=Promotion,3=AccountAlert,4=Review,5=AdminAlert,6=System',
    is_read BOOLEAN NOT NULL DEFAULT FALSE,
    send_email BOOLEAN NOT NULL DEFAULT FALSE,
    created_at DATETIME NOT NULL DEFAULT CURRENT_TIMESTAMP,
    FOREIGN KEY (user_id) REFERENCES users(user_id) ON DELETE CASCADE,
    INDEX idx_notif_user (user_id, is_read),
    INDEX idx_notif_date (created_at)
) ENGINE=InnoDB;

-- ============================================================
-- NOTIFICATION SETTINGS
-- ============================================================
CREATE TABLE IF NOT EXISTS notification_settings (
    setting_id INT AUTO_INCREMENT PRIMARY KEY,
    user_id INT NOT NULL UNIQUE,
    email_notifications BOOLEAN NOT NULL DEFAULT TRUE,
    order_updates BOOLEAN NOT NULL DEFAULT TRUE,
    promotional_emails BOOLEAN NOT NULL DEFAULT TRUE,
    account_alerts BOOLEAN NOT NULL DEFAULT TRUE,
    review_notifications BOOLEAN NOT NULL DEFAULT TRUE,
    admin_alerts BOOLEAN NOT NULL DEFAULT TRUE,
    FOREIGN KEY (user_id) REFERENCES users(user_id) ON DELETE CASCADE
) ENGINE=InnoDB;

-- ============================================================
-- ADMIN ROLE REQUESTS
-- ============================================================
CREATE TABLE IF NOT EXISTS admin_role_requests (
    request_id INT AUTO_INCREMENT PRIMARY KEY,
    requester_id INT NOT NULL,
    approved_by_id INT,
    reason VARCHAR(500) NOT NULL,
    status VARCHAR(20) NOT NULL DEFAULT 'Pending' COMMENT 'Pending, Approved, Rejected, Expired',
    time_limit DATETIME COMMENT 'NULL means permanent if approved',
    created_at DATETIME NOT NULL DEFAULT CURRENT_TIMESTAMP,
    resolved_at DATETIME,
    FOREIGN KEY (requester_id) REFERENCES users(user_id) ON DELETE CASCADE,
    FOREIGN KEY (approved_by_id) REFERENCES users(user_id) ON DELETE SET NULL,
    INDEX idx_adminreq_status (status)
) ENGINE=InnoDB;

-- ============================================================
-- AUDIT LOGS
-- ============================================================
CREATE TABLE IF NOT EXISTS audit_logs (
    log_id BIGINT AUTO_INCREMENT PRIMARY KEY,
    user_id INT,
    action VARCHAR(100) NOT NULL,
    table_name VARCHAR(100) NOT NULL,
    record_id INT,
    old_values TEXT,
    new_values TEXT,
    ip_address VARCHAR(50),
    created_at DATETIME NOT NULL DEFAULT CURRENT_TIMESTAMP,
    FOREIGN KEY (user_id) REFERENCES users(user_id) ON DELETE SET NULL,
    INDEX idx_log_user (user_id),
    INDEX idx_log_action (action),
    INDEX idx_log_date (created_at),
    INDEX idx_log_table (table_name)
) ENGINE=InnoDB;

-- ============================================================
-- SEED DATA
-- ============================================================

-- Default Administrator (password: Admin@123)
INSERT INTO users (full_name, email, password_hash, phone, role, is_email_verified, is_active)
VALUES ('System Administrator', 'admin@ecommerce.com',
        'nPx4yRmOaK0jhsfQ==:e3VH8Y/2WvZ0xQfZ9gKJhQ+7LrBkFd4b+FnSJcXJ6kE=',
        '+1234567890', 3, TRUE, TRUE);

-- Default Categories
INSERT INTO categories (category_name, description) VALUES
('Electronics', 'Electronic devices and accessories'),
('Clothing', 'Men and women apparel'),
('Home & Garden', 'Furniture, decor, and garden supplies'),
('Books', 'Physical and digital books'),
('Sports & Outdoors', 'Sporting goods and outdoor equipment'),
('Health & Beauty', 'Health products and beauty supplies'),
('Toys & Games', 'Toys, games, and entertainment'),
('Food & Beverages', 'Grocery and gourmet food'),
('Automotive', 'Car parts and accessories'),
('Jewelry & Watches', 'Fine jewelry and watches');

-- ============================================================
-- TRIGGERS FOR AUDIT LOGGING
-- ============================================================

DELIMITER //

CREATE TRIGGER trg_user_update_log
AFTER UPDATE ON users
FOR EACH ROW
BEGIN
    IF OLD.is_banned != NEW.is_banned THEN
        INSERT INTO audit_logs (user_id, action, table_name, record_id, old_values, new_values)
        VALUES (NEW.user_id, IF(NEW.is_banned, 'USER_BANNED', 'USER_UNBANNED'), 'users', NEW.user_id,
                CONCAT('is_banned=', OLD.is_banned), CONCAT('is_banned=', NEW.is_banned));
    END IF;
END//

CREATE TRIGGER trg_order_status_log
AFTER UPDATE ON orders
FOR EACH ROW
BEGIN
    IF OLD.status != NEW.status THEN
        INSERT INTO audit_logs (user_id, action, table_name, record_id, old_values, new_values)
        VALUES (NULL, 'ORDER_STATUS_CHANGE', 'orders', NEW.order_id,
                CONCAT('status=', OLD.status), CONCAT('status=', NEW.status));
    END IF;
END//

CREATE TRIGGER trg_product_ban_log
AFTER UPDATE ON products
FOR EACH ROW
BEGIN
    IF OLD.is_banned != NEW.is_banned THEN
        INSERT INTO audit_logs (user_id, action, table_name, record_id, old_values, new_values)
        VALUES (NULL, IF(NEW.is_banned, 'PRODUCT_BANNED', 'PRODUCT_UNBANNED'), 'products', NEW.product_id,
                CONCAT('is_banned=', OLD.is_banned), CONCAT('is_banned=', NEW.is_banned));
    END IF;
END//

DELIMITER ;

-- ============================================================
-- EVENTS FOR CLEANUP AND MAINTENANCE
-- ============================================================

-- Enable event scheduler (run: SET GLOBAL event_scheduler = ON;)

DELIMITER //

-- Cleanup expired OTPs (runs daily)
CREATE EVENT IF NOT EXISTS evt_cleanup_expired_otps
ON SCHEDULE EVERY 1 DAY
DO
BEGIN
    DELETE FROM otp_verifications WHERE expires_at < NOW() AND is_used = FALSE;
END//

-- Auto-expire admin access (runs every hour)
CREATE EVENT IF NOT EXISTS evt_expire_admin_access
ON SCHEDULE EVERY 1 HOUR
DO
BEGIN
    UPDATE admin_role_requests SET status = 'Expired', resolved_at = NOW()
    WHERE status = 'Approved' AND time_limit IS NOT NULL AND time_limit < NOW();

    -- Revert roles for expired admin access (except user_id=1 which is the permanent admin)
    UPDATE users u
    INNER JOIN admin_role_requests r ON u.user_id = r.requester_id
    SET u.role = 1
    WHERE r.status = 'Expired' AND u.role = 3 AND u.user_id != 1;
END//

-- Cleanup old notifications (older than 90 days)
CREATE EVENT IF NOT EXISTS evt_cleanup_old_notifications
ON SCHEDULE EVERY 1 WEEK
DO
BEGIN
    DELETE FROM notifications WHERE created_at < DATE_SUB(NOW(), INTERVAL 90 DAY) AND is_read = TRUE;
END//

DELIMITER ;

-- ============================================================
-- VIEWS FOR REPORTING
-- ============================================================

CREATE OR REPLACE VIEW vw_order_summary AS
SELECT
    o.order_id,
    u.full_name AS customer_name,
    o.total_amount,
    o.status,
    o.order_date,
    COUNT(oi.order_item_id) AS total_items,
    GROUP_CONCAT(DISTINCT s.full_name SEPARATOR ', ') AS sellers
FROM orders o
JOIN users u ON o.customer_id = u.user_id
JOIN order_items oi ON o.order_id = oi.order_id
JOIN users s ON oi.seller_id = s.user_id
GROUP BY o.order_id;

CREATE OR REPLACE VIEW vw_product_stats AS
SELECT
    p.product_id,
    p.product_name,
    u.full_name AS seller_name,
    c.category_name,
    p.price,
    p.stock_quantity,
    p.average_rating,
    p.total_reviews,
    COALESCE(SUM(oi.quantity), 0) AS total_sold,
    COALESCE(SUM(oi.total_price), 0) AS total_revenue
FROM products p
JOIN users u ON p.seller_id = u.user_id
JOIN categories c ON p.category_id = c.category_id
LEFT JOIN order_items oi ON p.product_id = oi.product_id
GROUP BY p.product_id;

CREATE OR REPLACE VIEW vw_seller_dashboard AS
SELECT
    u.user_id AS seller_id,
    u.full_name AS seller_name,
    COUNT(DISTINCT p.product_id) AS total_products,
    COALESCE(SUM(oi.total_price), 0) AS total_revenue,
    COUNT(DISTINCT o.order_id) AS total_orders
FROM users u
LEFT JOIN products p ON u.user_id = p.seller_id AND p.is_active = 1
LEFT JOIN order_items oi ON p.product_id = oi.product_id
LEFT JOIN orders o ON oi.order_id = o.order_id
WHERE u.role = 2
GROUP BY u.user_id;

-- ============================================================
-- STORED PROCEDURES
-- ============================================================

DELIMITER //

-- Backup procedure (generates backup command)
CREATE PROCEDURE IF NOT EXISTS sp_generate_backup_command(IN backup_path VARCHAR(500))
BEGIN
    SELECT CONCAT(
        'mysqldump --single-transaction --routines --triggers --events ',
        '--databases ecommerce_db > "', backup_path, '/ecommerce_backup_',
        DATE_FORMAT(NOW(), '%Y%m%d_%H%i%s'), '.sql"'
    ) AS backup_command;
END//

-- Dashboard statistics for admin
CREATE PROCEDURE IF NOT EXISTS sp_admin_dashboard_stats()
BEGIN
    SELECT
        (SELECT COUNT(*) FROM users WHERE role = 1) AS total_customers,
        (SELECT COUNT(*) FROM users WHERE role = 2) AS total_sellers,
        (SELECT COUNT(*) FROM users WHERE role = 3) AS total_admins,
        (SELECT COUNT(*) FROM users WHERE is_banned = 1) AS banned_users,
        (SELECT COUNT(*) FROM products WHERE is_active = 1) AS active_products,
        (SELECT COUNT(*) FROM products WHERE is_banned = 1) AS banned_products,
        (SELECT COUNT(*) FROM orders) AS total_orders,
        (SELECT COALESCE(SUM(total_amount), 0) FROM orders WHERE status NOT IN (6, 7)) AS total_revenue,
        (SELECT COUNT(*) FROM orders WHERE status = 1) AS pending_orders,
        (SELECT COUNT(*) FROM admin_role_requests WHERE status = 'Pending') AS pending_admin_requests;
END//

-- Monthly sales report
CREATE PROCEDURE IF NOT EXISTS sp_monthly_sales_report(IN report_year INT, IN report_month INT)
BEGIN
    SELECT
        DATE(o.order_date) AS sale_date,
        COUNT(DISTINCT o.order_id) AS total_orders,
        SUM(o.total_amount) AS daily_revenue,
        SUM(oi.quantity) AS items_sold
    FROM orders o
    JOIN order_items oi ON o.order_id = oi.order_id
    WHERE YEAR(o.order_date) = report_year
      AND MONTH(o.order_date) = report_month
      AND o.status NOT IN (6, 7)
    GROUP BY DATE(o.order_date)
    ORDER BY sale_date;
END//

DELIMITER ;

-- ============================================================
-- BACKUP SCRIPT (to be run externally via cron/scheduler)
-- ============================================================
-- Windows Task Scheduler command:
-- mysqldump --single-transaction --routines --triggers --events --databases ecommerce_db > "C:\ECommerceBackups\ecommerce_backup_%date:~-4%%date:~4,2%%date:~7,2%.sql"
--
-- Linux cron (daily at 2 AM):
-- 0 2 * * * mysqldump --single-transaction --routines --triggers --events --databases ecommerce_db > /backups/ecommerce_backup_$(date +\%Y\%m\%d).sql

SELECT 'Schema created successfully!' AS status;
