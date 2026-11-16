using System.Collections.Generic;
using System.Linq;

namespace DBWineStore.Models
{
    // Một dòng sản phẩm trong giỏ hàng (không lưu entity EF vào Session)
    public class CartItem
    {
        public int ProductID { get; set; }
        public string ProductName { get; set; }
        public string ImageUrl { get; set; }

        // Giá tại thời điểm thêm vào giỏ
        public decimal UnitPrice { get; set; }

        public int Quantity { get; set; }

        // Tồn kho tại thời điểm thêm/cập nhật giỏ
        public int StockQuantity { get; set; }

        public decimal LineTotal
        {
            get { return UnitPrice * Quantity; }
        }
    }

    // Toàn bộ giỏ hàng, được giữ trong Session["Cart"]
    public class Cart
    {
        private readonly List<CartItem> items = new List<CartItem>();

        public IEnumerable<CartItem> Items
        {
            get { return items; }
        }

        // Thêm 1 sản phẩm. Trả true nếu thêm / tăng số lượng thành công.
        public bool Add(Product product)
        {
            if (product == null || !product.IsActive || product.StockQuantity <= 0)
            {
                return false;
            }

            var item = items.FirstOrDefault(x => x.ProductID == product.ProductID);

            if (item == null)
            {
                items.Add(new CartItem
                {
                    ProductID = product.ProductID,
                    ProductName = product.ProductName,
                    ImageUrl = product.ImageUrl,
                    UnitPrice = product.Price,
                    Quantity = 1,
                    StockQuantity = product.StockQuantity
                });
                return true;
            }

            if (item.Quantity < product.StockQuantity)
            {
                item.Quantity++;
                item.StockQuantity = product.StockQuantity;
                return true;
            }

            return false;
        }

        // Cập nhật số lượng: phải > 0 và không vượt tồn kho
        public bool Update(int productId, int quantity, int currentStock)
        {
            var item = items.FirstOrDefault(x => x.ProductID == productId);

            if (item == null)
            {
                return false;
            }

            if (quantity <= 0 || quantity > currentStock)
            {
                return false;
            }

            item.Quantity = quantity;
            item.StockQuantity = currentStock;
            return true;
        }

        public void Remove(int productId)
        {
            items.RemoveAll(x => x.ProductID == productId);
        }

        public int TotalQuantity()
        {
            return items.Sum(x => x.Quantity);
        }

        public decimal TotalMoney()
        {
            return items.Sum(x => x.LineTotal);
        }

        public void Clear()
        {
            items.Clear();
        }
    }
}
