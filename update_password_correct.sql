-- Update admin user password to "password123" with correct hash
UPDATE "AspNetUsers" 
SET "PasswordHash" = 'AQAAAAIAAYagAAAAELyWDdV6BZW6Wphay+PndNns9i7//buNePTXoNzgSVkM1V0D3Zh+Wgmit8Rn5j+VlQ==' 
WHERE "UserName" = 'admin@msh.local'; 