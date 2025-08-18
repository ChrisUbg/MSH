-- Update admin user password to "password123"
UPDATE "AspNetUsers" 
SET "PasswordHash" = 'AQAAAAIAAYagAAAAELbXpFJNk2p+Pj5KqON+u6I+UcWHcbu0TF+Z7JzQn1k=' 
WHERE "UserName" = 'admin@msh.local'; 