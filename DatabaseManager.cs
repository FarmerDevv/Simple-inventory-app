using Microsoft.Data.Sqlite;
using PlusBin.Models;
using PlusBin.Utils;
using System;
using System.Collections.Generic;
using System.IO;

namespace PlusBin.Services
{
    public class DatabaseManager
    {
        private readonly string _dbPath;

        public DatabaseManager()
        {
            var baseDir = AppDomain.CurrentDomain.BaseDirectory;
            var dbFolder = Path.Combine(baseDir, "database");
            Directory.CreateDirectory(dbFolder);
            _dbPath = Path.Combine(dbFolder, "plusbin.db");
            InitializeDatabase();
        }

        private void InitializeDatabase()
        {
            if (!File.Exists(_dbPath))
            {
                using var connection = new SqliteConnection($"Data Source={_dbPath}");
                connection.Open();

                var createUsersTable = @"
                    CREATE TABLE users (
                        id INTEGER PRIMARY KEY,
                        username TEXT UNIQUE NOT NULL,
                        password_hash BLOB NOT NULL,
                        salt BLOB NOT NULL,
                        failed_attempts INTEGER DEFAULT 0,
                        last_attempt TEXT
                    );";

                var createProductsTable = @"
                    CREATE TABLE products (
                        id INTEGER PRIMARY KEY AUTOINCREMENT,
                        name TEXT NOT NULL,
                        cost_value REAL NOT NULL,
                        sell_value REAL NOT NULL,
                        image_path TEXT DEFAULT 'default.png'
                    );";

                using var cmd1 = new SqliteCommand(createUsersTable, connection);
                cmd1.ExecuteNonQuery();

                using var cmd2 = new SqliteCommand(createProductsTable, connection);
                cmd2.ExecuteNonQuery();
            }
        }

        public bool IsUserRegistered()
        {
            using var connection = new SqliteConnection($"Data Source={_dbPath}");
            connection.Open();
            using var cmd = new SqliteCommand("SELECT COUNT(*) FROM users", connection);
            var result = cmd.ExecuteScalar();
            return result is long count && count > 0;
        }

        public bool RegisterUser(string username, string password)
        {
            if (string.IsNullOrWhiteSpace(username) || string.IsNullOrWhiteSpace(password))
                return false;

            if (IsUserExists(username))
                return false;

            var (hash, salt) = PasswordHasher.HashPassword(password);

            using var connection = new SqliteConnection($"Data Source={_dbPath}");
            connection.Open();

            using var cmd = new SqliteCommand(@"
                INSERT INTO users (username, password_hash, salt, failed_attempts, last_attempt)
                VALUES (@username, @hash, @salt, 0, NULL)", connection);

            cmd.Parameters.AddWithValue("@username", username);
            cmd.Parameters.AddWithValue("@hash", hash);
            cmd.Parameters.AddWithValue("@salt", salt);

            try
            {
                cmd.ExecuteNonQuery();
                return true;
            }
            catch
            {
                return false;
            }
        }

        public bool ValidateCredentials(string username, string password)
        {
            using var connection = new SqliteConnection($"Data Source={_dbPath}");
            connection.Open();

            using var cmd = new SqliteCommand(@"
                SELECT password_hash, salt FROM users 
                WHERE username = @username", connection);

            cmd.Parameters.AddWithValue("@username", username);

            using var reader = cmd.ExecuteReader();
            if (!reader.Read())
                return false;

            if (reader["password_hash"] is not byte[] storedHash ||
                reader["salt"] is not byte[] storedSalt)
                return false;

            return PasswordHasher.VerifyPassword(password, storedHash, storedSalt);
        }

        private bool IsUserExists(string username)
        {
            using var connection = new SqliteConnection($"Data Source={_dbPath}");
            connection.Open();
            using var cmd = new SqliteCommand("SELECT 1 FROM users WHERE username = @username", connection);
            cmd.Parameters.AddWithValue("@username", username);
            return cmd.ExecuteScalar() != null;
        }

        // ========== ÜRÜN İŞLEMLERİ ==========

        public void AddProduct(Product product)
        {
            using var connection = new SqliteConnection($"Data Source={_dbPath}");
            connection.Open();
            using var cmd = new SqliteCommand(@"
                INSERT INTO products (name, cost_value, sell_value, image_path)
                VALUES (@name, @cost, @sell, @image)", connection);

            cmd.Parameters.AddWithValue("@name", product.Name ?? string.Empty);
            cmd.Parameters.AddWithValue("@cost", product.CostValue);
            cmd.Parameters.AddWithValue("@sell", product.SellValue);
            cmd.Parameters.AddWithValue("@image", product.ImagePath ?? "default.png");

            cmd.ExecuteNonQuery();
        }

        public void UpdateProduct(Product product)
        {
            using var connection = new SqliteConnection($"Data Source={_dbPath}");
            connection.Open();
            using var cmd = new SqliteCommand(@"
                UPDATE products 
                SET name = @name, cost_value = @cost, sell_value = @sell, image_path = @image
                WHERE id = @id", connection);

            cmd.Parameters.AddWithValue("@id", product.Id);
            cmd.Parameters.AddWithValue("@name", product.Name ?? string.Empty);
            cmd.Parameters.AddWithValue("@cost", product.CostValue);
            cmd.Parameters.AddWithValue("@sell", product.SellValue);
            cmd.Parameters.AddWithValue("@image", product.ImagePath ?? "default.png");

            cmd.ExecuteNonQuery();
        }

        public void DeleteProduct(int productId)
        {
            using var connection = new SqliteConnection($"Data Source={_dbPath}");
            connection.Open();
            using var cmd = new SqliteCommand("DELETE FROM products WHERE id = @id", connection);
            cmd.Parameters.AddWithValue("@id", productId);
            cmd.ExecuteNonQuery();
        }

        public List<Product> GetProducts(string searchTerm = "")
        {
            var products = new List<Product>();
            using var connection = new SqliteConnection($"Data Source={_dbPath}");
            connection.Open();

            string sql = string.IsNullOrWhiteSpace(searchTerm)
                ? "SELECT id, name, cost_value, sell_value, image_path FROM products ORDER BY name"
                : "SELECT id, name, cost_value, sell_value, image_path FROM products WHERE name LIKE @term ORDER BY name";

            using var cmd = new SqliteCommand(sql, connection);
            if (!string.IsNullOrWhiteSpace(searchTerm))
            {
                cmd.Parameters.AddWithValue("@term", $"%{searchTerm}%");
            }

            using var reader = cmd.ExecuteReader();
            while (reader.Read())
            {
                products.Add(new Product
                {
                    Id = reader.GetInt32(0),
                    Name = reader.IsDBNull(1) ? "" : reader.GetString(1),
                    CostValue = reader.GetDouble(2),
                    SellValue = reader.GetDouble(3),
                    ImagePath = reader.IsDBNull(4) ? "default.png" : reader.GetString(4)
                });
            }

            return products;
        }
    }
}