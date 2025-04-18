
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
        static void F(int a, int b, Func<int, int, bool> isNumbersEqual)
        {
            if (isNumbersEqual(a, b))
            {
                Console.WriteLine("1");
            }
            else
            {
                Console.WriteLine("2");
            }

        }
        static void Main(string[] args)
        {
            F(3, 8, (x, y) => x == y);

            var telegramBotClinet = new ConsoleBotClient();
            IUserRepository userRepository = new InMemoryUserRepository();
            IToDoRepository toDoRepository = new InMemoryToDoRepository();
            IToDoReportService toDoReportService = new ToDoReportService(toDoRepository);
            var updateHandler = new UpdateHandler(new UserService(userRepository), new ToDoService(20, 100, toDoRepository), new ToDoReportService(toDoRepository));

            try
            {
                telegramBotClinet.StartReceiving(updateHandler);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Something went wrong: {ex.GetType}, {ex.Message}, {ex.StackTrace}, {ex.InnerException}");
            }
        }

    }
}