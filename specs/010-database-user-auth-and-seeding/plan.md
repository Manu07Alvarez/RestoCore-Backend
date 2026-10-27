# Implementation Plan: PostgreSQL User Authentication & On-Demand Database Seeding

**Branch**: `feature/010-database-user-auth-and-seeding` | **Date**: 2026-10-26 | **Spec**: [specs/010-database-user-auth-and-seeding/spec.md](file:///r:/Programando/RestoCore/resto-core-back/specs/010-database-user-auth-and-seeding/spec.md)

---

## 1. Summary

Remove all hardcoded in-memory user credentials from `AuthEndpoints.cs`, introduce a persistent `User` domain entity in PostgreSQL with PBKDF2-HMAC-SHA256 cryptographic password hashing (`IPasswordHasher`), implement CQRS `LoginCommand` / `LoginCommandHandler`, and provide an on-demand, idempotent database seeder (`DatabaseSeeder`) invoked exclusively via `./scripts/seed-db.ps1` or `dotnet run --project src/RestoCore.Api -- --seed`.

---

## 2. Technical Context

**Language/Version**: C# / .NET 10.0  
**Primary Dependencies**: ASP.NET Core Minimal APIs, Entity Framework Core 10, `System.Security.Cryptography.Rfc2898DeriveBytes`  
**Storage**: PostgreSQL (`users`, `tenants`, `categories`, `menu_items`, `tables`)  
**Testing**: xUnit, FluentAssertions, Moq (`RestoCore.UnitTests`)  
**Constraints**: Zero hardcoded user accounts in application runtime; manual on-demand seeding only (`--seed`)  

---

## 3. Constitution Check

| Principle | Evaluation | Status |
| :--- | :--- | :--- |
| **I. Spec-Driven Development** | Follows `/api/v1/auth/login` contract in OpenAPI 3.1. | PASS |
| **II. Multi-Tenancy & Authorization** | JWT tokens populated from PostgreSQL `User` and `Tenant` records; OPA roles preserved. | PASS |
| **III. Clean Architecture** | `User` in Domain, `LoginCommand` in Application, `UserRepository` & `PasswordHasher` & `DatabaseSeeder` in Infrastructure, thin `AuthEndpoints` in Api. | PASS |
| **IV. Latency Budget** | Indexed lookup on `users.email` completes in < 15ms. | PASS |
| **V. Persistence Discipline** | PostgreSQL serves as the single source of truth for user identity and credentials. | PASS |
| **VI. Test-Driven Quality** | Unit tests for `PasswordHasher` and `LoginCommandHandler` precede implementation. | PASS |

---

## 4. Source Code Layout

```text
scripts/
└── seed-db.ps1                                              # On-demand PowerShell seeding script

src/
├── RestoCore.Domain/
│   └── Entities/
│       └── User.cs                                          # User entity
├── RestoCore.Application/
│   ├── Common/Interfaces/
│   │   ├── IPasswordHasher.cs                               # Password hashing abstraction
│   │   └── Repositories/IUserRepository.cs                  # User repository abstraction
│   └── Features/Auth/
│       └── Commands/LoginCommand.cs                         # LoginCommand, Handler, and DTOs
├── RestoCore.Infrastructure/
│   ├── Identity/
│   │   └── PasswordHasher.cs                                # PBKDF2 SHA-256 implementation
│   └── Persistence/
│       ├── ApplicationDbContext.cs                          # Add DbSet<User> Users
│       ├── Configurations/UserConfiguration.cs              # EF Core configuration for users table
│       ├── Repositories/UserRepository.cs                   # EF Core UserRepository
│       └── Seed/DatabaseSeeder.cs                           # Idempotent on-demand seeder
└── RestoCore.Api/
    ├── Endpoints/AuthEndpoints.cs                           # Remove hardcoded accounts; dispatch LoginCommand
    └── Program.cs                                           # Register services & handle --seed CLI flag

tests/RestoCore.UnitTests/
└── Features/Auth/
    ├── PasswordHasherTests.cs
    └── LoginCommandHandlerTests.cs
```
