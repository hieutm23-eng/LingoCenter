using System;
using System.Linq;
using System.Data.Entity;
using LingoCenter.Models;
using LingoCenter.Services;

namespace LingoCenter.Data
{
    // Creates only a missing database. Never drops or overwrites an existing database.
    public class DemoInitializer : CreateDatabaseIfNotExists<CenterDb>
    {
        protected override void Seed(CenterDb db)
        {
            db.Users.Add(new User { FullName = "Nguyễn Minh Anh", Email = "admin@lingocenter.test", Role = UserRole.Administrator, PasswordHash = PasswordService.Hash("Lingo@2026!") });
            string[] names =
            {
                "Sarah Nguyễn",
                "David Trần",
                "Emma Lê",
                "James Phạm",
                "Olivia Võ"
            };
            for (int i = 1; i <= 5; i++)
                db.Users.Add(new User { FullName = names[i - 1], Email = "teacher" + i + "@lingocenter.test", Role = UserRole.Teacher, PasswordHash = PasswordService.Hash("Lingo@2026!") });
            string[] students =
            {
                "Nguyễn Hoàng An",
                "Trần Bảo Anh",
                "Lê Minh Châu",
                "Phạm Gia Hân",
                "Võ Quốc Huy",
                "Đặng Thảo Linh",
                "Bùi Nhật Minh",
                "Đỗ Khánh Ngọc",
                "Hồ Anh Quân",
                "Ngô Phương Thảo",
                "Dương Đức Trung",
                "Lý Ngọc Ánh",
                "Nguyễn Tuấn Kiệt",
                "Trần Hà My",
                "Lê Thanh Nam",
                "Phạm Quỳnh Như",
                "Võ Hải Phong",
                "Đặng Yến Nhi",
                "Bùi Đức Anh",
                "Đỗ Bảo Trân"
            };
            for (int i = 1; i <= 20; i++)
                db.Users.Add(new User { FullName = students[i - 1], Email = "student" + i + "@lingocenter.test", Phone = "09000000" + i.ToString("00"), Role = UserRole.Student, PasswordHash = PasswordService.Hash("Lingo@2026!") });
            db.SaveChanges();
            string[] courses =
            {
                "English Foundations",
                "Everyday English",
                "Confident Communication",
                "English for Success",
                "Advanced Fluency",
                "IELTS Mastery 6.5+",
                "TOEIC Accelerator 750+",
                "IELTS Foundation"
            };
            CourseLevel[] levels =
            {
                CourseLevel.Beginner,
                CourseLevel.Elementary,
                CourseLevel.Intermediate,
                CourseLevel.UpperIntermediate,
                CourseLevel.Advanced,
                CourseLevel.IELTS,
                CourseLevel.TOEIC,
                CourseLevel.IELTS
            };
            for (int i = 0; i < 8; i++)
                db.Courses.Add(new Course { Name = courses[i], Level = levels[i], DurationWeeks = 8 + i, TuitionFee = 2400000 + i * 450000, Description = "Xây dựng sự tự tin qua các tình huống thực tế. Học tương tác, luyện tập theo nhóm nhỏ và nhận phản hồi cá nhân từ giáo viên.", Syllabus = "Module 1: Vocabulary & pronunciation\nModule 2: Listening & speaking\nModule 3: Reading & writing\nModule 4: Real-life projects\nMidterm assessment (40%)\nFinal assessment (60%)" });
            db.SaveChanges();
            var cs = db.Courses.OrderBy(c => c.Id).ToList();
            var ts = db.Users.Where(u => u.Role == UserRole.Teacher).OrderBy(u => u.Id).ToList();
            var ss = db.Users.Where(u => u.Role == UserRole.Student).OrderBy(u => u.Id).ToList();
            for (int i = 0; i < 12; i++)
                db.Classes.Add(new CenterClass { Code = "LC-" + (101 + i), CourseId = cs[i % 8].Id, TeacherId = ts[i % 5].Id, Room = "Studio " + (i + 1).ToString("00"), Capacity = i == 0 ? 3 : 16 + i % 3 * 2, Status = i >= 10 ? ClassStatus.Closed : ClassStatus.Open });
            db.SaveChanges();
            var classes = db.Classes.OrderBy(c => c.Id).ToList();
            // 36 enrollments, 18 distinct students; students 19 and 20 have no history.
            for (int i = 0; i < 36; i++)
            {
                var c = classes[i % 12];
                var status = i % 12 >= 10 ? EnrollmentStatus.Completed : i % 7 == 0 ? EnrollmentStatus.Pending : i % 7 == 1 ? EnrollmentStatus.Confirmed : i % 7 == 2 ? EnrollmentStatus.Withdrawn : EnrollmentStatus.Studying;
                if (i % 12 == 0)
                    status = EnrollmentStatus.Studying;
                db.Enrollments.Add(new Enrollment { StudentId = ss[(i % 12 * 3 + i / 12) % 18].Id, CenterClassId = c.Id, Status = status, EnrolledAt = CenterClock.Now.AddDays(-40 + i / 2), DueDate = CenterClock.Now.Date.AddDays(i % 8 - 3), AgreedFee = c.Course.TuitionFee, Midterm = 6 + i % 4, Final = 6 + (i + 1) % 4, ScoresPublished = status != EnrollmentStatus.Pending });
            }

            db.SaveChanges();
            var es = db.Enrollments.OrderBy(e => e.Id).ToList();
            var monday = CenterClock.Now.Date.AddDays(-((int)CenterClock.Now.DayOfWeek + 6) % 7);
            for (int i = 0; i < 12; i++)
                for (int w = -2; w <= 3; w++)
                {
                    var start = monday.AddDays(w * 7 + i % 6).AddHours(8 + i / 6 * 3);
                    db.Sessions.Add(new ClassSession { CenterClassId = classes[i].Id, StartsAt = start, EndsAt = start.AddHours(2), Topic = "Unit " + (w + 3) + ": Communication workshop" });
                }

            db.SaveChanges();
            foreach (var s in db.Sessions.Where(s => s.StartsAt < monday).ToList())
                foreach (var e in es.Where(e => e.CenterClassId == s.CenterClassId && e.Status != EnrollmentStatus.Withdrawn))
                    db.Attendances.Add(new Attendance { ClassSessionId = s.Id, EnrollmentId = e.Id, Status = e.Id % 5 == 0 ? AttendanceStatus.Absent : AttendanceStatus.Present });
            for (int i = 0; i < 24; i++)
            {
                var e = es[i];
                var status = i % 6 == 0 ? PaymentStatus.Pending : i % 6 == 1 ? PaymentStatus.Rejected : PaymentStatus.Paid;
                db.Payments.Add(new Payment { EnrollmentId = e.Id, Amount = i % 2 == 0 ? e.AgreedFee : e.AgreedFee / 2, Status = status, CreatedAt = CenterClock.Now.AddDays(-60 + i * 2), PaidAt = status == PaymentStatus.Paid ? (DateTime? )CenterClock.Now.AddDays(-60 + i * 2) : null, Reference = "DEMO-TRANSFER-" + (i + 1).ToString("000"), ReceiptNo = "LC-DEMO-" + (i + 1).ToString("0000"), RecordedById = status == PaymentStatus.Pending ? (int? )null : 1 });
            }

            db.SaveChanges();
            // Defense in depth: integrity checks also apply to direct SQL writes.
            db.Database.ExecuteSqlCommand("ALTER TABLE dbo.Payments ADD CONSTRAINT CK_Payment_Amount CHECK (Amount > 0)");
            db.Database.ExecuteSqlCommand("ALTER TABLE dbo.Enrollments ADD CONSTRAINT CK_Scores CHECK ((Midterm IS NULL OR Midterm BETWEEN 0 AND 10) AND (Final IS NULL OR Final BETWEEN 0 AND 10))");
            db.Database.ExecuteSqlCommand("ALTER TABLE dbo.ClassSessions ADD CONSTRAINT CK_Session_Time CHECK (EndsAt > StartsAt)");
            db.Database.ExecuteSqlCommand("ALTER TABLE dbo.CenterClasses ADD CONSTRAINT CK_Capacity CHECK (Capacity > 0)");
        }
    }
}
