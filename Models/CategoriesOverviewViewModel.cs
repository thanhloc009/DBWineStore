using System.Collections.Generic;

namespace DBWineStore.Models
{
    // One card on the "all categories" page.
    public class CategoryCardViewModel
    {
        public int CategoryID { get; set; }
        public string CategoryName { get; set; }
        public int ProductCount { get; set; }
        public decimal? LowestPrice { get; set; }
        public decimal? HighestPrice { get; set; }
        public List<string> PreviewImages { get; set; }   // up to 3 product image file names
    }

    public class CategoriesOverviewViewModel
    {
        public List<CategoryCardViewModel> Categories { get; set; }
        public int TotalProducts { get; set; }
    }
}