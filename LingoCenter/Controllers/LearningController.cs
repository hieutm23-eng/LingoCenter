using System;
using System.Linq;
using System.Web.Mvc;
using LingoCenter.Models;
using LingoCenter.Services;

namespace LingoCenter.Controllers
{
    [Authorize(Roles = "Student")]
    public class LearningController : BaseController
    {
        public ActionResult Index(int page = 1)
        {
            int studentId = UserId;

            var enrollments = Paginate(
                db.Enrollments
                    .Where(e => e.StudentId == studentId)
                    .OrderByDescending(e => e.EnrolledAt)
                    .ThenByDescending(e => e.Id),
                page
            );

            return View(enrollments);
        }

        [HttpPost, ValidateAntiForgeryToken]
        public ActionResult Enroll(int classId)
        {
            try
            {
                new EnrollmentService(db).Enroll(UserId, classId);
                return Success("Đã gửi đăng ký. Vui lòng thanh toán học phí để trung tâm xác nhận.");
            }
            catch (InvalidOperationException ex)
            {
                return Failure(ex.Message);
            }
        }

        [HttpPost, ValidateAntiForgeryToken]
        public ActionResult Withdraw(int id)
        {
            using (var tx = db.Database.BeginTransaction(System.Data.IsolationLevel.Serializable))
            {
                db.Database.SqlQuery<int>("SELECT Id FROM dbo.Enrollments WITH (UPDLOCK,HOLDLOCK) WHERE Id=@p0", id).ToList();
                var e = db.Enrollments.Find(id);
                if (e == null || e.StudentId != UserId)
                    return HttpNotFound();
                if (e.Status != EnrollmentStatus.Pending || e.Payments.Any(p => p.Status != PaymentStatus.Rejected))
                    return Failure("Chỉ hủy yêu cầu đang chờ chưa thanh toán. Các trường hợp khác vui lòng liên hệ trung tâm.");
                e.Status = EnrollmentStatus.Withdrawn;
                db.SaveChanges();
                tx.Commit();
                return Success("Đã rút đăng ký.");
            }
        }

        public ActionResult Results(int page = 1)
        {
            int studentId = UserId;

            var enrollments = Paginate(
                db.Enrollments
                    .Where(e => e.StudentId == studentId)
                    .OrderByDescending(e => e.EnrolledAt)
                    .ThenByDescending(e => e.Id),
                page
            );

            var enrollmentIds = enrollments
                .Select(e => e.Id)
                .ToList();

            ViewBag.Attendance = db.Attendances
                .Where(a =>
                    a.Enrollment.StudentId == studentId &&
                    enrollmentIds.Contains(a.EnrollmentId))
                .ToList();

            return View(enrollments);
        }
    }

    [Authorize]
    public class TimetableController : BaseController
    {
        public ActionResult Index(DateTime? week)
        {
            var date = (week ?? CenterClock.Now).Date;
            var start = date.AddDays(-((int)date.DayOfWeek + 6) % 7);
            var end = start.AddDays(7);
            var query = db.Sessions.Where(s => s.StartsAt >= start && s.StartsAt < end);
            if (User.IsInRole("Student"))
            {
                var ids = db.Enrollments.Where(e => e.StudentId == UserId && (e.Status == EnrollmentStatus.Confirmed || e.Status == EnrollmentStatus.Studying)).Select(e => e.CenterClassId);
                query = query.Where(s => ids.Contains(s.CenterClassId));
            }
            else if (User.IsInRole("Teacher"))
                query = query.Where(s => s.CenterClass.TeacherId == UserId);
            return View(new TimetableVm { Week = start, Sessions = query.OrderBy(s => s.StartsAt).ToList() });
        }
    }

    [Authorize]
    public class NotificationsController : BaseController
    {
        public ActionResult Index(int page = 1)
        {
            new NotificationService(db).Refresh(UserId);

            int userId = UserId;

            var notifications = Paginate(
                db.Notifications
                    .Where(n => n.UserId == userId)
                    .OrderByDescending(n => n.CreatedAt)
                    .ThenByDescending(n => n.Id),
                page
            );

            return View(notifications);
        }

        [HttpPost, ValidateAntiForgeryToken]
        public ActionResult Read(int id)
        {
            var n = db.Notifications.SingleOrDefault(x => x.Id == id && x.UserId == UserId);
            if (n == null)
                return HttpNotFound();
            n.IsRead = true;
            db.SaveChanges();
            return RedirectToAction("Index");
        }
    }
}
