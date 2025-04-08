namespace TaskBot.Exceptions
{
    class TaskCountLimitException : Exception
    {
        public TaskCountLimitException(int taskCountLimit) : base($"The max number of tasks has been exceeded: {taskCountLimit} ") { }
    }
}