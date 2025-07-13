INSERT INTO "ToDoUser"("UserId","TelegramUserId","TelegramUserName","RegisteredAt")
VALUES (1, 1234,'alice_dev',now()) 
INSERT INTO "ToDoUser"("UserId","TelegramUserId","TelegramUserName","RegisteredAt")
VALUES (2, 5678, 'maria_notes', now())
INSERT INTO "ToDoList"("Id", "Name", "UserId", "CreatedAt")
VALUES (1, 'Work', 1, now());
INSERT INTO "ToDoList"("Id", "Name", "UserId", "CreatedAt")
VALUES (2, 'Home', 2, now());
INSERT INTO "ToDoList"("Id", "Name", "UserId", "CreatedAt")
VALUES (3, 'Garden', 1, now());
INSERT INTO "ToDoItem"("Id", "UserId", "Name", "CreatedAt", "State", "StateChangedAt", "Deadline", "ListId")
VALUES (1, 1, 'do the task', now(), 0, now(), datetime('now', '+2 days'), 1);
INSERT INTO "ToDoItem"("Id", "UserId", "Name", "CreatedAt", "State", "StateChangedAt", "Deadline", "ListId")
VALUES (2, 2, 'buy the products', now(), 0, now(), datetime('now', '+1 days'), 2);
INSERT INTO "ToDoItem"("Id", "UserId", "Name", "CreatedAt", "State", "StateChangedAt", "Deadline", "ListId")
VALUES (3, 1, 'read the book', now(), 0, now(), datetime('now', '+5 days'), 1);