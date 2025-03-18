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

```Swagger
{
"openapi": "3.0.1",
"info": {
"title": "Task Management API",
"description": "A Task Management API built with .NET Core",
"contact": {
"name": "Tingtel Tech",
"url": "https://tingteltech.com",
"email": "info@tingteltech.com"
},
"version": "v1"
},
"paths": {
"/api/Tasks": {
"post": {
"tags": [
"Tasks"
],
"summary": "Creates a new task",
"requestBody": {
"description": "Task creation data",
"content": {
"application/json": {
"schema": {
"$ref": "#/components/schemas/CreateTaskCommand"
}
},
"text/json": {
"schema": {
"$ref": "#/components/schemas/CreateTaskCommand"
}
},
"application/*+json": {
"schema": {
"$ref": "#/components/schemas/CreateTaskCommand"
}
}
}
},
"responses": {
"201": {
"description": "Returns the ID of the created task",
"content": {
"application/json": {
"schema": {
"type": "string",
"format": "uuid"
}
}
}
},
"400": {
"description": "If the command is invalid",
"content": {
"application/json": {
"schema": {
"$ref": "#/components/schemas/ProblemDetails"
}
}
}
}
}
},
"get": {
"tags": [
"Tasks"
],
"summary": "Gets all tasks with pagination",
"parameters": [
{
"name": "page",
"in": "query",
"description": "Page number (default: 1)",
"schema": {
"type": "integer",
"format": "int32",
"default": 1
}
},
{
"name": "pageSize",
"in": "query",
"description": "Page size (default: 10)",
"schema": {
"type": "integer",
"format": "int32",
"default": 10
}
}
],
"responses": {
"200": {
"description": "Returns the list of tasks",
"content": {
"application/json": {
"schema": {
"$ref": "#/components/schemas/TaskDtoPagedResult"
}
}
}
}
}
}
},
"/api/Tasks/{id}": {
"put": {
"tags": [
"Tasks"
],
"summary": "Updates an existing task",
"parameters": [
{
"name": "id",
"in": "path",
"description": "Task ID",
"required": true,
"schema": {
"type": "string",
"format": "uuid"
}
}
],
"requestBody": {
"description": "Task update data",
"content": {
"application/json": {
"schema": {
"$ref": "#/components/schemas/UpdateTaskCommand"
}
},
"text/json": {
"schema": {
"$ref": "#/components/schemas/UpdateTaskCommand"
}
},
"application/*+json": {
"schema": {
"$ref": "#/components/schemas/UpdateTaskCommand"
}
}
}
},
"responses": {
"204": {
"description": "Task successfully updated"
},
"400": {
"description": "If the command is invalid",
"content": {
"application/json": {
"schema": {
"$ref": "#/components/schemas/ProblemDetails"
}
}
}
},
"404": {
"description": "Task not found",
"content": {
"application/json": {
"schema": {
"$ref": "#/components/schemas/ProblemDetails"
}
}
}
}
}
},
"delete": {
"tags": [
"Tasks"
],
"summary": "Deletes a task",
"parameters": [
{
"name": "id",
"in": "path",
"description": "Task ID",
"required": true,
"schema": {
"type": "string",
"format": "uuid"
}
}
],
"responses": {
"204": {
"description": "Task successfully deleted"
},
"404": {
"description": "Task not found",
"content": {
"application/json": {
"schema": {
"$ref": "#/components/schemas/ProblemDetails"
}
}
}
}
}
},
"get": {
"tags": [
"Tasks"
],
"summary": "Gets a task by its ID",
"parameters": [
{
"name": "id",
"in": "path",
"description": "Task ID",
"required": true,
"schema": {
"type": "string",
"format": "uuid"
}
}
],
"responses": {
"200": {
"description": "Returns the task",
"content": {
"application/json": {
"schema": {
"$ref": "#/components/schemas/TaskDto"
}
}
}
},
"404": {
"description": "Task not found",
"content": {
"application/json": {
"schema": {
"$ref": "#/components/schemas/ProblemDetails"
}
}
}
}
}
}
},
"/api/Tasks/{id}/status": {
"patch": {
"tags": [
"Tasks"
],
"summary": "Updates the status of a task",
"parameters": [
{
"name": "id",
"in": "path",
"description": "Task ID",
"required": true,
"schema": {
"type": "string",
"format": "uuid"
}
}
],
"requestBody": {
"description": "Status update data",
"content": {
"application/json": {
"schema": {
"$ref": "#/components/schemas/UpdateTaskStatusCommand"
}
},
"text/json": {
"schema": {
"$ref": "#/components/schemas/UpdateTaskStatusCommand"
}
},
"application/*+json": {
"schema": {
"$ref": "#/components/schemas/UpdateTaskStatusCommand"
}
}
}
},
"responses": {
"204": {
"description": "Task status successfully updated"
},
"400": {
"description": "If the status transition is invalid",
"content": {
"application/json": {
"schema": {
"$ref": "#/components/schemas/ProblemDetails"
}
}
}
},
"404": {
"description": "Task not found",
"content": {
"application/json": {
"schema": {
"$ref": "#/components/schemas/ProblemDetails"
}
}
}
}
}
}
},
"/api/Tasks/status/{status}": {
"get": {
"tags": [
"Tasks"
],
"summary": "Gets tasks by status with pagination",
"parameters": [
{
"name": "status",
"in": "path",
"description": "Task status\n\n0 = Pending\n\n1 = InProgress\n\n2 = Completed\n\n3 = Canceled\n\n4 = Blocked",
"required": true,
"schema": {
"$ref": "#/components/schemas/TaskStatus"
},
"x-enumNames": [
"Pending",
"InProgress",
"Completed",
"Canceled",
"Blocked"
]
},
{
"name": "page",
"in": "query",
"description": "Page number (default: 1)",
"schema": {
"type": "integer",
"format": "int32",
"default": 1
}
},
{
"name": "pageSize",
"in": "query",
"description": "Page size (default: 10)",
"schema": {
"type": "integer",
"format": "int32",
"default": 10
}
}
],
"responses": {
"200": {
"description": "Returns the list of tasks",
"content": {
"application/json": {
"schema": {
"$ref": "#/components/schemas/TaskDtoPagedResult"
}
}
}
},
"400": {
"description": "If the status is invalid",
"content": {
"application/json": {
"schema": {
"$ref": "#/components/schemas/ProblemDetails"
}
}
}
}
}
}
}
},
"components": {
"schemas": {
"CreateTaskCommand": {
"type": "object",
"properties": {
"title": {
"type": "string",
"nullable": true
},
"description": {
"type": "string",
"nullable": true
},
"dueDate": {
"type": "string",
"format": "date-time"
}
},
"additionalProperties": false
},
"ProblemDetails": {
"type": "object",
"properties": {
"type": {
"type": "string",
"nullable": true
},
"title": {
"type": "string",
"nullable": true
},
"status": {
"type": "integer",
"format": "int32",
"nullable": true
},
"detail": {
"type": "string",
"nullable": true
},
"instance": {
"type": "string",
"nullable": true
}
},
"additionalProperties": {}
},
"TaskDto": {
"type": "object",
"properties": {
"id": {
"type": "string",
"format": "uuid"
},
"title": {
"type": "string",
"nullable": true
},
"description": {
"type": "string",
"nullable": true
},
"status": {
"type": "string",
"nullable": true
},
"dueDate": {
"type": "string",
"format": "date-time"
},
"createdAt": {
"type": "string",
"format": "date-time"
},
"updatedAt": {
"type": "string",
"format": "date-time"
}
},
"additionalProperties": false
},
"TaskDtoPagedResult": {
"type": "object",
"properties": {
"items": {
"type": "array",
"items": {
"$ref": "#/components/schemas/TaskDto"
},
"nullable": true
},
"totalItems": {
"type": "integer",
"format": "int32"
},
"page": {
"type": "integer",
"format": "int32"
},
"pageSize": {
"type": "integer",
"format": "int32"
},
"totalPages": {
"type": "integer",
"format": "int32",
"readOnly": true
}
},
"additionalProperties": false
},
"TaskStatus": {
"enum": [
0,
1,
2,
3,
4
],
"type": "integer",
"description": "\n\n0 = Pending\n\n1 = InProgress\n\n2 = Completed\n\n3 = Canceled\n\n4 = Blocked",
"format": "int32",
"x-enumNames": [
"Pending",
"InProgress",
"Completed",
"Canceled",
"Blocked"
]
},
"UpdateTaskCommand": {
"type": "object",
"properties": {
"id": {
"type": "string",
"format": "uuid"
},
"title": {
"type": "string",
"nullable": true
},
"description": {
"type": "string",
"nullable": true
},
"dueDate": {
"type": "string",
"format": "date-time"
}
},
"additionalProperties": false
},
"UpdateTaskStatusCommand": {
"type": "object",
"properties": {
"id": {
"type": "string",
"format": "uuid"
},
"newStatus": {
"$ref": "#/components/schemas/TaskStatus"
}
},
"additionalProperties": false
}
}
}
}
```