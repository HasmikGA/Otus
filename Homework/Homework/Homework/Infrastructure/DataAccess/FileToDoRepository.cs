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

        public FileToDoRepository(string itemFolederName)
        {
            ItemFolderName = itemFolederName;

            if (!Directory.Exists(Path.Combine(itemFolederName)))
            {
                Directory.CreateDirectory(itemFolederName);
            }
        }

        public void Add(ToDoItem item, CancellationToken ct)
        {
            string itemSubFolderName = $"{item?.User?.UserId}";

            var pathFolder = Path.Combine(this.ItemFolderName, itemSubFolderName);

            Directory.CreateDirectory(pathFolder);

            string itemFileName = $"{item?.Id}.json";

            var pathFile = Path.Combine(pathFolder, itemFileName);

            using (File.Create(pathFile))
            {
                var json = JsonSerializer.Serialize(item);
                File.WriteAllText(pathFile, json);
            }

        }

        public async Task<int> CountActive(Guid userId, CancellationToken ct)
        {
            IReadOnlyList<ToDoItem> activetList = await GetActiveByUserId(userId, ct);
            return activetList.Count;
        }

        public void Delete(Guid id, CancellationToken ct)
        {
            var path = Path.Combine(ItemFolderName, $"{id}.json");
            if (File.Exists(path))
            {
                File.Delete(path);
            }

        }
        public bool ExistsByName(Guid userId, string name, CancellationToken ct)
        {
            var path = Path.Combine(ItemFolderName);

            if (Directory.Exists(path))
            {
                var files = Directory.GetFiles(path);

                for (int i = 0; i < files.Length; i++)
                {
                    var toDoItemJson = File.ReadAllText(files[i]);
                    var item = JsonSerializer.Deserialize<ToDoItem>(toDoItemJson);
                    if (item != null && item.User.UserId == userId && item.Name == name)
                    {
                        return true;
                    }
                }
            }
            return false;
        }

        public async Task<IReadOnlyList<ToDoItem>> Find(Guid userId, Func<ToDoItem, bool> predicate, CancellationToken ct)
        {
            List<ToDoItem> pridicateList = new List<ToDoItem>();

            var path = Path.Combine(ItemFolderName);

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

            return pridicateList;
        }

        public async Task<IReadOnlyList<ToDoItem>> GetActiveByUserId(Guid userId, CancellationToken ct)
        {
            List<ToDoItem> activetList = new List<ToDoItem>();

            var path = Path.Combine(ItemFolderName);

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

            return activetList;
        }

        public async Task<IReadOnlyList<ToDoItem>> GetAllByUserId(Guid userId, CancellationToken ct)
        {

            List<ToDoItem> allList = new List<ToDoItem>();

            var path = Path.Combine(ItemFolderName);

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
            return allList;
        }
        public async Task<IReadOnlyList<ToDoItem>> GetByUserIdAndList(Guid userId, Guid? listId, CancellationToken ct)
        {
            var toDoItemList = new List<ToDoItem>();
            var path = Path.Combine(ItemFolderName);
            if (Directory.Exists(path))
            {
                var files = Directory.GetFiles(path);
                for(int i  = 0; i < files.Length;i++)
                {
                    var toDoItemListJson = File.ReadAllText(files[i]);
                    var item = JsonSerializer.Deserialize<ToDoItem>(toDoItemListJson);
                    if(item?.User?.UserId == userId && item?.List?.Id == listId)
                    {
                        toDoItemList.Add(item);
                    }
                }
            }
            return toDoItemList;
        }
        public void Update(ToDoItem item, CancellationToken ct)
        {
            Add(item, ct);
        }
    }
}
