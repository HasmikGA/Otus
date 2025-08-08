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
        public async Task Add(ToDoItem item, CancellationToken ct)
        {
            using (var dbContext = factory.CreateDataContext())
            {
                var model = ModelMapper.MapToModel(item);
                await dbContext.InsertAsync(model);
            };
        }

        public async Task<int> CountActive(Guid userId, CancellationToken ct)
        {
            using (var dbContext = factory.CreateDataContext())
            {
                var count = await dbContext.ToDoItems.CountAsync(x => x.State == 0);
                return count;
            }

        }
        public async Task Delete(Guid id, Guid userId, CancellationToken ct)
        {
            using (var dbContext = factory.CreateDataContext())
            {
                var models = dbContext.ToDoItems.Where(i => i.Id == id && i.UserId == userId);
                await dbContext.DeleteAsync(models);
            }
        }

        public async Task<bool> ExistsByName(Guid userId, string name, CancellationToken ct)
        {
            using (var dbContext = factory.CreateDataContext())
            {
                var result = await dbContext.ToDoItems.AnyAsync(i => i.UserId == userId && i.Name == name);
                return result;
            }
        }

        public async Task<IReadOnlyList<ToDoItem>> Find(Guid userId, Func<ToDoItem, bool> predicate, CancellationToken ct)
        {
            using (var dbContext = factory.CreateDataContext())
            {
                var predicateModels = await dbContext.ToDoItems
                    .LoadWith(i => i.User)
                    .LoadWith(i => i.List)
                    .Select(x => ModelMapper.MapFromModel(x))
                    .Where(i => i.User.UserId == userId && predicate(i))
                    .ToListAsync();

                return predicateModels;
            }
        }

        public async Task<IReadOnlyList<ToDoItem>> GetActiveByUserId(Guid userId, CancellationToken ct)
        {
            using (var dbContext = factory.CreateDataContext())
            {
                var activeModels = await dbContext.ToDoItems
                    .LoadWith(i => i.User)
                    .LoadWith(i => i.List)
                    .Where(i => i.UserId == userId && i.State == 0)
                    .Select(x => ModelMapper.MapFromModel(x))
                    .ToListAsync();

                return activeModels;
            }
        }

        public async Task<IReadOnlyList<ToDoItem>> GetAllByUserId(Guid userId, CancellationToken ct)
        {
            using (var dbContext = factory.CreateDataContext())
            {
                var allModels = await dbContext.ToDoItems
                    .LoadWith(i => i.User)
                    .LoadWith(i => i.List)
                    .Where(i => i.UserId == userId)
                    .Select(x => ModelMapper.MapFromModel(x))
                    .ToListAsync();

                return allModels;
            }
        }

        public async Task<IReadOnlyList<ToDoItem>> GetByUserIdAndList(Guid userId, Guid? listId, CancellationToken ct)
        {
            using (var dbContext = factory.CreateDataContext())
            {
                var models = await dbContext.ToDoItems
                    .LoadWith(i => i.User)
                    .LoadWith(i => i.List)
                    .Where(i => i.UserId == userId && i.ListId == listId)
                    .Select(x => ModelMapper.MapFromModel(x))
                    .ToListAsync();

                return models;
            }
        }

        public async Task Update(ToDoItem item, CancellationToken ct)
        {
            using (var dbContext = factory.CreateDataContext())
            {
                var model = ModelMapper.MapToModel(item);
               await dbContext.UpdateAsync(model);
            }
        }

        private async Task<IReadOnlyList<ToDoItem>> GetItems(Predicate<ToDoItem> predicate, CancellationToken ct)
        {
            using (var dbContext = factory.CreateDataContext())
            {
                var models =await dbContext.ToDoItems
                    .LoadWith(i => i.User)
                    .LoadWith(i => i.List)
                    .Select(x => ModelMapper.MapFromModel(x))
                    .Where(x => predicate(x))
                    .ToListAsync();

                return models;
            }
        }
    }
}
