using TaskBot.Core.Entities;
using TaskBot.Core.Services;
using Telegram.Bot;
using Telegram.Bot.Types;

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
            
            switch (context.CurrentStep)
            {
                case null:
                    var user = await this.userService.GetUser(context.UserId, ct);
                    context.Data["User"] = user;
                    await bot.SendMessage(update.Message.Chat, "Enter the name of list", cancellationToken: ct);
                    context.CurrentStep = "Name";
                    return ScenarioResult.Transition;
                case "Name":
                    context.Data["Name"] = update.Message.Text;
                    this.toDoListService.Add((ToDoUser)context.Data["User"], context.Data["Name"].ToString(), ct);
                    return ScenarioResult.Completed;
            }

            return ScenarioResult.Transition;
        }
    }
}
