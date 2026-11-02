using System;
using System.Linq;
using System.Web.Mvc;
using DBWineStore.Models;

namespace DBWineStore.Controllers
{
    public class AccountController : Controller
    {
        private DBWineStoreEntities db = new DBWineStoreEntities();
        private LoginViewModel viewModel;
        private RegisterViewModel viewModelRegister;
        private AccountController accountController;

        // GET: /Account/Login
        public ActionResult Login(string returnUrl)
        {
            if (Session["UserID"] != null)
            {
                return RedirectToAction("Index","Store");
            }

            ViewBag.ReturnUrl = returnUrl;
            return View(new LoginViewModel());
        }

        // POST: /Account/Login
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Login(LoginViewModel model, string returnUrl)
        {
            if (!ModelState.IsValid)
            {
                return View(model);
            }

            var user = db.UserAccounts
                         .FirstOrDefault(u => u.UserName.ToLower() == model.Username.ToLower()
                                           && u.IsActive);

            if (user == null)
            {
                ModelState.AddModelError("", "Invalid username or password.");
                return View(model);
            }

            // Lab password check (matches your seed data)
            bool isValid =
                (user.UserName.Equals("Admin", StringComparison.OrdinalIgnoreCase) && model.Password == "Admin@123") ||
                (user.UserName.Equals("Customer01", StringComparison.OrdinalIgnoreCase) && model.Password == "Customer@123") ||
                (user.UserName.Equals("Customer02", StringComparison.OrdinalIgnoreCase) && model.Password == "Customer2@123") ||
                (user.PasswordSalt == model.Password);

            if (!isValid)
            {
                ModelState.AddModelError("", "Invalid username or password.");
                return View(model);
            }

            // ===== Login success =====
            Session["UserID"] = user.UserID;
            Session["UserName"] = user.UserName;
            Session["RoleID"] = user.RoleID;
            Session["RoleName"] = user.Role != null ? user.Role.RoleName : "Customer";

            var customer = db.Customers.FirstOrDefault(c => c.UserID == user.UserID);
            Session["FullName"] = customer != null ? customer.FullName : user.UserName;

            if (!string.IsNullOrEmpty(returnUrl) && Url.IsLocalUrl(returnUrl))
            {
                return Redirect(returnUrl);
            }

            return RedirectToAction("Index", "Store");
        }
        // ==================== REGISTER ====================

        // GET: /Account/Register
        public ActionResult Register()
        {
            if (Session["UserID"] != null)
            {
                return RedirectToAction("Index", "Store");
            }
            return View(new RegisterViewModel());
        }

        // POST: /Account/Register
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Register(RegisterViewModel model)
        {
            if (!ModelState.IsValid)
            {
                return View(model);
            }

            // Check username already exists
            if (db.UserAccounts.Any(u => u.UserName.ToLower() == model.Username.ToLower()))
            {
                ModelState.AddModelError("Username", "This username is already taken.");
                return View(model);
            }

            // Check email already exists (if provided)
            if (!string.IsNullOrWhiteSpace(model.Email) &&
                db.Customers.Any(c => c.Email == model.Email))
            {
                ModelState.AddModelError("Email", "This email is already registered.");
                return View(model);
            }

            try
            {
                // Get Customer role
                var customerRole = db.Roles.FirstOrDefault(r => r.RoleName == "Customer");
                if (customerRole == null)
                {
                    ModelState.AddModelError("", "System error: Customer role not found.");
                    return View(model);
                }

                // Create UserAccount
                var newUser = new UserAccount
                {
                    UserName = model.Username.Trim(),
                    PasswordHash = model.Password,      // Lab only – later you should hash
                    PasswordSalt = model.Password,      // Lab only
                    RoleID = customerRole.RoleID,
                    IsActive = true,
                    CreatedAt = DateTime.Now
                };

                db.UserAccounts.Add(newUser);
                db.SaveChanges();

                // Create Customer profile
                var newCustomer = new Customer
                {
                    UserID = newUser.UserID,
                    FullName = model.FullName.Trim(),
                    Email = string.IsNullOrWhiteSpace(model.Email) ? null : model.Email.Trim(),
                    Phone = string.IsNullOrWhiteSpace(model.Phone) ? null : model.Phone.Trim(),
                    Address = string.IsNullOrWhiteSpace(model.Address) ? null : model.Address.Trim()
                };

                db.Customers.Add(newCustomer);
                db.SaveChanges();

                // Auto login after register
                Session["UserID"] = newUser.UserID;
                Session["UserName"] = newUser.UserName;
                Session["RoleID"] = newUser.RoleID;
                Session["RoleName"] = "Customer";
                Session["FullName"] = newCustomer.FullName;

                return RedirectToAction("Index", "Store");
            }
            catch (Exception)
            {
                ModelState.AddModelError("", "An error occurred while creating your account. Please try again.");
                return View(model);
            }
        }

        // Logout
        // GET: /Account/Logout
        [HttpGet]
        public ActionResult Logout()
        {
            // Clear all session data
            Session.Clear();
            Session.Abandon();
            return RedirectToAction("Home", "MainPage");
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