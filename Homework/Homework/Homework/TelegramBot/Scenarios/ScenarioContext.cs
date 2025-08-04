using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TaskBot.Core.Entities;
using Telegram.Bot.Types;

namespace TaskBot.TelegramBot.Scenarios
{
    internal class ScenarioContext
    {
        public long UserId { get; set; } 
        public ScenarioType CurrentScenario { get; set; }
        public string? CurrentStep { get; set; }
        public Dictionary<string, object> Data { get; set; }
        public DateTime CreatedAt { get; }
         
        public ScenarioContext(ScenarioType scenario)
        {
            CurrentScenario = scenario;
            Data = new Dictionary<string, object>();
            CreatedAt = DateTime.UtcNow;
        }
    }
}
