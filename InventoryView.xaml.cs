using PlusBin.Models;
using PlusBin.Services;
using System.Collections.ObjectModel;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;

namespace PlusBin.Views
{
    public partial class InventoryView : UserControl
    {
        private readonly DatabaseManager _db = new();
        private readonly ObservableCollection<Product> _products = new();

        public InventoryView()
        {
            InitializeComponent();
            ProductsDataGrid.ItemsSource = _products;
            LoadProducts();
            SearchBox.TextChanged += OnSearchTextChanged;
            AddProductButton.Click += OnAddProductClick;
            ProductsDataGrid.SelectionChanged += OnProductSelected;
            ProductsDataGrid.MouseRightButtonUp += OnDataGridRightClick;
        }

        private void LoadProducts(string searchTerm = "")
        {
            _products.Clear();
            var products = _db.GetProducts(searchTerm);
            foreach (var p in products)
            {
                _products.Add(p);
            }
        }

        private void OnSearchTextChanged(object sender, TextChangedEventArgs e)
        {
            string term = SearchBox.Text?.Trim() ?? "";
            LoadProducts(term);
        }

        private void OnAddProductClick(object sender, RoutedEventArgs e)
        {
            var mainWindow = Window.GetWindow(this) as MainWindow;
            if (mainWindow != null)
            {
                mainWindow.SwitchToAddProduct(() => LoadProducts());
            }
        }

        private void OnDataGridRightClick(object sender, MouseButtonEventArgs e)
        {
            if (ProductsDataGrid.SelectedItem is Product product)
            {
                var contextMenu = new ContextMenu();
                
                var editMenuItem = new MenuItem { Header = "Düzenle" };
                editMenuItem.Click += (s, args) => EditProduct(product);
                
                var deleteMenuItem = new MenuItem { Header = "Sil" };
                deleteMenuItem.Click += (s, args) => DeleteProduct(product);
                
                contextMenu.Items.Add(editMenuItem);
                contextMenu.Items.Add(deleteMenuItem);
                
                contextMenu.IsOpen = true;
            }
        }

        private void EditProduct(Product product)
        {
            var mainWindow = Window.GetWindow(this) as MainWindow;
            if (mainWindow != null)
            {
                mainWindow.SwitchToEditProduct(product, () => LoadProducts());
            }
        }

        private void DeleteProduct(Product product)
        {
            var result = MessageBox.Show(
                $"\"{product.Name}\" ürününü silmek istiyor musunuz?",
                "Onayla",
                MessageBoxButton.YesNo,
                MessageBoxImage.Warning);

            if (result == MessageBoxResult.Yes)
            {
                _db.DeleteProduct(product.Id);
                LoadProducts();
                ClearPreview();
            }
        }

        private void OnProductSelected(object sender, SelectionChangedEventArgs e)
        {
            if (ProductsDataGrid.SelectedItem is Product product)
            {
                ProductNamePreview.Text = product.Name;
                CostPreview.Text = $"Maliyet: {product.CostValue:F2} ₺";
                SellPreview.Text = $"Satış: {product.SellValue:F2} ₺";
            }
            else
            {
                ClearPreview();
            }
        }

        private void ClearPreview()
        {
            ProductNamePreview.Text = "";
            CostPreview.Text = "";
            SellPreview.Text = "";
        }
    }
}