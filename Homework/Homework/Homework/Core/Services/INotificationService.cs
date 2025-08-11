using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TaskBot.Core.Entities;

namespace TaskBot.Core.Services
{
    internal interface INotificationService
    {
        Task<bool> ScheduleNotification(Guid userId, string type, string text, DateTime scheduledAt, CancellationToken ct);
        Task<IReadOnlyList<Notification>> GetScheduledNotification(DateTime scheduledBefore, CancellationToken ct);
        Task MarkNotified(Guid notificationId, CancellationToken ct);
    }
}
