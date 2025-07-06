using System.Text.Json;
using TaskBot.Core.DataAccess;
using TaskBot.Core.Entities;

namespace TaskBot.Infrastructure.DataAccess
{
    internal class FileToDoListRepository : IToDoListRepository
    {
        private readonly string listFoldername;
        public FileToDoListRepository(string listFoldername)
        {
            this.listFoldername = listFoldername;

            if (!Directory.Exists(listFoldername))
            {
                Directory.CreateDirectory(listFoldername);
            }
        }

        public Task Add(ToDoList list, CancellationToken ct)
        {
            string fileNameList = $"{list.Id}.json";
            var path = Path.Combine(listFoldername, fileNameList);

            var jsonList = JsonSerializer.Serialize(list);

            File.WriteAllText(path, jsonList);

            return Task.CompletedTask;
        }

        public Task Delete(Guid id, CancellationToken ct)
        {
            if (!Directory.Exists(listFoldername))
            {
                return Task.CompletedTask;
            }

            var lists = Directory.GetFiles(listFoldername);
            for (int i = 0; i < lists.Length; i++)
            {
                var toDoListJson = File.ReadAllText(lists[i]);
                var list = JsonSerializer.Deserialize<ToDoList>(toDoListJson);
                if (list?.Id == id)
                {
                    File.Delete(lists[i]);
                    return Task.FromResult(list);
                }
            }

            return Task.CompletedTask;
        }

        public Task<bool> ExistsByName(Guid userId, string name, CancellationToken ct)
        {
            var path = Path.Combine(this.listFoldername);
            if (!Directory.Exists(path))
            {
                return Task.FromResult(false);
            }

            var lists = Directory.GetFiles(path);
            foreach (var list in lists)
            {
                var toDoListJson = File.ReadAllText(list);
                var toDoList = JsonSerializer.Deserialize<ToDoList>(toDoListJson);
                if (toDoList?.User.UserId == userId && toDoList.Name == name)
                {
                    return Task.FromResult(true);
                }
            }

            return Task.FromResult(false);
        }

        public Task<ToDoList?> Get(Guid id, CancellationToken ct)
        {
            var path = Path.Combine(listFoldername);

            if (Directory.Exists(path))
            {
                var lists = Directory.GetFiles(path);

                for (int i = 0; i < lists.Length; i++)
                {
                    var toDoListJson = File.ReadAllText(lists[i]);
                    var list = JsonSerializer.Deserialize<ToDoList>(toDoListJson);
                    if (list?.Id == id)
                    {
                        return Task.FromResult<ToDoList?>(list);
                    }
                }
            }

            return Task.FromResult<ToDoList?>(null);
        }

        public Task<IReadOnlyList<ToDoList>> GetByUserId(Guid userId, CancellationToken ct)
        {
            var path = listFoldername;
            var userToDoList = new List<ToDoList>();

            if (Directory.Exists(path))
            {
                var lists = Directory.GetFiles(path);

                for (int i = 0; i < lists.Length; i++)
                {
                    var toDoListJson = File.ReadAllText(lists[i]);
                    var list = JsonSerializer.Deserialize<ToDoList>(toDoListJson);
                    if (list?.User.UserId == userId)
                    {
                        userToDoList.Add(list);
                    }
                }
            }

            return Task.FromResult<IReadOnlyList<ToDoList>>(userToDoList);
        }
    }
}
