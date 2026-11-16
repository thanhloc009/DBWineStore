using System;
using System.Linq;
using System.Net;
using System.Web.Mvc;
using DBWineStore.Models;

namespace DBWineStore.Controllers
{
    public class CategoriesController : Controller
    {
        private readonly DBWineStoreEntities db = new DBWineStoreEntities();

        // ---------- Admin guard ----------
        // TODO: adjust to however your login stores the role (same check as AdminOrderController).
        //private bool IsAdmin()
        //{
        //    string role = Session["Role"] as string;
        //    return string.Equals(role, "Admin", StringComparison.OrdinalIgnoreCase);
        //}

        //protected override void OnActionExecuting(ActionExecutingContext filterContext)
        //{
        //    if (!IsAdmin())
        //        filterContext.Result = new HttpStatusCodeResult(HttpStatusCode.Forbidden, "Admin only.");
        //    base.OnActionExecuting(filterContext);
        //}

        // ---------- List ----------
        public ActionResult Index(string q)
        {
            var query = db.Categories.AsQueryable();

            if (!String.IsNullOrWhiteSpace(q))
            {
                string kw = q.Trim();
                query = query.Where(c => c.CategoryName.Contains(kw));
            }

            var rows = query
                .OrderBy(c => c.CategoryName)
                .Select(c => new CategoryRowViewModel
                {
                    CategoryID = c.CategoryID,
                    CategoryName = c.CategoryName,
                    ProductCount = c.Products.Count()
                })
                .ToList();

            var model = new CategoryListViewModel
            {
                Categories = rows,
                Keyword = q,
                TotalCategories = db.Categories.Count(),
                TotalProducts = db.Products.Count()
            };

            return View(model);
        }

        // ---------- Create ----------
        public ActionResult Create()
        {
            return View(new CategoryFormViewModel());
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Create(CategoryFormViewModel model)
        {
            string name = (model.CategoryName ?? "").Trim();

            if (ModelState.IsValid && db.Categories.Any(c => c.CategoryName == name))
                ModelState.AddModelError("CategoryName", "Danh mục này đã tồn tại.");

            if (!ModelState.IsValid)
                return View(model);

            db.Categories.Add(new Category { CategoryName = name });
            db.SaveChanges();

            TempData["Message"] = "Đã thêm danh mục \"" + name + "\".";
            return RedirectToAction("Index");
        }

        // ---------- Edit ----------
        public ActionResult Edit(int? id)
        {
            if (id == null) return new HttpStatusCodeResult(HttpStatusCode.BadRequest);

            var category = db.Categories.Find(id);
            if (category == null) return HttpNotFound();

            return View(new CategoryFormViewModel
            {
                CategoryID = category.CategoryID,
                CategoryName = category.CategoryName,
                ProductCount = db.Products.Count(p => p.CategoryID == category.CategoryID)
            });
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Edit(CategoryFormViewModel model)
        {
            var category = db.Categories.Find(model.CategoryID);
            if (category == null) return HttpNotFound();

            model.ProductCount = db.Products.Count(p => p.CategoryID == category.CategoryID);
            string name = (model.CategoryName ?? "").Trim();

            if (ModelState.IsValid &&
                db.Categories.Any(c => c.CategoryName == name && c.CategoryID != model.CategoryID))
                ModelState.AddModelError("CategoryName", "Đã có danh mục khác dùng tên này.");

            if (!ModelState.IsValid)
                return View(model);

            category.CategoryName = name;
            db.SaveChanges();

            TempData["Message"] = "Đã cập nhật danh mục \"" + name + "\".";
            return RedirectToAction("Index");
        }

        // ---------- Delete ----------
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Delete(int id)
        {
            var category = db.Categories.Find(id);
            if (category == null) return HttpNotFound();

            // Product.CategoryID is a foreign key, so a category that still has products can't be removed.
            int productCount = db.Products.Count(p => p.CategoryID == id);
            if (productCount > 0)
            {
                TempData["Error"] = "Không thể xóa \"" + category.CategoryName + "\" vì còn " +
                                    productCount + " sản phẩm thuộc danh mục này.";
                return RedirectToAction("Index");
            }

            db.Categories.Remove(category);
            db.SaveChanges();

            TempData["Message"] = "Đã xóa danh mục \"" + category.CategoryName + "\".";
            return RedirectToAction("Index");
        }

        protected override void Dispose(bool disposing)
        {
            if (disposing) db.Dispose();
            base.Dispose(disposing);
        }
    }
}