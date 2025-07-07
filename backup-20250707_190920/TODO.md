# Smart Home Implementation TODO List

Allways look at the @basic-rules.md first.

## Phase 1: Core Infrastructure Setup
### Docker Environment
- [x] Create base docker-compose.yml
  - [x] Web service (Blazor Server)
  - [x] Database service (PostgreSQL)
  - [x] Matter bridge service (Python)
  - [x] Network configuration
  - [x] Volume management
- [x] Create docker-compose.dev-msh.yml
- [x] Create docker-compose.prod-msh.yml
- [x] Set up Dockerfile for main application
- [x] Set up Dockerfile for Matter protocol bridge
- [x] Set up Dockerfile.dev for main application
- [x] Set up Dockerfile.dev for Matter protocol bridge
- [x] Configure network settings
  - [x] Create network configuration script
  - [x] Implement mode switching
  - [x] Set up access point configuration
- [x] Create test script for Docker setup
- [x] Test Docker setup locally
- [x] Configure port settings
  - [x] Web service (8082)
  - [x] Database (5434 external, 5432 internal)
  - [x] Matter bridge (8084)
  - [x] Centralize port configuration in appsettings.Ports.json

### Database Setup
- [x] Design initial database schema
- [x] Create PostgreSQL migration scripts
- [x] Set up database backup procedures
- [x] Implement data access layer
- [x] Create database initialization scripts
  - [x] Configure PostgreSQL container
  - [x] Set up volume persistence
  - [x] Configure internal/external ports
- [x] Test database connectivity
  - [x] Verify container-to-container communication
  - [x] Test external access
  - [x] Implement connection health checks

### Network Setup (Hybrid Approach)
- [x] Configure Raspberry Pi WiFi client mode (A1 network)
- [x] Set up Raspberry Pi Access Point mode
- [x] Create mode switching script
- [x] Implement basic UI for mode control                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                       
- [x] Implement robust network test page with HTTP/HTTPS fallback and diagnostics
- [x] Refactor Network Settings UI for HTTP/HTTPS fallback and improved error handling
- [x] Add loading indicators and clear user feedback to network-related pages
- [x] Test network mode switching
- [x] Document network configuration

### Basic Application Structure
- [x] Set up Blazor Server project
- [x] Create basic project structure
  - [x] Application layer
  - [x] Domain layer
  - [x] Infrastructure layer
    - [x] Implement backup service
    - [x] Implement notification service
    - [x] Implement current user service
    - [x] Implement device management service
    - [x] Implement environmental monitoring service
    - [x] Implement rule engine service
    - [ ] Implement user management service
  - [x] Web layer
- [x] Implement basic authentication (Blazor Server authentication and authorization are now working; Logout and user management remain open)
- [x] Set up logging infrastructure
- [x] Create basic UI layout

### Matter Protocol Bridge
- [x] Set up Python environment
  - [x] Create requirements.txt
  - [x] Configure development environment
  - [x] Set up production environment
- [x] Implement basic Matter controller (python-matter-server)
- [x] Create device discovery mechanism (WebSocket-based)
- [x] Implement basic device communication (Real Matter OnOff cluster)
- [x] Set up bridge API endpoints
  - [x] Configure FastAPI server
  - [x] Set up port configuration
  - [x] Test basic API response
- [x] Test Matter protocol integration (python-matter-server operational)
- [x] Implement commissioning process (WebSocket commissioning ready)
- [x] Test production deployment
- [ ] Test mode switching during commissioning

## Phase 2: Basic Device Management
### Device Discovery
- [x] Implement device scanning
- [x] Create device registration process
- [x] Set up device status monitoring
- [x] Implement device type detection
- [x] Create device management UI
- [ ] Add commissioning mode indicators
- [ ] Implement device commissioning workflow

### Device Control
- [x] Implement basic device operations
  - [x] On/Off control
  - [x] Dimming control
  - [x] Temperature control
- [x] Create device control API
- [x] Implement device status updates
- [x] Add device control UI components

### Room and Device Grouping
- [x] Create room management system
  - [x] Design room entity and relationships
  - [x] Create room CRUD operations
  - [x] Implement room assignment UI
  - [x] Add room-based device filtering
- [x] Implement device grouping
  - [x] Create group entity and relationships
  - [x] Implement group CRUD operations
  - [x] Add group management UI
- [x] Implement group state management
- [ ] Add room/group management UI
  - [ ] Create room overview page
  - [ ] Add room device list
  - [ ] Implement room statistics
- [ ] Implement group status monitoring
  - [ ] Add group health checks
  - [ ] Create group status dashboard
  - [ ] Implement group notifications

### User Management
- [ ] Implement user authentication
- [ ] Create role-based access control
- [ ] Add user management UI
- [ ] Implement user preferences
- [ ] Set up user device permissions

## Phase 3: Monitoring & Statistics
### Environmental Monitoring
- [x] Create EnvironmentalSettings entity
- [x] Implement temperature monitoring
- [x] Implement humidity monitoring
- [x] Implement air quality monitoring
- [x] Add threshold configuration
- [x] Create warning system
- [x] Implement trend visualization
- [x] Add data persistence
- [ ] Add data export functionality
- [ ] Implement historical data analysis
- [ ] Add custom alert rules
- [ ] Create environmental reports

### Device Status Tracking
- [ ] Create device status history
- [ ] Implement status change tracking
- [ ] Add device health monitoring
- [ ] Create status reporting system
- [ ] Implement status visualization

### Basic Statistics
- [ ] Create usage tracking system
- [ ] Implement basic analytics
- [ ] Add statistical reporting
- [ ] Create data visualization
- [ ] Set up data export

### Data Visualization
- [ ] Create monitoring dashboards
- [ ] Implement real-time updates
- [ ] Add historical data views
- [ ] Create export functionality
- [ ] Implement data filtering

## Phase 4: Advanced Features
### Rule Engine
- [x] Design rule system
- [x] Implement rule execution engine
- [ ] Create rule management UI
- [ ] Add rule templates
- [ ] Implement rule testing

### Advanced Statistics
- [ ] Implement trend analysis
- [ ] Add predictive analytics
- [ ] Create advanced reporting
- [ ] Implement data mining
- [ ] Add custom analytics

### Mobile Access
- [ ] Design mobile interface
- [ ] Implement mobile authentication
- [ ] Create mobile device control
- [ ] Add push notifications
- [ ] Implement offline support

### Language Support
- [ ] Set up DeepL integration
- [ ] Implement language switching
- [ ] Add translation management
- [ ] Create language preferences
- [ ] Test multi-language support

## Documentation
- [x] Create API documentation
- [ ] Write deployment guide
- [ ] Create user manual
- [ ] Document database schema
- [ ] Create system architecture documentation

## Testing
- [ ] Set up unit testing
- [ ] Implement integration tests
- [ ] Create end-to-end tests
- [ ] Set up performance testing
- [ ] Implement security testing

## Deployment
- [ ] Create deployment scripts
- [ ] Set up CI/CD pipeline
- [x] Implement backup procedures

---

**Note:**
- Circular dependencies in service registration and DbContext usage have been resolved.
- Application now starts end-to-end, loads main/index/counter pages, and environmental monitoring is functional.
- Network settings page is implemented with mode switching functionality.
- Basic UI layout and navigation are working across different browsers.
- Device management system is implemented with simulated devices and standardized UI components.

## Core Infrastructure
- [x] Set up Entity Framework Core with PostgreSQL
- [ ] Implement user authentication and authorization
- [x] Create base entities and DbContext
- [x] Implement soft delete functionality
- [x] Add user tracking for entities
- [x] Set up dependency injection
- [x] Configure logging

## Device Management
- [x] Create Device entity
- [x] Create DeviceGroup entity
- [x] Create DeviceState entity
- [x] Create DeviceEvent entity
- [x] Implement device registration
- [x] Add device state tracking
- [x] Create device event logging
- [ ] Implement device grouping

## Rules and Automation
- [x] Create Rule entity
- [x] Create RuleTrigger entity
- [x] Implement rule evaluation engine
- [ ] Add time-based triggers
- [ ] Add condition-based triggers
- [ ] Implement action execution
- [ ] Add rule scheduling

## User Management
- [x] Create User entity
- [x] Create UserDevicePermission entity
- [ ] Implement user registration
- [ ] Add user authentication
- [ ] Implement permission system
- [x] Create user settings
- [x] Add user preferences

## Notifications
- [x] Implement real-time notifications
- [x] Add email notifications
- [x] Create notification preferences
- [x] Implement notification history
- [ ] Add push notifications
- [ ] Create notification templates
- [ ] Implement notification grouping

## UI/UX
- [x] Create responsive layout
- [x] Implement dark/light theme
- [x] Add device dashboard
- [x] Create settings pages
- [x] Implement environmental monitoring UI
- [ ] Add data visualization
- [ ] Create mobile app
- [ ] Implement progressive web app features

## Testing
- [ ] Write unit tests
- [ ] Add integration tests
- [ ] Implement end-to-end tests
- [ ] Create performance tests
- [ ] Add security tests

## Documentation
- [ ] Create API documentation
- [ ] Write user guide
- [ ] Add developer documentation
- [ ] Create deployment guide
- [ ] Add troubleshooting guide

## Deployment
- [ ] Set up CI/CD pipeline
- [ ] Configure production environment
- [x] Implement backup strategy
- [ ] Add monitoring and alerting
- [ ] Create deployment scripts 