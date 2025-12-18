using AirLineBookingSystemProject_Group03.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace AirLineBookingSystemProject_Group03.Controllers
{
    public class HomeController : Controller
    {
        private readonly QUANLYBANVEMAYBAY6Entities db = new QUANLYBANVEMAYBAY6Entities();
        public ActionResult Index()
        {
            var today = DateTime.Now.Date;

            var listFlightsQuery = db.CHUYENBAYs
                                     .Include("HANGHANGKHONG")
                                     .Include("TUYENBAY")
                                     .Where(c => c.THOIGIANXUATPHAT != null &&
                                                 System.Data.Entity.DbFunctions.TruncateTime(c.THOIGIANXUATPHAT) >= today);

            var listFlights = listFlightsQuery
                              .OrderBy(r => Guid.NewGuid()) 
                              .Take(4)
                              .ToList();

            var flightsForView = listFlights.Select(f => new
            {
                MaChuyenBay = f.MACHUYENBAY,
                TenHang = f.HANGHANGKHONG != null ? f.HANGHANGKHONG.TENHANG : "Chưa cập nhật",
                NoiDi = f.TUYENBAY?.SANBAY?.THANHPHO ?? "?",
                NoiDen = f.TUYENBAY?.SANBAY1?.THANHPHO ?? "?",
                GioDi = f.THOIGIANXUATPHAT,
                GioDen = f.THOIGIANDEN
            }).ToList();

            return View(listFlights);
        }

        public ActionResult GetPopularFlights(int page = 1)
        {
            int pageSize = 4;
            int maxItems = 10; 

            int skip = (page - 1) * pageSize;

            int take = pageSize;
            if (skip + take > maxItems) take = maxItems - skip;
            if (take < 0) take = 0;

            var listFlights = db.CHUYENBAYs.Include("HANGHANGKHONG").Include("TUYENBAY")
                                .OrderBy(x => x.THOIGIANXUATPHAT)
                                .Skip(skip)
                                .Take(take)
                                .ToList();

            ViewBag.TotalItems = maxItems;
            ViewBag.CurrentPage = page;
            ViewBag.PageSize = pageSize;

            return PartialView("_PopularFlightsPartial", listFlights);
        }
        public ActionResult About()
        {
            ViewBag.Message = "Your application description page.";

            return View();
        }

        public ActionResult Contact()
        {
            ViewBag.Message = "Your contact page.";

            return View();
        }
        public ActionResult Reviews()
        {
            // Lấy danh sách đánh giá kèm thông tin khách hàng và hãng hàng không
            var danhGiaList = db.DANHGIAs
                .OrderByDescending(d => d.NGAYDG)
                .ToList();

            ViewBag.DanhGiaList = danhGiaList;

            // Lấy danh sách hãng hàng không để hiển thị trong dropdown
            var hangHangKhongList = db.HANGHANGKHONGs.ToList();
            ViewBag.HangHangKhongList = new SelectList(hangHangKhongList, "MAHANG", "TENHANG");

            // Kiểm tra xem người dùng đã đăng nhập chưa và lấy thông tin khách hàng
            if (User.Identity.IsAuthenticated)
            {
                var userName = User.Identity.Name;
                var taiKhoan = db.TaiKhoans.FirstOrDefault(t => t.TenDangNhap == userName || t.Email == userName);
                if (taiKhoan != null)
                {
                    var khachHang = db.KHACHHANGs.FirstOrDefault(k => k.MaTk == taiKhoan.MaTK);
                    if (khachHang != null)
                    {
                        ViewBag.MaKH = khachHang.MAKH;
                        ViewBag.HoTen = khachHang.HOTEN;
                    }
                }
            }


            return View();
        }
        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize]
        public ActionResult Reviews(string maHang, int soSao, string noiDung)
        {
            if (string.IsNullOrEmpty(maHang) || soSao < 1 || soSao > 5 || string.IsNullOrEmpty(noiDung))
            {
                TempData["ErrorMessage"] = "Vui lòng điền đầy đủ thông tin đánh giá!";
                return RedirectToAction("Reviews");
            }

            try
            {
                var userName = User.Identity.Name;
                var taiKhoan = db.TaiKhoans.FirstOrDefault(t => t.TenDangNhap == userName || t.Email == userName);

                if (taiKhoan == null)
                {
                    TempData["ErrorMessage"] = "Không tìm thấy thông tin tài khoản!";
                    return RedirectToAction("Reviews");
                }

                var khachHang = db.KHACHHANGs.FirstOrDefault(k => k.MaTk == taiKhoan.MaTK);

                if (khachHang == null)
                {
                    TempData["ErrorMessage"] = "Không tìm thấy thông tin khách hàng!";
                    return RedirectToAction("Reviews");
                }

                var maxId = db.DANHGIAs
                    .Select(d => d.ID)
                    .Where(id => id != null)
                    .OrderByDescending(id => id)
                    .FirstOrDefault();

                int newIdNumber = 1;
                if (maxId != null && int.TryParse(maxId, out int parsedId))
                {
                    newIdNumber = parsedId + 1;
                }

                string newId = newIdNumber.ToString().PadLeft(10, '0');

                var danhGia = new DANHGIA
                {
                    ID = newId,
                    MAKH = khachHang.MAKH,
                    MAHANG = maHang,
                    SOSAO = soSao,
                    NOIDUNG = noiDung,
                    NGAYDG = DateTime.Now
                };

                db.DANHGIAs.Add(danhGia);
                db.SaveChanges();

                TempData["SuccessMessage"] = "Cảm ơn bạn đã đánh giá!";
                return RedirectToAction("Reviews");
            }
            catch (Exception ex)
            {
                TempData["ErrorMessage"] = "Có lỗi xảy ra khi lưu đánh giá: " + ex.Message;
                return RedirectToAction("Reviews");
            }
        }
        public ActionResult Promotions()
        {
            return View();
        }
        protected override void Dispose(bool disposing)
        {
            if (disposing) db.Dispose();
            base.Dispose(disposing);
        }
    }
}