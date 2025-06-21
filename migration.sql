CREATE TABLE IF NOT EXISTS "__EFMigrationsHistory" (
    "MigrationId" character varying(150) NOT NULL,
    "ProductVersion" character varying(32) NOT NULL,
    CONSTRAINT "PK___EFMigrationsHistory" PRIMARY KEY ("MigrationId")
);

START TRANSACTION;

CREATE TABLE "ApplicationUsers" (
    "Id" text NOT NULL,
    "UserName" text NOT NULL,
    "Email" text,
    "FirstName" text,
    "LastName" text,
    "IsActive" boolean NOT NULL,
    "LastLogin" timestamp with time zone NOT NULL,
    "CreatedAt" timestamp with time zone NOT NULL,
    "UpdatedAt" timestamp with time zone,
    "IsDeleted" boolean NOT NULL,
    "CreatedById" text NOT NULL,
    "UpdatedById" text,
    CONSTRAINT "PK_ApplicationUsers" PRIMARY KEY ("Id"),
    CONSTRAINT "FK_ApplicationUsers_ApplicationUsers_CreatedById" FOREIGN KEY ("CreatedById") REFERENCES "ApplicationUsers" ("Id") ON DELETE RESTRICT,
    CONSTRAINT "FK_ApplicationUsers_ApplicationUsers_UpdatedById" FOREIGN KEY ("UpdatedById") REFERENCES "ApplicationUsers" ("Id") ON DELETE RESTRICT
);

CREATE TABLE "AspNetRoles" (
    "Id" text NOT NULL,
    "Name" character varying(256),
    "NormalizedName" character varying(256),
    "ConcurrencyStamp" text,
    CONSTRAINT "PK_AspNetRoles" PRIMARY KEY ("Id")
);

CREATE TABLE "AspNetUsers" (
    "Id" text NOT NULL,
    "UserName" character varying(256),
    "NormalizedUserName" character varying(256),
    "Email" character varying(256),
    "NormalizedEmail" character varying(256),
    "EmailConfirmed" boolean NOT NULL,
    "PasswordHash" text,
    "SecurityStamp" text,
    "ConcurrencyStamp" text,
    "PhoneNumber" text,
    "PhoneNumberConfirmed" boolean NOT NULL,
    "TwoFactorEnabled" boolean NOT NULL,
    "LockoutEnd" timestamp with time zone,
    "LockoutEnabled" boolean NOT NULL,
    "AccessFailedCount" integer NOT NULL,
    CONSTRAINT "PK_AspNetUsers" PRIMARY KEY ("Id")
);

CREATE TABLE "DeviceTypes" (
    "Id" uuid NOT NULL,
    "Name" text NOT NULL,
    "Description" text,
    "Capabilities" jsonb NOT NULL,
    "IsSimulated" boolean NOT NULL,
    "CreatedAt" timestamp with time zone NOT NULL,
    "UpdatedAt" timestamp with time zone,
    "IsDeleted" boolean NOT NULL,
    "CreatedById" text NOT NULL,
    "UpdatedById" text,
    CONSTRAINT "PK_DeviceTypes" PRIMARY KEY ("Id"),
    CONSTRAINT "FK_DeviceTypes_ApplicationUsers_CreatedById" FOREIGN KEY ("CreatedById") REFERENCES "ApplicationUsers" ("Id") ON DELETE RESTRICT,
    CONSTRAINT "FK_DeviceTypes_ApplicationUsers_UpdatedById" FOREIGN KEY ("UpdatedById") REFERENCES "ApplicationUsers" ("Id") ON DELETE RESTRICT
);

CREATE TABLE "EnvironmentalSettings" (
    "Id" uuid NOT NULL,
    "UserId" text NOT NULL,
    "IndoorTemperatureMin" double precision NOT NULL,
    "IndoorTemperatureMax" double precision NOT NULL,
    "OutdoorTemperatureMin" double precision NOT NULL,
    "OutdoorTemperatureMax" double precision NOT NULL,
    "HumidityMin" double precision NOT NULL,
    "HumidityMax" double precision NOT NULL,
    "CO2Max" double precision NOT NULL,
    "VOCMax" double precision NOT NULL,
    "TemperatureWarning" double precision NOT NULL,
    "HumidityWarning" double precision NOT NULL,
    "LastUpdated" timestamp with time zone NOT NULL,
    "CreatedAt" timestamp with time zone NOT NULL,
    "UpdatedAt" timestamp with time zone,
    "IsDeleted" boolean NOT NULL,
    "CreatedById" text NOT NULL,
    "UpdatedById" text,
    CONSTRAINT "PK_EnvironmentalSettings" PRIMARY KEY ("Id"),
    CONSTRAINT "FK_EnvironmentalSettings_ApplicationUsers_CreatedById" FOREIGN KEY ("CreatedById") REFERENCES "ApplicationUsers" ("Id") ON DELETE RESTRICT,
    CONSTRAINT "FK_EnvironmentalSettings_ApplicationUsers_UpdatedById" FOREIGN KEY ("UpdatedById") REFERENCES "ApplicationUsers" ("Id") ON DELETE RESTRICT,
    CONSTRAINT "FK_EnvironmentalSettings_ApplicationUsers_UserId" FOREIGN KEY ("UserId") REFERENCES "ApplicationUsers" ("Id") ON DELETE CASCADE
);

CREATE TABLE "Groups" (
    "Id" uuid NOT NULL,
    "Name" text NOT NULL,
    "Description" text,
    "CreatedAt" timestamp with time zone NOT NULL,
    "UpdatedAt" timestamp with time zone,
    "IsDeleted" boolean NOT NULL,
    "CreatedById" text NOT NULL,
    "UpdatedById" text,
    CONSTRAINT "PK_Groups" PRIMARY KEY ("Id"),
    CONSTRAINT "FK_Groups_ApplicationUsers_CreatedById" FOREIGN KEY ("CreatedById") REFERENCES "ApplicationUsers" ("Id") ON DELETE RESTRICT,
    CONSTRAINT "FK_Groups_ApplicationUsers_UpdatedById" FOREIGN KEY ("UpdatedById") REFERENCES "ApplicationUsers" ("Id") ON DELETE RESTRICT
);

CREATE TABLE "Notifications" (
    "Id" uuid NOT NULL,
    "UserId" text NOT NULL,
    "Message" text NOT NULL,
    "Type" integer NOT NULL,
    "Severity" integer,
    "IsRead" boolean NOT NULL,
    "ReadAt" timestamp with time zone,
    "CreatedAt" timestamp with time zone NOT NULL,
    "UpdatedAt" timestamp with time zone,
    "IsDeleted" boolean NOT NULL,
    "CreatedById" text NOT NULL,
    "UpdatedById" text,
    CONSTRAINT "PK_Notifications" PRIMARY KEY ("Id"),
    CONSTRAINT "FK_Notifications_ApplicationUsers_CreatedById" FOREIGN KEY ("CreatedById") REFERENCES "ApplicationUsers" ("Id") ON DELETE RESTRICT,
    CONSTRAINT "FK_Notifications_ApplicationUsers_UpdatedById" FOREIGN KEY ("UpdatedById") REFERENCES "ApplicationUsers" ("Id") ON DELETE RESTRICT
);

CREATE TABLE "Rooms" (
    "Id" uuid NOT NULL,
    "Name" text NOT NULL,
    "Description" text,
    "Floor" integer,
    "CreatedAt" timestamp with time zone NOT NULL,
    "UpdatedAt" timestamp with time zone,
    "IsDeleted" boolean NOT NULL,
    "CreatedById" text NOT NULL,
    "UpdatedById" text,
    CONSTRAINT "PK_Rooms" PRIMARY KEY ("Id"),
    CONSTRAINT "FK_Rooms_ApplicationUsers_CreatedById" FOREIGN KEY ("CreatedById") REFERENCES "ApplicationUsers" ("Id") ON DELETE RESTRICT,
    CONSTRAINT "FK_Rooms_ApplicationUsers_UpdatedById" FOREIGN KEY ("UpdatedById") REFERENCES "ApplicationUsers" ("Id") ON DELETE RESTRICT
);

CREATE TABLE "Rules" (
    "Id" uuid NOT NULL,
    "Name" text NOT NULL,
    "Description" text,
    "Condition" jsonb NOT NULL,
    "Action" jsonb NOT NULL,
    "IsActive" boolean NOT NULL,
    "CreatedAt" timestamp with time zone NOT NULL,
    "UpdatedAt" timestamp with time zone,
    "IsDeleted" boolean NOT NULL,
    "CreatedById" text NOT NULL,
    "UpdatedById" text,
    CONSTRAINT "PK_Rules" PRIMARY KEY ("Id"),
    CONSTRAINT "FK_Rules_ApplicationUsers_CreatedById" FOREIGN KEY ("CreatedById") REFERENCES "ApplicationUsers" ("Id") ON DELETE RESTRICT,
    CONSTRAINT "FK_Rules_ApplicationUsers_UpdatedById" FOREIGN KEY ("UpdatedById") REFERENCES "ApplicationUsers" ("Id") ON DELETE RESTRICT
);

CREATE TABLE "UserSettings" (
    "UserId" text NOT NULL,
    "Theme" text NOT NULL,
    "Language" text NOT NULL,
    "ShowOfflineDevices" boolean NOT NULL,
    "DefaultView" text NOT NULL,
    "DashboardLayout" jsonb,
    "FavoriteDevices" jsonb,
    "QuickActions" jsonb,
    "EmailNotifications" boolean NOT NULL,
    "PushNotifications" boolean NOT NULL,
    "NotificationPreferences" jsonb,
    "DeviceDisplayPreferences" jsonb,
    "LastUsedDevices" jsonb,
    "RoomDisplayOrder" jsonb,
    "ShowEmptyRooms" boolean NOT NULL,
    "ShowAutomationSuggestions" boolean NOT NULL,
    "AutomationPreferences" jsonb,
    "UserId1" text NOT NULL,
    "Id" uuid NOT NULL,
    "CreatedAt" timestamp with time zone NOT NULL,
    "UpdatedAt" timestamp with time zone,
    "IsDeleted" boolean NOT NULL,
    "CreatedById" text NOT NULL,
    "UpdatedById" text,
    CONSTRAINT "PK_UserSettings" PRIMARY KEY ("UserId"),
    CONSTRAINT "FK_UserSettings_ApplicationUsers_CreatedById" FOREIGN KEY ("CreatedById") REFERENCES "ApplicationUsers" ("Id") ON DELETE RESTRICT,
    CONSTRAINT "FK_UserSettings_ApplicationUsers_UpdatedById" FOREIGN KEY ("UpdatedById") REFERENCES "ApplicationUsers" ("Id") ON DELETE RESTRICT,
    CONSTRAINT "FK_UserSettings_ApplicationUsers_UserId" FOREIGN KEY ("UserId") REFERENCES "ApplicationUsers" ("Id") ON DELETE CASCADE,
    CONSTRAINT "FK_UserSettings_ApplicationUsers_UserId1" FOREIGN KEY ("UserId1") REFERENCES "ApplicationUsers" ("Id") ON DELETE CASCADE
);

CREATE TABLE "AspNetRoleClaims" (
    "Id" integer GENERATED BY DEFAULT AS IDENTITY,
    "RoleId" text NOT NULL,
    "ClaimType" text,
    "ClaimValue" text,
    CONSTRAINT "PK_AspNetRoleClaims" PRIMARY KEY ("Id"),
    CONSTRAINT "FK_AspNetRoleClaims_AspNetRoles_RoleId" FOREIGN KEY ("RoleId") REFERENCES "AspNetRoles" ("Id") ON DELETE CASCADE
);

CREATE TABLE "AspNetUserClaims" (
    "Id" integer GENERATED BY DEFAULT AS IDENTITY,
    "UserId" text NOT NULL,
    "ClaimType" text,
    "ClaimValue" text,
    CONSTRAINT "PK_AspNetUserClaims" PRIMARY KEY ("Id"),
    CONSTRAINT "FK_AspNetUserClaims_AspNetUsers_UserId" FOREIGN KEY ("UserId") REFERENCES "AspNetUsers" ("Id") ON DELETE CASCADE
);

CREATE TABLE "AspNetUserLogins" (
    "LoginProvider" text NOT NULL,
    "ProviderKey" text NOT NULL,
    "ProviderDisplayName" text,
    "UserId" text NOT NULL,
    CONSTRAINT "PK_AspNetUserLogins" PRIMARY KEY ("LoginProvider", "ProviderKey"),
    CONSTRAINT "FK_AspNetUserLogins_AspNetUsers_UserId" FOREIGN KEY ("UserId") REFERENCES "AspNetUsers" ("Id") ON DELETE CASCADE
);

CREATE TABLE "AspNetUserRoles" (
    "UserId" text NOT NULL,
    "RoleId" text NOT NULL,
    CONSTRAINT "PK_AspNetUserRoles" PRIMARY KEY ("UserId", "RoleId"),
    CONSTRAINT "FK_AspNetUserRoles_AspNetRoles_RoleId" FOREIGN KEY ("RoleId") REFERENCES "AspNetRoles" ("Id") ON DELETE CASCADE,
    CONSTRAINT "FK_AspNetUserRoles_AspNetUsers_UserId" FOREIGN KEY ("UserId") REFERENCES "AspNetUsers" ("Id") ON DELETE CASCADE
);

CREATE TABLE "AspNetUserTokens" (
    "UserId" text NOT NULL,
    "LoginProvider" text NOT NULL,
    "Name" text NOT NULL,
    "Value" text,
    CONSTRAINT "PK_AspNetUserTokens" PRIMARY KEY ("UserId", "LoginProvider", "Name"),
    CONSTRAINT "FK_AspNetUserTokens_AspNetUsers_UserId" FOREIGN KEY ("UserId") REFERENCES "AspNetUsers" ("Id") ON DELETE CASCADE
);

CREATE TABLE "GroupStateHistory" (
    "Id" uuid NOT NULL,
    "GroupId" uuid NOT NULL,
    "GroupId1" uuid NOT NULL,
    "OldState" jsonb NOT NULL,
    "NewState" jsonb NOT NULL,
    "Description" text,
    "CreatedAt" timestamp with time zone NOT NULL,
    "UpdatedAt" timestamp with time zone,
    "IsDeleted" boolean NOT NULL,
    "CreatedById" text NOT NULL,
    "UpdatedById" text,
    CONSTRAINT "PK_GroupStateHistory" PRIMARY KEY ("Id"),
    CONSTRAINT "FK_GroupStateHistory_ApplicationUsers_CreatedById" FOREIGN KEY ("CreatedById") REFERENCES "ApplicationUsers" ("Id") ON DELETE RESTRICT,
    CONSTRAINT "FK_GroupStateHistory_ApplicationUsers_UpdatedById" FOREIGN KEY ("UpdatedById") REFERENCES "ApplicationUsers" ("Id") ON DELETE RESTRICT,
    CONSTRAINT "FK_GroupStateHistory_Groups_GroupId" FOREIGN KEY ("GroupId") REFERENCES "Groups" ("Id") ON DELETE CASCADE,
    CONSTRAINT "FK_GroupStateHistory_Groups_GroupId1" FOREIGN KEY ("GroupId1") REFERENCES "Groups" ("Id") ON DELETE CASCADE
);

CREATE TABLE "GroupStates" (
    "GroupId" uuid NOT NULL,
    "State" jsonb NOT NULL,
    "LastUpdated" timestamp with time zone NOT NULL,
    "Id" uuid NOT NULL,
    "CreatedAt" timestamp with time zone NOT NULL,
    "UpdatedAt" timestamp with time zone,
    "IsDeleted" boolean NOT NULL,
    "CreatedById" text NOT NULL,
    "UpdatedById" text,
    CONSTRAINT "PK_GroupStates" PRIMARY KEY ("GroupId"),
    CONSTRAINT "FK_GroupStates_ApplicationUsers_CreatedById" FOREIGN KEY ("CreatedById") REFERENCES "ApplicationUsers" ("Id") ON DELETE RESTRICT,
    CONSTRAINT "FK_GroupStates_ApplicationUsers_UpdatedById" FOREIGN KEY ("UpdatedById") REFERENCES "ApplicationUsers" ("Id") ON DELETE RESTRICT,
    CONSTRAINT "FK_GroupStates_Groups_GroupId" FOREIGN KEY ("GroupId") REFERENCES "Groups" ("Id") ON DELETE CASCADE
);

CREATE TABLE "DeviceGroups" (
    "Id" uuid NOT NULL,
    "Name" text NOT NULL,
    "Description" text,
    "RoomId" uuid,
    "CreatedAt" timestamp with time zone NOT NULL,
    "UpdatedAt" timestamp with time zone,
    "IsDeleted" boolean NOT NULL,
    "CreatedById" text NOT NULL,
    "UpdatedById" text,
    CONSTRAINT "PK_DeviceGroups" PRIMARY KEY ("Id"),
    CONSTRAINT "FK_DeviceGroups_ApplicationUsers_CreatedById" FOREIGN KEY ("CreatedById") REFERENCES "ApplicationUsers" ("Id") ON DELETE RESTRICT,
    CONSTRAINT "FK_DeviceGroups_ApplicationUsers_UpdatedById" FOREIGN KEY ("UpdatedById") REFERENCES "ApplicationUsers" ("Id") ON DELETE RESTRICT,
    CONSTRAINT "FK_DeviceGroups_Rooms_RoomId" FOREIGN KEY ("RoomId") REFERENCES "Rooms" ("Id")
);

CREATE TABLE "Devices" (
    "Id" uuid NOT NULL,
    "Name" text NOT NULL,
    "Description" text,
    "MatterDeviceId" text,
    "DeviceTypeId" uuid NOT NULL,
    "RoomId" uuid,
    "Properties" jsonb NOT NULL,
    "Status" text NOT NULL,
    "Configuration" jsonb,
    "LastStateChange" timestamp with time zone NOT NULL,
    "IsOnline" boolean NOT NULL,
    "LastSeen" timestamp with time zone NOT NULL,
    "CreatedAt" timestamp with time zone NOT NULL,
    "UpdatedAt" timestamp with time zone,
    "IsDeleted" boolean NOT NULL,
    "CreatedById" text NOT NULL,
    "UpdatedById" text,
    CONSTRAINT "PK_Devices" PRIMARY KEY ("Id"),
    CONSTRAINT "FK_Devices_ApplicationUsers_CreatedById" FOREIGN KEY ("CreatedById") REFERENCES "ApplicationUsers" ("Id") ON DELETE RESTRICT,
    CONSTRAINT "FK_Devices_ApplicationUsers_UpdatedById" FOREIGN KEY ("UpdatedById") REFERENCES "ApplicationUsers" ("Id") ON DELETE RESTRICT,
    CONSTRAINT "FK_Devices_DeviceTypes_DeviceTypeId" FOREIGN KEY ("DeviceTypeId") REFERENCES "DeviceTypes" ("Id") ON DELETE RESTRICT,
    CONSTRAINT "FK_Devices_Rooms_RoomId" FOREIGN KEY ("RoomId") REFERENCES "Rooms" ("Id") ON DELETE RESTRICT
);

CREATE TABLE "UserRoomPermissions" (
    "Id" uuid NOT NULL,
    "UserId" text NOT NULL,
    "RoomId" uuid NOT NULL,
    "Permission" text NOT NULL,
    "CreatedAt" timestamp with time zone NOT NULL,
    "UpdatedAt" timestamp with time zone,
    "IsDeleted" boolean NOT NULL,
    "CreatedById" text NOT NULL,
    "UpdatedById" text,
    CONSTRAINT "PK_UserRoomPermissions" PRIMARY KEY ("Id"),
    CONSTRAINT "FK_UserRoomPermissions_ApplicationUsers_CreatedById" FOREIGN KEY ("CreatedById") REFERENCES "ApplicationUsers" ("Id") ON DELETE RESTRICT,
    CONSTRAINT "FK_UserRoomPermissions_ApplicationUsers_UpdatedById" FOREIGN KEY ("UpdatedById") REFERENCES "ApplicationUsers" ("Id") ON DELETE RESTRICT,
    CONSTRAINT "FK_UserRoomPermissions_ApplicationUsers_UserId" FOREIGN KEY ("UserId") REFERENCES "ApplicationUsers" ("Id") ON DELETE CASCADE,
    CONSTRAINT "FK_UserRoomPermissions_Rooms_RoomId" FOREIGN KEY ("RoomId") REFERENCES "Rooms" ("Id") ON DELETE CASCADE
);

CREATE TABLE "RuleActions" (
    "Id" uuid NOT NULL,
    "RuleId" uuid NOT NULL,
    "RuleId1" uuid NOT NULL,
    "Action" jsonb NOT NULL,
    "Order" integer NOT NULL,
    "CreatedAt" timestamp with time zone NOT NULL,
    "UpdatedAt" timestamp with time zone,
    "IsDeleted" boolean NOT NULL,
    "CreatedById" text NOT NULL,
    "UpdatedById" text,
    CONSTRAINT "PK_RuleActions" PRIMARY KEY ("Id"),
    CONSTRAINT "FK_RuleActions_ApplicationUsers_CreatedById" FOREIGN KEY ("CreatedById") REFERENCES "ApplicationUsers" ("Id") ON DELETE RESTRICT,
    CONSTRAINT "FK_RuleActions_ApplicationUsers_UpdatedById" FOREIGN KEY ("UpdatedById") REFERENCES "ApplicationUsers" ("Id") ON DELETE RESTRICT,
    CONSTRAINT "FK_RuleActions_Rules_RuleId" FOREIGN KEY ("RuleId") REFERENCES "Rules" ("Id") ON DELETE CASCADE,
    CONSTRAINT "FK_RuleActions_Rules_RuleId1" FOREIGN KEY ("RuleId1") REFERENCES "Rules" ("Id") ON DELETE CASCADE
);

CREATE TABLE "RuleConditions" (
    "Id" uuid NOT NULL,
    "RuleId" uuid NOT NULL,
    "RuleId1" uuid NOT NULL,
    "Condition" jsonb NOT NULL,
    "Order" integer NOT NULL,
    "CreatedAt" timestamp with time zone NOT NULL,
    "UpdatedAt" timestamp with time zone,
    "IsDeleted" boolean NOT NULL,
    "CreatedById" text NOT NULL,
    "UpdatedById" text,
    CONSTRAINT "PK_RuleConditions" PRIMARY KEY ("Id"),
    CONSTRAINT "FK_RuleConditions_ApplicationUsers_CreatedById" FOREIGN KEY ("CreatedById") REFERENCES "ApplicationUsers" ("Id") ON DELETE RESTRICT,
    CONSTRAINT "FK_RuleConditions_ApplicationUsers_UpdatedById" FOREIGN KEY ("UpdatedById") REFERENCES "ApplicationUsers" ("Id") ON DELETE RESTRICT,
    CONSTRAINT "FK_RuleConditions_Rules_RuleId" FOREIGN KEY ("RuleId") REFERENCES "Rules" ("Id") ON DELETE CASCADE,
    CONSTRAINT "FK_RuleConditions_Rules_RuleId1" FOREIGN KEY ("RuleId1") REFERENCES "Rules" ("Id") ON DELETE CASCADE
);

CREATE TABLE "RuleExecutionHistory" (
    "Id" uuid NOT NULL,
    "RuleId" uuid NOT NULL,
    "RuleId1" uuid NOT NULL,
    "Success" boolean NOT NULL,
    "Result" jsonb,
    "ErrorMessage" text,
    "ExecutionTime" timestamp with time zone NOT NULL,
    "CreatedAt" timestamp with time zone NOT NULL,
    "UpdatedAt" timestamp with time zone,
    "IsDeleted" boolean NOT NULL,
    "CreatedById" text NOT NULL,
    "UpdatedById" text,
    CONSTRAINT "PK_RuleExecutionHistory" PRIMARY KEY ("Id"),
    CONSTRAINT "FK_RuleExecutionHistory_ApplicationUsers_CreatedById" FOREIGN KEY ("CreatedById") REFERENCES "ApplicationUsers" ("Id") ON DELETE RESTRICT,
    CONSTRAINT "FK_RuleExecutionHistory_ApplicationUsers_UpdatedById" FOREIGN KEY ("UpdatedById") REFERENCES "ApplicationUsers" ("Id") ON DELETE RESTRICT,
    CONSTRAINT "FK_RuleExecutionHistory_Rules_RuleId" FOREIGN KEY ("RuleId") REFERENCES "Rules" ("Id") ON DELETE CASCADE,
    CONSTRAINT "FK_RuleExecutionHistory_Rules_RuleId1" FOREIGN KEY ("RuleId1") REFERENCES "Rules" ("Id") ON DELETE CASCADE
);

CREATE TABLE "RuleTriggers" (
    "Id" uuid NOT NULL,
    "RuleId" uuid NOT NULL,
    "TriggerType" text NOT NULL,
    "Trigger" jsonb NOT NULL,
    "IsEnabled" boolean NOT NULL,
    "LastTriggered" timestamp with time zone,
    "RuleId1" uuid NOT NULL,
    "CreatedAt" timestamp with time zone NOT NULL,
    "UpdatedAt" timestamp with time zone,
    "IsDeleted" boolean NOT NULL,
    "CreatedById" text NOT NULL,
    "UpdatedById" text,
    CONSTRAINT "PK_RuleTriggers" PRIMARY KEY ("Id"),
    CONSTRAINT "FK_RuleTriggers_ApplicationUsers_CreatedById" FOREIGN KEY ("CreatedById") REFERENCES "ApplicationUsers" ("Id") ON DELETE RESTRICT,
    CONSTRAINT "FK_RuleTriggers_ApplicationUsers_UpdatedById" FOREIGN KEY ("UpdatedById") REFERENCES "ApplicationUsers" ("Id") ON DELETE RESTRICT,
    CONSTRAINT "FK_RuleTriggers_Rules_RuleId" FOREIGN KEY ("RuleId") REFERENCES "Rules" ("Id") ON DELETE CASCADE,
    CONSTRAINT "FK_RuleTriggers_Rules_RuleId1" FOREIGN KEY ("RuleId1") REFERENCES "Rules" ("Id") ON DELETE CASCADE
);

CREATE TABLE "DeviceDeviceGroup" (
    "DeviceGroupsId" uuid NOT NULL,
    "DevicesId" uuid NOT NULL,
    CONSTRAINT "PK_DeviceDeviceGroup" PRIMARY KEY ("DeviceGroupsId", "DevicesId"),
    CONSTRAINT "FK_DeviceDeviceGroup_DeviceGroups_DeviceGroupsId" FOREIGN KEY ("DeviceGroupsId") REFERENCES "DeviceGroups" ("Id") ON DELETE CASCADE,
    CONSTRAINT "FK_DeviceDeviceGroup_Devices_DevicesId" FOREIGN KEY ("DevicesId") REFERENCES "Devices" ("Id") ON DELETE CASCADE
);

CREATE TABLE "DeviceEvents" (
    "Id" uuid NOT NULL,
    "DeviceId" uuid NOT NULL,
    "EventType" text NOT NULL,
    "EventData" jsonb,
    "CreatedAt" timestamp with time zone NOT NULL,
    "UpdatedAt" timestamp with time zone,
    "IsDeleted" boolean NOT NULL,
    "CreatedById" text NOT NULL,
    "UpdatedById" text,
    CONSTRAINT "PK_DeviceEvents" PRIMARY KEY ("Id"),
    CONSTRAINT "FK_DeviceEvents_ApplicationUsers_CreatedById" FOREIGN KEY ("CreatedById") REFERENCES "ApplicationUsers" ("Id") ON DELETE RESTRICT,
    CONSTRAINT "FK_DeviceEvents_ApplicationUsers_UpdatedById" FOREIGN KEY ("UpdatedById") REFERENCES "ApplicationUsers" ("Id") ON DELETE RESTRICT,
    CONSTRAINT "FK_DeviceEvents_Devices_DeviceId" FOREIGN KEY ("DeviceId") REFERENCES "Devices" ("Id") ON DELETE CASCADE
);

CREATE TABLE "DeviceGroupMembers" (
    "DeviceId" uuid NOT NULL,
    "DeviceGroupId" uuid NOT NULL,
    "CreatedAt" timestamp with time zone NOT NULL,
    "UpdatedAt" timestamp with time zone,
    "IsDeleted" boolean NOT NULL,
    "CreatedById" text NOT NULL,
    "UpdatedById" text,
    "Comment" text,
    CONSTRAINT "PK_DeviceGroupMembers" PRIMARY KEY ("DeviceId", "DeviceGroupId"),
    CONSTRAINT "FK_DeviceGroupMembers_ApplicationUsers_CreatedById" FOREIGN KEY ("CreatedById") REFERENCES "ApplicationUsers" ("Id") ON DELETE CASCADE,
    CONSTRAINT "FK_DeviceGroupMembers_ApplicationUsers_UpdatedById" FOREIGN KEY ("UpdatedById") REFERENCES "ApplicationUsers" ("Id"),
    CONSTRAINT "FK_DeviceGroupMembers_DeviceGroups_DeviceGroupId" FOREIGN KEY ("DeviceGroupId") REFERENCES "DeviceGroups" ("Id") ON DELETE CASCADE,
    CONSTRAINT "FK_DeviceGroupMembers_Devices_DeviceId" FOREIGN KEY ("DeviceId") REFERENCES "Devices" ("Id") ON DELETE CASCADE
);

CREATE TABLE "DeviceHistory" (
    "Id" uuid NOT NULL,
    "DeviceId" uuid NOT NULL,
    "EventType" text NOT NULL,
    "OldState" jsonb,
    "NewState" jsonb,
    "Description" text,
    "CreatedAt" timestamp with time zone NOT NULL,
    "UpdatedAt" timestamp with time zone,
    "IsDeleted" boolean NOT NULL,
    "CreatedById" text NOT NULL,
    "UpdatedById" text,
    CONSTRAINT "PK_DeviceHistory" PRIMARY KEY ("Id"),
    CONSTRAINT "FK_DeviceHistory_ApplicationUsers_CreatedById" FOREIGN KEY ("CreatedById") REFERENCES "ApplicationUsers" ("Id") ON DELETE RESTRICT,
    CONSTRAINT "FK_DeviceHistory_ApplicationUsers_UpdatedById" FOREIGN KEY ("UpdatedById") REFERENCES "ApplicationUsers" ("Id") ON DELETE RESTRICT,
    CONSTRAINT "FK_DeviceHistory_Devices_DeviceId" FOREIGN KEY ("DeviceId") REFERENCES "Devices" ("Id") ON DELETE CASCADE
);

CREATE TABLE "DeviceStates" (
    "Id" uuid NOT NULL,
    "DeviceId" uuid NOT NULL,
    "StateType" text NOT NULL,
    "StateValue" jsonb NOT NULL,
    "RecordedAt" timestamp with time zone NOT NULL,
    "CreatedAt" timestamp with time zone NOT NULL,
    "UpdatedAt" timestamp with time zone,
    "IsDeleted" boolean NOT NULL,
    "CreatedById" text NOT NULL,
    "UpdatedById" text,
    CONSTRAINT "PK_DeviceStates" PRIMARY KEY ("Id"),
    CONSTRAINT "FK_DeviceStates_ApplicationUsers_CreatedById" FOREIGN KEY ("CreatedById") REFERENCES "ApplicationUsers" ("Id") ON DELETE RESTRICT,
    CONSTRAINT "FK_DeviceStates_ApplicationUsers_UpdatedById" FOREIGN KEY ("UpdatedById") REFERENCES "ApplicationUsers" ("Id") ON DELETE RESTRICT,
    CONSTRAINT "FK_DeviceStates_Devices_DeviceId" FOREIGN KEY ("DeviceId") REFERENCES "Devices" ("Id") ON DELETE CASCADE
);

CREATE TABLE "GroupMembers" (
    "GroupId" uuid NOT NULL,
    "DeviceId" uuid NOT NULL,
    "GroupId1" uuid NOT NULL,
    "DeviceId1" uuid NOT NULL,
    "Id" uuid NOT NULL,
    "CreatedAt" timestamp with time zone NOT NULL,
    "UpdatedAt" timestamp with time zone,
    "IsDeleted" boolean NOT NULL,
    "CreatedById" text NOT NULL,
    "UpdatedById" text,
    CONSTRAINT "PK_GroupMembers" PRIMARY KEY ("GroupId", "DeviceId"),
    CONSTRAINT "FK_GroupMembers_ApplicationUsers_CreatedById" FOREIGN KEY ("CreatedById") REFERENCES "ApplicationUsers" ("Id") ON DELETE RESTRICT,
    CONSTRAINT "FK_GroupMembers_ApplicationUsers_UpdatedById" FOREIGN KEY ("UpdatedById") REFERENCES "ApplicationUsers" ("Id") ON DELETE RESTRICT,
    CONSTRAINT "FK_GroupMembers_Devices_DeviceId" FOREIGN KEY ("DeviceId") REFERENCES "Devices" ("Id") ON DELETE CASCADE,
    CONSTRAINT "FK_GroupMembers_Devices_DeviceId1" FOREIGN KEY ("DeviceId1") REFERENCES "Devices" ("Id") ON DELETE CASCADE,
    CONSTRAINT "FK_GroupMembers_Groups_GroupId" FOREIGN KEY ("GroupId") REFERENCES "Groups" ("Id") ON DELETE CASCADE,
    CONSTRAINT "FK_GroupMembers_Groups_GroupId1" FOREIGN KEY ("GroupId1") REFERENCES "Groups" ("Id") ON DELETE CASCADE
);

CREATE TABLE "UserDevicePermissions" (
    "Id" uuid NOT NULL,
    "UserId" text NOT NULL,
    "DeviceId" uuid NOT NULL,
    "PermissionLevel" text NOT NULL,
    "CreatedAt" timestamp with time zone NOT NULL,
    "UpdatedAt" timestamp with time zone,
    "IsDeleted" boolean NOT NULL,
    "CreatedById" text NOT NULL,
    "UpdatedById" text,
    CONSTRAINT "PK_UserDevicePermissions" PRIMARY KEY ("Id"),
    CONSTRAINT "FK_UserDevicePermissions_ApplicationUsers_CreatedById" FOREIGN KEY ("CreatedById") REFERENCES "ApplicationUsers" ("Id") ON DELETE RESTRICT,
    CONSTRAINT "FK_UserDevicePermissions_ApplicationUsers_UpdatedById" FOREIGN KEY ("UpdatedById") REFERENCES "ApplicationUsers" ("Id") ON DELETE RESTRICT,
    CONSTRAINT "FK_UserDevicePermissions_ApplicationUsers_UserId" FOREIGN KEY ("UserId") REFERENCES "ApplicationUsers" ("Id") ON DELETE CASCADE,
    CONSTRAINT "FK_UserDevicePermissions_Devices_DeviceId" FOREIGN KEY ("DeviceId") REFERENCES "Devices" ("Id") ON DELETE CASCADE
);

CREATE INDEX "IX_ApplicationUsers_CreatedById" ON "ApplicationUsers" ("CreatedById");

CREATE UNIQUE INDEX "IX_ApplicationUsers_Email" ON "ApplicationUsers" ("Email");

CREATE INDEX "IX_ApplicationUsers_UpdatedById" ON "ApplicationUsers" ("UpdatedById");

CREATE UNIQUE INDEX "IX_ApplicationUsers_UserName" ON "ApplicationUsers" ("UserName");

CREATE INDEX "IX_AspNetRoleClaims_RoleId" ON "AspNetRoleClaims" ("RoleId");

CREATE UNIQUE INDEX "RoleNameIndex" ON "AspNetRoles" ("NormalizedName");

CREATE INDEX "IX_AspNetUserClaims_UserId" ON "AspNetUserClaims" ("UserId");

CREATE INDEX "IX_AspNetUserLogins_UserId" ON "AspNetUserLogins" ("UserId");

CREATE INDEX "IX_AspNetUserRoles_RoleId" ON "AspNetUserRoles" ("RoleId");

CREATE INDEX "EmailIndex" ON "AspNetUsers" ("NormalizedEmail");

CREATE UNIQUE INDEX "UserNameIndex" ON "AspNetUsers" ("NormalizedUserName");

CREATE INDEX "IX_DeviceDeviceGroup_DevicesId" ON "DeviceDeviceGroup" ("DevicesId");

CREATE INDEX "IX_DeviceEvents_CreatedById" ON "DeviceEvents" ("CreatedById");

CREATE INDEX "IX_DeviceEvents_DeviceId" ON "DeviceEvents" ("DeviceId");

CREATE INDEX "IX_DeviceEvents_UpdatedById" ON "DeviceEvents" ("UpdatedById");

CREATE INDEX "IX_DeviceGroupMembers_CreatedById" ON "DeviceGroupMembers" ("CreatedById");

CREATE INDEX "IX_DeviceGroupMembers_DeviceGroupId" ON "DeviceGroupMembers" ("DeviceGroupId");

CREATE INDEX "IX_DeviceGroupMembers_UpdatedById" ON "DeviceGroupMembers" ("UpdatedById");

CREATE INDEX "IX_DeviceGroups_CreatedById" ON "DeviceGroups" ("CreatedById");

CREATE INDEX "IX_DeviceGroups_RoomId" ON "DeviceGroups" ("RoomId");

CREATE INDEX "IX_DeviceGroups_UpdatedById" ON "DeviceGroups" ("UpdatedById");

CREATE INDEX "IX_DeviceHistory_CreatedById" ON "DeviceHistory" ("CreatedById");

CREATE INDEX "IX_DeviceHistory_DeviceId" ON "DeviceHistory" ("DeviceId");

CREATE INDEX "IX_DeviceHistory_UpdatedById" ON "DeviceHistory" ("UpdatedById");

CREATE INDEX "IX_Devices_CreatedById" ON "Devices" ("CreatedById");

CREATE INDEX "IX_Devices_DeviceTypeId" ON "Devices" ("DeviceTypeId");

CREATE UNIQUE INDEX "IX_Devices_MatterDeviceId" ON "Devices" ("MatterDeviceId") WHERE "MatterDeviceId" IS NOT NULL;

CREATE INDEX "IX_Devices_RoomId" ON "Devices" ("RoomId");

CREATE INDEX "IX_Devices_UpdatedById" ON "Devices" ("UpdatedById");

CREATE INDEX "IX_DeviceStates_CreatedById" ON "DeviceStates" ("CreatedById");

CREATE INDEX "IX_DeviceStates_DeviceId" ON "DeviceStates" ("DeviceId");

CREATE INDEX "IX_DeviceStates_UpdatedById" ON "DeviceStates" ("UpdatedById");

CREATE INDEX "IX_DeviceTypes_CreatedById" ON "DeviceTypes" ("CreatedById");

CREATE INDEX "IX_DeviceTypes_UpdatedById" ON "DeviceTypes" ("UpdatedById");

CREATE INDEX "IX_EnvironmentalSettings_CreatedById" ON "EnvironmentalSettings" ("CreatedById");

CREATE INDEX "IX_EnvironmentalSettings_UpdatedById" ON "EnvironmentalSettings" ("UpdatedById");

CREATE UNIQUE INDEX "IX_EnvironmentalSettings_UserId" ON "EnvironmentalSettings" ("UserId");

CREATE INDEX "IX_GroupMembers_CreatedById" ON "GroupMembers" ("CreatedById");

CREATE INDEX "IX_GroupMembers_DeviceId" ON "GroupMembers" ("DeviceId");

CREATE INDEX "IX_GroupMembers_DeviceId1" ON "GroupMembers" ("DeviceId1");

CREATE INDEX "IX_GroupMembers_GroupId1" ON "GroupMembers" ("GroupId1");

CREATE INDEX "IX_GroupMembers_UpdatedById" ON "GroupMembers" ("UpdatedById");

CREATE INDEX "IX_Groups_CreatedById" ON "Groups" ("CreatedById");

CREATE INDEX "IX_Groups_UpdatedById" ON "Groups" ("UpdatedById");

CREATE INDEX "IX_GroupStateHistory_CreatedById" ON "GroupStateHistory" ("CreatedById");

CREATE INDEX "IX_GroupStateHistory_GroupId" ON "GroupStateHistory" ("GroupId");

CREATE INDEX "IX_GroupStateHistory_GroupId1" ON "GroupStateHistory" ("GroupId1");

CREATE INDEX "IX_GroupStateHistory_UpdatedById" ON "GroupStateHistory" ("UpdatedById");

CREATE INDEX "IX_GroupStates_CreatedById" ON "GroupStates" ("CreatedById");

CREATE INDEX "IX_GroupStates_UpdatedById" ON "GroupStates" ("UpdatedById");

CREATE INDEX "IX_Notifications_CreatedById" ON "Notifications" ("CreatedById");

CREATE INDEX "IX_Notifications_UpdatedById" ON "Notifications" ("UpdatedById");

CREATE INDEX "IX_Rooms_CreatedById" ON "Rooms" ("CreatedById");

CREATE INDEX "IX_Rooms_UpdatedById" ON "Rooms" ("UpdatedById");

CREATE INDEX "IX_RuleActions_CreatedById" ON "RuleActions" ("CreatedById");

CREATE INDEX "IX_RuleActions_RuleId" ON "RuleActions" ("RuleId");

CREATE INDEX "IX_RuleActions_RuleId1" ON "RuleActions" ("RuleId1");

CREATE INDEX "IX_RuleActions_UpdatedById" ON "RuleActions" ("UpdatedById");

CREATE INDEX "IX_RuleConditions_CreatedById" ON "RuleConditions" ("CreatedById");

CREATE INDEX "IX_RuleConditions_RuleId" ON "RuleConditions" ("RuleId");

CREATE INDEX "IX_RuleConditions_RuleId1" ON "RuleConditions" ("RuleId1");

CREATE INDEX "IX_RuleConditions_UpdatedById" ON "RuleConditions" ("UpdatedById");

CREATE INDEX "IX_RuleExecutionHistory_CreatedById" ON "RuleExecutionHistory" ("CreatedById");

CREATE INDEX "IX_RuleExecutionHistory_RuleId" ON "RuleExecutionHistory" ("RuleId");

CREATE INDEX "IX_RuleExecutionHistory_RuleId1" ON "RuleExecutionHistory" ("RuleId1");

CREATE INDEX "IX_RuleExecutionHistory_UpdatedById" ON "RuleExecutionHistory" ("UpdatedById");

CREATE INDEX "IX_Rules_CreatedById" ON "Rules" ("CreatedById");

CREATE INDEX "IX_Rules_UpdatedById" ON "Rules" ("UpdatedById");

CREATE INDEX "IX_RuleTriggers_CreatedById" ON "RuleTriggers" ("CreatedById");

CREATE INDEX "IX_RuleTriggers_RuleId" ON "RuleTriggers" ("RuleId");

CREATE INDEX "IX_RuleTriggers_RuleId1" ON "RuleTriggers" ("RuleId1");

CREATE INDEX "IX_RuleTriggers_UpdatedById" ON "RuleTriggers" ("UpdatedById");

CREATE INDEX "IX_UserDevicePermissions_CreatedById" ON "UserDevicePermissions" ("CreatedById");

CREATE INDEX "IX_UserDevicePermissions_DeviceId" ON "UserDevicePermissions" ("DeviceId");

CREATE INDEX "IX_UserDevicePermissions_UpdatedById" ON "UserDevicePermissions" ("UpdatedById");

CREATE INDEX "IX_UserDevicePermissions_UserId" ON "UserDevicePermissions" ("UserId");

CREATE INDEX "IX_UserRoomPermissions_CreatedById" ON "UserRoomPermissions" ("CreatedById");

CREATE INDEX "IX_UserRoomPermissions_RoomId" ON "UserRoomPermissions" ("RoomId");

CREATE INDEX "IX_UserRoomPermissions_UpdatedById" ON "UserRoomPermissions" ("UpdatedById");

CREATE INDEX "IX_UserRoomPermissions_UserId" ON "UserRoomPermissions" ("UserId");

CREATE INDEX "IX_UserSettings_CreatedById" ON "UserSettings" ("CreatedById");

CREATE INDEX "IX_UserSettings_UpdatedById" ON "UserSettings" ("UpdatedById");

CREATE INDEX "IX_UserSettings_UserId1" ON "UserSettings" ("UserId1");

INSERT INTO "__EFMigrationsHistory" ("MigrationId", "ProductVersion")
VALUES ('20250620142356_FreshStartWithStringUserIds', '8.0.2');

COMMIT;

