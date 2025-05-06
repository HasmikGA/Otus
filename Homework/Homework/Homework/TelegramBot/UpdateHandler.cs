using Otus.ToDoList.ConsoleBot;
using Otus.ToDoList.ConsoleBot.Types;
using System.Diagnostics;
using System.Text.RegularExpressions;
using TaskBot.Core.Entities;
using TaskBot.Core.Exceptions;
using TaskBot.Core.Services;

namespace TaskBot.TelegramBot
{
    public  delegate void MessageEventHandler(string  message);

    internal class UpdateHandler : IUpdateHandler
    {
        public event MessageEventHandler OnHandleUpdateStarted;

        public event MessageEventHandler OnHandleUpdateCompleted;


        private readonly IUserService userService;
        private readonly IToDoService toDoService;
        private readonly IToDoReportService toDoReportService;

        public UpdateHandler(IUserService userService, IToDoService toDoService, IToDoReportService toDoReportService)
        {
            this.userService = userService;
            this.toDoService = toDoService;
            this.toDoReportService = toDoReportService;
        }

        public async Task HandleUpdateAsync(ITelegramBotClient botClient, Update update, CancellationToken ct)
        {
            Chat chat = update.Message.Chat;
            this.OnHandleUpdateStarted?.Invoke(update.Message.Text);

            try
            {
                await HandleUpdate(botClient, update, ct);
            }
            catch (ArgumentException ex)
            {
                await botClient.SendMessage(chat, ex.Message, ct);
            }
            catch (TaskCountLimitException ex)
            {
                await botClient.SendMessage(chat, ex.Message, ct);
            }
            catch (TaskLengthLimitException ex)
            {
                await botClient.SendMessage(chat, ex.Message, ct);
            }
            catch (DuplicateTaskException ex)
            {
                await botClient.SendMessage(chat, ex.Message, ct);
            }

            this.OnHandleUpdateCompleted?.Invoke(update.Message.Text);
        }

        public async Task HandleErrorAsync(ITelegramBotClient botClient, Exception exception, CancellationToken ct)
        {
            await botClient.SendMessage(null, exception.Message, ct);
        }

        private async Task HandleUpdate(ITelegramBotClient botClient, Update update, CancellationToken ct)
        {
            Chat chat = update.Message.Chat;
            string command = update.Message.Text;

            if (command.Contains("addtask"))
            {
                command = "addtask";
            }
            else if (command.Contains("removetask"))
            {
                command = "removetask";
            }
            else if (command.Contains("completetask"))
            {
                command = "completetask";
            }
            else if (command.Contains("find"))
            {
                command = "find";
            }

            switch (command)
            {
                case "start":
                    await userService.RegisterUser(update.Message.From.Id, update.Message.From.Username ?? string.Empty, ct);
                    await botClient.SendMessage(chat, "help, info, addtask, removetask, showtasks, showalltasks, completetask, report, find, exit:", ct);
                    break;

                case "help":
                    await Helping(botClient, chat, ct);
                    await botClient.SendMessage(chat, "start, info, addtask, removetask, showtasks, showalltasks, completetask, report, find, exit:", ct);
                    break;

                case "info":
                    await ProvideInfo(botClient, chat, ct);
                    await botClient.SendMessage(chat, "start, help, addtask, removetask, showtasks, showalltasks, completetask, report, find, exit:", ct);
                    break;

                case "addtask":
                    await AddTask(botClient, update, ct);
                    break;

                case "showtasks":
                    await ShowTasks(botClient, chat, ct);
                    break;

                case "showalltasks":
                    await ShowAllTasks(botClient, chat, ct);
                    break;

                case "removetask":
                    await RemoveTask(botClient, update, ct);
                    break;

                case "completetask":
                    await CompleteTask(botClient, update, ct);
                    break;

                case "report":
                    await ReportTasks(botClient, chat, ct);
                    break;
                case "find":
                    await FindTasks(botClient, update, ct);
                    break;

                case "exit":
                    await Exit(botClient, chat, ct);
                    return;

                default:
                    Console.WriteLine("The command isn`t correct.");
                    break;
            }
        }

        public async Task ProvideInfo(ITelegramBotClient botClient, Chat chat, CancellationToken ct)
        {
            await botClient.SendMessage(chat, "This is the 1.0.0 version of programm, which was created in 2025.", ct);
        }

        public async Task Helping(ITelegramBotClient botClient, Chat chat, CancellationToken ct)
        {

            await botClient.SendMessage(chat, "1.Open the programm and follow the commands you see. \n2.You should choose one of those. \n" +
                               "3.You have to write the command as you see it. \n4.Recommend you to start with command start.", ct);
            await botClient.SendMessage(chat, "Addtask - command to add a task for execution. For command addtask keep format (addtask taskname).\n Showalltasks - command to see all tasks you have.\n" +
                              "Showactivetasks - command to see only commands with active state. Removetask - command to remove any command you want. For command removetask keep format (removetask taskname).\n" +
                              "Completetask - command to get to the command competed state. Report - contains an information about tasks statistic.\n" +
                              "Find - command to find all tasks to starting with namePrefix.For command find keep format (find taskname)", ct);
        }

        public async Task ShowTasks(ITelegramBotClient botClient, Chat chat, CancellationToken ct)
        {
            await botClient.SendMessage(chat, "Here is your active tasklist:", ct);

            IReadOnlyList<ToDoItem> toDoList = await toDoService.GetActiveByUserId(Guid.NewGuid(), ct);

            for (int i = 0; i < toDoList.Count; i++)
            {
                await botClient.SendMessage(chat, $"{i + 1}.{toDoList[i].Name} - {toDoList[i].CreatedAt} - {toDoList[i].Id}", ct);
            }
        }

        public async Task AddTask(ITelegramBotClient botClient, Update update, CancellationToken ct)
        {
            Chat chat = update.Message.Chat;
            string messageText = update.Message.Text;
            string taskName = messageText.Substring(messageText.IndexOf(' ') + 1);

            ToDoItem task = await toDoService.Add(new ToDoUser(), taskName, ct);

            await botClient.SendMessage(chat, $"The task \"{task.Name}\" has been added.", ct);
        }

        public async Task RemoveTask(ITelegramBotClient botClient, Update update, CancellationToken ct)
        {
            Chat chat = update.Message.Chat;
            string messageText = update.Message.Text;
            int taskNumber;
            string number = messageText.Substring(messageText.IndexOf(' ') + 1);
            bool isTaskNumber = int.TryParse(number, out taskNumber);
            if (!isTaskNumber)
            {
                await botClient.SendMessage(chat, "Wrong number!", ct);
            }
            IReadOnlyList<ToDoItem> toDoList = await toDoService.GetActiveByUserId(Guid.NewGuid(), ct);
            if (taskNumber > toDoList.Count && taskNumber < 1)
            {
                throw new IndexOutOfRangeException("The task number isn`t in correct form");
            }
            int indexOfNum = taskNumber - 1;
            var itemToRemove = toDoList[indexOfNum];
            await botClient.SendMessage(chat, $"The task \"{itemToRemove}\" has been removed:", ct);
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
                await botClient.SendMessage(chat, "Wrong Id!", ct);
            }
            await toDoService.MarkCompleted(taskId, ct);
        }

        public async Task ShowAllTasks(ITelegramBotClient botClient, Chat chat, CancellationToken ct)
        {
            await botClient.SendMessage(chat, "Here is your all tasklist:", ct);
            var taskList = await toDoService.GetAllByUserId(Guid.NewGuid(), ct);

            for (int i = 0; i < taskList.Count; i++)
            {
                if (taskList[i].State == ToDoItemState.Completed)
                {
                    await botClient.SendMessage(chat, $" (Completed) {taskList[i].Name} - {taskList[i].CreatedAt} - {taskList[i].Id}", ct); ;
                }
                if (taskList[i].State == ToDoItemState.Active)
                {
                    await botClient.SendMessage(chat, $" (Active) {taskList[i].Name} - {taskList[i].CreatedAt} - {taskList[i].Id}", ct);
                }
            }
        }

        public async Task ReportTasks(ITelegramBotClient botClient, Chat chat, CancellationToken ct)
        {
            var report = await toDoReportService.GetUserStats(Guid.NewGuid(), ct);
            await botClient.SendMessage(chat, $" Tasks statistics for {DateTime.Now}. Total:{report.total}, Completed:{report.completed}, Active:{report.active}:", ct);
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
                await botClient.SendMessage(chat, $"{i + 1}.{namePrefixList[i].Name} - {namePrefixList[i].CreatedAt} - {namePrefixList[i].Id}", ct);
            }
        }
        public async Task Exit(ITelegramBotClient botClient, Chat chat, CancellationToken ct)
        {
            await botClient.SendMessage(chat, "The program is over!", ct);
        }
    }
}
