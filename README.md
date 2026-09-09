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
6. **Observabilidad Distribuida:** OpenTelemetry instrumentado con enriquecimiento automatico de `tenant.id` en trazas, metricas y logs estructurados visualizados en **.NET Aspire Dashboard**.

---

## Estructura del Repositorio

```text
resto-core-back/
+-- .agents/skills/              # Spec-Kit workflows y directrices SDD
+-- .specify/memory/             # Constitucion del proyecto
+-- scripts/
|   +-- k6/                      # Scripts k6 de pruebas de carga, estres y validacion de ETag
|   +-- run-stress-tests.ps1     # Runner automatizado k6 (CLI local o Docker fallback)
|   +-- verify-docker-env.ps1    # Verificacion integral de contenedores y suites de prueba
+-- specs/
|   +-- 001-multi-tenant-digital-menu/ # Especificacion inicial de carta digital
|   +-- 002-docker-dev-testing/        # Entorno Docker controlado y tests E2E
|   +-- 003-k6-aspire-swagger/         # Pruebas de estres k6, Aspire Dashboard y Swagger UI
|   +-- policies/                # Politicas de autorizacion Rego para OPA
+-- src/
|   +-- RestoCore.Domain/        # Entidades de dominio, Value Objects y contratos
|   +-- RestoCore.Application/   # Casos de uso (MediatR), comandos, queries y validadores
|   +-- RestoCore.Infrastructure/# Persistencia EF Core, SeaweedFS, OPA, telemetria OTLP y QR
|   +-- RestoCore.Api/           # Endpoints Minimal APIs, middlewares, Swagger UI y configuracion
+-- tests/
    +-- RestoCore.UnitTests/     # Pruebas unitarias de dominio y validaciones
    +-- RestoCore.IntegrationTests/ # Pruebas de integracion contra stack de contenedores
    +-- RestoCore.SecurityTests/ # Pruebas automatizadas de aislamiento multi-tenant en PostgreSQL
```

---

## Ejecucion Local y Herramientas de Desarrollo

### Prerrequisitos
- .NET 10 SDK (o .NET 9+)
- Docker y Docker Compose

### Infraestructura Local en Docker
El entorno de desarrollo orquesta todos los servicios requeridos:
- **PostgreSQL 16**: Base de datos relacional principal con soporte `JSONB` (puerto `5432`).
- **Redis 7**: Cache de alta velocidad (puerto `6379`).
- **SeaweedFS**: Almacenamiento desacoplado S3 (puertos `8333`, `8888`, `9333`) y bucket auto-inicializado `restocore-images`.
- **Open Policy Agent (OPA)**: Motor de politicas desacoplado para autorizacion declarativa (puerto `8181`).
- **.NET Aspire Dashboard**: Visualizador en tiempo real de trazas distribuidas OTLP, metricas y logs estructurados (Web UI en puerto `18888`, receptor OTLP en puerto `4317`).

Iniciar el stack completo:
```bash
docker compose -f docker-compose.dev.yml up -d
```

Acceder al panel de telemetria en tiempo real:
- **Aspire Dashboard**: `http://localhost:18888`

### Ejecutar API Backend y Swagger UI
```bash
dotnet run --project src/RestoCore.Api/RestoCore.Api.csproj
```

- **Swagger UI interactivo (Development)**: `http://localhost:5000/swagger`
- **Contrato OpenAPI**: `http://localhost:5000/swagger/v1/swagger.json`
- **Liveness Probe**: `http://localhost:5000/healthz`
- **Readiness Probe**: `http://localhost:5000/ready`

### Pruebas de Carga y Estres con k6
Ejecuta las pruebas de rendimiento validando que las respuestas cumplan los presupuestos de latencia (<500ms dinamico, <150ms p95 con ETag):
```powershell
# Carga base (20 VUs)
./scripts/run-stress-tests.ps1 -Scenario public-menu-load

# Validacion de cache ETag 304 (<150ms p95)
./scripts/run-stress-tests.ps1 -Scenario etag-cache-test

# Estres con rampa hasta 100 VUs
./scripts/run-stress-tests.ps1 -Scenario public-menu-stress
```

### Ejecutar Suites de Pruebas Automatizadas
```bash
dotnet test RestoCore.slnx
```
