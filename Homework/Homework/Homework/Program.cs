
using Otus.ToDoList.ConsoleBot;
using Otus.ToDoList.ConsoleBot.Types;
using System;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using TaskBot;

namespace Homework
{
    internal class Program
    {
        static void Main(string[] args)
        {
            var telegramBotClinet = new ConsoleBotClient();
            var updateHandler = new UpdateHandler(new UserService(), new ToDoService());

            try
            {
                telegramBotClinet.StartReceiving(updateHandler, "start, help, info, addtask, removetask, showtasks, showalltask, completetask, exit.");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Something went wrong: {ex.GetType}, {ex.Message}, {ex.StackTrace}, {ex.InnerException}");
            }
        }

    }
}