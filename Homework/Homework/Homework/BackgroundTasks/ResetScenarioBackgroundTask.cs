using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TaskBot.TelegramBot.Scenarios;
using Telegram.Bot;
using Telegram.Bot.Types.ReplyMarkups;
using static LinqToDB.Reflection.Methods.LinqToDB;

namespace TaskBot.BackgroundTasks
{
    internal class ResetScenarioBackgroundTask : BackgroundTask
    {
        private readonly TimeSpan resetScenarioTimeout;
        private readonly IScenarioContextRepository scenarioRepository;
        private readonly ITelegramBotClient bot;
        

        public ResetScenarioBackgroundTask(TimeSpan resetScenarioTimeout, IScenarioContextRepository scenarioRepository, ITelegramBotClient bot)
            :base(TimeSpan.FromHours(1), nameof(ResetScenarioBackgroundTask))
        {
            this.resetScenarioTimeout = resetScenarioTimeout;
            this.scenarioRepository = scenarioRepository;
            this.bot = bot;
            
        }

        protected async override Task Execute(CancellationToken ct)
        {
            var contexts = await scenarioRepository.GetContexts(ct);
            foreach(var context in contexts)
            {
                if(DateTime.UtcNow - context.CreatedAt >= resetScenarioTimeout)
                {
                    await scenarioRepository.ResetContext(context.UserId, ct);
                    var mеssege = $"The scenario was canceled because no response was received within {resetScenarioTimeout}.";
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

                    await bot.SendMessage((long)context.Data["ChatId"], mеssege, replyMarkup: reply, cancellationToken: ct);
                };
            }
        }
    }

}
