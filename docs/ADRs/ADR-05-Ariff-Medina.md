# ADR-05: Uso del patrón Observer para el desacoplamiento de notificaciones

| Campo  | Valor |
|--------|-------|
| Autor  | Ariff Medina |
| Fecha  | 26/06/2026 |
| Estado | Aceptado |

---

## Contexto

Tras haber adoptado la Arquitectura Hexagonal en el ADR-03 para proteger la lógica de negocio de Sharply, surgieron nuevos retos en la evolución del sistema: 

El primero fue la gestión de alertas cuando una habilidad cae en riesgo de ser olvidada.
El `SkillDecayService` tiene la responsabilidad de ejecutar el algoritmo de Ebbinghaus para calcular la retención de las habilidades. Sin embargo, cuando se detecta un deterioro crítico, el sistema debe notificar al usuario (actualmente mediante correos usando MailKit). En un futuro cercano, también tengo contemplado que el sistema de gamificación (`GamificationService`) reaccione restando puntos de experiencia. 
El segundo fue la rigidez del algoritmo de cálculo de retención. La fórmula de Ebbinghaus estaba hardcodeada como una constante (`DecayConstant = 1.0`) directamente dentro del `SkillDecayService`, sin posibilidad de intercambiarse según el tipo de habilidad o la prioridad del usuario, violando el Principio de Abierto/Cerrado (OCP).
Si inyectaba el `EmailService` y el `GamificationService` directamente en mi servicio de cálculo o en mi Worker, iba a generar un alto acoplamiento. Mi lógica de dominio terminaría mezclada con la infraestructura de correos, violando el Principio de Abierto/Cerrado (OCP) y el Principio de Responsabilidad Única (SRP), lo cual iría en contra del esfuerzo de limpieza arquitectónica que logré en las entregas pasadas.

---

## Decisión

Decidí implementar dos patrones de diseño de la familia GoF para resolver ambos retos de forma independiente y complementaria.

### Patrón Observer — Desacoplamiento de notificaciones
 
Se definió un Sujeto (`SkillDecayNotifier` o `ISkillDecaySubject`) que actúa exclusivamente como emisor de alertas. Por otro lado, los servicios de infraestructura como `EmailService` ahora implementan la interfaz `ISkillDecayObserver`, suscribiéndose al Sujeto.
 
El núcleo de negocio ahora es completamente "ignorante" de la infraestructura. El `SkillDecayNotifier` simplemente anuncia que una habilidad cayó por debajo del umbral, pero no le interesa quién escucha esa alerta ni cómo se procesa.
 
Esto me permite una escalabilidad limpia: si la próxima semana necesito agregar notificaciones Push para la futura aplicación en MAUI o un sistema de auditoría, simplemente creo un nuevo servicio, implemento la interfaz del observador y lo suscribo. El código del notificador y de la lógica de negocio central permanecerá intacto.
 
### Patrón Strategy — Algoritmo de decaimiento intercambiable
 
Se definió la interfaz `IDecayStrategy` en la capa de Dominio, con el método `Calculate(double initialRetention, int daysInactive, MasteryLevel mastery, SkillPriority priority)`. El `SkillDecayService` actúa como **Contexto**: ya no contiene ni conoce la fórmula de cálculo, simplemente delega en la estrategia inyectada por el contenedor de dependencias.
 
Se implementaron dos estrategias concretas:
 
- **`EbbinghausDecayStrategy`** — estrategia activa por defecto. Aplica la fórmula `R = R₀ * e^(-t/S)`, donde la estabilidad `S` varía según el nivel de maestría y la prioridad de la habilidad, haciendo el decaimiento más o menos agresivo dependiendo del contexto del usuario.
- **`LinearDecayStrategy`** — estrategia alternativa. Aplica una pérdida fija de retención por día según el nivel de maestría, útil para perfiles de habilidades con comportamiento más predecible.
En el MVP actual se registra `EbbinghausDecayStrategy` como implementación activa mediante el contenedor de DI (`AddScoped<IDecayStrategy, EbbinghausDecayStrategy>()`). Cambiar el algoritmo de decay en toda la plataforma requiere modificar únicamente esa línea en `Program.cs`, sin tocar ninguna otra clase.
 
---
 
### ¿Por qué estos dos patrones?
 
Ambos patrones atienden naturalezas de problema distintas y se complementan sin solaparse:
 
| Problema | Patrón aplicado | Rol en la arquitectura |
|----------|----------------|------------------------|
| Notificar a múltiples servicios cuando una habilidad cae en riesgo (1 a N) | **Observer** | `SkillDecayNotifier` como Sujeto; `EmailService` como Observador |
| Intercambiar el algoritmo de cálculo de retención sin modificar el servicio (algoritmo encapsulado) | **Strategy** | `SkillDecayService` como Contexto; `EbbinghausDecayStrategy` / `LinearDecayStrategy` como Estrategias |
 
### Alternativas consideradas
 
| Alternativa (Patrones GoF) | Por qué la descarté |
|-------------|---------------------|
| **Patrón Strategy** (para notificaciones) |Strategy sirve para encapsular algoritmos intercambiables, no para notificar eventos a múltiples interesados (que es un flujo de 1 a N).<br> |
| **Patrón Decorator** | Pude haber creado un `EmailNotifierDecorator` que envolviera el `SkillDecayService`, añadiéndole la función de enviar correos. Lo descarté porque seguiríamos encadenando dependencias si agregáramos la gamificación, volviendo el código frágil en comparación al modelo limpio de suscripciones del Observer.<br|
 
---
---

## Consecuencias

**✅ Lo que gano:**

*Desacoplamiento total:* La lógica de dominio queda aislada de la tecnología de envío de correos. Se respeta estrictamente la separación de capas de la Arquitectura Hexagonal.

*Facilidad de Pruebas (Testing):* Es mucho más fácil aislar y probar unitariamente el algoritmo de retención de Ebbinghaus y el Notificador cuando no hay un cliente SMTP real exigiendo credenciales y conexiones de red de por medio. Adicionalmente, Strategy permite inyectar un mock de `IDecayStrategy` en pruebas unitarias del `SkillDecayService` sin depender de ninguna implementación concreta.

**⚠️ Lo que sacrifico o asumo:**

*Complejidad arquitectónica inicial:* El flujo de ejecución ya no es estrictamente lineal (de arriba hacia abajo). Para un desarrollador nuevo, entender qué ocurre después de que una habilidad caduca requiere buscar qué clases están suscritas al `Notifier`, en lugar de solo leer el método paso a paso.

*Gestión de memoria y dependencias:* Asumo la responsabilidad técnica de tener que registrar y suscribir (Attach) correctamente los observadores al inicio de la ejecución del Worker, lidiando con los ciclos de vida del contenedor de inyección de dependencias de ASP.NET Core.

---

### 🤖 Declaración de uso de Inteligencia Artificial

Para el desarrollo de la arquitectura y la documentación de esta entrega utilicé Claude estrictamente como un asistente de apoyo. La IA fue empleada para rebotar ideas y explorar el catálogo de patrones GoF. 

El análisis del problema de acoplamiento, las decisiones de los *trade-offs* arquitectónicos, el descarte justificado de las alternativas y la autoría intelectual del código fuente son de mi completa autoría, desarrollados con base a las prácticas de resiliencia y patrones revisados en clase a partir de los recursos proporcionados por el profesor Jorge Pedrozo.
