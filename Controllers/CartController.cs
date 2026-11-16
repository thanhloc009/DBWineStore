using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;
using System.Web.Mvc;
using DBWineStore.Models;

namespace DBWineStore.Controllers
{
    public class CartController : Controller
    {
        private DBWineStoreEntities db = new DBWineStoreEntities();

        // Lấy giỏ hàng từ Session
        private List<CartItemViewModel> GetCart()
        {
            var cart = Session["Cart"] as List<CartItemViewModel>;

            if (cart == null)
            {
                cart = new List<CartItemViewModel>();
                Session["Cart"] = cart;
            }

            return cart;
        }

        // Nạp danh sách phương thức thanh toán từ bảng PaymentMethod
        private void FillPaymentMethods(CheckoutViewModel model)
        {
            model.PaymentMethods = db.PaymentMethods
                .OrderBy(m => m.PaymentMethodID)
                .Select(m => new PaymentMethodOption { Code = m.Code, Name = m.MethodName })
                .ToList();
        }

        // Hiển thị giỏ hàng
        public ActionResult Index()
        {
            var cart = GetCart();

            decimal total = cart.Sum(x => x.Total);

            // Tính phí giao hàng
            decimal shippingFee = total > 1500000 ? 0 : 49000;

            // Tính giảm giá KM10
            decimal discount = Session["KM10"] != null
                ? total * 0.1m
                : 0;

            // Tổng thanh toán
            decimal finalTotal = total - discount + shippingFee;

            ViewBag.Total = total;
            ViewBag.ShippingFee = shippingFee;
            ViewBag.Discount = discount;
            ViewBag.FinalTotal = finalTotal;
            ViewBag.IsKM10 = Session["KM10"] != null;

            return View(cart);
        }

        // Áp dụng mã giảm giá
        public ActionResult ApplyCoupon()
        {
            var cart = GetCart();

            if (cart.Count == 0)
            {
                TempData["CartMessage"] = "Giỏ hàng đang trống!";
                return RedirectToAction("Index");
            }

            Session["KM10"] = true;

            TempData["CouponMessage"] = "Áp dụng mã KM10 thành công! Bạn được giảm 10%.";

            return RedirectToAction("Index");
        }

        // Hủy mã giảm giá
        public ActionResult RemoveCoupon()
        {
            Session.Remove("KM10");

            TempData["CouponMessage"] = "Đã hủy mã giảm giá.";

            return RedirectToAction("Index");
        }

        // Thêm sản phẩm vào giỏ hàng (có kiểm tra tồn kho)
        public ActionResult AddToCart(int id)
        {
            var product = db.Products.Find(id);

            if (product == null || !product.IsActive)
            {
                TempData["CartMessage"] = "Sản phẩm không tồn tại hoặc đã ngừng bán!";
                return RedirectToAction("Index", "Store");
            }

            if (product.StockQuantity <= 0)
            {
                TempData["CartMessage"] = "Sản phẩm đã hết hàng!";
                return RedirectToAction("Index", "Store");
            }

            var cart = GetCart();
            var item = cart.FirstOrDefault(x => x.ProductID == id);

            if (item == null)
            {
                cart.Add(new CartItemViewModel
                {
                    ProductID = product.ProductID,
                    ProductName = product.ProductName,
                    ImageUrl = product.ImageUrl,
                    Price = product.Price,
                    Quantity = 1,
                    StockQuantity = product.StockQuantity
                });
            }
            else
            {
                if (item.Quantity >= product.StockQuantity)
                {
                    TempData["CartMessage"] = "Số lượng đã đạt giới hạn tồn kho!";
                    return RedirectToAction("Index");
                }
                item.Quantity++;
                item.StockQuantity = product.StockQuantity;
            }

            Session["Cart"] = cart;
            TempData["CartMessage"] = "Đã thêm sản phẩm vào giỏ hàng!";
            return RedirectToAction("Index");
        }

        // Cập nhật số lượng
        public ActionResult UpdateQuantity(int id, int quantity)
        {
            var cart = GetCart();
            var item = cart.FirstOrDefault(x => x.ProductID == id);

            if (item == null)
            {
                return RedirectToAction("Index");
            }

            var product = db.Products.Find(id);
            if (product == null)
            {
                cart.Remove(item);
                Session["Cart"] = cart;
                return RedirectToAction("Index");
            }

            if (quantity <= 0)
            {
                cart.Remove(item);
            }
            else if (quantity > product.StockQuantity)
            {
                TempData["CartMessage"] = "Số lượng không được vượt tồn kho (" + product.StockQuantity + ")!";
                item.Quantity = product.StockQuantity;
                item.StockQuantity = product.StockQuantity;
            }
            else
            {
                item.Quantity = quantity;
                item.StockQuantity = product.StockQuantity;
            }

            Session["Cart"] = cart;
            return RedirectToAction("Index");
        }

        // Xóa sản phẩm khỏi giỏ
        public ActionResult Remove(int id)
        {
            var cart = GetCart();
            var item = cart.FirstOrDefault(x => x.ProductID == id);
            if (item != null)
            {
                cart.Remove(item);
                Session["Cart"] = cart;
            }
            return RedirectToAction("Index");
        }

        // Xóa toàn bộ giỏ
        public ActionResult Clear()
        {
            Session.Remove("Cart");
            Session.Remove("KM10");
            return RedirectToAction("Index");
        }

        // ==================== CHECKOUT ====================
        [HttpGet]
        public ActionResult Checkout()
        {
            var cart = GetCart();
            if (cart.Count == 0)
            {
                TempData["CartMessage"] = "Giỏ hàng đang trống!";
                return RedirectToAction("Index");
            }

            decimal totalAmount = cart.Sum(x => x.Total);
            decimal shippingFee = totalAmount > 1500000 ? 0 : 49000;
            decimal discountAmount = Session["KM10"] != null ? totalAmount * 0.1m : 0;
            decimal finalAmount = totalAmount - discountAmount + shippingFee;

            var model = new CheckoutViewModel
            {
                TotalAmount = totalAmount,
                DiscountAmount = discountAmount,
                ShippingFee = shippingFee,
                FinalAmount = finalAmount,
                PaymentMethod = "COD"          // chọn sẵn COD
            };

            // Danh sách phương thức thanh toán lấy từ database
            FillPaymentMethods(model);

            // Nếu đã đăng nhập thì điền sẵn thông tin
            if (Session["CustomerID"] != null)
            {
                int customerId = Convert.ToInt32(Session["CustomerID"]);
                var customer = db.Customers.Find(customerId);
                if (customer != null)
                {
                    model.FullName = customer.FullName;
                    model.Phone = customer.Phone;
                    model.Email = customer.Email;
                    model.Address = customer.Address;
                }
            }

            return View("~/Views/Checkout/Index.cshtml", model);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Checkout(CheckoutViewModel model)
        {
            var cart = GetCart();

            if (cart.Count == 0)
            {
                TempData["CartMessage"] = "Giỏ hàng đang trống!";
                return RedirectToAction("Index");
            }

            decimal totalAmount = cart.Sum(x => x.Total);
            decimal shippingFee = totalAmount > 1500000 ? 0 : 49000;
            decimal discountAmount = Session["KM10"] != null ? totalAmount * 0.1m : 0;
            decimal finalAmount = totalAmount - discountAmount + shippingFee;

            model.TotalAmount = totalAmount;
            model.DiscountAmount = discountAmount;
            model.ShippingFee = shippingFee;
            model.FinalAmount = finalAmount;

            // Danh sách phương thức thanh toán không được post lại, nạp lại cho mọi lần trả View
            FillPaymentMethods(model);

            // Giá trị post lên phải là một PaymentMethod.Code có thật (COD / BANK)
            var paymentMethod = db.PaymentMethods.FirstOrDefault(m => m.Code == model.PaymentMethod);
            if (paymentMethod == null)
            {
                ModelState.AddModelError("PaymentMethod", "Phương thức thanh toán không hợp lệ.");
            }

            if (!ModelState.IsValid)
            {
                return View("~/Views/Checkout/Index.cshtml", model);
            }

            // Kiểm tra tồn kho lần cuối trước khi đặt
            foreach (var item in cart)
            {
                var product = db.Products.Find(item.ProductID);
                if (product == null || !product.IsActive || product.StockQuantity < item.Quantity)
                {
                    ModelState.AddModelError("", "Sản phẩm \"" + item.ProductName + "\" không đủ tồn kho hoặc đã ngừng bán.");
                    return View("~/Views/Checkout/Index.cshtml", model);
                }
            }

            using (var transaction = db.Database.BeginTransaction())
            {
                try
                {
                    Customer customer = null;

                    // Đã đăng nhập
                    if (Session["CustomerID"] != null)
                    {
                        int customerId = Convert.ToInt32(Session["CustomerID"]);
                        customer = db.Customers.Find(customerId);
                        if (customer != null)
                        {
                            customer.Phone = model.Phone;
                            customer.Address = model.Address;
                            if (!string.IsNullOrWhiteSpace(model.Email))
                                customer.Email = model.Email;
                            db.SaveChanges();
                        }
                    }

                    // Guest checkout
                    if (customer == null)
                    {
                        customer = new Customer
                        {
                            UserID = null,
                            FullName = model.FullName.Trim(),
                            Phone = model.Phone,
                            Email = string.IsNullOrWhiteSpace(model.Email) ? null : model.Email.Trim(),
                            Address = model.Address
                        };
                        db.Customers.Add(customer);
                        db.SaveChanges();
                    }

                    // Trạng thái ban đầu của mọi đơn hàng mới
                    var pendingPayment = db.PaymentStatus.First(s => s.Code == "PENDING");
                    var pendingShipping = db.ShippingStatus.First(s => s.Code == "PENDING");

                    var order = new OrderPro
                    {
                        OrderDate = DateTime.Now,
                        CustomerID = customer.CustomerID,
                        ShippingAddress = model.Address,
                        TotalAmount = finalAmount,
                        Status = model.PaymentMethod == "COD"
                            ? "Chờ xác nhận - COD"
                            : "Chờ chuyển khoản",
                        RecipientName = model.FullName,
                        ShippingPhone = model.Phone,

                        // Liên kết với bảng PaymentMethod / PaymentStatus / ShippingStatus
                        PaymentMethodID = paymentMethod.PaymentMethodID,
                        PaymentStatusID = pendingPayment.PaymentStatusID,
                        ShippingStatusID = pendingShipping.ShippingStatusID
                    };

                    db.OrderProes.Add(order);
                    db.SaveChanges();

                    foreach (var item in cart)
                    {
                        db.OrderDetails.Add(new OrderDetail
                        {
                            OrderID = order.OrderID,
                            ProductID = item.ProductID,
                            Quantity = item.Quantity,
                            UnitPrice = item.Price
                        });

                        // Trừ tồn kho
                        var product = db.Products.Find(item.ProductID);
                        if (product != null)
                        {
                            product.StockQuantity -= item.Quantity;
                        }
                    }

                    db.SaveChanges();
                    transaction.Commit();

                    Session.Remove("Cart");
                    Session.Remove("KM10");
                    Session["LastOrderId"] = order.OrderID;

                    TempData["OrderSuccess"] = "Đặt hàng thành công! Mã đơn: #" + order.OrderID;
                    return RedirectToAction("Success");
                }
                catch (Exception ex)
                {
                    transaction.Rollback();
                    ModelState.AddModelError("", "Có lỗi khi lưu đơn hàng: " + ex.GetBaseException().Message);
                    return View("~/Views/Checkout/Index.cshtml", model);
                }
            }
        }
        // Tăng số lượng (+1)
        public ActionResult Increase(int id)
        {
            var cart = GetCart();
            var item = cart.FirstOrDefault(x => x.ProductID == id);
            if (item == null)
            {
                return RedirectToAction("Index");
            }

            var product = db.Products.Find(id);
            if (product == null || !product.IsActive)
            {
                cart.Remove(item);
                Session["Cart"] = cart;
                TempData["CartMessage"] = "Sản phẩm không còn tồn tại hoặc đã ngừng bán.";
                return RedirectToAction("Index");
            }

            if (item.Quantity >= product.StockQuantity)
            {
                TempData["CartMessage"] = "Số lượng đã đạt giới hạn tồn kho (" + product.StockQuantity + ")!";
                item.StockQuantity = product.StockQuantity;
                Session["Cart"] = cart;
                return RedirectToAction("Index");
            }

            item.Quantity++;
            item.StockQuantity = product.StockQuantity;
            Session["Cart"] = cart;
            return RedirectToAction("Index");
        }

        // Giảm số lượng (-1). Về 1 rồi bấm - sẽ xóa khỏi giỏ
        public ActionResult Decrease(int id)
        {
            var cart = GetCart();
            var item = cart.FirstOrDefault(x => x.ProductID == id);
            if (item == null)
            {
                return RedirectToAction("Index");
            }

            if (item.Quantity <= 1)
            {
                cart.Remove(item);
            }
            else
            {
                item.Quantity--;
                var product = db.Products.Find(id);
                if (product != null)
                {
                    item.StockQuantity = product.StockQuantity;
                }
            }

            Session["Cart"] = cart;
            return RedirectToAction("Index");
        }

        public ActionResult Success()
        {
            if (TempData["OrderSuccess"] == null)
            {
                return RedirectToAction("Index", "Store");
            }

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