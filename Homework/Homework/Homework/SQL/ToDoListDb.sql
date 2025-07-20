CREATE TABLE "ToDoUser" (
"UserId" UUID PRIMARY KEY,
"TelegramUserId" BIGINT NOT NULL,
"TelegramUserName" TEXT,
"RegisteredAt" TIMESTAMP NOT NULL DEFAULT NOW()
)
CREATE TABLE "ToDoList" (
"Id" UUID PRIMARY KEY,
"Name" TEXT NOT NULL,
"UserId" UUID NOT NULL REFERENCES "ToDoUser"("UserId"),
"CreatedAt" TIMESTAMP NOT NULL DEFAULT NOW())

CREATE TABLE "ToDoItem" (
"Id" UUID PRIMARY KEY,
"UserId" UUID REFERENCES "ToDoUser" ("UserId"),
"Name" TEXT,
"CreatedAt" TIMESTAMP NOT NULL DEFAULT NOW(),
"State" INT NOT NULL DEFAULT 0,
"StateChangedAt" TIMESTAMP,
"Deadline" TIMESTAMP NOT NULL,
"ListId" UUID REFERENCES "ToDoList" ("Id") )

CREATE UNIQUE INDEX "Ix_ToDoUser_TelegramUserId"
ON "ToDoUser"("TelegramUserId")

CREATE INDEX "Ix_ToDoItem_ListId" 
ON "ToDoItem"("ListId")

CREATE INDEX "Ix_ToDoItem_UserId" 
ON "ToDoItem"("UserId")

CREATE INDEX "Ix_ToDoList_UserId"
ON "ToDoList"("UserId")