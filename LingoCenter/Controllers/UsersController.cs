using System;
using System.Linq;
using System.Web.Mvc;
using LingoCenter.Models;
using LingoCenter.Services;

namespace LingoCenter.Controllers
{
    [Authorize(Roles = "Administrator")]
    public class UsersController : BaseController
    {
        public ActionResult Index(string q, int page = 1)
        {
            var query = db.Users.AsQueryable();

            if (!string.IsNullOrWhiteSpace(q))
            {
                q = q.Trim();

                query = query.Where(u =>
                    u.FullName.Contains(q) ||
                    u.Email.Contains(q));
            }

            var users = Paginate(
                query
                    .OrderBy(u => u.Role)
                    .ThenBy(u => u.FullName)
                    .ThenBy(u => u.Id),
                page
            );

            return View(users);
        }

        [HttpPost, ValidateAntiForgeryToken]
        public ActionResult Create(RegisterVm m, UserRole role)
        {
            if (!ModelState.IsValid || (m.Password ?? "").Length < 10 || !Enum.IsDefined(typeof(UserRole), role))
                return Failure("Nhập đủ tên, email, mật khẩu ít nhất 10 ký tự, xác nhận trùng khớp và vai trò hợp lệ.");
            string email = m.Email.Trim().ToLowerInvariant();
            if (db.Users.Any(u => u.Email == email))
                return Failure("Email đã tồn tại.");
            db.Users.Add(new User { FullName = m.FullName, Email = email, Phone = m.Phone, Role = role, PasswordHash = PasswordService.Hash(m.Password) });
            try
            {
                db.SaveChanges();
            }
            catch (System.Data.Entity.Infrastructure.DbUpdateException)
            {
                return Failure("Không thể tạo tài khoản. Email có thể vừa được sử dụng; vui lòng kiểm tra và thử lại.");
            }
            return Success("Đã tạo tài khoản.");
        }

        [HttpPost, ValidateAntiForgeryToken]
        public ActionResult Update(int id, string fullName, string phone, UserRole role, bool active = false)
        {
            var u = db.Users.Find(id);
            if (u == null)
                return HttpNotFound();
            if (string.IsNullOrWhiteSpace(fullName) || fullName.Length > 120 || (phone ?? "").Length > 30 || !Enum.IsDefined(typeof(UserRole), role))
                return Failure("Thông tin không hợp lệ.");
            if (u.Id == UserId && (!active || role != UserRole.Administrator))
                return Failure("Không thể tự khóa hoặc đổi vai trò quản trị của chính mình.");
            if (u.Role != role && (db.Classes.Any(c => c.TeacherId == id) || db.Enrollments.Any(e => e.StudentId == id) || db.Payments.Any(p => p.RecordedById == id)))
                return Failure("Tài khoản đã có dữ liệu nghiệp vụ, không thể đổi vai trò.");
            if (!active && db.Classes.Any(c => c.TeacherId == id && c.Status == ClassStatus.Open))
                return Failure("Hãy phân công lại các lớp đang mở trước khi khóa giáo viên.");
            if (u.Role != role || u.IsActive != active)
                u.SecurityStamp = Guid.NewGuid().ToString();
            u.FullName = fullName.Trim();
            u.Phone = phone;
            u.Role = role;
            u.IsActive = active;
            db.SaveChanges();
            return Success("Đã cập nhật người dùng.");
        }
    }
}
