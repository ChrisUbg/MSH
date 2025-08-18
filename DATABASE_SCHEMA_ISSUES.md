# Database Schema Issues and Resolutions

## 🚨 **Critical Issue: Entity Framework vs Database Schema Mismatch**

### **Problem Summary**
During development, we encountered a critical mismatch between the Entity Framework Core model and the actual PostgreSQL database schema, causing application crashes and data access failures.

### **Root Cause**
The EF Core migration files defined columns that were not properly created in the actual database, leading to runtime errors when the application tried to access non-existent columns.

---

## 📋 **Issue #1: Missing DeviceGroupId Column**

### **Error Message**
```
Npgsql.PostgresException (0x80004005): 42703: column d0.DeviceGroupId does not exist
```

### **Problem Details**
- **Expected**: EF Core model included a `DeviceGroupId` property in the `Device` entity
- **Actual**: Database table `Devices` had no `DeviceGroupId` column
- **Impact**: Application crashed when trying to load devices from database

### **Investigation**
1. **EF Migration Check**: Migration file `20250620142356_FreshStartWithStringUserIds.cs` showed:
   ```csharp
   // Expected DeviceGroupId column definition
   ```

2. **Database Reality Check**: Actual database table structure:
   ```sql
   -- Missing DeviceGroupId column
   ```

3. **Root Cause**: The migration was applied but the column creation failed silently

### **Resolution Applied**
1. **Manual Database Fix**:
   ```sql
   -- Added missing DeviceGroupId column
   ALTER TABLE "Devices" ADD COLUMN "DeviceGroupId" text;
   ```

2. **EF Model Update**: Added proper many-to-many relationship:
   ```csharp
   // In Device.cs
   public ICollection<DeviceGroup> DeviceGroups { get; set; } = new List<DeviceGroup>();
   
   // In DeviceGroup.cs  
   public ICollection<Device> Devices { get; set; } = new List<Device>();
   
   // In ApplicationDbContext.cs
   modelBuilder.Entity<Device>()
       .HasMany(d => d.DeviceGroups)
       .WithMany(dg => dg.Devices)
       .UsingEntity(j => j.ToTable("DeviceDeviceGroup"));
   ```

---

## 📋 **Issue #2: Missing Icon and PreferredCommissioningMethod Columns**

### **Error Message**
```
Npgsql.PostgresException (0x80004005): 42703: column d0.Icon does not exist
```

### **Problem Details**
- **Expected**: `DeviceTypes` table should have `Icon` and `PreferredCommissioningMethod` columns
- **Actual**: These columns were missing from the database
- **Impact**: Device type loading failed, breaking device management functionality

### **Investigation**
1. **Migration File Check**: Migration showed these columns should exist
2. **Database Reality**: Columns were completely missing
3. **Root Cause**: Migration application failed for these specific columns

### **Resolution Applied**
1. **Manual Column Addition**:
   ```sql
   -- Add Icon column
   ALTER TABLE "DeviceTypes" ADD COLUMN "Icon" text;
   
   -- Set default value for Icon
   UPDATE "DeviceTypes" SET "Icon" = 'oi-device-hdd';
   
   -- Add PreferredCommissioningMethod column
   ALTER TABLE "DeviceTypes" ADD COLUMN "PreferredCommissioningMethod" text;
   ```

---

## 🔧 **Prevention Strategies**

### **1. Migration Verification**
Always verify migrations are applied correctly:
```bash
# Check migration status
dotnet ef migrations list --project src/MSH.Infrastructure --startup-project src/MSH.Web

# Verify database schema
dotnet ef database update --verbose --project src/MSH.Infrastructure --startup-project src/MSH.Web
```

### **2. Database Schema Validation**
Create a validation script to check schema consistency:
```sql
-- Check if expected columns exist
SELECT column_name 
FROM information_schema.columns 
WHERE table_name = 'Devices' 
  AND column_name IN ('DeviceGroupId', 'Icon', 'PreferredCommissioningMethod');
```

### **3. EF Model Validation**
Add model validation in startup:
```csharp
// In Program.cs or Startup.cs
using (var scope = app.Services.CreateScope())
{
    var context = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
    context.Database.EnsureCreated();
    
    // Validate critical columns exist
    var deviceColumns = context.Database.SqlQueryRaw<string>(
        "SELECT column_name FROM information_schema.columns WHERE table_name = 'Devices'"
    ).ToList();
    
    if (!deviceColumns.Contains("DeviceGroupId"))
    {
        throw new InvalidOperationException("Critical column DeviceGroupId missing from Devices table");
    }
}
```

---

## 📊 **Current Database Schema Status**

### **✅ Resolved Issues**
- [x] `DeviceGroupId` column added to `Devices` table
- [x] `Icon` column added to `DeviceTypes` table  
- [x] `PreferredCommissioningMethod` column added to `DeviceTypes` table
- [x] Many-to-many relationship properly configured
- [x] Database backup created after fixes

### **🔍 Verification Commands**
```bash
# Check Devices table structure
ssh chregg@192.168.0.107 "docker exec msh_db psql -U postgres -d matter_dev -c \"\\d Devices\""

# Check DeviceTypes table structure  
ssh chregg@192.168.0.107 "docker exec msh_db psql -U postgres -d matter_dev -c \"\\d DeviceTypes\""

# Verify data integrity
ssh chregg@192.168.0.107 "docker exec msh_db psql -U postgres -d matter_dev -c \"SELECT COUNT(*) FROM Devices;\""
```

---

## ⚠️ **Important Notes**

### **Migration Safety**
- **Never delete migration files** that have been applied to production
- **Always backup database** before applying migrations
- **Test migrations** in development environment first
- **Use verbose logging** when applying migrations

### **Future Migration Strategy**
1. **Development Testing**: Test all migrations in development first
2. **Schema Validation**: Add automated schema validation
3. **Rollback Plan**: Always have a rollback strategy
4. **Documentation**: Document all manual fixes applied

### **Backup Strategy**
- **Before Migration**: Always create backup
- **After Fixes**: Create backup after manual fixes
- **Documentation**: Record all manual changes in this document

---

## 📝 **Manual Fixes Applied**

### **Date**: August 18, 2025
### **Environment**: Production (Raspberry Pi)
### **Database**: matter_dev

```sql
-- Fix 1: Add DeviceGroupId column
ALTER TABLE "Devices" ADD COLUMN "DeviceGroupId" text;

-- Fix 2: Add Icon column to DeviceTypes
ALTER TABLE "DeviceTypes" ADD COLUMN "Icon" text;
UPDATE "DeviceTypes" SET "Icon" = 'oi-device-hdd';

-- Fix 3: Add PreferredCommissioningMethod column
ALTER TABLE "DeviceTypes" ADD COLUMN "PreferredCommissioningMethod" text;
```

### **Backup Created**
- **File**: `BackUpFiles/backup_20250818_210251.sql`
- **Size**: ~3.9KB
- **Status**: ✅ Verified and stored

---

*Last Updated: August 18, 2025*
*Status: ✅ RESOLVED - All schema issues fixed and documented*
