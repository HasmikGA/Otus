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

        async Task<ToDoUser>? IUserService.GetUser(long telegramUserId, CancellationToken ct)
        {
            var user = await userRepository.GetUserByTelegramUserId(telegramUserId, ct);
            return user;
        }

        async Task<ToDoUser> IUserService.RegisterUser(long telegramUserId, string telegramUserName, CancellationToken ct)
        {
            var user = new ToDoUser
            {
                UserId = Guid.NewGuid(),
                TelegramUserId = telegramUserId,
                TelegramUserName = telegramUserName,
                RegisteredAt = DateTime.Now,
            };

            await userRepository.Add(user, ct);

            return user;
        }
    }


}

