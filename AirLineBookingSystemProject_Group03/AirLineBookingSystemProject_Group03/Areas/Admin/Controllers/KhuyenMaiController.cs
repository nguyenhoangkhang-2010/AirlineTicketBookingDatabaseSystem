using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using System.Data.Entity;
using AirLineBookingSystemProject_Group03.Models;
using System.Net;
using PagedList;
using System.Web.UI;

namespace AirLineBookingSystemProject_Group03.Areas.Admin.Controllers
{
    [Authorize(Roles = "Admin")]
    public class KhuyenMaiController : Controller
    {
        private QUANLYBANVEMAYBAY6Entities db = new QUANLYBANVEMAYBAY6Entities();

        public ActionResult Index(int? page)
        {
            int pageSize = 10;
            int pageNumber = (page ?? 1);
            var list = db.KHUYENMAIs.OrderByDescending(k => k.NGAYBATDAU).ToList();
            return View(list.ToPagedList(pageNumber, pageSize));
        }

        public ActionResult Create()
        {
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Create(KHUYENMAI khuyenMai)
        {
            if (ModelState.IsValid)
            {
                if (db.KHUYENMAIs.Any(k => k.ID == khuyenMai.ID))
                {
                    ModelState.AddModelError("ID", "Mã khuyến mãi này đã tồn tại!");
                    return View(khuyenMai);
                }

                if (khuyenMai.NGAYKETTHUC < khuyenMai.NGAYBATDAU)
                {
                    ModelState.AddModelError("NGAYKETTHUC", "Ngày kết thúc phải lớn hơn hoặc bằng ngày bắt đầu!");
                    return View(khuyenMai);
                }

                khuyenMai.ID = khuyenMai.ID.ToUpper().Trim();
                khuyenMai.DASDUNG = 0;

                if (string.IsNullOrEmpty(khuyenMai.TRANGTHAI))
                {
                    khuyenMai.TRANGTHAI = "Hoạt động";
                }

                if (khuyenMai.LOAIKHUYENMAI == "PhanTram" && khuyenMai.GIATRI > 1)
                {
                    ModelState.AddModelError("GIATRI", "Với loại Phần Trăm, vui lòng nhập số thập phân (VD: 0.1 cho 10%)");
                    return View(khuyenMai);
                }

                db.KHUYENMAIs.Add(khuyenMai);
                db.SaveChanges();
                TempData["Msg"] = "Tạo mã khuyến mãi thành công!";
                return RedirectToAction("Index");
            }

            return View(khuyenMai);
        }

        public ActionResult Edit(string id)
        {
            if (id == null) return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            KHUYENMAI khuyenMai = db.KHUYENMAIs.Find(id);
            if (khuyenMai == null) return HttpNotFound();
            return View(khuyenMai);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Edit(KHUYENMAI khuyenMai)
        {
            if (ModelState.IsValid)
            {
                if (khuyenMai.NGAYKETTHUC < khuyenMai.NGAYBATDAU)
                {
                    ModelState.AddModelError("NGAYKETTHUC", "Ngày kết thúc không hợp lệ!");
                    return View(khuyenMai);
                }

                db.Entry(khuyenMai).State = EntityState.Modified;
                db.SaveChanges();
                TempData["Msg"] = "Cập nhật khuyến mãi thành công!";
                return RedirectToAction("Index");
            }
            return View(khuyenMai);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Delete(string id)
        {
            try
            {
                KHUYENMAI km = db.KHUYENMAIs.Find(id);
                if (km != null)
                {
                    if (km.DASDUNG > 0)
                    {
                        km.TRANGTHAI = "Đã hủy";
                        db.SaveChanges();
                        TempData["Msg"] = "Mã đã có người dùng, chuyển trạng thái sang 'Đã hủy'!";
                    }
                    else
                    {
                        db.KHUYENMAIs.Remove(km);
                        db.SaveChanges();
                        TempData["Msg"] = "Đã xóa mã khuyến mãi vĩnh viễn!";
                    }
                }
            }
            catch (Exception ex)
            {
                TempData["Error"] = "Lỗi hệ thống: " + ex.InnerException?.Message ?? ex.Message;
            }
            return RedirectToAction("Index");
        }
    }
}