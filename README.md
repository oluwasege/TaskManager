# Task Management API

A microservices-based Task Management API built with .NET Core, implementing CQRS pattern, state management, caching, and asynchronous messaging.

## Features

- RESTful API for CRUD operations on tasks
- CQRS pattern separating command and query responsibilities
- Task state management using a finite state machine
- Caching with Redis for improved performance
- Asynchronous task processing with RabbitMQ
- Comprehensive logging with Serilog
- API documentation with Swagger
- Unit and integration tests

## Technology Stack

- .NET 8.0
- Entity Framework Core
- SQL Server
- Redis
- RabbitMQ
- Serilog
- Swagger / OpenAPI
- xUnit for testing

## Project Structure

The solution follows a clean architecture approach:

- **TaskManager.API**: Controllers and API configuration
- **TaskManager.Application**: CQRS commands, queries, and handlers
- **TaskManager.Domain**: Domain models and business logic
- **TaskManager.Infrastructure**: Database, messaging, and caching implementations
- **TaskManager.Tests**: Unit and integration tests

## Design Patterns Used

1. **CQRS (Command Query Responsibility Segregation)**:
   - Commands: CreateTask, UpdateTask, UpdateTaskStatus, DeleteTask
   - Queries: GetTaskById, GetAllTasks, GetTasksByStatus
   - Separate handlers for each command and query

2. **Repository Pattern**:
   - ITaskRepository provides an abstraction layer for data access
   - TaskRepository implements database operations

3. **Finite State Machine**:
   - TaskStateMachine defines allowed transitions between task statuses
   - Ensures business rules for status changes are enforced

4. **Decorator Pattern**:
   - Redis cache service acts as a decorator for repository methods
   - Improves performance by caching frequently accessed data

5. **Publisher-Subscriber Pattern**:
   - RabbitMQ message queue enables asynchronous processing
   - Tasks creation events are published and consumed independently


## Setup Instructions

### Prerequisites

- .NET 7.0 SDK
- SQL Server (or SQL Server Express)
- Redis (for caching)
- RabbitMQ (for messaging)

### Database Setup

The application uses Entity Framework Core with code-first migrations. The database will be automatically created on first run, but you can manually create it:

```bash
# Navigate to the API project directory
cd TaskManager.API

# Apply migrations
dotnet ef database update
```

### Configuration

Update the connection strings in `appsettings.json` to match your environment:

```json
{
  "ConnectionStrings": {
    "DefaultConnection": "Server=(localdb)\\MSSQLLocalDB;Database=TaskManagerDb;Trusted_Connection=True;MultipleActiveResultSets=true",
    "Redis": "localhost:6379"
  },
  "RabbitMQ": {
    "Host": "localhost",
    "Username": "guest",
    "Password": "guest"
  }
}
```

### Running the Application

```bash
# Navigate to the API project directory
cd TaskManager.API

# Run the API
dotnet run
```

The API will be available at https://localhost:7049 (HTTPS).  
Swagger documentation will be available at https://localhost:7049/swagger/index.html.

### Running Tests

```bash
# Navigate to the test project directory
cd TaskManager.Tests

# Run tests
dotnet test
```

## API Endpoints

| Method | Endpoint                | Description                |
|--------|-------------------------|----------------------------|
| GET    | /api/tasks              | Get all tasks with pagination |
| GET    | /api/tasks/{id}         | Get task by ID             |
| GET    | /api/tasks/status/{status} | Get tasks by status     |
| POST   | /api/tasks              | Create a new task          |
| PUT    | /api/tasks/{id}         | Update an existing task    |
| PATCH  | /api/tasks/{id}/status  | Update task status         |
| DELETE | /api/tasks/{id}         | Delete a task              |

For detailed request and response schemas, refer to the Swagger documentation.

## Future Improvements

- Add authentication and authorization
- Implement event sourcing for task history
- Add more advanced filtering and sorting options
- Implement data validation using FluentValidation
- Add health checks and monitoring
- Deploy as Docker containers with orchestration

## API Documentation

You can view the complete API documentation in the Swagger format by clicking the link below:

[View Swagger JSON](https://raw.githubusercontent.com/oluwasege/TaskManager/refs/heads/master/TaskManager.API/swagger.json)
