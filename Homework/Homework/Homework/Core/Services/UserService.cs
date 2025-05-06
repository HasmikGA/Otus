using TaskBot.Core.DataAccess;
using TaskBot.Core.Entities;

namespace TaskBot.Core.Services
{
    internal class UserService : IUserService
    {
        private readonly List<ToDoUser> users = new List<ToDoUser>();
        private readonly IUserRepository userRepository;

        public UserService(IUserRepository userRepository)
        {
            this.userRepository = userRepository;
        }
        public ToDoUser? GetUser(long telegramUserId)
        {
            var user = userRepository.GetUserByTelegramUserId(telegramUserId);
            return user;    
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

            userRepository.Add(user);

            return user;
        }
    }


}
