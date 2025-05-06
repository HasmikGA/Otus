namespace TaskBot.Core.Exceptions
{
    class DuplicateTaskException : Exception
    {

        public DuplicateTaskException(string task) : base($"The task \"{task}\"already exists") { }
    }
}