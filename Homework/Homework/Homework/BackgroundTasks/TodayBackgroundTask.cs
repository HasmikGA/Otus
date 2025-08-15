using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TaskBot.Core.DataAccess;
using TaskBot.Core.Entities;
using TaskBot.Core.Services;
using static System.Runtime.InteropServices.JavaScript.JSType;

namespace TaskBot.BackgroundTasks
{
    internal class TodayBackgroundTask : BackgroundTask
    {
        private readonly INotificationService notificationService;
        private readonly IUserRepository userRepository;
        private readonly IToDoRepository toDoRepository;

        public TodayBackgroundTask(INotificationService notificationService, IUserRepository userRepository, IToDoRepository toDoRepository)
            : base(TimeSpan.FromDays(1), nameof(TodayBackgroundTask))
        {
            this.notificationService = notificationService;
            this.userRepository = userRepository;
            this.toDoRepository = toDoRepository;
        }

        protected override async Task Execute(CancellationToken ct)
        {
            var allusers = userRepository.GetUsers(ct);
            var today = DateTime.UtcNow.Date;
            var tomorrow = today.AddDays(1);

            foreach (var user in await allusers)
            {
                var tasks = await toDoRepository.GetActiveWithDeadline(user.UserId, today, tomorrow, ct);
                var taskListText = string.Join("\n", tasks.Select((t, i) => $"{i + 1}. {t.Name}"));
                var type = $"Today_{DateOnly.FromDateTime(DateTime.UtcNow)}";

                foreach (var task in tasks)
                {
                    await this.notificationService.ScheduleNotification(user.UserId, type, taskListText, DateTime.UtcNow, ct);
                }
            }
        }
    }
}
