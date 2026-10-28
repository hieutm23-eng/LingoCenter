using System;
using System.Data.Entity;
using System.Security.Principal;
using System.Web;
using System.Web.Mvc;
using System.Web.Routing;
using System.Web.Security;
using LingoCenter.Models;

namespace LingoCenter
{
    public class MvcApplication : HttpApplication
    {
        protected void Application_Start()
        {
            Database.SetInitializer(new Data.DemoInitializer());
            AreaRegistration.RegisterAllAreas();
            RouteTable.Routes.IgnoreRoute("{resource}.axd/{*pathInfo}");
            RouteTable.Routes.MapRoute("Default", "{controller}/{action}/{id}", new { controller = "Home", action = "Index", id = UrlParameter.Optional });
            MvcHandler.DisableMvcResponseHeader = true;
        }

        protected void Application_PostAuthenticateRequest()
        {
            var cookie = Request.Cookies[FormsAuthentication.FormsCookieName];
            if (cookie == null)
                return;
            try
            {
                var t = FormsAuthentication.Decrypt(cookie.Value);
                int id;
                if (t == null || t.Expired || !int.TryParse(t.Name, out id))
                    return;
                using (var db = new CenterDb())
                {
                    var u = db.Users.Find(id);
                    if (u != null && u.IsActive && u.SecurityStamp == t.UserData)
                    {
                        Context.User = new GenericPrincipal(new GenericIdentity(u.Id.ToString(), "Forms"), new[] { u.Role.ToString() });
                        return;
                    }
                }

                FormsAuthentication.SignOut();
                Context.User = new GenericPrincipal(new GenericIdentity(""), new string[0]);
            }
            catch (System.Security.Cryptography.CryptographicException)
            {
                FormsAuthentication.SignOut();
            }
        }
    }
}
