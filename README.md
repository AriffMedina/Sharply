# Sharply

> Dashboard personal de seguimiento de habilidades, construido con **ASP.NET Core** bajo **Arquitectura Hexagonal (Ports & Adapters)**.

![.NET](https://img.shields.io/badge/.NET-10.0-512BD4)
![Arquitectura](https://img.shields.io/badge/Arquitectura-Hexagonal-blue)
![API](https://img.shields.io/badge/API-REST%20%2B%20Swagger-85EA2D)

---

## Índice

- [¿Qué es esta rama?](#qué-es-esta-rama)
- [Qué se hizo](#qué-se-hizo)
- [Hallazgo más filoso](#hallazgo-más-filoso)
- [Cómo verificar los cambios](#cómo-verificar-los-cambios)
- [Qué queda pendiente](#qué-queda-pendiente)
- [Documentos completos](#documentos-completos)

---

## ¿Qué es esta rama?

`chore/saneamiento` no agrega funcionalidad nueva. Es una pasada de limpieza técnica sobre el código existente: nombres que no coincidían entre capas, código muerto, warnings ignorados, un secreto real committeado en texto plano, y un `BackgroundService` que estaba completo pero nunca llegaba a ejecutarse. Nada de esto rompía el proyecto de forma visible — son las inconsistencias típicas de construir contra entregas semanales — pero se acumulaban.

El trabajo siguió un proceso de dos pasos: primero un diagnóstico de solo lectura (`docs/saneamiento/DIAGNOSTICO.md`), después una lista cerrada de decisiones tomadas por el dueño del repo sobre qué corregir y cómo. Ningún cambio estructural se aplicó sin esa aprobación explícita.

## Qué se hizo

- **Coherencia de arquitectura hexagonal:** interfaces de servicios movidas a `Sharply.Domain/Interfaces`, `AuthService` desacoplado de `AppDbContext` vía `IUserRepository`.
- **Contrato de la Api:** `SkillsController` y `SkillLogsController` devuelven DTOs propios (`SkillResponse`, `SkillLogResponse`) en vez de exponer las entidades de dominio directamente.
- **`DecayWorker` reparado y activado:** tenía un bug de resolución de DI que garantizaba una excepción en su primer ciclo, y nunca estaba registrado como `IHostedService`. Ambos problemas corregidos y probados en vivo contra la base de datos real.
- **Seguridad:** la contraseña SMTP que estaba en texto plano en `appsettings.json` fue rotada y movida a User Secrets. Nunca tocó el historial de git.
- **Higiene general:** namespaces unificados, registros de DI duplicados eliminados, vestigios de una reorganización de carpetas vieja removidos, warnings de compilación en cero.

## Hallazgo más filoso

`DecayWorker` — el job que revisa el decaimiento de habilidades y dispara los recordatorios por email — estaba completo pero **nunca corría** en ningún ambiente: no estaba registrado en ningún `Program.cs`. Si se lo registraba tal cual estaba, fallaba en el primer ciclo por un problema de resolución de tipos en el contenedor de DI. Quedó reparado, activado, y verificado en vivo.

## Cómo verificar los cambios

```bash
dotnet build Sharply.slnx -c Debug
dotnet build Sharply.slnx -c Release
dotnet format Sharply.slnx --verify-no-changes
```

Ambas configuraciones compilan con 0 errores y 0 warnings (antes: 5). El contrato de la Api se verificó en vivo (host corriendo + `curl` contra `swagger.json`), y `dotnet ef migrations add DryRun` confirmó que el modelo de datos no se movió.

## Qué queda pendiente

- **Tests de caracterización:** deliberadamente fuera de esta rama, a pedido explícito del dueño del repo. Se retoma cuando se indique.
- **2 ítems en `pending-review.md`:** `LinearDecayStrategy` sin conectar a ningún flujo real, y una asimetría de configuración entre `Sharply.Api` y `Sharply.Web` (observación, no defecto).

## Documentos completos

| Documento | Contenido |
|---|---|
| [`docs/saneamiento/DIAGNOSTICO.md`](./docs/saneamiento/DIAGNOSTICO.md) | Auditoría inicial de solo lectura, con cada hallazgo etiquetado `[MECANICO]` / `[REVISAR]` / `[BLOQUEANTE]` |
| [`docs/saneamiento/pending-review.md`](./docs/saneamiento/pending-review.md) | Decisiones que quedaron abiertas, con opciones y recomendación |
| [`docs/saneamiento/REPORTE.md`](./docs/saneamiento/REPORTE.md) | Reporte final: commits, warnings antes/después, confirmaciones de verificación |

---

<div align="center">

*Rama `chore/saneamiento` — 2026.*

</div>
