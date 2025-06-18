
using System;
using System.Diagnostics;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Threading;
using TaskBot.Core.Entities;
using TaskBot.Core.Exceptions;
using TaskBot.Core.Services;
using Telegram.Bot;
using Telegram.Bot.Polling;
using Telegram.Bot.Types;
using Telegram.Bot.Types.Enums;
using Telegram.Bot.Types.ReplyMarkups;
using System.Collections;
using TaskBot.TelegramBot.Scenarios;
using TaskBot.TelegramBot.Dto;

namespace TaskBot.TelegramBot
{
    public delegate void MessageEventHandler(string message);

    internal class UpdateHandler : IUpdateHandler
    {
        public event MessageEventHandler OnHandleUpdateStarted = null;

        public event MessageEventHandler OnHandleUpdateCompleted = null;

        private readonly IUserService userService;
        private readonly IToDoService toDoService;
        private readonly IToDoReportService toDoReportService;
        private readonly IEnumerable<IScenario> scenarios;
        private readonly IScenarioContextRepository contextRepository;
        private readonly IToDoListService toDoListService;

        public UpdateHandler(IUserService userService, IToDoService toDoService, IToDoReportService toDoReportService, IEnumerable<IScenario> scenarios, IScenarioContextRepository contextRepository, IToDoListService toDoListService)
        {
            this.userService = userService;
            this.toDoService = toDoService;
            this.toDoReportService = toDoReportService;
            this.scenarios = scenarios;
            this.contextRepository = contextRepository;
            this.toDoListService = toDoListService;
        }

        public async Task HandleUpdateAsync(ITelegramBotClient botClient, Update update, CancellationToken ct)
        {
            if (update.Message != null)
            {
                await this.OnMessage(botClient, update, update.Message, ct);
                return;
            }

            if (update.CallbackQuery != null)
            {
                await this.OnCallbackQuery(botClient, update, update.CallbackQuery, ct);
                return;
            }

            await this.OnUnknown(update);
        }

        public Task HandleErrorAsync(ITelegramBotClient botClient, Exception exception, HandleErrorSource source, CancellationToken ct)
        {
            Console.WriteLine($"Error: {exception})");
            return Task.CompletedTask;
        }

        private async Task OnMessage(ITelegramBotClient botClient, Update update, Message message, CancellationToken ct)
        {
            Chat chat = update.Message.Chat;
            this.OnHandleUpdateStarted?.Invoke(update.Message.Text);

            try
            {
                await HandleUpdate(botClient, update, ct);
            }
            catch (ArgumentException ex)
            {
                await botClient.SendMessage(chat, ex.Message, cancellationToken: ct);
            }
            catch (TaskCountLimitException ex)
            {
                await botClient.SendMessage(chat, ex.Message, cancellationToken: ct);
            }
            catch (TaskLengthLimitException ex)
            {
                await botClient.SendMessage(chat, ex.Message, cancellationToken: ct);
            }
            catch (DuplicateTaskException ex)
            {
                await botClient.SendMessage(chat, ex.Message, cancellationToken: ct);
            }

            this.OnHandleUpdateCompleted?.Invoke(update.Message.Text);
        }

        private async Task OnCallbackQuery(ITelegramBotClient botClient, Update update, CallbackQuery callbackQuery, CancellationToken ct)
        {
            var telegramUserId = callbackQuery.From.Id;
            var user = await this.userService.GetUser(telegramUserId, ct);
            if (user == null)
            {
                throw new InvalidDataException("Unauthorized user");
            }

            var context = await this.contextRepository.GetContext(telegramUserId, ct);
            if (context != null)
            {
                await this.ProcessScenario(botClient, context, update, ct);
                return;
            }

            var callbackDto = CallbackDto.FromString(callbackQuery.Data);

            if (callbackDto.Action == "show")
            {
                var toDoListDto = ToDoListCallbackDto.FromString(callbackQuery.Data);
                var toDoItems = await this.toDoService.GetByUserIdAndList(user.UserId, toDoListDto.ToDoListId, ct);

                for (int i = 0; i < toDoItems.Count; i++)
                {
                    await botClient.SendMessage(update.Message.Chat.Id, $"{i + 1}.{toDoItems[i].Name} - {toDoItems[i].CreatedAt} - <code>{toDoItems[i].Id}</code>", ParseMode.Html, cancellationToken: ct);
                }
            }

            if(callbackDto.Action == "addlist")
            {
                await this.ProcessScenario(botClient, new ScenarioContext(ScenarioType.AddList), update, ct);
            }

            if (callbackDto.Action == "deletelist")
            {
                await this.ProcessScenario(botClient, new ScenarioContext(ScenarioType.DeleteList), update, ct);
            }

        }

        private async Task OnUnknown(Update update)
        {

        }

        private async Task HandleUpdate(ITelegramBotClient botClient, Update update, CancellationToken ct)
        {
            Chat chat = update.Message.Chat;
            string command = update.Message.Text;

            if (command.Contains("addtask"))
            {
                command = "/addtask";
            }
            else if (command.Contains("removetask"))
            {
                command = "/removetask";
            }
            else if (command.Contains("completetask"))
            {
                command = "/completetask";
            }
            else if (command.Contains("find"))
            {
                command = "/find";
            }
            else if (command.Contains("/cancel"))
            {
                await Cancel(botClient, update, ct);
                return;
            }

            var context = await this.contextRepository.GetContext(update.Message.From.Id, ct);
            if (context != null)
            {
                await this.ProcessScenario(botClient, context, update, ct);
                return;
            }

            switch (command)
            {
                case "/start":
                    await userService.RegisterUser(update.Message.From.Id, update.Message.From.Username ?? string.Empty, ct);

                    var reply = new ReplyKeyboardMarkup
                    {
                        Keyboard = new[]
                        {
                            new[]
                            {
                                 new KeyboardButton("/addtask"),
                                 new KeyboardButton ("/show"),
                                 new KeyboardButton ("/report")

                            }
                        }

                    };

                    await botClient.SendMessage(chat, "Welcome to your bot. How can I help you?", replyMarkup: reply, cancellationToken: ct);
                    break;

                case "/help":
                    await Helping(botClient, chat, ct);
                    break;

                case "/info":
                    await ProvideInfo(botClient, chat, ct);
                    break;

                case "/addtask":
                    await AddTask(botClient, update, ct);
                    break;

                case "/show":
                    await ShowTasks(botClient, update, ct);
                    break;

                case "/removetask":
                    await RemoveTask(botClient, update, ct);
                    break;

                case "/completetask":
                    await CompleteTask(botClient, update, ct);
                    break;

                case "/report":
                    await ReportTasks(botClient, update, ct);
                    break;
                case "/find":
                    await FindTasks(botClient, update, ct);
                    break;

                case "/exit":
                    await Exit(botClient, chat, ct);
                    return;


                default:
                    Console.WriteLine("The command isn`t correct.");
                    break;
            }


        }

        public IScenario GetScenario(ScenarioType scenarioType)
        {
            foreach (var scenario in this.scenarios)
            {
                if (scenario.CanHandle(scenarioType))
                {
                    return scenario;
                }
            }

            throw new Exception();
        }

        public async Task ProcessScenario(ITelegramBotClient botClient, ScenarioContext context, Update update, CancellationToken ct)
        {
            var scenario = GetScenario(context.CurrentScenario);
            var result = await scenario.HandleMessageAsync(botClient, context, update, ct);
            if (result == ScenarioResult.Transition)
            {
                var reply = new ReplyKeyboardMarkup
                {
                    Keyboard = new[]
                    {
                        new[]
                        {
                             new KeyboardButton("/cancel")
                        }
                    }

                };

                await botClient.SendMessage(update.Message.Chat, string.Empty, replyMarkup: reply, cancellationToken: ct);
            }

            if (result == ScenarioResult.Completed)
            {
                var reply = new ReplyKeyboardMarkup
                {
                    Keyboard = new[]
                       {
                            new[]
                            {
                                 new KeyboardButton("/addtask"),
                                 new KeyboardButton ("/show"),
                                 new KeyboardButton ("/report")

                            }
                        }

                };

                await botClient.SendMessage(update.Message.Chat, string.Empty, replyMarkup: reply, cancellationToken: ct);
                this.contextRepository.ResetContext(update.Message.From.Id, ct);
            }
        }

        public async Task Cancel(ITelegramBotClient botClient, Update update, CancellationToken ct)
        {
            var reply = new ReplyKeyboardMarkup
            {
                Keyboard = new[]
                      {
                            new[]
                            {
                                 new KeyboardButton("/addtask"),
                                 new KeyboardButton ("/show"),
                                 new KeyboardButton ("/report")
                            }
                        }

            };
            await botClient.SendMessage(update.Message.Chat, string.Empty, replyMarkup: reply, cancellationToken: ct);
            this.contextRepository.ResetContext(update.Message.From.Id, ct);
        }

        public async Task ProvideInfo(ITelegramBotClient botClient, Chat chat, CancellationToken ct)
        {
            await botClient.SendMessage(chat, "This is the 1.0.0 version of programm, which was created in 2025.", cancellationToken: ct);
        }

        public async Task Helping(ITelegramBotClient botClient, Chat chat, CancellationToken ct)
        {

            await botClient.SendMessage(chat, "1.Open the programm and follow the commands you see. \n2.You should choose one of those. \n" +
                               "3.You have to write the command as you see it. \n4.Recommend you to start with command start.", cancellationToken: ct);
            await botClient.SendMessage(chat, "Addtask - command to add a task for execution. For command addtask keep format (addtask taskname).\n Showalltasks - command to see all tasks you have.\n" +
                              "Showactivetasks - command to see only commands with active state. Removetask - command to remove any command you want. For command removetask keep format (removetask taskname).\n" +
                              "Completetask - command to get to the command competed state. Report - contains an information about tasks statistic.\n" +
                              "Find - command to find all tasks to starting with namePrefix.For command find keep format (find taskname)", cancellationToken: ct);
        }

        public async Task ShowTasks(ITelegramBotClient botClient, Update update, CancellationToken ct)
        {
            var user = await this.userService.GetUser(update.Message.From.Id, ct);
            var lists = await this.toDoListService.GetUserLists(user.UserId, ct);
            Chat chat = update.Message.Chat;

            var reply = new InlineKeyboardMarkup(new[]
            {
                new[]
                {
                    InlineKeyboardButton.WithCallbackData("Whithout list", new ToDoListCallbackDto { Action = "show", ToDoListId = null }.ToString()),
                },
                lists.Select(x => InlineKeyboardButton.WithCallbackData(x.Name, new ToDoListCallbackDto { Action = "show", ToDoListId = x.Id }.ToString())),
                new []
                {
                    InlineKeyboardButton.WithCallbackData("Add","addlist"),
                    InlineKeyboardButton.WithCallbackData("Delete","deletelist")
                }

            });

            await botClient.SendMessage(chat, "Choose the list:", replyMarkup: reply, cancellationToken: ct);

            IReadOnlyList<ToDoItem> toDoList = await toDoService.GetActiveByUserId(user.UserId, ct);

            for (int i = 0; i < toDoList.Count; i++)
            {
                await botClient.SendMessage(chat.Id, $"{i + 1}.{toDoList[i].Name} - {toDoList[i].CreatedAt} - <code>{toDoList[i].Id}</code>", ParseMode.Html, cancellationToken: ct);
            }
        }

        public async Task AddTask(ITelegramBotClient botClient, Update update, CancellationToken ct)
        {
            var context = new ScenarioContext(ScenarioType.AddTask);
            await this.ProcessScenario(botClient, context, update, ct);

        }

        public async Task RemoveTask(ITelegramBotClient botClient, Update update, CancellationToken ct)
        {
            Chat chat = update.Message.Chat;
            string messageText = update.Message.Text;
            var user = await userService.GetUser(update.Message.From.Id, ct);
            int taskNumber;
            string number = messageText.Substring(messageText.IndexOf(' ') + 1);
            bool isTaskNumber = int.TryParse(number, out taskNumber);
            if (!isTaskNumber)
            {
                await botClient.SendMessage(chat, "Wrong number!", cancellationToken: ct);
            }
            IReadOnlyList<ToDoItem> toDoList = await toDoService.GetActiveByUserId(user.UserId, ct);
            if (taskNumber > toDoList.Count && taskNumber < 1)
            {
                throw new IndexOutOfRangeException("The task number isn`t in correct form");
            }
            int indexOfNum = taskNumber - 1;
            var itemToRemove = toDoList[indexOfNum];
            await botClient.SendMessage(chat, $"The task \"{itemToRemove}\" has been removed:", cancellationToken: ct);
            await toDoService.Delete(toDoList[taskNumber - 1].Id, ct);

        }
        public async Task CompleteTask(ITelegramBotClient botClient, Update update, CancellationToken ct)
        {
            Chat chat = update.Message.Chat;
            string messageText = update.Message.Text;
            Guid taskId;
            string id = messageText.Substring(messageText.IndexOf(' ') + 1);
            bool isTaskId = Guid.TryParse(id, out taskId);
            if (!isTaskId)
            {
                await botClient.SendMessage(chat, "Wrong Id!", cancellationToken: ct);
            }
            await toDoService.MarkCompleted(taskId, ct);
        }

        public async Task ShowAllTasks(ITelegramBotClient botClient, Update update, CancellationToken ct)
        {
            Chat chat = update.Message.Chat;
            await botClient.SendMessage(chat, "Here is your all tasklist:", cancellationToken: ct);

            var user = await userService.GetUser(update.Message.From.Id, ct);
            var taskList = await toDoService.GetAllByUserId(user.UserId, ct);

            for (int i = 0; i < taskList.Count; i++)
            {
                if (taskList[i].State == ToDoItemState.Completed)
                {
                    await botClient.SendMessage(chat.Id, $" (Completed) {taskList[i].Name} - {taskList[i].CreatedAt} - <code>{taskList[i].Id}</code>", ParseMode.Html, cancellationToken: ct);
                }
                if (taskList[i].State == ToDoItemState.Active)
                {
                    await botClient.SendMessage(chat.Id, $" (Active) {taskList[i].Name} - {taskList[i].CreatedAt} - <code>{taskList[i].Id}</code>", ParseMode.Html, cancellationToken: ct);
                }
            }
        }

        public async Task ReportTasks(ITelegramBotClient botClient, Update update, CancellationToken ct)
        {
            Chat chat = update.Message.Chat;
            var user = await userService.GetUser(update.Message.From.Id, ct);
            var report = await toDoReportService.GetUserStats(user.UserId, ct);

            await botClient.SendMessage(chat, $" Tasks statistics for {DateTime.Now}. Total:{report.total}, Completed:{report.completed}, Active:{report.active}:", cancellationToken: ct);
        }

        public async Task FindTasks(ITelegramBotClient botClient, Update update, CancellationToken ct)
        {
            var user = await userService.GetUser(update.Message.From.Id, ct);
            Chat chat = update.Message.Chat;
            string messageText = update.Message.Text;
            string taskName = messageText.Substring(messageText.IndexOf(' ') + 1);
            var namePrefixList = await toDoService.Find(user, taskName, ct);

            for (int i = 0; i < namePrefixList.Count; i++)
            {
                await botClient.SendMessage(chat, $"{i + 1}.{namePrefixList[i].Name} - {namePrefixList[i].CreatedAt} - {namePrefixList[i].Id}", cancellationToken: ct);
            }
        }
        public async Task Exit(ITelegramBotClient botClient, Chat chat, CancellationToken ct)
        {
            await botClient.SendMessage(chat, "The program is over!", cancellationToken: ct);
        }


    }
}
