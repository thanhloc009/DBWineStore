using System.Collections.Generic;

namespace DBWineStore.Models
{
    // Gom dữ liệu Trang Chủ: danh mục, sản phẩm, bộ lọc, phân trang
    public class StoreHomeViewModel
    {
        public List<Category> Categories { get; set; }

        public List<ProductCardViewModel> Products { get; set; }

        public int? SelectedCategoryId { get; set; }
        public string SearchTerm { get; set; }
        public decimal? MinPrice { get; set; }
        public decimal? MaxPrice { get; set; }

        // "price_asc" | "price_desc" | null (mới nhất)
        public string SortOrder { get; set; }

        public int CurrentPage { get; set; }
        public int TotalPages { get; set; }
        public int TotalItems { get; set; }
    }
}
