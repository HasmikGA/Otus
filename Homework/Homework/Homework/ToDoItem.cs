using Otus.ToDoList.ConsoleBot.Types;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TaskBot.Exceptions;

namespace TaskBot
{
    internal class ToDoItem 
    {
        private string name;
        public Guid Id { get; set; }
        public ToDoUser User { get; set; }
        public string Name
        {
            get
            {
                return name;
            }
            set
            {
                if (value.Length > ItemlengthLimit)
                {
                    throw new TaskLengthLimitException(value.Length, ItemlengthLimit);
                }

                this.name = value;
            }
        }
        public DateTime CreatedAt { get; set; }
        public ToDoItemState State { get; set; }
        public DateTime? StateChangedAt { get; set; }
        public int ItemlengthLimit { get; set; } = 100;


    }
}
