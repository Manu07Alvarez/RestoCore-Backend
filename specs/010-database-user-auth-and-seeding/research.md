# Architectural Research: PostgreSQL User Authentication & On-Demand Database Seeding

**Feature**: `010-database-user-auth-and-seeding`  
**Date**: 2026-10-26  
**Status**: Completed

---

## 1. Cryptographic Password Hashing Strategy

### Decision
Implement `IPasswordHasher` using .NET's built-in `Rfc2898DeriveBytes.Pbkdf2` with `HashAlgorithmName.SHA256`, 100,000 iterations, a 16-byte cryptographic random salt (`RandomNumberGenerator.GetBytes(16)`), and a 32-byte derived key, verified via `CryptographicOperations.FixedTimeEquals`.
Format stored in `users.password_hash`:
`v1$<iterations>$<base64Salt>$<base64Subkey>`

### Rationale
- Requires zero third-party NuGet packages.
- Resistant to timing attacks via `CryptographicOperations.FixedTimeEquals`.
- Meets OWASP password storage guidelines for PBKDF2-HMAC-SHA256.

---

## 2. Multi-Tenancy Scope for `User` Entity

### Decision
`User` inherits from `BaseEntity` (and **not** `ITenantScopedEntity`) with an explicit nullable `TenantId: Guid?`.
- Why not `ITenantScopedEntity`?
  1. During `POST /api/v1/auth/login`, the client is unauthenticated and does not yet have a resolved `TenantId` in `ITenantContext` (the user logs in with their email address, and their associated `TenantId` and `TenantSlug` are discovered from their `User` record in PostgreSQL).
  2. Platform SuperAdmins (`super_admin`) operate globally across all tenants and have `TenantId = null`.

---

## 3. On-Demand Seeding Execution Mechanism

### Decision
- Support `--seed` CLI argument in `Program.cs` before `app.Run()`:
  - When `args.Contains("--seed", StringComparer.OrdinalIgnoreCase)` is detected, create an async scope, invoke `DatabaseSeeder.SeedAsync(scope.ServiceProvider, logger)`, and return immediately without starting the Kestrel HTTP listener (or if `--seed-and-run` is passed, seed and continue).
- Provide `scripts/seed-db.ps1` at the repository root so developers can run `./scripts/seed-db.ps1` with a single command whenever needed.
- Ensure `DatabaseSeeder` executes idempotent schema DDL (`CREATE TABLE IF NOT EXISTS users ...`, `ALTER TABLE tenants ADD COLUMN IF NOT EXISTS ...`) prior to seeding entities so it works seamlessly on both brand-new databases and pre-existing development volumes.
