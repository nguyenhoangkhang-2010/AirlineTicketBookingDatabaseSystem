using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace AirLineBookingSystemProject_Group03.Models
{
    public class BookingDetailViewModel
    {
        public string MaDatVe { get; set; }
        public DateTime NgayDat { get; set; }
        public string TrangThai { get; set; }
        public double TongTienThanhToan { get; set; }
        public string PhuongThucThanhToan { get; set; } 
        public string MaChuyenBay { get; set; }
        public string HangHangKhong { get; set; }
        public string SanBayDi { get; set; }
        public string SanBayDen { get; set; }
        public DateTime GioKhoiHanh { get; set; }
        public DateTime GioDen { get; set; }
        public string ThoiGianBay { get; set; }

        public List<TicketViewModel> DanhSachVe { get; set; }
    }

    public class TicketViewModel
    {
        public string MaVe { get; set; }
        public string HoTenHanhKhach { get; set; }
        public string LoaiVe { get; set; } 
        public double GiaTien { get; set; }
        public string HangVe { get; set; }
    }
}