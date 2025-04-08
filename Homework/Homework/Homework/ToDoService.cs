using Otus.ToDoList.ConsoleBot.Types;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using TaskBot.Exceptions;

namespace TaskBot
{
    internal class ToDoService : IToDoService
    {
        private List<ToDoItem> toDoItems = new List<ToDoItem>();

        public int ItemCountLimit { get; set; } = 20;
        
        public ToDoItem Add(ToDoUser user, string name) 
        {
            if(toDoItems.Count > this.ItemCountLimit)
            {
                throw new TaskCountLimitException(ItemCountLimit);
            }

            for (int i = 0; i < toDoItems.Count; i++)
            {
                if (toDoItems[i].Name == name)
                {
                    throw new DuplicateTaskException(name);
                }
            }

            this.ValidateString(name);

            

            ToDoItem toDoItem = new ToDoItem()
            {
                Id = Guid.NewGuid(),
                Name = name,
                User = user,
                CreatedAt = DateTime.Now,
                State = ToDoItemState.Active,
            };

            toDoItems.Add(toDoItem);

            return toDoItem;
        }

        public void Delete(Guid id)
        {
            for (int i = 0; i < toDoItems.Count; i++)
            {
                if (toDoItems[i].Id == id)
                {
                    toDoItems.RemoveAt(i);
                }
            }

        }
        public IReadOnlyList<ToDoItem> GetActiveByUserId(Guid userId)
        {
            List<ToDoItem> activetList = new List<ToDoItem>();
            for (int i = 0; i < toDoItems.Count; i++)
            {
                if (toDoItems[i].Id == userId && toDoItems[i].State == ToDoItemState.Active)
                {
                    activetList.Add(toDoItems[i]);
                }
            }
            return activetList;
        }
        public IReadOnlyList<ToDoItem> GetByUserId(Guid userId)
        {
            List<ToDoItem> allList = new List<ToDoItem>();
            for (int i = 0; i < toDoItems.Count; i++)
            {
                if (toDoItems[i].Id == userId )
                {
                    allList.Add(toDoItems[i]);
                }
            }
            return allList;
        }

        public void MarkCompleted(Guid id)
        {
            for (int i = 0; i < toDoItems.Count; i++)
            {
                if (toDoItems[i].Id == id)
                {
                    toDoItems[i].State = ToDoItemState.Completed;
                }
            }
        }
        private void ValidateString(string taskName)
        {
            bool containsSpecialChars = Regex.IsMatch(taskName, @"[!@#$%^&*(),.?""':;{}|<>]");

            if (string.IsNullOrEmpty(taskName) || containsSpecialChars)
            {
                throw new ArgumentException();
            }
        }
       
    }
}
