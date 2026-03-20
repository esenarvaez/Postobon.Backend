# Postobon.Backend

README — ExpenseApproval Backend (Postobon.Backend)
Backend API para gestión de solicitudes de gasto: creación, consulta con filtros, actualización (solo si está pendiente), aprobación, rechazo y consulta de métricas. Implementado en .NET 8 con persistencia en PostgreSQL usando Entity Framework Core.

1. Requisitos
   .NET SDK 8.0
   PostgreSQL (local o remoto)
   (Opcional) Visual Studio 2022 / Rider / VS Code
   Acceso a modificar appsettings.json / appsettings.Development.json
2. Estructura del repositorio (alto nivel)
   Solución: Postobon.Backend.sln

Proyectos:

ExpenseApproval.Api → Capa HTTP (Controllers + Middleware + Swagger)
ExpenseApproval.Application → Casos de uso, DTOs, Validadores, Servicios, Excepciones
ExpenseApproval.Domain → Entidades y Enums del dominio
ExpenseApproval.Infrastructure → EF Core, DbContext, Repositorios, DI para infraestructura 3) Configuración (Connection String)
La API espera la cadena de conexión en:

ExpenseApproval.Api/appsettings.json (o appsettings.Development.json)

Clave:

ConnectionStrings:DefaultConnection
Ejemplo:

json

{
"ConnectionStrings": {
"DefaultConnection": "Host=localhost;Port=5432;Database=expense_approval;Username=postgres;Password=postgres"
}
}
Nota: el proyecto usa UseNpgsql(...) (provider de PostgreSQL).

4. Ejecución local (paso a paso)
   Desde la carpeta raíz donde está Postobon.Backend.sln:

4.1 Restaurar y compilar
bash

dotnet restore
dotnet build
4.2 Ejecutar la API
bash

dotnet run --project ExpenseApproval.Api
4.3 Probar en Swagger
Al iniciar, abre la URL que indique la consola (según launchSettings.json), por ejemplo:

https://localhost:{puerto}/swagger
http://localhost:{puerto}/swagger 5) Endpoints principales
Base: /api/solicitudes

Listar con filtros opcionales
GET /api/solicitudes?estado=Pending&categoria=Transporte&fechaDesde=2026-01-01&fechaHasta=2026-12-31

estado corresponde a RequestStatus: Pending | Approved | Rejected
fechaDesde y fechaHasta se manejan como DateOnly
Obtener por Id
GET /api/solicitudes/{id}

Crear
POST /api/solicitudes

Body:

json

{
"category": "Transporte",
"description": "Taxi reunión",
"value": 25000,
"expenseDate": "2026-03-20",
"requestedBy": "user@empresa.com"
}
Actualizar (solo si Pending)
PUT /api/solicitudes/{id}

Body:

json

{
"category": "Alimentación",
"description": "Almuerzo cliente",
"value": 50000,
"expenseDate": "2026-03-19"
}
Aprobar (solo si Pending)
POST /api/solicitudes/{id}/approve

Body:

json

{
"decisionBy": "aprobador@empresa.com"
}
Rechazar (solo si Pending)
POST /api/solicitudes/{id}/reject

Body:

json

{
"decisionBy": "aprobador@empresa.com",
"reason": "Falta soporte del gasto"
}
Métricas agregadas
GET /api/solicitudes/metrics

Devuelve:

Conteos: total, pending, approved, rejected
Valores: totalValue, approvedValue, pendingValue, rejectedValue 6) Manejo de errores (respuestas consistentes)
Se implementa un middleware global:

ExpenseApproval.Api/Middleware/GlobalExceptionHandlerMiddleware.cs

Convierte excepciones a application/problem+json:

400 Bad Request → FluentValidation.ValidationException (errores por campo)
404 Not Found → NotFoundException
409 Conflict → BusinessRuleException
500 Internal Server Error → cualquier error no controlado
Además, el log incluye TraceId para trazabilidad.

7. Supuestos del sistema
   No hay autenticación/autorización implementada.
   RequestedBy y DecisionBy se reciben en el body del request.
   La solicitud sigue un flujo simple de estados:
   Pending → Approved o Rejected
   No se contempla “reversar” decisiones ni transiciones adicionales.
   Fechas:
   ExpenseDate no puede ser futura (validación con DateOnly vs fecha UTC actual).
   La persistencia se hace en una única base de datos PostgreSQL.
   No hay paginación en el listado (GET /api/solicitudes), se retorna todo lo que coincida con filtros.
8. Decisiones técnicas (y por qué)
   8.1 Arquitectura por capas (Api / Application / Domain / Infrastructure)
   Motivo: separar responsabilidades y mantener el dominio y la lógica de negocio desacopladas de frameworks.

Api: solo HTTP (controllers) y cross-cutting (middleware).
Application: casos de uso, validación, reglas de negocio, DTOs.
Domain: entidades/enum sin dependencias externas.
Infrastructure: EF Core, DbContext, repositorios, configuración de persistencia.
8.2 DTOs + AutoMapper
Motivo: evitar exponer entidades directamente y controlar contratos de API.

ExpenseRequestCreateDto, ExpenseRequestUpdateDto, ExpenseRequestReadDto, etc.
ExpenseRequestProfile centraliza mapeos.
8.3 FluentValidation
Motivo: validación declarativa y consistente antes de ejecutar casos de uso.

Validadores: ExpenseRequestCreateValidator, ExpenseRequestUpdateValidator, DecisionValidator, RejectValidator
Helper interno: ValidateOrThrowAsync(...) para no duplicar lógica.
8.4 Reglas de negocio en ExpenseRequestService
Motivo: mantener la lógica en la capa de aplicación y que el controller sea delgado. Ejemplos:

Solo se puede aprobar/rechazar/editar si Status == Pending.
En approve/reject se registra DecisionAtUtc = DateTime.UtcNow y DecisionBy.
8.5 EF Core + PostgreSQL
Motivo: persistencia relacional estándar y consultas eficientes.

Tabla: expense_requests
Índices en campos de consulta frecuente: Status, Category, ExpenseDate, CreatedAtUtc
Value almacenado como numeric(18,2). 9) Consideraciones de calidad / mejoras futuras
Autenticación (JWT) y autorización por roles para aprobar/rechazar.
Concurrencia optimista (RowVersion) para evitar doble decisión en paralelo.
Paginación y ordenamiento configurable en GET /api/solicitudes.
Tests:
Unitarios para ExpenseRequestService (mock repositorio + validadores)
Integración (API + Postgres con Testcontainers)
Mejor control de updates:
Actualmente el repositorio usa AsNoTracking() en lecturas y Update(entity) en escritura (válido, pero marca toda la entidad como modificada). Se puede refinar para actualizar solo campos cambiados. 10) Troubleshooting (problemas comunes)
10.1 Error de conexión a DB
Verifica ConnectionStrings:DefaultConnection
Verifica que PostgreSQL esté activo y el puerto/credenciales sean correctos.
10.2 400 Validation failed
Revisa el payload contra las reglas de FluentValidation:
Value > 0
ExpenseDate no futura
longitudes máximas (Category/Description/DecisionBy/Reason)
10.3 409 Conflict al aprobar/rechazar/editar
La solicitud no está en Pending. Debe estar pendiente para permitir la operación.
