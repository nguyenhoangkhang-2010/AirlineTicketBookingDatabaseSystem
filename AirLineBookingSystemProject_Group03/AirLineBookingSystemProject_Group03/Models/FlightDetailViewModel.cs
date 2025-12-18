using System;
using System.Collections.Generic;

namespace AirLineBookingSystemProject_Group03.Models
{
    public class TicketClassDisplay
    {
        public string MaHangVe { get; set; }
        public string TenHangVe { get; set; }
        public string TienIch { get; set; }
        public int SoLuongGheTrong { get; set; }

        public decimal GiaNguoiLon { get; set; }
        public decimal GiaTreEm { get; set; }
        public decimal GiaEmBe { get; set; }
    }

    public class FlightDetailViewModel
    {
        public string MaChuyenBay { get; set; }
        public string TenHang { get; set; }
        public string MaHang { get; set; }

        public string NoiDi { get; set; } 
        public string NoiDen { get; set; } 

        public DateTime GioDi { get; set; }
        public DateTime GioDen { get; set; }
        public string ThoiGianBay { get; set; }

        public List<TicketClassDisplay> TicketClasses { get; set; }

        public FlightDetailViewModel()
        {
            TicketClasses = new List<TicketClassDisplay>();
        }
    }
}