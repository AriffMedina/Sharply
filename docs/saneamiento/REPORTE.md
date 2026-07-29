# Reporte final — Saneamiento técnico

Rama `chore/saneamiento`, creada desde `Deuda_Tecnica`. 22 commits, sin tocar `master` en ningún momento.

## Impresión general

**Antes:** el respeto a la Arquitectura Hexagonal ya era real (Domain sin dependencias, Application solo depende de Domain, entidades limpias de atributos EF), pero el proyecto arrastraba el desgaste típico de crecer a los saltos entre entregas: namespaces rotos a mitad de camino, un registro de DI duplicado, vestigios de una reorganización de carpetas vieja, una contraseña real de Gmail committeada en texto plano, y — el hallazgo más serio — un `BackgroundService` (`DecayWorker`) completo pero que **nunca se ejecutaba** en ningún ambiente, y que si se activaba tal cual, fallaba en el primer ciclo por un problema de resolución de DI. El build compilaba con 5 warnings.

**Después:** namespaces unificados y coherentes en todo `Sharply.Web`, sin registros duplicados, sin vestigios de carpetas viejas, sin secretos en el repo (la clave real ya rotada y movida a User Secrets, verificado que nunca tocó el historial de git), `AuthService` desacoplado de `AppDbContext` vía `IUserRepository`, la Api con una capa de DTOs propia en vez de exponer las entidades de dominio, y `DecayWorker` reparado y activado de verdad — se probó en vivo contra la base de datos real, corrió su primer ciclo, y no explotó. Build en 0 warnings, 0 errores, en Debug y Release.

## Cambios más significativos por área

- **Arquitectura hexagonal:** `ISkillDecayService`, `IMissionService` e `IGamificationService` movidos de `Sharply.Application/Services/` a `Sharply.Domain/Interfaces/`, donde vive el resto de los puertos. `AuthService` dejó de depender de `AppDbContext` directamente — ahora inyecta `IUserRepository`, siguiendo el mismo patrón que `Skill`/`SkillLog`.
- **Contrato de la Api:** `SkillsController` y `SkillLogsController` devuelven `SkillResponse`/`SkillLogResponse` en vez de las entidades de dominio. Verificado en vivo (Api corriendo + `curl` a `swagger.json`): mismos endpoints (`GET /api/Skills`, `/api/Skills/{id}`, `/api/SkillLogs`, `/api/SkillLogs/{id}`), mismos verbos, mismos nombres/tipos de campo en camelCase (enums como enteros, igual que antes). El `swagger.json` **no** es byte-a-byte idéntico al de antes de la rama — eso era esperado y quedó aprobado de antemano.
- **`DecayWorker` reparado y activado:** resolvía un tipo concreto (`EmailService`) nunca registrado como tal, lo que garantizaba una excepción en el primer ciclo; ahora resuelve `IEmailService` (registrado) y lo castea a `ISkillDecayObserver` para el patrón Observer. Se agregó `try/catch` alrededor del ciclo para que una falla no tumbe el host completo. Se activó con `AddHostedService<DecayWorker>()` en `Sharply.Web` (no en `Sharply.Api` como decía el plan original — `Sharply.Api` no tiene `IEmailService` ni `EmailSettings` configurados; se documentó esa decisión en el momento). Probado en vivo: arrancó, ejecutó su primer ciclo contra la base real, sin errores.
- **`Sharply.Api` deja de estar "medio roto":** faltaba el registro de `IDecayStrategy`, del que depende `SkillDecayService` (sí registrado). Sin ese registro, el host ni siquiera lograba levantar en Development (falla de validación de DI al hacer `Build()`) — lo cual bloqueaba también la verificación en vivo de los DTOs hasta que se corrigió.
- **Seguridad:** la contraseña SMTP de Gmail, que estaba committeada en texto plano, fue rotada por el dueño del repo y movida a User Secrets (`dotnet user-secrets set`). Confirmado con `git log --all -p` que la clave rotada no aparece en ningún punto del historial de git.
- **Configuración:** nombre de base de datos unificado a `SharplyDb` en ambos hosts (antes difería en casing entre Web y Api). Typo corregido en el email remitente (`gmai.com` → `gmail.com`).
- **Documentación de intención:** `GamificationService`/`IGamificationService` y `LinearDecayStrategy` quedaron con comentarios XML explicando por qué existen sin estar conectados, en vez de quedar como código muerto sin explicación.

## Warnings

**Antes:** 5 (`CS8604`, todos en `EmailService.cs`, por leer configuración `string?` sin validar).
**Después:** 0.

## Paquetes tocados

Ninguno. El bloque de "actualizaciones de paquete de bajo riesgo" que proponía el `Saneamiento.md` original no llegó a la lista de 10 bloques aprobados en `RespuestaFase2.md`, así que no se tocó ningún `PackageReference`.

## Confirmaciones de verificación (Fase 5)

- ✅ `dotnet build Sharply.slnx -c Debug` → 0 errores, 0 warnings.
- ✅ `dotnet build Sharply.slnx -c Release` → 0 errores, 0 warnings.
- ✅ `dotnet format Sharply.slnx --verify-no-changes` → exit code 0.
- ⏸️ Tests: no se creó `Sharply.Tests` — Fase 4 diferida a pedido explícito del dueño del repo ("hasta que te lo diga"), no forma parte de este cierre.
- ✅ Contrato de la Api: verificación estructural manual contra la Api corriendo en vivo — endpoints, verbos y forma de cada campo JSON idénticos a los de antes de la rama (el `swagger.json` en sí cambia por los DTOs, tal como estaba aprobado).
- ✅ Modelo de datos intacto: `dotnet ef migrations add DryRun` generó `Up`/`Down` completamente vacíos. Carpeta temporal borrada, no se commiteó nada.

## Pendientes (ver `pending-review.md`)

2 ítems abiertos, ninguno urgente:

1. `LinearDecayStrategy` — implementación de referencia del patrón Strategy, no conectada a ningún flujo real. Documentada, no eliminada.
2. Asimetría de configuración entre `Sharply.Api` y `Sharply.Web` (`EmailSettings`/`Logging` solo existen en Web) — observación, no defecto.

## Cosas que quedaron sin hacer y por qué

- **Fase 4 (tests de caracterización):** diferida a pedido explícito del dueño del repo. El proyecto sigue sin cobertura de tests — vale la pena retomarlo pronto, justo se acaba de activar un `BackgroundService` real y se cambió el contrato público de la Api, que es exactamente el tipo de cambio que más se beneficia de tests de caracterización.
- **Actualización de paquetes de bajo riesgo:** quedó fuera del alcance aprobado en `RespuestaFase2.md`, no se evaluó en esta pasada.
