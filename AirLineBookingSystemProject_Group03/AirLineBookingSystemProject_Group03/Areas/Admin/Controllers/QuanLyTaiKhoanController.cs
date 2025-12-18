using AirLineBookingSystemProject_Group03.Models;
using Microsoft.AspNet.Identity;
using Microsoft.AspNet.Identity.EntityFramework;
using Microsoft.AspNet.Identity.Owin;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Principal;
using System.Web;
using System.Web.Mvc;

namespace AirLineBookingSystemProject_Group03.Areas.Admin.Controllers
{
    [Authorize(Roles = "Admin")]
    public class QuanLyTaiKhoanController : Controller
    {
        private ApplicationDbContext db = new ApplicationDbContext();
        private QUANLYBANVEMAYBAY6Entities dbBiz = new QUANLYBANVEMAYBAY6Entities();
        public ActionResult Index()
        {
            var userManager = new UserManager<ApplicationUser>(new UserStore<ApplicationUser>(db));
            var users = db.Users.ToList();
            var model = new List<UserViewModel>();

            foreach (var user in users)
            {
                var u = new UserViewModel();
                u.Id = user.Id;
                u.UserName = user.UserName;
                u.Email = user.Email;
                u.PhoneNumber = user.PhoneNumber;

                var roles = userManager.GetRoles(user.Id);
                u.Role = roles.FirstOrDefault() ?? "Chưa phân quyền";

                u.IsLocked = userManager.IsLockedOut(user.Id);

                model.Add(u);
            }

            return View(model);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult ChangeRole(string id, string newRole)
        {
            var userManager = new UserManager<ApplicationUser>(new UserStore<ApplicationUser>(db));
            var user = userManager.FindById(id);

            if (user == null)
            {
                TempData["Msg"] = "Không tìm thấy tài khoản!";
                return RedirectToAction("Index");
            }

            string roleNameForIdentity = newRole;

            if (newRole.Equals("KhachHang", StringComparison.OrdinalIgnoreCase))
            {
                roleNameForIdentity = "user";
            }

            if (newRole.Equals("Admin", StringComparison.OrdinalIgnoreCase))
            {
                roleNameForIdentity = "Admin";
            }

            var oldRoles = userManager.GetRoles(id);
            if (oldRoles.Count > 0)
            {
                userManager.RemoveFromRoles(id, oldRoles.ToArray());
            }
            var result = userManager.AddToRole(id, roleNameForIdentity);

            if (result.Succeeded)
            {
                try
                {
                    var taiKhoanSQL = dbBiz.TaiKhoans.FirstOrDefault(t => t.Email == user.Email);

                    if (taiKhoanSQL != null)
                    {
                        string vaiTroForSQL = "KhachHang";

                        if (roleNameForIdentity.Equals("Admin", StringComparison.OrdinalIgnoreCase) ||
                            roleNameForIdentity.Equals("QuanLy", StringComparison.OrdinalIgnoreCase))
                        {
                            vaiTroForSQL = "QuanLy";
                        }
                        taiKhoanSQL.VaiTro = vaiTroForSQL;
                        dbBiz.SaveChanges();

                        TempData["Msg"] = $"Đổi thành công: Identity là '{roleNameForIdentity}' - SQL là '{vaiTroForSQL}'";
                    }
                    else
                    {
                        TempData["Msg"] = "Đổi quyền Web thành công nhưng không tìm thấy Email trong bảng TaiKhoan.";
                    }
                }
                catch (Exception ex)
                {
                    TempData["Msg"] = "Lỗi đồng bộ SQL: " + ex.Message;
                }
            }
            else
            {
                TempData["Msg"] = "Lỗi Identity: " + string.Join(", ", result.Errors);
            }

            return RedirectToAction("Index");
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult ToggleLock(string id)
        {
            var userManager = new UserManager<ApplicationUser>(new UserStore<ApplicationUser>(db));

            userManager.SetLockoutEnabled(id, true);

            if (userManager.IsLockedOut(id))
            {
                userManager.SetLockoutEndDate(id, DateTimeOffset.UtcNow.AddMinutes(-1));
                userManager.ResetAccessFailedCount(id);
            }
            else
            {
                userManager.SetLockoutEndDate(id, DateTimeOffset.UtcNow.AddYears(999));
            }

            return RedirectToAction("Index");
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Delete(string id)
        {
            var userManager = new UserManager<ApplicationUser>(new UserStore<ApplicationUser>(db));
            var user = userManager.FindById(id);

            if (user != null)
            {
                userManager.Delete(user);
            }
            return RedirectToAction("Index");
        }
    }
}