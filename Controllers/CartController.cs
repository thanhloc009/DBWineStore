using System;
using System.Collections.Generic;
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
        // Thêm sản phẩm vào giỏ hàng
        public ActionResult AddToCart(int id)
        {
            var product = db.Products.Find(id);

            if (product == null || !product.IsActive)
            {
                TempData["CartMessage"] = "Sản phẩm không tồn tại!";
                return RedirectToAction("Index", "Store");
            }

            var cart = GetCart();

            var item = cart.FirstOrDefault(p => p.ProductID == id);

            if (item != null)
            {
                if (item.Quantity < product.StockQuantity)
                {
                    item.Quantity++;
                }
                else
                {
                    TempData["CartMessage"] = "Số lượng vượt quá tồn kho!";
                }
            }
            else
            {
                if (product.StockQuantity > 0)
                {
                    cart.Add(new CartItemViewModel
                    {
                        ProductID = product.ProductID,
                        ProductName = product.ProductName,
                        ImageUrl = product.ImageUrl,
                        Price = product.Price,
                        Quantity = 1
                    });
                }
                else
                {
                    TempData["CartMessage"] = "Sản phẩm đã hết hàng!";
                }
            }

            return RedirectToAction("Index");
        }

        // Tăng số lượng
        public ActionResult Increase(int id)
        {
            var cart = GetCart();
            var item = cart.FirstOrDefault(p => p.ProductID == id);

            if (item != null)
            {
                var product = db.Products.Find(id);

                if (product != null &&
                    item.Quantity < product.StockQuantity)
                {
                    item.Quantity++;
                }
            }

            return RedirectToAction("Index");
        }

        // Giảm số lượng
        public ActionResult Decrease(int id)
        {
            var cart = GetCart();
            var item = cart.FirstOrDefault(p => p.ProductID == id);

            if (item != null)
            {
                if (item.Quantity > 1)
                {
                    item.Quantity--;
                }
            }

            return RedirectToAction("Index");
        }

        // Xóa sản phẩm
        public ActionResult Remove(int id)
        {
            var cart = GetCart();
            var item = cart.FirstOrDefault(p => p.ProductID == id);

            if (item != null)
            {
                cart.Remove(item);
            }

            return RedirectToAction("Index");
        }
        public ActionResult Checkout()
        {
            var cart = GetCart();

            if (cart.Count == 0)
            {
                return RedirectToAction("Index");
            }

            decimal total = cart.Sum(x => x.Total);
            decimal discount = Session["KM10"] != null ? total * 0.1m : 0;
            decimal shippingFee = total > 1500000 ? 0 : 49000;
            var model = new CheckoutViewModel
            {
                TotalAmount = total,
                DiscountAmount = discount,
                ShippingFee = shippingFee,
                FinalAmount = total - discount + shippingFee
            };

            return View("~/Views/Checkout/Index.cshtml", model);
        }
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Checkout(CheckoutViewModel model)
        {
            var cart = GetCart();

            if (cart.Count == 0)
            {
                return RedirectToAction("Index");
            }

            // Tính tiền từ giỏ hàng
            decimal totalAmount = cart.Sum(x => x.Total);

            // Tính giảm giá KM10
            decimal discountAmount = Session["KM10"] != null
                ? totalAmount * 0.1m
                : 0;

            // Tính phí giao hàng
            decimal shippingFee = totalAmount > 1500000 ? 0 : 49000;

            // Tổng tiền cuối cùng
            decimal finalAmount = totalAmount - discountAmount + shippingFee;

            if (!ModelState.IsValid)
            {
                model.TotalAmount = totalAmount;
                model.DiscountAmount = discountAmount;
                model.ShippingFee = shippingFee;
                model.FinalAmount = finalAmount;

                return View("~/Views/Checkout/Index.cshtml", model);
            }

            if (model.PaymentMethod != "COD" &&
                model.PaymentMethod != "BankTransfer")
            {
                ModelState.AddModelError(
                    "PaymentMethod",
                    "Phương thức thanh toán không hợp lệ."
                );

                model.TotalAmount = totalAmount;
                model.DiscountAmount = discountAmount;
                model.ShippingFee = shippingFee;
                model.FinalAmount = finalAmount;

                return View("~/Views/Checkout/Index.cshtml", model);
            }

            using (var transaction = db.Database.BeginTransaction())
            {
                try
                {
                    var customer = db.Customers
    .FirstOrDefault(c => c.Email == model.Email);

                    if (customer == null)
                    {
                        customer = new Customer
                        {
                            FullName = model.FullName,
                            Phone = model.Phone,
                            Email = model.Email,
                            Address = model.Address
                        };

                        db.Customers.Add(customer);
                        db.SaveChanges();
                    }
                    else
                    {
                        customer.FullName = model.FullName;
                        customer.Phone = model.Phone;
                        customer.Address = model.Address;

                        db.SaveChanges();
                    }

                    var order = new OrderPro
                    {
                        OrderDate = DateTime.Now,
                        CustomerID = customer.CustomerID,
                        ShippingAddress = model.Address,
                        TotalAmount = finalAmount,
                        Status = model.PaymentMethod == "COD"
                            ? "Chờ xác nhận - COD"
                            : "Chờ chuyển khoản"
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
                    }

                    db.SaveChanges();
                    transaction.Commit();

                    Session.Remove("Cart");
                    Session.Remove("KM10");

                    TempData["OrderSuccess"] = "Đặt hàng thành công!";
                    return RedirectToAction("Success");
                }
                catch (Exception ex)
                {
                    transaction.Rollback();

                    ModelState.AddModelError("", "Chi tiết lỗi SQL: " + ex.GetBaseException().Message);

                    model.TotalAmount = totalAmount;
                    model.DiscountAmount = discountAmount;
                    model.ShippingFee = shippingFee;
                    model.FinalAmount = finalAmount;

                    return View("~/Views/Checkout/Index.cshtml", model);
                }
            }
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