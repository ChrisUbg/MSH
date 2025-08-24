-- Add new tables for firmware updates and clusters
-- This script adds the tables manually to avoid EF Core migration issues

-- 1. Create Clusters table
CREATE TABLE "Clusters" (
    "Id" uuid NOT NULL,
    "ClusterId" integer NOT NULL,
    "Name" character varying(100) NOT NULL,
    "Description" character varying(500),
    "Category" character varying(50),
    "IsRequired" boolean NOT NULL DEFAULT false,
    "IsOptional" boolean NOT NULL DEFAULT true,
    "MatterVersion" character varying(20),
    "Attributes" jsonb,
    "Commands" jsonb,
    "Events" jsonb,
    "CreatedAt" timestamp with time zone NOT NULL,
    "UpdatedAt" timestamp with time zone,
    "IsDeleted" boolean NOT NULL DEFAULT false,
    "CreatedById" character varying(500) NOT NULL,
    "UpdatedById" character varying(500),
    CONSTRAINT "PK_Clusters" PRIMARY KEY ("Id")
);

-- 2. Create DevicePropertyChanges table
CREATE TABLE "DevicePropertyChanges" (
    "Id" uuid NOT NULL,
    "DeviceId" uuid NOT NULL,
    "PropertyName" character varying(100) NOT NULL,
    "OldValue" jsonb,
    "NewValue" jsonb,
    "ChangeType" character varying(50) NOT NULL DEFAULT 'update',
    "Reason" character varying(200),
    "ChangeTimestamp" timestamp with time zone NOT NULL,
    "IsConfirmed" boolean NOT NULL DEFAULT false,
    "ConfirmedAt" timestamp with time zone,
    "ConfirmedBy" character varying(50),
    "CreatedAt" timestamp with time zone NOT NULL,
    "UpdatedAt" timestamp with time zone,
    "IsDeleted" boolean NOT NULL DEFAULT false,
    "CreatedById" character varying(500) NOT NULL,
    "UpdatedById" character varying(500),
    CONSTRAINT "PK_DevicePropertyChanges" PRIMARY KEY ("Id")
);

-- 3. Create FirmwareUpdates table
CREATE TABLE "FirmwareUpdates" (
    "Id" uuid NOT NULL,
    "Name" character varying(100) NOT NULL,
    "Description" character varying(500),
    "CurrentVersion" character varying(50) NOT NULL,
    "TargetVersion" character varying(50) NOT NULL,
    "DownloadUrl" character varying(200),
    "FileName" character varying(100),
    "FileSize" bigint,
    "Checksum" character varying(100),
    "Status" character varying(20) NOT NULL DEFAULT 'available',
    "DownloadStartedAt" timestamp with time zone,
    "DownloadCompletedAt" timestamp with time zone,
    "InstallationStartedAt" timestamp with time zone,
    "InstallationCompletedAt" timestamp with time zone,
    "ErrorMessage" character varying(500),
    "UpdateMetadata" jsonb,
    "IsCompatible" boolean NOT NULL DEFAULT true,
    "RequiresConfirmation" boolean NOT NULL DEFAULT false,
    "IsConfirmed" boolean NOT NULL DEFAULT false,
    "ConfirmedAt" timestamp with time zone,
    "ConfirmedBy" character varying(50),
    "CreatedAt" timestamp with time zone NOT NULL,
    "UpdatedAt" timestamp with time zone,
    "IsDeleted" boolean NOT NULL DEFAULT false,
    "CreatedById" character varying(500) NOT NULL,
    "UpdatedById" character varying(500),
    CONSTRAINT "PK_FirmwareUpdates" PRIMARY KEY ("Id")
);

-- 4. Create DeviceFirmwareUpdates table
CREATE TABLE "DeviceFirmwareUpdates" (
    "Id" uuid NOT NULL,
    "DeviceId" uuid NOT NULL,
    "FirmwareUpdateId" uuid NOT NULL,
    "CurrentVersion" character varying(50) NOT NULL,
    "TargetVersion" character varying(50) NOT NULL,
    "Status" character varying(20) NOT NULL DEFAULT 'pending',
    "DownloadStartedAt" timestamp with time zone,
    "DownloadCompletedAt" timestamp with time zone,
    "InstallationStartedAt" timestamp with time zone,
    "InstallationCompletedAt" timestamp with time zone,
    "TestCompletedAt" timestamp with time zone,
    "TestPassed" boolean NOT NULL DEFAULT false,
    "ErrorMessage" character varying(500),
    "TestResults" jsonb,
    "IsConfirmed" boolean NOT NULL DEFAULT false,
    "ConfirmedAt" timestamp with time zone,
    "ConfirmedBy" character varying(50),
    "IsRollbackAvailable" boolean NOT NULL DEFAULT false,
    "RollbackCompletedAt" timestamp with time zone,
    "RollbackReason" character varying(500),
    "UpdateLog" jsonb,
    "CreatedAt" timestamp with time zone NOT NULL,
    "UpdatedAt" timestamp with time zone,
    "IsDeleted" boolean NOT NULL DEFAULT false,
    "CreatedById" character varying(500) NOT NULL,
    "UpdatedById" character varying(500),
    CONSTRAINT "PK_DeviceFirmwareUpdates" PRIMARY KEY ("Id")
);

-- 5. Add foreign key constraints
ALTER TABLE "Clusters" ADD CONSTRAINT "FK_Clusters_ApplicationUsers_CreatedById" 
    FOREIGN KEY ("CreatedById") REFERENCES "ApplicationUsers" ("Id") ON DELETE RESTRICT;

ALTER TABLE "Clusters" ADD CONSTRAINT "FK_Clusters_ApplicationUsers_UpdatedById" 
    FOREIGN KEY ("UpdatedById") REFERENCES "ApplicationUsers" ("Id") ON DELETE RESTRICT;

ALTER TABLE "DevicePropertyChanges" ADD CONSTRAINT "FK_DevicePropertyChanges_ApplicationUsers_CreatedById" 
    FOREIGN KEY ("CreatedById") REFERENCES "ApplicationUsers" ("Id") ON DELETE RESTRICT;

ALTER TABLE "DevicePropertyChanges" ADD CONSTRAINT "FK_DevicePropertyChanges_ApplicationUsers_UpdatedById" 
    FOREIGN KEY ("UpdatedById") REFERENCES "ApplicationUsers" ("Id") ON DELETE RESTRICT;

ALTER TABLE "DevicePropertyChanges" ADD CONSTRAINT "FK_DevicePropertyChanges_Devices_DeviceId" 
    FOREIGN KEY ("DeviceId") REFERENCES "Devices" ("Id") ON DELETE CASCADE;

ALTER TABLE "FirmwareUpdates" ADD CONSTRAINT "FK_FirmwareUpdates_ApplicationUsers_CreatedById" 
    FOREIGN KEY ("CreatedById") REFERENCES "ApplicationUsers" ("Id") ON DELETE RESTRICT;

ALTER TABLE "FirmwareUpdates" ADD CONSTRAINT "FK_FirmwareUpdates_ApplicationUsers_UpdatedById" 
    FOREIGN KEY ("UpdatedById") REFERENCES "ApplicationUsers" ("Id") ON DELETE RESTRICT;

ALTER TABLE "DeviceFirmwareUpdates" ADD CONSTRAINT "FK_DeviceFirmwareUpdates_ApplicationUsers_CreatedById" 
    FOREIGN KEY ("CreatedById") REFERENCES "ApplicationUsers" ("Id") ON DELETE RESTRICT;

ALTER TABLE "DeviceFirmwareUpdates" ADD CONSTRAINT "FK_DeviceFirmwareUpdates_ApplicationUsers_UpdatedById" 
    FOREIGN KEY ("UpdatedById") REFERENCES "ApplicationUsers" ("Id") ON DELETE RESTRICT;

ALTER TABLE "DeviceFirmwareUpdates" ADD CONSTRAINT "FK_DeviceFirmwareUpdates_Devices_DeviceId" 
    FOREIGN KEY ("DeviceId") REFERENCES "Devices" ("Id") ON DELETE CASCADE;

ALTER TABLE "DeviceFirmwareUpdates" ADD CONSTRAINT "FK_DeviceFirmwareUpdates_FirmwareUpdates_FirmwareUpdateId" 
    FOREIGN KEY ("FirmwareUpdateId") REFERENCES "FirmwareUpdates" ("Id") ON DELETE CASCADE;

-- 6. Add indexes
CREATE UNIQUE INDEX "IX_Clusters_ClusterId" ON "Clusters" ("ClusterId");
CREATE INDEX "IX_Clusters_CreatedById" ON "Clusters" ("CreatedById");
CREATE INDEX "IX_Clusters_UpdatedById" ON "Clusters" ("UpdatedById");

CREATE INDEX "IX_DevicePropertyChanges_DeviceId" ON "DevicePropertyChanges" ("DeviceId");
CREATE INDEX "IX_DevicePropertyChanges_CreatedById" ON "DevicePropertyChanges" ("CreatedById");
CREATE INDEX "IX_DevicePropertyChanges_UpdatedById" ON "DevicePropertyChanges" ("UpdatedById");

CREATE INDEX "IX_FirmwareUpdates_CreatedById" ON "FirmwareUpdates" ("CreatedById");
CREATE INDEX "IX_FirmwareUpdates_UpdatedById" ON "FirmwareUpdates" ("UpdatedById");

CREATE INDEX "IX_DeviceFirmwareUpdates_DeviceId" ON "DeviceFirmwareUpdates" ("DeviceId");
CREATE INDEX "IX_DeviceFirmwareUpdates_FirmwareUpdateId" ON "DeviceFirmwareUpdates" ("FirmwareUpdateId");
CREATE INDEX "IX_DeviceFirmwareUpdates_CreatedById" ON "DeviceFirmwareUpdates" ("CreatedById");
CREATE INDEX "IX_DeviceFirmwareUpdates_UpdatedById" ON "DeviceFirmwareUpdates" ("UpdatedById");

-- 7. Add some initial cluster data
INSERT INTO "Clusters" ("Id", "ClusterId", "Name", "Description", "Category", "IsRequired", "IsOptional", "MatterVersion", "CreatedAt", "IsDeleted", "CreatedById") VALUES
(gen_random_uuid(), 3, 'Identify', 'Identify cluster for device identification', 'Basic', true, false, '1.0', NOW(), false, (SELECT "Id" FROM "ApplicationUsers" LIMIT 1)),
(gen_random_uuid(), 4, 'Groups', 'Groups cluster for device grouping', 'Basic', true, false, '1.0', NOW(), false, (SELECT "Id" FROM "ApplicationUsers" LIMIT 1)),
(gen_random_uuid(), 6, 'OnOff', 'OnOff cluster for device power control', 'Control', true, false, '1.0', NOW(), false, (SELECT "Id" FROM "ApplicationUsers" LIMIT 1)),
(gen_random_uuid(), 29, 'Descriptor', 'Descriptor cluster for device information', 'Basic', true, false, '1.0', NOW(), false, (SELECT "Id" FROM "ApplicationUsers" LIMIT 1)),
(gen_random_uuid(), 2820, 'Electrical Measurement', 'Electrical measurement cluster for energy monitoring', 'Measurement', false, true, '1.3', NOW(), false, (SELECT "Id" FROM "ApplicationUsers" LIMIT 1));

-- 8. Update existing device properties to include cluster information
UPDATE "Devices" 
SET "Properties" = "Properties" || '{"clusters": [3, 4, 6, 29], "device_type": 266, "device_revision": 2, "cluster_revision": 1}'::jsonb
WHERE "MatterDeviceId" IS NOT NULL;
