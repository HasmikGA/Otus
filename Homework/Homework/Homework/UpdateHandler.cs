using Otus.ToDoList.ConsoleBot;
using Otus.ToDoList.ConsoleBot.Types;
using System.Diagnostics;
using System.Text.RegularExpressions;
using TaskBot.Exceptions;

namespace TaskBot
{
    internal class UpdateHandler : IUpdateHandler
    {
        private readonly IUserService userService;
        private readonly IToDoService toDoService;

        public UpdateHandler(IUserService userService, IToDoService toDoService)
        {
            this.userService = userService;
            this.toDoService = toDoService;
        }

        public void HandleUpdateAsync(ITelegramBotClient botClient, Update update)
        {
            Chat chat = update.Message.Chat;
            try
            {
                this.HandleUpdate(botClient, update);
            }
            catch (ArgumentException ex)
            {
                botClient.SendMessage(chat, ex.Message);
            }
            catch (TaskCountLimitException ex)
            {
                botClient.SendMessage(chat, ex.Message);
            }
            catch (TaskLengthLimitException ex)
            {
                botClient.SendMessage(chat, ex.Message);
            }
            catch (DuplicateTaskException ex)
            {
                botClient.SendMessage(chat, ex.Message);
            }
        }
        public void HandleUpdate(ITelegramBotClient botClient, Update update)
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

            switch (command)
            {
                case "start":
                    this.userService.RegisterUser(update.Message.From.Id, update.Message.From.Username??string.Empty);
                    botClient.SendMessage(chat, "help, info, addtask, removetask, showtasks, showalltasks, completetask, exit:");
                    break;

                case "help":
                    Helping(botClient, chat);
                    botClient.SendMessage(chat, "start, info, addtask, removetask, showtasks, showalltasks, completetask, exit:");
                    break;

                case "info":
                    ProvideInfo(botClient, chat);
                    botClient.SendMessage(chat, "start, help, addtask, removetask, showtasks, showalltasks, completetask, exit:");
                    break;

                case "addtask":
                    AddTask(botClient, update);
                    break;

                case "showtasks":
                    ShowTasks(botClient, chat);
                    break;

                case "showalltasks":
                    ShowAllTasks(botClient, chat);
                    break;

                case "removetask":
                    RemoveTask(botClient, update);
                    break;

                case "completetask":
                    CompleteTask(botClient, update);
                    break;

                case "exit":
                    Exit(botClient, chat);
                    return;

                default:
                    Console.WriteLine("The command isn`t correct.");
                    break;
            }
        }

        public void ProvideInfo(ITelegramBotClient botClient, Chat chat)
        {
            botClient.SendMessage(chat, "This is the 1.0.0 version of programm, which was created in 2025.");
        }

        public void Helping(ITelegramBotClient botClient, Chat chat)
        {

            botClient.SendMessage(chat, "1.Open the programm and follow the commands you see. \n2.You should choose one of those. \n" +
                              "3.You have to write the command as you see it. \n4.Recommend you to start with command start.");
            botClient.SendMessage(chat, "Addtask - command to add a task for execution. For command addtask keep format (addtask taskname).\n Showalltasks - command to see all tasks you have.\n" +
                              "Showactivetasks - command to see only commands with active state. Removetask - command to remove any command you want. For command removetask keep format (removetask taskname).\n" +
                              "Completetask - command to get to the command competed state.");
        }

        public void ShowTasks(ITelegramBotClient botClient, Chat chat)
        {
            botClient.SendMessage(chat, "Here is your active tasklist:");

            IReadOnlyList<ToDoItem> toDoList = this.toDoService.GetActiveByUserId(Guid.NewGuid());

            for (int i = 0; i < toDoList.Count; i++)
            {
                botClient.SendMessage(chat, $"{i + 1}.{toDoList[i].Name} - {toDoList[i].CreatedAt} - {toDoList[i].Id}");
            }
        }

        public void AddTask(ITelegramBotClient botClient, Update update)
        {
            Chat chat = update.Message.Chat;
            string messageText = update.Message.Text;
            string taskName = messageText.Substring(messageText.IndexOf(' ') + 1);

            ToDoItem task = this.toDoService.Add(new ToDoUser(), taskName);

            botClient.SendMessage(chat, $"The task \"{task.Name}\" has been added. ");

        }

        public void RemoveTask(ITelegramBotClient botClient, Update update)
        {
            Chat chat = update.Message.Chat;
            string messageText = update.Message.Text;
            int taskNumber;
            string number = messageText.Substring(messageText.IndexOf(' ') + 1);
            bool isTaskNumber = int.TryParse(number, out taskNumber);
            if (!isTaskNumber)
            {
                botClient.SendMessage(chat, "Wrong number!");
            }
            IReadOnlyList<ToDoItem> toDoList = this.toDoService.GetActiveByUserId(Guid.NewGuid());
            if (taskNumber > toDoList.Count && taskNumber < 1)
            {
                throw new IndexOutOfRangeException("The task number isn`t in correct form");
            }
            int indexOfNum = taskNumber - 1;
            var itemToRemove = toDoList[indexOfNum];
            botClient.SendMessage(chat, $"The task \"{itemToRemove}\" has been removed:");
            this.toDoService.Delete(toDoList[taskNumber - 1].Id);

        }

        public void CompleteTask(ITelegramBotClient botClient, Update update)
        {
            Chat chat = update.Message.Chat;
            string messageText = update.Message.Text;
            Guid taskId;
            string id = messageText.Substring(messageText.IndexOf(' ') + 1);
            bool isTaskId = Guid.TryParse(id, out taskId);
            if (!isTaskId)
            {
                botClient.SendMessage(chat, "Wrong Id!");
            }
            this.toDoService.MarkCompleted(taskId);
        }

        public void ShowAllTasks(ITelegramBotClient botClient, Chat chat)
        {
            botClient.SendMessage(chat, "Here is your all tasklist:");
            var taskList = this.toDoService.GetByUserId(Guid.NewGuid());

            for (int i = 0; i < taskList.Count; i++)
            {
                if (taskList[i].State == ToDoItemState.Completed)
                {
                    botClient.SendMessage(chat, $" (Completed) {taskList[i].Name} - {taskList[i].CreatedAt} - {taskList[i].Id}"); ;
                }
                if (taskList[i].State == ToDoItemState.Active)
                {
                    botClient.SendMessage(chat, $" (Active) {taskList[i].Name} - {taskList[i].CreatedAt} - {taskList[i].Id}");
                }
            }
        }

        public void Exit(ITelegramBotClient botClient, Chat chat)
        {
            botClient.SendMessage(chat, "The program is over!");
        }
    }
}
