# MSH mDNS Configuration

This document explains how to set up and use mDNS (Multicast DNS) for the MSH Smart Home application, allowing access via a consistent hostname regardless of IP address changes.

## Overview

mDNS allows you to access your MSH application using the hostname `msh.local` instead of remembering the Raspberry Pi's IP address. This is particularly useful when the Pi's IP address changes frequently due to DHCP.

## Files Created

- `msh-avahi.service` - Avahi service configuration file
- `setup-mdns.sh` - Script to set up mDNS on the Raspberry Pi
- `test-mdns.sh` - Script to test mDNS functionality
- `MDNS_SETUP.md` - This documentation file

## Automatic Setup

The mDNS configuration is automatically set up when you run the deployment script:

```bash
./deploy-to-pi.sh
```

The deployment script will:
1. Deploy your MSH application to the Pi
2. Run the mDNS setup script automatically
3. Configure Avahi daemon with the MSH service

## Manual Setup

If you need to set up mDNS manually on the Raspberry Pi:

1. **Copy the setup script to the Pi:**
   ```bash
   scp setup-mdns.sh chregg@[pi-ip]:~/MSH/
   ```

2. **Run the setup script on the Pi:**
   ```bash
   ssh chregg@[pi-ip]
   cd ~/MSH
   sudo ./setup-mdns.sh
   ```

## Accessing Your Application

After setup, you can access your MSH application at:

- **Primary URL:** `http://msh.local:8083`
- **Fallback URL:** `http://[pi-ip]:8083`

## Testing mDNS

### On the Raspberry Pi
```bash
# Check if Avahi daemon is running
sudo systemctl status avahi-daemon

# Browse available services
avahi-browse -at

# Check if MSH service is registered
avahi-browse -at | grep MSH
```

### From Your Development Machine
```bash
# Test hostname resolution
ping msh.local

# Test web access
curl http://msh.local:8083

# Run the test script
./test-mdns.sh
```

## Troubleshooting

### msh.local Not Resolving

1. **Check Avahi daemon status:**
   ```bash
   ssh chregg@[pi-ip] "sudo systemctl status avahi-daemon"
   ```

2. **Restart Avahi daemon:**
   ```bash
   ssh chregg@[pi-ip] "sudo systemctl restart avahi-daemon"
   ```

3. **Check service file:**
   ```bash
   ssh chregg@[pi-ip] "cat /etc/avahi/services/msh.service"
   ```

### Application Not Accessible

1. **Check if containers are running:**
   ```bash
   ssh chregg@[pi-ip] "cd ~/MSH && docker-compose -f docker-compose.prod-msh.yml ps"
   ```

2. **Check container logs:**
   ```bash
   ssh chregg@[pi-ip] "cd ~/MSH && docker-compose -f docker-compose.prod-msh.yml logs web"
   ```

3. **Test direct IP access:**
   ```bash
   curl http://[pi-ip]:8083
   ```

### Firewall Issues

If you have a firewall on the Pi, ensure port 8083 is open:

```bash
ssh chregg@[pi-ip] "sudo ufw allow 8083"
```

## Service Configuration Details

The Avahi service configuration (`/etc/avahi/services/msh.service`) contains:

```xml
<?xml version="1.0" encoding="UTF-8"?>
<!DOCTYPE service-group SYSTEM "avahi-service.dtd">
<service-group>
  <name>MSH</name>
  <service>
    <name>MSH Smart Home Web App</name>
    <type>_http._tcp</type>
    <port>8083</port>
    <txt-record>description=MSH Smart Home Blazor Application</txt-record>
    <txt-record>version=1.0</txt-record>
  </service>
</service-group>
```

## Benefits

1. **Consistent Access:** Use `http://msh.local:8083` regardless of IP changes
2. **No Router Configuration:** Works without router admin access
3. **Automatic Discovery:** Works on most modern operating systems
4. **Network Independence:** Works within your local network
5. **Easy to Remember:** Simple hostname instead of IP address

## Operating System Support

- **macOS:** Built-in mDNS support (Bonjour)
- **Linux:** Built-in mDNS support (Avahi)
- **Windows 10/11:** Built-in mDNS support
- **Windows 7/8:** May require Bonjour installation

## Security Considerations

- mDNS only works within your local network
- No external internet access required
- Service is only advertised when the application is running
- Uses standard HTTP, not HTTPS (for local network use)

## Maintenance

The mDNS configuration is persistent and will survive:
- Pi reboots
- Network reconnections
- IP address changes
- Application restarts

To update the configuration, simply re-run the deployment script or the setup script manually. 