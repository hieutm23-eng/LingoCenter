using System;
using System.Linq;
using System.Web.Mvc;
using LingoCenter.Models;

namespace LingoCenter.Controllers
{
    [Authorize(Roles = "Administrator")]
    public class ClassesController : BaseController
    {
        public ActionResult Index(int page = 1)
        {
            int pageSize = 5;
            int totalItems = db.Classes.Count();

            int totalPages = Math.Max(
                1,
                (int)Math.Ceiling(totalItems / (double)pageSize)
            );

            page = Math.Max(1, Math.Min(page, totalPages));

            var classes = db.Classes
                .OrderBy(c => c.Code)
                .ThenBy(c => c.Id)
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .ToList();

            ViewBag.CurrentPage = page;
            ViewBag.TotalPages = totalPages;
            ViewBag.TotalItems = totalItems;

            return View(classes);
        }

        private void Options()
        {
            ViewBag.Courses = new SelectList(db.Courses.OrderBy(c => c.Name).ToList(), "Id", "Name");
            ViewBag.Teachers = new SelectList(db.Users.Where(u => u.Role == UserRole.Teacher && u.IsActive).ToList(), "Id", "FullName");
        }

        public ActionResult Edit(int? id)
        {
            Options();
            var c = id.HasValue ? db.Classes.Find(id) : new CenterClass
            {
                Capacity = 20
            };
            if (c == null)
                return HttpNotFound();
            return View(c);
        }

        [HttpPost, ValidateAntiForgeryToken]
        public ActionResult Edit([Bind(Include = "Id,Code,CourseId,TeacherId,Room,Capacity,Status")] CenterClass m)
        {
            if (!Enum.IsDefined(typeof(ClassStatus), m.Status) || !db.Courses.Any(c => c.Id == m.CourseId) || !db.Users.Any(u => u.Id == m.TeacherId && u.Role == UserRole.Teacher && u.IsActive))
                ModelState.AddModelError("", "Khóa học, giáo viên hoặc trạng thái không hợp lệ.");
            if (db.Classes.Any(c => c.Id != m.Id && c.Code == m.Code))
                ModelState.AddModelError("Code", "Mã lớp đã tồn tại.");
            using (var tx = db.Database.BeginTransaction(System.Data.IsolationLevel.Serializable))
            {
                db.Database.SqlQuery<int>("SELECT Id FROM dbo.CenterClasses WITH (UPDLOCK,HOLDLOCK) WHERE Id=@p0", m.Id).ToList();
                var c = m.Id == 0 ? new CenterClass() : db.Classes.Find(m.Id);
                if (c == null)
                    return HttpNotFound();
                if (m.Capacity < c.Occupied)
                    ModelState.AddModelError("Capacity", "Sĩ số tối đa không được thấp hơn số đã đăng ký.");
                if (c.Enrollments.Any() && c.CourseId != m.CourseId)
                    ModelState.AddModelError("CourseId", "Không thể đổi khóa học khi đã có đăng ký.");
                var other = db.Sessions.Where(s => s.CenterClassId != m.Id && (s.CenterClass.TeacherId == m.TeacherId || s.CenterClass.Room == m.Room)).ToList();
                if (c.Sessions.Any(a => other.Any(b => a.StartsAt < b.EndsAt && b.StartsAt < a.EndsAt)))
                    ModelState.AddModelError("", "Giáo viên hoặc phòng bị trùng lịch.");
                if (!ModelState.IsValid)
                {
                    Options();
                    return View(m);
                }

                c.Code = m.Code;
                c.CourseId = m.CourseId;
                c.TeacherId = m.TeacherId;
                c.Room = m.Room;
                c.Capacity = m.Capacity;
                c.Status = m.Status;
                if (m.Id == 0)
                    db.Classes.Add(c);
                db.SaveChanges();
                tx.Commit();
                return Success("Đã lưu lớp học.");
            }
        }

        public ActionResult Sessions(int id)
        {
            var c = db.Classes.Find(id);
            if (c == null)
                return HttpNotFound();
            return View(c);
        }

        [HttpPost, ValidateAntiForgeryToken]
        public ActionResult AddSession(int classId, DateTime startsAt, DateTime endsAt, string topic)
        {
            var c = db.Classes.Find(classId);
            if (c == null)
                return HttpNotFound();
            using (var tx = db.Database.BeginTransaction(System.Data.IsolationLevel.Serializable))
            {
                if (!ModelState.IsValid || endsAt <= startsAt || endsAt - startsAt > TimeSpan.FromHours(8) || string.IsNullOrWhiteSpace(topic) || topic.Length > 200)
                    TempData["Error"] = "Buổi học cần chủ đề và thời gian hợp lệ, tối đa 8 giờ.";
                else
                {
                    var studentIds = c.Enrollments.Where(e => e.Status != EnrollmentStatus.Withdrawn && e.Status != EnrollmentStatus.Completed).Select(e => e.StudentId).ToList();
                    var conflict = db.Sessions.Where(s => s.StartsAt < endsAt && startsAt < s.EndsAt).ToList().Any(s => s.CenterClassId == classId || s.CenterClass.TeacherId == c.TeacherId || s.CenterClass.Room == c.Room || s.CenterClass.Enrollments.Any(e => studentIds.Contains(e.StudentId) && e.Status != EnrollmentStatus.Withdrawn && e.Status != EnrollmentStatus.Completed));
                    if (conflict)
                        TempData["Error"] = "Trùng lịch lớp, giáo viên, phòng hoặc học viên đã đăng ký.";
                    else
                    {
                        db.Sessions.Add(new ClassSession { CenterClassId = classId, StartsAt = startsAt, EndsAt = endsAt, Topic = topic.Trim() });
                        db.SaveChanges();
                        tx.Commit();
                        TempData["Success"] = "Đã thêm buổi học.";
                    }
                }
            }

            return RedirectToAction("Sessions", new { id = classId });
        }

        [HttpPost, ValidateAntiForgeryToken]
        public ActionResult DeleteSession(int id)
        {
            var s = db.Sessions.Find(id);
            if (s == null)
                return HttpNotFound();
            var cid = s.CenterClassId;
            if (db.Attendances.Any(a => a.ClassSessionId == id))
                TempData["Error"] = "Buổi học đã có điểm danh, không thể xóa.";
            else
            {
                db.Sessions.Remove(s);
                db.SaveChanges();
            }

            return RedirectToAction("Sessions", new { id = cid });
        }

        [HttpPost, ValidateAntiForgeryToken]
        public ActionResult Delete(int id)
        {
            var c = db.Classes.Find(id);
            if (c == null)
                return HttpNotFound();
            if (c.Enrollments.Any() || c.Sessions.Any())
                return Failure("Lớp đã có dữ liệu. Hãy đóng lớp để giữ lịch sử.");
            db.Classes.Remove(c);
            db.SaveChanges();
            return Success("Đã xóa lớp.");
        }
    }
}
