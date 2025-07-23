using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Linq;
using TaskBot.Core.DataAccess.Models;
using TaskBot.Core.Entities;
using Telegram.Bot.Types;

namespace TaskBot.Infrastructure.DataAccess
{
    internal static class ModelMapper
    {
        public static ToDoUser MapFromModel(ToDoUserModel model)
        {
            if (model == null)
            {
                return null;
            }
            return new ToDoUser()
            {
                UserId = model.UserId,
                TelegramUserId = model.TelegramUserId,
                TelegramUserName = model.TelegramUserName,
                RegisteredAt = model.RegisteredAt,
            };
        }
        public static ToDoUserModel MapToModel(ToDoUser entity)
        {
            if (entity == null)
            {
                return null;
            }
            return new ToDoUserModel()
            {
                UserId = entity.UserId,
                TelegramUserId = entity.TelegramUserId,
                TelegramUserName = entity.TelegramUserName,
                RegisteredAt = entity.RegisteredAt,
            };
        }
        public static ToDoItem MapFromModel(ToDoItemModel model)
        {
            if (model == null)
            {
                return null;
            }
            return new ToDoItem()
            {
                Id = model.Id,
                User = model.User != null ? model.User : null,
                Name = model.Name,
                CreatedAt = model.CreatedAt,
                State = model.State,
                StateChangedAt = model.StateChangedAt,
                Deadline = model.Deadline,
                List = model.List != null ? model.List : null

            };
        }
        public static ToDoItemModel MapToModel(ToDoItem entity)
        {
            if (entity == null)
            {
                return null;
            }
            return new ToDoItemModel()
            {
                Id = entity.Id,
                UserId = entity.User.UserId,
                Name = entity.Name,
                CreatedAt = entity.CreatedAt,
                State = entity.State,
                StateChangedAt = entity.StateChangedAt,
                Deadline = entity.Deadline,
                ListId = entity.List.Id,
            };
        }

        public static ToDoList MapFromModel(ToDoListModel model)
        {
            if(model == null)
            {
                return null;
            }
            return new ToDoList()
            {
                Id = model.Id,
                Name = model.Name,
                User = model.User,
                CreatedAt = model.CreatedAt,
            };
        }
        public static ToDoListModel MapToModel(ToDoList entity)
        {
            if (entity == null)
            {
                return null;
            }
            return new ToDoListModel()
            {
                Id = entity.Id,
                Name = entity.Name,
                UserId = entity.User.UserId,
                CreatedAt = entity.CreatedAt
            };
        }
    }
}
