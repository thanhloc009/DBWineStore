using System;
using System.Data.Entity;
using System.Linq;
using System.Net;
using System.Web;
using System.Web.Mvc;
using DBWineStore.Models;
//using DBWineStore.Filters;

namespace DBWineStore.Controllers
{
    //[AdminAuthorize]
    public class ProductsController : Controller
    {
        private DBWineStoreEntities db = new DBWineStoreEntities();

        private static readonly string[] AllowedExtensions = { ".jpg", ".jpeg", ".png", ".webp" };
        private const int MaxImageBytes = 5 * 1024 * 1024;

        // ---------- Helpers ----------

        private void ValidateImage(HttpPostedFileBase imageFile)
        {
            if (imageFile == null || imageFile.ContentLength <= 0) return;

            string extension = System.IO.Path.GetExtension(imageFile.FileName).ToLower();

            if (!AllowedExtensions.Contains(extension))
            {
                ModelState.AddModelError("", "Invalid image format. Use JPG, JPEG, PNG or WEBP.");
            }
            else if (imageFile.ContentLength > MaxImageBytes)
            {
                ModelState.AddModelError("", "Image must be smaller than 5 MB.");
            }
        }

        // Lưu vào ~/Content/images, giữ tên gốc (trùng thì -1, -2...)
        // DB chỉ lưu tên file → Index: ~/Content/images/{ImageUrl}
        private string SaveImage(HttpPostedFileBase imageFile)
        {
            string originalFileName = System.IO.Path.GetFileName(imageFile.FileName);
            string fileNameWithoutExtension = System.IO.Path.GetFileNameWithoutExtension(originalFileName);
            string extension = System.IO.Path.GetExtension(originalFileName).ToLower();

            string folder = Server.MapPath("~/Content/images");

            if (!System.IO.Directory.Exists(folder))
            {
                System.IO.Directory.CreateDirectory(folder);
            }

            string fileName = fileNameWithoutExtension + extension;
            string path = System.IO.Path.Combine(folder, fileName);

            int count = 1;
            while (System.IO.File.Exists(path))
            {
                fileName = fileNameWithoutExtension + "-" + count + extension;
                path = System.IO.Path.Combine(folder, fileName);
                count++;
            }

            imageFile.SaveAs(path);
            return fileName;
        }

        private void PopulateCategories(object selected = null)
        {
            ViewBag.CategoryID = new SelectList(db.Categories, "CategoryID", "CategoryName", selected);
        }

        // ---------- Search ----------

        [HttpGet]
        public ActionResult Search(string q)
        {
            var products = db.Products.Where(p => p.IsActive);

            if (!string.IsNullOrWhiteSpace(q))
            {
                products = products.Where(p =>
                    p.ProductName.Contains(q) ||
                    p.Description.Contains(q));
            }
            else
            {
                products = products.Where(p => false);
            }

            var results = products.ToList();

            var model = new SearchViewModel
            {
                Query = q,
                TotalCount = results.Count,
                Results = results
            };

            return View(model);
        }

        // GET: Products
        public ActionResult Index()
        {
            var products = db.Products.Include(p => p.Category);
            return View(products.ToList());
        }

        // GET: Products/Details/5
        public ActionResult Details(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }

            Product product = db.Products.Find(id);
            if (product == null)
            {
                return HttpNotFound();
            }

            return View(product);
        }

        // GET: Products/Create
        public ActionResult Create()
        {
            PopulateCategories();
            return View(new Product { IsActive = true });
        }

        // POST: Products/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Create(
            [Bind(Include = "ProductName,Description,CategoryID,Price,StockQuantity,IsActive")]
            Product product,
            HttpPostedFileBase imageFile)
        {
            ValidateImage(imageFile);

            if (ModelState.IsValid)
            {
                if (imageFile != null && imageFile.ContentLength > 0)
                {
                    product.ImageUrl = SaveImage(imageFile);
                }

                db.Products.Add(product);
                db.SaveChanges();

                TempData["Success"] = "Wine created successfully.";
                return RedirectToAction("Index");
            }

            PopulateCategories(product.CategoryID);
            return View(product);
        }

        // GET: Products/Edit/5
        public ActionResult Edit(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }

            Product product = db.Products.Find(id);
            if (product == null)
            {
                return HttpNotFound();
            }

            PopulateCategories(product.CategoryID);
            return View(product);
        }

        // POST: Products/Edit/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Edit(Product product, HttpPostedFileBase imageFile)
        {
            ValidateImage(imageFile);

            if (ModelState.IsValid)
            {
                bool newImage = imageFile != null && imageFile.ContentLength > 0;

                if (newImage)
                {
                    product.ImageUrl = SaveImage(imageFile);
                }

                db.Entry(product).State = EntityState.Modified;

                // Không upload ảnh mới → giữ ImageUrl cũ
                if (!newImage)
                {
                    db.Entry(product).Property(p => p.ImageUrl).IsModified = false;
                }

                db.SaveChanges();

                TempData["Success"] = "Wine updated successfully.";
                return RedirectToAction("Index");
            }

            PopulateCategories(product.CategoryID);
            return View(product);
        }

        // GET: Products/Delete/5
        public ActionResult Delete(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }

            Product product = db.Products.Find(id);
            if (product == null)
            {
                return HttpNotFound();
            }

            return View(product);
        }

        // POST: Products/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public ActionResult DeleteConfirmed(int id)
        {
            Product product = db.Products.Find(id);
            if (product == null)
            {
                return HttpNotFound();
            }

            db.Products.Remove(product);
            db.SaveChanges();
            return RedirectToAction("Index");
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