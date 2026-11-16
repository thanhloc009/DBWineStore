using System;
using System.Collections.Generic;
using System.Linq;
using System.Web.Mvc;
using DBWineStore.Models;

namespace DBWineStore.Controllers
{
    public class StoreCategoryController : Controller
    {
        private DBWineStoreEntities db = new DBWineStoreEntities();

        // Trang chủ: danh mục + tìm kiếm + lọc giá + sắp xếp + phân trang
        // "q" là ô tìm kiếm trên header (layout), "keyword" là ô tìm kiếm trong thanh lọc của trang.
        public ActionResult Index(
            int? categoryId,
            string keyword,
            string q,
            decimal? minPrice,
            decimal? maxPrice,
            string sortOrder,
            int page = 1)
        {
            const int pageSize = 8;   // 4 cột x 2 hàng, không bị lẻ hàng

            if (page < 1) page = 1;

            // Ô tìm kiếm ở header gửi "q" -> dùng chung với "keyword"
            if (string.IsNullOrWhiteSpace(keyword)) keyword = q;
            keyword = string.IsNullOrWhiteSpace(keyword) ? null : keyword.Trim();

            // Nếu nhập "giá từ" lớn hơn "giá đến" thì tự đổi chỗ
            if (minPrice.HasValue && maxPrice.HasValue && minPrice.Value > maxPrice.Value)
            {
                decimal tmp = minPrice.Value;
                minPrice = maxPrice;
                maxPrice = tmp;
            }

            var products = db.Products.Where(p => p.IsActive);

            if (categoryId.HasValue)
            {
                products = products.Where(p => p.CategoryID == categoryId.Value);
            }

            if (keyword != null)
            {
                string kw = keyword;
                products = products.Where(
                    p => p.ProductName.Contains(kw)
                      || (p.Description != null && p.Description.Contains(kw)));
            }

            if (minPrice.HasValue)
            {
                products = products.Where(p => p.Price >= minPrice.Value);
            }

            if (maxPrice.HasValue)
            {
                products = products.Where(p => p.Price <= maxPrice.Value);
            }

            int totalItems = products.Count();
            int totalPages = (int)Math.Ceiling((double)totalItems / pageSize);
            if (totalPages == 0) totalPages = 1;
            if (page > totalPages) page = totalPages;

            IOrderedQueryable<Product> ordered;
            if (sortOrder == "price_asc")
            {
                ordered = products.OrderBy(p => p.Price).ThenByDescending(p => p.ProductID);
            }
            else if (sortOrder == "price_desc")
            {
                ordered = products.OrderByDescending(p => p.Price).ThenByDescending(p => p.ProductID);
            }
            else
            {
                sortOrder = null;
                ordered = products.OrderByDescending(p => p.ProductID);
            }

            var model = new StoreHomeViewModel
            {
                Categories = db.Categories.OrderBy(c => c.CategoryName).ToList(),

                Products = ordered
                    .Skip((page - 1) * pageSize)
                    .Take(pageSize)
                    .Select(p => new ProductCardViewModel
                    {
                        ProductID = p.ProductID,
                        NamePro = p.ProductName,
                        CategoryName = p.Category.CategoryName,
                        Description = p.Description,
                        ShortDescription = p.Description,
                        Price = p.Price,
                        ImagePro = p.ImageUrl,
                        StockQuantity = p.StockQuantity
                    })
                    .ToList(),

                SelectedCategoryId = categoryId,
                SearchTerm = keyword,
                MinPrice = minPrice,
                MaxPrice = maxPrice,
                SortOrder = sortOrder,
                CurrentPage = page,
                TotalPages = totalPages,
                TotalItems = totalItems
            };

            // Số sản phẩm đang bán của từng danh mục (hiện bên cạnh tên danh mục)
            var counts = db.Products
                .Where(p => p.IsActive)
                .GroupBy(p => p.CategoryID)
                .Select(g => new { CategoryID = g.Key, Count = g.Count() })
                .ToDictionary(x => x.CategoryID, x => x.Count);

            ViewBag.CategoryCounts = counts;
            ViewBag.AllCount = counts.Values.Sum();

            return View(model);
        }

        // Trang tổng hợp tất cả danh mục (mỗi danh mục là một thẻ): /Store/Categories
        public ActionResult Categories()
        {
            var active = db.Products.Where(p => p.IsActive);

            var stats = active
                .GroupBy(p => p.CategoryID)
                .Select(g => new
                {
                    CategoryID = g.Key,
                    Count = g.Count(),
                    Min = g.Min(p => p.Price),
                    Max = g.Max(p => p.Price)
                })
                .ToList();

            var cards = new System.Collections.Generic.List<CategoryCardViewModel>();

            foreach (var c in db.Categories.OrderBy(x => x.CategoryID).ToList())
            {
                var s = stats.FirstOrDefault(x => x.CategoryID == c.CategoryID);

                // 3 ảnh sản phẩm mới nhất của danh mục để làm ảnh xem trước trên thẻ
                var images = active
                    .Where(p => p.CategoryID == c.CategoryID && p.ImageUrl != null && p.ImageUrl != "")
                    .OrderByDescending(p => p.ProductID)
                    .Select(p => p.ImageUrl)
                    .Take(3)
                    .ToList();

                cards.Add(new CategoryCardViewModel
                {
                    CategoryID = c.CategoryID,
                    CategoryName = c.CategoryName,
                    ProductCount = s != null ? s.Count : 0,
                    LowestPrice = s != null ? s.Min : (decimal?)null,
                    HighestPrice = s != null ? s.Max : (decimal?)null,
                    PreviewImages = images
                });
            }

            return View(new CategoriesOverviewViewModel
            {
                Categories = cards,
                TotalProducts = cards.Sum(x => x.ProductCount)
            });
        }

        // Trang "View all" của một danh mục: /Store/Category/{id}
        public ActionResult Category(
            int? id,
            decimal? minPrice,
            decimal? maxPrice,
            string sortOrder,
            int page = 1)
        {
            if (id == null) return RedirectToAction("Index");

            var category = db.Categories.Find(id.Value);
            if (category == null) return HttpNotFound();

            const int pageSize = 12;   // 4 cột x 3 hàng
            if (page < 1) page = 1;

            // Toàn bộ sản phẩm đang bán của danh mục (dùng cho số liệu trên banner)
            var all = db.Products.Where(p => p.IsActive && p.CategoryID == category.CategoryID);
            int inCategory = all.Count();
            decimal? lowest = inCategory > 0 ? all.Min(p => p.Price) : (decimal?)null;
            decimal? highest = inCategory > 0 ? all.Max(p => p.Price) : (decimal?)null;

            if (minPrice.HasValue && maxPrice.HasValue && minPrice.Value > maxPrice.Value)
            {
                decimal tmp = minPrice.Value;
                minPrice = maxPrice;
                maxPrice = tmp;
            }

            var products = all;
            if (minPrice.HasValue) products = products.Where(p => p.Price >= minPrice.Value);
            if (maxPrice.HasValue) products = products.Where(p => p.Price <= maxPrice.Value);

            int totalItems = products.Count();
            int totalPages = (int)Math.Ceiling((double)totalItems / pageSize);
            if (totalPages == 0) totalPages = 1;
            if (page > totalPages) page = totalPages;

            IOrderedQueryable<Product> ordered;
            if (sortOrder == "price_asc")
            {
                ordered = products.OrderBy(p => p.Price).ThenByDescending(p => p.ProductID);
            }
            else if (sortOrder == "price_desc")
            {
                ordered = products.OrderByDescending(p => p.Price).ThenByDescending(p => p.ProductID);
            }
            else
            {
                sortOrder = null;
                ordered = products.OrderByDescending(p => p.ProductID);
            }

            var counts = db.Products
                .Where(p => p.IsActive)
                .GroupBy(p => p.CategoryID)
                .Select(g => new { CategoryID = g.Key, Count = g.Count() })
                .ToDictionary(x => x.CategoryID, x => x.Count);

            var model = new CategoryPageViewModel
            {
                CategoryID = category.CategoryID,
                CategoryName = category.CategoryName,
                ProductsInCategory = inCategory,
                LowestPrice = lowest,
                HighestPrice = highest,

                Products = ordered
                    .Skip((page - 1) * pageSize)
                    .Take(pageSize)
                    .Select(p => new ProductCardViewModel
                    {
                        ProductID = p.ProductID,
                        NamePro = p.ProductName,
                        CategoryName = p.Category.CategoryName,
                        Description = p.Description,
                        ShortDescription = p.Description,
                        Price = p.Price,
                        ImagePro = p.ImageUrl,
                        StockQuantity = p.StockQuantity
                    })
                    .ToList(),

                Categories = db.Categories
                    .OrderBy(c => c.CategoryName)
                    .ToList()
                    .Select(c => new CategoryLinkViewModel
                    {
                        CategoryID = c.CategoryID,
                        CategoryName = c.CategoryName,
                        ProductCount = counts.ContainsKey(c.CategoryID) ? counts[c.CategoryID] : 0
                    })
                    .ToList(),

                SortOrder = sortOrder,
                MinPrice = minPrice,
                MaxPrice = maxPrice,
                CurrentPage = page,
                TotalPages = totalPages,
                TotalItems = totalItems
            };

            return View(model);
        }

        public ActionResult AboutStore()
        {
            return View();
        }

        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                db.Dispose();
            }
            base.Dispose(disposing);
        }
    }
}