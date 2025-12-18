using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace AirLineBookingSystemProject_Group03.Models
{
    public class LichSuDatVeViewModel
    {
        public string MaDatVe { get; set; }
        public DateTime? NgayDat { get; set; }
        public string MaChuyenBay { get; set; }
        public string TenSanBayDi { get; set; }
        public string TenSanBayDen { get; set; }
        public DateTime? GioBay { get; set; }
        public int SoLuongVe { get; set; }
        public double TongTien { get; set; }
        public string TrangThai { get; set; }
    }
}