using System.Data.Entity;
using System.Data.Entity.ModelConfiguration.Conventions;

namespace LingoCenter.Models
{
    public class CenterDb : DbContext
    {
        public CenterDb() : base("CenterDb")
        {
        }

        public CenterDb(string connectionString) : base(connectionString)
        {
        }

        public DbSet<User> Users { get; set; }
        public DbSet<Course> Courses { get; set; }
        public DbSet<CenterClass> Classes { get; set; }
        public DbSet<ClassSession> Sessions { get; set; }
        public DbSet<Enrollment> Enrollments { get; set; }
        public DbSet<Attendance> Attendances { get; set; }
        public DbSet<Payment> Payments { get; set; }
        public DbSet<Notification> Notifications { get; set; }

        protected override void OnModelCreating(DbModelBuilder b)
        {
            b.Conventions.Remove<OneToManyCascadeDeleteConvention>();
            b.Entity<User>().ToTable("Users");
            b.Entity<Course>().ToTable("Courses");
            b.Entity<CenterClass>().ToTable("CenterClasses");
            b.Entity<ClassSession>().ToTable("ClassSessions");
            b.Entity<Enrollment>().ToTable("Enrollments");
            b.Entity<Attendance>().ToTable("Attendances");
            b.Entity<Payment>().ToTable("Payments");
            b.Entity<Notification>().ToTable("Notifications");
            b.Entity<Course>().Property(x => x.TuitionFee).HasPrecision(18, 0);
            b.Entity<Enrollment>().Property(x => x.AgreedFee).HasPrecision(18, 0);
            b.Entity<Payment>().Property(x => x.Amount).HasPrecision(18, 0);
            b.Entity<Enrollment>().Property(x => x.Midterm).HasPrecision(4, 2);
            b.Entity<Enrollment>().Property(x => x.Final).HasPrecision(4, 2);
        }
    }
}
