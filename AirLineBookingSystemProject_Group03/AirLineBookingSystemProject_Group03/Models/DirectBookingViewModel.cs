using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Web;

namespace AirLineBookingSystemProject_Group03.Models
{
    public class TicketCount
    {
        public string MaLoaiVe { get; set; }
        public int SoLuong { get; set; }
        public decimal GiaVe { get; set; }
    }

    public class DirectBookingViewModel
    {
        [Required]
        public string MaChuyenBay { get; set; }
        [Required]
        public string MaHangVe { get; set; }

        public string HoTen { get; set; }
        public string SoDienThoai { get; set; }
        public string Email { get; set; }
        public string PhuongThucTT { get; set; }
        public List<TicketCount> TicketDetails { get; set; }
    }
}