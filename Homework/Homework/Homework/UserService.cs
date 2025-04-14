namespace TaskBot
{
    internal class UserService : IUserService
    {
        private readonly List<ToDoUser> users = new List<ToDoUser>();

        public ToDoUser? GetUser(long telegramUserId)
        {
            for (int i = 0; i < this.users.Count; i++)
            {
                if (this.users[i].TelegramUserId == telegramUserId)
                {
                    return this.users[i];
                }
            }

            return null;
        }

        public ToDoUser RegisterUser(long telegramUserId, string telegramUserName)
        {
            var user = new ToDoUser
            {
                UserId = Guid.NewGuid(),
                TelegramUserId = telegramUserId,
                TelegramUserName = telegramUserName,
                RegisteredAt = DateTime.Now,
            };

            this.users.Add(user);

            return user;
        }
    }


}
