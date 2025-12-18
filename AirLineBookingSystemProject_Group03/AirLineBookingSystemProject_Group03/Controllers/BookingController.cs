using AirLineBookingSystemProject_Group03.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Web;
using System.Web.Mvc;
using PagedList;
using System.Web.UI;

namespace AirLineBookingSystemProject_Group03.Controllers
{
    public class BookingController : Controller
    {
        private readonly QUANLYBANVEMAYBAY6Entities db = new QUANLYBANVEMAYBAY6Entities();
        // GET: Booking
        public ActionResult Flights(string from, string to, DateTime? date, int? page)
        {
            var query = db.CHUYENBAYs.AsQueryable();

            if (!string.IsNullOrEmpty(from))
            {
                query = query.Where(c => c.TUYENBAY.SANBAY.THANHPHO.Contains(from) || c.TUYENBAY.SANBAY.MASANBAY == from);
            }
            if (!string.IsNullOrEmpty(to))
            {
                query = query.Where(c => c.TUYENBAY.SANBAY1.THANHPHO.Contains(to) || c.TUYENBAY.SANBAY1.MASANBAY == to);
            }
            if (date.HasValue)
            {
                query = query.Where(c => c.THOIGIANXUATPHAT.Value.Year == date.Value.Year
                                      && c.THOIGIANXUATPHAT.Value.Month == date.Value.Month
                                      && c.THOIGIANXUATPHAT.Value.Day == date.Value.Day);
            }

            var rawData = query.ToList();

            var model = rawData.Select(item => new FlightSearchViewModel
            {
                MaChuyenBay = item.MACHUYENBAY,
                TenHang = item.HANGHANGKHONG.TENHANG,
                MaHang = item.HANGHANGKHONG.MAHANG,

                NoiDi = item.TUYENBAY.SANBAY.THANHPHO,
                NoiDen = item.TUYENBAY.SANBAY1.THANHPHO,
                GioDi = item.THOIGIANXUATPHAT.Value,
                GioDen = item.THOIGIANDEN.Value,

                ThoiGianBay = string.Format("{0}h {1}p",
                    (item.THOIGIANDEN.Value - item.THOIGIANXUATPHAT.Value).Hours,
                    (item.THOIGIANDEN.Value - item.THOIGIANXUATPHAT.Value).Minutes),

                GiaThapNhat = db.CHI_TIET_CHUYENBAY
                                .Where(p => p.MACHUYENBAY == item.MACHUYENBAY && p.MALOAIVE.Trim() == "ADT")
                                .OrderBy(p => p.GIA)
                                .Select(p => p.GIA)
                                .FirstOrDefault() 
            }).ToList();

            int pageSize = 5; 
            int pageNumber = (page ?? 1);

            return View(model.ToPagedList(pageNumber, pageSize));
        }

        // GET: Booking/Cart
        [Authorize]
        public ActionResult Cart()
        {
            var userName = User.Identity.Name;
            var taiKhoan = db.TaiKhoans.FirstOrDefault(t => t.TenDangNhap == userName || t.Email == userName);

            if (taiKhoan != null)
            {
                var khachHang = db.KHACHHANGs.FirstOrDefault(k => k.MaTk == taiKhoan.MaTK);

                if (khachHang != null)
                {
                    var cartItems = db.GIOHANGs
                        .Include("CHUYENBAY")
                        .Include("CHUYENBAY.HANGHANGKHONG")
                        .Include("CHUYENBAY.TUYENBAY")
                        .Where(g => g.MAKH == khachHang.MAKH)
                        .OrderByDescending(g => g.NGAYTHEM)
                        .ToList();

                    ViewBag.CartItems = cartItems;
                    ViewBag.MaKH = khachHang.MAKH;
                }
            }

            return View();
        }

        // POST: Booking/ApplyPromotion
        [HttpPost]
        [Authorize]
        public JsonResult ApplyPromotion(string cartItemId, decimal discountAmount)
        {
            try
            {
                var cartItem = db.GIOHANGs.FirstOrDefault(g => g.ID == cartItemId);
                if (cartItem != null && cartItem.GIATIEN.HasValue)
                {
                    var originalPrice = (decimal)cartItem.GIATIEN;
                    var newPrice = originalPrice - discountAmount;

                    if (newPrice < 0) newPrice = 0;
                    return Json(new
                    {
                        success = true,
                        originalPrice = originalPrice,
                        discountAmount = discountAmount,
                        newPrice = newPrice
                    });
                }
                return Json(new { success = false, message = "Không tìm thấy vé trong giỏ hàng" });
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = ex.Message });
            }
        }
        public ActionResult Details(string id)
        {
            if (string.IsNullOrEmpty(id))
            {
                return Content("Lỗi: Mã chuyến bay (id) bị rỗng.");
            }
            var cb = db.CHUYENBAYs.ToList()
                        .FirstOrDefault(x => x.MACHUYENBAY.Trim().Equals(id.Trim(), StringComparison.OrdinalIgnoreCase));

            if (cb == null)
            {
                return Content($"Lỗi 404: Không tìm thấy chuyến bay có mã '{id}' trong Database.");
            }

            var allPrices = db.CHI_TIET_CHUYENBAY.Where(x => x.MACHUYENBAY == cb.MACHUYENBAY).ToList();

            var model = new FlightDetailViewModel
            {
                MaChuyenBay = cb.MACHUYENBAY,
                TenHang = cb.HANGHANGKHONG != null ? cb.HANGHANGKHONG.TENHANG : "Chưa cập nhật",
                MaHang = cb.HANGHANGKHONG != null ? cb.HANGHANGKHONG.MAHANG : "",
                NoiDi = cb.TUYENBAY.SANBAY.THANHPHO,
                NoiDen = cb.TUYENBAY.SANBAY1.THANHPHO,
                GioDi = cb.THOIGIANXUATPHAT.Value,
                GioDen = cb.THOIGIANDEN.Value,
                ThoiGianBay = cb.THOIGIANDUNG + " phút",
                TicketClasses = new List<TicketClassDisplay>()
            };

            var grouped = allPrices.GroupBy(x => x.MAHANGVE);
            foreach (var g in grouped)
            {
                var hangInfo = db.HANGVEs.Find(g.Key);
                if (hangInfo != null)
                {
                    var ticketDisplay = new TicketClassDisplay
                    {
                        MaHangVe = g.Key,
                        TenHangVe = hangInfo.TENHANGVE,
                        TienIch = hangInfo.TIENICH,
                        GiaNguoiLon = g.FirstOrDefault(x => x.MALOAIVE != null && x.MALOAIVE.Trim() == "ADT")?.GIA ?? 0,
                        GiaTreEm = g.FirstOrDefault(x => x.MALOAIVE != null && x.MALOAIVE.Trim() == "CHD")?.GIA ?? 0,
                        GiaEmBe = g.FirstOrDefault(x => x.MALOAIVE != null && x.MALOAIVE.Trim() == "INF")?.GIA ?? 0,
                        SoLuongGheTrong = g.FirstOrDefault(x => x.MALOAIVE != null && x.MALOAIVE.Trim() == "ADT")?.SOLUONGGHE ?? 0
                    };
                    model.TicketClasses.Add(ticketDisplay);
                }
            }

            return View(model);
        }
        [HttpPost]
        [Authorize]
        public ActionResult AddToCart(
    string MaChuyenBay,
    string MaHangVe,
    int slNguoiLon,
    int slTreEm,
    int slEmBe
)
        {
            var userName = User.Identity.Name;
            var taiKhoan = db.TaiKhoans.FirstOrDefault(t => t.TenDangNhap == userName || t.Email == userName);

            if (taiKhoan == null) return RedirectToAction("Login", "Account");
            var khachHang = db.KHACHHANGs.FirstOrDefault(k => k.MaTk == taiKhoan.MaTK);
            if (khachHang == null) return Content("Không tìm thấy khách hàng.");

            string gioHangPrefix = "GH";
            int maxNumericId = 0;
            var lastGioHangId = db.GIOHANGs.OrderByDescending(g => g.ID).Select(g => g.ID).FirstOrDefault();

            if (!string.IsNullOrEmpty(lastGioHangId) && lastGioHangId.Trim().StartsWith(gioHangPrefix))
            {
                string numericPart = lastGioHangId.Substring(gioHangPrefix.Length).Trim();

                int tempId = 0;
                if (int.TryParse(numericPart, out tempId))
                {
                    maxNumericId = tempId;
                }
            }

            int currentGioHangId = maxNumericId;

            string GenerateNewId()
            {
                currentGioHangId++;
                return gioHangPrefix + currentGioHangId.ToString("D7");
            }

            void AddCartItem(string loaiVe, int soLuong)
            {
                if (soLuong <= 0) return;

                var chiTietGia = db.CHI_TIET_CHUYENBAY
                    .Where(x => x.MACHUYENBAY.Trim() == MaChuyenBay.Trim()
                             && x.MAHANGVE.Trim() == MaHangVe.Trim()
                             && x.MALOAIVE.Trim() == loaiVe)
                    .Select(x => new { x.GIA, x.SOLUONGGHE })
                    .FirstOrDefault();

                if (chiTietGia == null) return;

                decimal unitPrice = chiTietGia.GIA;

                var gioHang = new GIOHANG
                {
                    ID = GenerateNewId(),
                    MAKH = khachHang.MAKH.Trim(),
                    MACHUYENBAY = MaChuyenBay.Trim(),
                    LOAIVE = loaiVe,
                    HANGVE = MaHangVe,
                    SOLUONG = soLuong,
                    GIATIEN = unitPrice * soLuong,
                    GIATIEN_SAUKM = unitPrice * soLuong,
                    NGAYTHEM = DateTime.Now.Date
                };

                db.GIOHANGs.Add(gioHang);
            }

            AddCartItem("ADT", slNguoiLon);
            AddCartItem("CHD", slTreEm);
            AddCartItem("INF", slEmBe);

            db.SaveChanges();

            return RedirectToAction("Cart", "Booking");
        }

        // GET: Booking/DeleteFromCart?id=1
        [Authorize]
        public ActionResult DeleteFromCart(string id)
        {
            if (string.IsNullOrEmpty(id))
            {
                TempData["Error"] = "Không tìm thấy mã vé!";
                return RedirectToAction("Cart");
            }

            var userName = User.Identity.Name;
            var taiKhoan = db.TaiKhoans.FirstOrDefault(t => t.TenDangNhap == userName || t.Email == userName);

            if (taiKhoan == null)
                return RedirectToAction("Login", "Account");

            var khachHang = db.KHACHHANGs.FirstOrDefault(k => k.MaTk == taiKhoan.MaTK);
            if (khachHang == null)
            {
                TempData["Error"] = "Không tìm thấy thông tin khách hàng!";
                return RedirectToAction("Cart");
            }

            var cartItem = db.GIOHANGs.FirstOrDefault(g => g.ID.Trim() == id.Trim());

            if (cartItem == null)
            {
                TempData["Error"] = "Không tìm thấy vé trong giỏ hàng!";
                return RedirectToAction("Cart");
            }

            if (cartItem.MAKH.Trim() != khachHang.MAKH.Trim())
            {
                TempData["Error"] = "Bạn không có quyền xóa vé này!";
                return RedirectToAction("Cart");
            }

            db.GIOHANGs.Remove(cartItem);
            db.SaveChanges();

            TempData["Success"] = "Đã xóa vé khỏi giỏ hàng thành công!";
            return RedirectToAction("Cart");
        }

        // POST: Booking/DeleteFromCartAjax
        [HttpPost]
        [Authorize]
        public JsonResult DeleteFromCartAjax(string id)
        {
            try
            {
                if (string.IsNullOrEmpty(id))
                    return Json(new { success = false, message = "Không tìm thấy mã vé!" });

                var userName = User.Identity.Name;
                var taiKhoan = db.TaiKhoans.FirstOrDefault(t => t.TenDangNhap == userName || t.Email == userName);

                if (taiKhoan == null)
                    return Json(new { success = false, message = "Vui lòng đăng nhập!" });

                var khachHang = db.KHACHHANGs.FirstOrDefault(k => k.MaTk == taiKhoan.MaTK);
                if (khachHang == null)
                    return Json(new { success = false, message = "Không tìm thấy thông tin khách hàng!" });

                var cartItem = db.GIOHANGs.FirstOrDefault(g => g.ID.Trim() == id.Trim());

                if (cartItem == null)
                    return Json(new { success = false, message = "Không tìm thấy vé trong giỏ hàng!" });

                if (cartItem.MAKH.Trim() != khachHang.MAKH.Trim())
                    return Json(new { success = false, message = "Bạn không có quyền xóa vé này!" });

                db.GIOHANGs.Remove(cartItem);
                db.SaveChanges();

                var newTotal = db.GIOHANGs
                    .Where(g => g.MAKH == khachHang.MAKH)
                    .ToList()
                    .Sum(g => (g.GIATIEN ?? 0) * (g.SOLUONG ?? 1));

                return Json(new
                {
                    success = true,
                    message = "Đã xóa vé khỏi giỏ hàng!",
                    newTotal = newTotal
                });
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = "Lỗi: " + ex.Message });
            }
        }
        [Authorize]
        public ActionResult History()
        {
            string currentUserName = User.Identity.Name;

            using (var db = new QUANLYBANVEMAYBAY6Entities())
            {

                var khachHang = db.KHACHHANGs.FirstOrDefault(k =>
                                    k.TaiKhoan.TenDangNhap == currentUserName ||
                                    k.TaiKhoan.Email == currentUserName);

                if (khachHang == null)
                {
                    return Content($"LỖI DỮ LIỆU: Bạn đang đăng nhập là '{currentUserName}' (lưu ở LocalDB), " +
                                   $"nhưng tài khoản này chưa có trong bảng KHACHHANG của SQL Server (HOANGKHANG). " +
                                   $"Vui lòng mở SQL Server và thêm tài khoản '{currentUserName}' vào bảng TaiKhoan & KHACHHANG.");
                }

                var listHistory = (from dv in db.DATVEs
                                   join cb in db.CHUYENBAYs on dv.MACHUYENBAY equals cb.MACHUYENBAY
                                   join tb in db.TUYENBAYs on cb.MATUYENBAY equals tb.MATUYENBAY
                                   join sbDi in db.SANBAYs on tb.MASANBAYDI equals sbDi.MASANBAY
                                   join sbDen in db.SANBAYs on tb.MASANBAYVE equals sbDen.MASANBAY
                                   where dv.MAKH == khachHang.MAKH
                                         && dv.TRANGTHAI == "Đã thanh toán"
                                   orderby dv.NGAYDAT descending
                                   select new LichSuDatVeViewModel
                                   {
                                       MaDatVe = dv.MADATVE.Trim(),
                                       NgayDat = dv.NGAYDAT,
                                       MaChuyenBay = dv.MACHUYENBAY,
                                       TenSanBayDi = sbDi.THANHPHO + " (" + sbDi.TENSANBAY + ")",
                                       TenSanBayDen = sbDen.THANHPHO + " (" + sbDen.TENSANBAY + ")",
                                       GioBay = cb.THOIGIANXUATPHAT,
                                       TrangThai = dv.TRANGTHAI,
                                       SoLuongVe = db.VEs.Count(v => v.MADATVE == dv.MADATVE),
                                       TongTien = db.VEs.Where(v => v.MADATVE == dv.MADATVE)
                                                        .Sum(v => (double?)v.GIATIEN) ?? 0
                                   }).ToList();

                return View(listHistory);
            }
        }
        [Authorize]
        public ActionResult BookingDetails(string id)
        {
            if (string.IsNullOrEmpty(id))
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }

            var datVe = db.DATVEs
                .Include("CHUYENBAY")
                .Include("CHUYENBAY.TUYENBAY.SANBAY")
                .Include("CHUYENBAY.TUYENBAY.SANBAY1")
                .Include("CHUYENBAY.HANGHANGKHONG")
                .Include("VEs") 
                .Include("VEs.THANHTOANs")
                .FirstOrDefault(dv => dv.MADATVE.Trim() == id.Trim());

            if (datVe == null)
            {
                return HttpNotFound($"Không tìm thấy chi tiết đặt vé với mã: {id}");
            }

            var userName = User.Identity.Name;
            var khachHang = db.KHACHHANGs.FirstOrDefault(k => k.MaTk == datVe.KHACHHANG.MaTk);

            if (khachHang == null || (khachHang.TaiKhoan.TenDangNhap != userName && khachHang.TaiKhoan.Email != userName))
            {
                return new HttpStatusCodeResult(HttpStatusCode.Forbidden, "Bạn không có quyền truy cập thông tin đặt vé này.");
            }

            var viewModel = new BookingDetailViewModel
            {
                MaDatVe = datVe.MADATVE,
                NgayDat = datVe.NGAYDAT ?? DateTime.MinValue,
                TrangThai = datVe.TRANGTHAI,

                TongTienThanhToan = datVe.VEs.Sum(v => v.GIATIEN) ?? 0,

                PhuongThucThanhToan = datVe.VEs.FirstOrDefault()?.THANHTOANs.FirstOrDefault()?.PHUONGTHUCTT,

                MaChuyenBay = datVe.MACHUYENBAY,
                HangHangKhong = datVe.CHUYENBAY?.HANGHANGKHONG?.TENHANG,
                SanBayDi = datVe.CHUYENBAY?.TUYENBAY?.SANBAY?.TENSANBAY,
                SanBayDen = datVe.CHUYENBAY?.TUYENBAY?.SANBAY1?.TENSANBAY,
                GioKhoiHanh = datVe.CHUYENBAY?.THOIGIANXUATPHAT ?? DateTime.MinValue,
                GioDen = datVe.CHUYENBAY?.THOIGIANDEN ?? DateTime.MinValue,
                ThoiGianBay = datVe.CHUYENBAY != null && datVe.CHUYENBAY.THOIGIANXUATPHAT.HasValue && datVe.CHUYENBAY.THOIGIANDEN.HasValue
                    ? (datVe.CHUYENBAY.THOIGIANDEN.Value - datVe.CHUYENBAY.THOIGIANXUATPHAT.Value).ToString(@"hh\h\ mm\p")
                    : "N/A",

                DanhSachVe = datVe.VEs.Select(v => new TicketViewModel
                {
                    MaVe = v.MAVE,
                    HoTenHanhKhach = "Chưa cập nhật",

                    LoaiVe = v.LOAIVE,
                    GiaTien = v.GIATIEN ?? 0,
                    HangVe = v.HANGVE
                }).ToList()
            };

            return View(viewModel);
        }
        protected override void Dispose(bool disposing)
        {
            if (disposing) db.Dispose();
            base.Dispose(disposing);
        }
    }
}