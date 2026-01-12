using PlusBin.Services;
using System.Windows;
using System.Windows.Controls;

namespace PlusBin.Views
{
    public partial class RegisterView : UserControl
    {
        private readonly DatabaseManager _db = new();

        public RegisterView()
        {
            InitializeComponent();
            RegisterButton.Click += OnRegisterClick;
        }

        private void OnRegisterClick(object sender, RoutedEventArgs e)
        {
            string? username = UsernameBox.Text?.Trim();
            string? password = PasswordBox.Password?.Trim();

            ErrorMessage.Text = "";
            ErrorMessage.Visibility = Visibility.Collapsed;

            if (string.IsNullOrEmpty(username) || string.IsNullOrEmpty(password))
            {
                ErrorMessage.Text = "Kullanıcı adı ve şifre gerekli.";
                ErrorMessage.Visibility = Visibility.Visible;
                return;
            }

            if (username.Length < 3 || password.Length < 6)
            {
                ErrorMessage.Text = "Kullanıcı adı en az 3, şifre en az 6 karakter olmalı.";
                ErrorMessage.Visibility = Visibility.Visible;
                return;
            }

            if (_db.RegisterUser(username, password))
            {
                var mainWindow = Window.GetWindow(this) as MainWindow;
                mainWindow?.SwitchToLogin();
            }
            else
            {
                ErrorMessage.Text = "Kayıt başarısız. Kullanıcı adı zaten alınmış olabilir.";
                ErrorMessage.Visibility = Visibility.Visible;
            }
        }
    }
}