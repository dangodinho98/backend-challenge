[Back to README](../README.md)

## Project Structure

The solution follows **Clean Architecture** with **DDD** for the Sales bounded context. User/Auth endpoints from the original template remain alongside Sales.

```
root
├── .doc/                          # Project documentation (API specs, conventions)
├── src/
│   ├── Ambev.DeveloperEvaluation.Domain/
│   │   ├── Entities/              # Sale aggregate, SaleItem, User
│   │   ├── Events/                # SaleCreated, SaleModified, etc.
│   │   ├── Exceptions/            # Domain exceptions
│   │   ├── Repositories/          # ISaleRepository, IUserRepository
│   │   ├── Services/              # DiscountPolicy
│   │   └── ValueObjects/          # ExternalIdentity
│   ├── Ambev.DeveloperEvaluation.Application/
│   │   ├── Sales/                 # MediatR handlers (Create, Get, List, Update, Cancel, Delete)
│   │   ├── Events/                # DomainEventDispatcher + log handlers
│   │   └── Users/                 # Template user/auth use cases
│   ├── Ambev.DeveloperEvaluation.ORM/
│   │   ├── Mapping/               # EF Core configurations
│   │   ├── Migrations/
│   │   └── Repositories/          # SaleRepository, UserRepository
│   ├── Ambev.DeveloperEvaluation.IoC/       # DI module initializers
│   ├── Ambev.DeveloperEvaluation.Common/    # Cross-cutting (JWT, validation, health)
│   └── Ambev.DeveloperEvaluation.WebApi/
│       ├── Features/
│       │   ├── Sales/             # SalesController, DTOs, validators
│       │   ├── Users/
│       │   └── Auth/
│       └── Middleware/            # Domain + validation exception handling
├── tests/
│   ├── Ambev.DeveloperEvaluation.Unit/        # Domain + handler unit tests
│   ├── Ambev.DeveloperEvaluation.Integration/ # Repository integration tests
│   └── Ambev.DeveloperEvaluation.Functional/  # WebApplicationFactory API tests
├── Ambev.DeveloperEvaluation.sln
├── docker-compose.yml             # PostgreSQL, MongoDB, Redis (template infra)
└── README.md
```

### Layer responsibilities

| Layer | Role |
|-------|------|
| **Domain** | Business rules, aggregates, domain events — no infrastructure references |
| **Application** | Use cases (MediatR commands/queries), mapping to domain models |
| **ORM** | EF Core persistence, PostgreSQL, repository implementations |
| **WebApi** | HTTP endpoints, request/response DTOs, AutoMapper profiles |
| **IoC** | Service registration wiring |

### Sales feature layout

Vertical slices under `Application/Sales/` and `WebApi/Features/Sales/`:

- `CreateSale`, `GetSale`, `ListSales`, `ModifySale` (PUT), `CancelSale`, `CancelSaleItem`, `DeleteSale`

<br/>
<div style="display: flex; justify-content: space-between;">
  <a href="./auth-api.md">Previous: Auth API</a>
  <a href="../README.md">Next: Read Me</a>
</div>
