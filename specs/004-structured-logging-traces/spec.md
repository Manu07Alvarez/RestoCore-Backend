# Feature Specification: Structured Logging, Distributed Traces and Correlation

**Feature Branch**: `004-structured-logging-traces`

**Created**: 2026-09-09

**Status**: Ready

**Input**: User description: "analiza la carpeta R:\Programando\RestoCore-Docs y de ahi centrate en como se explica el estructurado de logs y trazas que la aplicacion tiene que utilizar"

## User Scenarios & Testing *(mandatory)*

### User Story 1 - Observabilidad y Diagnóstico Forense de Incidentes por Tenant (Priority: P1)

Como ingeniero de operaciones y soporte técnico de RestoCore, necesito consultar logs estructurados indexados por restaurante (`tenant_id`) y código de error, de modo que pueda identificar la causa raíz de un fallo en producción de forma inmediata sin revisar texto plano desordenado.

**Why this priority**: Es el pilar fundamental para garantizar la estabilidad operativa del backend en producción y resolver fallos de clientes con el menor tiempo de recuperación posible (MTTR).

**Independent Test**: Puede validarse completamente provocando una excepción de negocio o fallo de base de datos y verificando que el evento se registre en formato estructurado conteniendo `tenant_id`, `http.status_code`, `error.code` y el detalle del incidente.

**Acceptance Scenarios**:

1. **Given** un comensal accediendo a la Carta QR de un restaurante, **When** ocurre un fallo de validación o excepción no controlada, **Then** el sistema emite un evento estructurado en formato JSON con timestamp UTC, severidad, identificador del tenant (`tenant_id`), código de respuesta HTTP y código unívoco de error.
2. **Given** un operador consultando el motor de logs, **When** aplica un filtro por `tenant_id` y `http.status_code >= 500`, **Then** obtiene exclusivamente los registros estructurados correspondientes a dicho restaurante.

---

### User Story 2 - Salto Bidireccional entre Métricas de Latencia y Causa Raíz mediante Trazas (Priority: P2)

Como ingeniero de confiabilidad de sitio (SRE), necesito navegar desde una gráfica de degradación de latencia o span fallido hacia el log exacto asociado, utilizando la correlación estricta del estándar W3C (`trace_id` y `span_id`), para saber con precisión qué consulta o servicio generó el cuello de botella.

**Why this priority**: Permite auditar el cumplimiento estricto del presupuesto de latencia (< 500ms en backend y < 2s LCP en carta móvil) e identificar instantáneamente qué componente degradó el rendimiento.

**Independent Test**: Puede comprobarse ejecutando una solicitud HTTP que invoque persistencia o servicios externos, confirmando que todos los spans compartan el mismo `trace_id` y que cualquier log emitido durante la solicitud contenga dicho `trace_id` y su `span_id`.

**Acceptance Scenarios**:

1. **Given** una solicitud entrante HTTP con o sin encabezado de contexto distribuido, **When** se procesa en el backend a través de Minimal APIs, controladores o middleware, **Then** se inicializa o propaga un árbol de spans con un `trace_id` consistente y spans hijos para persistencia (PostgreSQL) y almacenamiento (SeaweedFS).
2. **Given** un log estructurado emitido durante la ejecución de cualquier etapa, **When** se examinan sus atributos contextuales, **Then** contiene obligatoriamente los campos `trace_id` y `span_id` coincidentes con la traza activa.

---

### User Story 3 - Cumplimiento de Políticas Anti-Redundancia y Protección de Privacidad (Priority: P3)

Como auditor de seguridad y arquitectura de la plataforma, necesito garantizar que el sistema no emita logs informativos superfluos de inicio/fin de método y que nunca se almacene información personal identificable (PII) ni credenciales en los registros de telemetría.

**Why this priority**: Evita la saturación y sobrecostos de almacenamiento en motores de agregación de logs y garantiza el estricto cumplimiento normativo de privacidad de comensales y operadores.

**Independent Test**: Puede auditarse revisando la totalidad de los eventos registrados durante la ejecución de las suites de prueba de carga y verificando la ausencia de mensajes triviales de entrada/salida de métodos, así como la ausencia de tokens de autenticación, contraseñas o datos de pago.

**Acceptance Scenarios**:

1. **Given** la ejecución regular de cualquier operación de negocio, **When** se procesa la solicitud, **Then** el tiempo de ejecución se mide exclusivamente a través de la duración de los spans de la traza distribuida, absteniéndose de emitir logs de inicio y fin con fines cronométricos.
2. **Given** una solicitud con cabeceras de autorización o payloads de autenticación, **When** se formatea cualquier evento de auditoría o error, **Then** el sistema suprime contraseñas, secretos, tokens en texto plano y datos personales de comensales.

---

### Edge Cases

- **Solicitudes Huérfanas sin Tenant Context**: Solicitudes a endpoints de sistema (`/healthz`, `/ready`, `/swagger`) o solicitudes malformadas antes de resolver el tenant deben registrar `tenant_id` como ausente o `system` sin fallar la instrumentación.
- **Interrupción o Caída del Colector de Telemetría**: La indisponibilidad del colector OTLP o backend de logs no debe bloquear el flujo HTTP ni degradar la experiencia de usuario comensal.
- **Excepciones Críticas en Middleware Inicial**: Fallos tempranos en la canalización HTTP deben capturarse con el `trace_id` generado a nivel de servidor web y asociarse a una traza de error en el span raíz.

## Requirements *(mandatory)*

### Functional Requirements

- **FR-001 (EARS - Ubiquitous)**: El sistema DEBERÁ mantener una separación formal entre registros discretos de eventos (Logs) y grafos de duración acumulada del ciclo de vida de la solicitud (Trazas).
- **FR-002 (EARS - Ubiquitous)**: El sistema DEBERÁ emitir todos los logs de aplicación en formato estructurado (JSON) incluyendo `timestamp` (ISO 8601 UTC), `level`, `tenant_id`, `http.status_code` y `error.code`.
- **FR-003 (EARS - Event-Driven)**: CUANDO se atiende cualquier solicitud HTTP, el sistema DEBERÁ inicializar o propagar el contexto distribuido mediante un identificador unívoco de traza (`trace_id`) y delimitar las etapas internas en unidades jerárquicas de trabajo (`spans`).
- **FR-004 (EARS - Event-Driven)**: CUANDO se emita un log estructurado durante el ciclo de vida de una solicitud, el sistema DEBERÁ incorporar obligatoriamente los atributos `trace_id` y `span_id` correlacionados con la traza activa.
- **FR-005 (EARS - Ubiquitous)**: El sistema DEBERÁ delegar la medición de inicio, fin y latencia acumulada exclusivamente a la instrumentación de spans de trazas distribuidas, aplicando una política estricta anti-redundancia que prohíbe logs dedicados únicamente a cronometrar métodos.
- **FR-006 (EARS - Unwanted Behavior)**: SI un evento de log procesa parámetros de usuario, cabeceras o cargas útiles, ENTONCES el sistema DEBERÁ suprimir u ofuscar cualquier dato personal identificable (PII), contraseñas, credenciales y tokens en texto plano.
- **FR-007 (EARS - State-Driven)**: MIENTRAS una solicitud ejecuta consultas hacia base de datos o almacenamiento de objetos, el sistema DEBERÁ generar spans hijos especializados enriquecidos con atributos de infraestructura (`db.system`, comandos normalizados o llamadas HTTP salientes).

### Key Entities *(include if feature involves data)*

- **Structured Log Event**: Evento puntual e inmutable que describe un hecho relevante en el sistema. Atributos clave: `timestamp`, `level`, `message`, `tenant_id`, `http.status_code`, `error.code`, `trace_id`, `span_id`.
- **Distributed Trace**: Grafo acíclico dirigido que registra el recorrido distribuido completo de una operación a través de componentes y límites de red. Atributo clave: `trace_id`.
- **Trace Span**: Intervalo delimitado de trabajo dentro de una traza. Atributos clave: `span_id`, `parent_span_id`, `name`, `start_time`, `end_time`, `duration`, `status`, `attributes` (`service.name`, `tenant.id`, `db.system`).

## Success Criteria *(mandatory)*

### Measurable Outcomes

- **SC-001**: El 100% de los logs emitidos durante la atención de solicitudes HTTP incluyen los identificadores de correlación `trace_id` y `span_id`.
- **SC-002**: El 100% de los logs de solicitudes asociadas a un restaurante incluyen el atributo estructurado `tenant_id`.
- **SC-003**: Reducción del tiempo medio de diagnóstico y aislamiento de incidentes (MTTR) permitiendo saltar de una traza con latencia anómala al log de excepción en menos de 10 segundos en las consolas operativas.
- **SC-004**: Cero registros de logs redundantes con mensajes exclusivos de entrada y salida cronométrica de métodos en la base de código.
- **SC-005**: Cero incidentes o alertas por fuga de datos personales (PII) o credenciales sensibles en los depósitos de logs.

## Assumptions

- Se utiliza el estándar universal W3C Trace Context (`traceparent`) para la interoperabilidad y propagación entre límites de red.
- La infraestructura de desarrollo y producción cuenta con colectores compatibles con el protocolo OTLP (OpenTelemetry Protocol).
- La carta QR del cliente opera en modo anónimo, por lo que las solicitudes públicas no asocian `user.id`, manteniendo el foco de contexto en `tenant_id`.
