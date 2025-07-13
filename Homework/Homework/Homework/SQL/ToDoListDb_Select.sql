SELECT COUNT(*)
FROM "ToDoItem" i
WHERE i."UserId" IN (SELECT u."UserId" FROM "ToDoUser" u)
  AND i."State" = 0;

SELECT * FROM "ToDoItem" i
WHERE i."UserId" IN(SELECT u."UserId" From "ToDoUser" u);

SELECT * FROM "ToDoItem" i
WHERE i."UserId" IN(SELECT u."UserId" FROM "ToDoUser" u)
AND i."UserId" IN(SELECT l."UserId" FROM "ToDoList" l);

DELETE FROM "ToDoItem" 
WHERE "Id"= '1';

SELECT * FROM "ToDoItem"
WHERE "Name" = 'Read the book';