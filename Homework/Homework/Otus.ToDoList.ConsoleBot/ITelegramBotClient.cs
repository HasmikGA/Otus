using Otus.ToDoList.ConsoleBot.Types;

namespace Otus.ToDoList.ConsoleBot;
public interface ITelegramBotClient
{
    void StartReceiving(IUpdateHandler handler, string initialCommands);
    void SendMessage(Chat chat, string text);
}
