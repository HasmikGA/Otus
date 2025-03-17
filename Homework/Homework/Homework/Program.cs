using System;

namespace Homework
{
    internal class Program
    {
        static void Main(string[] args)
        {

            ProcessCommands();

            Console.ReadKey();
        }

        static void ProcessCommands()
        {
            Console.Write("Hi! There are the commands:");
            string commands = "start, help, info, addtask, removetask, showtasks, exit.";
            string name = string.Empty;
            List<string> taskList = new List<string>();

            while (true)
            {
                Console.WriteLine($"{name} You can choose one of the commands: {commands}");
                string command = Console.ReadLine();
                switch (command)
                {
                    case "start":
                        name = Starting();
                        commands = "help, info, echo, exit";
                        break;
                    case "help":
                        Helping(name);
                        commands = string.IsNullOrEmpty(name) ? "start, info, exit" : "info, echo, exit";
                        break;
                    case "info":
                        ProvideInfo(name);
                        commands = string.IsNullOrEmpty(name) ? "start, help, exit" : "help, echo, exit";
                        break;
                    case "echo":
                        Echo(name);
                        commands = "help, info, echo, exit";
                        break;
                    case "addtask":
                        AddTask(taskList);
                        break;
                    case "showtasks":
                        ShowTasks(taskList);
                        break;
                    case "removetask":
                        RemoveTask(taskList);
                        break;
                    case "exit":
                        Exit();
                        return;
                    default:
                        Console.WriteLine("The command isn`t correct.");
                        break;
                }
            }

        }

        static string Starting()
        {
            Console.WriteLine("Write your name, please!");
            string name = Console.ReadLine();
            return name;
        }

        static void ProvideInfo(string name)
        {
            if (name != null && name != string.Empty)
            {
                Console.Write($"{name}!");
            }

            Console.WriteLine("This is the 1.0.0 version of programm, which was created in 2025.");
        }

        static void Helping(string name)
        {
            if (!string.IsNullOrEmpty(name))
            {
                Console.Write($"{name}!");
            }

            Console.WriteLine("1.Open the programm and follow the commands you see. \n2.You should choose one of those. \n" +
                              "3.You have to write the command as you see it. \n4.Recommend you to start with command start.");
            Console.WriteLine("Addtask - command to add a task for execution.\n Showtasks - command to see all tasks you have.\n" +
                              "Removetask - command to remove any command you want");
        }

        static void Echo(string name)
        {
            Console.WriteLine($"{name}! Add a massage, you want return, after the command echo: ");
            string massage = Console.ReadLine();
            Console.WriteLine(massage);
        }

        static void ShowTasks(List<string> taskList)
        {
            Console.WriteLine("Here is your tasklist:");
            for (int i = 0; i < taskList.Count; i++)
            {
                Console.WriteLine($"{i + 1}. {taskList[i]}");
            }
        }
        static void AddTask(List<string> taskList)
        {
            Console.Write("Please enter a description of the task:");
            string task = Console.ReadLine();
            taskList.Add(task);
            Console.WriteLine($"The task \"{task}\" has been added. ");

        }
        static void RemoveTask(List<string> taskList)
        {
            if (taskList.Count == 0)
            {
                Console.WriteLine("The tasklist is empty.");
                return;
            }

            ShowTasks(taskList);

            while (true)
            {
                Console.Write("Enter the task number to remove:");
                int taskNumber;
                string number = Console.ReadLine();
                bool isTaskNumber = int.TryParse(number, out taskNumber);
                if (isTaskNumber && taskNumber <= taskList.Count && taskNumber > 0)
                {
                    int indexOfNum = taskNumber - 1;
                    var itemToRemove = taskList[indexOfNum];
                    taskList.RemoveAt(indexOfNum);
                    Console.WriteLine($"The task \"{itemToRemove}\" has been removed");
                    break;
                }
                else
                {
                    Console.WriteLine("Wrong number!");
                    continue;
                }
            }
        }
        static void Exit()
        {
            Console.WriteLine("The program is over!");
        }
    }
}