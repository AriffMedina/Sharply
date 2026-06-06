# ADR-02: Vistas arquitectónicas del sistema

| Campo  | Valor |
|--------|-------|
| Autor  | Ariff Medina |
| Fecha  | 05/06/2026 |
| Estado | `Propuesto` |

---
## 1. Contexto

Tras haber establecido en el primer ADR la decisión de utilizar el patrón MVC con ASP.NET Core 10.0 como arquitectura base para Sharply, resulta estrictamente necesario e importante definir cómo se estructurará este diseño en la práctica. 

Esa primera decisión arquitectónica determinó las tecnologías y el enfoque general, pero documentar ahora las cuatro vistas arquitectónicas es el paso fundamental para comunicar cómo el sistema resuelve el problema del deterioro de habilidades y cómo interactúan sus piezas. Estas vistas responden a las siguientes preguntas clave del sistema para distintas audiencias:

- **Vista lógica:** ¿Qué módulos funcionales componen Sharply y cómo se relacionan?
- **Vista de desarrollo:** ¿Cómo está organizado el código fuente del proyecto?
- **Vista de procesos:** ¿Cómo se comporta el sistema en tiempo de ejecución?
- **Vista de despliegue:** ¿Dónde vive físicamente la aplicación y en qué infraestructura?

---

## 2. Vistas Arquitectónicas

---

### 2.1 Vista Lógica

#### Diagrama

![Vista Lógica — Sharply](./Vistas_arquitectonicas/vista-logica.jpeg)

#### Descripción y aspectos clave

| Módulo funcional            | Responsabilidad                                                      | Relación principal                          |
|-----------------------------|----------------------------------------------------------------------|---------------------------------------------|
| Gestión de Usuarios         | Administra los perfiles y preferencias de autenticación (Google).    | **Asocia** un catálogo a la Gestión de Habilidades. |
| Gestión de Habilidades      | Mantiene el catálogo de tecnologías y su nivel de maestría actual.   | **Actualiza** el estado tras nuevos Registros de Práctica. |
| Registros de Práctica       | Almacena el historial de misiones y sesiones de refuerzo completadas.| Funciona como insumo histórico para la evaluación.             |
| Motor de Evaluación de deterioro | Ejecuta el algoritmo de la curva del olvido para calcular días inactivos. | **Consulta** Habilidades y **Activa** Notificaciones. |
| Gestor de Notificaciones    | Construye y despacha los correos con misiones de refuerzo.           | **Alerta** al usuario sobre habilidades en riesgo. |

---

### 2.2 Vista de Desarrollo

#### Diagrama

```text
Sharply.sln
└── Sharply/
    │
    ├── Configuration/                       # Delegación de servicios para mantener Program.cs limpio
    │   ├── AuthConfig.cs              
    │   ├── DatabaseConfig.cs                
    │   └── ServicesConfig.cs                
    │
    ├── Controllers/
    │   ├── AccountController.cs             # External authentication (Google OAuth / Gmail)
    │   ├── HomeController.cs               
    │   ├── MissionsController.cs         
    │   └── SkillsController.cs           
    │
    ├── Models/                              # Entidades
    │   ├── Enums/
    │   │   └── MasteryLevel.cs              # Maestria: Oxidado, intermedio, afilado
    │   ├── User.cs                        
    │   ├── Skill.cs                       
    │   └── SkillLog.cs                     
    │
    ├── ViewModels/                           # Transfiere los modelos únicamente al apartado UI
    │   ├── DashboardViewModel.cs        
    │   ├── DailyMissionViewModel.cs   
    │   └── SkillFormViewModel.cs           
    │
    ├── Services/
    │   ├── Logic/
    │   │   ├── ISkillDecayService.cs
    │   │   └── SkillDecayService.cs          # Algoritmo de Ebbinghaus
    │   ├── Messaging/
    │   │   ├── IEmailService.cs
    │   │   └── EmailService.cs
    │   └── Jobs/
    │       ├── ISkillWorker.cs   
    │       └── DecayWorker.cs   
    │
    ├── Data/
    │   └── ApplicationDbContext.cs           # EF Core y configuración para SQL Server
    │
    ├── Views/
    │   ├── Account/
    │   │   └── Login.cshtml                 
    │   ├── Home/
    │   │                             
    │   ├── Missions/
    │   │            
    │   ├── Skills/
    │   │           
    │   └── Shared/
    │       ├── _Layout.cshtml        
    │       └── Error.cshtml            
    ├── _ViewImports.cshtml                 
    └── _ViewStart.cshtml                     
    │
    ├── appsettings.json                  
    ├── appsettings.Development.json       
    └── Program.cs
```

#### Descripción y aspectos clave

La **Vista de Desarrollo** organiza el código fuente de Sharply estructurando el sistema en capas desacopladas dentro del framework ASP.NET Core 10.0 MVC:

**`Configuration/`:** Centraliza la inyección de dependencias para mantener `Program.cs` limpio.

**`Controllers/`:** Orquestan las peticiones HTTP y dirigen el flujo de la aplicación.

**`Models/`:** Define las entidades de dominio (`User`, `Skill`, `SkillLog`) y los enumeradores (`MasteryLevel`).

**`ViewModels/`:** Objetos de transferencia exclusivos para la UI; evitan exponer la base de datos a las vistas.

**`Services/`:** Aísla la lógica central: el algoritmo de Ebbinghaus, el envío de correos y las tareas en segundo plano (`DecayWorker`).

**`Data/`:** Contiene `ApplicationDbContext` para manejar la persistencia en SQL Server vía EF Core.

**`Views/`:** Plantillas Razor (`.cshtml`) encargadas del renderizado visual de la interfaz.

---

### 2.3 Vista de Procesos

#### Diagrama

![Vista de Procesos — Sharply](./Vistas_arquitectonicas/vista-procesos.jpeg)

#### Descripción y aspectos clave

**Flujo síncrono — petición del usuario**
Corresponde al ciclo de vida transaccional de MVC, accionado explícitamente por el usuario:
1. El usuario abre el Dashboard (`GET /Skills/Index`).
2. El controlador `SkillsController` consulta `ApplicationDbContext`.
3. `SkillDecayService` evalúa el estado en memoria de cada habilidad.
4. Se construye el `DashboardViewModel` y se renderiza la vista `Index.cshtml`.
5. El usuario presiona el botón "Misión Completada" (`POST`).
6. `MissionsController` recibe la confirmación.
7. Se ejecuta un `INSERT` en `SkillLog` y un `UPDATE` del nivel de maestría directamente en SQL Server.

**Flujo asíncrono — DecayWorker (hilo independiente)**
Es un servicio alojado (`BackgroundService`) que se ejecuta de forma paralela e independiente a las peticiones web. Se activa mediante un temporizador interno (ej. a las 2:00 AM) para realizar un barrido autónomo de la base de datos, calcular el deterioro y detonar el envío de correos a través del `EmailService`. Es un hilo independiente ya que la prevención no puede depender de que el usuario inicie sesión previamente.

---

### 2.4 Vista de Despliegue

#### Diagrama

![Vista de Despliegue — Sharply](./Vistas_arquitectonicas/vista-despliegue.jpeg)

#### Descripción y aspectos clave

**AWS us-east-1**
La región de Virginia del Norte se elige como entorno de alojamiento por ser el estándar con mayor disponibilidad de servicios y latencia optimizada.

**VPC 10.0.0.0/16**
Una Virtual Private Cloud (VPC) es una red lógica aislada dentro de AWS. Permite segmentar la infraestructura para que los componentes críticos de persistencia de datos no estén expuestos directamente al internet.

**EC2 — Subnet pública (10.0.1.0/24)**
Servidor que aloja la aplicación web (ASP.NET Core). Reside en una subnet pública porque el Internet Gateway le permite recibir tráfico HTTP/HTTPS externo. El Security Group permite únicamente conexiones en los puertos 80 y 443 al mundo.

**RDS SQL Server — Subnet privada (10.0.2.0/24)**
Servicio de base de datos administrado que reside en una subnet privada sin salida directa a internet. Su Security Group está estrictamente configurado para permitir conexiones TCP por el puerto 1433 única y exclusivamente si provienen de la IP interna de la instancia EC2 pública.

**Internet Gateway**
El componente de red en el borde de la VPC que actúa como puerta de enlace, permitiendo que el tráfico del usuario final alcance la instancia EC2.

**Gmail SMTP (externo)**
Servicio externo utilizado por el `EmailService`. Se comunica a través de una conexión cifrada saliendo de la VPC hacia los servidores de Google para el despacho de alertas.

**AWS Secrets Manager**
Bóveda segura que custodia cadenas de conexión y credenciales SMTP. Previene la exposición de datos sensibles en el repositorio o en los archivos de configuración locales.

---

## 3. Consecuencias y Trade-offs

---

### Trade-off 1 — Subnet privada para RDS vs acceso directo desde desarrollo

**Decisión:** El RDS vive en una subnet privada sin IP pública, accesible únicamente desde el EC2 vía TCP 1433.

**Lo que se gana:**
Protección de red absoluta contra escaneos de puertos e intentos de inyección directa desde internet. La base de datos es virtualmente invisible desde el exterior.

**Lo que se sacrifica:**
Añade fricción operativa al desarrollo; requiere configurar un túnel SSH (Bastion Host) o una conexión VPN si se desea conectar un cliente local (como SSMS) directamente a la base de datos en la nube.

**Justificación en el contexto de Sharply:**
La integridad de los historiales y perfiles de los usuarios prioriza la seguridad y aislamiento del entorno de producción por encima de la comodidad temporal durante el desarrollo.

---

### Trade-off 2 — DecayWorker como hilo independiente vs evento disparado por la Web App

**Decisión:** El cálculo y envío de alertas de deterioro se gestiona mediante un `BackgroundService` autónomo, en lugar de un evento disparado por la interacción del usuario web.

**Lo que se gana:**
Garantiza que el sistema cumpla su promesa central (alertas preventivas oportunas) evaluando la base de datos de forma autónoma, incluso si el usuario lleva días sin iniciar sesión en la plataforma.

**Lo que se sacrifica:**
Introduce complejidad técnica al requerir el manejo de múltiples ciclos de vida (Scoped vs Singleton) en la inyección de dependencias para evitar colisiones en Entity Framework Core, además de consumir recursos de cómputo en segundo plano constantemente.

**Justificación en el contexto de Sharply:**
El propósito fundamental de la plataforma es alertar proactivamente sobre el olvido inminente; depender de una acción reactiva del usuario anularía el valor preventivo del sistema.

---

### Trade-off 3 — Un solo EC2 vs múltiples instancias con balanceador de carga

**Decisión:** La aplicación corre en una única instancia EC2 sin un esquema de redundancia ni balanceo de carga.

**Lo que se gana:**
Mantiene la arquitectura de despliegue inicial simple y reduce drásticamente los costos fijos de infraestructura mensual, facilitando el mantenimiento y las actualizaciones en fases tempranas.

**Lo que se sacrifica:**
Ausencia de alta disponibilidad. Si la instancia virtual experimenta un fallo o requiere un reinicio, la plataforma completa sufrirá tiempo de inactividad (Downtime).

**Justificación en el contexto de Sharply:**
Tratándose de una entrega académica y un MVP, el volumen de tráfico concurrente es bajo y predecible. La complejidad y el costo de un esquema con balanceadores de carga y grupos de autoescalado son innecesarios en este momento.

---

### Trade-off 4 — AWS Secrets Manager vs variables de entorno en el sistema operativo

**Decisión:** Las credenciales críticas (contraseña RDS, accesos SMTP, tokens) se almacenan en AWS Secrets Manager en lugar del `appsettings.json` o de las variables de entorno directas del EC2.

**Lo que se gana:**
Seguridad de nivel empresarial, auditoría de accesos centralizada y nulo riesgo de filtración de claves en repositorios de código o imágenes de sistema operativo.

**Lo que se sacrifica:**
Incrementa levemente la latencia durante el inicio del servidor web al tener que solicitar las credenciales a la API de AWS, además de introducir un costo marginal adicional por la retención y petición de secretos.

**Justificación en el contexto de Sharply:**
Implementar el manejo seguro de credenciales desde el día uno evita deudas técnicas críticas en materia de seguridad y se alinea con las mejores prácticas arquitectónicas para la integración en la nube.

---

### Declaración de uso de Inteligencia Artificial

Para el desarrollo de la arquitectura y la documentación de este proyecto se utilizaron herramientas de Inteligencia Artificial (Gemini) estrictamente como un asistente de apoyo. La IA fue empleada para rebotar ideas y agilizar la redacción. 

Sin embargo, el análisis del problema, la lógica de negocio, las decisiones de los *trade-offs* arquitectónicos y la autoría intelectual del código fuente son de mi completa autoría, desarrollados con base a las diapositivas otorgadas por el profesor Joreg Pedrozo.
