using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TaskBot.Core.DataAccess;
using TaskBot.Core.Entities;

namespace TaskBot.Core.Services
{
    internal class ToDoListService : IToDoListService
    {
        private readonly IToDoListRepository toDoListRepository;

        public ToDoListService(IToDoListRepository toDoListRepository)
        {
            this.toDoListRepository = toDoListRepository;
        }
        public async Task<ToDoList> Add(ToDoUser user, string name, CancellationToken ct)
        {
            if (await toDoListRepository.ExistsByName(user.UserId, name, ct) || name.Length > 10)
            {
                throw new ArgumentException("Repeated or long list name");
            }

            var toDoList = new ToDoList
            {
                Id = Guid.NewGuid(),
                Name = name,
                User = user,
                CreatedAt = DateTime.Now
            };

            await toDoListRepository.Add(toDoList, ct);
            return toDoList;
        }

        public async Task Delete(Guid id, CancellationToken ct)
        { 
            await toDoListRepository.Delete(id, ct);
        }

        public async Task<ToDoList?> Get(Guid id, CancellationToken ct)
        {
           return await toDoListRepository.Get(id, ct);
        }

        public async Task<IReadOnlyList<ToDoList>> GetUserLists(Guid userId, CancellationToken ct)
        {
            return await toDoListRepository.GetByUserId(userId, ct);
        }
    }
}
