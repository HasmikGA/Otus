using Otus.ToDoList.ConsoleBot.Types;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using TaskBot.Core.DataAccess;
using TaskBot.Core.Entities;
using TaskBot.Core.Exceptions;


namespace TaskBot.Core.Services
{
    internal class ToDoService : IToDoService
    {
        private readonly List<ToDoItem> toDoItems = new List<ToDoItem>();

        private int itemCountLimit;
        private int itemLengthLimit;
        private readonly IToDoRepository toDoRepository;


        public ToDoService(int itemCountLimit, int itemLengthLimit, IToDoRepository toDoRepository)
        {
            this.itemCountLimit = itemCountLimit;
            this.itemLengthLimit = itemLengthLimit;
            this.toDoRepository = toDoRepository;
        }


        public async Task<ToDoItem> Add(ToDoUser user, string name, CancellationToken ct)
        {
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
            };

            await toDoRepository.Add(toDoItem, ct);

            return toDoItem;
        }

        public async Task Delete(Guid id, CancellationToken ct)
        {
            for (int i = 0; i < toDoItems.Count; i++)
            {
                if (toDoItems[i].Id == id)
                {
                    toDoItems.RemoveAt(i);
                    break;
                }
            }

        }
        public async Task<IReadOnlyList<ToDoItem>> GetActiveByUserId(Guid userId, CancellationToken ct)
        {
            var activeList = await toDoRepository.GetActiveByUserId(userId, ct);

            return activeList;
        }
        public async Task<IReadOnlyList<ToDoItem>> GetAllByUserId(Guid userId, CancellationToken ct)
        {
            var allList = await toDoRepository.GetAllByUserId(userId, ct);

            return allList;
        }

        public async Task MarkCompleted(Guid id, CancellationToken ct)
        {
            for (int i = 0; i < toDoItems.Count; i++)
            {
                if (toDoItems[i].Id == id)
                {
                    toDoItems[i].State = ToDoItemState.Completed;
                    break;
                }
            }
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
