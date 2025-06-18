using System.IO;
using TaskBot;
using TaskBot.Core.DataAccess;
using TaskBot.Core.Entities;
using TaskBot.Core.Services;
using TaskBot.Infrastructure.DataAccess;
using TaskBot.TelegramBot;
using TaskBot.TelegramBot.Scenarios;
using Telegram.Bot;
using Telegram.Bot.Polling;
using Telegram.Bot.Types;
using Telegram.Bot.Types.Enums;


namespace Homework
{
    internal class Program
    {

        static async Task Main(string[] args)
        {

            string clToken = Environment.GetEnvironmentVariable("TELEGRAM_BOT_TOKEN", EnvironmentVariableTarget.User);
            if (string.IsNullOrEmpty(clToken))
            {
                Console.WriteLine("Bot token not found. Please set the TELEGRAM_BOT_TOKEN environment variable.");
                return;
            }

            var telegramBotClinet = new TelegramBotClient(clToken);
            IUserRepository userRepository = new FileUserRepository("TaskBot-ToDoUser");
            IToDoRepository toDoRepository = new FileToDoRepository("TaskBot-ToDoItems");
            IScenarioContextRepository contextRepository = new InMemoryScenarioContextRepository();
            IToDoReportService toDoReportService = new ToDoReportService(toDoRepository);
            IToDoListRepository toDoListRepository = new FileToDoListRepository("TaskBot-ToDoList");
            IToDoListService toDoListService = new ToDoListService(toDoListRepository);
            var userService = new UserService(userRepository);
            var toDoService = new ToDoService(20, 100, toDoRepository);
            var scenarios = new[] { new AddTaskScenario(userService, toDoService) };
            var updateHandler = new UpdateHandler(userService, toDoService, new ToDoReportService(toDoRepository), scenarios, contextRepository, toDoListService);
            CancellationTokenSource cancellationToken = new CancellationTokenSource();
            CancellationToken ct = cancellationToken.Token;
            ReceiverOptions receiverOptions = new ReceiverOptions()
            {
                AllowedUpdates = [UpdateType.Message],
                DropPendingUpdates = true
            };
            await SetBotCommands(telegramBotClinet);

            try
            {
                updateHandler.OnHandleUpdateStarted += OnUpdateStarted;
                updateHandler.OnHandleUpdateCompleted += OnUpdateCompleted;

                telegramBotClinet.StartReceiving(updateHandler, receiverOptions, ct);

                var me = await telegramBotClinet.GetMe();

                while (true)
                {
                    Console.WriteLine("Enter 'A' and 'Enter' to exit");
                    var s = Console.ReadLine();
                    if (s?.ToUpper() == "A")
                    {
                        cancellationToken.Cancel();
                        Console.WriteLine("Bot stopped");
                        break;
                    }
                    else
                    {
                        Console.WriteLine($"Bot has been started {me.Id} - {me.FirstName} - {me.Username}");
                    }

                }

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
        private static async Task SetBotCommands(ITelegramBotClient botClient)
        {
            var commands = new BotCommand[]
            {
            new BotCommand { Command = "/start", Description = "Запустить бота" },
            new BotCommand { Command = "/help", Description = "Получить помощь" },
            new BotCommand { Command = "/info", Description = "Получить информацию  " },
            new BotCommand { Command = "/addtask", Description = "Добавить задачу" },
            new BotCommand { Command = "/removetask", Description = "Удалить задачу" },
            new BotCommand { Command = "/showalltasks", Description = "Показать все задачи" },
            new BotCommand { Command = "/showtasks", Description = "Показать активные задачи" },
            new BotCommand { Command = "/report", Description = "Получить количество задач" },

        };

            await botClient.SetMyCommands(commands);
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