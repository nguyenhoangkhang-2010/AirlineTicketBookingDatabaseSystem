using AirLineBookingSystemProject_Group03.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using System.Data.Entity;


namespace AirLineBookingSystemProject_Group03.Controllers
{
    public class CartController : Controller
    {
        private readonly QUANLYBANVEMAYBAY6Entities db = new QUANLYBANVEMAYBAY6Entities();

        // GET: Cart
        public ActionResult Index()
        {
            var maKH = Session["MaKH"] as string;
            if (string.IsNullOrEmpty(maKH))
            {
                TempData["Error"] = "Vui lòng đăng nhập để xem giỏ hàng!";
                return RedirectToAction("Login", "Account");
            }

            var gioHang = db.GIOHANGs
                .Include(g => g.CHUYENBAY)
                .Include(g => g.CHUYENBAY.TUYENBAY)
                .Include(g => g.CHUYENBAY.TUYENBAY.SANBAY)
                .Include(g => g.CHUYENBAY.TUYENBAY.SANBAY1)
                .Include(g => g.CHUYENBAY.HANGHANGKHONG)
                .Where(g => g.MAKH.Trim() == maKH.Trim())
                .ToList();

            decimal tongTienGoc = gioHang.Sum(g => (g.GIATIEN ?? 0) * (g.SOLUONG ?? 1));
            decimal tongTienSauKM = gioHang.Sum(g => (g.GIATIEN_SAUKM ?? g.GIATIEN ?? 0) * (g.SOLUONG ?? 1));
            decimal tienGiam = tongTienGoc - tongTienSauKM;

            ViewBag.TongTienGoc = tongTienGoc;
            ViewBag.TienGiam = tienGiam;
            ViewBag.TongTienSauKM = tongTienSauKM;

            return View(gioHang);
        }

        [HttpGet]
        public ActionResult AddToCart()
        {
            TempData["Error"] = "Vui lòng chọn chuyến bay trước!";
            return RedirectToAction("Index", "Flight");
        }

        private void AddOrUpdateCartItem(string maKH, string maChuyenBay, string maHangVe,
            string loaiVe, int soLuong, decimal gia, Func<string> generateId)
        {
            var existing = db.GIOHANGs.FirstOrDefault(g =>
                g.MAKH.Trim() == maKH.Trim() &&
                g.MACHUYENBAY.Trim() == maChuyenBay.Trim() &&
                g.HANGVE.Trim() == maHangVe.Trim() &&
                g.LOAIVE.Trim() == loaiVe);

            if (existing != null)
            {
                existing.SOLUONG = (existing.SOLUONG ?? 0) + soLuong;
            }
            else
            {
                db.GIOHANGs.Add(new GIOHANG
                {
                    ID = generateId(),
                    MAKH = maKH,
                    MACHUYENBAY = maChuyenBay,
                    LOAIVE = loaiVe,
                    HANGVE = maHangVe,
                    SOLUONG = soLuong,
                    GIATIEN = gia,
                    NGAYTHEM = DateTime.Now
                });
            }
        }

        [HttpPost]
        public ActionResult RemoveItem(string id)
        {
            var maKH = Session["MaKH"] as string;
            if (string.IsNullOrEmpty(maKH))
            {
                return Json(new { success = false, message = "Chưa đăng nhập!" });
            }

            var item = db.GIOHANGs.FirstOrDefault(g => g.ID.Trim() == id.Trim() && g.MAKH.Trim() == maKH.Trim());
            if (item != null)
            {
                db.GIOHANGs.Remove(item);
                db.SaveChanges();

                var cart = db.GIOHANGs.Where(g => g.MAKH.Trim() == maKH.Trim()).ToList();
                decimal total = cart.Sum(g => (g.GIATIEN ?? 0) * (g.SOLUONG ?? 1));
                int count = cart.Sum(g => g.SOLUONG ?? 0);

                return Json(new { success = true, total = total, count = count });
            }

            return Json(new { success = false, message = "Không tìm thấy!" });
        }

        [HttpPost]
        public ActionResult UpdateQuantity(string id, int quantity)
        {
            var maKH = Session["MaKH"] as string;
            if (string.IsNullOrEmpty(maKH))
            {
                return Json(new { success = false, message = "Chưa đăng nhập!" });
            }

            if (quantity <= 0)
            {
                return Json(new { success = false, message = "Số lượng không hợp lệ!" });
            }

            var item = db.GIOHANGs.FirstOrDefault(g => g.ID.Trim() == id.Trim() && g.MAKH.Trim() == maKH.Trim());
            if (item != null)
            {
                item.SOLUONG = quantity;
                db.SaveChanges();

                decimal itemTotal = (item.GIATIEN ?? 0) * quantity;
                var cart = db.GIOHANGs.Where(g => g.MAKH.Trim() == maKH.Trim()).ToList();
                decimal cartTotal = cart.Sum(g => (g.GIATIEN ?? 0) * (g.SOLUONG ?? 1));
                int count = cart.Sum(g => g.SOLUONG ?? 0);

                return Json(new { success = true, itemTotal = itemTotal, cartTotal = cartTotal, count = count });
            }

            return Json(new { success = false, message = "Không tìm thấy!" });
        }

        [HttpGet]
        public ActionResult GetCartCount()
        {
            var maKH = Session["MaKH"] as string;
            int count = 0;

            if (!string.IsNullOrEmpty(maKH))
            {
                count = db.GIOHANGs
                    .Where(g => g.MAKH.Trim() == maKH.Trim())
                    .Sum(g => (int?)g.SOLUONG) ?? 0;
            }

            return Json(new { count = count }, JsonRequestBehavior.AllowGet);
        }

        [HttpPost]
        public ActionResult ClearCart()
        {
            var maKH = Session["MaKH"] as string;
            if (string.IsNullOrEmpty(maKH))
            {
                return Json(new { success = false });
            }

            var items = db.GIOHANGs.Where(g => g.MAKH.Trim() == maKH.Trim()).ToList();
            db.GIOHANGs.RemoveRange(items);
            db.SaveChanges();

            return Json(new { success = true });
        }

        [HttpPost]
        public ActionResult ApplyPromotion(string promoCode)
        {
            var maKH = Session["MaKH"] as string;
            if (string.IsNullOrEmpty(maKH))
            {
                return Json(new { success = false, message = "Chưa đăng nhập!" });
            }

            if (string.IsNullOrEmpty(promoCode))
            {
                return Json(new { success = false, message = "Vui lòng nhập mã khuyến mãi!" });
            }

            var promo = db.KHUYENMAIs.FirstOrDefault(k =>
                k.ID.Trim() == promoCode.Trim() &&
                k.TRANGTHAI == "Hoạt động" &&
                k.NGAYBATDAU <= DateTime.Today &&
                k.NGAYKETTHUC >= DateTime.Today);

            if (promo == null)
            {
                return Json(new { success = false, message = "Mã khuyến mãi không hợp lệ hoặc đã hết hạn!" });
            }

            var cartItems = db.GIOHANGs.Where(g => g.MAKH.Trim() == maKH.Trim()).ToList();
            foreach (var item in cartItems)
            {
                item.MAKHUYENMAI = promo.ID;
                decimal giaGoc = item.GIATIEN ?? 0;
                decimal giamGia = giaGoc * (decimal)(promo.GIATRI ?? 0);

                if (promo.GIATRITOIDA.HasValue && giamGia > promo.GIATRITOIDA.Value)
                {
                    giamGia = promo.GIATRITOIDA.Value;
                }

                item.GIATIEN_SAUKM = giaGoc - giamGia;
            }
            db.SaveChanges();

            decimal tongGoc = cartItems.Sum(g => (g.GIATIEN ?? 0) * (g.SOLUONG ?? 1));
            decimal tongSauKM = cartItems.Sum(g => (g.GIATIEN_SAUKM ?? g.GIATIEN ?? 0) * (g.SOLUONG ?? 1));

            return Json(new
            {
                success = true,
                message = $"Áp dụng mã {promoCode} thành công! Giảm {(promo.GIATRI ?? 0) * 100}%",
                tongGoc = tongGoc,
                tongSauKM = tongSauKM,
                tienGiam = tongGoc - tongSauKM
            });
        }

        protected override void Dispose(bool disposing)
        {
            if (disposing) db.Dispose();
            base.Dispose(disposing);
        }
    }
}