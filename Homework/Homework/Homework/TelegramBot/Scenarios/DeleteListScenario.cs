using System;
using System.Collections.Generic;
using System.Linq;
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
    internal class DeleteListScenario : IScenario
    {
        private readonly IUserService userService;
        private readonly IToDoListService toDoListService;
        private readonly IToDoService toDoService;
        public DeleteListScenario(IUserService userService, IToDoListService toDoListService, IToDoService toDoService)
        {
            this.userService = userService;
            this.toDoListService = toDoListService;
            this.toDoService = toDoService;
        }

        public bool CanHandle(ScenarioType scenario)
        {
            return scenario == ScenarioType.DeleteList;
        }

        public async Task<ScenarioResult> HandleMessageAsync(ITelegramBotClient bot, ScenarioContext context, Update update, CancellationToken ct)
        {
            switch (context.CurrentStep)
            {
                case null:
                    var user = await this.userService.GetUser(context.UserId, ct);
                    var lists = await this.toDoListService.GetUserLists(user.UserId, ct);
                    context.Data["User"] = user;
                    var reply = new InlineKeyboardMarkup(new[]
                    {
                        lists.Select(list => InlineKeyboardButton.WithCallbackData(list.Name, new ToDoListCallbackDto { Action = "deletelist", ToDoListId = list.Id }.ToString()))
                    });

                    await bot.SendMessage(update.Message.Chat, "Choose the list to delete", replyMarkup: reply, cancellationToken: ct);
                    context.CurrentStep = "Approve";
                    return ScenarioResult.Transition;
                case "Approve":
                    var list = await this.toDoListService.Get(((ToDoUser)context.Data["User"]).UserId, ct);
                    context.Data["List"] = list;

                    var inLine = new InlineKeyboardMarkup(new[]
                    {
                        new[]
                        {
                            InlineKeyboardButton.WithCallbackData("Yes","yes"),
                            InlineKeyboardButton.WithCallbackData("No", "no"),
                        }
                    });

                    await bot.SendMessage(update.Message.Chat, $"Confirm deletion of the list {list.Name} and all it`s tasks ", cancellationToken: ct);
                    context.CurrentStep = "Delete";
                    return ScenarioResult.Transition;
                case "Delete":
                    if (update.CallbackQuery.Data == "yes")
                    {
                        var userId = ((ToDoUser)context.Data["User"]).UserId;
                        var listId = ((ToDoList)context.Data["List"]).Id;
                        await this.toDoListService.Delete(listId, ct);
                        var items = await this.toDoService.GetByUserIdAndList(userId, listId, ct);
                        for(int i = 0; i < items.Count; i++)
                        {
                            await this.toDoService.Delete(items[i].Id, ct);
                        }
                        
                    }
                    if (update.CallbackQuery.Data == "no")
                    {
                        await bot.SendMessage(update.Message.Chat, "Deletion has been canceled ", cancellationToken: ct);
                    }
                    return ScenarioResult.Completed;


            }
            return ScenarioResult.Transition;
        }
    }
}
