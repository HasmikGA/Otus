using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TaskBot.Core.DataAccess;
using TaskBot.Core.Entities;


namespace TaskBot.Infrastructure.DataAccess
{
    internal class InMemoryToDoRepository : IToDoRepository
    {
        private readonly List<ToDoItem> toDoItems = new List<ToDoItem>();
        public void Add(ToDoItem item)
        {
            toDoItems.Add(item);
        }

        public int CountActive(Guid userId)
        {
            IReadOnlyList<ToDoItem> activetList = GetActiveByUserId(userId);
            return activetList.Count;
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

        public bool ExistsByName(Guid userId, string name)
        {
            for (int i = 0; i < toDoItems.Count; i++)
            {
                if (toDoItems[i].User?.UserId == userId && toDoItems[i].Name == name)
                {
                    return true;
                }
            }
            return false;
        }

        public IReadOnlyList<ToDoItem> Find(Guid userId, Func<ToDoItem, bool> predicate)
        {
            List<ToDoItem> pridicateList = new List<ToDoItem>();
            for (int i = 0; i < toDoItems.Count; i++)
            {
                if (toDoItems[i].User?.UserId == userId && predicate(toDoItems[i]))
                {
                pridicateList.Add(toDoItems[i]);
            }
        }
            return pridicateList;
        }

    public IReadOnlyList<ToDoItem> GetActiveByUserId(Guid userId)
    {
        List<ToDoItem> activetList = new List<ToDoItem>();
        for (int i = 0; i < toDoItems.Count; i++)
        {
            if (toDoItems[i].User?.UserId == userId && toDoItems[i].State == ToDoItemState.Active)
            {
                activetList.Add(toDoItems[i]);
            }
        }
        return activetList;
    }

    public IReadOnlyList<ToDoItem> GetAllByUserId(Guid userId)
    {
        List<ToDoItem> allList = new List<ToDoItem>();
        for (int i = 0; i < toDoItems.Count; i++)
        {
            if (toDoItems[i].User?.UserId == userId)
            {
                allList.Add(toDoItems[i]);
            }
        }
        return allList;
    }

    public void Update(ToDoItem item)
    {
        for (int i = 0; i < toDoItems.Count; i++)
        {
            if (toDoItems[i].Id == item.Id)
            {
                var itemToUpdate = toDoItems[i];
                itemToUpdate.Name = item.Name;
                break;
            }
        }
    }
}
}
