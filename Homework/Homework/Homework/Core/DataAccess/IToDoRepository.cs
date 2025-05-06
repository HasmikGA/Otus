using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TaskBot.Core.Entities;

namespace TaskBot.Core.DataAccess
{
    internal interface IToDoRepository
    {
        IReadOnlyList<ToDoItem> Find(Guid userId, Func<ToDoItem, bool> predicate);
        IReadOnlyList<ToDoItem> GetAllByUserId(Guid userId);
        IReadOnlyList<ToDoItem> GetActiveByUserId(Guid userId);
        void Add(ToDoItem item);
        void Update(ToDoItem item);
        void Delete(Guid id);
        bool ExistsByName(Guid userId, string name);
        int CountActive(Guid userId);


    }
}
