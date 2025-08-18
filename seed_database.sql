                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                -- MSH Database Seeding Script
-- This script creates all necessary master data for the MSH application

-- 1. Create System User (required for all entity relationships)
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
    "LastLogin" = NOW(),
    "UpdatedAt" = NOW();

-- 2. Create Admin User
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
    'admin-user-id-1234-5678-9012-345678901234',
    'admin',
    'admin@msh.local',
    'Admin',
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
    "LastLogin" = NOW(),
    "UpdatedAt" = NOW();

-- 3. Create Default Rooms
INSERT INTO "Rooms" (
    "Id",
    "Name",
    "Description",
    "Floor",
    "CreatedAt",
    "UpdatedAt",
    "IsDeleted",
    "CreatedById",
    "UpdatedById"
) VALUES 
    (
        gen_random_uuid(),
        'Living Room',
        'Main living area',
        1,
        NOW(),
        NOW(),
        false,
        'bb1be326-f26e-4684-bbf5-5c3df450dc61',
        'bb1be326-f26e-4684-bbf5-5c3df450dc61'
    ),
    (
        gen_random_uuid(),
        'Kitchen',
        'Kitchen area',
        1,
        NOW(),
        NOW(),
        false,
        'bb1be326-f26e-4684-bbf5-5c3df450dc61',
        'bb1be326-f26e-4684-bbf5-5c3df450dc61'
    ),
    (
        gen_random_uuid(),
        'Bedroom',
        'Bedroom area',
        1,
        NOW(),
        NOW(),
        false,
        'bb1be326-f26e-4684-bbf5-5c3df450dc61',
        'bb1be326-f26e-4684-bbf5-5c3df450dc61'
    ),
    (
        gen_random_uuid(),
        'Bathroom',
        'Bathroom area',
        1,
        NOW(),
        NOW(),
        false,
        'bb1be326-f26e-4684-bbf5-5c3df450dc61',
        'bb1be326-f26e-4684-bbf5-5c3df450dc61'
    )
ON CONFLICT DO NOTHING;

-- 4. Create Default Device Groups
INSERT INTO "DeviceGroups" (
    "Id",
    "Name",
    "Description",
    "RoomId",
    "CreatedAt",
    "UpdatedAt",
    "IsDeleted",
    "CreatedById",
    "UpdatedById"
) VALUES 
    (
        gen_random_uuid(),
        'Smart Plugs & Sockets',
        'Power control devices for appliances and lighting',
        NULL,
        NOW(),
        NOW(),
        false,
        'bb1be326-f26e-4684-bbf5-5c3df450dc61',
        'bb1be326-f26e-4684-bbf5-5c3df450dc61'
    ),
    (
        gen_random_uuid(),
        'Smart Lighting',
        'LED bulbs, strips, and lighting controls',
        NULL,
        NOW(),
        NOW(),
        false,
        'bb1be326-f26e-4684-bbf5-5c3df450dc61',
        'bb1be326-f26e-4684-bbf5-5c3df450dc61'
    ),
    (
        gen_random_uuid(),
        'Smart Sensors',
        'Environmental and security sensors',
        NULL,
        NOW(),
        NOW(),
        false,
        'bb1be326-f26e-4684-bbf5-5c3df450dc61',
        'bb1be326-f26e-4684-bbf5-5c3df450dc61'
    ),
    (
        gen_random_uuid(),
        'Smart Switches',
        'Wall switches and dimmers',
        NULL,
        NOW(),
        NOW(),
        false,
        'bb1be326-f26e-4684-bbf5-5c3df450dc61',
        'bb1be326-f26e-4684-bbf5-5c3df450dc61'
    ),
    (
        gen_random_uuid(),
        'Other Devices',
        'Other Matter-compatible devices',
        NULL,
        NOW(),
        NOW(),
        false,
        'bb1be326-f26e-4684-bbf5-5c3df450dc61',
        'bb1be326-f26e-4684-bbf5-5c3df450dc61'
    )
ON CONFLICT DO NOTHING;

-- 5. Create Default Device Types
INSERT INTO "DeviceTypes" (
    "Id",
    "Name",
    "Description",
    "Capabilities",
    "IsSimulated",
    "CreatedAt",
    "UpdatedAt",
    "IsDeleted",
    "CreatedById",
    "UpdatedById"
) VALUES 
    (
        gen_random_uuid(),
        'Smart Plug',
        'Matter-compatible smart plug with power monitoring',
        '{"on_off": true, "power_monitoring": true}',
        false,
        NOW(),
        NOW(),
        false,
        'bb1be326-f26e-4684-bbf5-5c3df450dc61',
        'bb1be326-f26e-4684-bbf5-5c3df450dc61'
    ),
    (
        gen_random_uuid(),
        'Smart Switch',
        'Matter-compatible smart switch',
        '{"on_off": true}',
        false,
        NOW(),
        NOW(),
        false,
        'bb1be326-f26e-4684-bbf5-5c3df450dc61',
        'bb1be326-f26e-4684-bbf5-5c3df450dc61'
    ),
    (
        gen_random_uuid(),
        'Smart Bulb',
        'Matter-compatible smart bulb with color control',
        '{"on_off": true, "color_control": true}',
        false,
        NOW(),
        NOW(),
        false,
        'bb1be326-f26e-4684-bbf5-5c3df450dc61',
        'bb1be326-f26e-4684-bbf5-5c3df450dc61'
    ),
    (
        gen_random_uuid(),
        'NOUS A8M Socket',
        'NOUS A8M 16A Smart Socket with Matter support',
        '{"onOff": true, "maxPower": 3680, "powerMonitoring": true, "energyMonitoring": true}',
        false,
        NOW(),
        NOW(),
        false,
        'bb1be326-f26e-4684-bbf5-5c3df450dc61',
        'bb1be326-f26e-4684-bbf5-5c3df450dc61'
    ),
    (
        gen_random_uuid(),
        'Temperature Sensor',
        'Matter-compatible temperature sensor',
        '{"temperature": true, "humidity": true}',
        false,
        NOW(),
        NOW(),
        false,
        'bb1be326-f26e-4684-bbf5-5c3df450dc61',
        'bb1be326-f26e-4684-bbf5-5c3df450dc61'
    ),
    (
        gen_random_uuid(),
        'Motion Sensor',
        'Matter-compatible motion sensor',
        '{"motion": true, "occupancy": true}',
        false,
        NOW(),
        NOW(),
        false,
        'bb1be326-f26e-4684-bbf5-5c3df450dc61',
        'bb1be326-f26e-4684-bbf5-5c3df450dc61'
    )
ON CONFLICT DO NOTHING;

-- 6. Create ASP.NET Core Identity Roles
INSERT INTO "AspNetRoles" ("Id", "Name", "NormalizedName", "ConcurrencyStamp")
VALUES 
    ('admin-role-id', 'Admin', 'ADMIN', 'admin-role-stamp'),
    ('standard-role-id', 'Standard', 'STANDARD', 'standard-role-stamp'),
    ('guest-role-id', 'Guest', 'GUEST', 'guest-role-stamp')
ON CONFLICT ("Id") DO NOTHING;

-- 7. Create ASP.NET Core Identity Admin User
INSERT INTO "AspNetUsers" (
    "Id",
    "UserName",
    "NormalizedUserName",
    "Email",
    "NormalizedEmail",
    "EmailConfirmed",
    "PasswordHash",
    "SecurityStamp",
    "ConcurrencyStamp",
    "PhoneNumberConfirmed",
    "TwoFactorEnabled",
    "LockoutEnabled",
    "AccessFailedCount"
) VALUES (
    'admin-user-id-1234-5678-9012-345678901234',
    'admin@msh.local',
    'ADMIN@MSH.LOCAL',
    'admin@msh.local',
    'ADMIN@MSH.LOCAL',
    true,
    'AQAAAAEAACcQAAAAELbXpFJNk2p+Pj5KqON+u6I+UcWHcbu0TF+Z7JzQn1k=',
    '5CFSE2STPI62ZURHZAL6SERB3QVGOMRZ',
    'c8554266-b401-4a07-8a47-5a919162d79a',
    false,
    false,
    false,
    0
) ON CONFLICT ("Id") DO UPDATE SET
    "UserName" = EXCLUDED."UserName",
    "NormalizedUserName" = EXCLUDED."NormalizedUserName",
    "Email" = EXCLUDED."Email",
    "NormalizedEmail" = EXCLUDED."NormalizedEmail",
    "EmailConfirmed" = EXCLUDED."EmailConfirmed",
    "PasswordHash" = EXCLUDED."PasswordHash",
    "SecurityStamp" = EXCLUDED."SecurityStamp",
    "ConcurrencyStamp" = EXCLUDED."ConcurrencyStamp";

-- 8. Assign Admin Role to Admin User
INSERT INTO "AspNetUserRoles" ("UserId", "RoleId")
VALUES ('admin-user-id-1234-5678-9012-345678901234', 'admin-role-id')
ON CONFLICT ("UserId", "RoleId") DO NOTHING;

-- Log successful seeding
\echo 'MSH Database seeded successfully with all required master data'; 