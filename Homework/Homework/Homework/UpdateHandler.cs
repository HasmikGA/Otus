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
        private ITelegramBotClient telegramBotClinet;
        private IToDoService toDoService;
        private Chat chat;

        public UpdateHandler(IUserService userService, IToDoService toDoService)
        {
            this.userService = userService;
            this.toDoService = toDoService;
        }

        public void HandleUpdateAsync(ITelegramBotClient botClient, Update update)
        {
            this.telegramBotClinet = botClient;
            this.chat = update.Message.Chat;

            try
            {
                this.HandleUpdate(update);
            }
            catch (ArgumentException ex)
            {
                this.telegramBotClinet.SendMessage(this.chat, ex.Message);
            }
            catch (TaskCountLimitException ex)
            {
                this.telegramBotClinet.SendMessage(this.chat, ex.Message);
            }
            catch (TaskLengthLimitException ex)
            {
                this.telegramBotClinet.SendMessage(this.chat, ex.Message);
            }
            catch (DuplicateTaskException ex)
            {
                this.telegramBotClinet.SendMessage(this.chat, ex.Message);
            }
        }

        public void HandleUpdate(Update update)
        {
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
                    this.userService.RegisterUser(update.Message.From.Id, update.Message.From.Username);
                    this.telegramBotClinet.SendMessage(this.chat, "help, info, addtask, removetask, showtasks, showalltasks, completetask, exit:");
                    break;

                case "help":
                    Helping();
                    this.telegramBotClinet.SendMessage(this.chat, "start, info, addtask, removetask, showtasks, showalltasks, completetask, exit:");
                    break;

                case "info":
                    ProvideInfo();
                    this.telegramBotClinet.SendMessage(this.chat, "start, help, addtask, removetask, showtasks,showalltasks, completetask, exit:");
                    break;

                case "addtask":
                    AddTask(update);
                    break;

                case "showtasks":
                    ShowTasks(update);
                    break;

                case "showalltasks":
                    ShowAllTasks(update);
                    break;

                case "removetask":
                    RemoveTask(update);
                    break;

                case "completetask":
                    CompleteTask(update);
                    break;

                case "exit":
                    Exit();
                    return;

                default:
                    Console.WriteLine("The command isn`t correct.");
                    break;
            }
        }

        public void ProvideInfo()
        {
            this.telegramBotClinet.SendMessage(this.chat, "This is the 1.0.0 version of programm, which was created in 2025.");

        }

        public void Helping()
        {
            this.telegramBotClinet.SendMessage(this.chat, "1.Open the programm and follow the commands you see. \n2.You should choose one of those. \n" +
                              "3.You have to write the command as you see it. \n4.Recommend you to start with command start.");
            this.telegramBotClinet.SendMessage(this.chat, "Addtask - command to add a task for execution. For command addtask keep format (addtask taskname).\n Showalltasks - command to see all tasks you have.\n" +
                              "Showactivetasks - command to see only commands with active state. Removetask - command to remove any command you want. For command removetask keep format (removetask taskname).\n" +
                              "Completetask - command to get to the command competed state.");
        }

        public void ShowTasks(Update update)
        {
            this.telegramBotClinet.SendMessage(this.chat, "Here is your active tasklist:");
            IReadOnlyList<ToDoItem> toDoList = this.toDoService.GetActiveByUserId(Guid.NewGuid());

            for (int i = 0; i < toDoList.Count; i++)
            {
                this.telegramBotClinet.SendMessage(this.chat, $"{i + 1}.{toDoList[i].Name} - {toDoList[i].CreatedAt} - {toDoList[i].Id}");
            }
        }

        public void AddTask(Update update)
        {
            string messageText = update.Message.Text;
            string taskName = messageText.Substring(messageText.IndexOf(' ') + 1);

            ToDoItem task = this.toDoService.Add(new ToDoUser(), taskName);

            this.telegramBotClinet.SendMessage(this.chat, $"The task \"{task.Name}\" has been added. ");

        }

        public void RemoveTask(Update update)
        {
            string messageText = update.Message.Text;
            int taskNumber;
            string number = messageText.Substring(messageText.IndexOf(' ') + 1);
            bool isTaskNumber = int.TryParse(number, out taskNumber);
            if (!isTaskNumber)
            {
                this.telegramBotClinet.SendMessage(this.chat, "Wrong number!");
            }
            IReadOnlyList<ToDoItem> toDoList = this.toDoService.GetActiveByUserId(Guid.NewGuid());
            if (taskNumber > toDoList.Count && taskNumber < 1)
            {
                throw new IndexOutOfRangeException();
            }
            int indexOfNum = taskNumber - 1;
            var itemToRemove = toDoList[indexOfNum];
            this.telegramBotClinet.SendMessage(this.chat, $"The task \"{itemToRemove}\" has been removed:");
            this.toDoService.Delete(toDoList[taskNumber - 1].Id);

        }

        public void CompleteTask(Update update)
        {
            string messageText = update.Message.Text;
            Guid taskId;
            string id = messageText.Substring(messageText.IndexOf(' ') + 1);
            bool isTaskId = Guid.TryParse(id, out taskId);
            if (!isTaskId)
            {
                this.telegramBotClinet.SendMessage(this.chat, "Wrong Id!");
            }
            this.toDoService.MarkCompleted(taskId);
        }

        public void ShowAllTasks(Update update)
        {
            this.telegramBotClinet.SendMessage(this.chat, "Here is your all tasklist:");
            var taskList = this.toDoService.GetByUserId(Guid.NewGuid());

            for (int i = 0; i < taskList.Count; i++)
            {
                if (taskList[i].State == ToDoItemState.Completed)
                {
                    this.telegramBotClinet.SendMessage(this.chat, $" (Completed) {taskList[i].Name} - {taskList[i].CreatedAt} - {taskList[i].Id}"); ;
                }
                if (taskList[i].State == ToDoItemState.Active)
                {
                    this.telegramBotClinet.SendMessage(this.chat, $" (Active) {taskList[i].Name} - {taskList[i].CreatedAt} - {taskList[i].Id}");
                }
            }
        }

        public void Exit()
        {
            this.telegramBotClinet.SendMessage(this.chat, "The program is over!");
        }
    }
}
