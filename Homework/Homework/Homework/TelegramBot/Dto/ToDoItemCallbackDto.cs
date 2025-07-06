using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TaskBot.TelegramBot.Dto
{
    internal class ToDoItemCallbackDto: CallbackDto
    {
        public Guid? ToDoItemId { get; set; }

        public static new ToDoItemCallbackDto FromString(string input)
        {
            var inputs = input.Split('|');
            var success = Guid.TryParse(inputs[1], out Guid toDoItemId);
            var result = new ToDoItemCallbackDto
            {
                Action = inputs[0],
                ToDoItemId = toDoItemId
            };

            return result;
        }

        public override string ToString()
        {
            return $"{base.ToString()}|{ToDoItemId}";
        }
    }
}
