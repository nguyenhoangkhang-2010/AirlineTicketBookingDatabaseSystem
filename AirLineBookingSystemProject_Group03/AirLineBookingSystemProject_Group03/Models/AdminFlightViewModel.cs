using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Web;

namespace AirLineBookingSystemProject_Group03.Models
{
    public class FlightPriceDetail
    {
        public string MaHangVe { get; set; }
        public string TenHangVe { get; set; }
        public string MaLoaiVe { get; set; }
        public string TenLoaiVe { get; set; }
        public decimal Gia { get; set; }
        public int SoLuongGhe { get; set; }
    }

    public class CreateFlightViewModel
    {
        [Required(ErrorMessage = "Vui lòng nhập mã chuyến bay")]
        public string MaChuyenBay { get; set; }

        [Required(ErrorMessage = "Vui lòng chọn tuyến bay")]
        public string MaTuyenBay { get; set; }

        [Required(ErrorMessage = "Vui lòng chọn hãng")]
        public string MaHang { get; set; }

        public DateTime NgayDi { get; set; }

        public TimeSpan GioDi { get; set; }

        [Required]
        public int ThoiGianBay { get; set; } // Phút

        public List<FlightPriceDetail> PriceList { get; set; }
    }

    public class FlightDetailsEditViewModel
    {
        public CHUYENBAY ChuyenBay { get; set; }

        public List<FlightPriceDetail> PriceList { get; set; }
    }
}