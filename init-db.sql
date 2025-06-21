-- MSH Database Initialization Script
-- This script runs automatically when the PostgreSQL container starts for the first time

-- Ensure the database exists
\c msh;

-- Create required users if they don't exist
INSERT INTO "ApplicationUsers" ("Id", "UserName", "Email", "FirstName", "LastName", "IsActive", "IsDeleted", "CreatedAt", "UpdatedAt", "CreatedById", "UpdatedById")
VALUES 
    ('bb1be326-f26e-4684-bbf5-5c3df450dc61', 'system', 'system@msh.local', 'System', 'User', true, false, NOW(), NOW(), 'bb1be326-f26e-4684-bbf5-5c3df450dc61', 'bb1be326-f26e-4684-bbf5-5c3df450dc61'),
    ('admin-user-id-1234-5678-9012-345678901234', 'admin', 'admin@msh.local', 'Admin', 'User', true, false, NOW(), NOW(), 'bb1be326-f26e-4684-bbf5-5c3df450dc61', 'bb1be326-f26e-4684-bbf5-5c3df450dc61')
ON CONFLICT ("Id") DO NOTHING;

-- Ensure postgres user password is set correctly
ALTER USER postgres PASSWORD 'postgres';

-- Log successful initialization
\echo 'MSH Database initialized successfully'; 