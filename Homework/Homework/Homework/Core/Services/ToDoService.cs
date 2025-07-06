using System.Text.RegularExpressions;
using TaskBot.Core.DataAccess;
using TaskBot.Core.Entities;
using TaskBot.Core.Exceptions;

namespace TaskBot.Core.Services
{
    internal class ToDoService : IToDoService
    {
        private int itemCountLimit;
        private int itemLengthLimit;
        private readonly IToDoRepository toDoRepository;

        public ToDoService(int itemCountLimit, int itemLengthLimit, IToDoRepository toDoRepository)
        {
            this.itemCountLimit = itemCountLimit;
            this.itemLengthLimit = itemLengthLimit;
            this.toDoRepository = toDoRepository;
        }

        public async Task<ToDoItem> Add(ToDoUser user, string name, DateTime deadline, ToDoList? list, CancellationToken ct)
        {
            var toDoItems = await this.toDoRepository.GetAllByUserId(user.UserId, ct);
            if (toDoItems.Count > itemCountLimit)
            {
                throw new TaskCountLimitException(itemCountLimit);
            }

            if (name.Length > itemLengthLimit)
            {
                throw new TaskLengthLimitException(name.Length, itemLengthLimit);
            }

            for (int i = 0; i < toDoItems.Count; i++)
            {
                if (toDoItems[i].Name == name)
                {
                    throw new DuplicateTaskException(name);
                }
            }

            ValidateString(name);

            ToDoItem toDoItem = new ToDoItem()
            {
                Id = Guid.NewGuid(),
                Name = name,
                User = user,
                CreatedAt = DateTime.Now,
                State = ToDoItemState.Active,
                List = list,
            };

            toDoRepository.Add(toDoItem, ct);

            return toDoItem;
        }

        public async Task Delete(Guid id, Guid userId, CancellationToken ct)
        {
            this.toDoRepository.Delete(id, userId, ct);
            return;
        }
        public async Task<IReadOnlyList<ToDoItem>> GetByUserIdAndList(Guid userId, Guid? listId, CancellationToken ct)
        {
            var toDoItemList = await this.toDoRepository.GetByUserIdAndList(userId, listId, ct);

            return toDoItemList;
        }
        public async Task<IReadOnlyList<ToDoItem>> GetActiveByUserId(Guid userId, CancellationToken ct)
        {
            var activeList = await toDoRepository.GetActiveByUserId(userId, ct);

            return activeList;
        }

        public async Task<IReadOnlyList<ToDoItem>> FindCompleted(Guid userId, CancellationToken ct)
        {
            var result = await toDoRepository.Find(userId, (item) => item.State == ToDoItemState.Completed, ct);
            return result;
        }

        public async Task<IReadOnlyList<ToDoItem>> GetAllByUserId(Guid userId, CancellationToken ct)
        {
            var allList = await toDoRepository.GetAllByUserId(userId, ct);

            return allList;
        }

        public async Task<ToDoItem?> Get(Guid userId, Guid toDoItemId, CancellationToken ct)
        {
            var items = await this.toDoRepository.Find(userId, (item) => item.Id == toDoItemId, ct);

            return items.FirstOrDefault();
        }

        public async Task MarkCompleted(Guid userId, Guid id, CancellationToken ct)
        {
            var item = await this.Get(userId, id, ct);
            if (item == null)
            {
                return;
            }

            item.State = ToDoItemState.Completed;
            item.StateChangedAt = DateTime.UtcNow;
            this.toDoRepository.Update(item, ct);
        }

        private void ValidateString(string taskName)
        {
            bool containsSpecialChars = Regex.IsMatch(taskName, @"[!@#$%^&*(),.?""':;{}|<>]");

            if (string.IsNullOrEmpty(taskName) || containsSpecialChars)
            {
                throw new ArgumentException("The task isn`t in correct form");
            }
        }

        public async Task<IReadOnlyList<ToDoItem>> Find(ToDoUser user, string namePrefix, CancellationToken ct)
        {
            var result = await toDoRepository.Find(user.UserId, (item) => item.Name.StartsWith(namePrefix), ct);
            return result;
        }



    }
}
