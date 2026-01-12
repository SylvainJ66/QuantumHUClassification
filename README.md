# QuantumHU Classification

A .NET 10 C# project implementing CQRS with Domain-Driven Design, following hexagonal architecture patterns.

## What is HU (Hounsfield Units) ?

HU is a standardized scale that measures the **radiological density** of tissues in a CT scan. 
It is the unit of measurement for pixels in a CT image.

## What does this project do ?

Use quantum amplitude encoding to classify tissue types based on HU values (bones, soft tissues, air, etc)

## Project Structure

- **SharedKernel**: Common infrastructure (Result pattern, IDateTimeProvider)
- **ExtractHUContext.WriteSide**: Context to extract HU from study.
- **QuantumHUContext.Api**: API for all contexts.
- **QuantumHUClassification.AppHost**: Aspire orchestration
- Wip: Q#

## Technologies

- .NET 10
- Wolverine (mediator + message handling)
- EF Core 10 + PostgreSQL (Write-Side)
- Dapper (Read-Side)
- Aspire (orchestration)
- Q#

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

