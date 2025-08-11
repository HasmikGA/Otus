using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TaskBot.Core.Services;
using Telegram.Bot;
using Telegram.Bot.Types;

namespace TaskBot.BackgroundTasks
{
    internal class NotificationBackgroundTask: BackgroundTask
    {
        private readonly INotificationService notificationService;
        private readonly ITelegramBotClient bot;
        public NotificationBackgroundTask(INotificationService notificationService, ITelegramBotClient bot) 
            : base(TimeSpan.FromMinutes(1), nameof(NotificationBackgroundTask))
        { 
            this.notificationService = notificationService;
            this.bot = bot;
        }
        protected override async Task Execute(CancellationToken ct)
        {
           var notifications =  await notificationService.GetScheduledNotification(DateTime.UtcNow, ct);
            foreach (var notification in notifications)
            {
                var chatId = new ChatId(notification.User.TelegramUserId);
                await bot.SendMessage(chatId, notification.Text, cancellationToken: ct);
                await notificationService.MarkNotified(notification.Id, ct);
            }
        }
    }
}
