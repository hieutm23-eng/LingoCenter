using System;
using System.Linq;
using System.Web.Mvc;
using LingoCenter.Models;
using LingoCenter.Services;

namespace LingoCenter.Controllers
{
    [Authorize(Roles = "Student,Administrator")]
    public class PaymentsController : BaseController
    {
        public ActionResult Index(int page = 1)
        {
            var enrollments = db.Enrollments.AsQueryable();
            var payments = db.Payments.AsQueryable();

            if (!User.IsInRole("Administrator"))
            {
                int studentId = UserId;

                enrollments = enrollments.Where(e =>
                    e.StudentId == studentId);

                payments = payments.Where(p =>
                    p.Enrollment.StudentId == studentId);
            }

            // Giữ đầy đủ đăng ký để tính công nợ và chọn khoản cần thu.
            ViewBag.Enrollments = enrollments.ToList();

            // Tính trên toàn bộ giao dịch được phép xem,
            // trước khi chia trang.
            ViewBag.TotalPaid = payments
                .Where(p => p.Status == PaymentStatus.Paid)
                .Select(p => (decimal?)p.Amount)
                .Sum() ?? 0m;

            ViewBag.PendingCount = payments.Count(p =>
                p.Status == PaymentStatus.Pending);

            var paymentPage = Paginate(
                payments
                    .OrderByDescending(p => p.CreatedAt)
                    .ThenByDescending(p => p.Id),
                page
            );

            return View(paymentPage);
        }

        [Authorize(Roles = "Student"), HttpPost, ValidateAntiForgeryToken]
        public ActionResult Submit(int enrollmentId, decimal amount, string reference)
        {
            if (!ModelState.IsValid)
                return Failure("Dữ liệu thanh toán không hợp lệ.");
            try
            {
                new PaymentService(db).Submit(enrollmentId, UserId, amount, reference);
                return Success("Đã gửi thông tin chuyển khoản. Trung tâm sẽ kiểm tra trước khi xác nhận.");
            }
            catch (InvalidOperationException ex)
            {
                return Failure(ex.Message);
            }
        }

        [Authorize(Roles = "Administrator"), HttpPost, ValidateAntiForgeryToken]
        public ActionResult Record(int enrollmentId, decimal amount, string reference)
        {
            if (!ModelState.IsValid)
                return Failure("Dữ liệu thanh toán không hợp lệ.");
            try
            {
                new PaymentService(db).Record(enrollmentId, amount, reference, UserId);
                return Success("Đã ghi nhận thu học phí.");
            }
            catch (InvalidOperationException ex)
            {
                return Failure(ex.Message);
            }
        }

        [Authorize(Roles = "Administrator"), HttpPost, ValidateAntiForgeryToken]
        public ActionResult Review(int id, bool approve)
        {
            try
            {
                new PaymentService(db).Review(id, approve, UserId);
                return Success("Đã xử lý giao dịch.");
            }
            catch (InvalidOperationException ex)
            {
                return Failure(ex.Message);
            }
        }

        public ActionResult Receipt(int id)
        {
            var p = db.Payments.Find(id);
            if (p == null || p.Status != PaymentStatus.Paid || (!User.IsInRole("Administrator") && p.Enrollment.StudentId != UserId))
                return HttpNotFound();
            return View(p);
        }
    }
}
