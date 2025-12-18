using System;
using System.Linq;
using System.Net;
using System.Web.Mvc;
using System.Data.Entity;
using AirLineBookingSystemProject_Group03.Models;

namespace AirLineBookingSystemProject_Group03.Areas.Admin.Controllers
{
    [Authorize(Roles = "Admin")]
    public class TuyenBayController : Controller
    {
        private QUANLYBANVEMAYBAY6Entities db = new QUANLYBANVEMAYBAY6Entities();

        public ActionResult Index(string searchString)
        {
            var tuyenBays = db.TUYENBAYs.Include(t => t.SANBAY).Include(t => t.SANBAY1);

            if (!String.IsNullOrEmpty(searchString))
            {
                tuyenBays = tuyenBays.Where(t => t.SANBAY.THANHPHO.Contains(searchString)
                                              || t.SANBAY1.THANHPHO.Contains(searchString));
            }

            return View(tuyenBays.ToList());
        }

        public ActionResult Create()
        {
            ViewBag.MASANBAYDI = new SelectList(db.SANBAYs.Select(s => new {
                MASANBAY = s.MASANBAY,
                TenHienThi = s.THANHPHO + " (" + s.TENSANBAY + ")"
            }), "MASANBAY", "TenHienThi");

            ViewBag.MASANBAYVE = new SelectList(db.SANBAYs.Select(s => new {
                MASANBAY = s.MASANBAY,
                TenHienThi = s.THANHPHO + " (" + s.TENSANBAY + ")"
            }), "MASANBAY", "TenHienThi");

            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Create([Bind(Include = "MATUYENBAY,MASANBAYDI,MASANBAYVE")] TUYENBAY tuyenBay)
        {
            if (ModelState.IsValid)
            {
                if (tuyenBay.MASANBAYDI == tuyenBay.MASANBAYVE)
                {
                    ModelState.AddModelError("", "Sân bay Đi và Sân bay Về không được trùng nhau!");
                    ReLoadDropdown(tuyenBay);
                    return View(tuyenBay);
                }

                bool exists = db.TUYENBAYs.Any(x => x.MASANBAYDI == tuyenBay.MASANBAYDI && x.MASANBAYVE == tuyenBay.MASANBAYVE);
                if (exists)
                {
                    ModelState.AddModelError("", "Tuyến bay này đã tồn tại trong hệ thống!");
                    ReLoadDropdown(tuyenBay);
                    return View(tuyenBay);
                }

                tuyenBay.MATUYENBAY = "TB" + new Random().Next(1000, 9999);

                db.TUYENBAYs.Add(tuyenBay);
                db.SaveChanges();
                TempData["Msg"] = "Thêm tuyến bay thành công!";
                return RedirectToAction("Index");
            }

            ReLoadDropdown(tuyenBay);
            return View(tuyenBay);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Delete(string id)
        {
            TUYENBAY tb = db.TUYENBAYs.Find(id);
            try
            {
                db.TUYENBAYs.Remove(tb);
                db.SaveChanges();
                TempData["Msg"] = "Đã xóa tuyến bay!";
            }
            catch (Exception)
            {
                TempData["Error"] = "Không thể xóa! Tuyến này đang có lịch bay hoạt động.";
            }
            return RedirectToAction("Index");
        }

        public ActionResult Edit(string id)
        {
            if (id == null) return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            TUYENBAY tuyenBay = db.TUYENBAYs.Find(id);
            if (tuyenBay == null) return HttpNotFound();

            ReLoadDropdown(tuyenBay);
            return View(tuyenBay);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Edit([Bind(Include = "MATUYENBAY,MASANBAYDI,MASANBAYVE")] TUYENBAY tuyenBay)
        {
            if (ModelState.IsValid)
            {
                if (tuyenBay.MASANBAYDI == tuyenBay.MASANBAYVE)
                {
                    ModelState.AddModelError("", "Sân bay Đi và Sân bay Về không được trùng nhau!");
                    ReLoadDropdown(tuyenBay);
                    return View(tuyenBay);
                }


                bool exists = db.TUYENBAYs.Any(x => x.MASANBAYDI == tuyenBay.MASANBAYDI
                                                 && x.MASANBAYVE == tuyenBay.MASANBAYVE
                                                 && x.MATUYENBAY != tuyenBay.MATUYENBAY);
                if (exists)
                {
                    ModelState.AddModelError("", "Tuyến bay này đã tồn tại rồi (trùng với một mã khác)!");
                    ReLoadDropdown(tuyenBay);
                    return View(tuyenBay);
                }

                db.Entry(tuyenBay).State = System.Data.Entity.EntityState.Modified;
                db.SaveChanges();
                TempData["Msg"] = "Cập nhật tuyến bay thành công!";
                return RedirectToAction("Index");
            }

            ReLoadDropdown(tuyenBay);
            return View(tuyenBay);
        }

        private void ReLoadDropdown(TUYENBAY tb)
        {
            ViewBag.MASANBAYDI = new SelectList(db.SANBAYs.Select(s => new {
                MASANBAY = s.MASANBAY,
                TenHienThi = s.THANHPHO + " (" + s.TENSANBAY + ")"
            }), "MASANBAY", "TenHienThi", tb.MASANBAYDI);

            ViewBag.MASANBAYVE = new SelectList(db.SANBAYs.Select(s => new {
                MASANBAY = s.MASANBAY,
                TenHienThi = s.THANHPHO + " (" + s.TENSANBAY + ")"
            }), "MASANBAY", "TenHienThi", tb.MASANBAYVE);
        }
    }
}