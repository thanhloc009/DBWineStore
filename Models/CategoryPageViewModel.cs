using System.Collections.Generic;

namespace DBWineStore.Models
{
    // A category shown as a chip on the category page.
    public class CategoryLinkViewModel
    {
        public int CategoryID { get; set; }
        public string CategoryName { get; set; }
        public int ProductCount { get; set; }
    }

    // Model of the "View all" page of one category.
    public class CategoryPageViewModel
    {
        public int CategoryID { get; set; }
        public string CategoryName { get; set; }

        // numbers shown in the banner (whole category, not affected by the price filter)
        public int ProductsInCategory { get; set; }
        public decimal? LowestPrice { get; set; }
        public decimal? HighestPrice { get; set; }

        public List<ProductCardViewModel> Products { get; set; }
        public List<CategoryLinkViewModel> Categories { get; set; }

        // filters + paging
        public string SortOrder { get; set; }
        public decimal? MinPrice { get; set; }
        public decimal? MaxPrice { get; set; }
        public int CurrentPage { get; set; }
        public int TotalPages { get; set; }
        public int TotalItems { get; set; }   // after the price filter
    }
}