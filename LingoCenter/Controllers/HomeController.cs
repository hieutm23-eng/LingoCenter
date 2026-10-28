using System;
using System.Linq;
using System.Web.Mvc;
using System.Data.Entity;
using LingoCenter.Models;
using LingoCenter.Services;

namespace LingoCenter.Controllers
{
    public class HomeController : BaseController
    {
        public ActionResult Index()
        {
            if (User.IsInRole("Administrator"))
                return RedirectToAction("Index", "Dashboard");
            if (User.IsInRole("Teacher"))
                return RedirectToAction("Index", "Teaching");
            if (User.IsInRole("Student"))
                return RedirectToAction("Index", "Learning");
            return View(db.Courses.Where(c => c.IsActive).OrderBy(c => c.Id).Take(6).ToList());
        }

        public ActionResult Courses(string q, CourseLevel? level, int page = 1)
        {
            var query = db.Courses.Where(c => c.IsActive);
            if (!string.IsNullOrWhiteSpace(q))
                query = query.Where(c => c.Name.Contains(q));
            if (level.HasValue)
                query = query.Where(c => c.Level == level.Value);
            var pages = Math.Max(1, (int)Math.Ceiling(query.Count() / 6.0));
            page = Math.Max(1, Math.Min(page, pages));
            ViewBag.Pages = pages;
            ViewBag.Page = page;
            ViewBag.Query = q;
            ViewBag.Level = level;
            return View(query.OrderBy(c => c.Id).Skip((page - 1) * 6).Take(6).ToList());
        }

        public ActionResult Course(int id)
        {
            var c = db.Courses.Find(id);
            if (c == null || !c.IsActive)
                return HttpNotFound();
            return View(c);
        }
    }

    [Authorize(Roles = "Administrator")]
    public class DashboardController : BaseController
    {
        public ActionResult Index(DateTime? from, DateTime? to)
        {
            var start = (from ?? CenterClock.Now.Date.AddMonths(-5)).Date;
            var end = (to ?? CenterClock.Now.Date).Date.AddDays(1);
            if (end <= start)
                return Failure("Khoảng ngày không hợp lệ.");
            var paid = db.Payments.Where(p => p.Status == PaymentStatus.Paid && p.PaidAt >= start && p.PaidAt < end).ToList();
            var classes = db.Classes.Include(c => c.Course).Include(c => c.Teacher).Include(c => c.Enrollments).ToList();
            var es = db.Enrollments.Include(e => e.Payments).ToList();
            ViewBag.From = start.ToString("yyyy-MM-dd");
            ViewBag.To = end.AddDays(-1).ToString("yyyy-MM-dd");
            return View(new DashboardVm { Students = db.Users.Count(u => u.Role == UserRole.Student), Classes = classes.Count, Enrollments = es.Count, Revenue = paid.Sum(p => p.Amount), Outstanding = es.Where(e => e.Status != EnrollmentStatus.Withdrawn).Sum(e => e.Balance), ClassList = classes, RecentPayments = db.Payments.OrderByDescending(p => p.CreatedAt).Take(5).ToList(), RevenueMonths = paid.GroupBy(p => p.PaidAt.Value.ToString("yyyy-MM")).OrderBy(g => g.Key).Select(g => new Metric { Label = g.Key, Value = g.Sum(p => p.Amount) }).ToList(), Popularity = es.GroupBy(e => e.CenterClass.Course.Name).Select(g => new Metric { Label = g.Key, Value = g.Count() }).OrderByDescending(m => m.Value).ToList(), Workloads = db.Sessions.Where(s => s.StartsAt >= start && s.StartsAt < end).ToList().GroupBy(s => s.CenterClass.Teacher.FullName).Select(g => new Metric { Label = g.Key, Value = (decimal)g.Sum(s => (s.EndsAt - s.StartsAt).TotalHours) }).ToList() });
        }
    }
}
