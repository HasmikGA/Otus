using System.IO;
using TaskBot;
using TaskBot.BackgroundTasks;
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
            string connectionString = "Host=localhost;Port=5432;Username=postgres;Password=1234;Database=ToDoList";
            var telegramBotClinet = new TelegramBotClient(clToken);
            var dataContextFactory = new DataContextFactory(connectionString);
            
            IUserRepository userRepository = new SqlUserRepository(dataContextFactory);
            IToDoRepository toDoRepository = new SqlToDoRepository(dataContextFactory);
            IScenarioContextRepository contextRepository = new InMemoryScenarioContextRepository();
            IToDoReportService toDoReportService = new ToDoReportService(toDoRepository);
            IToDoListRepository toDoListRepository = new SqlToDoListRepository(dataContextFactory);
            IToDoListService toDoListService = new ToDoListService(toDoListRepository);
            BackgroundTaskRunner backgroundTaskRunner = new BackgroundTaskRunner();
            backgroundTaskRunner.AddTask(new ResetScenarioBackgroundTask(TimeSpan.FromMinutes(5), contextRepository, telegramBotClinet));

            var userService = new UserService(userRepository);
            var toDoService = new ToDoService(20, 100, toDoRepository);
            var scenarios = new IScenario[]
            { 
                new AddTaskScenario(userService, toDoService, toDoListService),
                new AddListScenario(userService, toDoListService)
            };

            var updateHandler = new UpdateHandler(userService, toDoService, new ToDoReportService(toDoRepository), scenarios, contextRepository, toDoListService);
            CancellationTokenSource cancellationToken = new CancellationTokenSource();
            CancellationToken ct = cancellationToken.Token;
            ReceiverOptions receiverOptions = new ReceiverOptions()
            {
                AllowedUpdates = [UpdateType.Message, UpdateType.CallbackQuery],
                DropPendingUpdates = true
            };
            await SetBotCommands(telegramBotClinet);

            try
            {
                updateHandler.OnHandleUpdateStarted += OnUpdateStarted;
                updateHandler.OnHandleUpdateCompleted += OnUpdateCompleted;

                backgroundTaskRunner.StartTasks(ct);
                telegramBotClinet.StartReceiving(updateHandler, receiverOptions, ct);

                var me = await telegramBotClinet.GetMe();

                while (true)
                {
                    Console.WriteLine("Enter 'A' and 'Enter' to exit");
                    var s = Console.ReadLine();
                    if (s?.ToUpper() == "A")
                    {
                        await backgroundTaskRunner.StopTasks(ct);
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