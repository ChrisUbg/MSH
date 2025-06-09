# MSH Database Schema Design

## Core Tables

### Users
```sql
CREATE TABLE users (
    id UUID PRIMARY KEY DEFAULT gen_random_uuid(),
    username VARCHAR(50) UNIQUE NOT NULL,
    email VARCHAR(255) UNIQUE NOT NULL,
    password_hash VARCHAR(255) NOT NULL,
    role VARCHAR(20) NOT NULL DEFAULT 'user',
    created_at TIMESTAMP WITH TIME ZONE DEFAULT CURRENT_TIMESTAMP,
    last_login TIMESTAMP WITH TIME ZONE,
    is_active BOOLEAN DEFAULT true
);
```

### Rooms
```sql
CREATE TABLE rooms (
    id UUID PRIMARY KEY DEFAULT gen_random_uuid(),
    name VARCHAR(100) NOT NULL,
    description TEXT,
    floor INTEGER,
    created_at TIMESTAMP WITH TIME ZONE DEFAULT CURRENT_TIMESTAMP,
    updated_at TIMESTAMP WITH TIME ZONE DEFAULT CURRENT_TIMESTAMP,
    is_deleted BOOLEAN DEFAULT FALSE,
    created_by_id UUID NOT NULL REFERENCES users(id),
    updated_by_id UUID REFERENCES users(id)
);
```

### Device Types
```sql
CREATE TABLE device_types (
    id UUID PRIMARY KEY DEFAULT gen_random_uuid(),
    name VARCHAR(100) NOT NULL,
    description TEXT,
    capabilities JSONB NOT NULL, -- Stores supported features/commands
    created_at TIMESTAMP WITH TIME ZONE DEFAULT CURRENT_TIMESTAMP,
    updated_at TIMESTAMP WITH TIME ZONE DEFAULT CURRENT_TIMESTAMP,
    is_deleted BOOLEAN DEFAULT FALSE,
    created_by_id UUID NOT NULL REFERENCES users(id),
    updated_by_id UUID REFERENCES users(id)
);
```

### Devices
```sql
CREATE TABLE devices (
    id UUID PRIMARY KEY DEFAULT gen_random_uuid(),
    name VARCHAR(100) NOT NULL,
    device_type_id UUID REFERENCES device_types(id),
    room_id UUID REFERENCES rooms(id),
    matter_device_id VARCHAR(255) UNIQUE, -- Matter protocol device identifier
    status VARCHAR(20) DEFAULT 'offline',
    last_seen TIMESTAMP WITH TIME ZONE,
    configuration JSONB, -- Device-specific configuration
    created_at TIMESTAMP WITH TIME ZONE DEFAULT CURRENT_TIMESTAMP,
    updated_at TIMESTAMP WITH TIME ZONE DEFAULT CURRENT_TIMESTAMP,
    is_deleted BOOLEAN DEFAULT FALSE,
    created_by_id UUID NOT NULL REFERENCES users(id),
    updated_by_id UUID REFERENCES users(id)
);
```

### Device Groups
```sql
CREATE TABLE device_groups (
    id UUID PRIMARY KEY DEFAULT gen_random_uuid(),
    name VARCHAR(100) NOT NULL,
    description TEXT,
    room_id UUID REFERENCES rooms(id),
    created_at TIMESTAMP WITH TIME ZONE DEFAULT CURRENT_TIMESTAMP,
    updated_at TIMESTAMP WITH TIME ZONE DEFAULT CURRENT_TIMESTAMP,
    is_deleted BOOLEAN DEFAULT FALSE,
    created_by_id UUID NOT NULL REFERENCES users(id),
    updated_by_id UUID REFERENCES users(id)
);
```

### Device Group Members
```sql
CREATE TABLE device_group_members (
    id UUID PRIMARY KEY DEFAULT gen_random_uuid(),
    device_id UUID REFERENCES devices(id),
    group_id UUID REFERENCES device_groups(id),
    created_at TIMESTAMP WITH TIME ZONE DEFAULT CURRENT_TIMESTAMP,
    updated_at TIMESTAMP WITH TIME ZONE DEFAULT CURRENT_TIMESTAMP,
    is_deleted BOOLEAN DEFAULT FALSE,
    created_by_id UUID NOT NULL REFERENCES users(id),
    updated_by_id UUID REFERENCES users(id)
);
```

## Monitoring & Statistics

### Device States
```sql
CREATE TABLE device_states (
    id SERIAL PRIMARY KEY,
    device_id INTEGER REFERENCES devices(id),
    state_type VARCHAR(50) NOT NULL, -- e.g., 'power', 'temperature', 'humidity'
    state_value JSONB NOT NULL,
    recorded_at TIMESTAMP WITH TIME ZONE DEFAULT CURRENT_TIMESTAMP
);
```

### Device Events
```sql
CREATE TABLE device_events (
    id SERIAL PRIMARY KEY,
    device_id INTEGER REFERENCES devices(id),
    event_type VARCHAR(50) NOT NULL,
    event_data JSONB,
    created_at TIMESTAMP WITH TIME ZONE DEFAULT CURRENT_TIMESTAMP
);
```

## Rules & Automation

### Rules
```sql
CREATE TABLE rules (
    id SERIAL PRIMARY KEY,
    name VARCHAR(100) NOT NULL,
    description TEXT,
    condition JSONB NOT NULL, -- Rule trigger conditions
    action JSONB NOT NULL, -- Actions to take when triggered
    is_active BOOLEAN DEFAULT true,
    created_at TIMESTAMP WITH TIME ZONE DEFAULT CURRENT_TIMESTAMP,
    updated_at TIMESTAMP WITH TIME ZONE DEFAULT CURRENT_TIMESTAMP
);
```

### Rule Triggers
```sql
CREATE TABLE rule_triggers (
    id SERIAL PRIMARY KEY,
    rule_id INTEGER REFERENCES rules(id),
    trigger_type VARCHAR(50) NOT NULL, -- e.g., 'device_state', 'time', 'event'
    trigger_config JSONB NOT NULL,
    created_at TIMESTAMP WITH TIME ZONE DEFAULT CURRENT_TIMESTAMP
);
```

## Security & Access Control

### User Device Permissions
```sql
CREATE TABLE user_device_permissions (
    user_id INTEGER REFERENCES users(id),
    device_id INTEGER REFERENCES devices(id),
    permission_level VARCHAR(20) NOT NULL, -- e.g., 'read', 'write', 'admin'
    PRIMARY KEY (user_id, device_id)
);
```

### User Room Permissions
```sql
CREATE TABLE user_room_permissions (
    user_id INTEGER REFERENCES users(id),
    room_id INTEGER REFERENCES rooms(id),
    permission_level VARCHAR(20) NOT NULL,
    PRIMARY KEY (user_id, room_id)
);
```

## Indexes

```sql
-- Device States Indexes
CREATE INDEX idx_device_states_device_id ON device_states(device_id);
CREATE INDEX idx_device_states_recorded_at ON device_states(recorded_at);

-- Device Events Indexes
CREATE INDEX idx_device_events_device_id ON device_events(device_id);
CREATE INDEX idx_device_events_created_at ON device_events(created_at);

-- Rules Indexes
CREATE INDEX idx_rules_is_active ON rules(is_active);

-- Device Search Indexes
CREATE INDEX idx_devices_matter_device_id ON devices(matter_device_id);
CREATE INDEX idx_devices_status ON devices(status);
```

## Notes

1. **JSONB Usage**: 
   - Used for flexible device configurations
   - Stores device capabilities
   - Handles rule conditions and actions
   - Stores state values and event data

2. **Timestamps**:
   - All tables include created_at
   - Relevant tables include updated_at
   - Device states and events use recorded_at/created_at

3. **Relationships**:
   - Devices belong to rooms
   - Devices can be in multiple groups
   - Users have permissions on devices and rooms
   - Rules can trigger based on device states/events

4. **Security**:
   - Password hashing for users
   - Role-based access control
   - Granular permissions for devices and rooms

5. **Monitoring**:
   - Device states for current values
   - Device events for history
   - Both support various data types via JSONB

6. **Automation**:
   - Flexible rule system
   - Support for various trigger types
   - JSONB for complex conditions and actions 