using System.Linq;
using System.Web.Mvc;
using AirLineBookingSystemProject_Group03.Models;

namespace AirLineBookingSystemProject_Group03.Areas.Admin.Controllers
{
    [Authorize(Roles = "Admin")] 
    public class DashboardController : Controller
    {
        private QUANLYBANVEMAYBAY6Entities db = new QUANLYBANVEMAYBAY6Entities();

        // GET: Admin/Dashboard
        public ActionResult Index()
        {
            ViewBag.TongChuyenBay = db.CHUYENBAYs.Count();

            var doanhThu = db.THANHTOANs.Sum(t => (decimal?)t.TONGTIEN) ?? 0;
            ViewBag.DoanhThu = doanhThu;

            return View();
        }
    }
}