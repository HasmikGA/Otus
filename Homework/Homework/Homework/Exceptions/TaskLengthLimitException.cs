namespace TaskBot.Exceptions
{
    class TaskLengthLimitException : Exception
    {
        public TaskLengthLimitException(int taskLength, int taskLengthLimit)
            : base($"The length of the task {taskLength} axceeds the max allowed length {taskLengthLimit} ") { }
    }
}