using LinqToDB.Mapping;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TaskBot.Core.Entities;

namespace TaskBot.Core.DataAccess.Models
{
    [Table("Notification")]
    public  class NotificationModel
    {
        [PrimaryKey]
        [Column("Id")]
        public Guid Id { get; set; }

        [Column("UserId"), NotNull]
        public Guid UserId { get; set; }

        [Association(ThisKey = nameof(UserId), OtherKey = nameof(User.UserId))]
        public ToDoUser? User { get; set; }

        [Column("Type"),NotNull]
        public string? Type { get; set; }

        [Column("Text"),NotNull]
        public string? Text { get; set; }

        [Column("ScheduledAt"), NotNull]
        public DateTime ScheduledAt { get; set; }

        [Column("IsNotified"), NotNull]
        public bool IsNotified { get; set; } = false;

        [Column("NotifiedAt"), NotNull]
        public DateTime NotifiedAt { get; set; }

    }
}
