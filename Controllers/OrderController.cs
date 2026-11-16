using DBWineStore.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Web;
using System.Web.Mvc;

namespace DBWineStore.Controllers
{
    public class OrderController : Controller
    {
        // GET: Order
        public ActionResult Index()
        {
            return View();
        }

        // TODO: change to the name of your EF context class.
        public DBWineStoreEntities db = new DBWineStoreEntities();

        // Works for logged-in users (Session["UserID"]) and for guests who just checked out
        // (Session["CustomerID"]). Returns null when nobody is identified.
        private int? GetCurrentCustomerId()
        {
            int? customerId = Session["CustomerID"] as int?;
            if (customerId.HasValue) return customerId;

            int? userId = Session["UserID"] as int?;
            if (userId.HasValue)
            {
                return db.Customers
                    .Where(c => c.UserID == userId.Value)
                    .Select(c => (int?)c.CustomerID)
                    .FirstOrDefault();
            }
            return null;
        }

        // Order history: every order made by the current customer, newest first.
        public ActionResult History()
        {
            int? customerId = GetCurrentCustomerId();
            if (!customerId.HasValue)
            {
                ViewBag.NeedLogin = true;
                return View(new List<OrderSummaryViewModel>());
            }

            var orders = db.OrderProes
                .Where(o => o.CustomerID == customerId.Value)
                .OrderByDescending(o => o.OrderDate)
                .Select(o => new OrderSummaryViewModel
                {
                    OrderID = o.OrderID,
                    OrderDate = o.OrderDate,
                    ItemCount = o.OrderDetails.Sum(d => (int?)d.Quantity) ?? 0,
                    Total = o.TotalAmount,
                    PaymentStatusName = o.PaymentStatu.StatusName,
                    PaymentStatusCode = o.PaymentStatu.Code,
                    ShippingStatusName = o.ShippingStatu.StatusName,
                    ShippingStatusCode = o.ShippingStatu.Code
                })
                .ToList();

            return View(orders);
        }

        // Order details: all items bought in one order + payment / shipping status set by the admin.
        public ActionResult Details(int? id)
        {
            if (id == null) return new HttpStatusCodeResult(HttpStatusCode.BadRequest);

            int? customerId = GetCurrentCustomerId();
            if (!customerId.HasValue) return RedirectToAction("History");

            // Customers can only open their own orders.
            var model = db.OrderProes
                .Where(o => o.OrderID == id.Value && o.CustomerID == customerId.Value)
                .Select(o => new OrderDetailsViewModel
                {
                    OrderID = o.OrderID,
                    OrderDate = o.OrderDate,
                    RecipientName = o.RecipientName,
                    ShippingPhone = o.ShippingPhone,
                    ShippingAddress = o.ShippingAddress,
                    TotalAmount = o.TotalAmount,
                    PaymentMethodName = o.PaymentMethod.MethodName,
                    PaymentStatusName = o.PaymentStatu.StatusName,
                    PaymentStatusCode = o.PaymentStatu.Code,
                    ShippingStatusName = o.ShippingStatu.StatusName,
                    ShippingStatusCode = o.ShippingStatu.Code
                })
                .FirstOrDefault();

            if (model == null) return HttpNotFound();

            model.Items = db.OrderDetails
                .Where(d => d.OrderID == id.Value)
                .Select(d => new OrderItemViewModel
                {
                    ProductID = d.ProductID,
                    ProductName = d.Product.ProductName,
                    ImageUrl = d.Product.ImageUrl,
                    UnitPrice = d.UnitPrice,
                    Quantity = d.Quantity
                })
                .ToList();

            return View(model);
        }

        protected override void Dispose(bool disposing)
        {
            if (disposing) db.Dispose();
            base.Dispose(disposing);
        }
    }
}




