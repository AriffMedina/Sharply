# Diagnóstico de saneamiento técnico — Fase 1

Auditoría de solo lectura sobre `Sharply` en la rama `chore/saneamiento` (partiendo de `Deuda_Tecnica`). No se modificó código ni configuración; este documento es la base para decidir qué se corrige en la Fase 2.

## Impresión general

El respeto a la Arquitectura Hexagonal es real, no solo declarativo: `Sharply.Domain` no tiene una sola `PackageReference` ni `ProjectReference`, `Sharply.Application` solo depende de `Domain`, y las entidades (`Skill`, `User`, `SkillLog`) están limpias de atributos de EF Core — todo el mapeo vive en `AppDbContext.OnModelCreating` vía Fluent API. La dirección de las dependencias entre los 5 proyectos es correcta en todos los `.csproj`, no se encontró ningún `using Sharply.Infrastructure` colado en Domain o Application, y tampoco apareció el namespace `Domain.Entities` en ningún lado (el estándar `Domain.Models` se respeta consistentemente). El build compila sin errores y con solo 5 warnings, todos del mismo tipo y concentrados en un único archivo. No hay artefactos de build (`bin/`, `obj/`, `.vs/`, `*.user`) versionados por error — el `.gitignore` está bien armado.

Dicho esto, el proyecto tiene el desgaste típico de haber crecido a los saltos, sin refactors de limpieza: hay convenciones que se rompen a mitad de camino (namespaces de controllers, ubicación de interfaces), registros de DI duplicados o incompletos, código muerto (clases vacías, una estrategia nunca conectada), y — el hallazgo más serio — un `BackgroundService` completamente implementado (`DecayWorker`) que en este momento **nunca se ejecuta** porque no está registrado en ningún `Program.cs`, y que si se registrara tal cual, fallaría en el primer ciclo por un problema de resolución de DI. Nada de esto es catastrófico para un proyecto universitario de un solo desarrollador, pero sí amerita las decisiones humanas que se marcan abajo antes de tocar código en Fase 2.

---

## Coherencia de arquitectura hexagonal

**`Sharply.Application/Services/ISkillDecayService.cs`, `IMissionService.cs`, `IGamificationService.cs`**
Estas tres interfaces viven en `Sharply.Application.Services`, mientras que el resto de los puertos del sistema (`ISkillRepository`, `ISkillLogRepository`, `IAuthService`, `IEmailService`, `IDecayStrategy`, `ISkillDecayObserver`, `ISkillDecaySubject`) viven en `Sharply.Domain.Interfaces`. No hay una regla visible que explique por qué estas tres son la excepción — rompe la convención dominante del propio proyecto sobre dónde viven los puertos. `[REVISAR]`

**`Sharply.Infrastructure/Services/AuthService.cs`**
`AuthService` inyecta `AppDbContext` directamente y opera sobre `_context.Users` sin pasar por un repositorio, a diferencia de `Skill` y `SkillLog`, que sí tienen `ISkillRepository`/`ISkillLogRepository` con sus implementaciones. No existe `IUserRepository` en ningún lado del código. Es una inconsistencia del patrón repository ya establecido en el resto del proyecto. `[REVISAR]`

**`Sharply.Infrastructure/Jobs/DecayWorker.cs` (línea 43)**
`RunDecayCheckAsync` resuelve el tipo **concreto** `EmailService` con `scope.ServiceProvider.GetRequiredService<EmailService>()`. En ambos `Program.cs` (Web y Api), `EmailService` solo está registrado como `AddScoped<IEmailService, EmailService>()`. El contenedor de DI de ASP.NET Core no resuelve un tipo concreto a partir de un registro por interfaz: si este método llegara a ejecutarse, lanzaría `InvalidOperationException` en el primer ciclo. `[BLOQUEANTE]`

---

## Coherencia de namespaces y carpetas

**`Sharply.Web/Controllers/AccountController.cs`**
No declara ningún `namespace` — la clase queda en el namespace global. Es el único archivo `.cs` de todo el proyecto en esa situación. `[MECANICO]`

**`Sharply.Web/Controllers/HomeController.cs`, `Sharply.Web/Controllers/SkillsController.cs`, `Sharply.Web/Models/ErrorViewModel.cs`**
Usan `namespace Sharply.Controllers` y `namespace Sharply.Models` respectivamente — les falta el segmento `.Web.` que sí usan correctamente los ViewModels (`Sharply.Web.ViewModels`) y que sí usa `Sharply.Api` en sus controllers (`Sharply.Api.Controllers`). Dentro del mismo proyecto conviven tres convenciones de namespace distintas para carpetas equivalentes (`Controllers`, `Models`, `ViewModels`). `Sharply.Web/Views/_ViewImports.cshtml` ya tiene `@using Sharply` y `@using Sharply.Models`, coherente con la convención rota — habría que actualizarlo junto con el fix. `[MECANICO]`

**`Sharply.Api/Controllers/SkillLogController.cs`**
El archivo se llama `SkillLogController.cs` pero la clase declarada adentro es `SkillLogsController` (en plural, como su ruta `[Route("api/[controller]")]` → `api/skilllogs`). Nombre de archivo y nombre de clase no coinciden. `[MECANICO]`

---

## Coherencia de nombres

No se encontraron problemas de fondo: los campos privados siguen `_camelCase` de forma consistente, los métodos async llevan el sufijo `Async` en el 100% de los casos revisados, las propiedades son PascalCase en todos los modelos/DTOs/ViewModels. No hay hallazgo que anotar en esta área más allá de lo ya cubierto en namespaces.

**`Sharply.Application/Services/LinearDecayStrategy.cs`**
`Calculate(double initialRetention, int daysInactive, MasteryLevel mastery, SkillPriority priority)` recibe `priority` pero nunca lo usa en el cuerpo del método, a diferencia de `EbbinghausDecayStrategy.Calculate`, que sí aplica un multiplicador según prioridad con la misma firma. Puede ser intencional (una estrategia deliberadamente "ciega" a la prioridad) o un olvido — requiere criterio de quien diseñó el patrón Strategy acá. `[REVISAR]`

---

## Repetición, contradicciones y código muerto

**`Sharply.Application/Services/GamificationService.cs`, `IGamificationService.cs`**
Ambos son una clase y una interfaz `internal` completamente vacías (sin un solo miembro), no registradas en ningún `Program.cs`, no referenciadas desde ningún otro archivo. Scaffolding muerto. `[REVISAR]` (decidir si se borra o se completa es una decisión de alcance/roadmap, no un arreglo mecánico)

**`Sharply.Application/Services/LinearDecayStrategy.cs`**
Implementa `IDecayStrategy` pero nunca se registra como implementación en ningún `Program.cs` (solo `EbbinghausDecayStrategy` se registra, y solo en `Sharply.Web`) ni se instancia en ningún otro punto del código. Es una segunda estrategia completa que hoy no se puede alcanzar desde ningún flujo real. `[REVISAR]`

**`Sharply.Web/Program.cs` (líneas 32 y 34)**
```csharp
builder.Services.AddScoped<ISkillDecayService, SkillDecayService>();
builder.Services.AddScoped<IDecayStrategy, EbbinghausDecayStrategy>();
builder.Services.AddScoped<ISkillDecayService, SkillDecayService>();
```
`AddScoped<ISkillDecayService, SkillDecayService>()` está registrado dos veces, de forma idéntica. `[MECANICO]`

**`Sharply.Web/Sharply.Web.csproj` (líneas 30-31)**
```xml
<Folder Include="src\" />
<Folder Include="src\" />
```
El mismo `<Folder Include>` duplicado. `[MECANICO]`

**`Sharply.Web/Sharply.Web.csproj` (líneas 11-14)**
```xml
<Compile Remove="Sharply.Api\**\*.cs" />
<Compile Remove="Sharply.Domain\**\*.cs" />
<Compile Remove="Sharply.Application\**\*.cs" />
<Compile Remove="Sharply.Infrastructure\**\*.cs" />
```
Ninguna de esas cuatro carpetas existe físicamente dentro de `Sharply.Web/` — son globs que hoy no matchean nada. Todo indica que son un vestigio de antes del commit `4ada21c` ("refactor: reorganización de carpetas del proyecto"), cuando esos proyectos probablemente estaban anidados dentro de `Sharply.Web`. Quitarlos no cambia el build (ya no excluyen nada). `[MECANICO]`

**`Sharply.Api/WeatherForecast.cs`**
Es la clase de ejemplo por defecto del template `webapi` de .NET. No hay ningún `WeatherForecastController` ni referencia a esta clase en el resto del código. `[MECANICO]`

---

## Warnings de compilación y estado de Nullable

`Nullable` está activado (`<Nullable>enable</Nullable>`) de forma uniforme en los 5 `.csproj`. No se encontró ningún uso del operador null-forgiving (`!`) fuera de los casos habituales de inicialización de navegación EF (`= null!;` en `Skill.User`, `SkillLog.Skill`), que es el patrón estándar y no amerita hallazgo.

`dotnet build Sharply.slnx` compila sin errores, con exactamente 5 warnings, todos `CS8604` (posible argumento nulo) y todos concentrados en:

**`Sharply.Infrastructure/Messaging/EmailService.cs` (líneas 25, 39, 39, 40, 40)**
`_config["EmailSettings:SenderEmail"]`, `_config["EmailSettings:SmtpPort"]` y `_config["EmailSettings:Password"]` devuelven `string?` (el indexador de `IConfiguration`) y se pasan sin validar a `MailboxAddress(...)`, `int.Parse(...)`, `ConnectAsync(...)` y `AuthenticateAsync(...)`, que esperan `string` no nulo. Agregar una validación explícita (o un null-check con excepción clara si falta configuración) resuelve los 5 warnings sin alterar el comportamiento en el camino feliz. `[MECANICO]`

---

## Configuración

**`Sharply.Web/appsettings.json` (línea 17)**
```json
"Password": "ceepjrhjqdqfarhn"
```
Contraseña de aplicación de Gmail en texto plano, versionada en el repo. El `.csproj` de `Sharply.Web` ya tiene `<UserSecretsId>` configurado, así que el mecanismo para sacarla de acá (User Secrets en desarrollo, variables de entorno o un vault en producción) ya está disponible — falta usarlo. Mover un secreto real y decidir si hay que rotarlo es una decisión humana, no un find-and-replace. `[BLOQUEANTE]`

**`Sharply.Web/appsettings.json` (línea 16)**
```json
"SenderEmail": "sharply.reminders@gmai.com"
```
Typo en el dominio: `gmai.com` en vez de `gmail.com`. Este mismo valor se usa como `From` del mensaje y como `userName` en `AuthenticateAsync` contra el servidor `smtp.gmail.com` — si la cuenta real es `@gmail.com`, la autenticación SMTP fallaría en cada intento real de envío. Puede ser un alias intencional que no puedo confirmar sin acceso a la cuenta real. `[REVISAR]`

**Connection strings — nombre de base de datos**
`Sharply.Web/appsettings.json`: `Database=SharplyDB` (`Server=(localdb)\mssqllocaldb`). `Sharply.Api/appsettings.json`: `Database=SharplyDb` (`Server=(localdb)\MSSQLLocalDB`). Difieren solo en mayúsculas/minúsculas del nombre de base; con la collation por defecto de SQL Server (case-insensitive) probablemente resuelven al mismo objeto, pero es una inconsistencia de configuración que debería unificarse explícitamente en vez de confiar en el comportamiento de la collation. `[REVISAR]`

**`Sharply.Api/appsettings.json`**
No tiene sección `EmailSettings` ni bloque `Logging` propio (a diferencia de Web). Consistente con que Api no envía correos, pero vale dejarlo anotado como asimetría entre ambos hosts. Sin etiqueta — es una observación, no un defecto.

---

## Registro en DI (`Program.cs` de Web y de Api)

**`Sharply.Api/Program.cs` (líneas 19-20)**
```csharp
builder.Services.AddScoped<ISkillDecayService, SkillDecayService>();
builder.Services.AddScoped<IMissionService, MissionService>();
```
Ningún controller de `Sharply.Api` inyecta `ISkillDecayService` ni `IMissionService` (no existe un `MissionsController`; `SkillsController` y `SkillLogController` solo usan `ISkillRepository`/`ISkillLogRepository`). Además, `SkillDecayService` depende de `IDecayStrategy` en su constructor, y `IDecayStrategy` **no está registrado en `Sharply.Api/Program.cs`** (solo en `Sharply.Web/Program.cs`). Si en el futuro un controller de Api llegara a inyectar `ISkillDecayService`, la aplicación fallaría al arrancar ese endpoint con `InvalidOperationException: Unable to resolve service for type 'IDecayStrategy'`. Hoy es un registro sin consumidor con una dependencia rota debajo; mañana es un bug de arranque. `[BLOQUEANTE]`

**`Sharply.Infrastructure/Jobs/DecayWorker.cs`**
Es un `BackgroundService` completo (con `ILogger`, scope por ciclo, recorrido de usuarios y notificación vía `SkillDecayNotifier`), pero **no aparece ningún `AddHostedService<DecayWorker>()` en todo el repositorio** (confirmado por búsqueda global). El worker nunca se agrega al pipeline de hosting: hoy, en ningún ambiente, se ejecuta. `[BLOQUEANTE]`

**`Sharply.Infrastructure/Jobs/DecayWorker.cs` — `ExecuteAsync`**
El bucle `while (!stoppingToken.IsCancellationRequested)` llama a `RunDecayCheckAsync()` sin ningún `try/catch` alrededor. Desde .NET 6, una excepción no controlada dentro de un `BackgroundService` detiene el host completo por defecto (`BackgroundServiceExceptionBehavior.StopHost`). Combinado con el hallazgo de `EmailService` sin registrar como tipo concreto, el primer ciclo real tumbaría toda la aplicación. `[REVISAR]`

**Lifetimes**
Todos los servicios (repos, `AppDbContext`, servicios de aplicación) están registrados como `Scoped` de forma coherente entre sí — apropiado para una app request-scoped, sin mezclas de `Singleton`/`Transient` sin justificación aparente. Sin hallazgo.

---

## EF Core y migraciones

Hay 3 migraciones (`InitialCreate`, `AddUserPasswordAndSetupDB`, `SyncModelChanges`) con nombres descriptivos y fechas coherentes con el historial de commits; no se ven migraciones de prueba abandonadas ni renombradas a mano.

**`Sharply.Infrastructure/Data/AppDbContext.cs` (líneas 1-4)**
```csharp
using System;
using System.Collections.Generic;
using System.Reflection.Emit;
using System.Text;
```
Ninguno de estos cuatro `using` se usa en el archivo (el `using Microsoft.EntityFrameworkCore` y `using Sharply.Domain.Models` sí). `[MECANICO]`

`OnModelCreating` está limpio: solo relaciones (`HasOne`/`WithMany`/`HasForeignKey`) y conversiones de enum a string, sin lógica de negocio. Las entidades de Domain no tienen atributos de EF Core (`[Key]`, `[Column]`, etc.) — el mapeo vive donde corresponde. Sin hallazgo.

---

## API y Swagger

`Sharply.Api/Controllers/SkillsController.cs` y `SkillLogController.cs` (clase `SkillLogsController`) usan `[ApiController]` + `[Route("api/[controller]")]` + `[HttpGet]`/`[HttpGet("{id}")]` de forma consistente entre sí, con `ActionResult<T>` y `Ok()`/`NotFound()` como únicos tipos de retorno. Swagger está bien configurado en `Program.cs` con `SwaggerDoc` y metadata propia.

**Ausencia de DTOs de request/response**
Ambos controllers de Api devuelven las entidades de dominio (`Skill`, `SkillLog`) directamente como cuerpo de la respuesta, sin una capa de DTO intermedia. No es un bug — es una decisión de diseño válida para un proyecto de este tamaño — pero expone el modelo de persistencia tal cual al contrato público de la API (incluidas las colecciones de navegación si el serializer no las corta). `[REVISAR]`

---

## Manejo de errores y logging

El único `try/catch` de todo el código (`Sharply.Web/Controllers/HomeController.cs`, método `SendTestEmail`) captura la excepción del envío de correo y la traduce a un mensaje de usuario sin tragársela ni perder información — es el patrón correcto. `DecayWorker` es el único punto que usa `ILogger<T>`; no se encontró ningún `Console.Write`/`Console.WriteLine` en el código de producción, ni comentarios `TODO`/`FIXME`/`HACK`. Sin hallazgos adicionales a los ya listados en la sección de DI (el `try/catch` faltante en `DecayWorker.ExecuteAsync`).

---

## Tests

No existe ningún proyecto de test en la solución: `Sharply.slnx` solo referencia los 5 proyectos de producción (`Domain`, `Application`, `Infrastructure`, `Web`, `Api`), y no hay ninguna carpeta `*.Tests`/`*.Test` en el repo. Confirmado, sin profundizar más por indicación explícita. `[REVISAR]` (decidir framework y alcance de la primera suite excede el saneamiento en sí)

---

## Higiene de git

El `.gitignore` es el estándar de Visual Studio/.NET (de gitignore.io) y cubre `bin/`, `obj/`, `.vs/`, `*.user` correctamente — `git ls-files` confirma que ninguno de esos artefactos está trackeado. Sin hallazgos.

---

## Recomendaciones que exceden el saneamiento

Estas ideas surgieron durante la auditoría pero **no son parte de la Fase 1 ni deberían colarse en la Fase 2** (que es limpieza sin cambiar comportamiento): valdría la pena, en otro momento, introducir `MediatR` o un patrón CQRS liviano si `Sharply.Application` sigue creciendo con más casos de uso; agregar `ILogger` a los controllers y servicios de Web/Api más allá de `DecayWorker`, hoy el único punto instrumentado; definir DTOs de request/response explícitos en `Sharply.Api` en vez de exponer las entidades de `Domain.Models` directamente; y, una vez resuelto el registro de `DecayWorker`, envolver su ciclo en una política de reintentos/circuit breaker en vez de un `try/catch` simple, dado que depende de un servicio externo (Gmail SMTP) que puede fallar de forma transitoria.
