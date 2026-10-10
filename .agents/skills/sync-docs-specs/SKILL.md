---
name: sync-docs-specs
description: "Compara la documentacion centralizada en RestoCore-Docs contra el estado actual del backend, detecta discrepancias o nuevas decisiones arquitectonicas (ADRs/contratos) y genera specs de Spec-Kit para aplicarlas."
compatibility: "Requires RestoCore-Docs repository and .specify/ structure"
metadata:
  author: "restocore-agents"
  source: "custom-skills/sync-docs-specs.md"
---

# Sync Docs to Backend Specs (`sync-docs-specs`)

Esta skill analiza y compara de forma bidireccional los contratos arquitectonicos, ADRs (Architecture Decision Records) y especificaciones en `r:\Programando\RestoCore\RestoCore-Docs` con respecto a las especificaciones y codigo implementado en el backend (`resto-core-back`).

Al detectar brechas (*deltas* o *gaps*) entre la documentacion y el backend, crea automaticamente una nueva feature specification (`spec.md`) alineada con el flujo **Spec-Driven Development (SDD)**.

---

## 1. Activacion y Entradas del Usuario

```text
$ARGUMENTS
```

Argumentos opcionales soportados:
- **`--docs-path <ruta>`**: Ruta hacia el repositorio de documentacion (por defecto `R:\Programando\RestoCore\RestoCore-Docs`).
- **`--adr <numero_o_nombre>`**: Enfoca el analisis en un ADR o documento especifico (ej. `--adr 0007` o `--adr 0010`).
- **`--auto-spec`**: Genera inmediatamente el directorio y archivo `spec.md` para la mayor brecha detectada.

---

## 2. Flujo de Ejecucion

### Paso 1: Deteccion y Comparacion de Artefactos

1. Ejecutar el script analizador de diferencias:
   ```powershell
   powershell -ExecutionPolicy Bypass -File .agents/skills/sync-docs-specs/scripts/compare-docs.ps1 -DocsPath "R:\Programando\RestoCore\RestoCore-Docs"
   ```
2. Inspeccionar las siguientes fuentes en `RestoCore-Docs`:
   - `docs/adr/`: Decisiones arquitectonicas vigentes (ADR-0001 a ADR-0012).
   - `specs/openapi.yaml`: Contrato canonico de la API.
   - `specs/policies/`: Politicas declarativas de seguridad (OPA / Rego).
   - `docs/runbooks/`: Procedimientos operativos y de carga.

3. Cruzar contra `specs/` en `resto-core-back`:
   - Verificar si el tema o requerimiento del ADR ya cuenta con una carpeta en `specs/NNN-<tema>/`.
   - Comparar las operaciones en `specs/openapi.yaml` contra los endpoints expuestos en el backend (`src/RestoCore.Api/Endpoints/`).

### Paso 2: Determinacion de Gaps Prioritarios

Clasificar las diferencias encontradas segun impacto arquitectonico:
- **P1 (Critico - Inviolable de Arquitectura):**
  - Procesamiento asincrono de pedidos con FIFO y WebSockets (ADR-0007).
  - Baja suave de platos y snapshots inmutables de comandas (ADR-0010).
- **P2 (Estructural):**
  - Optimizacion de esquemas `JSONB` e indices `GIN` (ADR-0003).
- **P3 (Gobernanza / Alcance):**
  - Postergacion de estados de mesa QR a modulo de turnos (ADR-0012).

### Paso 3: Generacion de Especificacion SDD

Si se solicita `--auto-spec` o el usuario aprueba generar la spec para una brecha seleccionada:

1. Determinar el siguiente numero de feature secuencial en `specs/` (ej. `007-<nombre-feature>`).
2. Crear el directorio `specs/NNN-<nombre-feature>/`.
3. Redactar `specs/NNN-<nombre-feature>/spec.md` respetando la plantilla `.specify/templates/spec-template.md`:
   - **User Stories** priorizadas (P1, P2, P3).
   - **Acceptance Scenarios** con formato *Given-When-Then*.
   - **Functional Requirements (FR-###)** extraidos del ADR canonico.
   - **Success Criteria (SC-###)** medibles y libres de supuestos de framework.
   - Cita explicita al ADR de origen en `RestoCore-Docs`.
4. Actualizar `.specify/feature.json` apuntando a la nueva feature.
5. Indicar al usuario que la spec esta lista para continuar con `/speckit-plan`.

---

## 3. Ejemplo de Salida y Reporte

```text
==========================================================
 RestoCore Docs vs Backend Delta Analyzer
==========================================================
[1] ADRs found in Docs (12):
  [OK]  0002-separacion-de-repositorios -> 001-multi-tenant-digital-menu
  [OK]  0005-desacoplamiento-catalogo-canvas -> 005-canvas-layout-aot-compilation
  [GAP] 0007-procesamiento-pedidos-cola-fifo-websockets.md
  [GAP] 0010-baja-suave-de-platos-y-snapshots-inmutables.md

Siguiente paso recomendado:
Crear spec specs/007-order-fifo-processing-websockets/ a partir de ADR-0007.
```
