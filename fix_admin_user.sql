-- Fix admin user with all required fields for ASP.NET Core Identity
UPDATE "AspNetUsers" 
SET 
    "PasswordHash" = 'AQAAAAEAACcQAAAAELbXpFJNk2p+Pj5KqON+u6I+UcWHcbu0TF+Z7JzQn1k=',
    "SecurityStamp" = '5CFSE2STPI62ZURHZAL6SERB3QVGOMRZ',
    "ConcurrencyStamp" = 'c8554266-b401-4a07-8a47-5a919162d79a',
    "EmailConfirmed" = true,
    "PhoneNumberConfirmed" = false,
    "TwoFactorEnabled" = false,
    "LockoutEnabled" = false,
    "AccessFailedCount" = 0
WHERE "Email" = 'admin@msh.local';

-- Ensure admin role exists
INSERT INTO "AspNetRoles" ("Id", "Name", "NormalizedName", "ConcurrencyStamp")
VALUES ('admin-role-id', 'Admin', 'ADMIN', 'admin-role-stamp')
ON CONFLICT ("Id") DO NOTHING;

-- Assign admin role to admin user
INSERT INTO "AspNetUserRoles" ("UserId", "RoleId")
VALUES ('admin-user-id-140f7b56-f4d6-4d95-8731-b16f904232a1', 'admin-role-id')
ON CONFLICT ("UserId", "RoleId") DO NOTHING; 