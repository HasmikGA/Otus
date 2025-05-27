using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TaskBot.Core.Services;
using Telegram.Bot;
using Telegram.Bot.Types;

namespace TaskBot.TelegramBot.Scenarios
{
    internal class AddTaskScenario : IScenario
    {
        private readonly IUserService userService;
        private readonly IToDoService toDoService;

        public ScenarioType Type => ScenarioType.AddTask;

        public AddTaskScenario(IUserService userService, IToDoService toDoService)
        {
            this.userService = userService;
            this.toDoService = toDoService;
        }
        public bool CanHandle(ScenarioType scenario)
        {
            throw new NotImplementedException();
        }

        public async Task<ScenarioResult> HandleMessageAsync(ITelegramBotClient bot, ScenarioContext context, Update update, CancellationToken ct)
        {
            switch (context.CurrentStep)
            {
                case null:
                    context.UserId = update.Message.From.Id;
                    await bot.SendMessage(update.Message.Chat, "Enter the name of task", cancellationToken: ct);
                    context.CurrentStep = "Name";
                    return ScenarioResult.Transition;
                case "Name":
                    context.CurrentStep = "Deadline";
                    context.Data["Name"] = update.Message.Text;
                    await bot.SendMessage(update.Message.Chat, $"Enter the deadline of the task in this format dd.MM.yyyy.", cancellationToken: ct);
                    return ScenarioResult.Transition;
                case "Deadline":
                    if (!DateTime.TryParse(update.Message.Text, out var deadLine))
                    {
                        await bot.SendMessage(update.Message.Chat, "Please try again whith correct format dd.MM.yyyy.", cancellationToken: ct);
                        return ScenarioResult.Transition;
                    }

                    var user = await this.userService.GetUser(context.UserId, ct);
                    await toDoService.Add(user, context.Data["Name"].ToString(), deadLine, ct);
                    await bot.SendMessage(update.Message.Chat, $"The task \"{update.Message.Text}\" has been added.", cancellationToken: ct);
                    return ScenarioResult.Completed;
            }

            return ScenarioResult.Transition;
        }
    }
}
