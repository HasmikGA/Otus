using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TaskBot.Core.DataAccess;

namespace TaskBot.Infrastructure.DataAccess
{
    internal class DataContextFactory : IDataContextFactory<ToDoDataContext>
    {
        public ToDoDataContext CreateDataContext()
        {
            return new ToDoDataContext("Host=localhost;Port=5432;Username=postgres;Password=1234;Database=ToDoList");
        }
    }
}
