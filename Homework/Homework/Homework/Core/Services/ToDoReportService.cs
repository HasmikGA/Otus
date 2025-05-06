using Otus.ToDoList.ConsoleBot.Types;
using Otus.ToDoList.ConsoleBot;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TaskBot.Core.DataAccess;

namespace TaskBot.Core.Services
{
    internal class ToDoReportService : IToDoReportService
    {
        private IToDoRepository toDoRepository;

        public ToDoReportService(IToDoRepository toDoRepository)
        {
            this.toDoRepository = toDoRepository;
        }
        public async Task <(int total, int completed, int active, DateTime generatedAt)> GetUserStats(Guid userId, CancellationToken ct)
        {
            var allList = await toDoRepository.GetAllByUserId(userId, ct);
            int active = await toDoRepository.CountActive(userId, ct);
            int completed = allList.Count - active;

            return (allList.Count, completed, active, DateTime.Now);
        }

    }
}
