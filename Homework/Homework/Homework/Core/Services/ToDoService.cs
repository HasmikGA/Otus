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


        public ToDoItem Add(ToDoUser user, string name)
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

            toDoRepository.Add(toDoItem);

            return toDoItem;
        }

        public void Delete(Guid id)
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
        public IReadOnlyList<ToDoItem> GetActiveByUserId(Guid userId)
        {
            var activeList = toDoRepository.GetActiveByUserId(userId);

            return activeList;
        }
        public IReadOnlyList<ToDoItem> GetAllByUserId(Guid userId)
        {
            var allList = toDoRepository.GetAllByUserId(userId);

            return allList;
        }

        public void MarkCompleted(Guid id)
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

        public IReadOnlyList<ToDoItem> Find(ToDoUser user, string namePrefix)
        {
            var result = toDoRepository.Find(user.UserId, (item) => item.Name.StartsWith(namePrefix));
            return result;
        }
    }
}
