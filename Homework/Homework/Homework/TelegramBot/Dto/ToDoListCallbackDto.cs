namespace TaskBot.TelegramBot.Dto
{
    internal class ToDoListCallbackDto : CallbackDto
    {
        public Guid? ToDoListId { get; set; }

        public static new ToDoListCallbackDto FromString(string input)
        {
            var inputs = input.Split('|');
            var success = Guid.TryParse(inputs[1], out Guid toDoListId);
            var result = new ToDoListCallbackDto
            {
                Action = inputs[0],
                ToDoListId = toDoListId
            };

            return result;
        }

        public override string ToString()
        {
            return $"{base.ToString()}|{ToDoListId}";
        }
    }
}
