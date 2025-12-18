using AirLineBookingSystemProject_Group03.Models;
using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Diagnostics;
using System.Linq;
using System.Data.SqlClient;
using System.Web;
using System.Web.Mvc;

namespace AirLineBookingSystemProject_Group03.Controllers
{
    public class CheckoutController : Controller
    {
        private readonly QUANLYBANVEMAYBAY6Entities db = new QUANLYBANVEMAYBAY6Entities();
        private static int nextDatVeId = 0;
        private static int nextVeId = 0;
        // GET: Checkout/Index
        [Authorize]
        public ActionResult Index()
        {
            var userName = User.Identity.Name;
            var taiKhoan = db.TaiKhoans.FirstOrDefault(t => t.TenDangNhap == userName || t.Email == userName);

            if (taiKhoan == null)
                return RedirectToAction("Login", "Account");

            var khachHang = db.KHACHHANGs.FirstOrDefault(k => k.MaTk == taiKhoan.MaTK);
            if (khachHang == null)
                return RedirectToAction("Cart", "Booking");

            var cartItems = db.GIOHANGs
                .Include(g => g.CHUYENBAY)
                .Include(g => g.CHUYENBAY.HANGHANGKHONG)
                .Include(g => g.CHUYENBAY.TUYENBAY)
                .Include(g => g.CHUYENBAY.TUYENBAY.SANBAY)
                .Include(g => g.CHUYENBAY.TUYENBAY.SANBAY1)
                .Where(g => g.MAKH == khachHang.MAKH)
                .ToList();

            if (!cartItems.Any())
            {
                TempData["Error"] = "Giỏ hàng trống! Vui lòng chọn vé trước khi thanh toán.";
                return RedirectToAction("Cart", "Booking");
            }

            decimal tongTien = cartItems.Sum(g => (g.GIATIEN ?? 0) * (g.SOLUONG ?? 1));

            ViewBag.KhachHang = khachHang;
            ViewBag.TaiKhoan = taiKhoan;
            ViewBag.CartItems = cartItems;
            ViewBag.TongTien = tongTien;

            return View();
        }

        // POST: Checkout/Confirm
        [HttpPost]
        [Authorize]
        [ValidateAntiForgeryToken]
        public ActionResult Confirm(string hoTen, string soDienThoai, string email,
                            string diaChi, string ghiChu, string phuongThucTT)
        {
            try
            {
                var userName = User.Identity.Name;
                var taiKhoan = db.TaiKhoans
                    .FirstOrDefault(t => t.TenDangNhap == userName || t.Email == userName);

                if (taiKhoan == null)
                    return RedirectToAction("Login", "Account");

                var khachHang = db.KHACHHANGs.FirstOrDefault(k => k.MaTk == taiKhoan.MaTK);
                if (khachHang == null)
                {
                    TempData["Error"] = "Vui lòng cập nhật thông tin khách hàng.";
                    return RedirectToAction("Profile", "Account");
                }

                var cartItems = db.GIOHANGs
                    .Include(g => g.CHUYENBAY)
                    .Include(g => g.CHUYENBAY.TUYENBAY)
                    .Where(g => g.MAKH == khachHang.MAKH)
                    .ToList();

                if (!cartItems.Any())
                {
                    TempData["Error"] = "Giỏ hàng trống!";
                    return RedirectToAction("Cart", "Booking");
                }

                string maDatVe = GenerateBookingCode();

                db.DATVEs.Add(new DATVE
                {
                    MADATVE = maDatVe,
                    MAKH = khachHang.MAKH,
                    MACHUYENBAY = cartItems.First().MACHUYENBAY,
                    TRANGTHAI = "Chưa thanh toán"
                });

                int tongSoVe = 0;

                foreach (var item in cartItems)
                {
                    for (int i = 0; i < item.SOLUONG; i++)
                    {
                        string loaiVeSql = item.LOAIVE == "CHD" ? "Trẻ em" :
                                           item.LOAIVE == "INF" ? "Em bé" :
                                           "Người lớn";

                        string hangVeSql = item.HANGVE == "BUS" ? "Thương Gia" :
                                           item.HANGVE == "VIP" ? "VIP" :
                                           "Phổ Thông";

                        db.VEs.Add(new VE
                        {
                            MAVE = GenerateTicketCode(),
                            MADATVE = maDatVe,
                            MASANBAY = item.CHUYENBAY.TUYENBAY.MASANBAYDI,
                            GIATIEN = (double)(item.GIATIEN_SAUKM ?? item.GIATIEN ?? 0),
                            LOAIVE = loaiVeSql,
                            HANGVE = hangVeSql,
                            MAGHE = "GHE" + Guid.NewGuid().ToString("N").Substring(0, 5)
                        });

                        tongSoVe++;
                    }
                }

                db.SaveChanges();

                db.Database.ExecuteSqlCommand(
                    "EXEC PROC_THANHTOAN @MADATVE, @PHUONGTHUCTT",
                    new SqlParameter("@MADATVE", maDatVe),
                    new SqlParameter("@PHUONGTHUCTT", phuongThucTT)
                );

                TempData["MaDatVe"] = maDatVe;
                TempData["SoVe"] = tongSoVe;
                TempData["HoTen"] = hoTen;
                TempData["Email"] = email;

                return RedirectToAction("Success");
            }
            catch (Exception ex)
            {
                TempData["Error"] = ex.InnerException?.InnerException?.Message ?? ex.Message;
                return RedirectToAction("Index");
            }
        }

        // GET: Checkout/Success
        [Authorize]
        public ActionResult Success()
        {
            if (TempData["MaDatVe"] == null)
            {
                return RedirectToAction("Cart", "Booking");
            }

            ViewBag.MaDatVe = TempData["MaDatVe"];
            ViewBag.HoTen = TempData["HoTen"];
            ViewBag.Email = TempData["Email"];
            ViewBag.SoDienThoai = TempData["SoDienThoai"];
            ViewBag.TongTien = TempData["TongTien"];
            ViewBag.NgayDat = TempData["NgayDat"];
            ViewBag.SoVe = TempData["SoVe"];

            return View();
        }
        private string GenerateBookingCode()
        {
            string prefix = "DV";
            int currentIdValue = 0;

            if (nextDatVeId == 0)
            {
                var lastId = db.DATVEs.OrderByDescending(d => d.MADATVE).Select(d => d.MADATVE).FirstOrDefault();
                if (!string.IsNullOrEmpty(lastId) && lastId.Trim().StartsWith(prefix))
                {
                    string trimmedId = lastId.Trim();
                    string numericPart = trimmedId.Substring(prefix.Length);

                    if (int.TryParse(numericPart, out currentIdValue))
                    {
                        nextDatVeId = currentIdValue;
                    }
                }
            }
            nextDatVeId++;
            return prefix + nextDatVeId.ToString("D5");
        }
        private string GenerateTicketCode()
        {
            string prefix = "VE";
            int currentIdValue = 0;

            if (nextVeId == 0)
            {
                var lastId = db.VEs.OrderByDescending(v => v.MAVE).Select(v => v.MAVE).FirstOrDefault();
                if (!string.IsNullOrEmpty(lastId) && lastId.Trim().StartsWith(prefix))
                {
                    string trimmedId = lastId.Trim();
                    string numericPart = trimmedId.Substring(prefix.Length);

                    if (int.TryParse(numericPart, out currentIdValue))
                    {
                        nextVeId = currentIdValue;
                    }
                }
            }
            nextVeId++;
            return prefix + nextVeId.ToString("D5");
        }
        protected override void Dispose(bool disposing)
        {
            if (disposing) db.Dispose();
            base.Dispose(disposing);
        }
    }
}