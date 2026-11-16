using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace DBWineStore.Models
{
    // One row in the admin category table.
    public class CategoryRowViewModel
    {
        public int CategoryID { get; set; }
        public string CategoryName { get; set; }
        public int ProductCount { get; set; }
    }

    // Admin list page.
    public class CategoryListViewModel
    {
        public List<CategoryRowViewModel> Categories { get; set; }
        public string Keyword { get; set; }
        public int TotalCategories { get; set; }
        public int TotalProducts { get; set; }
    }

    // Create / Edit form.
    public class CategoryFormViewModel
    {
        public int CategoryID { get; set; }

        [Required(ErrorMessage = "Vui lòng nhập tên danh mục")]
        [StringLength(100, ErrorMessage = "Tên danh mục tối đa 100 ký tự")]
        [Display(Name = "Tên danh mục")]
        public string CategoryName { get; set; }

        public int ProductCount { get; set; }   // only shown on the Edit page
    }
}