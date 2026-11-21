# Feature Specification: Estrategia de Empaquetado y Canales de Distribución para Licencias Perpetuas (Self-Hosted)

**Feature Branch**: `feature/013-perpetual-license-selfhosted-packaging`  
**Created**: 2026-11-12  
**Status**: Draft  
**Input**: Sincronización documental de `RestoCore-Docs` (`sync-docs-specs`) para implementar `ADR-0019` (`0019-empaquetado-distribucion-licencias-perpetuas.md`).

**Decisiones de Arquitectura de Origen (`RestoCore-Docs`)**:
- [ADR-0019: Estrategia de Empaquetado y Canales de Distribución para Licencias Perpetuas (Self-Hosted)](../../../../RestoCore-Docs/docs/Website/docs/developers/adr/0019-empaquetado-distribucion-licencias-perpetuas.md) (`0019-empaquetado-distribucion-licencias-perpetuas.md`)
- [ADR-0003: Selección de PostgreSQL con JSONB para Persistencia Semiestructurada](../../../../RestoCore-Docs/docs/adr/0003-seleccion-de-postgresql-con-jsonb-para-persistencia-semiestructurada.md)
- [ADR-0004: Carga Asíncrona de Imágenes hacia SeaweedFS](../../../../RestoCore-Docs/docs/adr/0004-carga-asincrona-de-imagenes-seaweedfs.md)

---

## User Scenarios & Testing *(mandatory)*

### User Story 1 - Modo de Ejecución Self-Hosted, Validación Criptográfica de Licencia Offline y Abstracción de Almacenamiento Local (Priority: P1)

Como cliente con licencia perpetua auto-hospedada (Self-Hosted / On-Premise), necesito que el backend C# / .NET 10 opere bajo `DEPLOYMENT_MODE=SelfHosted`, valide de forma criptográfica y offline los atributos de mi licencia perpetua (`ILicenseValidator`, incluyendo el límite `max_branches = 1` en instalaciones de Nodo Único / Canal B) y permita conmutar por configuración entre almacenamiento de objetos compatible con S3 (`Storage:Provider=SeaweedFS`, Canal A) y almacenamiento en disco local (`Storage:Provider=LocalDisk`, Canal B) detrás de `IStorageService`.

**Why this priority**: Constituye la base de negocio y de arquitectura en el código de aplicación e infraestructura para habilitar tanto el Canal A (Docker Compose) como el Canal B (Nodo Único Windows POS) desde una única base de código `.NET 10` sin alterar los contratos de dominio.

**Independent Test**: Se puede probar de forma independiente instanciando `ILicenseValidator` con licencias firmadas válidas, expiradas o que excedan `max_branches = 1` en Canal B, y verificando que `LocalDiskStorageService` e `IStorageService` se resuelven según `Storage:Provider` sin requerir conexión externa a SeaweedFS cuando se configura `LocalDisk`.

**Acceptance Scenarios**:

1. **Given** el backend configurado con `Deployment:Mode=SelfHosted` (o variable de entorno `DEPLOYMENT_MODE=SelfHosted`) y una licencia perpetua válida firmada criptográficamente, **When** la aplicación inicia o se consulta el estado de licencia/salud, **Then** `ILicenseValidator` verifica la firma offline sin llamadas a servidores externos y expone las capacidades autorizadas (`max_branches`, canal de distribución, fecha de emisión).
2. **Given** una instalación en Canal B (`Deployment:Channel=SingleNodeWindows`) cuya licencia o intento de creación de sucursal supera `max_branches = 1`, **When** se intenta registrar un segundo tenant/sucursal activo, **Then** el sistema rechaza la operación con un error de política de licencia (`403 Forbidden` / `ProblemDetails` RFC 7807) indicando que el Canal B está limitado a 1 sucursal y requiere migración al Canal A.
3. **Given** la configuración `Storage:Provider=LocalDisk` en modo Nodo Único (Canal B), **When** el administrador solicita una URL de carga de imagen o sirve un activo estático del catálogo, **Then** la implementación `LocalDiskStorageService` gestiona el archivo dentro del directorio local configurado manteniendo intacta la interfaz `IStorageService`.

---

### User Story 2 - Canal A: Paquete de Distribución Contenerizado Minimalista con Docker Compose (Priority: P1)

Como ingeniero de despliegue o administrador de sistemas de una cadena gastronómica o restaurante multi-terminal (Canal A), necesito contar con artefactos de despliegue listos para producción (`docker-compose.selfhosted.yml`, plantilla `.env.selfhosted.template`, perfil de publicación `PublishSingleFile` para imagen minimalista `runtime-deps:10.0-noble-chiseled` y script de verificación inicial) que orquesten el binario `.NET 10`, PostgreSQL 16+ (`JSONB`/`GIN`), SeaweedFS y proxy inverso con terminación TLS.

**Why this priority**: El Canal A es el estándar principal de distribución para licencias perpetuas en servidores Linux, VPS y entornos multi-terminal con KDS activo.

**Independent Test**: Se puede verificar validando la compilación del perfil de publicación `PublishSingleFile` sin símbolos de depuración (`DebugType=None`, `DebugSymbols=false`) y comprobando la sintaxis y completitud de `docker-compose.selfhosted.yml`, `.env.selfhosted.template` y el script de verificación pre-vuelo.

**Acceptance Scenarios**:

1. **Given** el repositorio del backend `.NET 10`, **When** se compila con el perfil de publicación de distribución perpetua (`PublishSingleFile=true`, `DebugType=None`, `DebugSymbols=false`), **Then** el artefacto resultante no incluye archivos `.pdb` ni código fuente expuesto.
2. **Given** el paquete de despliegue del Canal A (`deploy/selfhosted/canal-a/`), **When** se valida la definición de servicios en `docker-compose.selfhosted.yml`, **Then** incluye los servicios `app` (`DEPLOYMENT_MODE=SelfHosted`), `db` (PostgreSQL 16+), `storage` (SeaweedFS compatible con S3), `redis` y `proxy` (Caddy/Traefik para TLS local) utilizando únicamente componentes bajo licencias OSI permisivas.

---

### User Story 3 - Canal B: Soporte de Nodo Único Windows POS, Servicio de Windows y Adaptación de Persistencia Subordinada a ADR-0003 (Priority: P2)

Como propietario de un comercio gastronómico independiente que opera sobre una única PC con Windows en mostrador (Canal B), necesito que el binario `.NET 10` pueda empaquetarse como ejecutable autocontenido (`win-x64`, `SelfContained=true`, `PublishSingleFile=true`) capaz de registrarse como Servicio de Windows y soportar configuración de proveedor de base de datos (`Database:Provider=Postgres|Sqlite` en modo WAL con respaldo local) garantizando que todas las consultas que usan operadores nativos `JSONB`/`GIN` de PostgreSQL (`ADR-0003` / `ADR-0014`) cuenten con adaptación explícita para proveedores no-Npgsql.

**Why this priority**: Elimina la barrera técnica y el consumo de memoria RAM de Docker Desktop en terminales POS Windows de caja única, manteniendo compatibilidad con la suite de pruebas en ambos motores.

**Independent Test**: Se puede probar ejecutando las consultas de repositorios y el registro de `ApplicationDbContext` bajo proveedor SQLite/no-Npgsql y verificando que las operaciones de catálogo, órdenes y autenticación funcionan sin invocar funciones exclusivas de `Npgsql` cuando `Database.IsNpgsql()` es falso.

**Acceptance Scenarios**:

1. **Given** `Database:Provider=Sqlite` configurado para una terminal de Nodo Único (Canal B), **When** se inicializa la persistencia y se ejecutan consultas sobre órdenes (`HasActiveOrdersForMenuItemAsync`) y catálogo, **Then** el sistema activa `PRAGMA journal_mode=WAL;` y utiliza la ruta de evaluación compatible con SQLite sin lanzar excepciones de traducción LINQ.
2. **Given** los artefactos de empaquetado del Canal B (`deploy/selfhosted/canal-b/`), **When** se inspecciona el manifiesto de instalación (Inno Setup / script de registro de Windows Service y respaldo automático), **Then** configura el ejecutable autocontenido `win-x64` con inicio automático, `DEPLOYMENT_MODE=SelfHosted`, `Storage__Provider=LocalDisk` y validación de límite `max_branches = 1`.

---

### Edge Cases

- **Licencia perpetua ausente o con firma inválida en `DEPLOYMENT_MODE=SelfHosted`**: En modo `SelfHosted`, si el archivo o clave de licencia no existe o su firma criptográfica ha sido adulterada, `ILicenseValidator` reporta estado inválido y bloquea operaciones administrativas de mutación (`HTTP 403 Forbidden`), permitiendo únicamente endpoints de diagnóstico (`/healthz`, `/ready`) y carga de licencia. En modo `SaaS` (por defecto en nube/desarrollo), la validación de licencia perpetua local opera como permitida (`SaaS mode`).
- **Consultas `JSONB` en Canal B con SQLite**: Cualquier repositorio que utilice `EF.Functions.JsonContains` (`@>`) verifica primero `_context.Database.IsNpgsql()` (conforme al patrón establecido en `OrderRepository.HasActiveOrdersForMenuItemAsync`) y aplica evaluación compatible cuando el proveedor es SQLite.
- **Intento de superar el umbral de 1 sucursal en Canal B**: Si el modo de despliegue es Canal B (`SingleNodeWindows` o `Database:Provider=Sqlite`), el límite `max_branches = 1` es forzado incluso si se intenta crear un segundo tenant vía API.

---

## Requirements *(mandatory)*

### Functional Requirements

- **FR-001 (`ADR-0019` Sección 3 y 5)**: El sistema DEBE soportar la configuración `Deployment:Mode` (`SaaS` por defecto, `SelfHosted` para licencias perpetuas, mapeado también desde la variable de entorno `DEPLOYMENT_MODE`) y `Deployment:Channel` (`DockerCompose` para Canal A, `SingleNodeWindows` para Canal B).
- **FR-002 (`ADR-0019` Sección 5)**: El sistema DEBE definir e implementar `ILicenseValidator` en `RestoCore.Application` y `RestoCore.Infrastructure` para verificar de manera criptográfica y 100% offline licencias perpetuas firmadas (RSA/ECDSA o HMAC-SHA256 de clave pública/verificación), validando identificador de cliente, canal autorizado y `max_branches`.
- **FR-003 (`ADR-0019` Sección 5)**: En el Canal B (`SingleNodeWindows` o `max_branches = 1`), el sistema DEBE impedir la creación de más de 1 sucursal/tenant activo (`max_branches = 1`), devolviendo un error estructurado RFC 7807 que indique la necesidad de migrar al Canal A para operar múltiples sucursales.
- **FR-004 (`ADR-0019` Sección 3 - Canal B)**: El sistema DEBE soportar `Storage:Provider=LocalDisk` mediante una implementación `LocalDiskStorageService : IStorageService` para entornos de nodo único sin SeaweedFS, manteniendo `SeaweedStorageService` (`Storage:Provider=SeaweedFS`) como predeterminado para SaaS y Canal A (`ADR-0004`).
- **FR-005 (`ADR-0019` Sección 3 - Canal B y `ADR-0003`)**: `Program.cs` y `ApplicationDbContext` DEBEN permitir seleccionar el proveedor de persistencia mediante `Database:Provider` (`Postgres` por defecto conforme a `ADR-0003`, o `Sqlite` en modo WAL para Canal B), asegurando que las conversiones JSON y consultas de repositorios sean compatibles con ambos proveedores.
- **FR-006 (`ADR-0019` Sección 3 - Canal A)**: El repositorio DEBE incluir en `deploy/selfhosted/canal-a/` el paquete de despliegue para servidores y multi-terminal compuesto por `docker-compose.selfhosted.yml` (`app`, `db` PostgreSQL 16+, `storage` SeaweedFS, `redis`, `proxy` Caddy), `.env.selfhosted.template` y el script de verificación pre-vuelo `verify-preflight.ps1` / `verify-preflight.sh`.
- **FR-007 (`ADR-0019` Sección 3 - Canal B y Sección 5)**: El proyecto `RestoCore.Api` y el directorio `deploy/selfhosted/canal-b/` DEBEN incluir los perfiles y scripts de publicación de binario único sin símbolos de depuración (`PublishSingleFile=true`, `DebugType=None`, `DebugSymbols=false`) para Linux (`linux-x64` Canal A) y Windows autocontenido (`win-x64`, `SelfContained=true` Canal B), junto con el manifiesto de instalación de Inno Setup y script de respaldo automático local.

### Key Entities

- **`DeploymentOptions`**: Configuración tipada (`Mode`: `SaaS | SelfHosted`, `Channel`: `DockerCompose | SingleNodeWindows`, `LicenseFilePath`, `LicensePublicKey`).
- **`PerpetualLicenseInfo`**: Modelo inmutable resultante de `ILicenseValidator.ValidateCurrentLicense()` con `LicenseId`, `CustomerName`, `Channel`, `MaxBranches`, `IssuedAt`, `IsValid` y `FailureReason`.
- **`LocalDiskStorageService`**: Implementación de `IStorageService` para `Storage:Provider=LocalDisk` que almacena y sirve activos de catálogo desde el sistema de archivos local en instalaciones de Nodo Único (Canal B).

---

## Success Criteria *(mandatory)*

### Measurable Outcomes

- **SC-001**: `ILicenseValidator` valida firmas criptográficas de licencias perpetuas en menos de 5 ms de forma 100% offline (0 solicitudes HTTP/DNS externas).
- **SC-002**: En modo Canal B (`SingleNodeWindows` / `max_branches = 1`), el 100% de los intentos de registrar una segunda sucursal (`Tenant`) son bloqueados determinísticamente antes de persistir en la base de datos.
- **SC-003**: Tanto las pruebas unitarias como las pruebas de compatibilidad de persistencia (`Postgres` y `Sqlite` para Canal B) y almacenamiento (`SeaweedStorageService` y `LocalDiskStorageService`) compilan y pasan al 100% sin regresiones.
- **SC-004**: El analizador de sincronización documental `compare-docs.ps1` reporta **18/18 ADRs en estado `[OK]`** (incluyendo `ADR-0019` / `0019-empaquetado-distribucion-licencias-perpetuas.md`) y `0` brechas de especificación en P1, P2 y P3.

---

## Assumptions

- El modo predeterminado del backend cuando no se especifica `DEPLOYMENT_MODE=SelfHosted` continúa siendo `SaaS` con PostgreSQL (`ADR-0003`) y SeaweedFS (`ADR-0004`), garantizando cero impacto sobre el entorno de desarrollo actual y las pruebas de integración existentes.
- En el Canal B (`Storage:Provider=LocalDisk`), las URLs pre-firmadas emitidas por `LocalDiskStorageService` apuntan a un endpoint local autenticado/firmado de subida o ruta local servida por la propia instancia de la API en la red LAN del comercio.
