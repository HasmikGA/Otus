using LinqToDB.Mapping;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TaskBot.Core.Entities;

namespace TaskBot.Core.DataAccess.Models
{
    [Table("ToDoList")]
    public class ToDoListModel
    {
        [PrimaryKey]
        [Column("Id")]
        public Guid Id { get; set; }

        [Column("Name"), NotNull]
        public string Name { get; set; }

        [Column("UserId"), NotNull]
        public Guid UserId { get; set; }

        [Association(ThisKey = nameof(UserId), OtherKey = nameof(User.UserId))]
        public ToDoUser User { get; set; }

        [Column("CreatedAt"), NotNull]
        public DateTime CreatedAt { get; set; }
    }
}
