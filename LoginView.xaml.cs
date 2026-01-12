using PlusBin.Services;
using Microsoft.Data.Sqlite;
using System;
using System.Windows;
using System.Windows.Controls;

namespace PlusBin.Views
{
    public partial class LoginView : UserControl
    {
        private readonly DatabaseManager _db = new();

        public LoginView()
        {
            InitializeComponent();
            LoginButton.Click += OnLoginClick;
        }

        private void OnLoginClick(object sender, RoutedEventArgs e)
        {
            string? username = UsernameBox.Text?.Trim();
            string? password = PasswordBox.Password?.Trim();

            ErrorMessage.Text = "";
            ErrorMessage.Visibility = Visibility.Collapsed;

            if (string.IsNullOrEmpty(username) || string.IsNullOrEmpty(password))
            {
                ErrorMessage.Text = "Kullanıcı adı ve şifre girin.";
                ErrorMessage.Visibility = Visibility.Visible;
                return;
            }

            if (IsRateLimited())
            {
                ErrorMessage.Text = "Çok fazla hatalı giriş yaptınız. Lütfen 1 dakika bekleyin.";
                ErrorMessage.Visibility = Visibility.Visible;
                return;
            }

            if (_db.ValidateCredentials(username, password))
            {
                var mainWindow = Window.GetWindow(this) as MainWindow;
                if (mainWindow != null)
                {
                    mainWindow.SwitchToInventory();
                }
            }
            else
            {
                HandleFailedAttempt();
                ErrorMessage.Text = "Yanlış kullanıcı adı veya şifre.";
                ErrorMessage.Visibility = Visibility.Visible;
            }
        }

        private bool IsRateLimited()
        {
            using var connection = new SqliteConnection("Data Source=database/plusbin.db");
            connection.Open();
            using var cmd = new SqliteCommand("SELECT failed_attempts, last_attempt FROM users LIMIT 1", connection);
            using var reader = cmd.ExecuteReader();
            if (reader.Read())
            {
                var attempts = reader.GetInt32(0);
                if (attempts >= 3 && !reader.IsDBNull(1))
                {
                    var lastAttemptStr = reader.GetString(1);
                    if (DateTime.TryParse(lastAttemptStr, out DateTime lastAttempt))
                    {
                        if (DateTime.Now - lastAttempt < TimeSpan.FromMinutes(1))
                        {
                            return true;
                        }
                    }
                }
            }
            return false;
        }

        private void HandleFailedAttempt()
        {
            using var connection = new SqliteConnection("Data Source=database/plusbin.db");
            connection.Open();

            int currentAttempts = 0;
            using (var cmd = new SqliteCommand("SELECT failed_attempts FROM users LIMIT 1", connection))
            using (var reader = cmd.ExecuteReader())
            {
                if (reader.Read())
                {
                    currentAttempts = reader.GetInt32(0);
                }
            }

            currentAttempts++;
            var lastAttempt = DateTime.Now;

            using var updateCmd = new SqliteCommand(@"
                UPDATE users SET failed_attempts = @attempts, last_attempt = @time", connection);
            updateCmd.Parameters.AddWithValue("@attempts", currentAttempts);
            updateCmd.Parameters.AddWithValue("@time", lastAttempt.ToString("o"));
            updateCmd.ExecuteNonQuery();
        }
    }
}