using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TaskBot.Core.DataAccess;
using TaskBot.Core.Entities;
using System.Text.Json;
using System.IO;

namespace TaskBot.Infrastructure.DataAccess
{
    internal class FileToDoRepository : IToDoRepository
    {

        private string ItemFolderName { get; set; }

        public FileToDoRepository(string itemFolderName)
        {
            ItemFolderName = itemFolderName;

            if (!Directory.Exists(Path.Combine(ItemFolderName)))
            {
                Directory.CreateDirectory(itemFolderName);
            }
        }

        public Task Add(ToDoItem item, CancellationToken ct)
        {

            string itemSubFolderName = $"{item?.User?.UserId}";

            var pathFolder = Path.Combine(this.ItemFolderName, itemSubFolderName);

            Directory.CreateDirectory(pathFolder);

            string itemFileName = $"{item?.Id}.json";

            var pathFile = Path.Combine(pathFolder, itemFileName);

            var json = JsonSerializer.Serialize(item);
            File.WriteAllText(pathFile, json);

            return Task.CompletedTask;
        }

        public async Task<int> CountActive(Guid userId, CancellationToken ct)
        {
            IReadOnlyList<ToDoItem> activetList = await GetActiveByUserId(userId, ct);
            return activetList.Count;
        }

        public Task Delete(Guid id, Guid userId, CancellationToken ct)
        {
            var userFolder = Path.Combine(ItemFolderName, userId.ToString());
            var path = Path.Combine(userFolder, $"{id}.json");

            if (File.Exists(path))
            {
                File.Delete(path);
            }
            return Task.CompletedTask;  
        }
        public Task<bool> ExistsByName(Guid userId, string name, CancellationToken ct)
        {
            var path = Path.Combine(ItemFolderName, userId.ToString());

            if (Directory.Exists(path))
            {
                var files = Directory.GetFiles(path);

                for (int i = 0; i < files.Length; i++)
                {
                    var toDoItemJson = File.ReadAllText(files[i]);
                    var item = JsonSerializer.Deserialize<ToDoItem>(toDoItemJson);
                    if (item != null && item?.User?.UserId == userId && item.Name == name)
                    {
                        return Task.FromResult(true);
                    }
                }
            }
            return Task.FromResult(false);
        }

        public Task<IReadOnlyList<ToDoItem>> Find(Guid userId, Func<ToDoItem, bool> predicate, CancellationToken ct)
        {
            List<ToDoItem> pridicateList = new List<ToDoItem>();

            var path = Path.Combine(ItemFolderName, userId.ToString());

            if (Directory.Exists(path))
            {
                var files = Directory.GetFiles(path);
                for (int i = 0; i < files.Length; i++)
                {
                    var toDoItemJson = File.ReadAllText(files[i]);
                    var item = JsonSerializer.Deserialize<ToDoItem>(toDoItemJson);

                    if (item?.User?.UserId == userId && predicate(item))
                    {
                        pridicateList.Add(item);
                    }
                }
            }

            return Task.FromResult<IReadOnlyList<ToDoItem>>(pridicateList);
        }

        public Task<IReadOnlyList<ToDoItem>> GetActiveByUserId(Guid userId, CancellationToken ct)
        {
            List<ToDoItem> activetList = new List<ToDoItem>();

            var path = Path.Combine(ItemFolderName, userId.ToString());

            if (Directory.Exists(path))
            {
                var files = Directory.GetFiles(path);
                for (int i = 0; i < files.Length; i++)
                {
                    var toDoItemJson = File.ReadAllText(files[i]);
                    var item = JsonSerializer.Deserialize<ToDoItem>(toDoItemJson);
                    if (item?.User?.UserId == userId && item.State == ToDoItemState.Active)
                    {
                        activetList.Add(item);
                    }
                }

            }

            return Task.FromResult<IReadOnlyList<ToDoItem>>(activetList);
        }

        public Task<IReadOnlyList<ToDoItem>> GetAllByUserId(Guid userId, CancellationToken ct)
        {

            List<ToDoItem> allList = new List<ToDoItem>();

            var path = Path.Combine(ItemFolderName, userId.ToString());

            if (Directory.Exists(path))
            {
                var files = Directory.GetFiles(path);
                for (int i = 0; i < files.Length; i++)
                {
                    var toDoItemJson = File.ReadAllText(files[i]);
                    var item = JsonSerializer.Deserialize<ToDoItem>(toDoItemJson);
                    if (item?.User?.UserId == userId)
                    {
                        allList.Add(item);
                    }
                }

            }
            return Task.FromResult<IReadOnlyList<ToDoItem>> (allList);
        }
        public Task<IReadOnlyList<ToDoItem>> GetByUserIdAndList(Guid userId, Guid? listId, CancellationToken ct)
        {
            var toDoItemList = new List<ToDoItem>();

            var path = Path.Combine(ItemFolderName, userId.ToString());
            if (Directory.Exists(path))
            {
                var files = Directory.GetFiles(path);
                for (int i = 0; i < files.Length; i++)
                {
                    var toDoItemListJson = File.ReadAllText(files[i]);
                    var item = JsonSerializer.Deserialize<ToDoItem>(toDoItemListJson);
                    if (item?.User?.UserId == userId && (!listId.HasValue || listId.Value == Guid.Empty || item?.List?.Id == listId))
                    {
                        toDoItemList.Add(item);
                    }
                }
            }
            return Task.FromResult<IReadOnlyList<ToDoItem>>(toDoItemList);
        }
        public Task Update(ToDoItem item, CancellationToken ct)
        {
            Add(item, ct);
            return Task.CompletedTask;
        }
    }
}
