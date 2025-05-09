using TaskBot.Core.DataAccess;
using TaskBot.Core.Entities;

namespace TaskBot.Core.Services
{
    internal class UserService : IUserService
    {
        private readonly IUserRepository userRepository;

        public UserService(IUserRepository userRepository)
        {
            this.userRepository = userRepository;
        }

        public async Task<ToDoUser>? GetUser(long telegramUserId, CancellationToken ct)
        {
            var user = userRepository.GetUserByTelegramUserId(telegramUserId, ct);
            return user;
        }

        public async Task<ToDoUser> RegisterUser(long telegramUserId, string telegramUserName, CancellationToken ct)
        {
            var user = new ToDoUser
            {
                UserId = Guid.NewGuid(),
                TelegramUserId = telegramUserId,
                TelegramUserName = telegramUserName,
                RegisteredAt = DateTime.Now,
            };

            userRepository.Add(user, ct);

            return user;
        }
    }


}

