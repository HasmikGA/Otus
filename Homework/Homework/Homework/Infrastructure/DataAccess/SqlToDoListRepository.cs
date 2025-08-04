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
        public async Task Add(ToDoList list, CancellationToken ct)
        {
            using (var dbContext = this.factory.CreateDataContext())
            {
                var listModel = ModelMapper.MapToModel(list);
                await dbContext.InsertAsync(listModel);
            }
        }

        public async Task Delete(Guid id, CancellationToken ct)
        {
            using (var dbContext = this.factory.CreateDataContext())
            {
                var list = dbContext.ToDoLists.Where(i => i.Id == id);
                await dbContext.DeleteAsync(list);
            }
        }

        public async Task<bool> ExistsByName(Guid userId, string name, CancellationToken ct)
        {
            using (var dbContext = this.factory.CreateDataContext())

            {
                var result = await dbContext.ToDoLists.AnyAsync(i => i.UserId == userId && i.Name == name);
                return result;
            }
        }

        public async Task<ToDoList?> Get(Guid id, CancellationToken ct)
        {
            using (var dbContext = factory.CreateDataContext())
            {
                var list = await dbContext.ToDoLists
                    .LoadWith(i => i.User)
                    .FirstOrDefaultAsync(i => i.Id == id);

                return ModelMapper.MapFromModel(list);
            }
        }

        public async Task<IReadOnlyList<ToDoList>> GetByUserId(Guid userId, CancellationToken ct)
        {
            using (var dbContext = factory.CreateDataContext())
            {
                var lists = await dbContext.ToDoLists
                    .LoadWith(i => i.User)
                    .Where(i => i.UserId == userId)
                    .Select(i => ModelMapper.MapFromModel(i))
                    .ToListAsync();

                return lists;
            }
        }
    }
}
