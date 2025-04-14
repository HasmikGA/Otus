using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TaskBot
{
    internal interface IToDoService
    {
        IReadOnlyList<ToDoItem> GetActiveByUserId(Guid userId);
        IReadOnlyList<ToDoItem> GetByUserId(Guid userId);
        ToDoItem Add(ToDoUser user, string name);
        void MarkCompleted(Guid id);
        void Delete(Guid id);
    }
}
