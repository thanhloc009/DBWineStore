namespace DBWineStore.Models
{
    public class CartItemViewModel
    {
        public int ProductID { get; set; }

        public string ProductName { get; set; }

        public string ImageUrl { get; set; }

        public decimal Price { get; set; }

        public int Quantity { get; set; }

        public int StockQuantity { get; set; }

        public decimal Total
        {
            get { return Price * Quantity; }
        }
    }
}
