using Otus.ToDoList.ConsoleBot.Types;
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
        public void Add(ToDoUser user)
        {
            users.Add(user);
        }

        public ToDoUser? GetUser(Guid userId)
        {
            for (int i = 0; i < users.Count; i++)
            {
                if (users[i].UserId == userId)
                {
                    return users[i];
                }
            }

            return null;
        }

        public ToDoUser? GetUserByTelegramUserId(long telegramUserId)
        {
            for (int i = 0; i < users.Count; i++)
            {
                if (users[i].TelegramUserId == telegramUserId)
                {
                    return users[i];
                }
            }

            return null;
        }
    }
}
