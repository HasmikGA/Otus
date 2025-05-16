using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;
using TaskBot.Core.DataAccess;
using TaskBot.Core.Entities;
using Telegram.Bot.Types;

namespace TaskBot.Infrastructure.DataAccess
{
    internal class FileUserRepository : IUserRepository
    {
        private string UserFolderName { get; set; }
        public FileUserRepository(string folderNameUser)
        {
            UserFolderName = folderNameUser;
            if (!Directory.Exists(Path.Combine(folderNameUser)))
            {
                Directory.CreateDirectory(folderNameUser);
            }
        }
        public void Add(ToDoUser user, CancellationToken ct)
        {
            string fileNameUser = $"{user.UserId}.json";
            var path = Path.Combine(UserFolderName, fileNameUser);

            using (File.Create(path)) ;

            var json = JsonSerializer.Serialize(user);

            File.WriteAllText(path, json);
        }

        public ToDoUser? GetUser(Guid userId, CancellationToken ct)
        {
            var path = Path.Combine(UserFolderName);
            if (Directory.Exists(path))
            {
                var users = Directory.GetFiles(path);

                for (int i = 0; i < users.Length; i++)
                {
                    var toDoUserJson = File.ReadAllText(users[i]);
                    var user = JsonSerializer.Deserialize<ToDoUser>(toDoUserJson);
                    if (user?.UserId == userId)
                    {
                        return user;
                    }
                }
            }

            return null;
        }

        public ToDoUser? GetUserByTelegramUserId(long telegramUserId, CancellationToken ct)
        {
            var path = Path.Combine(UserFolderName);
            if (Directory.Exists(path))
            {
                var users = Directory.GetFiles(path);

                for (int i = 0; i < users.Length; i++)
                {
                    var toDoUserJson = File.ReadAllText(users[i]);
                    var user = JsonSerializer.Deserialize<ToDoUser>(toDoUserJson);
                    if (user?.TelegramUserId == telegramUserId)
                    {
                        return user;
                    }
                }
            }

            return null;
        }
    }
}
