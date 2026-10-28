using System;
using System.Linq;
using System.Web.Mvc;
using LingoCenter.Models;

namespace LingoCenter.Controllers
{
    public abstract class BaseController : Controller
    {
        protected readonly CenterDb db = new CenterDb();
        protected int UserId
        {
            get
            {
                return int.Parse(User.Identity.Name);
            }
        }

        protected override void OnActionExecuting(ActionExecutingContext context)
        {
            if (User.Identity.IsAuthenticated)
            {
                var u = db.Users.Find(UserId);
                ViewBag.CurrentUser = u;
                // Lab 6: Session stores only a temporary refresh timestamp, never business records.
                var last = Session["NoticeRefresh"] as DateTime? ;
                if (u.Role == UserRole.Student && (!last.HasValue || last.Value.AddMinutes(5) < DateTime.UtcNow))
                {
                    new LingoCenter.Services.NotificationService(db).Refresh(u.Id);
                    Session["NoticeRefresh"] = DateTime.UtcNow;
                }

                ViewBag.Unread = db.Notifications.Count(n => n.UserId == u.Id && !n.IsRead);
            }

            base.OnActionExecuting(context);
        }

        protected ActionResult Failure(string message, string action = "Index")
        {
            TempData["Error"] = message;
            return RedirectToAction(action);
        }

        protected ActionResult Success(string message, string action = "Index")
        {
            TempData["Success"] = message;
            return RedirectToAction(action);
        }

        protected override void Dispose(bool disposing)
        {
            if (disposing)
                db.Dispose();
            base.Dispose(disposing);
        }
        protected System.Collections.Generic.List<T> Paginate<T>(
    IOrderedQueryable<T> query,
    int page,
    int pageSize = 5)
        {
            int totalItems = query.Count();

            int totalPages = Math.Max(
                1,
                (int)Math.Ceiling(totalItems / (double)pageSize)
            );

            page = Math.Max(1, Math.Min(page, totalPages));

            ViewBag.CurrentPage = page;
            ViewBag.TotalPages = totalPages;
            ViewBag.TotalItems = totalItems;

            return query
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .ToList();
        }
    }
}
