using System.Linq;
using System.Web.Mvc;
using DBWineStore.Models;

namespace DBWineStore.Controllers
{
    public class MainPageController : Controller
    {
        private DBWineStoreEntities db = new DBWineStoreEntities();

        public ActionResult Home()
        {
            ViewBag.Categories = db.Categories
                                    .OrderBy(c => c.CategoryName)
                                    .ToList();

            ViewBag.Products = db.Products
                                  .Include("Category")
                                  .Where(p => p.IsActive)
                                  .OrderBy(p => p.ProductName)
                                  .ToList();

            return View();
        }

        protected override void Dispose(bool disposing)
        {
            if (disposing) db.Dispose();
            base.Dispose(disposing);
        }
    }
}