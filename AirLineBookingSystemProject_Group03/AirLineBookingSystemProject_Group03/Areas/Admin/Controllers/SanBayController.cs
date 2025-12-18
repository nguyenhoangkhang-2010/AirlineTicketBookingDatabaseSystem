using System;
using System.Linq;
using System.Net;
using System.Web.Mvc;
using AirLineBookingSystemProject_Group03.Models;

namespace AirLineBookingSystemProject_Group03.Areas.Admin.Controllers
{
    [Authorize(Roles = "Admin")]
    public class SanBayController : Controller
    {
        private QUANLYBANVEMAYBAY6Entities db = new QUANLYBANVEMAYBAY6Entities();

        public ActionResult Index(string searchString)
        {
            var sanBays = from s in db.SANBAYs select s;

            if (!String.IsNullOrEmpty(searchString))
            {
                sanBays = sanBays.Where(s => s.MASANBAY.Contains(searchString)
                                          || s.TENSANBAY.Contains(searchString)
                                          || s.THANHPHO.Contains(searchString)
                                          || s.DATNUOC.Contains(searchString));
            }

            return View(sanBays.OrderBy(s => s.MASANBAY).ToList());
        }

        // 2. TẠO MỚI (GET)
        public ActionResult Create()
        {
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Create([Bind(Include = "MASANBAY,TENSANBAY,THANHPHO,DATNUOC")] SANBAY sanBay)
        {
            if (ModelState.IsValid)
            {
                // Check trùng mã
                if (db.SANBAYs.Any(x => x.MASANBAY == sanBay.MASANBAY))
                {
                    ModelState.AddModelError("MASANBAY", "Mã sân bay này đã tồn tại!");
                    return View(sanBay);
                }

                db.SANBAYs.Add(sanBay);
                db.SaveChanges();
                TempData["Msg"] = "Thêm sân bay thành công!";
                return RedirectToAction("Index");
            }
            return View(sanBay);
        }

        public ActionResult Edit(string id)
        {
            if (id == null) return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            SANBAY sanBay = db.SANBAYs.Find(id);
            if (sanBay == null) return HttpNotFound();
            return View(sanBay);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Edit([Bind(Include = "MASANBAY,TENSANBAY,THANHPHO,DATNUOC")] SANBAY sanBay)
        {
            if (ModelState.IsValid)
            {
                db.Entry(sanBay).State = System.Data.Entity.EntityState.Modified;
                db.SaveChanges();
                TempData["Msg"] = "Cập nhật thông tin thành công!";
                return RedirectToAction("Index");
            }
            return View(sanBay);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Delete(string id)
        {
            SANBAY sanBay = db.SANBAYs.Find(id);
            try
            {
                db.SANBAYs.Remove(sanBay);
                db.SaveChanges();
                TempData["Msg"] = "Đã xóa sân bay!";
            }
            catch (Exception)
            {
                TempData["Error"] = "Không thể xóa! Sân bay này đang có chuyến bay hoạt động.";
            }
            return RedirectToAction("Index");
        }
    }
}