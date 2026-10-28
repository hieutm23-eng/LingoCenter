using System;
using System.Data;
using System.Linq;
using LingoCenter.Models;

namespace LingoCenter.Services
{
    // Serializable transactions and class locks serialize competing requests for the final seat.
    public class EnrollmentService
    {
        private readonly CenterDb db;
        public EnrollmentService(CenterDb db)
        {
            this.db = db;
        }

        public Enrollment Enroll(int studentId, int classId)
        {
            using (var tx = db.Database.BeginTransaction(IsolationLevel.Serializable))
            {
                db.Database.SqlQuery<int>("SELECT Id FROM dbo.CenterClasses WITH (UPDLOCK,HOLDLOCK) WHERE Id=@p0", classId).ToList();
                var c = db.Classes.Find(classId);
                var s = db.Users.Find(studentId);
                if (c == null || s == null || s.Role != UserRole.Student || !s.IsActive)
                    throw new InvalidOperationException("Học viên hoặc lớp không hợp lệ.");
                if (c.Status != ClassStatus.Open || !c.Course.IsActive)
                    throw new InvalidOperationException("Lớp đã đóng đăng ký.");
                if (db.Enrollments.Any(x => x.StudentId == studentId && x.CenterClassId == classId))
                    throw new InvalidOperationException("Bạn đã có đăng ký ở lớp này. Vui lòng liên hệ trung tâm nếu cần đăng ký lại.");
                if (db.Enrollments.Count(x => x.CenterClassId == classId && x.Status != EnrollmentStatus.Withdrawn) >= c.Capacity)
                    throw new InvalidOperationException("Lớp đã đủ sĩ số.");
                var mine = db.Enrollments.Where(x => x.StudentId == studentId && x.Status != EnrollmentStatus.Withdrawn && x.Status != EnrollmentStatus.Completed).Select(x => x.CenterClassId).ToList();
                var other = db.Sessions.Where(x => mine.Contains(x.CenterClassId)).ToList();
                if (c.Sessions.Any(a => other.Any(b => a.StartsAt < b.EndsAt && b.StartsAt < a.EndsAt)))
                    throw new InvalidOperationException("Lịch học bị trùng với lớp bạn đã đăng ký.");
                var e = new Enrollment
                {
                    StudentId = studentId,
                    CenterClassId = classId,
                    Status = EnrollmentStatus.Pending,
                    AgreedFee = c.Course.TuitionFee,
                    EnrolledAt = CenterClock.Now,
                    DueDate = CenterClock.Now.Date.AddDays(7)
                };
                db.Enrollments.Add(e);
                db.SaveChanges();
                tx.Commit();
                return e;
            }
        }

        public void ChangeStatus(int id, EnrollmentStatus next)
        {
            using (var tx = db.Database.BeginTransaction(IsolationLevel.Serializable))
            {
                db.Database.SqlQuery<int>("SELECT Id FROM dbo.Enrollments WITH (UPDLOCK,HOLDLOCK) WHERE Id=@p0", id).ToList();
                var e = db.Enrollments.Find(id);
                if (e == null)
                    throw new InvalidOperationException("Không tìm thấy đăng ký.");
                bool valid = (e.Status == EnrollmentStatus.Pending && (next == EnrollmentStatus.Confirmed || next == EnrollmentStatus.Withdrawn)) || (e.Status == EnrollmentStatus.Confirmed && (next == EnrollmentStatus.Studying || next == EnrollmentStatus.Withdrawn)) || (e.Status == EnrollmentStatus.Studying && (next == EnrollmentStatus.Completed || next == EnrollmentStatus.Withdrawn));
                if (!valid)
                    throw new InvalidOperationException("Chuyển trạng thái không hợp lệ.");
                if (next == EnrollmentStatus.Completed && (!e.ScoresPublished || !e.Result.HasValue))
                    throw new InvalidOperationException("Cần công bố đủ điểm trước khi hoàn thành.");
                e.Status = next;
                db.SaveChanges();
                tx.Commit();
            }
        }
    }
}
