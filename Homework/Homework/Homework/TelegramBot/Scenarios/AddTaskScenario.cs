using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.ConstrainedExecution;
using System.Text;
using System.Threading.Tasks;
using TaskBot.Core.Entities;
using TaskBot.Core.Services;
using TaskBot.TelegramBot.Dto;
using Telegram.Bot;
using Telegram.Bot.Types;
using Telegram.Bot.Types.ReplyMarkups;

namespace TaskBot.TelegramBot.Scenarios
{
    internal class AddTaskScenario : IScenario
    {
        private readonly IUserService userService;
        private readonly IToDoService toDoService;
        private readonly IToDoListService toDoListService;

        public AddTaskScenario(IUserService userService, IToDoService toDoService, IToDoListService toDoListService)
        {
            this.userService = userService;
            this.toDoService = toDoService;
            this.toDoListService = toDoListService;
        }

        public bool CanHandle(ScenarioType scenario)
        {
            return scenario == ScenarioType.AddTask;
        }

        public async Task<ScenarioResult> HandleMessageAsync(ITelegramBotClient bot, ScenarioContext context, Update update, CancellationToken ct)
        {
            if(update.CallbackQuery != null)
            {
                var callbackDto = ToDoListCallbackDto.FromString(update.CallbackQuery.Data);
                var list = await this.toDoListService.Get(callbackDto.ToDoListId.Value, ct);
                var user = await this.userService.GetUser(context.UserId, ct);
                await toDoService.Add(user, context.Data["Name"].ToString(), (DateTime)context.Data["Deadline"], list, ct);

                await bot.SendMessage(update.Message.Chat, $"The task \"{context.Data["Name"]}\" has been added into the list {list.Name} ", cancellationToken: ct);
                return ScenarioResult.Completed;
            }

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

                    context.Data["Deadline"] = deadLine;

                    var user = await this.userService.GetUser(context.UserId, ct);
                    var lists = await this.toDoListService.GetUserLists(user.UserId, ct);
                    var reply = new InlineKeyboardMarkup(new[]
                    {
                        new[]
                        {
                            InlineKeyboardButton.WithCallbackData("Whithout list", new ToDoListCallbackDto { Action = "show", ToDoListId = null }.ToString()),
                        },
                        lists.Select(list => InlineKeyboardButton.WithCallbackData(list.Name, new ToDoListCallbackDto { Action = "addtask_list", ToDoListId = list.Id }.ToString()))
                    });
                    
                    await bot.SendMessage(update.Message.Chat, "Choose the list for task", replyMarkup:reply, cancellationToken: ct);
                    return ScenarioResult.Transition;
            }

            return ScenarioResult.Transition;
        }
    }
}
