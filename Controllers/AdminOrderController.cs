using DBWineStore.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Web;
using System.Web.Mvc;

namespace DBWineStore.Controllers
{
    public class AdminOrderController : Controller
    {

        // TODO: change to the name of your EF context class.
        private readonly DBWineStoreEntities db = new DBWineStoreEntities();

        // ---------- Admin guard ----------
        // TODO: adjust to however your login stores the role (e.g. Session["Role"] = "Admin").
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

        // ---------- List of all orders ----------
        public ActionResult Index(string q, int? payment, int? shipping)
        {
            var query = db.OrderProes.AsQueryable();

            if (!String.IsNullOrWhiteSpace(q))
            {
                string kw = q.Trim();
                int orderId;
                if (int.TryParse(kw.TrimStart('#'), out orderId))
                {
                    query = query.Where(o => o.OrderID == orderId
                        || o.Customer.FullName.Contains(kw)
                        || o.RecipientName.Contains(kw));
                }
                else
                {
                    query = query.Where(o => o.Customer.FullName.Contains(kw)
                        || o.RecipientName.Contains(kw));
                }
            }

            if (payment.HasValue)
                query = query.Where(o => o.PaymentStatusID == payment.Value);
            if (shipping.HasValue)
                query = query.Where(o => o.ShippingStatusID == shipping.Value);

            var rows = query
                .OrderByDescending(o => o.OrderDate)
                .Select(o => new AdminOrderRowViewModel
                {
                    OrderID = o.OrderID,
                    OrderDate = o.OrderDate,
                    CustomerName = o.Customer.FullName,
                    RecipientName = o.RecipientName,
                    Total = o.TotalAmount,
                    PaymentMethodName = o.PaymentMethod.MethodName,
                    PaymentStatusID = o.PaymentStatusID,
                    ShippingStatusID = o.ShippingStatusID
                })
                .ToList();

            var model = new AdminOrderListViewModel
            {
                Orders = rows,
                PaymentStatuses = GetPaymentStatuses(),
                ShippingStatuses = GetShippingStatuses(),
                Keyword = q,
                PaymentFilter = payment,
                ShippingFilter = shipping,
                TotalOrders = db.OrderProes.Count(),
                WaitingOrders = db.OrderProes.Count(o => o.ShippingStatu.Code == "PENDING"),
                UnpaidOrders = db.OrderProes.Count(o => o.PaymentStatu.Code == "PENDING")
            };

            return View(model);
        }

        // ---------- One order in full ----------
        public ActionResult Details(int? id)
        {
            if (id == null) return new HttpStatusCodeResult(HttpStatusCode.BadRequest);

            var model = db.OrderProes
                .Where(o => o.OrderID == id.Value)
                .Select(o => new AdminOrderDetailsViewModel
                {
                    OrderID = o.OrderID,
                    OrderDate = o.OrderDate,
                    CustomerName = o.Customer.FullName,
                    RecipientName = o.RecipientName,
                    ShippingPhone = o.ShippingPhone,
                    ShippingAddress = o.ShippingAddress,
                    TotalAmount = o.TotalAmount,
                    PaymentMethodName = o.PaymentMethod.MethodName,
                    PaymentStatusID = o.PaymentStatusID,
                    ShippingStatusID = o.ShippingStatusID
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

            model.PaymentStatuses = GetPaymentStatuses();
            model.ShippingStatuses = GetShippingStatuses();

            return View(model);
        }

        // ---------- Change payment / shipping status ----------
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult UpdateStatus(int orderId, int paymentStatusId, int shippingStatusId, string returnUrl)
        {
            var order = db.OrderProes.Find(orderId);
            if (order == null) return HttpNotFound();

            var pay = db.PaymentStatus.Find(paymentStatusId);
            var ship = db.ShippingStatus.Find(shippingStatusId);
            if (pay == null || ship == null)
            {
                TempData["Error"] = "Invalid status selected.";
                return Back(returnUrl);
            }

            order.PaymentStatusID = pay.PaymentStatusID;
            order.ShippingStatusID = ship.ShippingStatusID;
            order.Status = ToLegacyStatus(ship.Code);   // keep the old Status column in sync
            db.SaveChanges();

            TempData["Message"] = "Order #" + orderId + " updated: " + pay.StatusName + " / " + ship.StatusName + ".";
            return Back(returnUrl);
        }

        // ---------- helpers ----------
        private ActionResult Back(string returnUrl)
        {
            if (!String.IsNullOrEmpty(returnUrl) && Url.IsLocalUrl(returnUrl))
                return Redirect(returnUrl);
            return RedirectToAction("Index");
        }

        // Old OrderPro.Status text, derived from the shipping status.
        private static string ToLegacyStatus(string shippingCode)
        {
            switch (shippingCode)
            {
                case "DELIVERED": return "Completed";
                case "FAILED":
                case "RETURNED": return "Cancelled";
                case "SHIPPING": return "Shipping";
                default: return "Pending";
            }
        }

        private System.Collections.Generic.List<StatusOption> GetPaymentStatuses()
        {
            return db.PaymentStatus
                .OrderBy(s => s.PaymentStatusID)
                .Select(s => new StatusOption { ID = s.PaymentStatusID, Name = s.StatusName })
                .ToList();
        }

        private System.Collections.Generic.List<StatusOption> GetShippingStatuses()
        {
            return db.ShippingStatus
                .OrderBy(s => s.ShippingStatusID)
                .Select(s => new StatusOption { ID = s.ShippingStatusID, Name = s.StatusName })
                .ToList();
        }

        protected override void Dispose(bool disposing)
        {
            if (disposing) db.Dispose();
            base.Dispose(disposing);
        }
    }
}
