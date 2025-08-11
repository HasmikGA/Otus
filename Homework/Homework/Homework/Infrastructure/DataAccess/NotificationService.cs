using LinqToDB;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TaskBot.Core.DataAccess;
using TaskBot.Core.Entities;
using TaskBot.Core.Services;

namespace TaskBot.Infrastructure.DataAccess
{
    internal class NotificationService : INotificationService
    {
        private readonly IDataContextFactory<ToDoDataContext> factory;
        public NotificationService(IDataContextFactory<ToDoDataContext> factory)
        {
            this.factory = factory;
        }
        public async Task<IReadOnlyList<Notification>> GetScheduledNotification(DateTime scheduledBefore, CancellationToken ct)
        {
            using (var dbContext = this.factory.CreateDataContext())
            {
                var notifications = await dbContext.Notifications
                    .LoadWith(i=>i.User)
                    .Where(i => i.IsNotified == false && i.ScheduledAt <= scheduledBefore)
                    .Select(i => ModelMapper.MapFromModel(i))
                    .ToListAsync();

                return notifications;
            }
        }

        public async Task MarkNotified(Guid notificationId, CancellationToken ct)
        {
            using (var dbContext = this.factory.CreateDataContext())
            {
                var notification = await dbContext.Notifications
                    .FirstOrDefaultAsync(i => i.Id == notificationId);

                if (notification == null)
                {
                    return;
                }

                notification.IsNotified = true;

                await dbContext.UpdateAsync(notification);
            }
        }

        public async Task<bool> ScheduleNotification(Guid userId, string type, string text, DateTime scheduledAt, CancellationToken ct)
        {
            using (var dbContext = this.factory.CreateDataContext())
            {
                var exists = await dbContext.Notifications.AnyAsync(n => n.UserId == userId && n.Type == type, ct);
                if (exists)
                {
                    return false;
                }

                var userModel = await dbContext.ToDoUsers.FirstOrDefaultAsync(i => i.UserId == userId);
                var user = ModelMapper.MapFromModel(userModel);
                var notification = new Notification
                {
                    Id = Guid.NewGuid(),
                    User = user,
                    Type = type,
                    Text = text,
                    ScheduledAt = scheduledAt,
                    IsNotified = false,
                    NotifiedAt = DateTime.UtcNow,
                };

                await dbContext.InsertAsync(notification);

                return true;
            }
        }
    }
}
