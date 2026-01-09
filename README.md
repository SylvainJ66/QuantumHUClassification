# QuantumHU Classification

A .NET 10 C# project implementing CQRS with Domain-Driven Design, following hexagonal architecture patterns.

## Project Structure

- **SharedKernel**: Common infrastructure (Result pattern, IDateTimeProvider)
- **QuantumHUContext.WriteSide**: Command handling with EF Core
- **QuantumHUContext.ReadSide**: Query handling with Dapper
- **QuantumHUContext.Api**: API layer with Wolverine
- **QuantumHUClassification.AppHost**: Aspire orchestration

## Technologies

- .NET 10
- Wolverine (CQRS message handling)
- EF Core 10 + PostgreSQL (Write-Side)
- Dapper (Read-Side)
- Aspire (orchestration)

## Getting Started

### Prerequisites

- .NET 10 SDK
- Docker Desktop (for PostgreSQL via Aspire)

### Local Development Setup

#### Option 1: Using Aspire Orchestration (Recommended)

1. Make sure Docker Desktop is running

2. Run the AppHost project (this will start PostgreSQL and the API):
   ```bash
   dotnet run --project QuantumHUClassification.AppHost
   ```

3. Access the Aspire dashboard (URL will be shown in console)

**Note:** When using Aspire orchestration, database credentials are automatically provided by the AppHost. No manual configuration needed!

#### Option 2: Running API Standalone (Without Aspire)

If you need to run the API project directly (for debugging, testing, etc.), you must configure database credentials using User Secrets:

1. Navigate to the API project directory:
   ```bash
   cd QuantumHUContext.Api
   ```

2. Initialize User Secrets (if not already done):
   ```bash
   dotnet user-secrets init
   ```

3. Set your database connection string:
   ```bash
   dotnet user-secrets set "ConnectionStrings:Database" "Host=localhost;Port=15433;Database=quantumhu;Pooling=true;Username=quantumhu_user;Password=your-password"
   ```

4. Verify the secret is set:
   ```bash
   dotnet user-secrets list
   ```

5. Run the API:
   ```bash
   dotnet run
   ```

### Security Notes

**IMPORTANT:**
- NEVER commit database credentials to git
- Use User Secrets for local development
- Use environment variables for production deployments
- The `appsettings.Development.json` file should NOT contain connection strings
- User Secrets are stored at: `%APPDATA%\Microsoft\UserSecrets\<user_secrets_id>\secrets.json`

### Testing the API

Once running, you can test the endpoints:

**Create a greeting:**
```bash
curl -X POST http://localhost:5000/api/quantum-greetings \
  -H "Content-Type: application/json" \
  -d '{"message": "Hello Quantum World!"}'
```

**Get all greetings:**
```bash
curl http://localhost:5000/api/quantum-greetings
```

## Architecture Highlights

### Write-Side (Commands)
- Rich domain models with business logic
- Snapshot pattern for persistence decoupling
- Result pattern for error handling
- Wolverine for command handling

### Read-Side (Queries)
- Raw SQL with Dapper for optimal performance
- Separate read models
- Cursor-based pagination ready

### Patterns Used
- CQRS
- Domain-Driven Design (DDD)
- Hexagonal Architecture (Ports & Adapters)
- Repository Pattern
- Result Pattern
- Snapshot Pattern

## Next Steps

This is a starter template with a "Hello Quantum World" feature. You can now:
1. Implement your real domain models
2. Add business rules and validations
3. Create comprehensive tests
4. Add more commands and queries
