using AirLineBookingSystemProject_Group03.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace AirLineBookingSystemProject_Group03.Controllers
{
    public class PromotionController : Controller
    {
        private readonly QUANLYBANVEMAYBAY6Entities db = new QUANLYBANVEMAYBAY6Entities();

        // GET: Promotion - Hiển thị danh sách mã khuyến mãi
        public ActionResult Index()
        {
            // Lấy ngày hiện tại
            var today = DateTime.Today;

            // Lấy các mã khuyến mãi còn hiệu lực từ database
            var promotions = db.KHUYENMAIs
                .Where(km => km.TRANGTHAI == "Hoạt động"
                          && km.NGAYBATDAU <= today
                          && km.NGAYKETTHUC >= today
                          && (km.SOLUONG - km.DASDUNG) > 0) // Còn mã để sử dụng
                .OrderByDescending(km => km.GIATRI) // Sắp xếp theo giá trị giảm cao nhất
                .ToList();

            // Lấy mã khuyến mãi đã áp dụng (nếu có)
            ViewBag.AppliedPromotion = Session["AppliedPromotion"] as AppliedPromotionInfo;

            return View(promotions);
        }

        // POST: Áp dụng mã khuyến mãi vào giỏ hàng
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult ApplyPromotion(string maKhuyenMai)
        {
            // Kiểm tra đăng nhập
            var maKH = Session["MaKH"] as string;
            if (string.IsNullOrEmpty(maKH))
            {
                TempData["Error"] = "Vui lòng đăng nhập để sử dụng mã khuyến mãi!";
                return RedirectToAction("Login", "Account");
            }

            if (string.IsNullOrEmpty(maKhuyenMai))
            {
                TempData["Error"] = "Vui lòng nhập hoặc chọn mã khuyến mãi!";
                return RedirectToAction("Index");
            }

            // Tìm mã khuyến mãi trong database
            var today = DateTime.Today;
            var promotion = db.KHUYENMAIs
                .FirstOrDefault(km => km.ID.Trim() == maKhuyenMai.Trim()
                                   && km.TRANGTHAI == "Hoạt động"
                                   && km.NGAYBATDAU <= today
                                   && km.NGAYKETTHUC >= today
                                   && (km.SOLUONG - km.DASDUNG) > 0);

            if (promotion == null)
            {
                TempData["Error"] = "Mã khuyến mãi không hợp lệ, đã hết hạn hoặc đã hết lượt sử dụng!";
                return RedirectToAction("Index");
            }

            // Kiểm tra giỏ hàng có sản phẩm không
            var gioHang = db.GIOHANGs.Where(g => g.MAKH.Trim() == maKH.Trim()).ToList();
            if (!gioHang.Any())
            {
                TempData["Error"] = "Giỏ hàng trống! Vui lòng thêm vé vào giỏ hàng trước.";
                return RedirectToAction("Index");
            }

            // Cập nhật mã khuyến mãi vào TẤT CẢ các item trong giỏ hàng của khách hàng
            foreach (var item in gioHang)
            {
                item.MAKHUYENMAI = promotion.ID.Trim();

                // Tính giá sau khuyến mãi
                if (item.GIATIEN.HasValue)
                {
                    decimal giaGoc = item.GIATIEN.Value;
                    decimal soTienGiam = 0;

                    if (promotion.LOAIKHUYENMAI == "PhanTram")
                    {
                        soTienGiam = giaGoc * (decimal)(promotion.GIATRI ?? 0);

                        // Giới hạn số tiền giảm tối đa
                        if (promotion.GIATRITOIDA.HasValue && soTienGiam > promotion.GIATRITOIDA.Value)
                        {
                            soTienGiam = promotion.GIATRITOIDA.Value;
                        }
                    }
                    else // SoTien
                    {
                        soTienGiam = (decimal)(promotion.GIATRI ?? 0);
                    }

                    item.GIATIEN_SAUKM = giaGoc - soTienGiam;

                    // Đảm bảo giá không âm
                    if (item.GIATIEN_SAUKM < 0)
                        item.GIATIEN_SAUKM = 0;
                }
            }

            // Lưu thay đổi vào database
            db.SaveChanges();

            // Lưu thông tin khuyến mãi vào Session để hiển thị
            Session["AppliedPromotion"] = new AppliedPromotionInfo
            {
                MaKhuyenMai = promotion.ID.Trim(),
                MoTa = promotion.MOTA,
                GiaTri = promotion.GIATRI ?? 0,
                LoaiKhuyenMai = promotion.LOAIKHUYENMAI,
                GiaTriToiDa = promotion.GIATRITOIDA
            };

            TempData["Success"] = $"Đã áp dụng mã khuyến mãi '{promotion.ID.Trim()}' - {promotion.MOTA} thành công!";

            // Redirect đến giỏ hàng
            return RedirectToAction("Index", "Cart");
        }

        // POST: Áp dụng mã từ input nhập tay
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult ApplyPromotionCode(string promoCode)
        {
            return ApplyPromotion(promoCode);
        }

        // GET: Xóa mã khuyến mãi đã áp dụng
        public ActionResult RemovePromotion()
        {
            var maKH = Session["MaKH"] as string;
            if (string.IsNullOrEmpty(maKH))
            {
                return RedirectToAction("Login", "Account");
            }

            // Xóa mã khuyến mãi khỏi giỏ hàng
            var gioHang = db.GIOHANGs.Where(g => g.MAKH.Trim() == maKH.Trim()).ToList();
            foreach (var item in gioHang)
            {
                item.MAKHUYENMAI = null;
                item.GIATIEN_SAUKM = item.GIATIEN; // Reset về giá gốc
            }
            db.SaveChanges();

            // Xóa session
            Session["AppliedPromotion"] = null;

            TempData["Success"] = "Đã xóa mã khuyến mãi!";
            return RedirectToAction("Index", "Cart");
        }

        // GET: Kiểm tra mã khuyến mãi (AJAX)
        [HttpGet]
        public JsonResult CheckPromotion(string maKhuyenMai)
        {
            var today = DateTime.Today;
            var promotion = db.KHUYENMAIs
                .FirstOrDefault(km => km.ID.Trim() == maKhuyenMai.Trim()
                                   && km.TRANGTHAI == "Hoạt động"
                                   && km.NGAYBATDAU <= today
                                   && km.NGAYKETTHUC >= today
                                   && (km.SOLUONG - km.DASDUNG) > 0);

            if (promotion != null)
            {
                string giaTriText = promotion.LOAIKHUYENMAI == "PhanTram"
                    ? $"{(promotion.GIATRI ?? 0) * 100}%"
                    : $"{promotion.GIATRI:N0}đ";

                return Json(new
                {
                    success = true,
                    maKhuyenMai = promotion.ID.Trim(),
                    moTa = promotion.MOTA,
                    giaTri = promotion.GIATRI,
                    giaTriText = giaTriText,
                    loaiKhuyenMai = promotion.LOAIKHUYENMAI,
                    giaTriToiDa = promotion.GIATRITOIDA,
                    conLai = promotion.SOLUONG - promotion.DASDUNG
                }, JsonRequestBehavior.AllowGet);
            }

            return Json(new
            {
                success = false,
                message = "Mã khuyến mãi không hợp lệ hoặc đã hết hạn!"
            }, JsonRequestBehavior.AllowGet);
        }

        // GET: Lấy danh sách khuyến mãi (AJAX cho dropdown)
        [HttpGet]
        public JsonResult GetActivePromotions()
        {
            var today = DateTime.Today;
            var promotions = db.KHUYENMAIs
                .Where(km => km.TRANGTHAI == "Hoạt động"
                          && km.NGAYBATDAU <= today
                          && km.NGAYKETTHUC >= today
                          && (km.SOLUONG - km.DASDUNG) > 0)
                .Select(km => new
                {
                    id = km.ID.Trim(),
                    moTa = km.MOTA,
                    giaTri = km.GIATRI,
                    loaiKM = km.LOAIKHUYENMAI,
                    giaTriToiDa = km.GIATRITOIDA,
                    conLai = km.SOLUONG - km.DASDUNG
                })
                .ToList();

            return Json(promotions, JsonRequestBehavior.AllowGet);
        }

        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                db.Dispose();
            }
            base.Dispose(disposing);
        }
    }

    // Class lưu thông tin khuyến mãi đã áp dụng
    [Serializable]
    public class AppliedPromotionInfo
    {
        public string MaKhuyenMai { get; set; }
        public string MoTa { get; set; }
        public double GiaTri { get; set; }
        public string LoaiKhuyenMai { get; set; }
        public decimal? GiaTriToiDa { get; set; }
    }
}