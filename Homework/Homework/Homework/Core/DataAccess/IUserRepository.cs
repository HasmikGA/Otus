using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TaskBot.Core.Entities;

namespace TaskBot.Core.DataAccess
{
    internal interface IUserRepository
    {
        ToDoUser? GetUser(Guid userId, CancellationToken ct);
        ToDoUser? GetUserByTelegramUserId(long telegramUserId, CancellationToken ct);
        void Add(ToDoUser user, CancellationToken ct);
    }
}
