using LinqToDB;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TaskBot.Core.DataAccess;
using TaskBot.Core.Entities;
using Telegram.Bot.Types;

namespace TaskBot.Infrastructure.DataAccess
{
    internal class SqlUserRepository : IUserRepository
    {
        private readonly IDataContextFactory<ToDoDataContext> factory;
        public SqlUserRepository(IDataContextFactory<ToDoDataContext> factory)
        {
            this.factory = factory;
        }
        public void Add(ToDoUser user, CancellationToken ct)
        {
            using (var dbContext = this.factory.CreateDataContext())
            {
                var userModel = ModelMapper.MapToModel(user);
                dbContext.Insert(userModel);
            }
        }

        public ToDoUser? GetUser(Guid userId, CancellationToken ct)
        {
            using (var dbContext = this.factory.CreateDataContext())
            {
                var user = dbContext.ToDoUsers.FirstOrDefault(i => i.UserId == userId);
                return ModelMapper.MapFromModel(user);
            }

        }

        public ToDoUser? GetUserByTelegramUserId(long telegramUserId, CancellationToken ct)
        {
            using (var dbContext = factory.CreateDataContext())
            {
                var user = dbContext.ToDoUsers.FirstOrDefault(i => i.TelegramUserId == telegramUserId);
                return ModelMapper.MapFromModel(user);
            }
        }
    }
}
