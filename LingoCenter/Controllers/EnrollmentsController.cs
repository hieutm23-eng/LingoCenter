using System;
using System.Linq;
using System.Web.Mvc;
using LingoCenter.Models;
using LingoCenter.Services;

namespace LingoCenter.Controllers
{
    [Authorize(Roles = "Administrator")]
    public class EnrollmentsController : BaseController
    {
        public ActionResult Index(
    EnrollmentStatus? status,
    string q,
    int page = 1)
        {
            var query = db.Enrollments.AsQueryable();

            if (status.HasValue)
            {
                query = query.Where(e => e.Status == status.Value);
            }

            if (!string.IsNullOrWhiteSpace(q))
            {
                q = q.Trim();

                query = query.Where(e =>
                    e.Student.FullName.Contains(q) ||
                    e.CenterClass.Code.Contains(q));
            }

            ViewBag.Students = new SelectList(
                db.Users
                    .Where(u => u.Role == UserRole.Student && u.IsActive)
                    .ToList(),
                "Id",
                "FullName"
            );

            ViewBag.Classes = new SelectList(
                db.Classes
                    .Where(c => c.Status == ClassStatus.Open)
                    .ToList(),
                "Id",
                "Code"
            );

            var enrollments = Paginate(
                query
                    .OrderByDescending(e => e.EnrolledAt)
                    .ThenByDescending(e => e.Id),
                page
            );

            return View(enrollments);
        }

        [HttpPost, ValidateAntiForgeryToken]
        public ActionResult Create(int studentId, int classId)
        {
            try
            {
                new EnrollmentService(db).Enroll(studentId, classId);
                return Success("Đã thêm đăng ký.");
            }
            catch (InvalidOperationException ex)
            {
                return Failure(ex.Message);
            }
        }

        [HttpPost, ValidateAntiForgeryToken]
        public ActionResult Status(int id, EnrollmentStatus status)
        {
            try
            {
                new EnrollmentService(db).ChangeStatus(id, status);
                return Success("Đã cập nhật trạng thái.");
            }
            catch (InvalidOperationException ex)
            {
                return Failure(ex.Message);
            }
        }
    }
}
