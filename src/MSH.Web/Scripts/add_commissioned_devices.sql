-- Add commissioned NOUS A8M devices to the database
-- These devices have been successfully commissioned and are communicating via OTBR

-- First, ensure we have the required device type
INSERT INTO "DeviceTypes" ("Id", "Name", "Description", "CreatedAt", "UpdatedAt", "IsDeleted")
VALUES (
    gen_random_uuid(),
    'Smart Socket',
    'NOUS A8M Smart Socket with Matter support',
    NOW(),
    NOW(),
    false
) ON CONFLICT DO NOTHING;

-- Get the device type ID
DO $$
DECLARE
    device_type_id UUID;
    admin_user_id UUID := 'bb1be326-f26e-4684-bbf5-5c3df450dc61'; -- Admin user ID from the application
BEGIN
    -- Get the device type ID
    SELECT "Id" INTO device_type_id FROM "DeviceTypes" WHERE "Name" = 'Smart Socket' LIMIT 1;
    
    -- Add Office Socket 1 (Node ID: 4328ED19954E9DC0)
    INSERT INTO "Devices" (
        "Id", 
        "Name", 
        "Description", 
        "MatterDeviceId", 
        "DeviceTypeId", 
        "Status", 
        "LastSeen", 
        "Configuration", 
        "Properties", 
        "CreatedAt", 
        "UpdatedAt", 
        "IsDeleted", 
        "CreatedById", 
        "UpdatedById"
    ) VALUES (
        gen_random_uuid(),
        'Office Socket 1',
        'NOUS A8M Smart Socket - Office Area',
        '4328ED19954E9DC0',
        device_type_id,
        'online',
        NOW(),
        '{}'::jsonb,
        '{"manufacturer": "NOUS", "model": "A8M", "matter_version": "1.0"}'::jsonb,
        NOW(),
        NOW(),
        false,
        admin_user_id,
        admin_user_id
    ) ON CONFLICT ("MatterDeviceId") DO UPDATE SET
        "Status" = 'online',
        "LastSeen" = NOW(),
        "UpdatedAt" = NOW();
    
    -- Add Office Socket 2 (Node ID: 4328ED19954E9DC1)
    INSERT INTO "Devices" (
        "Id", 
        "Name", 
        "Description", 
        "MatterDeviceId", 
        "DeviceTypeId", 
        "Status", 
        "LastSeen", 
        "Configuration", 
        "Properties", 
        "CreatedAt", 
        "UpdatedAt", 
        "IsDeleted", 
        "CreatedById", 
        "UpdatedById"
    ) VALUES (
        gen_random_uuid(),
        'Office Socket 2',
        'NOUS A8M Smart Socket - Office Area',
        '4328ED19954E9DC1',
        device_type_id,
        'online',
        NOW(),
        '{}'::jsonb,
        '{"manufacturer": "NOUS", "model": "A8M", "matter_version": "1.0"}'::jsonb,
        NOW(),
        NOW(),
        false,
        admin_user_id,
        admin_user_id
    ) ON CONFLICT ("MatterDeviceId") DO UPDATE SET
        "Status" = 'online',
        "LastSeen" = NOW(),
        "UpdatedAt" = NOW();
        
    RAISE NOTICE 'Successfully added/updated commissioned devices';
END $$;
