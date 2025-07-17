INSERT INTO "ToDoUser"("UserId","TelegramUserId","TelegramUserName","RegisteredAt")
VALUES ('550e8400-e29b-41d4-a716-446655440000', 1234,'alice_dev',now()) 

INSERT INTO "ToDoUser"("UserId","TelegramUserId","TelegramUserName","RegisteredAt")
VALUES ('7f3a5c1b-8e0f-4e8b-a1b1-3c7a2d932321', 5678, 'maria_notes', now())

INSERT INTO "ToDoList"("Id", "Name", "UserId", "CreatedAt")
VALUES ('e3f6f8b9-7d3f-4e9a-bfd1-3c2f1ad2a943', 'Work', '550e8400-e29b-41d4-a716-446655440000', now());

INSERT INTO "ToDoList"("Id", "Name", "UserId", "CreatedAt")
VALUES ('a9c1e78d-21aa-41f4-9f33-924fb2c74bcd', 'Home', '7f3a5c1b-8e0f-4e8b-a1b1-3c7a2d932321', now());

INSERT INTO "ToDoList"("Id", "Name", "UserId", "CreatedAt")
VALUES ('c4a2dd9e-9c1b-4a2c-879e-b78e9837b312', 'Garden', '550e8400-e29b-41d4-a716-446655440000', now());

INSERT INTO "ToDoItem"("Id", "UserId", "Name", "CreatedAt", "State", "StateChangedAt", "Deadline", "ListId")
VALUES ('fa7e3b64-9c45-4a4e-981b-2fdc2abfef76', '550e8400-e29b-41d4-a716-446655440000', 'do the task', now(), 0, now(), datetime('now', '+2 days'), 'e3f6f8b9-7d3f-4e9a-bfd1-3c2f1ad2a943');

INSERT INTO "ToDoItem"("Id", "UserId", "Name", "CreatedAt", "State", "StateChangedAt", "Deadline", "ListId")
VALUES ('c982b2fc-06fa-4e9d-99e4-3ed9b2f8b6a5', '7f3a5c1b-8e0f-4e8b-a1b1-3c7a2d932321', 'buy the products', now(), 0, now(), datetime('now', '+1 days'), 'a9c1e78d-21aa-41f4-9f33-924fb2c74bcd');

INSERT INTO "ToDoItem"("Id", "UserId", "Name", "CreatedAt", "State", "StateChangedAt", "Deadline", "ListId")
VALUES ('10fbe284-7d44-4ae9-88e8-5c4c231ba0fc', '550e8400-e29b-41d4-a716-446655440000', 'read the book', now(), 0, now(), datetime('now', '+5 days'), 'e3f6f8b9-7d3f-4e9a-bfd1-3c2f1ad2a943');