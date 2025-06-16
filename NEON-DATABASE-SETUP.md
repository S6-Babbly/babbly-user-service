# Neon Database Setup Guide for Babbly User Service

## Overview

This guide explains how to configure the Babbly User Service to use Neon cloud PostgreSQL databases for both development and production environments.

## Prerequisites

1. **Neon Account**: Create a free account at [neon.tech](https://neon.tech)
2. **Two Database Projects**: Create separate projects for development and production

## 1. Neon Database Setup

### Step 1: Create Development Database
1. Log into your Neon dashboard
2. Create a new project: `babbly-users-dev`
3. Create database: `babbly_users_dev`
4. Note the connection details:
   - Host: `ep-xxx-xxx.us-east-1.aws.neon.tech`
   - Database: `babbly_users_dev`
   - Username: `your_username`
   - Password: `your_password`

### Step 2: Create Production Database
1. Create another project: `babbly-users-prod`
2. Create database: `babbly_users_prod`
3. Note the connection details for production

## 2. Development Environment Configuration

### Option A: Using Environment Variables (Recommended)

1. **Copy environment template:**
   ```bash
   cp env.example .env
   ```

2. **Update `.env` file:**
   ```bash
   # Set environment
   ASPNETCORE_ENVIRONMENT=Development
   
   # Database connection (will override appsettings.Development.json)
   ConnectionStrings__DefaultConnection=Host=ep-xxx-xxx.us-east-1.aws.neon.tech;Database=babbly_users_dev;Username=your_username;Password=your_password;SSL Mode=Require
   
   # Kafka configuration (optional override)
   Kafka__BootstrapServers=localhost:9092
   Kafka__UserTopic=user-events
   
   # CORS configuration (optional override)
   CorsOrigins__0=http://localhost:3000
   ```

### Option B: Update appsettings.Development.json

Update `appsettings.Development.json`:
```bash
{
  "ConnectionStrings": {
    "DefaultConnection": "Host=ep-xxx-xxx.us-east-1.aws.neon.tech;Database=babbly_users_dev;Username=your_username;Password=your_password;SSL Mode=Require"
  }
}
```

## 3. Production Environment Configuration

### Kubernetes Deployment

The production configuration is handled through Kubernetes secrets and environment variables.

1. **Update the Kubernetes secret in `k8s/deployment.yaml`:**
   ```yaml
   apiVersion: v1
   kind: Secret
   metadata:
     name: user-service-secrets
   type: Opaque
   stringData:
     connection-string: "Host=ep-xxx-xxx.us-east-1.aws.neon.tech;Database=babbly_users_prod;Username=your_username;Password=your_password;SSL Mode=Require"
   ```

2. **Update frontend URL in deployment:**
   ```yaml
   - name: CorsOrigins__0
     value: "https://your-actual-frontend-domain.com"
   ```

### Alternative: Update appsettings.Production.json

For non-Kubernetes deployments, update `appsettings.Production.json`:
```json
{
  "ConnectionStrings": {
    "DefaultConnection": "Host=ep-xxx-xxx.us-east-1.aws.neon.tech;Database=babbly_users_prod;Username=your_username;Password=your_password;SSL Mode=Require"
  },
  "CorsOrigins": [
    "https://your-production-frontend.com"
  ]
}
```

## 4. Environment Variable Priority

The application loads configuration in this order (later sources override earlier ones):

1. `appsettings.json` (base configuration)
2. `appsettings.{Environment}.json` (environment-specific)
3. Environment variables (highest priority)

### .NET Configuration Binding Format

Environment variables use double underscores (`__`) to represent nested configuration:

- `ConnectionStrings:DefaultConnection` → `ConnectionStrings__DefaultConnection`
- `Kafka:BootstrapServers` → `Kafka__BootstrapServers`
- `CorsOrigins[0]` → `CorsOrigins__0`

## 5. Database Migration

### Automatic Migration (Development)
The application automatically applies database migrations in development mode.

### Manual Migration (Production)
For production, you can enable automatic migration by setting:
```bash
ENABLE_AUTO_MIGRATION=true
```

Or run migrations manually:
```bash
dotnet ef database update
```

## 6. Verification

### Test Database Connection
1. Start the application
2. Check logs for successful database connection
3. Verify that database tables are created

### Test API Endpoints
```bash
# Health check (if available)
curl http://localhost:8080/health

# Test user endpoints
curl http://localhost:8080/api/users
```

## 7. Security Best Practices

1. **Never commit credentials** to version control
2. **Use Kubernetes secrets** for production
3. **Enable SSL Mode** for Neon connections
4. **Rotate passwords** regularly
5. **Use least privilege** database users

## 8. Troubleshooting

### Common Issues

1. **Connection timeout**
   - Verify Neon project is not suspended
   - Check network connectivity
   - Ensure SSL Mode=Require is set

2. **Authentication failed**
   - Verify username and password
   - Check if user has necessary permissions

3. **Migration errors**
   - Ensure database user has DDL permissions
   - Check for existing conflicting data

### Debug Connection
Add detailed logging to `appsettings.json`:
```json
{
  "Logging": {
    "LogLevel": {
      "Default": "Information",
      "Microsoft.EntityFrameworkCore.Database.Command": "Information"
    }
  }
}
```

## 9. Environment Files

- `.env` - Local development environment variables (not committed)
- `env.example` - Template for environment variables (committed)
- `appsettings.json` - Base configuration
- `appsettings.Development.json` - Development overrides
- `appsettings.Production.json` - Production overrides

## Support

For Neon-specific issues, check:
- [Neon Documentation](https://neon.tech/docs)
- [Neon Discord](https://discord.gg/92vNTzKDGp)

For application issues, check the application logs and ensure proper configuration. 