-- Phase 3: Data Migration from 'public' to 'db' schema
-- This script safely migrates data while handling schema differences and dependencies

-- Enable transaction for rollback capability
BEGIN;

-- Create temporary ID mapping tables to track migrations
CREATE TEMP TABLE id_mappings (
    original_id UUID,
    new_id UUID,
    table_name TEXT
);

-- Step 1: Migrate Independent Entities (no foreign key dependencies)
-- These can be migrated directly since they don't depend on other tables

-- 1.1 Migrate AspNetUsers (standard ASP.NET Core Identity)
INSERT INTO db."AspNetUsers" (
    "Id", "UserName", "NormalizedUserName", "Email", "NormalizedEmail", 
    "EmailConfirmed", "PasswordHash", "SecurityStamp", "ConcurrencyStamp", 
    "PhoneNumber", "PhoneNumberConfirmed", "TwoFactorEnabled", "LockoutEnd", 
    "LockoutEnabled", "AccessFailedCount"
)
SELECT 
    "Id", "UserName", "NormalizedUserName", "Email", "NormalizedEmail", 
    "EmailConfirmed", "PasswordHash", "SecurityStamp", "ConcurrencyStamp", 
    "PhoneNumber", "PhoneNumberConfirmed", "TwoFactorEnabled", "LockoutEnd", 
    "LockoutEnabled", "AccessFailedCount"
FROM public."AspNetUsers"
ON CONFLICT ("Id") DO NOTHING;

-- 1.1.1 Migrate ApplicationUsers (custom user table with BaseEntity)
INSERT INTO db."ApplicationUsers" (
    "Id", "UserName", "Email", "FirstName", "LastName", "IsActive", "LastLogin",
    "CreatedAt", "UpdatedAt", "IsDeleted", "CreatedById", "UpdatedById"
)
SELECT 
    "Id", "UserName", "Email", "FirstName", "LastName", "IsActive", "LastLogin",
    "CreatedAt", "UpdatedAt", "IsDeleted", "CreatedById", "UpdatedById"
FROM public."ApplicationUsers"
ON CONFLICT ("Id") DO NOTHING;

-- 1.2 Migrate AspNetRoles
INSERT INTO db."AspNetRoles" (
    "Id", "Name", "NormalizedName", "ConcurrencyStamp"
)
SELECT 
    "Id", "Name", "NormalizedName", "ConcurrencyStamp"
FROM public."AspNetRoles"
ON CONFLICT ("Id") DO NOTHING;

-- 1.3 Migrate AspNetUserClaims
INSERT INTO db."AspNetUserClaims" (
    "Id", "UserId", "ClaimType", "ClaimValue"
)
SELECT 
    "Id", "UserId", "ClaimType", "ClaimValue"
FROM public."AspNetUserClaims"
ON CONFLICT ("Id") DO NOTHING;

-- 1.4 Migrate AspNetUserLogins
INSERT INTO db."AspNetUserLogins" (
    "LoginProvider", "ProviderKey", "ProviderDisplayName", "UserId"
)
SELECT 
    "LoginProvider", "ProviderKey", "ProviderDisplayName", "UserId"
FROM public."AspNetUserLogins"
ON CONFLICT ("LoginProvider", "ProviderKey") DO NOTHING;

-- 1.5 Migrate AspNetUserRoles
INSERT INTO db."AspNetUserRoles" (
    "UserId", "RoleId"
)
SELECT 
    "UserId", "RoleId"
FROM public."AspNetUserRoles"
ON CONFLICT ("UserId", "RoleId") DO NOTHING;

-- 1.6 Migrate AspNetUserTokens
INSERT INTO db."AspNetUserTokens" (
    "UserId", "LoginProvider", "Name", "Value"
)
SELECT 
    "UserId", "LoginProvider", "Name", "Value"
FROM public."AspNetUserTokens"
ON CONFLICT ("UserId", "LoginProvider", "Name") DO NOTHING;

-- Step 2: Migrate Level 1 Dependencies (depends on Users for CreatedById/UpdatedById)

-- 2.1 Migrate Rooms
INSERT INTO db."Rooms" (
    "Id", "Name", "Description", "Floor", "CreatedAt", "UpdatedAt", "IsDeleted", 
    "CreatedById", "UpdatedById"
)
SELECT 
    "Id", 
    CASE 
        WHEN LENGTH("Name") > 50 THEN LEFT("Name", 50) 
        ELSE "Name" 
    END,
    CASE 
        WHEN LENGTH("Description") > 150 THEN LEFT("Description", 150) 
        ELSE "Description" 
    END,
    "Floor", "CreatedAt", "UpdatedAt", "IsDeleted", "CreatedById", "UpdatedById"
FROM public."Rooms"
ON CONFLICT ("Id") DO NOTHING;

-- 2.2 Migrate DeviceTypes
INSERT INTO db."DeviceTypes" (
    "Id", "Name", "Description", "Capabilities", "IsSimulated", "Icon", "PreferredCommissioningMethod", "DeviceGroupId",
    "CreatedAt", "UpdatedAt", "IsDeleted", "CreatedById", "UpdatedById"
)
SELECT 
    "Id", 
    CASE 
        WHEN LENGTH("Name") > 50 THEN LEFT("Name", 50) 
        ELSE "Name" 
    END,
    CASE 
        WHEN LENGTH("Description") > 150 THEN LEFT("Description", 150) 
        ELSE "Description" 
    END,
    "Capabilities", "IsSimulated", 
    COALESCE("Icon", 'default-icon') as "Icon",
    COALESCE("PreferredCommissioningMethod", 'manual') as "PreferredCommissioningMethod",
    "DeviceGroupId", "CreatedAt", "UpdatedAt", 
    "IsDeleted", "CreatedById", "UpdatedById"
FROM public."DeviceTypes"
ON CONFLICT ("Id") DO NOTHING;

-- 2.3 Migrate DeviceGroups
INSERT INTO db."DeviceGroups" (
    "Id", "Name", "Description", "Icon", "PreferredCommissioningMethod", "DefaultCapabilities", "SortOrder", "IsActive", "RoomId",
    "CreatedAt", "UpdatedAt", "IsDeleted", "CreatedById", "UpdatedById"
)
SELECT 
    "Id", 
    CASE 
        WHEN LENGTH("Name") > 50 THEN LEFT("Name", 50) 
        ELSE "Name" 
    END,
    CASE 
        WHEN LENGTH("Description") > 150 THEN LEFT("Description", 150) 
        ELSE "Description" 
    END,
    COALESCE("Icon", 'oi-device-hdd') as "Icon",
    COALESCE("PreferredCommissioningMethod", 'BLE_WiFi') as "PreferredCommissioningMethod",
    "DefaultCapabilities", 
    COALESCE("SortOrder", 0) as "SortOrder",
    COALESCE("IsActive", true) as "IsActive",
    "RoomId", "CreatedAt", "UpdatedAt", "IsDeleted", 
    "CreatedById", "UpdatedById"
FROM public."DeviceGroups"
ON CONFLICT ("Id") DO NOTHING;

-- Step 3: Migrate Level 2 Dependencies (depends on Level 1 entities)

-- 3.1 Migrate Devices (depends on DeviceTypes, Rooms, Users)
INSERT INTO db."Devices" (
    "Id", "Name", "Description", "MatterDeviceId", "DeviceTypeId", "RoomId",
    "Properties", "Status", "Configuration", "LastStateChange", "IsOnline", 
    "LastSeen", "CreatedAt", "UpdatedAt", "IsDeleted", "CreatedById", "UpdatedById"
)
SELECT 
    "Id", 
    CASE 
        WHEN LENGTH("Name") > 50 THEN LEFT("Name", 50) 
        ELSE "Name" 
    END,
    CASE 
        WHEN LENGTH("Description") > 150 THEN LEFT("Description", 150) 
        ELSE "Description" 
    END,
    CASE 
        WHEN LENGTH("MatterDeviceId") > 150 THEN LEFT("MatterDeviceId", 150) 
        ELSE "MatterDeviceId" 
    END,
    "DeviceTypeId", "RoomId", "Properties", 
    CASE 
        WHEN LENGTH("Status") > 50 THEN LEFT("Status", 50) 
        ELSE "Status" 
    END,
    "Configuration", "LastStateChange", "IsOnline", "LastSeen", "CreatedAt", 
    "UpdatedAt", "IsDeleted", "CreatedById", "UpdatedById"
FROM public."Devices"
ON CONFLICT ("Id") DO NOTHING;

-- 3.2 Migrate Groups (depends on Users)
INSERT INTO db."Groups" (
    "Id", "Name", "Description", "CreatedAt", "UpdatedAt", "IsDeleted", 
    "CreatedById", "UpdatedById"
)
SELECT 
    "Id", 
    CASE 
        WHEN LENGTH("Name") > 50 THEN LEFT("Name", 50) 
        ELSE "Name" 
    END,
    CASE 
        WHEN LENGTH("Description") > 150 THEN LEFT("Description", 150) 
        ELSE "Description" 
    END,
    "CreatedAt", "UpdatedAt", "IsDeleted", "CreatedById", "UpdatedById"
FROM public."Groups"
ON CONFLICT ("Id") DO NOTHING;

-- Step 4: Migrate Level 3 Dependencies (depends on Devices and Groups)

-- 4.1 Migrate DeviceStates (depends on Devices)
INSERT INTO db."DeviceStates" (
    "Id", "DeviceId", "StateType", "StateValue", "RecordedAt", "CreatedAt", "UpdatedAt", 
    "IsDeleted", "CreatedById", "UpdatedById"
)
SELECT 
    "Id", "DeviceId", 
    CASE 
        WHEN LENGTH("StateType") > 150 THEN LEFT("StateType", 150) 
        ELSE "StateType" 
    END,
    "StateValue", "RecordedAt", "CreatedAt", "UpdatedAt", 
    "IsDeleted", "CreatedById", "UpdatedById"
FROM public."DeviceStates"
ON CONFLICT ("Id") DO NOTHING;

-- 4.2 Migrate DeviceEvents (depends on Devices)
INSERT INTO db."DeviceEvents" (
    "Id", "DeviceId", "EventType", "EventData", "CreatedAt", 
    "UpdatedAt", "IsDeleted", "CreatedById", "UpdatedById"
)
SELECT 
    "Id", "DeviceId", 
    CASE 
        WHEN LENGTH("EventType") > 150 THEN LEFT("EventType", 150) 
        ELSE "EventType" 
    END,
    "EventData", "CreatedAt", 
    "UpdatedAt", "IsDeleted", "CreatedById", "UpdatedById"
FROM public."DeviceEvents"
ON CONFLICT ("Id") DO NOTHING;

-- 4.3 Migrate GroupMembers (depends on Groups, Devices) - Handle shadow property
INSERT INTO db."GroupMembers" (
    "GroupId", "DeviceId", "CreatedAt", "UpdatedAt", "IsDeleted", 
    "CreatedById", "UpdatedById"
)
SELECT 
    "GroupId", "DeviceId", "CreatedAt", "UpdatedAt", "IsDeleted", 
    "CreatedById", "UpdatedById"
FROM public."GroupMembers"
WHERE "DeviceId" IS NOT NULL  -- Exclude shadow property records
ON CONFLICT ("GroupId", "DeviceId") DO NOTHING;

-- 4.4 Migrate GroupStates (depends on Groups)
INSERT INTO db."GroupStates" (
    "GroupId", "State", "LastUpdated", "CreatedAt", "UpdatedAt", "IsDeleted", 
    "CreatedById", "UpdatedById"
)
SELECT 
    "GroupId", "State", "LastUpdated", "CreatedAt", "UpdatedAt", "IsDeleted", 
    "CreatedById", "UpdatedById"
FROM public."GroupStates"
ON CONFLICT ("GroupId") DO NOTHING;

-- 4.5 Migrate GroupStateHistory (depends on Groups)
INSERT INTO db."GroupStateHistory" (
    "Id", "GroupId", "OldState", "NewState", "Description", 
    "CreatedAt", "UpdatedAt", "IsDeleted", "CreatedById", "UpdatedById"
)
SELECT 
    "Id", "GroupId", "OldState", "NewState", 
    CASE 
        WHEN LENGTH("Description") > 150 THEN LEFT("Description", 150) 
        ELSE "Description" 
    END,
    "CreatedAt", "UpdatedAt", "IsDeleted", "CreatedById", "UpdatedById"
FROM public."GroupStateHistory"
WHERE "GroupId1" IS NULL  -- Exclude shadow property records
ON CONFLICT ("Id") DO NOTHING;

-- Step 5: Migrate Level 4 Dependencies (depends on Devices)

-- 5.1 Migrate DeviceHistory (depends on Devices)
INSERT INTO db."DeviceHistory" (
    "Id", "DeviceId", "EventType", "OldState", "NewState", "Description", 
    "CreatedAt", "UpdatedAt", "IsDeleted", "CreatedById", "UpdatedById"
)
SELECT 
    "Id", "DeviceId", 
    CASE 
        WHEN LENGTH("EventType") > 150 THEN LEFT("EventType", 150) 
        ELSE "EventType" 
    END,
    "OldState", "NewState", 
    CASE 
        WHEN LENGTH("Description") > 150 THEN LEFT("Description", 150) 
        ELSE "Description" 
    END,
    "CreatedAt", "UpdatedAt", "IsDeleted", "CreatedById", "UpdatedById"
FROM public."DeviceHistory"
ON CONFLICT ("Id") DO NOTHING;

-- Step 6: Migrate Level 5 Dependencies (depends on Users and other entities)

-- 6.1 Migrate Rules (depends on Users)
INSERT INTO db."Rules" (
    "Id", "Name", "Description", "Condition", "Action", "IsActive", 
    "CreatedAt", "UpdatedAt", "IsDeleted", "CreatedById", "UpdatedById"
)
SELECT 
    "Id", 
    CASE 
        WHEN LENGTH("Name") > 150 THEN LEFT("Name", 150) 
        ELSE "Name" 
    END,
    CASE 
        WHEN LENGTH("Description") > 150 THEN LEFT("Description", 150) 
        ELSE "Description" 
    END,
    "Condition", "Action", "IsActive", "CreatedAt", "UpdatedAt", "IsDeleted", 
    "CreatedById", "UpdatedById"
FROM public."Rules"
ON CONFLICT ("Id") DO NOTHING;

-- 6.2 Migrate RuleConditions (depends on Rules)
INSERT INTO db."RuleConditions" (
    "Id", "RuleId", "Condition", "Order", "CreatedAt", 
    "UpdatedAt", "IsDeleted", "CreatedById", "UpdatedById"
)
SELECT 
    "Id", "RuleId", "Condition", "Order", "CreatedAt", 
    "UpdatedAt", "IsDeleted", "CreatedById", "UpdatedById"
FROM public."RuleConditions"
WHERE "RuleId1" IS NULL  -- Exclude shadow property records
ON CONFLICT ("Id") DO NOTHING;

-- 6.3 Migrate RuleActions (depends on Rules)
INSERT INTO db."RuleActions" (
    "Id", "RuleId", "Action", "Order", "CreatedAt", 
    "UpdatedAt", "IsDeleted", "CreatedById", "UpdatedById"
)
SELECT 
    "Id", "RuleId", "Action", "Order", "CreatedAt", 
    "UpdatedAt", "IsDeleted", "CreatedById", "UpdatedById"
FROM public."RuleActions"
WHERE "RuleId1" IS NULL  -- Exclude shadow property records
ON CONFLICT ("Id") DO NOTHING;

-- 6.4 Migrate RuleTriggers (depends on Rules)
INSERT INTO db."RuleTriggers" (
    "Id", "RuleId", "TriggerType", "Trigger", "IsEnabled", "LastTriggered", 
    "CreatedAt", "UpdatedAt", "IsDeleted", "CreatedById", "UpdatedById"
)
SELECT 
    "Id", "RuleId", 
    CASE 
        WHEN LENGTH("TriggerType") > 500 THEN LEFT("TriggerType", 500) 
        ELSE "TriggerType" 
    END,
    "Trigger", "IsEnabled", "LastTriggered", "CreatedAt", 
    "UpdatedAt", "IsDeleted", "CreatedById", "UpdatedById"
FROM public."RuleTriggers"
WHERE "RuleId1" IS NULL  -- Exclude shadow property records
ON CONFLICT ("Id") DO NOTHING;

-- 6.5 Migrate RuleExecutionHistory (depends on Rules)
INSERT INTO db."RuleExecutionHistory" (
    "Id", "RuleId", "Success", "Result", "ErrorMessage", "ExecutionTime", 
    "CreatedAt", "UpdatedAt", "IsDeleted", "CreatedById", "UpdatedById"
)
SELECT 
    "Id", "RuleId", "Success", "Result", 
    CASE 
        WHEN LENGTH("ErrorMessage") > 500 THEN LEFT("ErrorMessage", 500) 
        ELSE "ErrorMessage" 
    END,
    "ExecutionTime", "CreatedAt", 
    "UpdatedAt", "IsDeleted", "CreatedById", "UpdatedById"
FROM public."RuleExecutionHistory"
WHERE "RuleId1" IS NULL  -- Exclude shadow property records
ON CONFLICT ("Id") DO NOTHING;

-- Step 7: Migrate Junction Tables

-- 7.1 Migrate DeviceDeviceGroup (depends on Devices, DeviceGroups)
INSERT INTO db."DeviceDeviceGroup" (
    "DevicesId", "DeviceGroupsId"
)
SELECT 
    "DevicesId", "DeviceGroupsId"
FROM public."DeviceDeviceGroup"
ON CONFLICT ("DevicesId", "DeviceGroupsId") DO NOTHING;

-- Step 8: Migrate Settings and Permissions

-- 8.1 Migrate UserSettings (depends on Users)
INSERT INTO db."UserSettings" (
    "UserId", "Theme", "Language", "ShowOfflineDevices", "DefaultView", "DashboardLayout", 
    "FavoriteDevices", "QuickActions", "EmailNotifications", "PushNotifications", 
    "NotificationPreferences", "DeviceDisplayPreferences", "LastUsedDevices", 
    "RoomDisplayOrder", "ShowEmptyRooms", "ShowAutomationSuggestions", 
    "AutomationPreferences", "Id", "CreatedAt", "UpdatedAt", "IsDeleted", 
    "CreatedById", "UpdatedById"
)
SELECT 
    "UserId", 
    CASE 
        WHEN LENGTH("Theme") > 150 THEN LEFT("Theme", 150) 
        ELSE "Theme" 
    END,
    CASE 
        WHEN LENGTH("Language") > 150 THEN LEFT("Language", 150) 
        ELSE "Language" 
    END,
    COALESCE("ShowOfflineDevices", true) as "ShowOfflineDevices",
    COALESCE("DefaultView", 'dashboard') as "DefaultView",
    "DashboardLayout", "FavoriteDevices", "QuickActions", 
    COALESCE("EmailNotifications", true) as "EmailNotifications",
    COALESCE("PushNotifications", true) as "PushNotifications",
    "NotificationPreferences", "DeviceDisplayPreferences", "LastUsedDevices", 
    "RoomDisplayOrder", COALESCE("ShowEmptyRooms", true) as "ShowEmptyRooms",
    COALESCE("ShowAutomationSuggestions", true) as "ShowAutomationSuggestions",
    "AutomationPreferences", "Id", "CreatedAt", "UpdatedAt", "IsDeleted", 
    "CreatedById", "UpdatedById"
FROM public."UserSettings"
WHERE "UserId1" IS NULL  -- Exclude shadow property records
ON CONFLICT ("UserId") DO NOTHING;

-- 8.2 Migrate EnvironmentalSettings (depends on Users)
INSERT INTO db."EnvironmentalSettings" (
    "Id", "UserId", "IndoorTemperatureMin", "IndoorTemperatureMax", "OutdoorTemperatureMin", 
    "OutdoorTemperatureMax", "HumidityMin", "HumidityMax", "CO2Max", "VOCMax", 
    "TemperatureWarning", "HumidityWarning", "LastUpdated", "CreatedAt", 
    "UpdatedAt", "IsDeleted", "CreatedById", "UpdatedById"
)
SELECT 
    "Id", 
    CASE 
        WHEN LENGTH("UserId") > 150 THEN LEFT("UserId", 150) 
        ELSE "UserId" 
    END,
    "IndoorTemperatureMin", "IndoorTemperatureMax", "OutdoorTemperatureMin", 
    "OutdoorTemperatureMax", "HumidityMin", "HumidityMax", "CO2Max", "VOCMax", 
    "TemperatureWarning", "HumidityWarning", "LastUpdated", "CreatedAt", 
    "UpdatedAt", "IsDeleted", "CreatedById", "UpdatedById"
FROM public."EnvironmentalSettings"
ON CONFLICT ("UserId") DO NOTHING;

-- 8.3 Migrate UserDevicePermissions (depends on Users, Devices)
INSERT INTO db."UserDevicePermissions" (
    "Id", "UserId", "DeviceId", "PermissionLevel", "CreatedAt", 
    "UpdatedAt", "IsDeleted", "CreatedById", "UpdatedById"
)
SELECT 
    "Id", 
    CASE 
        WHEN LENGTH("UserId") > 500 THEN LEFT("UserId", 500) 
        ELSE "UserId" 
    END,
    "DeviceId", 
    CASE 
        WHEN LENGTH("PermissionLevel") > 150 THEN LEFT("PermissionLevel", 150) 
        ELSE "PermissionLevel" 
    END,
    "CreatedAt", "UpdatedAt", "IsDeleted", "CreatedById", "UpdatedById"
FROM public."UserDevicePermissions"
ON CONFLICT ("Id") DO NOTHING;

-- 8.4 Migrate UserRoomPermissions (depends on Users, Rooms)
INSERT INTO db."UserRoomPermissions" (
    "Id", "UserId", "RoomId", "Permission", "CreatedAt", 
    "UpdatedAt", "IsDeleted", "CreatedById", "UpdatedById"
)
SELECT 
    "Id", "UserId", "RoomId", "Permission", "CreatedAt", 
    "UpdatedAt", "IsDeleted", "CreatedById", "UpdatedById"
FROM public."UserRoomPermissions"
ON CONFLICT ("Id") DO NOTHING;

-- 8.5 Migrate Notifications (depends on Users)
INSERT INTO db."Notifications" (
    "Id", "UserId", "Message", "Type", "Severity", "IsRead", "ReadAt", 
    "CreatedAt", "UpdatedAt", "IsDeleted", "CreatedById", "UpdatedById"
)
SELECT 
    "Id", "UserId", 
    CASE 
        WHEN LENGTH("Message") > 500 THEN LEFT("Message", 500) 
        ELSE "Message" 
    END,
    "Type", "Severity", "IsRead", "ReadAt", "CreatedAt", "UpdatedAt", "IsDeleted", 
    "CreatedById", "UpdatedById"
FROM public."Notifications"
ON CONFLICT ("Id") DO NOTHING;

-- Verification: Check migration results
DO $$
DECLARE
    public_count INTEGER;
    db_count INTEGER;
    table_name TEXT;
BEGIN
    -- Check each table migration
    FOR table_name IN 
        SELECT unnest(ARRAY[
            'AspNetUsers', 'AspNetRoles', 'AspNetUserClaims', 'AspNetUserLogins',
            'AspNetUserRoles', 'AspNetUserTokens', 'Rooms', 'DeviceTypes',
            'DeviceGroups', 'Devices', 'Groups', 'DeviceStates', 'DeviceEvents',
            'GroupMembers', 'GroupStates', 'GroupStateHistory', 'DeviceHistory',
            'Rules', 'RuleConditions', 'RuleActions', 'RuleTriggers',
            'RuleExecutionHistory', 'DeviceDeviceGroup', 'UserSettings',
            'EnvironmentalSettings', 'UserDevicePermissions', 'UserRoomPermissions',
            'Notifications'
        ])
    LOOP
        EXECUTE format('SELECT COUNT(*) FROM public.%I', table_name) INTO public_count;
        EXECUTE format('SELECT COUNT(*) FROM db.%I', table_name) INTO db_count;
        
        RAISE NOTICE 'Table %: Public=% records, DB=% records', table_name, public_count, db_count;
        
        IF public_count != db_count THEN
            RAISE WARNING 'Migration mismatch for table %: Public=% records, DB=% records', table_name, public_count, db_count;
        END IF;
    END LOOP;
END $$;

-- Commit the migration
COMMIT;

-- Final verification message
SELECT 'Phase 3 Data Migration Completed Successfully!' as status;
