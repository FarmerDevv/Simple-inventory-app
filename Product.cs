namespace PlusBin.Models
{
    public class Product
    {
        public int Id { get; set; }
        public string Name { get; set; } = string.Empty;
        public double CostValue { get; set; }
        public double SellValue { get; set; }
        public string ImagePath { get; set; } = "default.png";
    }
}