
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TaskBot.Core.DataAccess;
using TaskBot.Core.Entities;

namespace TaskBot.Infrastructure.DataAccess
{
    internal class InMemoryUserRepository : IUserRepository
    {
        private readonly List<ToDoUser> users = new List<ToDoUser>();
        public Task Add(ToDoUser user, CancellationToken ct)
        {
            for (var i = 0; i < users.Count; i++)
            {
                if (users[i].TelegramUserId == user.TelegramUserId)
                {
                    return Task.CompletedTask;
                }
            }
            users.Add(user);
            return Task.CompletedTask;
        }

        public Task <ToDoUser?> GetUser(Guid userId, CancellationToken ct)
        {
            for (int i = 0; i < users.Count; i++)
            {
                if (users[i].UserId == userId)
                {
                    return Task.FromResult(users[i]);
                }
            }
            return null;
        }

        public Task <ToDoUser?> GetUserByTelegramUserId(long telegramUserId, CancellationToken ct)
        {
            for (int i = 0; i < users.Count; i++)
            {
                if (users[i].TelegramUserId == telegramUserId)
                {
                    return Task.FromResult(users[i]);
                }
            }

            return null;
        }
    }
}
