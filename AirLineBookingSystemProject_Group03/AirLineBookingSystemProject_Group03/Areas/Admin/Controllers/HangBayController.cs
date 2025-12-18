using AirLineBookingSystemProject_Group03.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Web;
using System.Web.Mvc;

namespace AirLineBookingSystemProject_Group03.Areas.Admin.Controllers
{
    public class HangBayController : Controller
    {
        private QUANLYBANVEMAYBAY6Entities db = new QUANLYBANVEMAYBAY6Entities();

        public ActionResult Index()
        {
            ViewBag.Active = "ManageAirlines";
            var model = db.HANGHANGKHONGs.ToList();
            return View(model);
        }

        public ActionResult Create()
        {
            ViewBag.Active = "CreateAirlines";
            return View(new HANGHANGKHONG());
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Create(HANGHANGKHONG model)
        {
            ViewBag.Active = "CreateAirlines";
            if (ModelState.IsValid)
            {
                if (db.HANGHANGKHONGs.Any(x => x.MAHANG == model.MAHANG))
                {
                    ModelState.AddModelError("MAHANG", "Mã hãng này đã tồn tại! Vui lòng chọn mã khác.");
                    return View(model);
                }

                try
                {
                    db.HANGHANGKHONGs.Add(model);
                    db.SaveChanges();

                    TempData["AdminMessage"] = "Thêm hãng bay " + model.TENHANG + " thành công!";
                    TempData["AlertType"] = "success";
                    return RedirectToAction("Index");
                }
                catch (Exception ex)
                {
                    TempData["AdminMessage"] = "Lỗi: " + ex.Message;
                    TempData["AlertType"] = "error";
                }
            }
            return View(model);
        }

        public ActionResult Edit(string id)
        {
            ViewBag.Active = "ManageAirlines";
            if (string.IsNullOrEmpty(id)) return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            var model = db.HANGHANGKHONGs.Find(id);
            if (model == null) return HttpNotFound();

            return View(model);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Edit(HANGHANGKHONG model)
        {
            ViewBag.Active = "ManageAirlines";
            if (ModelState.IsValid)
            {
                var hang = db.HANGHANGKHONGs.Find(model.MAHANG);
                if (hang != null)
                {
                    hang.TENHANG = model.TENHANG;
                    hang.LOGO = model.LOGO;

                    db.SaveChanges();
                    TempData["AdminMessage"] = "Cập nhật hãng bay thành công!";
                    TempData["AlertType"] = "success";
                    return RedirectToAction("Index");
                }
            }
            return View(model);
        }

        public ActionResult Delete(string id)
        {
            ViewBag.Active = "ManageAirlines";
            if (string.IsNullOrEmpty(id)) return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            var model = db.HANGHANGKHONGs.Find(id);
            if (model == null) return HttpNotFound();
            return View(model);
        }

        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public ActionResult DeleteConfirmed(string id)
        {
            ViewBag.Active = "ManageAirlines";
            var model = db.HANGHANGKHONGs.Find(id);
            try
            {
                if (db.CHUYENBAYs.Any(x => x.MAHANG == id))
                {
                    TempData["AdminMessage"] = "Không thể xóa! Hãng này đang có các chuyến bay hoạt động.";
                    TempData["AlertType"] = "error";
                    return RedirectToAction("Index");
                }

                db.HANGHANGKHONGs.Remove(model);
                db.SaveChanges();

                TempData["AdminMessage"] = "Đã xóa hãng bay thành công!";
                TempData["AlertType"] = "success";
            }
            catch (Exception ex)
            {
                TempData["AdminMessage"] = "Lỗi hệ thống: " + ex.Message;
                TempData["AlertType"] = "error";
            }
            return RedirectToAction("Index");
        }
    }
}