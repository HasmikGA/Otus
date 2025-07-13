using TaskBot.Core.Entities;
using TaskBot.Core.Exceptions;
using TaskBot.Core.Services;
using TaskBot.Helper;
using TaskBot.TelegramBot.Dto;
using TaskBot.TelegramBot.Scenarios;
using Telegram.Bot;
using Telegram.Bot.Polling;
using Telegram.Bot.Types;
using Telegram.Bot.Types.Enums;
using Telegram.Bot.Types.ReplyMarkups;

namespace TaskBot.TelegramBot
{
    public delegate void MessageEventHandler(string message);

    internal class UpdateHandler : IUpdateHandler
    {
        private int pageSize = 5;

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

            var context = await this.contextRepository.GetContext(callbackQuery.Message.Chat.Id, ct);
            if (context != null)
            {
                await this.ProcessScenario(botClient, context, update, ct);
                return;
            }

            var callbackDto = CallbackDto.FromString(callbackQuery.Data);
            var message = update.Type == UpdateType.CallbackQuery ? update.CallbackQuery.Message : update.Message;

            if (callbackDto.Action == "show")
            {
                var toDoListDto = PagedListCallbackDto.FromString(callbackQuery.Data);
                var toDoItems = await this.toDoService.GetByUserIdAndList(user.UserId, toDoListDto.ToDoListId, ct);

                var itemsCallbackDto = toDoItems.Select(item => new KeyValuePair<string, string>(item.Name, new ToDoItemCallbackDto { Action = "showtask", ToDoItemId = item.Id }.ToString()));
                var reply = this.BuildPagedButtons(itemsCallbackDto, toDoListDto);

                await botClient.EditMessageText(message.Chat.Id, message.MessageId, $"Here is your tasks", replyMarkup: reply, cancellationToken: ct);
            }

            if (callbackDto.Action == "showtask")
            {
                var toDoItemDto = ToDoItemCallbackDto.FromString(callbackQuery.Data);
                var toDoItem = await this.toDoService.Get(user.UserId, toDoItemDto.ToDoItemId.Value, ct);

                var reply = new InlineKeyboardMarkup(new[]
                {
                    InlineKeyboardButton.WithCallbackData("Complete",new ToDoItemCallbackDto{Action = "completetask", ToDoItemId = toDoItem.Id}.ToString()),
                    InlineKeyboardButton.WithCallbackData("Delete",new ToDoItemCallbackDto{Action = "deletetask", ToDoItemId = toDoItem.Id}.ToString())
                });

                await botClient.SendMessage(message.Chat.Id, $"Deadline is - {toDoItem.Deadline} \n CreatedAt - {toDoItem.CreatedAt} \n Id is - {toDoItem.Id}", replyMarkup: reply, cancellationToken: ct);
            }

            if (callbackDto.Action == "completetask")
            {
                var toDoItemDto = ToDoItemCallbackDto.FromString(callbackQuery.Data);
                await this.toDoService.MarkCompleted(user.UserId, toDoItemDto.ToDoItemId.Value, ct);

            }

            if (callbackDto.Action == "deletetask")
            {
                var toDoItemDto = ToDoItemCallbackDto.FromString(callbackQuery.Data);
                await this.toDoService.Delete(toDoItemDto.ToDoItemId.Value, user.UserId, ct);
            }

            if (callbackDto.Action == "addlist")
            {
                await this.ProcessScenario(botClient, new ScenarioContext(ScenarioType.AddList), update, ct);
            }

            if (callbackDto.Action == "deletelist")
            {
                await this.ProcessScenario(botClient, new ScenarioContext(ScenarioType.DeleteList), update, ct);
            }
            if (callbackDto.Action == "show_completed")
            {
               var items =  await this.toDoService.GetCompleted(user.UserId,ct);
            }

        }

        private InlineKeyboardMarkup BuildPagedButtons(IEnumerable<KeyValuePair<string, string>> callbackData, PagedListCallbackDto listDto)
        {
            var totalPages = (callbackData.Count() + this.pageSize - 1) / this.pageSize;
            var batchItems = callbackData.GetBatchByNumber(this.pageSize, listDto.Page);
            var items = batchItems.Select(item => InlineKeyboardButton.WithCallbackData(item.Key, item.Value));
            var leftRight = new List<InlineKeyboardButton>();

            if (listDto.Page > 0)
            {
                leftRight.Add(InlineKeyboardButton.WithCallbackData("Left", new PagedListCallbackDto { Action = listDto.Action, Page = listDto.Page - 1 }.ToString()));
            }

            if (listDto.Page < totalPages - 1)
            {
                leftRight.Add(InlineKeyboardButton.WithCallbackData("Right", new PagedListCallbackDto { Action = listDto.Action, Page = listDto.Page + 1 }.ToString()));
            }

            return new InlineKeyboardMarkup(new[]
            {
                items,
                leftRight,
                new [] { InlineKeyboardButton.WithCallbackData("Colpleted tasks",new  PagedListCallbackDto { Action = "show_completed", ToDoListId = listDto.ToDoListId, Page = 0 }.ToString())}
            });
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
            else if (command.Contains("find"))
            {
                command = "/find";
            }
            else if (command.Contains("/cancel"))
            {
                await Cancel(botClient, update, ct);
                return;
            }

            var context = await this.contextRepository.GetContext(update.Message.Chat.Id, ct);
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
            var message = update.Type == UpdateType.CallbackQuery ? update.CallbackQuery.Message : update.Message;
            if (!await this.contextRepository.HasContext(message.Chat.Id, ct))
            {
                await this.contextRepository.SetContext(message.Chat.Id, context, ct);
            }

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
                            new KeyboardButton("/addtask"),
                            new KeyboardButton ("/show"),
                            new KeyboardButton ("/report"),
                            new KeyboardButton("/cancel")
                        }
                    }
                };

                await botClient.SendMessage(message.Chat, "For cancel press cancel.", replyMarkup: reply, cancellationToken: ct);
            }

            if (result == ScenarioResult.Completed)
            {
                this.contextRepository.ResetContext(message.Chat.Id, ct);
            }
        }

        public async Task Cancel(ITelegramBotClient botClient, Update update, CancellationToken ct)
        {
            var message = update.Type == UpdateType.CallbackQuery ? update.CallbackQuery.Message : update.Message;
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
            await botClient.SendMessage(message.Chat, string.Empty, replyMarkup: reply, cancellationToken: ct);
            this.contextRepository.ResetContext(message.Chat.Id, ct);
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
                    InlineKeyboardButton.WithCallbackData("Whithout list", new PagedListCallbackDto { Action = "show", ToDoListId = null }.ToString()),
                },
                lists.Select(x => InlineKeyboardButton.WithCallbackData(x.Name, new PagedListCallbackDto { Action = "show", ToDoListId = x.Id }.ToString())),
                new []
                {
                    InlineKeyboardButton.WithCallbackData("Add","addlist"),
                    InlineKeyboardButton.WithCallbackData("Delete","deletelist")
                }

            });

            await botClient.SendMessage(chat, "Choose the list:", replyMarkup: reply, cancellationToken: ct);

        }
        public async Task AddTask(ITelegramBotClient botClient, Update update, CancellationToken ct)
        {
            var context = new ScenarioContext(ScenarioType.AddTask);
            await this.ProcessScenario(botClient, context, update, ct);

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
