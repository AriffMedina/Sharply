# Diagramas de Arquitectura — Sharply (Modelo C4)

## Nivel 1 — Contexto

*Para: Cualquier persona interesada en mi plataforma y que quiera tener una noción primaria del funcionamiento de esta. Responde: ¿Qué es el sistema y quién lo usa?*

```mermaid
graph TD
    Usuario["👤 Usuario<br/>Registra sus habilidades y<br/>sesiones de práctica"]

    Sharply["🧠 Sharply<br/>Sistema de seguimiento del<br/>deterioro de habilidades<br/>(curva del olvido de Ebbinghaus)"]

    SMTP["✉️ Servidor de correo<br/>(Gmail SMTP)"]

    Usuario -->|"Inicia sesión, crea skills,<br/>registra práctica, consulta su dashboard"| Sharply
    Sharply -->|"Envía alerta cuando una<br/>skill está en riesgo de olvido"| SMTP
    SMTP -.->|"Entrega el correo"| Usuario
```

## Nivel 2 — Contenedores

*Para: El equipo técnico que necesita entender la estructura general del sistema. Responde: ¿De qué piezas grandes se compone?*

```mermaid
graph TD
    Navegador["🖥️ Navegador<br/>Cliente Web"]
    ClienteApi["🔌 Cliente API<br/>Swagger UI / Postman"]

    subgraph SharplySystem["Sharply"]
        Web["Sharply.Web<br/>[ASP.NET Core MVC]<br/>Login, dashboard,<br/>gestión de skills"]
        Api["Sharply.Api<br/>[ASP.NET Core Web API]<br/>Endpoints REST de solo<br/>lectura + Swagger"]
    end

    DB[("🗄️ SharplyDB<br/>[SQL Server]<br/>Users · Skills · SkillLogs")]
    SMTP["✉️ Servidor SMTP<br/>(Gmail, externo)"]

    Navegador -->|"HTTPS"| Web
    ClienteApi -->|"HTTPS / JSON"| Api
    Web -->|"EF Core"| DB
    Api -->|"EF Core"| DB
    Web -->|"SMTP / MailKit"| SMTP
```

## Nivel 3 — Componentes dentro de Sharply.Web

*Para: Aquellos que van a modificar el código de la plataforma y que necesitan entender la estructura interna de cada componente. Responde: "¿Qué hay dentro de cada pieza?*

```mermaid
graph TD
    Navegador["🖥️ Navegador"]

    subgraph Web["Sharply.Web — arquitectura hexagonal"]
        subgraph Entrada["Adaptadores de entrada"]
            CtrlAccount["AccountController<br/>Login / registro"]
            CtrlHome["HomeController<br/>Dashboard + envío de<br/>correo de prueba"]
            CtrlSkills["SkillsController<br/>CRUD de skills +<br/>registrar práctica"]
        end

        subgraph Nucleo["Aplicación — casos de uso"]
            DecaySvc["SkillDecayService<br/>Calcula retención y<br/>detecta skills en riesgo"]
            DecayStrategy["EbbinghausDecayStrategy<br/>Fórmula del olvido<br/>(Strategy)"]
        end

        subgraph Dominio["Dominio — modelos y puertos"]
            Modelos["Skill · SkillLog · User"]
            Puertos["Puertos: ISkillRepository,<br/>ISkillLogRepository,<br/>IAuthService, IEmailService,<br/>IDecayStrategy"]
        end

        subgraph Salida["Adaptadores de salida"]
            Repo["SkillRepository /<br/>SkillLogRepository"]
            Auth["AuthService<br/>(hash BCrypt)"]
            Email["EmailService<br/>(MailKit, patrón Observer)"]
            Ctx["AppDbContext<br/>(EF Core)"]
        end
    end

    DB[("🗄️ SharplyDB")]
    SMTP["✉️ Servidor SMTP"]

    Navegador --> CtrlAccount
    Navegador --> CtrlHome
    Navegador --> CtrlSkills

    CtrlAccount --> Puertos
    CtrlHome --> Puertos
    CtrlHome --> DecaySvc
    CtrlSkills --> Puertos
    CtrlSkills --> DecaySvc

    DecaySvc --> DecayStrategy
    DecaySvc --> Puertos

    Puertos -.implementado por.-> Repo
    Puertos -.implementado por.-> Auth
    Puertos -.implementado por.-> Email

    Repo --> Ctx
    Auth --> Ctx
    Ctx --> DB
    Email --> SMTP
```