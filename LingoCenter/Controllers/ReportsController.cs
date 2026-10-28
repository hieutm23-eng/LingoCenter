using System;
using System.Linq;
using System.Collections.Generic;
using System.Web.Mvc;
using LingoCenter.Services;

namespace LingoCenter.Controllers
{
    [Authorize(Roles = "Administrator")]
    public class ReportsController : BaseController
    {
        public ActionResult Index()
        {
            ViewBag.Classes = new SelectList(db.Classes.OrderBy(c => c.Code).ToList(), "Id", "Code");
            return View();
        }

        public ActionResult Export(string kind, int? classId, DateTime? from, DateTime? to)
        {
            if (from.HasValue && to.HasValue && from > to)
                return Failure("Khoảng ngày không hợp lệ.");
            var rows = new List<string[]>();
            var until = to.HasValue ? to.Value.Date.AddDays(1) : (DateTime? )null;
            if (kind == "students")
            {
                rows.Add(new[] { "Mã đăng ký", "Học viên", "Email", "Lớp", "Trạng thái", "Học phí VND", "Đã thu VND", "Còn lại VND" });
                var es = db.Enrollments.AsQueryable();
                if (classId.HasValue)
                    es = es.Where(e => e.CenterClassId == classId);
                if (from.HasValue)
                    es = es.Where(e => e.EnrolledAt >= from);
                if (until.HasValue)
                    es = es.Where(e => e.EnrolledAt < until);
                foreach (var e in es.ToList())
                    rows.Add(new[] { e.Id.ToString(), e.Student.FullName, e.Student.Email, e.CenterClass.Code, e.Status.ToString(), e.AgreedFee.ToString("0"), e.Paid.ToString("0"), e.Balance.ToString("0") });
            }
            else if (kind == "attendance")
            {
                rows.Add(new[] { "Lớp", "Buổi học", "Chủ đề", "Học viên", "Điểm danh" });
                var ats = db.Attendances.AsQueryable();
                if (classId.HasValue)
                    ats = ats.Where(a => a.ClassSession.CenterClassId == classId);
                if (from.HasValue)
                    ats = ats.Where(a => a.ClassSession.StartsAt >= from);
                if (until.HasValue)
                    ats = ats.Where(a => a.ClassSession.StartsAt < until);
                foreach (var a in ats.ToList())
                    rows.Add(new[] { a.ClassSession.CenterClass.Code, a.ClassSession.StartsAt.ToString("yyyy-MM-dd HH:mm"), a.ClassSession.Topic, a.Enrollment.Student.FullName, a.Status.ToString() });
            }
            else if (kind == "payments")
            {
                rows.Add(new[] { "Mã biên lai", "Học viên", "Lớp", "Số tiền VND", "Trạng thái", "Ngày tạo", "Ngày thu", "Tham chiếu" });
                var ps = db.Payments.AsQueryable();
                if (classId.HasValue)
                    ps = ps.Where(p => p.Enrollment.CenterClassId == classId);
                if (from.HasValue)
                    ps = ps.Where(p => p.CreatedAt >= from);
                if (until.HasValue)
                    ps = ps.Where(p => p.CreatedAt < until);
                foreach (var p in ps.ToList())
                    rows.Add(new[] { p.ReceiptNo, p.Enrollment.Student.FullName, p.Enrollment.CenterClass.Code, p.Amount.ToString("0"), p.Status.ToString(), p.CreatedAt.ToString("yyyy-MM-dd"), p.PaidAt.HasValue ? p.PaidAt.Value.ToString("yyyy-MM-dd") : "", p.Reference });
            }
            else
                return new HttpStatusCodeResult(400);
            return File(ExcelExport.Create(rows), "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet", "LingoCenter-" + kind + "-" + CenterClock.Now.ToString("yyyyMMdd") + ".xlsx");
        }
    }
}
