using TaskBot.Core.Entities;
using TaskBot.Core.Services;
using Telegram.Bot;
using Telegram.Bot.Types;
using Telegram.Bot.Types.Enums;

namespace TaskBot.TelegramBot.Scenarios
{
    internal class AddListScenario : IScenario

    {
        private readonly IUserService userService;
        private readonly IToDoListService toDoListService;
        public AddListScenario(IUserService userService, IToDoListService toDoListService)
        {
            this.userService = userService;
            this.toDoListService = toDoListService;
        }

        public bool CanHandle(ScenarioType scenario)
        {
            return scenario == ScenarioType.AddList;
        }

        public async Task<ScenarioResult> HandleMessageAsync(ITelegramBotClient bot, ScenarioContext context, Update update, CancellationToken ct)
        {
            var message = update.Type == UpdateType.CallbackQuery ? update.CallbackQuery.Message : update.Message;

            switch (context.CurrentStep)
            {
                case null:
                    var user = await this.userService.GetUser(message.Chat.Id, ct);
                    context.Data["User"] = user;
                    await bot.SendMessage(message.Chat, "Enter the name of list", cancellationToken: ct);
                    context.CurrentStep = "Name";
                    return ScenarioResult.Transition;
                case "Name":
                    context.Data["Name"] = message.Text;
                    this.toDoListService.Add((ToDoUser)context.Data["User"], context.Data["Name"].ToString(), ct);
                    return ScenarioResult.Completed;
            }

            return ScenarioResult.Transition;
        }
    }
}
