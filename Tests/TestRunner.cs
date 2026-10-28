using System;
using System.IO;
using System.IO.Compression;
using System.Linq;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Reflection;
using System.Web.Mvc;
using System.Data.Entity;
using System.Data.Entity.Infrastructure;
using System.Data.SqlClient;
using LingoCenter.Models;
using LingoCenter.Services;
using LingoCenter.Controllers;

class TestRunner
{
    static int passed;
    static void Check(bool condition, string name)
    {
        if (!condition)
            throw new Exception("FAIL: " + name);
        passed++;
        Console.WriteLine("PASS: " + name);
    }

    static void Reject(Action action, string name)
    {
        try
        {
            action();
        }
        catch (InvalidOperationException)
        {
            Check(true, name);
            return;
        }

        throw new Exception("FAIL: " + name);
    }

    static int Main(string[] args)
    {
        try
        {
            var hash = PasswordService.Hash("Lingo@2026!");
            Check(PasswordService.Verify("Lingo@2026!", hash), "Password verification");
            Check(!PasswordService.Verify("wrong-password", hash), "Wrong password rejected");
            Check(hash != PasswordService.Hash("Lingo@2026!"), "Random salt per password");
            var e = new Enrollment
            {
                AgreedFee = 3000000,
                Midterm = 7,
                Final = 8
            };
            Check(e.Result == 7.6m, "40/60 weighted result");
            e.Final = null;
            Check(e.Result == null, "Incomplete assessment has no result");
            e.Payments.Add(new Payment { Amount = 1000000, Status = PaymentStatus.Paid });
            e.Payments.Add(new Payment { Amount = 500000, Status = PaymentStatus.Pending });
            e.Payments.Add(new Payment { Amount = 200000, Status = PaymentStatus.Rejected });
            Check(e.Paid == 1000000 && e.Balance == 2000000, "Pending and rejected payments do not reduce debt");
            var c = new CenterClass
            {
                Capacity = 1
            };
            c.Enrollments.Add(new Enrollment { Status = EnrollmentStatus.Withdrawn });
            Check(c.Availability == "Open", "Withdrawn enrollment releases seat");
            c.Enrollments.Add(new Enrollment { Status = EnrollmentStatus.Pending });
            Check(c.Availability == "Full", "Pending enrollment reserves seat");
            c.Status = ClassStatus.Closed;
            Check(c.Availability == "Closed", "Closed status wins over full");
            var results = new List<ValidationResult>();
            var invalidScore = new ScoreRow
            {
                Midterm = 11
            };
            Check(!Validator.TryValidateObject(invalidScore, new ValidationContext(invalidScore), results, true), "Scores above 10 fail validation");
            var bytes = ExcelExport.Create(new[] { new[] { "Học viên", "Amount" }, new[] { "=1+1", "2000000" } });
            using (var ms = new MemoryStream(bytes))
            using (var z = new ZipArchive(ms))
            {
                Check(z.GetEntry("xl/workbook.xml") != null && z.GetEntry("[Content_Types].xml") != null, "Export is an XLSX package");
                using (var r = new StreamReader(z.GetEntry("xl/worksheets/sheet1.xml").Open()))
                {
                    var text = r.ReadToEnd();
                    Check(text.Contains("inlineStr") && !text.Contains("<f>"), "Export values cannot execute formulas");
                }
            }

            foreach (var type in typeof(AccountController).Assembly.GetTypes().Where(t => typeof(Controller).IsAssignableFrom(t)))
                foreach (var method in type.GetMethods(BindingFlags.Public | BindingFlags.Instance | BindingFlags.DeclaredOnly).Where(m => m.IsDefined(typeof(HttpPostAttribute), true)))
                    Check(method.IsDefined(typeof(ValidateAntiForgeryTokenAttribute), true), "CSRF protection: " + type.Name + "." + method.Name);
            foreach (var type in new[]
            {
                typeof(CoursesController),
                typeof(ClassesController),
                typeof(UsersController),
                typeof(EnrollmentsController),
                typeof(ReportsController),
                typeof(DashboardController)
            }

            )
                Check(type.GetCustomAttributes(typeof(AuthorizeAttribute), true).Cast<AuthorizeAttribute>().Any(a => a.Roles == "Administrator"), "Admin gate: " + type.Name);
            var b = new DbModelBuilder();
            foreach (var t in new[]
            {
                typeof(User),
                typeof(Course),
                typeof(CenterClass),
                typeof(ClassSession),
                typeof(Enrollment),
                typeof(Attendance),
                typeof(Payment),
                typeof(Notification)
            }

            )
                typeof(DbModelBuilder).GetMethod("Entity").MakeGenericMethod(t).Invoke(b, null);
            using (var context = new CenterDb())
                typeof(CenterDb).GetMethod("OnModelCreating", BindingFlags.NonPublic | BindingFlags.Instance).Invoke(context, new object[] { b });
            Check(b.Build(new DbProviderInfo("System.Data.SqlClient", "2012")).Compile() != null, "Entity Framework SQL Server model compiles");
            if (args.Contains("--integration"))
                Integration();
            else
                Console.WriteLine("NOT RUN: SQL/IIS integration. Run this test project with --integration on a machine with SQL Server access.");
            Console.WriteLine("SUCCESS: " + passed + " checks passed.");
            return 0;
        }
        catch (Exception ex)
        {
            Console.WriteLine(ex.ToString());
            return 1;
        }
    }

    static void Integration()
    {
        // Tests own a newly named throwaway database; never touch LingoCenterDemo.
        string name = "LingoCenterTest_" + Guid.NewGuid().ToString("N");
        var server = Environment.GetEnvironmentVariable("LINGO_TEST_SQLSERVER") ?? @".\SQLEXPRESS";
        var connection = new SqlConnectionStringBuilder
        {
            DataSource = server,
            InitialCatalog = name,
            IntegratedSecurity = true,
            TrustServerCertificate = true
        }.ConnectionString;
        Database.SetInitializer(new LingoCenter.Data.DemoInitializer());
        try
        {
            using (var db = new CenterDb(connection))
            {
                db.Database.Initialize(true);
                Check(db.Users.Count() == 26 && db.Courses.Count() == 8 && db.Classes.Count() == 12, "Demo users, courses, classes");
                Check(db.Enrollments.Count() == 36 && db.Payments.Count() == 24, "Demo enrollments and payments");
                Check(db.Attendances.Select(a => a.ClassSessionId).Distinct().Count() >= 10, "Attendance on 10+ sessions");
                Check(db.Enrollments.Where(x => x.Midterm != null && x.Final != null).Select(x => x.StudentId).Distinct().Count() >= 10, "Scores for 10+ students");
                int student = db.Users.Single(u => u.Email == "student20@lingocenter.test").Id;
                int full = db.Classes.Single(x => x.Code == "LC-101").Id;
                int closed = db.Classes.Single(x => x.Code == "LC-112").Id;
                Reject(() => new EnrollmentService(db).Enroll(student, full), "Reject full class");
                Reject(() => new EnrollmentService(db).Enroll(student, closed), "Reject closed class");
                var first = db.Enrollments.First();
                Reject(() => new EnrollmentService(db).Enroll(first.StudentId, first.CenterClassId), "Reject duplicate enrollment");
                var open = db.Classes.Single(x => x.Code == "LC-102");
                var created = new EnrollmentService(db).Enroll(student, open.Id);
                Check(created.Status == EnrollmentStatus.Pending && created.AgreedFee == open.Course.TuitionFee, "Enroll stores tuition snapshot");
                Reject(() => new EnrollmentService(db).ChangeStatus(created.Id, EnrollmentStatus.Completed), "Reject invalid state transition");
                new EnrollmentService(db).ChangeStatus(created.Id, EnrollmentStatus.Confirmed);
                Check(created.Status == EnrollmentStatus.Confirmed, "Confirm enrollment");
                var service = new PaymentService(db);
                Reject(() => service.Submit(created.Id, student, -1, "TEST"), "Reject negative payment");
                Reject(() => service.Submit(created.Id, student, created.AgreedFee + 1, "TEST"), "Reject overpayment");
                service.Submit(created.Id, student, 100000, "TEST-TRANSFER");
                db.Entry(created).Collection(x => x.Payments).Load();
                var payment = db.Payments.Single(p => p.EnrollmentId == created.Id);
                Check(payment.Status == PaymentStatus.Pending && created.Paid == 0, "Submitted payment awaits confirmation");
                int admin = db.Users.Single(u => u.Role == UserRole.Administrator).Id;
                service.Review(payment.Id, true, admin);
                Check(payment.Status == PaymentStatus.Paid && created.Paid == 100000, "Confirmed payment reduces debt");
                Reject(() => service.Review(payment.Id, true, admin), "Cannot confirm a payment twice");
                new NotificationService(db).Refresh(student);
                int count = db.Notifications.Count(n => n.UserId == student);
                new NotificationService(db).Refresh(student);
                Check(db.Notifications.Count(n => n.UserId == student) == count, "Notifications are idempotent");
            }
        }
        finally
        {
            // Verify exact task-generated prefix and GUID before deleting our test database.
            Guid g;
            if (name.StartsWith("LingoCenterTest_") && Guid.TryParseExact(name.Substring(16), "N", out g))
                using (var db = new CenterDb(connection))
                {
                    if (db.Database.Exists())
                        db.Database.Delete();
                }
        }
    }
}
