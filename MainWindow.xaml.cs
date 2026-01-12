using PlusBin.Views;
using PlusBin.Services;
using PlusBin.Models;
using System.Windows;

namespace PlusBin
{
    public partial class MainWindow : Window
    {
        private readonly DatabaseManager _db = new();

        public MainWindow()
        {
            InitializeComponent();
            LoadInitialView();
        }

        private void LoadInitialView()
        {
            if (_db.IsUserRegistered())
            {
                MainContent.Content = new LoginView();
            }
            else
            {
                MainContent.Content = new RegisterView();
            }
        }

        public void SwitchToLogin()
        {
            MainContent.Content = new LoginView();
        }

        public void SwitchToInventory()
        {
            try
            {
                MainContent.Content = new InventoryView();
            }
            catch (System.Exception ex)
            {
                MessageBox.Show($"Envanter ekranı açılamadı:\n{ex.Message}", "Hata", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        public void SwitchToAddProduct(System.Action onSaved)
        {
            MainContent.Content = new AddProductView(onSaved);
        }

        public void SwitchToEditProduct(Product product, System.Action onSaved)
        {
            MainContent.Content = new AddProductView(product, onSaved);
        }
    }
}