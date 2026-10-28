using System;
using System.Linq;
using System.Web.Mvc;
using LingoCenter.Models;

namespace LingoCenter.Controllers
{
    [Authorize(Roles = "Administrator")]
    public class CoursesController : BaseController
    {
        public ActionResult Index(int page = 1)
        {
            // Mỗi trang hiển thị 5 khóa học.
            int pageSize = 5;

            // Đếm tổng số khóa học.
            int totalItems = db.Courses.Count();

            // Tính tổng số trang, tối thiểu là 1.
            int totalPages = Math.Max(
                1,
                (int)Math.Ceiling(totalItems / (double)pageSize)
            );

            // Giữ số trang trong phạm vi hợp lệ.
            page = Math.Max(1, Math.Min(page, totalPages));

            // Lấy dữ liệu của trang đang chọn.
            var courses = db.Courses
                .OrderBy(c => c.Id)
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .ToList();

            // Gửi thông tin phân trang sang giao diện.
            ViewBag.CurrentPage = page;
            ViewBag.TotalPages = totalPages;
            ViewBag.TotalItems = totalItems;

            return View(courses);
        }

        public ActionResult Edit(int? id)
        {
            var c = id.HasValue ? db.Courses.Find(id) : new Course
            {
                DurationWeeks = 12,
                TuitionFee = 3000000
            };
            if (c == null)
                return HttpNotFound();
            return View(c);
        }

        [HttpPost, ValidateAntiForgeryToken]
        public ActionResult Edit([Bind(Include = "Id,Name,Level,Description,DurationWeeks,TuitionFee,Syllabus,IsActive")] Course m)
        {
            if (m.TuitionFee != decimal.Truncate(m.TuitionFee))
                ModelState.AddModelError("TuitionFee", "Học phí VND phải là số nguyên.");
            if (!Enum.IsDefined(typeof(CourseLevel), m.Level))
                ModelState.AddModelError("Level", "Trình độ không hợp lệ.");
            if (!ModelState.IsValid)
                return View(m);
            var c = m.Id == 0 ? new Course() : db.Courses.Find(m.Id);
            if (c == null)
                return HttpNotFound();
            c.Name = m.Name;
            c.Level = m.Level;
            c.Description = m.Description;
            c.DurationWeeks = m.DurationWeeks;
            c.TuitionFee = m.TuitionFee;
            c.Syllabus = m.Syllabus;
            c.IsActive = m.IsActive;
            if (m.Id == 0)
                db.Courses.Add(c);
            db.SaveChanges();
            return Success("Đã lưu khóa học.");
        }

        [HttpPost, ValidateAntiForgeryToken]
        public ActionResult Delete(int id)
        {
            var c = db.Courses.Find(id);
            if (c == null)
                return HttpNotFound();
            if (c.Classes.Any())
                return Failure("Khóa học đã có lớp. Hãy tắt hiển thị thay vì xóa.");
            db.Courses.Remove(c);
            db.SaveChanges();
            return Success("Đã xóa khóa học.");
        }
    }
}
