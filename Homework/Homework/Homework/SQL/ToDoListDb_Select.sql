SELECT COUNT(*)
FROM "ToDoItem" i
JOIN "ToDoUser" u ON i."UserId"=u."UserId"
WHERE i."State"=0;

SELECT * FROM "ToDoItem" i
JOIN "ToDoUser" u ON i."UserId" = u."UserId";

SELECT * FROM "ToDoItem" i
JOIN "ToDoUser" u ON i."UserId"= u."UserId"
JOIN "ToDoList" l ON i."UserId" = l."UserId";

DELETE FROM "ToDoItem" 
WHERE "Id"= '7f3a5c1b-8e0f-4e8b-a1b1-3c7a2d932321';

SELECT * FROM "ToDoItem"
WHERE "Name" = 'Read the book';