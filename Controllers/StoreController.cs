using DBWineStore.Models;
using System.Linq;
using System.Web.Mvc;

namespace DBWineStore.Controllers
{
    public class StoreController : Controller
    {
        private DBWineStoreEntities db = new DBWineStoreEntities();
        private ProductCardViewModel viewModel;

        public ActionResult Index()
        {
            var products = db.Products
                .Where(p => p.IsActive)
                .Select(p => new ProductCardViewModel
                {
                    ProductID = p.ProductID,
                    NamePro = p.ProductName,
                    CategoryName = p.Category.CategoryName,
                    Price = p.Price,
                    ImagePro = p.ImageUrl,
                    ShortDescription = p.Description
                })
                .ToList();

            return View(products);
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