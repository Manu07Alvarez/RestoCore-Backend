# RestoCore Backend (`resto-core-back`)

Plataforma SaaS multi-tenant para la gestion operativa y publicacion de cartas digitales accesibles mediante codigos QR dinamicos para el sector gastronomico.

---

## Arquitectura del Sistema

1. **Aislamiento Multi-Tenant Estricto:** Particionamiento por `TenantId` a nivel ORM mediante Global Query Filters en EF Core y politicas OPA desacopladas.
2. **Presupuesto de Latencia Dinamica:** Procesamiento de solicitudes en menos de 500ms en servidor y menos de 150ms p95 en la carta QR publica, con soporte para cabeceras `Cache-Control` y `ETag` (304 Not Modified).
3. **Persistencia Relacional Flexible:** Base de datos PostgreSQL 16+ con soporte de esquemas semiestructurados mediante columnas `JSONB` e indices `GIN`.
4. **Carga Asincrona Desacoplada de Archivos:** Emision de URLs pre-firmadas temporales hacia SeaweedFS (S3 compatible).
5. **Seguridad y Autorizacion Declarativa:** Politicas de control de acceso mediante Open Policy Agent (OPA / Rego).
6. **Observabilidad Distribuida:** OpenTelemetry instrumentado con enriquecimiento automatico de `tenant.id` en trazas, metricas y logs estructurados.

---

## Estructura del Repositorio

```text
resto-core-back/
+-- policies/                    # Politicas de autorizacion Rego para OPA
+-- src/
    +-- RestoCore.Domain/        # Entidades de dominio, Value Objects y contratos
    +-- RestoCore.Application/   # Casos de uso (CQRS), comandos, queries y validadores
    +-- RestoCore.Infrastructure/# Persistencia EF Core, SeaweedFS, OPA, telemetria OTLP y QR
    +-- RestoCore.Api/           # Endpoints Minimal APIs, middlewares y configuracion
```

---

## Despliegue e Infraestructura

### Prerrequisitos
- .NET 10 Runtime / SDK
- Docker y Docker Compose

### Servicios de Infraestructura (`docker-compose.yml`)
- **PostgreSQL 16**: Base de datos relacional principal con soporte `JSONB` (puerto `5432`).
- **Redis 7**: Cache distribuido y cola de procesamiento FIFO con Redis Streams (puerto `6379`).
- **SeaweedFS**: Almacenamiento de objetos compatible con S3 (puertos `8333`, `8888`, `9333`).
- **Open Policy Agent (OPA)**: Motor de politicas desacoplado para autorizacion declarativa (puerto `8181`).

Iniciar servicios de infraestructura:
```bash
docker compose up -d
```

### Ejecucion del Backend API
```bash
dotnet run --project src/RestoCore.Api/RestoCore.Api.csproj -c Release
```

Endpoints de salud:
- **Liveness Probe**: `/healthz`
- **Readiness Probe**: `/ready`
