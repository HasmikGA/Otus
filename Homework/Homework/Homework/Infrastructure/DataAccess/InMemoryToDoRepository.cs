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
        public Task Add(ToDoItem item, CancellationToken ct)
        {
            toDoItems.Add(item);
            return Task.CompletedTask;
        }

        public async Task<int> CountActive(Guid userId, CancellationToken ct)
        {
            IReadOnlyList<ToDoItem> activetList = await GetActiveByUserId(userId, ct);
            return activetList.Count;
        }

        public Task Delete(Guid id, Guid userId, CancellationToken ct)
        {
            for (int i = 0; i < toDoItems.Count; i++)
            {
                if (toDoItems[i].Id == id)
                {
                    toDoItems.RemoveAt(i);
                    break;
                }
            }
            return Task.CompletedTask;
        }
        public Task<bool> ExistsByName(Guid userId, string name, CancellationToken ct)
        {
            for (int i = 0; i < toDoItems.Count; i++)
            {
                if (toDoItems[i].User?.UserId == userId && toDoItems[i].Name == name)
                {
                    return Task.FromResult(true);
                }
            }
            return Task.FromResult(false);
        }

        public async Task<IReadOnlyList<ToDoItem>> Find(Guid userId, Func<ToDoItem, bool> predicate, CancellationToken ct)
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

        public async Task<IReadOnlyList<ToDoItem>> GetActiveByUserId(Guid userId, CancellationToken ct)
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

        public async Task<IReadOnlyList<ToDoItem>> GetAllByUserId(Guid userId, CancellationToken ct)
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
        public async Task<IReadOnlyList<ToDoItem>> GetByUserIdAndList(Guid userId, Guid? listId, CancellationToken ct)
        {
            var toDoItemList = new List<ToDoItem>();
            
            for (int i = 0; i < toDoItems.Count; i++)
            {
                if (toDoItems[i].User?.UserId == userId && toDoItems[i]?.List?.Id == listId)
                {
                    toDoItemList.Add(toDoItems[i]);
                }
            }
            return toDoItemList;
        }

        public Task Update(ToDoItem item, CancellationToken ct)
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
            return Task.CompletedTask;
        }
    }
}
