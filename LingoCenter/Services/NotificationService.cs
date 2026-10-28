using System;
using System.Linq;
using LingoCenter.Models;

namespace LingoCenter.Services
{
    public class NotificationService
    {
        private readonly CenterDb db;
        public NotificationService(CenterDb db)
        {
            this.db = db;
        }

        public void Refresh(int userId)
        {
            using (var tx = db.Database.BeginTransaction(System.Data.IsolationLevel.Serializable))
            {
                db.Database.SqlQuery<int>("SELECT Id FROM dbo.Users WITH (UPDLOCK,HOLDLOCK) WHERE Id=@p0", userId).ToList();
                var now = CenterClock.Now;
                var tomorrow = now.AddHours(24);
                var es = db.Enrollments.Where(x => x.StudentId == userId && x.Status != EnrollmentStatus.Withdrawn).ToList();
                foreach (var e in es)
                {
                    if (e.Balance > 0 && e.DueDate <= now.AddDays(3))
                        Add(userId, "due-" + e.Id + "-" + now.ToString("yyyyMMdd"), "Học phí " + e.CenterClass.Code + ": còn " + e.Balance.ToString("N0") + " ₫, hạn " + e.DueDate.ToString("dd/MM/yyyy"));
                    if (e.ScoresPublished)
                        Add(userId, "score-" + e.Id, "Điểm lớp " + e.CenterClass.Code + " đã được công bố. Xem tại Kết quả học tập.");
                    if (e.Status == EnrollmentStatus.Confirmed || e.Status == EnrollmentStatus.Studying)
                        foreach (var s in e.CenterClass.Sessions.Where(x => x.StartsAt >= now && x.StartsAt <= tomorrow))
                            Add(userId, "session-" + s.Id, "Sắp học " + e.CenterClass.Code + " lúc " + s.StartsAt.ToString("HH:mm dd/MM") + ", phòng " + e.CenterClass.Room);
                }

                db.SaveChanges();
                tx.Commit();
            }
        }

        private void Add(int id, string key, string message)
        {
            if (!db.Notifications.Any(n => n.RecipientKey == id && n.EventKey == key))
                db.Notifications.Add(new Notification { UserId = id, RecipientKey = id, EventKey = key, Message = message, CreatedAt = CenterClock.Now });
        }
    }
}
