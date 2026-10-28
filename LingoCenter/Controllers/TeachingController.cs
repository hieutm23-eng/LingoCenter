using System;
using System.Linq;
using System.Collections.Generic;
using System.Web.Mvc;
using LingoCenter.Models;

namespace LingoCenter.Controllers
{
    [Authorize(Roles = "Teacher,Administrator")]
    public class TeachingController : BaseController
    {
        private bool Allowed(CenterClass c)
        {
            return c != null && (User.IsInRole("Administrator") || c.TeacherId == UserId);
        }

        public ActionResult Index(int page = 1)
        {
            bool admin = User.IsInRole("Administrator");
            int teacherId = UserId;

            var classes = Paginate(
                db.Classes
                    .Where(c => admin || c.TeacherId == teacherId)
                    .OrderBy(c => c.Code)
                    .ThenBy(c => c.Id),
                page
            );

            return View(classes);
        }

        public ActionResult Class(int id)
        {
            var c = db.Classes.Find(id);
            if (!Allowed(c))
                return HttpNotFound();
            return View(c);
        }

        public ActionResult Attendance(int id)
        {
            var s = db.Sessions.Find(id);
            if (s == null || !Allowed(s.CenterClass))
                return HttpNotFound();
            ViewBag.Existing = db.Attendances.Where(a => a.ClassSessionId == id).ToList();
            return View(s);
        }

        [HttpPost, ValidateAntiForgeryToken]
        public ActionResult Attendance(int id, List<AttendanceRow> rows)
        {
            var s = db.Sessions.Find(id);
            if (s == null || !Allowed(s.CenterClass))
                return HttpNotFound();
            if (s.StartsAt > LingoCenter.Services.CenterClock.Now)
                return Failure("Chưa đến buổi học, không thể điểm danh.");
            var eligible = s.CenterClass.Enrollments.Where(e => e.Status == EnrollmentStatus.Confirmed || e.Status == EnrollmentStatus.Studying || e.Status == EnrollmentStatus.Completed).Select(e => e.Id).ToList();
            if (!ModelState.IsValid || rows == null || rows.Select(r => r.EnrollmentId).Distinct().Count() != rows.Count || rows.Any(r => !eligible.Contains(r.EnrollmentId) || !Enum.IsDefined(typeof(AttendanceStatus), r.Status)))
                return new HttpStatusCodeResult(400, "Invalid attendance data");
            foreach (var row in rows)
            {
                var a = db.Attendances.SingleOrDefault(x => x.ClassSessionId == id && x.EnrollmentId == row.EnrollmentId);
                if (a == null)
                {
                    a = new Attendance
                    {
                        ClassSessionId = id,
                        EnrollmentId = row.EnrollmentId
                    };
                    db.Attendances.Add(a);
                }

                a.Status = row.Status;
            }

            db.SaveChanges();
            TempData["Success"] = "Đã lưu điểm danh.";
            return RedirectToAction("Class", new { id = s.CenterClassId });
        }

        public ActionResult Scores(int id)
        {
            var c = db.Classes.Find(id);
            if (!Allowed(c))
                return HttpNotFound();
            return View(c);
        }

        [HttpPost, ValidateAntiForgeryToken]
        public ActionResult Scores(int id, List<ScoreRow> rows, bool publish = false)
        {
            var c = db.Classes.Find(id);
            if (!Allowed(c))
                return HttpNotFound();
            var eligible = c.Enrollments.Where(e => e.Status == EnrollmentStatus.Studying || e.Status == EnrollmentStatus.Completed).Select(e => e.Id).ToList();
            if (!ModelState.IsValid || rows == null || rows.Select(r => r.EnrollmentId).Distinct().Count() != rows.Count || rows.Any(r => !eligible.Contains(r.EnrollmentId) || publish && (!r.Midterm.HasValue || !r.Final.HasValue)))
            {
                TempData["Error"] = "Điểm phải từ 0 đến 10. Khi công bố cần nhập đủ điểm giữa kỳ và cuối kỳ.";
                return RedirectToAction("Scores", new { id });
            }

            foreach (var r in rows)
            {
                var e = db.Enrollments.Find(r.EnrollmentId);
                e.Midterm = r.Midterm;
                e.Final = r.Final;
                e.ScoresPublished = publish;
            }

            db.SaveChanges();
            TempData["Success"] = publish ? "Đã công bố kết quả." : "Đã lưu bản nháp điểm.";
            return RedirectToAction("Scores", new { id });
        }
    }
}
