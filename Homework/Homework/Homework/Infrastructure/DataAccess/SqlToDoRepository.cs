using LinqToDB;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TaskBot.Core.DataAccess;
using TaskBot.Core.DataAccess.Models;
using TaskBot.Core.Entities;


namespace TaskBot.Infrastructure.DataAccess
{
    internal class SqlToDoRepository : IToDoRepository
    {
        private readonly IDataContextFactory<ToDoDataContext> factory;
        public SqlToDoRepository(IDataContextFactory<ToDoDataContext> factory)
        {
            this.factory = factory;
        }
        public void Add(ToDoItem item, CancellationToken ct)
        {
            using (var dbContext = factory.CreateDataContext())
            {
                var model = ModelMapper.MapToModel(item);
                dbContext.Insert(model);
            };
        }

        public Task<int> CountActive(Guid userId, CancellationToken ct)
        {
            using (var dbContext = factory.CreateDataContext())
            {
                var count = dbContext.ToDoItems.Count(x => x.State == 0);
                return Task.FromResult(count);
            }

        }
        public void Delete(Guid id, Guid userId, CancellationToken ct)
        {
            using (var dbContext = factory.CreateDataContext())
            {
                var models = dbContext.ToDoItems.Where(i => i.Id == id && i.UserId == userId);
                dbContext.Delete(models);
            }
        }

        public bool ExistsByName(Guid userId, string name, CancellationToken ct)
        {
            using (var dbContext = factory.CreateDataContext())
            {
                var result = dbContext.ToDoItems.Any(i => i.UserId == userId && i.Name == name);
                return result;
            }
        }

        public Task<IReadOnlyList<ToDoItem>> Find(Guid userId, Func<ToDoItem, bool> predicate, CancellationToken ct)
        {
            using (var dbContext = factory.CreateDataContext())
            {
                var predicateModels = dbContext.ToDoItems
                    .LoadWith(i => i.User)
                    .LoadWith(i => i.List)
                    .Select(x => ModelMapper.MapFromModel(x))
                    .Where(i => i.User.UserId == userId && predicate(i))
                    .ToList();

                return Task.FromResult<IReadOnlyList<ToDoItem>>(predicateModels);
            }
        }

        public Task<IReadOnlyList<ToDoItem>> GetActiveByUserId(Guid userId, CancellationToken ct)
        {
            using (var dbContext = factory.CreateDataContext())
            {
                var activeModels = dbContext.ToDoItems
                    .LoadWith(i => i.User)
                    .LoadWith(i => i.List)
                    .Where(i => i.UserId == userId && i.State == 0)
                    .Select(x => ModelMapper.MapFromModel(x))
                    .ToList();

                return Task.FromResult<IReadOnlyList<ToDoItem>>(activeModels);
            }
        }

        public Task<IReadOnlyList<ToDoItem>> GetAllByUserId(Guid userId, CancellationToken ct)
        {
            using (var dbContext = factory.CreateDataContext())
            {
                var allModels = dbContext.ToDoItems
                    .LoadWith(i => i.User)
                    .LoadWith(i => i.List)
                    .Where(i => i.UserId == userId)
                    .Select(x => ModelMapper.MapFromModel(x))
                    .ToList();

                return Task.FromResult<IReadOnlyList<ToDoItem>>(allModels);
            }
        }

        public Task<IReadOnlyList<ToDoItem>> GetByUserIdAndList(Guid userId, Guid? listId, CancellationToken ct)
        {
            using (var dbContext = factory.CreateDataContext())
            {
                var models = dbContext.ToDoItems
                    .LoadWith(i => i.User)
                    .LoadWith(i => i.List)
                    .Where(i => i.UserId == userId && i.ListId == listId)
                    .Select(x => ModelMapper.MapFromModel(x))
                    .ToList();

                return Task.FromResult<IReadOnlyList<ToDoItem>>(models);
            }
        }

        public void Update(ToDoItem item, CancellationToken ct)
        {
            using (var dbContext = factory.CreateDataContext())
            {
                var model = ModelMapper.MapToModel(item);
                dbContext.Update(model);
            }
        }

        private Task<IReadOnlyList<ToDoItem>> GetItems(Predicate<ToDoItem> predicate, CancellationToken ct)
        {
            using (var dbContext = factory.CreateDataContext())
            {
                var models = dbContext.ToDoItems
                    .LoadWith(i => i.User)
                    .LoadWith(i => i.List)
                    .Select(x => ModelMapper.MapFromModel(x))
                    .Where(x => predicate(x))
                    .ToList();

                return Task.FromResult<IReadOnlyList<ToDoItem>>(models);
            }
        }
    }
}
