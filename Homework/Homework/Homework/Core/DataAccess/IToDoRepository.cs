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
        Task<IReadOnlyList<ToDoItem>> Find(Guid userId, Func<ToDoItem, bool> predicate, CancellationToken ct);
        Task<IReadOnlyList<ToDoItem>> GetAllByUserId(Guid userId, CancellationToken ct);
        Task<IReadOnlyList<ToDoItem>> GetActiveByUserId(Guid userId, CancellationToken ct);
        void Add(ToDoItem item, CancellationToken ct);
        void Update(ToDoItem item, CancellationToken ct);
        void Delete(Guid id, CancellationToken ct);
        bool ExistsByName(Guid userId, string name, CancellationToken ct);
        Task<int> CountActive(Guid userId, CancellationToken ct);
        Task<IReadOnlyList<ToDoItem>> GetByUserIdAndList(Guid userId, Guid? listId, CancellationToken ct);



    }
}
