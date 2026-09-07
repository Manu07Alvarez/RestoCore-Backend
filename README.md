# RestoCore Backend (`resto-core-back`)

Plataforma SaaS multi-tenant para la gestion operativa y publicacion de cartas digitales accesibles mediante codigos QR dinamicos para el sector gastronomico.

---

## Inviolables de Arquitectura

El backend se rige estrictamente por los principios definidos en la Constitucion del proyecto (`.specify/memory/constitution.md`) y las directrices de `AGENTS.md`:

1. **Aislamiento Multi-Tenant Estricto:** Particionamiento obligatorio por `TenantId` a nivel ORM mediante Global Query Filters en EF Core y OPA policies desacopladas.
2. **Presupuesto de Latencia Dinamica:** Procesamiento de solicitudes en menos de 500ms en servidor y menos de 150ms p95 en la carta QR publica, con soporte para cabeceras `Cache-Control` y `ETag` (304 Not Modified).
3. **Persistencia Relacional Flexible:** Base de datos PostgreSQL 16+ con soporte de esquemas semiestructurados mediante columnas `JSONB` e indices `GIN` (`ADR-0003`).
4. **Carga Asincrona Desacoplada de Archivos:** El backend emite URLs pre-firmadas temporales hacia SeaweedFS (`ADR-0004`).
5. **Seguridad y Autorizacion Declarativa:** Politicas de control de acceso mediante Open Policy Agent (OPA / Rego) con resolucion de contexto (`ADR-0006`).
6. **Observabilidad Distribuida:** OpenTelemetry instrumentado con enriquecimiento automatico de `tenant.id` en trazas, metricas y logs estructurados.

---

## Estructura del Repositorio

```text
resto-core-back/
+-- .agents/skills/              # Spec-Kit workflows y directrices SDD
+-- .specify/memory/             # Constitucion del proyecto
+-- scripts/                     # Scripts de automatizacion y verificacion (verify-docker-env.ps1)
+-- specs/
|   +-- 001-multi-tenant-digital-menu/ # Especificacion, plan, tareas y contratos
|   +-- 002-docker-dev-testing/        # Entorno Docker controlado y tests E2E
|   +-- policies/                # Politicas de autorizacion Rego para OPA
+-- src/
|   +-- RestoCore.Domain/        # Entidades de dominio, Value Objects y contratos
|   +-- RestoCore.Application/   # Casos de uso (MediatR), comandos, queries y validadores
|   +-- RestoCore.Infrastructure/# Persistencia EF Core, migraciones, SeaweedFS, OPA client, QR y telemetria
|   +-- RestoCore.Api/           # Endpoints Minimal APIs, middlewares y configuracion
+-- tests/
    +-- RestoCore.UnitTests/     # Pruebas unitarias de dominio, validaciones y resolucion
    +-- RestoCore.IntegrationTests/ # Pruebas de integracion contra stack de contenedores
    +-- RestoCore.SecurityTests/ # Pruebas automatizadas de aislamiento multi-tenant en PostgreSQL
```

---

## Ejecucion Local y Pruebas

### Prerrequisitos
- .NET 10 SDK (o .NET 9+)
- Docker y Docker Compose

### Infraestructura Local en Docker
El entorno de desarrollo y pruebas utiliza Docker Compose para orquestar todos los servicios requeridos:
- **PostgreSQL 16**: Base de datos principal relacional con soporte `JSONB`.
- **Redis 7**: Cache de alta velocidad y encolado de eventos.
- **SeaweedFS**: Almacenamiento desacoplado compatible con S3 (puerto S3 8333) y bucket auto-inicializado `restocore-images`.
- **Open Policy Agent (OPA)**: Motor de politicas desacoplado para autorizacion declarativa.

Iniciar la infraestructura:
```bash
docker compose -f docker-compose.dev.yml up -d
```

### Script de Verificacion Automatizada (Entorno y Suites de Pruebas)
Para verificar de forma integral la salud del stack, aplicar migraciones y ejecutar todas las suites de prueba:
```powershell
./scripts/verify-docker-env.ps1
```

### Migraciones de Base de Datos
Aplicar el esquema relacional en PostgreSQL contenedorizado:
```bash
dotnet ef database update --project src/RestoCore.Infrastructure/RestoCore.Infrastructure.csproj --startup-project src/RestoCore.Api/RestoCore.Api.csproj
```

### Ejecutar Pruebas Automatizadas

```bash
# Ejecutar todas las suites de pruebas unitarias, integracion y seguridad
dotnet test tests/RestoCore.UnitTests/RestoCore.UnitTests.csproj
dotnet test tests/RestoCore.IntegrationTests/RestoCore.IntegrationTests.csproj
dotnet test tests/RestoCore.SecurityTests/RestoCore.SecurityTests.csproj
```

### Ejecutar API Backend
```bash
dotnet run --project src/RestoCore.Api/RestoCore.Api.csproj
```

- Explorador OpenAPI / Scalar: `http://localhost:5000/openapi/v1.json`
- Chequeo de salud basico (Liveness): `http://localhost:5000/healthz`
- Chequeo de dependencias profundas (Readiness): `http://localhost:5000/ready`
