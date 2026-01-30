# Books API — Minimal MVP

A simple REST API for selling books, built with .NET 8, Clean Architecture, and dependency injection. Designed to test AI agent capabilities.

## Features

- ✅ **CRUD Operations** — Books and Orders endpoints
- ✅ **In-Memory Persistence** — No external database dependency
- ✅ **Clean Architecture** — Separated Domain, Application, Infrastructure, and API layers
- ✅ **Dependency Injection** — Async/await patterns end-to-end
- ✅ **Health Checks** — `/health` endpoint
- ✅ **Swagger/OpenAPI** — Auto-generated API docs
- ✅ **Containerized** — Dockerfile and Kubernetes manifest included
- ✅ **Production Patterns** — Proper error handling, validation, and structured logging

## Project Structure

```
books-api/
├── src/
│   ├── Books.Domain/          # Domain entities (Book, Order)
│   ├── Books.Application/      # DTOs, service interfaces, repositories
│   ├── Books.Infrastructure/   # In-memory repositories, service implementations
│   └── Books.Api/              # REST controllers, Program.cs
├── tests/                      # Test projects (unit, integration)
├── Dockerfile                  # Multi-stage build
├── k8s-deployment.yaml        # Kubernetes deployment & service
└── books-api.sln              # Solution file
```

## Building & Running

### Prerequisites
- .NET 8 SDK

### Run Locally
```bash
cd src/Books.Api
dotnet run
```

Access the API:
- **Swagger UI**: http://localhost:5000/swagger
- **Health Check**: http://localhost:5000/health

### Run Tests
```bash
dotnet test
```

### Docker
```bash
docker build -t books-api:latest .
docker run -p 8080:8080 books-api:latest
```

### Kubernetes
```bash
kubectl apply -f k8s-deployment.yaml
kubectl get svc books-api
```

## API Endpoints

### Books
- `GET /api/books` — List all books
- `GET /api/books/{id}` — Get book by ID
- `POST /api/books` — Create a new book
- `PUT /api/books/{id}` — Update a book
- `DELETE /api/books/{id}` — Delete a book

### Orders
- `GET /api/orders` — List all orders
- `GET /api/orders/{id}` — Get order by ID
- `POST /api/orders` — Create a new order (validates stock, decrements inventory)

### Health
- `GET /health` — Health check

## Example Requests

### Create a Book
```bash
curl -X POST http://localhost:5000/api/books \
  -H "Content-Type: application/json" \
  -d '{
    "title": "Clean Code",
    "author": "Robert C. Martin",
    "price": 39.99,
    "stockQuantity": 10,
    "description": "A Handbook of Agile Software Craftsmanship"
  }'
```

### Create an Order
```bash
curl -X POST http://localhost:5000/api/orders \
  -H "Content-Type: application/json" \
  -d '{
    "bookId": 1,
    "quantity": 2
  }'
```

## Design Principles

- **SOLID** — Single Responsibility, Dependency Inversion
- **DRY** — Reusable components and minimal duplication
- **Async/Await** — All I/O operations are fully async
- **Validation** — Input validation at service layer
- **Error Handling** — Precise exceptions; no silent catches
- **Testability** — Repositories and services abstracted behind interfaces

## Future Enhancements

- [ ] Database integration (Entity Framework Core + PostgreSQL)
- [ ] Authentication & Authorization (JWT)
- [ ] Logging & Observability (Serilog, OpenTelemetry)
- [ ] CQRS & Event Sourcing
- [ ] Unit & Integration tests
- [ ] Rate limiting & API versioning
