using LinqToDB.Mapping;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TaskBot.Core.DataAccess.Models
{
    [Table("ToDoUser")]
    public class ToDoUserModel
    {
        [PrimaryKey]
        [Column("UserId"),NotNull]
        public Guid UserId { get; set; }

        [Column("TelegramUserId"),NotNull]
        public long TelegramUserId { get; set; }

        [Column("TelegramUserName")]
        public string? TelegramUserName { get; set; }

        [Column ("RegisteredAt"),NotNull]
        public DateTime RegisteredAt { get; set; }

    }
}
