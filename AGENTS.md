# Directrices de Operacion para Agentes de IA en resto-core-back

Este documento define las normas operativas, metodologias y estandares tecnicos para agentes de inteligencia artificial que asistan en el desarrollo y mantenimiento del backend de **RestoCore** (`resto-core-back`).

---

## 1. Rol del Agente y Flujo de Trabajo

El agente opera bajo la metodologia **Spec-Driven Development (SDD)** y utiliza el conjunto de habilidades de **Spec-Kit** alojadas en `.agents/skills/`.

### Reglas Fundamentales de Operacion
1. **Unica Fuente de Verdad:** Ninguna funcionalidad de negocio, endpoint o modelo de persistencia debe implementarse sin una especificacion previa aprobada bajo `specs/` y alineada con los contratos de `RestoCore-Docs`.
2. **Estandar Profesional de Redaccion:** Todo documento tecnico, especificacion, comentario de codigo, commit o PR debe redactarse en tono tecnico y profesional, evitando el uso de emoticonos y jerga informal.
3. **Descripciones Exhaustivas en Pull Requests:** Todo PR generado debe incluir una explicacion detallada del contexto, decisiones tomadas, pruebas ejecutadas y posibles impactos colaterales, actuando como centro de revision y debate.

---

## 2. Inviolables de Arquitectura del Backend

Cualquier diseno o implementacion en este repositorio debe respetar de forma obligatoria los siguientes pilares tecnicos:

1. **Arquitectura de API:** Exposicion exclusiva mediante **REST API** conforme al contrato OpenAPI 3.1.
2. **Presupuesto de Latencia:** Procesamiento de solicitudes dinamicas en menos de 500ms en el servidor para garantizar el presupuesto global de carga de la carta QR (< 2s LCP movil). Endpoints de catalogo publico deben suministrar cabeceras `Cache-Control` y `ETag` para aceleracion por CDN.
3. **Persistencia:** Base de datos principal **PostgreSQL**, utilizando columnas `JSONB` e indices `GIN` para estructuras semiestructuradas y esquemas flexibles de catalogo (ADR-0003).
4. **Carga de Archivos e Imagenes:** El backend nunca transfiere binarios pesados de imagenes; emite URLs pre-firmadas temporales hacia **SeaweedFS** para subida directa desacoplada (ADR-0004).
5. **Seguridad y Autorizacion Declarativa:** Validacion de permisos desacoplada mediante politicas **OPA / Rego**, asegurando aislamiento estricto por tenant y jerarquia de roles: Cliente Anonimo, Mozo (`waiter`), Cocinero (`cook`), Dueno (`owner`), SuperAdmin (ADR-0006).
6. **Procesamiento de Comandas y Pedidos:** Ingestion asincrona con respuesta HTTP 202 Accepted y encolado secuencial FIFO con Redis Streams y notificaciones en tiempo real mediante WebSockets seguros (`wss://`) (ADR-0007).
7. **Disciplina Test-First:** Desarrollo guiado por pruebas (TDD), pruebas unitarias y pruebas de integracion obligatorias antes de dar por completada una tarea.

---

## 3. Habilidades Disponibles (.agents/skills)

El agente debe ejecutar y seguir los flujos correspondientes a las habilidades de Spec-Kit:
* `speckit-constitution`: Gobernanza y principios inmutables del proyecto.
* `speckit-specify`: Creacion de especificaciones iniciales para features.
* `speckit-clarify`: Elicitacion y desambiguacion previa al diseno.
* `speckit-plan`: Planificacion de arquitectura e investigacion tecnica.
* `speckit-checklist`: Generacion de listas de control de calidad.
* `speckit-tasks`: Desglose secuencial de tareas de implementacion.
* `speckit-analyze`: Analisis cruzado de consistencia entre especificaciones y tareas.
* `speckit-implement`: Ejecucion rigurosa del codigo guiada por tareas.
* `speckit-git-*`: Gestion estandarizada de ramas y commits.

---

<!-- SPECKIT START -->
For additional context about technologies to be used, project structure,
shell commands, and other important information, read the current plan:
specs/004-structured-logging-traces/plan.md
<!-- SPECKIT END -->
