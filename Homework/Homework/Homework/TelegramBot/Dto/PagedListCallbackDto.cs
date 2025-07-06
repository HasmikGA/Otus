namespace TaskBot.TelegramBot.Dto
{
    internal class PagedListCallbackDto : ToDoListCallbackDto
    {
        public int Page { get; set; }
        public static new PagedListCallbackDto FromString(string input)
        {
            var basePart = ToDoListCallbackDto.FromString(input);
            var parts = input.Split('|');
            int page = parts.Length > 1 && int.TryParse(parts[^1], out var p) ? p : 0;

            var result = new PagedListCallbackDto
            {
                Action = basePart.Action,
                ToDoListId = basePart.ToDoListId,
                Page = page,
            };

            return result;
        }
        public override string ToString()
        {
            return $"{base.ToString()}|{Page}";
        }
    }
}
