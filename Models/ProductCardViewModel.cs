namespace DBWineStore.Models
{
    public class ProductCardViewModel
    {
        public int ProductID { get; set; }
        public string NamePro { get; set; }
        public string CategoryName { get; set; }
        public decimal Price { get; set; }
        public string ImagePro { get; set; }

        // Tương thích view cũ
        public string ShortDescription { get; set; }

        // Giống Sport
        public string Description { get; set; }
        public int StockQuantity { get; set; }
    }
}
