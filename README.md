# RestoCore Backend (`resto-core-back`)

Plataforma SaaS multi-tenant para la gestión operativa y publicación de cartas digitales accesibles mediante códigos QR dinámicos para el sector gastronómico.

---

## Inviolables de Arquitectura

El backend se rige estrictamente por los principios definidos en la Constitución del proyecto (`.specify/memory/constitution.md`) y las directrices de `AGENTS.md`:

1. **Aislamiento Multi-Tenant Estricto:** Particionamiento obligatorio por `TenantId` a nivel ORM mediante Global Query Filters en EF Core y OPA policies desacopladas.
2. **Presupuesto de Latencia Dinámica:** Procesamiento de solicitudes en menos de 500ms en servidor y menos de 150ms p95 en la carta QR pública, con soporte para cabeceras `Cache-Control` y `ETag` (304 Not Modified).
3. **Persistencia Relacional Flexible:** Base de datos PostgreSQL 16+ con soporte de esquemas semiestructurados mediante columnas `JSONB` e índices `GIN` (`ADR-0003`).
4. **Carga Asíncrona Desacoplada de Archivos:** El backend emite URLs pre-firmadas temporales hacia SeaweedFS (`ADR-0004`).
5. **Seguridad y Autorización Declarativa:** Políticas de control de acceso mediante Open Policy Agent (OPA / Rego) con resolución de contexto (`ADR-0006`).
6. **Observabilidad Distribuida:** OpenTelemetry instrumentado con enriquecimiento automático de `tenant.id` en trazas, métricas y logs estructurados.

---

## Estructura del Repositorio

```text
resto-core-back/
+-- .agents/skills/              # Spec-Kit workflows y directrices SDD
+-- .specify/memory/             # Constitución del proyecto
+-- specs/
¦   +-- 001-multi-tenant-digital-menu/ # Especificación, plan, tareas y contratos
¦   +-- policies/                # Políticas de autorización Rego para OPA
+-- src/
¦   +-- RestoCore.Domain/        # Entidades de dominio, Value Objects y contratos
¦   +-- RestoCore.Application/   # Casos de uso (MediatR), comandos, queries y validadores
¦   +-- RestoCore.Infrastructure/# Persistencia EF Core, migraciones, OPA client, QR y telemetría
¦   +-- RestoCore.Api/           # Endpoints Minimal APIs, middlewares y configuración
+-- tests/
    +-- RestoCore.UnitTests/     # Pruebas unitarias de dominio, validaciones y resolución
    +-- RestoCore.IntegrationTests/ # Pruebas de integración de endpoints
    +-- RestoCore.SecurityTests/ # Pruebas automatizadas de aislamiento multi-tenant
```

---

## Ejecución Local y Pruebas

### Prerrequisitos
- .NET 10 SDK (o .NET 9+)
- Docker y Docker Compose

### Infraestructura Local
```bash
docker compose -f docker-compose.dev.yml up -d
```

### Ejecutar Pruebas Unitarias
```bash
dotnet test tests/RestoCore.UnitTests/RestoCore.UnitTests.csproj
```

### Ejecutar API Backend
```bash
dotnet run --project src/RestoCore.Api/RestoCore.Api.csproj
```

Acceso al explorador Swagger / OpenAPI: `http://localhost:5000/swagger`
Chequeo de salud del sistema: `http://localhost:5000/healthz`
