using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data.SqlClient;
using System.IO;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace AirLineBookingSystemProject_Group03.Areas.Admin.Controllers
{
    [Authorize(Roles = "Admin")]
    public class BackupController : Controller
    {
        private readonly string dbName = "QUANLYBANVEMAYBAY6";
        private readonly string backupPath =
            @"D:\DaiHoc\HocKy1_Nam2025\HQT_CSDL\BACKUP";

        private readonly string connStr =
            ConfigurationManager.ConnectionStrings["BackupRestoreConnection"].ConnectionString;

        public ActionResult Index()
        {
            if (!Directory.Exists(backupPath))
                Directory.CreateDirectory(backupPath);

            var files = Directory.GetFiles(backupPath, "*.bak")
                                  .Select(Path.GetFileName)
                                  .OrderByDescending(f => f)
                                  .ToList();

            ViewBag.BackupFiles = files;
            return View();
        }


        [HttpPost]
        public ActionResult Backup()
        {
            try
            {
                if (!Directory.Exists(backupPath))
                    Directory.CreateDirectory(backupPath);

                string fileName = $"{dbName}_{DateTime.Now:yyyyMMdd_HHmmss}.bak";
                string fullPath = Path.Combine(backupPath, fileName);

                string sql = $@"
                BACKUP DATABASE [{dbName}]
                TO DISK = N'{fullPath}'
                WITH INIT, STATS = 10";

                using (SqlConnection conn = new SqlConnection(connStr))
                {
                    conn.Open();
                    new SqlCommand(sql, conn).ExecuteNonQuery();
                }

                TempData["Success"] = "Backup dữ liệu thành công!";
            }
            catch (Exception ex)
            {
                TempData["Error"] = ex.Message;
            }

            return RedirectToAction("Index");
        }
        [HttpPost]
        public ActionResult Restore(string fileName)
        {
            try
            {
                string fullPath = Path.Combine(backupPath, fileName);

                string sql = $@"
        ALTER DATABASE [{dbName}] SET SINGLE_USER WITH ROLLBACK IMMEDIATE;

        RESTORE DATABASE [{dbName}]
        FROM DISK = N'{fullPath}'
        WITH REPLACE, RECOVERY;

        ALTER DATABASE [{dbName}] SET MULTI_USER;";

                using (SqlConnection conn = new SqlConnection(connStr))
                {
                    conn.Open();
                    new SqlCommand(sql, conn).ExecuteNonQuery();
                }

                TempData["Success"] = "Phục hồi dữ liệu thành công!";
            }
            catch (Exception ex)
            {
                TempData["Error"] = ex.Message;
            }

            return RedirectToAction("Index");
        }

    }
}