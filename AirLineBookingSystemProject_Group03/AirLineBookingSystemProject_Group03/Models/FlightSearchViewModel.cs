using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace AirLineBookingSystemProject_Group03.Models
{
	public class FlightSearchViewModel
	{
        public string MaChuyenBay { get; set; }
        public string TenHang { get; set; }
        public string MaHang { get; set; }
        public string LogoHang { get; set; }
        public string NoiDi { get; set; }
        public string NoiDen { get; set; }
        public DateTime GioDi { get; set; }
        public DateTime GioDen { get; set; }
        public string ThoiGianBay { get; set; }
        public decimal GiaThapNhat { get; set; }
    }
}