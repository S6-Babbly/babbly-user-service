# Database Migrations Guide

This guide explains how to use Entity Framework Core migrations to manage the database schema for the Babbly User Service.

## Prerequisites

- .NET SDK (8.0 or later)
- Entity Framework Core CLI tools

## Installing EF Core Tools

If you haven't installed the EF Core CLI tools globally, you can do so with:

```
dotnet tool install --global dotnet-ef
```

Or update them if they're already installed:

```
dotnet tool update --global dotnet-ef
```

## Creating a New Migration

### Using Scripts

We've provided scripts to simplify migration creation:

**Windows:**

```powershell
.\scripts\add-migration.ps1 "MigrationName"
```

**Linux/macOS:**

```bash
./scripts/add-migration.sh "MigrationName"
```

### Manual Command

Alternatively, you can run the EF Core command directly:

```bash
cd babbly-user-service
dotnet ef migrations add "MigrationName" --output-dir "Migrations"
```

## Applying Migrations

Migrations are automatically applied when the application starts in Development mode. In Production, migrations should be applied through a controlled process before deployment.

### Development Environment

In development, migrations are automatically applied through the Docker environment.

### Production Environment

For production Kubernetes environment, use the following steps:

1. Create a Kubernetes job to apply migrations before deploying the service
2. Ensure the database connection string is correctly configured in Kubernetes secrets

## Connection Strings

### Development

Connection strings for development are configured in `appsettings.Development.json` and can be overridden using environment variables.

### Production

In production, the connection string should be stored as a Kubernetes secret and injected as an environment variable.

Example Kubernetes secret:

```yaml
apiVersion: v1
kind: Secret
metadata:
  name: db-secrets
type: Opaque
data:
  user-service-connection-string: <base64-encoded-connection-string>
```

## Database Management Best Practices

1. **Always test migrations on a development environment first**
2. **Backup production database before applying migrations**
3. **Use meaningful migration names that describe the changes**
4. **For complex migrations, consider using SQL scripts for more control**

## Troubleshooting

- If you encounter conflicts between migrations, you may need to revert to a previous migration:

  ```
  dotnet ef database update <PreviousMigrationName>
  ```

- To remove the last migration (if it hasn't been applied):

  ```
  dotnet ef migrations remove
  ```

- To generate a SQL script from migrations:
  ```
  dotnet ef migrations script
  ```
