-- First, create the system user with self-reference
INSERT INTO "ApplicationUsers" (
    "Id", 
    "UserName", 
    "Email", 
    "FirstName", 
    "LastName", 
    "IsActive", 
    "LastLogin", 
    "CreatedAt", 
    "UpdatedAt", 
    "IsDeleted", 
    "CreatedById", 
    "UpdatedById"
) VALUES (
    'bb1be326-f26e-4684-bbf5-5c3df450dc61',
    'system',
    'system@msh.local',
    'System',
    'User',
    true,
    NOW(),
    NOW(),
    NOW(),
    false,
    'bb1be326-f26e-4684-bbf5-5c3df450dc61',
    'bb1be326-f26e-4684-bbf5-5c3df450dc61'
) ON CONFLICT ("Id") DO UPDATE SET
    "UserName" = EXCLUDED."UserName",
    "Email" = EXCLUDED."Email",
    "FirstName" = EXCLUDED."FirstName",
    "LastName" = EXCLUDED."LastName",
    "IsActive" = EXCLUDED."IsActive",
    "LastLogin" = EXCLUDED."LastLogin",
    "UpdatedAt" = NOW(),
    "CreatedById" = EXCLUDED."CreatedById",
    "UpdatedById" = EXCLUDED."UpdatedById";

-- Now update the existing admin user to reference the system user
UPDATE "ApplicationUsers" 
SET 
    "CreatedById" = 'bb1be326-f26e-4684-bbf5-5c3df450dc61',
    "UpdatedById" = 'bb1be326-f26e-4684-bbf5-5c3df450dc61',
    "IsActive" = true,
    "LastLogin" = NOW(),
    "UpdatedAt" = NOW()
WHERE "Email" = 'admin@msh.local'; 