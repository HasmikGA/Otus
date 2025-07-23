using LinqToDB;
using LinqToDB.Data;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TaskBot.Core.DataAccess.Models;
using TaskBot.Core.Entities;

namespace TaskBot.Infrastructure.DataAccess
{
    public class ToDoDataContext : DataConnection
    {
        public ITable<ToDoUserModel> ToDoUsers => this.GetTable<ToDoUserModel>();
        public ITable<ToDoListModel> ToDoLists => this.GetTable<ToDoListModel>();
        public ITable<ToDoItemModel> ToDoItems => this.GetTable<ToDoItemModel>();
        public ToDoDataContext(string connectionString) : base(ProviderName.PostgreSQL, connectionString) { }

    }
}
