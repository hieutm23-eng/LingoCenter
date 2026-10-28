using System;
using System.Data;
using System.Linq;
using LingoCenter.Models;

namespace LingoCenter.Services
{
    public class PaymentService
    {
        private readonly CenterDb db;
        public PaymentService(CenterDb db)
        {
            this.db = db;
        }

        public void Submit(int enrollmentId, int studentId, decimal amount, string reference)
        {
            using (var tx = db.Database.BeginTransaction(IsolationLevel.Serializable))
            {
                Lock(enrollmentId);
                var e = db.Enrollments.Find(enrollmentId);
                if (e == null || e.StudentId != studentId || e.Status == EnrollmentStatus.Withdrawn)
                    throw new InvalidOperationException("Đăng ký không hợp lệ.");
                Validate(amount, reference);
                var reserved = db.Payments.Where(x => x.EnrollmentId == enrollmentId && x.Status != PaymentStatus.Rejected).Select(x => (decimal? )x.Amount).Sum() ?? 0;
                if (amount > e.AgreedFee - reserved)
                    throw new InvalidOperationException("Số tiền vượt dư nợ còn lại (đã trừ giao dịch chờ xác nhận).");
                db.Payments.Add(New(enrollmentId, amount, reference, PaymentStatus.Pending, null));
                db.SaveChanges();
                tx.Commit();
            }
        }

        public void Record(int enrollmentId, decimal amount, string reference, int adminId)
        {
            using (var tx = db.Database.BeginTransaction(IsolationLevel.Serializable))
            {
                Lock(enrollmentId);
                var e = db.Enrollments.Find(enrollmentId);
                if (e == null || e.Status == EnrollmentStatus.Withdrawn)
                    throw new InvalidOperationException("Đăng ký không hợp lệ.");
                Validate(amount, reference);
                var reserved = db.Payments.Where(x => x.EnrollmentId == enrollmentId && x.Status != PaymentStatus.Rejected).Select(x => (decimal? )x.Amount).Sum() ?? 0;
                if (amount > e.AgreedFee - reserved)
                    throw new InvalidOperationException("Vượt công nợ. Hãy xử lý giao dịch đang chờ trước.");
                db.Payments.Add(New(enrollmentId, amount, reference, PaymentStatus.Paid, adminId));
                db.SaveChanges();
                tx.Commit();
            }
        }

        public void Review(int id, bool approve, int adminId)
        {
            using (var tx = db.Database.BeginTransaction(IsolationLevel.Serializable))
            {
                var enrollmentId = db.Payments.Where(x => x.Id == id).Select(x => (int? )x.EnrollmentId).FirstOrDefault();
                if (!enrollmentId.HasValue)
                    throw new InvalidOperationException("Không tìm thấy giao dịch.");
                Lock(enrollmentId.Value);
                var p = db.Payments.Find(id);
                if (p.Status != PaymentStatus.Pending)
                    throw new InvalidOperationException("Giao dịch đã được xử lý.");
                if (approve && (p.Enrollment.Status == EnrollmentStatus.Withdrawn || p.Amount > p.Enrollment.Balance))
                    throw new InvalidOperationException("Không thể xác nhận: đăng ký đã rút hoặc vượt dư nợ.");
                p.Status = approve ? PaymentStatus.Paid : PaymentStatus.Rejected;
                p.PaidAt = approve ? (DateTime? )CenterClock.Now : null;
                p.RecordedById = adminId;
                db.SaveChanges();
                tx.Commit();
            }
        }

        private void Lock(int id)
        {
            db.Database.SqlQuery<int>("SELECT Id FROM dbo.Enrollments WITH (UPDLOCK,HOLDLOCK) WHERE Id=@p0", id).ToList();
        }

        private static void Validate(decimal amount, string reference)
        {
            if (amount <= 0 || amount != decimal.Truncate(amount) || string.IsNullOrWhiteSpace(reference) || reference.Length > 120)
                throw new InvalidOperationException("Nhập số tiền VND nguyên dương và mã giao dịch tối đa 120 ký tự.");
        }

        private static Payment New(int id, decimal amount, string reference, PaymentStatus status, int? admin)
        {
            return new Payment
            {
                EnrollmentId = id,
                Amount = amount,
                Reference = reference.Trim(),
                Status = status,
                CreatedAt = CenterClock.Now,
                PaidAt = status == PaymentStatus.Paid ? (DateTime? )CenterClock.Now : null,
                RecordedById = admin,
                ReceiptNo = "LC-" + Guid.NewGuid().ToString("N").Substring(0, 16).ToUpperInvariant()
            };
        }
    }
}
