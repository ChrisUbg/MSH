--
-- PostgreSQL database dump
--

-- Dumped from database version 16.8 (Debian 16.8-1.pgdg120+1)
-- Dumped by pg_dump version 16.8 (Debian 16.8-1.pgdg120+1)

SET statement_timeout = 0;
SET lock_timeout = 0;
SET idle_in_transaction_session_timeout = 0;
SET client_encoding = 'UTF8';
SET standard_conforming_strings = on;
SELECT pg_catalog.set_config('search_path', '', false);
SET check_function_bodies = false;
SET xmloption = content;
SET client_min_messages = warning;
SET row_security = off;

--
-- Data for Name: ApplicationUsers; Type: TABLE DATA; Schema: db; Owner: -
--

COPY db."ApplicationUsers" ("Id", "CreatedAt", "UpdatedAt", "IsDeleted", "CreatedById", "UpdatedById", "UserName", "Email", "FirstName", "LastName", "IsActive", "LastLogin") FROM stdin;
bb1be326-f26e-4684-bbf5-5c3df450dc61	2025-07-27 21:05:26.961571+00	2025-07-27 21:06:29.003431+00	f	bb1be326-f26e-4684-bbf5-5c3df450dc61	bb1be326-f26e-4684-bbf5-5c3df450dc61	system	system@msh.local	System	User	t	2025-07-27 21:06:29.003431+00
admin-user-id-1234-5678-9012-345678901234	2025-07-27 21:05:26.968293+00	2025-07-27 21:06:29.008029+00	f	bb1be326-f26e-4684-bbf5-5c3df450dc61	bb1be326-f26e-4684-bbf5-5c3df450dc61	admin	admin@msh.local	Admin	User	t	2025-07-27 21:06:29.008029+00
\.


--
-- Data for Name: AspNetRoles; Type: TABLE DATA; Schema: db; Owner: -
--

COPY db."AspNetRoles" ("Id", "Name", "NormalizedName", "ConcurrencyStamp") FROM stdin;
admin-role-id	Admin	ADMIN	admin-role-stamp
standard-role-id	Standard	STANDARD	standard-role-stamp
guest-role-id	Guest	GUEST	guest-role-stamp
\.


--
-- Data for Name: AspNetRoleClaims; Type: TABLE DATA; Schema: db; Owner: -
--

COPY db."AspNetRoleClaims" ("Id", "RoleId", "ClaimType", "ClaimValue") FROM stdin;
\.


--
-- Data for Name: AspNetUsers; Type: TABLE DATA; Schema: db; Owner: -
--

COPY db."AspNetUsers" ("Id", "UserName", "NormalizedUserName", "Email", "NormalizedEmail", "EmailConfirmed", "PasswordHash", "SecurityStamp", "ConcurrencyStamp", "PhoneNumber", "PhoneNumberConfirmed", "TwoFactorEnabled", "LockoutEnd", "LockoutEnabled", "AccessFailedCount") FROM stdin;
admin-user-id-1234-5678-9012-345678901234	admin@msh.local	ADMIN@MSH.LOCAL	admin@msh.local	ADMIN@MSH.LOCAL	t	AQAAAAEAACcQAAAAELbXpFJNk2p+Pj5KqON+u6I+UcWHcbu0TF+Z7JzQn1k=	5CFSE2STPI62ZURHZAL6SERB3QVGOMRZ	c8554266-b401-4a07-8a47-5a919162d79a	\N	f	f	\N	f	0
\.


--
-- Data for Name: AspNetUserClaims; Type: TABLE DATA; Schema: db; Owner: -
--

COPY db."AspNetUserClaims" ("Id", "UserId", "ClaimType", "ClaimValue") FROM stdin;
\.


--
-- Data for Name: AspNetUserLogins; Type: TABLE DATA; Schema: db; Owner: -
--

COPY db."AspNetUserLogins" ("LoginProvider", "ProviderKey", "ProviderDisplayName", "UserId") FROM stdin;
\.


--
-- Data for Name: AspNetUserRoles; Type: TABLE DATA; Schema: db; Owner: -
--

COPY db."AspNetUserRoles" ("UserId", "RoleId") FROM stdin;
admin-user-id-1234-5678-9012-345678901234	admin-role-id
\.


--
-- Data for Name: AspNetUserTokens; Type: TABLE DATA; Schema: db; Owner: -
--

COPY db."AspNetUserTokens" ("UserId", "LoginProvider", "Name", "Value") FROM stdin;
\.


--
-- Data for Name: Clusters; Type: TABLE DATA; Schema: db; Owner: -
--

COPY db."Clusters" ("Id", "ClusterId", "Name", "Description", "Category", "IsRequired", "IsOptional", "MatterVersion", "Attributes", "Commands", "Events", "CreatedAt", "UpdatedAt", "IsDeleted", "CreatedById", "UpdatedById") FROM stdin;
\.


--
-- Data for Name: Rooms; Type: TABLE DATA; Schema: db; Owner: -
--

COPY db."Rooms" ("Id", "Name", "Description", "Floor", "CreatedAt", "UpdatedAt", "IsDeleted", "CreatedById", "UpdatedById") FROM stdin;
40fbaeab-200b-46dc-9098-486b47b1d3e6	Living Room	Main living area	1	2025-07-27 21:05:26.970972+00	2025-07-27 21:05:26.970972+00	f	bb1be326-f26e-4684-bbf5-5c3df450dc61	bb1be326-f26e-4684-bbf5-5c3df450dc61
cbe6c554-1b35-4872-b064-ff7545962a8c	Kitchen	Kitchen area	1	2025-07-27 21:05:26.970972+00	2025-07-27 21:05:26.970972+00	f	bb1be326-f26e-4684-bbf5-5c3df450dc61	bb1be326-f26e-4684-bbf5-5c3df450dc61
c134bccd-0158-4b47-940d-355a5584ffba	Bedroom	Bedroom area	1	2025-07-27 21:05:26.970972+00	2025-07-27 21:05:26.970972+00	f	bb1be326-f26e-4684-bbf5-5c3df450dc61	bb1be326-f26e-4684-bbf5-5c3df450dc61
f524a365-46c8-4599-aba8-c90e2385af93	Bathroom	Bathroom area	1	2025-07-27 21:05:26.970972+00	2025-07-27 21:05:26.970972+00	f	bb1be326-f26e-4684-bbf5-5c3df450dc61	bb1be326-f26e-4684-bbf5-5c3df450dc61
64f82b49-f0d5-4025-8646-2e1a12f8baa2	Office	Office Base Floor	0	2025-08-18 20:13:46.618856+00	2025-08-18 20:13:46.619468+00	f	admin-user-id-1234-5678-9012-345678901234	admin-user-id-1234-5678-9012-345678901234
\.


--
-- Data for Name: DeviceGroups; Type: TABLE DATA; Schema: db; Owner: -
--

COPY db."DeviceGroups" ("Id", "Name", "Description", "Icon", "PreferredCommissioningMethod", "DefaultCapabilities", "SortOrder", "IsActive", "RoomId", "CreatedAt", "UpdatedAt", "IsDeleted", "CreatedById", "UpdatedById") FROM stdin;
ef743eff-8ff7-4243-b016-614c9c95a3c1	Smart Plugs & Sockets	Power control devices for appliances and lighting	oi-device-hdd	BLE_WiFi	\N	0	t	\N	2025-07-27 21:06:29.015946+00	2025-07-27 21:06:29.015946+00	f	bb1be326-f26e-4684-bbf5-5c3df450dc61	bb1be326-f26e-4684-bbf5-5c3df450dc61
c1734b3d-3daa-4327-8fc0-7b4724eb0cbb	Smart Lighting	LED bulbs, strips, and lighting controls	oi-device-hdd	BLE_WiFi	\N	0	t	\N	2025-07-27 21:06:29.015946+00	2025-07-27 21:06:29.015946+00	f	bb1be326-f26e-4684-bbf5-5c3df450dc61	bb1be326-f26e-4684-bbf5-5c3df450dc61
c4af0811-415d-4d69-a90d-b3276c2eaeb9	Smart Sensors	Environmental and security sensors	oi-device-hdd	BLE_WiFi	\N	0	t	\N	2025-07-27 21:06:29.015946+00	2025-07-27 21:06:29.015946+00	f	bb1be326-f26e-4684-bbf5-5c3df450dc61	bb1be326-f26e-4684-bbf5-5c3df450dc61
2ace4b05-3a79-4e93-98ec-998b943da87e	Smart Switches	Wall switches and dimmers	oi-device-hdd	BLE_WiFi	\N	0	t	\N	2025-07-27 21:06:29.015946+00	2025-07-27 21:06:29.015946+00	f	bb1be326-f26e-4684-bbf5-5c3df450dc61	bb1be326-f26e-4684-bbf5-5c3df450dc61
75b43d94-7161-48aa-8108-3e9ef13cca26	Other Devices	Other Matter-compatible devices	oi-device-hdd	BLE_WiFi	\N	0	t	\N	2025-07-27 21:06:29.015946+00	2025-07-27 21:06:29.015946+00	f	bb1be326-f26e-4684-bbf5-5c3df450dc61	bb1be326-f26e-4684-bbf5-5c3df450dc61
\.


--
-- Data for Name: DeviceTypes; Type: TABLE DATA; Schema: db; Owner: -
--

COPY db."DeviceTypes" ("Id", "Name", "Description", "Capabilities", "IsSimulated", "Icon", "PreferredCommissioningMethod", "DeviceGroupId", "CreatedAt", "UpdatedAt", "IsDeleted", "CreatedById", "UpdatedById") FROM stdin;
00269a40-4765-4fb8-ab23-95d5b9cd5ae0	Smart Plug	Matter-compatible smart plug with power monitoring	{"on_off": true, "power_monitoring": true}	f	oi-device-hdd	BLE_WiFi	\N	2025-07-27 21:05:26.977434+00	2025-07-27 21:05:26.977434+00	f	bb1be326-f26e-4684-bbf5-5c3df450dc61	bb1be326-f26e-4684-bbf5-5c3df450dc61
e538939c-bd7f-444e-9d12-aebb0652f5e3	Smart Switch	Matter-compatible smart switch	{"on_off": true}	f	oi-device-hdd	BLE_WiFi	\N	2025-07-27 21:05:26.977434+00	2025-07-27 21:05:26.977434+00	f	bb1be326-f26e-4684-bbf5-5c3df450dc61	bb1be326-f26e-4684-bbf5-5c3df450dc61
5be3439a-2231-4b46-8c61-65ab2faddaa3	Smart Bulb	Matter-compatible smart bulb with color control	{"on_off": true, "color_control": true}	f	oi-device-hdd	BLE_WiFi	\N	2025-07-27 21:05:26.977434+00	2025-07-27 21:05:26.977434+00	f	bb1be326-f26e-4684-bbf5-5c3df450dc61	bb1be326-f26e-4684-bbf5-5c3df450dc61
c9235be2-412f-450e-88eb-21eaab660ccf	NOUS A8M Socket	NOUS A8M 16A Smart Socket with Matter support	{"onOff": true, "maxPower": 3680, "powerMonitoring": true, "energyMonitoring": true}	f	oi-device-hdd	BLE_WiFi	\N	2025-07-27 21:05:26.977434+00	2025-07-27 21:05:26.977434+00	f	bb1be326-f26e-4684-bbf5-5c3df450dc61	bb1be326-f26e-4684-bbf5-5c3df450dc61
4ef22bd5-1f58-43d8-8824-559cbba714f2	Temperature Sensor	Matter-compatible temperature sensor	{"humidity": true, "temperature": true}	f	oi-device-hdd	BLE_WiFi	\N	2025-07-27 21:05:26.977434+00	2025-07-27 21:05:26.977434+00	f	bb1be326-f26e-4684-bbf5-5c3df450dc61	bb1be326-f26e-4684-bbf5-5c3df450dc61
af3a50bc-6a08-48aa-9497-ff3aff8c3ce8	Motion Sensor	Matter-compatible motion sensor	{"motion": true, "occupancy": true}	f	oi-device-hdd	BLE_WiFi	\N	2025-07-27 21:05:26.977434+00	2025-07-27 21:05:26.977434+00	f	bb1be326-f26e-4684-bbf5-5c3df450dc61	bb1be326-f26e-4684-bbf5-5c3df450dc61
1e9153e0-3609-479c-a87c-20412f12525a	Smart Plug	Matter-compatible smart plug with power monitoring	{"on_off": true, "power_monitoring": true}	f	oi-device-hdd	BLE_WiFi	\N	2025-07-27 21:06:29.021368+00	2025-07-27 21:06:29.021368+00	f	bb1be326-f26e-4684-bbf5-5c3df450dc61	bb1be326-f26e-4684-bbf5-5c3df450dc61
8f8949d8-5aae-48b0-971f-d8a231e9e95e	Smart Switch	Matter-compatible smart switch	{"on_off": true}	f	oi-device-hdd	BLE_WiFi	\N	2025-07-27 21:06:29.021368+00	2025-07-27 21:06:29.021368+00	f	bb1be326-f26e-4684-bbf5-5c3df450dc61	bb1be326-f26e-4684-bbf5-5c3df450dc61
ec533e31-88bd-4366-9203-b5fb62507c2a	Smart Bulb	Matter-compatible smart bulb with color control	{"on_off": true, "color_control": true}	f	oi-device-hdd	BLE_WiFi	\N	2025-07-27 21:06:29.021368+00	2025-07-27 21:06:29.021368+00	f	bb1be326-f26e-4684-bbf5-5c3df450dc61	bb1be326-f26e-4684-bbf5-5c3df450dc61
2d070c66-0d6c-4100-8ac9-db67797a7a8b	NOUS A8M Socket	NOUS A8M 16A Smart Socket with Matter support	{"onOff": true, "maxPower": 3680, "powerMonitoring": true, "energyMonitoring": true}	f	oi-device-hdd	BLE_WiFi	\N	2025-07-27 21:06:29.021368+00	2025-07-27 21:06:29.021368+00	f	bb1be326-f26e-4684-bbf5-5c3df450dc61	bb1be326-f26e-4684-bbf5-5c3df450dc61
01ccb8d9-29fd-4690-b84f-ab6556e38947	Temperature Sensor	Matter-compatible temperature sensor	{"humidity": true, "temperature": true}	f	oi-device-hdd	BLE_WiFi	\N	2025-07-27 21:06:29.021368+00	2025-07-27 21:06:29.021368+00	f	bb1be326-f26e-4684-bbf5-5c3df450dc61	bb1be326-f26e-4684-bbf5-5c3df450dc61
8964f520-764b-40b1-b34a-d08d7d34d49d	Motion Sensor	Matter-compatible motion sensor	{"motion": true, "occupancy": true}	f	oi-device-hdd	BLE_WiFi	\N	2025-07-27 21:06:29.021368+00	2025-07-27 21:06:29.021368+00	f	bb1be326-f26e-4684-bbf5-5c3df450dc61	bb1be326-f26e-4684-bbf5-5c3df450dc61
\.


--
-- Data for Name: Devices; Type: TABLE DATA; Schema: db; Owner: -
--

COPY db."Devices" ("Id", "Name", "Description", "MatterDeviceId", "DeviceTypeId", "RoomId", "Properties", "Status", "Configuration", "LastStateChange", "IsOnline", "LastSeen", "CreatedAt", "UpdatedAt", "IsDeleted", "CreatedById", "UpdatedById") FROM stdin;
196c7b82-bf71-4e88-b714-0354c2d06c04	Office Socket 1	NOUS A8M Smart Socket - Office Area	4328ED19954E9DC0	c9235be2-412f-450e-88eb-21eaab660ccf	64f82b49-f0d5-4025-8646-2e1a12f8baa2	{"model": "A8M", "clusters": [3, 4, 6, 29], "device_type": 266, "manufacturer": "NOUS", "matter_version": "1.0", "device_revision": 2, "cluster_revision": 1}	online	{}	2025-08-17 20:59:54.301596+00	t	2025-08-17 20:59:54.301596+00	2025-08-17 20:59:54.301596+00	2025-08-19 18:41:59.35248+00	f	bb1be326-f26e-4684-bbf5-5c3df450dc61	bb1be326-f26e-4684-bbf5-5c3df450dc61
083c779a-e717-4361-8183-4146414c2f48	Office Socket 2	NOUS A8M Smart Socket - Office Area	4328ED19954E9DC1	c9235be2-412f-450e-88eb-21eaab660ccf	64f82b49-f0d5-4025-8646-2e1a12f8baa2	{"model": "A8M", "clusters": [3, 4, 6, 29], "device_type": 266, "manufacturer": "NOUS", "matter_version": "1.0", "device_revision": 2, "cluster_revision": 1}	online	{}	2025-08-17 21:01:09.465949+00	t	2025-08-17 21:01:09.465949+00	2025-08-17 21:01:09.465949+00	2025-08-19 19:01:04.364835+00	f	bb1be326-f26e-4684-bbf5-5c3df450dc61	bb1be326-f26e-4684-bbf5-5c3df450dc61
\.


--
-- Data for Name: DeviceDeviceGroup; Type: TABLE DATA; Schema: db; Owner: -
--

COPY db."DeviceDeviceGroup" ("DeviceGroupsId", "DevicesId") FROM stdin;
\.


--
-- Data for Name: DeviceEventLogs; Type: TABLE DATA; Schema: db; Owner: -
--

COPY db."DeviceEventLogs" ("Id", "DeviceId", "Event", "Timestamp", "Description", "EventType", "Severity", "EventData", "OldState", "NewState", "Source", "UserId", "CreatedAt", "UpdatedAt", "IsDeleted", "CreatedById", "UpdatedById") FROM stdin;
\.


--
-- Data for Name: DeviceEvents; Type: TABLE DATA; Schema: db; Owner: -
--

COPY db."DeviceEvents" ("Id", "DeviceId", "EventType", "EventData", "CreatedAt", "UpdatedAt", "IsDeleted", "CreatedById", "UpdatedById") FROM stdin;
\.


--
-- Data for Name: FirmwareUpdates; Type: TABLE DATA; Schema: db; Owner: -
--

COPY db."FirmwareUpdates" ("Id", "Name", "Description", "CurrentVersion", "TargetVersion", "DownloadUrl", "FileName", "FileSize", "Checksum", "Status", "DownloadStartedAt", "DownloadCompletedAt", "InstallationStartedAt", "InstallationCompletedAt", "ErrorMessage", "UpdateMetadata", "IsCompatible", "RequiresConfirmation", "IsConfirmed", "ConfirmedAt", "ConfirmedBy", "CreatedAt", "UpdatedAt", "IsDeleted", "CreatedById", "UpdatedById") FROM stdin;
\.


--
-- Data for Name: DeviceFirmwareUpdates; Type: TABLE DATA; Schema: db; Owner: -
--

COPY db."DeviceFirmwareUpdates" ("Id", "DeviceId", "FirmwareUpdateId", "CurrentVersion", "TargetVersion", "Status", "DownloadStartedAt", "DownloadCompletedAt", "InstallationStartedAt", "InstallationCompletedAt", "TestCompletedAt", "TestPassed", "ErrorMessage", "TestResults", "IsConfirmed", "ConfirmedAt", "ConfirmedBy", "IsRollbackAvailable", "RollbackCompletedAt", "RollbackReason", "UpdateLog", "CreatedAt", "UpdatedAt", "IsDeleted", "CreatedById", "UpdatedById") FROM stdin;
\.


--
-- Data for Name: DeviceHistory; Type: TABLE DATA; Schema: db; Owner: -
--

COPY db."DeviceHistory" ("Id", "DeviceId", "EventType", "OldState", "NewState", "Description", "CreatedAt", "UpdatedAt", "IsDeleted", "CreatedById", "UpdatedById") FROM stdin;
\.


--
-- Data for Name: DevicePropertyChanges; Type: TABLE DATA; Schema: db; Owner: -
--

COPY db."DevicePropertyChanges" ("Id", "DeviceId", "PropertyName", "OldValue", "NewValue", "ChangeType", "Reason", "ChangeTimestamp", "IsConfirmed", "ConfirmedAt", "ConfirmedBy", "CreatedAt", "UpdatedAt", "IsDeleted", "CreatedById", "UpdatedById") FROM stdin;
\.


--
-- Data for Name: DeviceStates; Type: TABLE DATA; Schema: db; Owner: -
--

COPY db."DeviceStates" ("Id", "DeviceId", "StateType", "StateValue", "RecordedAt", "CreatedAt", "UpdatedAt", "IsDeleted", "CreatedById", "UpdatedById") FROM stdin;
\.


--
-- Data for Name: EnvironmentalSettings; Type: TABLE DATA; Schema: db; Owner: -
--

COPY db."EnvironmentalSettings" ("Id", "UserId", "IndoorTemperatureMin", "IndoorTemperatureMax", "OutdoorTemperatureMin", "OutdoorTemperatureMax", "HumidityMin", "HumidityMax", "CO2Max", "VOCMax", "TemperatureWarning", "HumidityWarning", "LastUpdated", "CreatedAt", "UpdatedAt", "IsDeleted", "CreatedById", "UpdatedById") FROM stdin;
\.


--
-- Data for Name: Groups; Type: TABLE DATA; Schema: db; Owner: -
--

COPY db."Groups" ("Id", "Name", "Description", "CreatedAt", "UpdatedAt", "IsDeleted", "CreatedById", "UpdatedById") FROM stdin;
\.


--
-- Data for Name: GroupMembers; Type: TABLE DATA; Schema: db; Owner: -
--

COPY db."GroupMembers" ("GroupId", "DeviceId", "Id", "CreatedAt", "UpdatedAt", "IsDeleted", "CreatedById", "UpdatedById") FROM stdin;
\.


--
-- Data for Name: GroupStateHistory; Type: TABLE DATA; Schema: db; Owner: -
--

COPY db."GroupStateHistory" ("Id", "GroupId", "OldState", "NewState", "Description", "CreatedAt", "UpdatedAt", "IsDeleted", "CreatedById", "UpdatedById") FROM stdin;
\.


--
-- Data for Name: GroupStates; Type: TABLE DATA; Schema: db; Owner: -
--

COPY db."GroupStates" ("GroupId", "State", "LastUpdated", "Id", "CreatedAt", "UpdatedAt", "IsDeleted", "CreatedById", "UpdatedById") FROM stdin;
\.


--
-- Data for Name: Notifications; Type: TABLE DATA; Schema: db; Owner: -
--

COPY db."Notifications" ("Id", "UserId", "Message", "Type", "Severity", "IsRead", "ReadAt", "CreatedAt", "UpdatedAt", "IsDeleted", "CreatedById", "UpdatedById") FROM stdin;
\.


--
-- Data for Name: Rules; Type: TABLE DATA; Schema: db; Owner: -
--

COPY db."Rules" ("Id", "Name", "Description", "Condition", "Action", "IsActive", "CreatedAt", "UpdatedAt", "IsDeleted", "CreatedById", "UpdatedById") FROM stdin;
\.


--
-- Data for Name: RuleActions; Type: TABLE DATA; Schema: db; Owner: -
--

COPY db."RuleActions" ("Id", "RuleId", "Action", "Order", "CreatedAt", "UpdatedAt", "IsDeleted", "CreatedById", "UpdatedById") FROM stdin;
\.


--
-- Data for Name: RuleConditions; Type: TABLE DATA; Schema: db; Owner: -
--

COPY db."RuleConditions" ("Id", "RuleId", "Condition", "Order", "CreatedAt", "UpdatedAt", "IsDeleted", "CreatedById", "UpdatedById") FROM stdin;
\.


--
-- Data for Name: RuleExecutionHistory; Type: TABLE DATA; Schema: db; Owner: -
--

COPY db."RuleExecutionHistory" ("Id", "RuleId", "Success", "Result", "ErrorMessage", "ExecutionTime", "CreatedAt", "UpdatedAt", "IsDeleted", "CreatedById", "UpdatedById") FROM stdin;
\.


--
-- Data for Name: RuleTriggers; Type: TABLE DATA; Schema: db; Owner: -
--

COPY db."RuleTriggers" ("Id", "RuleId", "TriggerType", "Trigger", "IsEnabled", "LastTriggered", "CreatedAt", "UpdatedAt", "IsDeleted", "CreatedById", "UpdatedById") FROM stdin;
\.


--
-- Data for Name: UserDevicePermissions; Type: TABLE DATA; Schema: db; Owner: -
--

COPY db."UserDevicePermissions" ("Id", "UserId", "DeviceId", "PermissionLevel", "CreatedAt", "UpdatedAt", "IsDeleted", "CreatedById", "UpdatedById") FROM stdin;
\.


--
-- Data for Name: UserRoomPermissions; Type: TABLE DATA; Schema: db; Owner: -
--

COPY db."UserRoomPermissions" ("Id", "UserId", "RoomId", "Permission", "CreatedAt", "UpdatedAt", "IsDeleted", "CreatedById", "UpdatedById") FROM stdin;
\.


--
-- Data for Name: UserSettings; Type: TABLE DATA; Schema: db; Owner: -
--

COPY db."UserSettings" ("UserId", "Theme", "Language", "ShowOfflineDevices", "DefaultView", "DashboardLayout", "FavoriteDevices", "QuickActions", "EmailNotifications", "PushNotifications", "NotificationPreferences", "DeviceDisplayPreferences", "LastUsedDevices", "RoomDisplayOrder", "ShowEmptyRooms", "ShowAutomationSuggestions", "AutomationPreferences", "Id", "CreatedAt", "UpdatedAt", "IsDeleted", "CreatedById", "UpdatedById") FROM stdin;
\.


--
-- Data for Name: __EFMigrationsHistory; Type: TABLE DATA; Schema: db; Owner: -
--

COPY db."__EFMigrationsHistory" ("MigrationId", "ProductVersion") FROM stdin;
20250823112915_InitialDbSchemaBaseline	8.0.2
20250823115320_AddFirmwareAndClusterEntities	8.0.2
\.


--
-- Data for Name: ApplicationUsers; Type: TABLE DATA; Schema: public; Owner: -
--

COPY public."ApplicationUsers" ("Id", "UserName", "Email", "FirstName", "LastName", "IsActive", "LastLogin", "CreatedAt", "UpdatedAt", "IsDeleted", "CreatedById", "UpdatedById") FROM stdin;
bb1be326-f26e-4684-bbf5-5c3df450dc61	system	system@msh.local	System	User	t	2025-07-27 21:06:29.003431+00	2025-07-27 21:05:26.961571+00	2025-07-27 21:06:29.003431+00	f	bb1be326-f26e-4684-bbf5-5c3df450dc61	bb1be326-f26e-4684-bbf5-5c3df450dc61
admin-user-id-1234-5678-9012-345678901234	admin	admin@msh.local	Admin	User	t	2025-07-27 21:06:29.008029+00	2025-07-27 21:05:26.968293+00	2025-07-27 21:06:29.008029+00	f	bb1be326-f26e-4684-bbf5-5c3df450dc61	bb1be326-f26e-4684-bbf5-5c3df450dc61
\.


--
-- Data for Name: AspNetRoles; Type: TABLE DATA; Schema: public; Owner: -
--

COPY public."AspNetRoles" ("Id", "Name", "NormalizedName", "ConcurrencyStamp") FROM stdin;
admin-role-id	Admin	ADMIN	admin-role-stamp
standard-role-id	Standard	STANDARD	standard-role-stamp
guest-role-id	Guest	GUEST	guest-role-stamp
\.


--
-- Data for Name: AspNetRoleClaims; Type: TABLE DATA; Schema: public; Owner: -
--

COPY public."AspNetRoleClaims" ("Id", "RoleId", "ClaimType", "ClaimValue") FROM stdin;
\.


--
-- Data for Name: AspNetUsers; Type: TABLE DATA; Schema: public; Owner: -
--

COPY public."AspNetUsers" ("Id", "UserName", "NormalizedUserName", "Email", "NormalizedEmail", "EmailConfirmed", "PasswordHash", "SecurityStamp", "ConcurrencyStamp", "PhoneNumber", "PhoneNumberConfirmed", "TwoFactorEnabled", "LockoutEnd", "LockoutEnabled", "AccessFailedCount") FROM stdin;
admin-user-id-1234-5678-9012-345678901234	admin@msh.local	ADMIN@MSH.LOCAL	admin@msh.local	ADMIN@MSH.LOCAL	t	AQAAAAEAACcQAAAAELbXpFJNk2p+Pj5KqON+u6I+UcWHcbu0TF+Z7JzQn1k=	5CFSE2STPI62ZURHZAL6SERB3QVGOMRZ	c8554266-b401-4a07-8a47-5a919162d79a	\N	f	f	\N	f	0
\.


--
-- Data for Name: AspNetUserClaims; Type: TABLE DATA; Schema: public; Owner: -
--

COPY public."AspNetUserClaims" ("Id", "UserId", "ClaimType", "ClaimValue") FROM stdin;
\.


--
-- Data for Name: AspNetUserLogins; Type: TABLE DATA; Schema: public; Owner: -
--

COPY public."AspNetUserLogins" ("LoginProvider", "ProviderKey", "ProviderDisplayName", "UserId") FROM stdin;
\.


--
-- Data for Name: AspNetUserRoles; Type: TABLE DATA; Schema: public; Owner: -
--

COPY public."AspNetUserRoles" ("UserId", "RoleId") FROM stdin;
admin-user-id-1234-5678-9012-345678901234	admin-role-id
\.


--
-- Data for Name: AspNetUserTokens; Type: TABLE DATA; Schema: public; Owner: -
--

COPY public."AspNetUserTokens" ("UserId", "LoginProvider", "Name", "Value") FROM stdin;
\.


--
-- Data for Name: Clusters; Type: TABLE DATA; Schema: public; Owner: -
--

COPY public."Clusters" ("Id", "ClusterId", "Name", "Description", "Category", "IsRequired", "IsOptional", "MatterVersion", "Attributes", "Commands", "Events", "CreatedAt", "UpdatedAt", "IsDeleted", "CreatedById", "UpdatedById") FROM stdin;
38793a57-25b4-44cd-816c-fa92bbd11742	3	Identify	Identify cluster for device identification	Basic	t	f	1.0	\N	\N	\N	2025-08-23 10:24:26.334547+00	\N	f	bb1be326-f26e-4684-bbf5-5c3df450dc61	\N
b172f0fb-5f20-4e5e-b63b-4ef32a54beca	4	Groups	Groups cluster for device grouping	Basic	t	f	1.0	\N	\N	\N	2025-08-23 10:24:26.334547+00	\N	f	bb1be326-f26e-4684-bbf5-5c3df450dc61	\N
44dc7bbe-1f6b-4e72-8c96-be348b7b6ebf	6	OnOff	OnOff cluster for device power control	Control	t	f	1.0	\N	\N	\N	2025-08-23 10:24:26.334547+00	\N	f	bb1be326-f26e-4684-bbf5-5c3df450dc61	\N
45d70e98-2bb7-47b3-ba42-7468380a84aa	29	Descriptor	Descriptor cluster for device information	Basic	t	f	1.0	\N	\N	\N	2025-08-23 10:24:26.334547+00	\N	f	bb1be326-f26e-4684-bbf5-5c3df450dc61	\N
89b3dfe6-8f73-4576-ad15-b19fa8975a9f	2820	Electrical Measurement	Electrical measurement cluster for energy monitoring	Measurement	f	t	1.3	\N	\N	\N	2025-08-23 10:24:26.334547+00	\N	f	bb1be326-f26e-4684-bbf5-5c3df450dc61	\N
\.


--
-- Data for Name: Rooms; Type: TABLE DATA; Schema: public; Owner: -
--

COPY public."Rooms" ("Id", "Name", "Description", "Floor", "CreatedAt", "UpdatedAt", "IsDeleted", "CreatedById", "UpdatedById") FROM stdin;
40fbaeab-200b-46dc-9098-486b47b1d3e6	Living Room	Main living area	1	2025-07-27 21:05:26.970972+00	2025-07-27 21:05:26.970972+00	f	bb1be326-f26e-4684-bbf5-5c3df450dc61	bb1be326-f26e-4684-bbf5-5c3df450dc61
cbe6c554-1b35-4872-b064-ff7545962a8c	Kitchen	Kitchen area	1	2025-07-27 21:05:26.970972+00	2025-07-27 21:05:26.970972+00	f	bb1be326-f26e-4684-bbf5-5c3df450dc61	bb1be326-f26e-4684-bbf5-5c3df450dc61
c134bccd-0158-4b47-940d-355a5584ffba	Bedroom	Bedroom area	1	2025-07-27 21:05:26.970972+00	2025-07-27 21:05:26.970972+00	f	bb1be326-f26e-4684-bbf5-5c3df450dc61	bb1be326-f26e-4684-bbf5-5c3df450dc61
f524a365-46c8-4599-aba8-c90e2385af93	Bathroom	Bathroom area	1	2025-07-27 21:05:26.970972+00	2025-07-27 21:05:26.970972+00	f	bb1be326-f26e-4684-bbf5-5c3df450dc61	bb1be326-f26e-4684-bbf5-5c3df450dc61
64f82b49-f0d5-4025-8646-2e1a12f8baa2	Office	Office Base Floor	0	2025-08-18 20:13:46.618856+00	2025-08-18 20:13:46.619468+00	f	admin-user-id-1234-5678-9012-345678901234	admin-user-id-1234-5678-9012-345678901234
\.


--
-- Data for Name: DeviceGroups; Type: TABLE DATA; Schema: public; Owner: -
--

COPY public."DeviceGroups" ("Id", "Name", "Description", "RoomId", "CreatedAt", "UpdatedAt", "IsDeleted", "CreatedById", "UpdatedById", "DefaultCapabilities", "Icon", "PreferredCommissioningMethod", "SortOrder", "IsActive") FROM stdin;
ef743eff-8ff7-4243-b016-614c9c95a3c1	Smart Plugs & Sockets	Power control devices for appliances and lighting	\N	2025-07-27 21:06:29.015946+00	2025-07-27 21:06:29.015946+00	f	bb1be326-f26e-4684-bbf5-5c3df450dc61	bb1be326-f26e-4684-bbf5-5c3df450dc61	\N	oi-device-hdd	BLE_WiFi	0	t
c1734b3d-3daa-4327-8fc0-7b4724eb0cbb	Smart Lighting	LED bulbs, strips, and lighting controls	\N	2025-07-27 21:06:29.015946+00	2025-07-27 21:06:29.015946+00	f	bb1be326-f26e-4684-bbf5-5c3df450dc61	bb1be326-f26e-4684-bbf5-5c3df450dc61	\N	oi-device-hdd	BLE_WiFi	0	t
c4af0811-415d-4d69-a90d-b3276c2eaeb9	Smart Sensors	Environmental and security sensors	\N	2025-07-27 21:06:29.015946+00	2025-07-27 21:06:29.015946+00	f	bb1be326-f26e-4684-bbf5-5c3df450dc61	bb1be326-f26e-4684-bbf5-5c3df450dc61	\N	oi-device-hdd	BLE_WiFi	0	t
2ace4b05-3a79-4e93-98ec-998b943da87e	Smart Switches	Wall switches and dimmers	\N	2025-07-27 21:06:29.015946+00	2025-07-27 21:06:29.015946+00	f	bb1be326-f26e-4684-bbf5-5c3df450dc61	bb1be326-f26e-4684-bbf5-5c3df450dc61	\N	oi-device-hdd	BLE_WiFi	0	t
75b43d94-7161-48aa-8108-3e9ef13cca26	Other Devices	Other Matter-compatible devices	\N	2025-07-27 21:06:29.015946+00	2025-07-27 21:06:29.015946+00	f	bb1be326-f26e-4684-bbf5-5c3df450dc61	bb1be326-f26e-4684-bbf5-5c3df450dc61	\N	oi-device-hdd	BLE_WiFi	0	t
\.


--
-- Data for Name: DeviceTypes; Type: TABLE DATA; Schema: public; Owner: -
--

COPY public."DeviceTypes" ("Id", "Name", "Description", "Capabilities", "IsSimulated", "CreatedAt", "UpdatedAt", "IsDeleted", "CreatedById", "UpdatedById", "DeviceGroupId", "Icon", "PreferredCommissioningMethod") FROM stdin;
00269a40-4765-4fb8-ab23-95d5b9cd5ae0	Smart Plug	Matter-compatible smart plug with power monitoring	{"on_off": true, "power_monitoring": true}	f	2025-07-27 21:05:26.977434+00	2025-07-27 21:05:26.977434+00	f	bb1be326-f26e-4684-bbf5-5c3df450dc61	bb1be326-f26e-4684-bbf5-5c3df450dc61	\N	oi-device-hdd	BLE_WiFi
e538939c-bd7f-444e-9d12-aebb0652f5e3	Smart Switch	Matter-compatible smart switch	{"on_off": true}	f	2025-07-27 21:05:26.977434+00	2025-07-27 21:05:26.977434+00	f	bb1be326-f26e-4684-bbf5-5c3df450dc61	bb1be326-f26e-4684-bbf5-5c3df450dc61	\N	oi-device-hdd	BLE_WiFi
5be3439a-2231-4b46-8c61-65ab2faddaa3	Smart Bulb	Matter-compatible smart bulb with color control	{"on_off": true, "color_control": true}	f	2025-07-27 21:05:26.977434+00	2025-07-27 21:05:26.977434+00	f	bb1be326-f26e-4684-bbf5-5c3df450dc61	bb1be326-f26e-4684-bbf5-5c3df450dc61	\N	oi-device-hdd	BLE_WiFi
c9235be2-412f-450e-88eb-21eaab660ccf	NOUS A8M Socket	NOUS A8M 16A Smart Socket with Matter support	{"onOff": true, "maxPower": 3680, "powerMonitoring": true, "energyMonitoring": true}	f	2025-07-27 21:05:26.977434+00	2025-07-27 21:05:26.977434+00	f	bb1be326-f26e-4684-bbf5-5c3df450dc61	bb1be326-f26e-4684-bbf5-5c3df450dc61	\N	oi-device-hdd	BLE_WiFi
4ef22bd5-1f58-43d8-8824-559cbba714f2	Temperature Sensor	Matter-compatible temperature sensor	{"humidity": true, "temperature": true}	f	2025-07-27 21:05:26.977434+00	2025-07-27 21:05:26.977434+00	f	bb1be326-f26e-4684-bbf5-5c3df450dc61	bb1be326-f26e-4684-bbf5-5c3df450dc61	\N	oi-device-hdd	BLE_WiFi
af3a50bc-6a08-48aa-9497-ff3aff8c3ce8	Motion Sensor	Matter-compatible motion sensor	{"motion": true, "occupancy": true}	f	2025-07-27 21:05:26.977434+00	2025-07-27 21:05:26.977434+00	f	bb1be326-f26e-4684-bbf5-5c3df450dc61	bb1be326-f26e-4684-bbf5-5c3df450dc61	\N	oi-device-hdd	BLE_WiFi
1e9153e0-3609-479c-a87c-20412f12525a	Smart Plug	Matter-compatible smart plug with power monitoring	{"on_off": true, "power_monitoring": true}	f	2025-07-27 21:06:29.021368+00	2025-07-27 21:06:29.021368+00	f	bb1be326-f26e-4684-bbf5-5c3df450dc61	bb1be326-f26e-4684-bbf5-5c3df450dc61	\N	oi-device-hdd	BLE_WiFi
8f8949d8-5aae-48b0-971f-d8a231e9e95e	Smart Switch	Matter-compatible smart switch	{"on_off": true}	f	2025-07-27 21:06:29.021368+00	2025-07-27 21:06:29.021368+00	f	bb1be326-f26e-4684-bbf5-5c3df450dc61	bb1be326-f26e-4684-bbf5-5c3df450dc61	\N	oi-device-hdd	BLE_WiFi
ec533e31-88bd-4366-9203-b5fb62507c2a	Smart Bulb	Matter-compatible smart bulb with color control	{"on_off": true, "color_control": true}	f	2025-07-27 21:06:29.021368+00	2025-07-27 21:06:29.021368+00	f	bb1be326-f26e-4684-bbf5-5c3df450dc61	bb1be326-f26e-4684-bbf5-5c3df450dc61	\N	oi-device-hdd	BLE_WiFi
2d070c66-0d6c-4100-8ac9-db67797a7a8b	NOUS A8M Socket	NOUS A8M 16A Smart Socket with Matter support	{"onOff": true, "maxPower": 3680, "powerMonitoring": true, "energyMonitoring": true}	f	2025-07-27 21:06:29.021368+00	2025-07-27 21:06:29.021368+00	f	bb1be326-f26e-4684-bbf5-5c3df450dc61	bb1be326-f26e-4684-bbf5-5c3df450dc61	\N	oi-device-hdd	BLE_WiFi
01ccb8d9-29fd-4690-b84f-ab6556e38947	Temperature Sensor	Matter-compatible temperature sensor	{"humidity": true, "temperature": true}	f	2025-07-27 21:06:29.021368+00	2025-07-27 21:06:29.021368+00	f	bb1be326-f26e-4684-bbf5-5c3df450dc61	bb1be326-f26e-4684-bbf5-5c3df450dc61	\N	oi-device-hdd	BLE_WiFi
8964f520-764b-40b1-b34a-d08d7d34d49d	Motion Sensor	Matter-compatible motion sensor	{"motion": true, "occupancy": true}	f	2025-07-27 21:06:29.021368+00	2025-07-27 21:06:29.021368+00	f	bb1be326-f26e-4684-bbf5-5c3df450dc61	bb1be326-f26e-4684-bbf5-5c3df450dc61	\N	oi-device-hdd	BLE_WiFi
\.


--
-- Data for Name: Devices; Type: TABLE DATA; Schema: public; Owner: -
--

COPY public."Devices" ("Id", "Name", "Description", "MatterDeviceId", "DeviceTypeId", "RoomId", "Properties", "Status", "Configuration", "LastStateChange", "IsOnline", "LastSeen", "CreatedAt", "UpdatedAt", "IsDeleted", "CreatedById", "UpdatedById") FROM stdin;
196c7b82-bf71-4e88-b714-0354c2d06c04	Office Socket 1	NOUS A8M Smart Socket - Office Area	4328ED19954E9DC0	c9235be2-412f-450e-88eb-21eaab660ccf	64f82b49-f0d5-4025-8646-2e1a12f8baa2	{"model": "A8M", "clusters": [3, 4, 6, 29], "device_type": 266, "manufacturer": "NOUS", "matter_version": "1.0", "device_revision": 2, "cluster_revision": 1}	online	{}	2025-08-17 20:59:54.301596+00	t	2025-08-17 20:59:54.301596+00	2025-08-17 20:59:54.301596+00	2025-08-19 18:41:59.35248+00	f	bb1be326-f26e-4684-bbf5-5c3df450dc61	bb1be326-f26e-4684-bbf5-5c3df450dc61
083c779a-e717-4361-8183-4146414c2f48	Office Socket 2	NOUS A8M Smart Socket - Office Area	4328ED19954E9DC1	c9235be2-412f-450e-88eb-21eaab660ccf	64f82b49-f0d5-4025-8646-2e1a12f8baa2	{"model": "A8M", "clusters": [3, 4, 6, 29], "device_type": 266, "manufacturer": "NOUS", "matter_version": "1.0", "device_revision": 2, "cluster_revision": 1}	online	{}	2025-08-17 21:01:09.465949+00	t	2025-08-17 21:01:09.465949+00	2025-08-17 21:01:09.465949+00	2025-08-19 19:01:04.364835+00	f	bb1be326-f26e-4684-bbf5-5c3df450dc61	bb1be326-f26e-4684-bbf5-5c3df450dc61
\.


--
-- Data for Name: DeviceDeviceGroup; Type: TABLE DATA; Schema: public; Owner: -
--

COPY public."DeviceDeviceGroup" ("DeviceGroupsId", "DevicesId") FROM stdin;
\.


--
-- Data for Name: DeviceEvents; Type: TABLE DATA; Schema: public; Owner: -
--

COPY public."DeviceEvents" ("Id", "DeviceId", "EventType", "EventData", "CreatedAt", "UpdatedAt", "IsDeleted", "CreatedById", "UpdatedById") FROM stdin;
\.


--
-- Data for Name: FirmwareUpdates; Type: TABLE DATA; Schema: public; Owner: -
--

COPY public."FirmwareUpdates" ("Id", "Name", "Description", "CurrentVersion", "TargetVersion", "DownloadUrl", "FileName", "FileSize", "Checksum", "Status", "DownloadStartedAt", "DownloadCompletedAt", "InstallationStartedAt", "InstallationCompletedAt", "ErrorMessage", "UpdateMetadata", "IsCompatible", "RequiresConfirmation", "IsConfirmed", "ConfirmedAt", "ConfirmedBy", "CreatedAt", "UpdatedAt", "IsDeleted", "CreatedById", "UpdatedById") FROM stdin;
\.


--
-- Data for Name: DeviceFirmwareUpdates; Type: TABLE DATA; Schema: public; Owner: -
--

COPY public."DeviceFirmwareUpdates" ("Id", "DeviceId", "FirmwareUpdateId", "CurrentVersion", "TargetVersion", "Status", "DownloadStartedAt", "DownloadCompletedAt", "InstallationStartedAt", "InstallationCompletedAt", "TestCompletedAt", "TestPassed", "ErrorMessage", "TestResults", "IsConfirmed", "ConfirmedAt", "ConfirmedBy", "IsRollbackAvailable", "RollbackCompletedAt", "RollbackReason", "UpdateLog", "CreatedAt", "UpdatedAt", "IsDeleted", "CreatedById", "UpdatedById") FROM stdin;
\.


--
-- Data for Name: DeviceGroupMembers; Type: TABLE DATA; Schema: public; Owner: -
--

COPY public."DeviceGroupMembers" ("DeviceId", "DeviceGroupId", "CreatedAt", "UpdatedAt", "IsDeleted", "CreatedById", "UpdatedById", "Comment") FROM stdin;
\.


--
-- Data for Name: DeviceHistory; Type: TABLE DATA; Schema: public; Owner: -
--

COPY public."DeviceHistory" ("Id", "DeviceId", "EventType", "OldState", "NewState", "Description", "CreatedAt", "UpdatedAt", "IsDeleted", "CreatedById", "UpdatedById") FROM stdin;
\.


--
-- Data for Name: DevicePropertyChanges; Type: TABLE DATA; Schema: public; Owner: -
--

COPY public."DevicePropertyChanges" ("Id", "DeviceId", "PropertyName", "OldValue", "NewValue", "ChangeType", "Reason", "ChangeTimestamp", "IsConfirmed", "ConfirmedAt", "ConfirmedBy", "CreatedAt", "UpdatedAt", "IsDeleted", "CreatedById", "UpdatedById") FROM stdin;
\.


--
-- Data for Name: DeviceStates; Type: TABLE DATA; Schema: public; Owner: -
--

COPY public."DeviceStates" ("Id", "DeviceId", "StateType", "StateValue", "RecordedAt", "CreatedAt", "UpdatedAt", "IsDeleted", "CreatedById", "UpdatedById") FROM stdin;
\.


--
-- Data for Name: EnvironmentalSettings; Type: TABLE DATA; Schema: public; Owner: -
--

COPY public."EnvironmentalSettings" ("Id", "UserId", "IndoorTemperatureMin", "IndoorTemperatureMax", "OutdoorTemperatureMin", "OutdoorTemperatureMax", "HumidityMin", "HumidityMax", "CO2Max", "VOCMax", "TemperatureWarning", "HumidityWarning", "LastUpdated", "CreatedAt", "UpdatedAt", "IsDeleted", "CreatedById", "UpdatedById") FROM stdin;
\.


--
-- Data for Name: Groups; Type: TABLE DATA; Schema: public; Owner: -
--

COPY public."Groups" ("Id", "Name", "Description", "CreatedAt", "UpdatedAt", "IsDeleted", "CreatedById", "UpdatedById") FROM stdin;
\.


--
-- Data for Name: GroupMembers; Type: TABLE DATA; Schema: public; Owner: -
--

COPY public."GroupMembers" ("GroupId", "DeviceId", "GroupId1", "DeviceId1", "Id", "CreatedAt", "UpdatedAt", "IsDeleted", "CreatedById", "UpdatedById") FROM stdin;
\.


--
-- Data for Name: GroupStateHistory; Type: TABLE DATA; Schema: public; Owner: -
--

COPY public."GroupStateHistory" ("Id", "GroupId", "GroupId1", "OldState", "NewState", "Description", "CreatedAt", "UpdatedAt", "IsDeleted", "CreatedById", "UpdatedById") FROM stdin;
\.


--
-- Data for Name: GroupStates; Type: TABLE DATA; Schema: public; Owner: -
--

COPY public."GroupStates" ("GroupId", "State", "LastUpdated", "Id", "CreatedAt", "UpdatedAt", "IsDeleted", "CreatedById", "UpdatedById") FROM stdin;
\.


--
-- Data for Name: Notifications; Type: TABLE DATA; Schema: public; Owner: -
--

COPY public."Notifications" ("Id", "UserId", "Message", "Type", "Severity", "IsRead", "ReadAt", "CreatedAt", "UpdatedAt", "IsDeleted", "CreatedById", "UpdatedById") FROM stdin;
\.


--
-- Data for Name: Rules; Type: TABLE DATA; Schema: public; Owner: -
--

COPY public."Rules" ("Id", "Name", "Description", "Condition", "Action", "IsActive", "CreatedAt", "UpdatedAt", "IsDeleted", "CreatedById", "UpdatedById") FROM stdin;
\.


--
-- Data for Name: RuleActions; Type: TABLE DATA; Schema: public; Owner: -
--

COPY public."RuleActions" ("Id", "RuleId", "RuleId1", "Action", "Order", "CreatedAt", "UpdatedAt", "IsDeleted", "CreatedById", "UpdatedById") FROM stdin;
\.


--
-- Data for Name: RuleConditions; Type: TABLE DATA; Schema: public; Owner: -
--

COPY public."RuleConditions" ("Id", "RuleId", "RuleId1", "Condition", "Order", "CreatedAt", "UpdatedAt", "IsDeleted", "CreatedById", "UpdatedById") FROM stdin;
\.


--
-- Data for Name: RuleExecutionHistory; Type: TABLE DATA; Schema: public; Owner: -
--

COPY public."RuleExecutionHistory" ("Id", "RuleId", "RuleId1", "Success", "Result", "ErrorMessage", "ExecutionTime", "CreatedAt", "UpdatedAt", "IsDeleted", "CreatedById", "UpdatedById") FROM stdin;
\.


--
-- Data for Name: RuleTriggers; Type: TABLE DATA; Schema: public; Owner: -
--

COPY public."RuleTriggers" ("Id", "RuleId", "TriggerType", "Trigger", "IsEnabled", "LastTriggered", "RuleId1", "CreatedAt", "UpdatedAt", "IsDeleted", "CreatedById", "UpdatedById") FROM stdin;
\.


--
-- Data for Name: UserDevicePermissions; Type: TABLE DATA; Schema: public; Owner: -
--

COPY public."UserDevicePermissions" ("Id", "UserId", "DeviceId", "PermissionLevel", "CreatedAt", "UpdatedAt", "IsDeleted", "CreatedById", "UpdatedById") FROM stdin;
\.


--
-- Data for Name: UserRoomPermissions; Type: TABLE DATA; Schema: public; Owner: -
--

COPY public."UserRoomPermissions" ("Id", "UserId", "RoomId", "Permission", "CreatedAt", "UpdatedAt", "IsDeleted", "CreatedById", "UpdatedById") FROM stdin;
\.


--
-- Data for Name: UserSettings; Type: TABLE DATA; Schema: public; Owner: -
--

COPY public."UserSettings" ("UserId", "Theme", "Language", "ShowOfflineDevices", "DefaultView", "DashboardLayout", "FavoriteDevices", "QuickActions", "EmailNotifications", "PushNotifications", "NotificationPreferences", "DeviceDisplayPreferences", "LastUsedDevices", "RoomDisplayOrder", "ShowEmptyRooms", "ShowAutomationSuggestions", "AutomationPreferences", "UserId1", "Id", "CreatedAt", "UpdatedAt", "IsDeleted", "CreatedById", "UpdatedById") FROM stdin;
\.


--
-- Data for Name: __EFMigrationsHistory; Type: TABLE DATA; Schema: public; Owner: -
--

COPY public."__EFMigrationsHistory" ("MigrationId", "ProductVersion") FROM stdin;
\.


--
-- Name: AspNetRoleClaims_Id_seq; Type: SEQUENCE SET; Schema: db; Owner: -
--

SELECT pg_catalog.setval('db."AspNetRoleClaims_Id_seq"', 1, false);


--
-- Name: AspNetUserClaims_Id_seq; Type: SEQUENCE SET; Schema: db; Owner: -
--

SELECT pg_catalog.setval('db."AspNetUserClaims_Id_seq"', 1, false);


--
-- Name: AspNetRoleClaims_Id_seq; Type: SEQUENCE SET; Schema: public; Owner: -
--

SELECT pg_catalog.setval('public."AspNetRoleClaims_Id_seq"', 1, false);


--
-- Name: AspNetUserClaims_Id_seq; Type: SEQUENCE SET; Schema: public; Owner: -
--

SELECT pg_catalog.setval('public."AspNetUserClaims_Id_seq"', 1, false);


--
-- PostgreSQL database dump complete
--

