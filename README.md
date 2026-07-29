# Sharply

> Dashboard personal de seguimiento de habilidades, construido con **ASP.NET Core** bajo **Arquitectura Hexagonal (Ports & Adapters)**.

![.NET](https://img.shields.io/badge/.NET-10.0-512BD4)
![Arquitectura](https://img.shields.io/badge/Arquitectura-Hexagonal-blue)
![API](https://img.shields.io/badge/API-REST%20%2B%20Swagger-85EA2D)

---

## Índice

- [¿Qué es Sharply?](#qué-es-sharply)
- [Cómo funciona el decay](#cómo-funciona-el-decay)
- [Estructura del repositorio](#estructura-del-repositorio)
- [Diagramas C4](#diagramas-c4)
- [Tecnologías](#tecnologías)
- [Requisitos previos](#requisitos-previos)
- [Cómo ejecutar el proyecto](#cómo-ejecutar-el-proyecto)
- [Endpoints API](#endpoints-api)

---

## ¿Qué es Sharply?

Sharply parte de una pregunta concreta: ¿cuánto de lo que aprendiste la semana pasada te queda hoy?

El sistema modela el olvido usando la curva de Ebbinghaus. Cada habilidad registrada tiene un nivel de retención que cae exponencialmente con el tiempo de inactividad, modulado por el nivel de dominio del usuario y la prioridad que le asignó. Cuando la retención baja de un umbral configurable, el sistema notifica por email para que el usuario repase antes de olvidar.

El proyecto arrancó como experimento para explorar cómo aplicar arquitectura hexagonal en un contexto pequeño, sin sobrediseñar ni depender de frameworks en el núcleo del dominio.

---

## Cómo funciona el decay

La retención de una habilidad se calcula en cada solicitud con la fórmula:

```
R(t) = R₀ · e^(-t / S)
```

Donde:

- `R₀` es la retención inicial (1.0 al practicar)
- `t` es el número de días sin practicar
- `S` es la constante de estabilidad, que depende del nivel de dominio y la prioridad asignada

| Nivel de dominio | Estabilidad base |
|---|---|
| Sharp | 30 días |
| Intermediate | 15 días |
| Rusty | 7 días |

La prioridad aplica un multiplicador sobre esa base: `High → ×1.5`, `Low → ×0.7`, `Medium → ×1.0`. También hay una estrategia de decaimiento lineal disponible como alternativa, intercambiable gracias a la interfaz `IDecayStrategy`.

El `DecayWorker` corre en background cada 24 horas, recorre todos los usuarios, calcula la retención de cada habilidad y dispara notificaciones por email usando el patrón Observer (`SkillDecayNotifier` + `EmailService`).

---

## Estructura del repositorio

```text
Sharply-Hexagonal/
├── Sharply.Domain/                   # Núcleo — sin dependencias externas
│   ├── Models/                       # Skill, SkillLog, User
│   ├── Enums/                        # MasteryLevel, SkillPriority
│   └── Interfaces/                   # ISkillRepository, ISkillLogRepository,
│                                     # IDecayStrategy, ISkillDecayService,
│                                     # IAuthService, IEmailService,
│                                     # ISkillDecayObserver, ISkillDecaySubject
│
├── Sharply.Application/              # Casos de uso y estrategias
│   └── Services/                     # SkillDecayService, EbbinghausDecayStrategy,
│                                     # LinearDecayStrategy, SkillDecayNotifier,
│                                     # MissionService
│
├── Sharply.Infrastructure/           # Adaptadores de salida
│   ├── Data/                         # AppDbContext (EF Core)
│   ├── Repositories/                 # SkillRepository, SkillLogRepository
│   ├── Services/                     # AuthService (BCrypt)
│   ├── Messaging/                    # EmailService (MailKit + Observer)
│   ├── Jobs/                         # DecayWorker (BackgroundService)
│   └── Migrations/                   # EF Core migrations
│
├── Sharply.Web/                      # Adaptador de entrada — ASP.NET Core MVC
│   ├── Controllers/                  # HomeController, SkillsController, AccountController
│   ├── ViewModels/                   # DashboardViewModel, SkillCardViewModel,
│   │                                 # SkillFormViewModel, LoginViewModel
│   └── Views/                        # Razor Views
│
└── Sharply.Api/                      # Adaptador de entrada — ASP.NET Core Web API
    ├── Controllers/                  # SkillsController, SkillLogsController
    └── Program.cs                    # Composición de dependencias + Swagger
```

---

## Diagramas C4

La arquitectura de Sharply está documentada con el modelo C4, en tres niveles de zoom progresivo. Cada diagrama vive como código (Mermaid) dentro del repositorio, se versiona junto al resto del proyecto y se revisa en Pull Request como cualquier otro cambio — no es una imagen suelta que se desactualiza.

| Nivel | Qué responde | Para quién |
|---|---|---|
| [Nivel 1 — Contexto](./docs/Diagramas_C4/DiagramasC4.md#nivel-1--contexto) | ¿Qué es el sistema y quién lo usa? | Cualquiera |
| [Nivel 2 — Contenedores](./docs/Diagramas_C4/DiagramasC4.md#nivel-2--contenedores) | ¿De qué piezas técnicas grandes se compone? (Web, Api, base de datos, servidor SMTP) | Equipo técnico |
| [Nivel 3 — Componentes](./docs/Diagramas_C4/DiagramasC4.md#nivel-3--componentes-dentro-de-sharplyweb) | ¿Qué hay dentro de `Sharply.Web`? (controllers, casos de uso, dominio, adaptadores) | Quien modifica el código |


---

## Tecnologías

| Categoría | Tecnología |
|---|---|
| Framework | ASP.NET Core (.NET 10) |
| Arquitectura | Hexagonal (Ports & Adapters) |
| ORM | Entity Framework Core 10 + SQL Server |
| Autenticación | Cookie auth + BCrypt (BCrypt.Net-Next) |
| Email | MailKit + MimeKit |
| API | ASP.NET Core Web API + Swagger (Swashbuckle) |
| Patrones de diseño | Strategy (decay), Observer (notificaciones), Background Service |

---

## Requisitos previos

- [.NET SDK 10.0](https://dotnet.microsoft.com/download)
- SQL Server (local o en contenedor)
- Un editor compatible: Visual Studio, VS Code con C# Dev Kit, Rider

---

## Cómo ejecutar el proyecto

```bash
# Restaurar dependencias
dotnet restore

# Aplicar migraciones (requiere cadena de conexión configurada)
dotnet ef database update --project Sharply.Infrastructure --startup-project Sharply.Web

# Levantar el sitio web (dashboard + autenticación)
dotnet run --project Sharply.Web
# → https://localhost:7287

# Levantar la API REST (con Swagger)
dotnet run --project Sharply.Api
# → http://localhost:5000/swagger
```

### Configuración

La cadena de conexión a SQL Server va en `appsettings.json` o en User Secrets:

```json
{
  "ConnectionStrings": {
    "DefaultConnection": "Server=...;Database=SharplyDb;..."
  },
  "EmailSettings": {
    "SmtpServer": "smtp.example.com",
    "SmtpPort": "587",
    "SenderEmail": "noreply@example.com",
    "SenderName": "Sharply",
    "Password": "..."
  }
}
```

No incluir credenciales reales en `appsettings.json` ni en el repositorio. Usar User Secrets en desarrollo:

```bash
dotnet user-secrets set "EmailSettings:Password" "tu-password" --project Sharply.Web
```

---

## Endpoints API

Con `Sharply.Api` corriendo, Swagger queda disponible en `/swagger`. Los endpoints actuales cubren lectura de habilidades y logs:

### Skills — `/api/skills`

| Método | Ruta | Descripción |
|---|---|---|
| GET | `/api/skills` | Lista todas las habilidades (filtrable por `?priority=High`) |
| GET | `/api/skills/{id}` | Obtiene una habilidad por id |

### Skill Logs — `/api/skilllogs`

| Método | Ruta | Descripción |
|---|---|---|
| GET | `/api/skilllogs` | Lista todos los logs (filtrable por `?skillId=3`) |
| GET | `/api/skilllogs/{id}` | Obtiene un log por id |

El CRUD completo de habilidades y el cálculo de retención están disponibles en `Sharply.Web` y quedan pendientes de exponer en la API.

---

<div align="center">

**⭐ Si te gustó este proyecto, dale una estrella ⭐**

Hecho con 💙 por Ariff Medina — 2026

</div>

*Sharply — 2026.*
