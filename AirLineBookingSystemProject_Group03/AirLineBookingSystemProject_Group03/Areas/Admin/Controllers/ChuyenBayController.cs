using AirLineBookingSystemProject_Group03.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Web;
using System.Web.Mvc;

namespace AirLineBookingSystemProject_Group03.Areas.Admin.Controllers
{
    public class ChuyenBayController : Controller
    {
        private QUANLYBANVEMAYBAY6Entities db = new QUANLYBANVEMAYBAY6Entities();

        public ActionResult Index()
        {
            ViewBag.Active = "ManageFlights";

            var flights = db.CHUYENBAYs
                            .Include("HANGHANGKHONG")
                            .Include("TUYENBAY.SANBAY")
                            .Include("TUYENBAY.SANBAY1")
                            .OrderByDescending(x => x.THOIGIANXUATPHAT)
                            .ToList();
            return View(flights);
        }

        public ActionResult Create()
        {
            ViewBag.Active = "CreateFlight";

            var model = new CreateFlightViewModel();
            model.PriceList = new List<FlightPriceDetail>();

            var dsHangVe = db.HANGVEs.ToList();
            var dsLoaiVe = db.LOAIVEs.ToList();
            var listTuyenBay = db.TUYENBAYs.Select(t => new
            {
                Value = t.MATUYENBAY,
                Text = t.SANBAY.THANHPHO + " (" + t.SANBAY.MASANBAY + ") ➝ " + t.SANBAY1.THANHPHO + " (" + t.SANBAY1.MASANBAY + ")"
            }).ToList();

            foreach (var hang in dsHangVe)
            {
                foreach (var loai in dsLoaiVe)
                {
                    model.PriceList.Add(new FlightPriceDetail
                    {
                        MaHangVe = hang.MAHANGVE,
                        TenHangVe = hang.TENHANGVE,
                        MaLoaiVe = loai.MALOAIVE,
                        TenLoaiVe = loai.TENLOAIVE,
                        Gia = 0,
                        SoLuongGhe = (loai.MALOAIVE == "ADT") ? 50 : 0
                    });
                }
            }

            ViewBag.MaTuyenBay = new SelectList(listTuyenBay, "Value", "Text", model.MaTuyenBay);
            ViewBag.MaHang = new SelectList(db.HANGHANGKHONGs, "MAHANG", "TENHANG");

            return View(model);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Create(CreateFlightViewModel model, string strNgayDi, string strGioDi)
        {
            ViewBag.Active = "CreateFlight";
            DateTime ngayKhoiHanh;
            TimeSpan gioKhoiHanh;
            bool isDateValid = DateTime.TryParse(strNgayDi, out ngayKhoiHanh);
            bool isTimeValid = TimeSpan.TryParse(strGioDi, out gioKhoiHanh);

            if (!isDateValid || !isTimeValid)
            {
                ModelState.AddModelError("", "Vui lòng nhập đầy đủ Ngày và Giờ khởi hành hợp lệ.");
            }
            else if (ngayKhoiHanh.Year < 1900)
            {
                ModelState.AddModelError("", "Năm khởi hành không hợp lệ (Phải > 1900).");
            }
            else
            {
                model.NgayDi = ngayKhoiHanh;
                model.GioDi = gioKhoiHanh;
            }

            if (ModelState.IsValid)
            {
                if (db.CHUYENBAYs.Any(x => x.MACHUYENBAY == model.MaChuyenBay))
                {
                    ModelState.AddModelError("MaChuyenBay", "Mã chuyến bay này đã tồn tại!");
                }
                else
                {
                    try
                    {
                        var cb = new CHUYENBAY();
                        cb.MACHUYENBAY = model.MaChuyenBay;
                        cb.MATUYENBAY = model.MaTuyenBay;
                        cb.MAHANG = model.MaHang;

                        DateTime fullDate = ngayKhoiHanh.Date + gioKhoiHanh;
                        cb.THOIGIANXUATPHAT = fullDate;
                        cb.THOIGIANDUNG = model.ThoiGianBay;

                        cb.THOIGIANDEN = fullDate.AddMinutes(model.ThoiGianBay);

                        cb.SOLUONGGHE = model.PriceList.Where(x => x.MaLoaiVe == "ADT").Sum(x => x.SoLuongGhe);

                        db.CHUYENBAYs.Add(cb);

                        foreach (var item in model.PriceList)
                        {
                            var ct = new CHI_TIET_CHUYENBAY();
                            ct.MACHUYENBAY = cb.MACHUYENBAY;
                            ct.MAHANGVE = item.MaHangVe;
                            ct.MALOAIVE = item.MaLoaiVe;
                            ct.GIA = item.Gia;
                            ct.SOLUONGGHE = (item.MaLoaiVe == "ADT") ? item.SoLuongGhe : 0;
                            db.CHI_TIET_CHUYENBAY.Add(ct);
                        }

                        db.SaveChanges();

                        TempData["AdminMessage"] = "Đã thêm chuyến bay " + cb.MACHUYENBAY + " thành công!";
                        TempData["AlertType"] = "success";

                        return RedirectToAction("Index");
                    }
                    catch (Exception ex)
                    {
                        var msg = ex.InnerException != null ? ex.InnerException.Message : ex.Message;
                        TempData["AdminMessage"] = "Lỗi hệ thống: " + msg;
                        TempData["AlertType"] = "error";
                        ModelState.AddModelError("", "Lỗi lưu Database: " + msg);
                    }
                }
            }

            var listTuyenBay = db.TUYENBAYs.Select(t => new
            {
                Value = t.MATUYENBAY,
                Text = t.SANBAY.THANHPHO + " (" + t.SANBAY.MASANBAY + ") ➝ " + t.SANBAY1.THANHPHO + " (" + t.SANBAY1.MASANBAY + ")"
            }).ToList();

            ViewBag.MaTuyenBay = new SelectList(listTuyenBay, "Value", "Text", model.MaTuyenBay);
            ViewBag.MaHang = new SelectList(db.HANGHANGKHONGs, "MAHANG", "TENHANG", model.MaHang);

            return View(model);
        }

        public ActionResult Edit(string id)
        {
            ViewBag.Active = "ManageFlights";
            if (string.IsNullOrEmpty(id)) return new HttpStatusCodeResult(HttpStatusCode.BadRequest);

            var cb = db.CHUYENBAYs.ToList().FirstOrDefault(x => x.MACHUYENBAY.Trim() == id.Trim());
            if (cb == null) return HttpNotFound();

            var priceList = cb.CHI_TIET_CHUYENBAY
                              .Select(ct => new FlightPriceDetail
                              {
                                  MaHangVe = ct.MAHANGVE,
                                  TenHangVe = ct.HANGVE.TENHANGVE,
                                  MaLoaiVe = ct.MALOAIVE,
                                  TenLoaiVe = ct.LOAIVE.TENLOAIVE,
                                  Gia = ct.GIA,
                                  SoLuongGhe = ct.SOLUONGGHE.GetValueOrDefault()
                              })
                              .OrderBy(x => x.MaHangVe)
                              .ThenBy(x => x.MaLoaiVe)
                              .ToList();

            var viewModel = new FlightDetailsEditViewModel
            {
                ChuyenBay = cb,
                PriceList = priceList
            };

            var listTuyenBay = db.TUYENBAYs.Select(t => new
            {
                Value = t.MATUYENBAY,
                Text = t.SANBAY.THANHPHO + " (" + t.SANBAY.MASANBAY + ") ➝ " + t.SANBAY1.THANHPHO + " (" + t.SANBAY1.MASANBAY + ")"
            }).ToList();

            ViewBag.MaTuyenBay = new SelectList(listTuyenBay, "Value", "Text", cb.MATUYENBAY);
            ViewBag.MaHang = new SelectList(db.HANGHANGKHONGs, "MAHANG", "TENHANG", cb.MAHANG);

            return View(viewModel);
        }

        // 3. SỬA - POST (Lưu thay đổi vào DB)
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Edit(FlightDetailsEditViewModel model)
        {
            ViewBag.Active = "ManageFlights";
            if (ModelState.IsValid)
            {
                var existingFlight = db.CHUYENBAYs.Find(model.ChuyenBay.MACHUYENBAY);

                if (existingFlight != null)
                {
                    existingFlight.MATUYENBAY = model.ChuyenBay.MATUYENBAY;
                    existingFlight.MAHANG = model.ChuyenBay.MAHANG;
                    existingFlight.THOIGIANXUATPHAT = model.ChuyenBay.THOIGIANXUATPHAT;
                    existingFlight.THOIGIANDUNG = model.ChuyenBay.THOIGIANDUNG;

                    if (model.ChuyenBay.THOIGIANXUATPHAT.HasValue && model.ChuyenBay.THOIGIANDUNG.HasValue)
                    {
                        existingFlight.THOIGIANDEN = model.ChuyenBay.THOIGIANXUATPHAT.Value.AddMinutes((double)model.ChuyenBay.THOIGIANDUNG);
                    }

                    int totalSeats = 0;

                    if (model.PriceList != null)
                    {
                        foreach (var item in model.PriceList)
                        {
                            var ct = db.CHI_TIET_CHUYENBAY
                                        .FirstOrDefault(x => x.MACHUYENBAY == existingFlight.MACHUYENBAY &&
                                                             x.MAHANGVE == item.MaHangVe &&
                                                             x.MALOAIVE == item.MaLoaiVe);
                            if (ct != null)
                            {
                                ct.GIA = item.Gia;

                                if (item.MaLoaiVe == "ADT")
                                {
                                    ct.SOLUONGGHE = item.SoLuongGhe;
                                    totalSeats += item.SoLuongGhe;
                                }
                            }
                        }
                    }

                    existingFlight.SOLUONGGHE = totalSeats;

                    db.SaveChanges();

                    TempData["AdminMessage"] = $"Cập nhật chuyến bay {existingFlight.MACHUYENBAY} thành công!";
                    TempData["AlertType"] = "success";

                    return RedirectToAction("Index");
                }
            }

            var listTuyenBay = db.TUYENBAYs.Select(t => new
            {
                Value = t.MATUYENBAY,
                Text = t.SANBAY.THANHPHO + " (" + t.SANBAY.MASANBAY + ") ➝ " + t.SANBAY1.THANHPHO + " (" + t.SANBAY1.MASANBAY + ")"
            }).ToList();

            ViewBag.MaTuyenBay = new SelectList(listTuyenBay, "Value", "Text", model.ChuyenBay.MATUYENBAY);
            ViewBag.MaHang = new SelectList(db.HANGHANGKHONGs, "MAHANG", "TENHANG", model.ChuyenBay.MAHANG);

            return View(model);
        }

        // 4. CHI TIẾT (Details)
        public ActionResult Details(string id)
        {
            ViewBag.Active = "ManageFlights";
            if (string.IsNullOrEmpty(id)) return new HttpStatusCodeResult(HttpStatusCode.BadRequest);

            var cb = db.CHUYENBAYs.ToList().FirstOrDefault(x => x.MACHUYENBAY.Trim() == id.Trim());
            if (cb == null) return HttpNotFound();

            var priceList = cb.CHI_TIET_CHUYENBAY
                              .Select(ct => new FlightPriceDetail
                              {
                                  MaHangVe = ct.MAHANGVE,
                                  TenHangVe = ct.HANGVE.TENHANGVE,
                                  MaLoaiVe = ct.MALOAIVE,
                                  TenLoaiVe = ct.LOAIVE.TENLOAIVE,
                                  Gia = ct.GIA,
                                  SoLuongGhe = ct.SOLUONGGHE ?? 0
                              })
                              .OrderBy(x => x.MaHangVe)
                              .ThenBy(x => x.MaLoaiVe)
                              .ToList();

            var viewModel = new FlightDetailsEditViewModel
            {
                ChuyenBay = cb,
                PriceList = priceList
            };

            return View(viewModel);
        }

        public ActionResult Delete(string id)
        {
            ViewBag.Active = "ManageFlights";
            if (string.IsNullOrEmpty(id)) return new HttpStatusCodeResult(HttpStatusCode.BadRequest);

            var model = db.CHUYENBAYs.ToList().FirstOrDefault(x => x.MACHUYENBAY.Trim() == id.Trim());
            if (model == null) return HttpNotFound();

            return View(model);
        }

        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public ActionResult DeleteConfirmed(string id)
        {
            ViewBag.Active = "ManageFlights";
            var model = db.CHUYENBAYs.ToList().FirstOrDefault(x => x.MACHUYENBAY.Trim() == id.Trim());
            if (model == null)
            {
                TempData["AdminMessage"] = "Không tìm thấy chuyến bay!";
                TempData["AlertType"] = "error";
                return RedirectToAction("Index");
            }

            try
            {
                if (db.DATVEs.Any(v => v.MACHUYENBAY == model.MACHUYENBAY))
                {
                    TempData["AdminMessage"] = "Không thể xóa! Chuyến bay này đã có khách đặt vé.";
                    TempData["AlertType"] = "error";
                    return RedirectToAction("Index");
                }
                var gioHangItems = db.GIOHANGs.Where(g => g.MACHUYENBAY == model.MACHUYENBAY).ToList();
                if (gioHangItems.Any())
                {
                    db.GIOHANGs.RemoveRange(gioHangItems);
                }
                var chiTietGia = db.CHI_TIET_CHUYENBAY.Where(x => x.MACHUYENBAY == model.MACHUYENBAY).ToList();
                if (chiTietGia.Any())
                {
                    db.CHI_TIET_CHUYENBAY.RemoveRange(chiTietGia);
                }
                db.CHUYENBAYs.Remove(model);

                db.SaveChanges();

                TempData["AdminMessage"] = "Đã xóa chuyến bay và dữ liệu liên quan thành công!";
                TempData["AlertType"] = "success";
            }
            catch (Exception ex)
            {
                var msg = ex.InnerException != null ? ex.InnerException.Message : ex.Message;
                TempData["AdminMessage"] = "Lỗi khi xóa: " + msg;
                TempData["AlertType"] = "error";
            }

            return RedirectToAction("Index");
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
}