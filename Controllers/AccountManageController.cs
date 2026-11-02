using System;
using System.Linq;
using System.Web.Mvc;
using DBWineStore.Models;

namespace DBWineStore.Controllers
{
    public class AccountManageController : Controller
    {
        private DBWineStoreEntities db = new DBWineStoreEntities();
        private AccountManageViewModel viewModel;

        //Only Admin can access
        private bool IsAdmin()
        {
            return Session["RoleName"] != null &&
                   Session["RoleName"].ToString() == "Admin";
        }

        // GET: AccountManage
        public ActionResult Index()
        {
            if (!IsAdmin())
                return RedirectToAction("Login", "Account");

            var accounts = (from u in db.UserAccounts
                            join r in db.Roles on u.RoleID equals r.RoleID
                            join c in db.Customers on u.UserID equals c.UserID into customerGroup
                            from c in customerGroup.DefaultIfEmpty()
                            select new AccountManageViewModel
                            {
                                UserID = u.UserID,
                                UserName = u.UserName,
                                FullName = c != null ? c.FullName : u.UserName,
                                Email = c != null ? c.Email : "",
                                Phone = c != null ? c.Phone : "",
                                RoleName = r.RoleName,
                                RoleID = u.RoleID,
                                IsActive = u.IsActive,
                                CreatedAt = u.CreatedAt
                            }).OrderByDescending(x => x.CreatedAt).ToList();

            return View(accounts);
        }

        // GET: AccountManage/Edit/5
        public ActionResult Edit(int id)
        {
            if (!IsAdmin())
                return RedirectToAction("Login", "Account");

            var user = db.UserAccounts.Find(id);
            if (user == null) return HttpNotFound();

            var customer = db.Customers.FirstOrDefault(c => c.UserID == id);

            var model = new EditAccountViewModel
            {
                UserID = user.UserID,
                UserName = user.UserName,
                FullName = customer?.FullName ?? "",
                Email = customer?.Email ?? "",
                Phone = customer?.Phone ?? "",
                Address = customer?.Address ?? "",
                RoleID = user.RoleID,
                IsActive = user.IsActive
            };

            ViewBag.Roles = new SelectList(db.Roles.ToList(), "RoleID", "RoleName", user.RoleID);
            return View(model);
        }

        // POST: AccountManage/Edit/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Edit(EditAccountViewModel model)
        {
            if (!IsAdmin())
                return RedirectToAction("Login", "Account");

            if (!ModelState.IsValid)
            {
                ViewBag.Roles = new SelectList(db.Roles.ToList(), "RoleID", "RoleName", model.RoleID);
                return View(model);
            }

            var user = db.UserAccounts.Find(model.UserID);
            if (user == null) return HttpNotFound();

            user.RoleID = model.RoleID;
            user.IsActive = model.IsActive;

            var customer = db.Customers.FirstOrDefault(c => c.UserID == model.UserID);
            if (customer != null)
            {
                customer.FullName = model.FullName;
                customer.Email = model.Email;
                customer.Phone = model.Phone;
                customer.Address = model.Address;
            }
            else if (!string.IsNullOrWhiteSpace(model.FullName))
            {
                // Create customer profile if not exists
                db.Customers.Add(new Customer
                {
                    UserID = model.UserID,
                    FullName = model.FullName,
                    Email = model.Email,
                    Phone = model.Phone,
                    Address = model.Address
                });
            }

            db.SaveChanges();
            TempData["Success"] = "Account updated successfully!";
            return RedirectToAction("Index");
        }

        // Toggle Active / Inactive
        public ActionResult ToggleActive(int id)
        {
            if (!IsAdmin())
                return RedirectToAction("Login", "Account");

            var user = db.UserAccounts.Find(id);
            if (user == null) return HttpNotFound();

            // Don't allow deactivating yourself
            if (Session["UserID"] != null && (int)Session["UserID"] == id)
            {
                TempData["Error"] = "You cannot deactivate your own account.";
                return RedirectToAction("Index");
            }

            user.IsActive = !user.IsActive;
            db.SaveChanges();

            TempData["Success"] = user.IsActive ? "Account activated." : "Account deactivated.";
            return RedirectToAction("Index");
        }

        // Delete account
        public ActionResult Delete(int id)
        {
            if (!IsAdmin())
                return RedirectToAction("Login", "Account");

            var user = db.UserAccounts.Find(id);
            if (user == null) return HttpNotFound();

            if (Session["UserID"] != null && (int)Session["UserID"] == id)
            {
                TempData["Error"] = "You cannot delete your own account.";
                return RedirectToAction("Index");
            }

            // Delete customer first
            var customer = db.Customers.FirstOrDefault(c => c.UserID == id);
            if (customer != null)
            {
                // Check if customer has orders
                if (db.OrderProes.Any(o => o.CustomerID == customer.CustomerID))
                {
                    TempData["Error"] = "Cannot delete account because it has orders.";
                    return RedirectToAction("Index");
                }
                db.Customers.Remove(customer);
            }

            db.UserAccounts.Remove(user);
            db.SaveChanges();

            TempData["Success"] = "Account deleted successfully.";
            return RedirectToAction("Index");
        }

        protected override void Dispose(bool disposing)
        {
            if (disposing) db.Dispose();
            base.Dispose(disposing);
        }
    }
}