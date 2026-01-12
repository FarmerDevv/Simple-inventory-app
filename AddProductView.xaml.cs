using PlusBin.Models;
using PlusBin.Services;
using System;
using System.Windows;
using System.Windows.Controls;

namespace PlusBin.Views
{
    public partial class AddProductView : UserControl
    {
        private readonly DatabaseManager _db = new();
        private readonly Action? _onSaved;
        private readonly Product? _editingProduct;

        // Yeni ürün eklemek için
        public AddProductView(Action onSaved)
        {
            InitializeComponent();
            _onSaved = onSaved ?? throw new ArgumentNullException(nameof(onSaved));
            TitleText.Text = "YENİ ÜRÜN EKLE";
            SaveButton.Content = "Kaydet";
            SaveButton.Click += OnSaveClick;
            CancelButton.Click += OnCancelClick;
        }

        // Ürün güncellemek için
        public AddProductView(Product product, Action onSaved)
        {
            InitializeComponent();
            _editingProduct = product ?? throw new ArgumentNullException(nameof(product));
            _onSaved = onSaved ?? throw new ArgumentNullException(nameof(onSaved));
            TitleText.Text = "ÜRÜNÜ GÜNCELLE";
            SaveButton.Content = "Güncelle";
            LoadProduct(product);
            SaveButton.Click += OnSaveClick;
            CancelButton.Click += OnCancelClick;
        }

        private void LoadProduct(Product p)
        {
            ProductNameBox.Text = p.Name ?? string.Empty;
            CostBox.Text = p.CostValue.ToString("F2");
            SellBox.Text = p.SellValue.ToString("F2");
        }

        private void OnCostOrSellChanged(object sender, TextChangedEventArgs e)
        {
            if (double.TryParse(CostBox.Text, out double cost) &&
                double.TryParse(SellBox.Text, out double sell) &&
                cost > 0)
            {
                double profitPercent = (sell - cost) / cost * 100;
                ProfitBox.Text = $"{profitPercent:F1} %";
            }
            else
            {
                ProfitBox.Text = "";
            }
        }

        private void OnSaveClick(object sender, RoutedEventArgs e)
        {
            string? name = ProductNameBox.Text?.Trim();
            if (string.IsNullOrEmpty(name))
            {
                ShowError("Ürün adı girin.");
                return;
            }

            if (!double.TryParse(CostBox.Text, out double cost) || cost <= 0)
            {
                ShowError("Geçerli bir maliyet girin.");
                return;
            }

            if (!double.TryParse(SellBox.Text, out double sell) || sell <= 0)
            {
                ShowError("Geçerli bir satış fiyatı girin.");
                return;
            }

            if (sell < cost)
            {
                ShowError("Satış fiyatı maliyetten düşük olamaz.");
                return;
            }

            try
            {
                if (_editingProduct != null)
                {
                    _editingProduct.Name = name;
                    _editingProduct.CostValue = cost;
                    _editingProduct.SellValue = sell;
                    _db.UpdateProduct(_editingProduct);
                }
                else
                {
                    var newProduct = new Product
                    {
                        Name = name,
                        CostValue = cost,
                        SellValue = sell
                    };
                    _db.AddProduct(newProduct);
                }

                _onSaved?.Invoke();
                var mainWindow = Window.GetWindow(this) as MainWindow;
                mainWindow?.SwitchToInventory();
            }
            catch (Exception ex)
            {
                ShowError($"Kayıt başarısız: {ex.Message}");
            }
        }

        private void OnCancelClick(object sender, RoutedEventArgs e)
        {
            var mainWindow = Window.GetWindow(this) as MainWindow;
            mainWindow?.SwitchToInventory();
        }

        private void ShowError(string message)
        {
            ErrorMessage.Text = message;
            ErrorMessage.Visibility = Visibility.Visible;
        }
    }
}