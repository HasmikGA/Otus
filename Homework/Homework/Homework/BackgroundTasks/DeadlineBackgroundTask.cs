using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TaskBot.Core.DataAccess;
using TaskBot.Core.Entities;
using TaskBot.Core.Services;

namespace TaskBot.BackgroundTasks
{
    internal class DeadlineBackgroundTask : BackgroundTask
    {
        private readonly INotificationService notificationService;
        private readonly IUserRepository userRepository;
        private readonly IToDoRepository toDoRepository;

        public DeadlineBackgroundTask(INotificationService notificationService, IUserRepository userRepository, IToDoRepository toDoRepository)
            : base(TimeSpan.FromHours(1), nameof(DeadlineBackgroundTask))
        {
            this.notificationService = notificationService;
            this.userRepository = userRepository;
            this.toDoRepository = toDoRepository;
        }

        protected override async Task Execute(CancellationToken ct)
        {
            var allusers = userRepository.GetUsers(ct);
            foreach (var user in await allusers)
            {
                var deadlineTasks = await toDoRepository.GetActiveWithDeadline(user.UserId, DateTime.UtcNow.AddDays(-1).Date, DateTime.UtcNow.Date, ct);
                foreach (var task in deadlineTasks)
                {
                    var type = $"Dealine_{task.Id}";
                    var text = $"Oops! You missed the deadline for the task {task.Name}";
                    await this.notificationService.ScheduleNotification(user.UserId, type, text, DateTime.UtcNow, ct);
                }
            }
        }
    }
}
