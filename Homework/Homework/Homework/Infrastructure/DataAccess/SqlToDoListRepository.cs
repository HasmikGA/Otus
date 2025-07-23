using LinqToDB;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TaskBot.Core.DataAccess;
using TaskBot.Core.Entities;

namespace TaskBot.Infrastructure.DataAccess
{
    internal class SqlToDoListRepository : IToDoListRepository
    {
        private readonly IDataContextFactory<ToDoDataContext> factory;
        public SqlToDoListRepository(IDataContextFactory<ToDoDataContext> factory)
        {
            this.factory = factory;
        }
        public Task Add(ToDoList list, CancellationToken ct)
        {
            using (var dbContext = this.factory.CreateDataContext())
            {
                var listModel = ModelMapper.MapToModel(list);
                dbContext.Insert(listModel);
            }

            return Task.CompletedTask;
        }

        public Task Delete(Guid id, CancellationToken ct)
        {
            using (var dbContext = this.factory.CreateDataContext())
            {
                var list = dbContext.ToDoLists.Where(i => i.Id == id);
                dbContext.Delete(list);
            }

            return Task.CompletedTask;
        }

        public Task<bool> ExistsByName(Guid userId, string name, CancellationToken ct)
        {
            using (var dbContext = this.factory.CreateDataContext())

            {
                var result = dbContext.ToDoLists.Any(i => i.UserId == userId && i.Name == name);
                return Task.FromResult(result);
            }
        }

        public Task<ToDoList?> Get(Guid id, CancellationToken ct)
        {
            using (var dbContext = factory.CreateDataContext())
            {
                var list = dbContext.ToDoLists
                    .LoadWith(i => i.User)
                    .FirstOrDefault(i => i.Id == id);

                return Task.FromResult<ToDoList?>(ModelMapper.MapFromModel(list));
            }
        }

        public Task<IReadOnlyList<ToDoList>> GetByUserId(Guid userId, CancellationToken ct)
        {
            using (var dbContext = factory.CreateDataContext())
            {
                var lists = dbContext.ToDoLists
                    .LoadWith(i => i.User)
                    .Where(i => i.UserId == userId)
                    .Select(i => ModelMapper.MapFromModel(i))
                    .ToList();

                return Task.FromResult<IReadOnlyList<ToDoList>>(lists);
            }
        }
    }
}
