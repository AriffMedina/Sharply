# ADR-01: Deuda Técnica Identificada en Sharply

| Campo  | Valor |
|--------|-------|
| Autor  | Ariff Medina |
| Fecha  | 15/07/2026 |
| Estado | `Aceptado` |

---

## Contexto

Sharply es una app en .NET 10.0 para tracking de habilidades, con arquitectura por capas (`Domain`, `Application`, `Infrastructure`, `Web`, `Api`). El sistema depende de una base de datos SQL Server LocalDB compartida y de un servicio de correo (Gmail) para enviar alertas de decaimiento de habilidades. Durante una revisión del proyecto se identificaron 2 deudas técnicas de tipos distintos, con el objetivo de dejarlas documentadas antes de que su costo siga creciendo:

| Deuda | Tipo | Dónde vive |
|---|---|---|
| Configuración y secretos no centralizados | Infraestructura | `Sharply.Web/appsettings.json`, `Sharply.Api/appsettings.json` |
| Datos de demo mezclados con el flujo real | Deliberada | `HomeController.cs` |

Ambas deudas conviven en el proyecto sin que nadie las haya tratado como un problema formal hasta ahora, aunque una de ellas (la de infraestructura) ya se manifestó como un incidente real durante el desarrollo.

---

## Deuda 1 — De Infraestructura: Secretos y configuración duplicada

**Qué es**
- `Sharply.Web` y `Sharply.Api` tienen `ConnectionStrings` distintas: `SharplyDB` vs `SharplyDb` (dos bases diferentes para LocalDB, aunque el nombre parezca casi igual a simple vista).
- `Sharply.Web/appsettings.json` tiene la contraseña de la cuenta de Gmail usada para notificaciones **en texto plano, commiteada en git** (no está en `.gitignore`).
- No existe un solo lugar de verdad para la configuración: cada proyecto mantiene su propia copia, y nada obliga a que ambas coincidan.

**Por qué existe**
- Nadie lo decidió. Cada proyecto (`Web`, `Api`) generó su propio `appsettings.json` de forma independiente al crearse, probablemente por plantillas de scaffolding distintas, y nunca se centralizó la configuración compartida entre ambos. Es el tipo de deuda que crece por descuido acumulado más que por una decisión puntual: cada vez que alguien agregó un valor de configuración, lo hizo en el archivo del proyecto en el que estaba trabajando, sin revisar si el otro proyecto ya tenía (o necesitaba) ese mismo valor.

**Costo de no pagarla**

| Riesgo | Ya pasó? |
|---|---|
| Migración aplicada a una base pero no a la otra → login roto (`SqlException: Invalid column name 'PasswordHash'`) | ✅ Sí |
| Password de Gmail usable por cualquiera con acceso al repo/historial | ⚠️ Activo |
| Futuros cambios de esquema se desincronizan de nuevo entre Web/Api | 🔜 Si no se corrige |

Este no es un riesgo hipotético: ya causó un bug concreto. Cuando se aplicó la migración `AddUserPasswordAndSetupDB` (que agrega la columna `PasswordHash` a la tabla `Users`), solo se ejecutó contra una de las dos bases. Como `Sharply.Web` lee de la otra, el login empezó a fallar con un error de columna inexistente — un síntoma que, sin conocer la causa raíz, es fácil de confundir con un bug de código cuando en realidad es un problema de configuración desalineada. Si esta deuda no se paga, cualquier futura migración corre el mismo riesgo de aplicarse solo a un lado, generando bugs intermitentes y difíciles de diagnosticar porque el código en sí no cambió. Por otro lado, la exposición del password de Gmail es un riesgo de seguridad independiente y ya vigente: no depende de que ocurra un evento futuro, la contraseña ya está en el historial del repositorio en este momento.

**Propuesta de solución**
1. `dotnet user-secrets` en desarrollo, variables de entorno en despliegue, para que ningún secreto vuelva a vivir dentro del código fuente versionado.
2. Una sola connection string compartida entre `Web` y `Api`, leída desde el mismo origen, de modo que sea estructuralmente imposible que ambos proyectos apunten a bases distintas.
3. Rotar la contraseña de Gmail ya expuesta, ya que sacarla del archivo no invalida la que quedó registrada en commits anteriores.

---

## Deuda 2 — Deliberada: Datos de demo hardcodeados

**Qué es**
- `BuildSampleDashboard()` regresa nombres, rachas y un leaderboard **inventados** ("Jordan Hayes", "Alex Rivera"), usados como base incluso cuando el usuario que inició sesión ya tiene skills reales cargadas en su cuenta.
- `SendTestEmail()` siempre manda la alerta de decaimiento con el mismo skill hardcodeado (`"React Fundamentals"`, 12 días), sin importar qué usuario o skill real se esté probando en ese momento.

**Por qué existe**
- Fue una decisión consciente, no un descuido: durante el desarrollo se necesitaba un dashboard presentable para mostrar en una demo, sin depender de tener suficientes usuarios o datos reales cargados en la base en ese momento. El problema no es que este atajo haya existido — es razonable querer algo que se vea bien para una demostración — sino que nunca se condicionó ni se limpió después de cumplir su propósito original, y terminó quedando activo como comportamiento por defecto para cualquier usuario sin skills.

**Costo de no pagarla**

| Riesgo | Impacto |
|---|---|
| Usuario real sin skills ve un leaderboard con gente inexistente | Rompe confianza en la app |
| `SendTestEmail()` muestra datos que no corresponden al usuario que la dispara | Confusión / falsa alarma |

Si esta deuda no se atiende, cualquier usuario nuevo que aún no haya registrado ninguna skill se encontrará con un leaderboard poblado de personas que no existen, lo cual puede leerse como un error del sistema o, peor, como una app que "inventa" usuarios — algo que daña la percepción de confiabilidad justo en el momento en que un usuario nuevo está formándose su primera impresión. De forma similar, si alguien dispara `SendTestEmail()` esperando validar el comportamiento con su propio skill, y en cambio recibe siempre la misma alerta de `"React Fundamentals"`, puede interpretar erróneamente que el sistema de decaimiento no está funcionando correctamente para su cuenta.

**Propuesta de solución**
1. Introducir un flag explícito `IsDemoMode`, de modo que `BuildSampleDashboard()` deje de comportarse como un fallback silencioso y solo se active cuando el modo demo se solicite intencionalmente.
2. Aplicar Extract Method en `SendTestEmail()` para separar la construcción del correo de la elección de sus datos, de manera que reciba el usuario y el skill real como parámetros en lugar de tener el valor fijo dentro del método.

---

## Cláusula de uso de IA

Este ADR se elaboró con apoyo de un asistente de IA (Claude), usado para acelerar la redacción y la verificación de los hallazgos contra el código fuente del proyecto. La identificación de las 2 deudas técnicas y la validación de la información fueron hechas y revisadas por el autor.