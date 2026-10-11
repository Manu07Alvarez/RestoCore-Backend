---
name: consult-docs
description: "Consulta y extrae especificaciones, decisiones de arquitectura (ADRs), contratos OpenAPI 3.1, reglas de negocio EARS, políticas OPA y requisitos de producto desde RestoCore-Docs y sus grafos de conocimiento de Graphify cada vez que falte información técnica, contratos de endpoints, modelos de datos o lógica de negocio."
compatibility: "Requiere acceso al repositorio RestoCore-Docs (R:\Programando\RestoCore-Docs)"
metadata:
  author: "restocore-architecture"
  source: "RestoCore-Docs"
---

# Consulta de Documentación Arquitectónica (`consult-docs`)

Esta skill es el punto de referencia canónico para inspeccionar y resolver dudas sobre contratos, especificaciones y reglas de negocio del ecosistema **RestoCore** alojadas en `r:\Programando\RestoCore-Docs`.

Debe activarse **inmediatamente** cada vez que:
1. Se desconozca la estructura de un endpoint, método HTTP, códigos de estado o formato de payload DTO.
2. Exista ambigüedad sobre el ciclo de vida de una entidad (ej. baja suave de platos, períodos de retención, estados de comanda).
3. Se requiera verificar una regla de negocio formal o cláusula EARS (Event-Driven, State-Driven, Unwanted Behavior).
4. Se necesite comprobar decisiones de persistencia (PostgreSQL, índices GIN, SeaweedFS, Redis Streams, OPA).
5. Se busque el comportamiento acordado en los Product Requirement Documents (PRDs).

---

## 1. Mapa de Fuentes en `RestoCore-Docs`

Ruta base: `r:\Programando\RestoCore-Docs\`

| Categoría | Ruta Relativa | Contenido y Propósito |
| :--- | :--- | :--- |
| **Contrato de API** | `specs/openapi.yaml` | Especificación canónica OpenAPI 3.1 con todos los endpoints, DTOs, esquemas de error (`ProblemDetails`) y códigos HTTP. |
| **Decisiones Arquitectónicas** | `docs/adr/` | Registros inmutables (ADR-0001 a ADR-0012) que justifican el porqué de cada pilar técnico (ADR-0007 pedidos FIFO, ADR-0010 baja suave, etc.). |
| **Reglas de Negocio (EARS)** | `docs/ears-menu.md` | Requisitos funcionales y operacionales expresados bajo la sintaxis EARS formal. |
| **Requisitos de Producto** | `docs/prd-admin-crud.md`<br>`docs/prd-menu-qr.md`<br>`docs/prd-comanda-cocina.md` | PRDs con alcance funcional para el panel administrativo, carta pública móvil y cocina (KDS). |
| **Seguridad y Autorización** | `specs/policies/authz.rego` | Políticas declarativas de control de acceso basadas en atributos y roles (OPA / Rego). |
| **Glosario de Dominio** | `docs/domain-glossary.md` | Definición uniforme de conceptos (`Tenant`, `MenuItem`, `ModifierGroup`, `Order`, `TableToken`). |
| **Grafo de Conocimiento** | `graphify-out/` | Grafo relacional procesado por Graphify (`GRAPH_REPORT.md` y `graph.json`) para consultar arquitecturas complejas. |

---

## 2. Flujo de Ejecución Paso a Paso

### Paso 1: Búsqueda Rápida de Referencias
Utilizar el script automatizado para localizar los archivos y líneas relevantes:

```powershell
powershell -ExecutionPolicy Bypass -File .agents/skills/consult-docs/scripts/search-docs.ps1 -Query "<termino_o_concepto>" -Section "<seccion>"
```

Parámetros soportados para `-Section`:
* `all` (predeterminado): busca en todas las fuentes de documentación.
* `adr`: filtra exclusivamente en `docs/adr/`.
* `openapi`: busca en el contrato `specs/openapi.yaml`.
* `ears`: busca en `docs/ears-*.md`.
* `prd`: busca en los PRDs de producto `docs/prd-*.md`.
* `graphify`: busca en los reportes del grafo `graphify-out/`.

**Ejemplos:**
```powershell
# Buscar ciclo de vida y retención de baja suave en los ADRs:
powershell -ExecutionPolicy Bypass -File .agents/skills/consult-docs/scripts/search-docs.ps1 -Query "baja suave" -Section "adr"

# Buscar endpoints de modificadores en OpenAPI:
powershell -ExecutionPolicy Bypass -File .agents/skills/consult-docs/scripts/search-docs.ps1 -Query "modifiers" -Section "openapi"

# Buscar reglas de negocio sobre mesas y QR:
powershell -ExecutionPolicy Bypass -File .agents/skills/consult-docs/scripts/search-docs.ps1 -Query "table_token" -Section "ears"
```

### Paso 2: Lectura Dirigida del Documento
Una vez identificado el archivo canónico:
1. Usar `view_file` para inspeccionar la sección correspondiente con suficiente contexto.
2. Extraer las cláusulas normativas:
   * **Método y Ruta HTTP:** (ej. `DELETE /api/v1/admin/items/{id}`).
   * **Códigos de Respuesta:** (ej. `200 OK`, `409 Conflict`).
   * **Estructura del Payload:** DTOs de entrada y salida con tipos de datos exactos.
   * **Plazos y Reglas Temporales:** (ej. "retención de 7 días", "purga física mediante cron job").
   * **Mensajes al Usuario:** Textos aprobados en lenguaje no técnico.

### Paso 3: Consulta en el Grafo de Graphify (Opcional)
Si la duda involucra interconexiones complejas entre componentes, módulos o servicios:
1. Inspeccionar `r:\Programando\RestoCore-Docs\graphify-out\GRAPH_REPORT.md`.
2. O consultar conceptos mediante el CLI de graphify en la ruta de documentación:
   ```powershell
   graphify query "<pregunta o concepto>"
   ```

### Paso 4: Aplicación y Citado de la Fuente
Al responder al usuario o implementar el código correspondiente:
1. **Citar la fuente exacta:** Mencionar el archivo y número de ADR/cláusula (ej. `ADR-0010: Baja Suave de Platos`, `specs/openapi.yaml#/paths/~1api~1v1~1admin~1items~1{id}`).
2. **Respetar el contrato estricto:** No inventar rutas ni formatos que contradigan el contrato de `RestoCore-Docs`.

---

## 3. Ejemplo Práctico de Resolución

**Caso de Uso:** Se desconoce cómo debe reaccionar el sistema si se intenta borrar un plato que tiene pedidos pendientes en cocina.

1. **Búsqueda:**
   ```powershell
   powershell -ExecutionPolicy Bypass -File .agents/skills/consult-docs/scripts/search-docs.ps1 -Query "pedidos activos" -Section "adr"
   ```
2. **Hallazgo en ADR-0010:**
   > *"Verifica que el plato no se encuentre en pedidos abiertos o en preparación en cocina (KDS). De existir, responde con HTTP 409 Conflict y advertencia comprensible no técnica."*
3. **Decisión Técnica:** Implementar manejo del status code `409 Conflict` mostrando la advertencia amigable sin eliminar el plato.
