using System;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using System.Web.Security;
using LingoCenter.Models;
using LingoCenter.Services;

namespace LingoCenter.Controllers
{
    public class AccountController : BaseController
    {
        [AllowAnonymous]
        public ActionResult Login()
        {
            return View(new LoginVm());
        }

        [HttpPost, AllowAnonymous, ValidateAntiForgeryToken]
        public ActionResult Login(LoginVm model, string returnUrl)
        {
            if (!ModelState.IsValid)
                return View(model);
            var email = model.Email.Trim().ToLowerInvariant();
            var u = db.Users.SingleOrDefault(x => x.Email == email);
            if (u == null || !u.IsActive || u.LockedUntilUtc > DateTime.UtcNow || !PasswordService.Verify(model.Password, u.PasswordHash))
            {
                if (u != null)
                {
                    u.FailedLogins++;
                    if (u.FailedLogins >= 5)
                        u.LockedUntilUtc = DateTime.UtcNow.AddMinutes(10);
                    db.SaveChanges();
                }

                ModelState.AddModelError("", "Email hoặc mật khẩu không đúng, hoặc tài khoản tạm khóa.");
                return View(model);
            }

            u.FailedLogins = 0;
            u.LockedUntilUtc = null;
            db.SaveChanges();
            var ticket = new FormsAuthenticationTicket(1, u.Id.ToString(), DateTime.Now, DateTime.Now.AddHours(8), false, u.SecurityStamp);
            Response.Cookies.Add(new HttpCookie(FormsAuthentication.FormsCookieName, FormsAuthentication.Encrypt(ticket)) { HttpOnly = true, Secure = Request.IsSecureConnection, SameSite = SameSiteMode.Lax });
            return Url.IsLocalUrl(returnUrl) ? (ActionResult)Redirect(returnUrl) : RedirectToAction("Index", "Home");
        }

        [AllowAnonymous]
        public ActionResult Register()
        {
            return View(new RegisterVm());
        }

        [HttpPost, AllowAnonymous, ValidateAntiForgeryToken]
        public ActionResult Register(RegisterVm m)
        {
            if ((m.Password ?? "").Length < 10)
                ModelState.AddModelError("Password", "Mật khẩu ít nhất 10 ký tự.");
            if (!ModelState.IsValid)
                return View(m);
            string email = m.Email.Trim().ToLowerInvariant();
            if (db.Users.Any(u => u.Email == email))
            {
                ModelState.AddModelError("Email", "Email đã được sử dụng.");
                return View(m);
            }

            db.Users.Add(new User { FullName = m.FullName.Trim(), Email = email, Phone = m.Phone, PasswordHash = PasswordService.Hash(m.Password), Role = UserRole.Student });
            try
            {
                db.SaveChanges();
            }
            catch (System.Data.Entity.Infrastructure.DbUpdateException)
            {
                ModelState.AddModelError("", "Email đã tồn tại. Vui lòng thử lại.");
                return View(m);
            }

            return Success("Đăng ký thành công. Bạn có thể đăng nhập.", "Login");
        }

        [Authorize, HttpPost, ValidateAntiForgeryToken]
        public ActionResult Logout()
        {
            FormsAuthentication.SignOut();
            Session.Abandon();
            return RedirectToAction("Login");
        }

        [Authorize]
        public new ActionResult Profile()
        {
            var u = db.Users.Find(UserId);
            return View(new ProfileVm { FullName = u.FullName, Phone = u.Phone });
        }

        [Authorize, HttpPost, ValidateAntiForgeryToken]
        public new ActionResult Profile(ProfileVm m)
        {
            if (!ModelState.IsValid)
                return View(m);
            var u = db.Users.Find(UserId);
            u.FullName = m.FullName;
            u.Phone = m.Phone;
            db.SaveChanges();
            return Success("Đã cập nhật hồ sơ.", "Profile");
        }

        [Authorize]
        public ActionResult Password()
        {
            return View(new PasswordVm());
        }

        [Authorize, HttpPost, ValidateAntiForgeryToken]
        public ActionResult Password(PasswordVm m)
        {
            if (!ModelState.IsValid)
                return View(m);
            var u = db.Users.Find(UserId);
            if (!PasswordService.Verify(m.CurrentPassword, u.PasswordHash))
            {
                ModelState.AddModelError("", "Mật khẩu hiện tại không đúng.");
                return View(m);
            }

            u.PasswordHash = PasswordService.Hash(m.NewPassword);
            u.SecurityStamp = Guid.NewGuid().ToString();
            db.SaveChanges();
            FormsAuthentication.SignOut();
            return Success("Đã đổi mật khẩu. Hãy đăng nhập lại.", "Login");
        }
    }
}
