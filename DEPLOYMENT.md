# Deployment Guide

This document outlines how to configure database credentials for different environments.

## Environment Configuration Strategy

| Environment | Method | Location |
|------------|--------|----------|
| Local Development (Aspire) | AppHost Configuration | Automatically managed by Aspire |
| Local Development (Standalone) | User Secrets | `%APPDATA%\Microsoft\UserSecrets\` |
| CI/CD | Environment Variables | CI/CD platform settings |
| Production | Environment Variables | Container/hosting platform |

## Development Environment

### With Aspire (Recommended)

The AppHost automatically configures PostgreSQL and provides connection strings. No manual setup required.

```bash
dotnet run --project QuantumHUClassification.AppHost
```

### Without Aspire (Standalone API)

Use .NET User Secrets:

```bash
cd QuantumHUContext.Api
dotnet user-secrets set "ConnectionStrings:Database" "Host=localhost;Port=15433;Database=quantumhu;Pooling=true;Username=quantumhu_user;Password=your-password"
```

## Production Deployments

### Environment Variables

For production, set environment variables using your hosting platform's configuration:

**Connection String Format:**
```
ConnectionStrings__Database=Host=prod-db-host;Port=5432;Database=quantumhu;Username=prod_user;Password=secure-password;SSL Mode=Require
```

Note: ASP.NET Core uses double underscores `__` in environment variable names to represent nested configuration keys.

### Platform-Specific Examples

#### Azure App Service

1. Navigate to: Configuration > Application settings
2. Add new setting:
   - Name: `ConnectionStrings__Database`
   - Value: `Host=your-azure-postgres.postgres.database.azure.com;Port=5432;Database=quantumhu;Username=admin@servername;Password=SecurePassword;SSL Mode=Require`

#### Docker / Kubernetes

**docker-compose.yml:**
```yaml
version: '3.8'
services:
  api:
    image: quantumhu-api:latest
    environment:
      - ConnectionStrings__Database=Host=postgres;Port=5432;Database=quantumhu;Username=apiuser;Password=${DB_PASSWORD}
    env_file:
      - .env.production
```

**.env.production (NOT committed to git):**
```
DB_PASSWORD=your-secure-production-password
```

**Kubernetes Secret:**
```bash
kubectl create secret generic quantumhu-db-secret \
  --from-literal=connection-string="Host=postgres-service;Port=5432;Database=quantumhu;Username=apiuser;Password=SecurePassword"
```

**deployment.yaml:**
```yaml
apiVersion: apps/v1
kind: Deployment
metadata:
  name: quantumhu-api
spec:
  template:
    spec:
      containers:
      - name: api
        env:
        - name: ConnectionStrings__Database
          valueFrom:
            secretKeyRef:
              name: quantumhu-db-secret
              key: connection-string
```

#### AWS ECS / Elastic Beanstalk

Store connection string in AWS Secrets Manager or Systems Manager Parameter Store, then reference in task definition.

## CI/CD Configuration

### GitHub Actions Example

**.github/workflows/deploy.yml:**
```yaml
name: Deploy API

on:
  push:
    branches: [main]

jobs:
  deploy:
    runs-on: ubuntu-latest
    steps:
      - uses: actions/checkout@v3

      - name: Setup .NET
        uses: actions/setup-dotnet@v3
        with:
          dotnet-version: '10.0.x'

      - name: Build
        run: dotnet build --configuration Release

      - name: Test
        run: dotnet test
        env:
          ConnectionStrings__Database: ${{ secrets.TEST_DB_CONNECTION }}

      - name: Deploy
        run: # your deployment script
        env:
          ConnectionStrings__Database: ${{ secrets.PROD_DB_CONNECTION }}
```

**GitHub Repository Secrets:**
- Navigate to: Settings > Secrets and variables > Actions
- Add secrets:
  - `TEST_DB_CONNECTION`: Test database connection string
  - `PROD_DB_CONNECTION`: Production database connection string

## Security Best Practices

1. **Never commit credentials to source control**
   - Use `.gitignore` to exclude sensitive files
   - Review files before committing (`git diff --staged`)

2. **Use different credentials per environment**
   - Development: Low-privilege local account
   - Staging: Separate credentials from production
   - Production: High-security credentials with minimal privileges

3. **Rotate credentials regularly**
   - Update production credentials quarterly
   - Use managed identities when possible (Azure AD, AWS IAM)

4. **Use SSL/TLS for database connections**
   - Production connection strings should include `SSL Mode=Require`
   - Validate certificates in production

5. **Principle of least privilege**
   - API users should only have necessary permissions
   - Read-only queries should use read-only database users

## Troubleshooting

### Connection String Not Found

**Error:**
```
InvalidOperationException: Database connection string not found. For local development, configure User Secrets...
```

**Solutions:**
- For local development: Configure User Secrets (see Development section)
- For production: Set environment variable `ConnectionStrings__Database`
- For Aspire: Ensure running via AppHost

### User Secrets Not Loading

**Issue:** User Secrets configured but not being read

**Check:**
1. Verify `<UserSecretsId>` exists in `.csproj`
2. Run `dotnet user-secrets list` to confirm secrets are set
3. Ensure running in Development environment (`ASPNETCORE_ENVIRONMENT=Development`)

### Environment Variables in Docker

**Issue:** Environment variables not being read in containerized application

**Solution:**
Ensure environment variables are passed to container:
```bash
docker run -e ConnectionStrings__Database="your-connection-string" quantumhu-api:latest
```
