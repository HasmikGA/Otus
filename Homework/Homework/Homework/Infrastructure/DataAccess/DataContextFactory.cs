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
        private readonly string connectionString;
        public DataContextFactory(string connectionString) 
        {
            this.connectionString = connectionString;
        }
        public ToDoDataContext CreateDataContext()
        {
            return new ToDoDataContext(connectionString);
        }
    }
}
