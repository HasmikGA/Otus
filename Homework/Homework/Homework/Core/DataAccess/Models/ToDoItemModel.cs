using LinqToDB.Mapping;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TaskBot.Core.Entities;

namespace TaskBot.Core.DataAccess.Models
{
    [Table("ToDoItem")]
    public class ToDoItemModel
    {
        [PrimaryKey]
        [Column("Id")]
        public Guid Id { get; set; }

        [Column("UserId"), NotNull]
        public Guid UserId { get; set; }

        [Association(ThisKey = nameof(UserId), OtherKey = nameof(User.UserId))]
        public ToDoUser? User { get; set; }

        [Column("Name")]
        public string? Name { get; set; }

        [Column("CreatedAt"),NotNull]
        public DateTime CreatedAt { get; set; }

        [Column("State"),NotNull]
        public ToDoItemState State { get; set; }

        [Column("StateChangedAt")]
        public DateTime? StateChangedAt { get; set; }

        [Column("Deadline"),NotNull]
        public DateTime Deadline { get; set; }

        [Column("ListId")]
        public Guid ListId { get; set; }

        [Association(ThisKey = nameof(ListId), OtherKey = nameof(List.Id))]
        public ToDoList? List { get; set; }


    }
}
