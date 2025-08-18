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
-- Data for Name: ApplicationUsers; Type: TABLE DATA; Schema: public; Owner: postgres
--

COPY public."ApplicationUsers" ("Id", "UserName", "Email", "FirstName", "LastName", "IsActive", "LastLogin", "CreatedAt", "UpdatedAt", "IsDeleted", "CreatedById", "UpdatedById") FROM stdin;
bb1be326-f26e-4684-bbf5-5c3df450dc61	system	system@msh.local	System	User	t	2025-07-21 18:02:54.88176+00	2025-07-21 18:02:54.88176+00	2025-07-21 18:02:54.88176+00	f	bb1be326-f26e-4684-bbf5-5c3df450dc61	bb1be326-f26e-4684-bbf5-5c3df450dc61
admin	admin	admin@msh.local	Admin	User	t	2025-07-21 18:02:54.88176+00	2025-07-21 14:51:40.032646+00	2025-07-21 18:02:54.88176+00	f	bb1be326-f26e-4684-bbf5-5c3df450dc61	bb1be326-f26e-4684-bbf5-5c3df450dc61
\.


--
-- Data for Name: Rooms; Type: TABLE DATA; Schema: public; Owner: postgres
--

COPY public."Rooms" ("Id", "Name", "Description", "Floor", "CreatedAt", "UpdatedAt", "IsDeleted", "CreatedById", "UpdatedById") FROM stdin;
c2cefa3c-1ad3-414b-83b0-ce3d69bad5cc	Living Room	Main living area	1	2025-07-21 14:52:16.497427+00	\N	f	admin	\N
d06e6011-0495-4ab9-a8fc-d85bf0d1158b	Kitchen	Kitchen area	1	2025-07-21 14:52:24.348979+00	\N	f	admin	\N
\.


--
-- Data for Name: DeviceGroups; Type: TABLE DATA; Schema: public; Owner: postgres
--

COPY public."DeviceGroups" ("Id", "Name", "Description", "RoomId", "CreatedAt", "UpdatedAt", "IsDeleted", "CreatedById", "UpdatedById") FROM stdin;
8abb9475-876a-47a8-b8d0-e1c9b4b3a17f	Sockets	All Sockets	\N	2025-07-27 12:41:35.484024+00	2025-07-27 12:41:35.484707+00	f	bb1be326-f26e-4684-bbf5-5c3df450dc61	bb1be326-f26e-4684-bbf5-5c3df450dc61
\.


--
-- Data for Name: DeviceTypes; Type: TABLE DATA; Schema: public; Owner: postgres
--

COPY public."DeviceTypes" ("Id", "Name", "Description", "Capabilities", "IsSimulated", "CreatedAt", "UpdatedAt", "IsDeleted", "CreatedById", "UpdatedById") FROM stdin;
757ea92b-e4bf-4b2d-962b-834f951130b0	Smart Plug	Matter-compatible smart plug with power monitoring	{"on_off": true, "power_monitoring": true}	f	2025-07-21 14:51:49.783331+00	\N	f	admin	\N
985d77c4-e235-460f-8720-f1b213a10525	Smart Switch	Matter-compatible smart switch	{"on_off": true}	f	2025-07-21 14:52:00.382702+00	\N	f	admin	\N
9148cefc-6311-4086-9cec-33748f7a0273	Smart Bulb	Matter-compatible smart bulb with color control	{"on_off": true, "color_control": true}	f	2025-07-21 14:52:07.865851+00	\N	f	admin	\N
50d09f98-a28e-4950-813f-59aa5ffe3b93	NOUS A8M Socket	NOUS A8M 16A Smart Socket with Matter support	{"onOff": true, "maxPower": 3680, "powerMonitoring": true, "energyMonitoring": true}	f	2025-07-21 18:03:08.937758+00	2025-07-21 18:03:08.937773+00	f	bb1be326-f26e-4684-bbf5-5c3df450dc61	bb1be326-f26e-4684-bbf5-5c3df450dc61
\.


--
-- Data for Name: Devices; Type: TABLE DATA; Schema: public; Owner: postgres
--

COPY public."Devices" ("Id", "Name", "Description", "MatterDeviceId", "DeviceTypeId", "RoomId", "Properties", "Status", "Configuration", "LastStateChange", "IsOnline", "LastSeen", "CreatedAt", "UpdatedAt", "IsDeleted", "CreatedById", "UpdatedById") FROM stdin;
62987259-f757-43dc-8cef-0dc3ba3f1cbc	Matter Device 2697944665914280097	\N	2697944665914280097	757ea92b-e4bf-4b2d-962b-834f951130b0	c2cefa3c-1ad3-414b-83b0-ce3d69bad5cc	{"vendor": "Test"}	Online	\N	2025-07-21 14:13:03.104408+00	t	2025-07-21 14:13:03.104408+00	2025-07-27 14:37:40.188359+00	2025-07-27 12:40:58.702917+00	f	bb1be326-f26e-4684-bbf5-5c3df450dc61	bb1be326-f26e-4684-bbf5-5c3df450dc61
\.


--
-- PostgreSQL database dump complete
--

