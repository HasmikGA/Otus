
using Otus.ToDoList.ConsoleBot;
using Otus.ToDoList.ConsoleBot.Types;
using System;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using TaskBot;
using TaskBot.Core.DataAccess;
using TaskBot.Core.Services;
using TaskBot.Infrastructure.DataAccess;
using TaskBot.TelegramBot;


namespace Homework
{
    internal class Program
    {

        static void Main(string[] args)
        {
            var telegramBotClinet = new ConsoleBotClient();
            IUserRepository userRepository = new InMemoryUserRepository();
            IToDoRepository toDoRepository = new InMemoryToDoRepository();
            IToDoReportService toDoReportService = new ToDoReportService(toDoRepository);
            var updateHandler = new UpdateHandler(new UserService(userRepository), new ToDoService(20, 100, toDoRepository), new ToDoReportService(toDoRepository));
            CancellationTokenSource cts = new CancellationTokenSource();
            CancellationToken token = cts.Token;

            try
            {
                updateHandler.OnHandleUpdateStarted += OnUpdateStarted;
                updateHandler.OnHandleUpdateCompleted += OnUpdateCompleted;

                telegramBotClinet.StartReceiving(updateHandler, token);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Something went wrong: {ex.GetType}, {ex.Message}, {ex.StackTrace}, {ex.InnerException}");
            }
            finally
            {
                updateHandler.OnHandleUpdateStarted -= OnUpdateStarted;
                updateHandler.OnHandleUpdateCompleted -= OnUpdateCompleted;
            }

        }
        static void OnUpdateStarted(string message)
        {
            Console.WriteLine($"Началась обработка сообщения '{message}'");
        }
        static void OnUpdateCompleted(string message)
        {
            Console.WriteLine($"Закончилась обработка сообщения '{message}'");
        }


    }
}