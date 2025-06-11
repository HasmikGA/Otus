using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TaskBot.TelegramBot.Scenarios
{
    internal class InMemoryScenarioContextRepository : IScenarioContextRepository
    {
        Dictionary<long, ScenarioContext> scenarios = new Dictionary<long, ScenarioContext>();
        public Task<ScenarioContext?> GetContext(long userId, CancellationToken ct)
        {
            ScenarioContext? context = null;
            if (scenarios.ContainsKey(userId))
            {
                context = this.scenarios[userId];
            }

            return Task.FromResult(context);
        }

        public Task SetContext(long userId, ScenarioContext context, CancellationToken ct)
        {
            this.scenarios[userId] = context;
            return Task.CompletedTask;
        }

        public Task ResetContext(long userId, CancellationToken ct)
        {
            this.scenarios.Remove(userId);
            return Task.CompletedTask;
        }
    }
}
