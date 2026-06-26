# ADR-05: Uso del patrón Observer para el desacoplamiento de notificaciones

| Campo  | Valor |
|--------|-------|
| Autor  | Ariff Medina |
| Fecha  | 26/06/2026 |
| Estado | Aceptado |

---

## Contexto

Tras haber adoptado la Arquitectura Hexagonal en el ADR-03 para proteger la lógica de negocio de Sharply, surgió un nuevo reto en la evolución del sistema: la gestión de alertas cuando una habilidad cae en riesgo de ser olvidada.

El `SkillDecayService` tiene la responsabilidad de ejecutar el algoritmo de Ebbinghaus para calcular la retención de las habilidades. Sin embargo, cuando se detecta un deterioro crítico, el sistema debe notificar al usuario (actualmente mediante correos usando MailKit). En un futuro cercano, también tengo contemplado que el sistema de gamificación (`GamificationService`) reaccione restando puntos de experiencia. 

Si inyectaba el `EmailService` y el `GamificationService` directamente en mi servicio de cálculo o en mi Worker, iba a generar un alto acoplamiento. Mi lógica de dominio terminaría mezclada con la infraestructura de correos, violando el Principio de Abierto/Cerrado (OCP) y el Principio de Responsabilidad Única (SRP), lo cual iría en contra del esfuerzo de limpieza arquitectónica que logré en las entregas pasadas.

---

## Decisión

Decidí implementar el patrón de diseño estructural **Observer** (de la familia GoF) para gestionar las reacciones al deterioro de habilidades.

Se definió un Sujeto (`SkillDecayNotifier` o `ISkillDecaySubject`) que actúa exclusivamente como emisor de alertas. Por otro lado, los servicios de infraestructura como `EmailService` ahora implementan la interfaz `ISkillDecayObserver`, suscribiéndose al Sujeto. 

### ¿Por qué?

El núcleo de negocio ahora es completamente "ignorante" de la infraestructura. El `SkillDecayNotifier` simplemente anuncia que una habilidad cayó por debajo del umbral, pero no le interesa quién escucha esa alerta ni cómo se procesa. 

Esto me permite una escalabilidad limpia: si la próxima semana necesito agregar notificaciones Push para la futura aplicación en MAUI o un sistema de auditoría, simplemente creo un nuevo servicio, implemento la interfaz del observador y lo suscribo. El código del notificador y de la lógica de negocio central permanecerá intacto.

### Alternativas consideradas

Para resolver este acoplamiento analicé otros patrones de la familia GoF, los cuales, aunque fueron descartados para este problema en específico, tienen una gran proyección para el futuro de Sharply:

| Alternativa (Patrones GoF) | Por qué la descarté para las notificaciones (y su proyección futura) |
|-------------|---------------------|
| **Patrón Strategy** | **Por qué se descartó:** Strategy sirve para encapsular algoritmos intercambiables, no para notificar eventos a múltiples interesados (que es un flujo de 1 a N).<br><br>**Proyección futura:** Será extremadamente útil en el futuro para el núcleo del sistema. Actualmente todas las habilidades usan la curva de Ebbinghaus, pero en futuras iteraciones Sharply podría necesitar calcular el deterioro de forma distinta (ej. una "Estrategia Lineal" vs. "Estrategia Logarítmica") dependiendo del tipo de habilidad. |
| **Patrón Decorator** | **Por qué se descartó:** Pude haber creado un `EmailNotifierDecorator` que envolviera el `SkillDecayService`, añadiéndole la función de enviar correos. Lo descarté porque seguiríamos encadenando dependencias si agregáramos la gamificación, volviendo el código frágil en comparación al modelo limpio de suscripciones del Observer.<br><br>**Proyección futura:** Lo tengo contemplado para añadir una capa de **Caché temporal** al repositorio de base de datos (`ISkillRepository`). Así evitaré saturar SQL Server de consultas cuando el `DecayWorker` analice masivamente las habilidades, sin tener que modificar la lógica actual de Entity Framework. |

---

## Consecuencias

**✅ Lo que gano:**

*Desacoplamiento total:* La lógica de dominio queda aislada de la tecnología de envío de correos. Se respeta estrictamente la separación de capas de la Arquitectura Hexagonal.

*Facilidad de Pruebas (Testing):* Es mucho más fácil aislar y probar unitariamente el algoritmo de retención de Ebbinghaus y el Notificador cuando no hay un cliente SMTP real exigiendo credenciales y conexiones de red de por medio.

**⚠️ Lo que sacrifico o asumo:**

*Complejidad arquitectónica inicial:* El flujo de ejecución ya no es estrictamente lineal (de arriba hacia abajo). Para un desarrollador nuevo, entender qué ocurre después de que una habilidad caduca requiere buscar qué clases están suscritas al `Notifier`, en lugar de solo leer el método paso a paso.

*Gestión de memoria y dependencias:* Asumo la responsabilidad técnica de tener que registrar y suscribir (Attach) correctamente los observadores al inicio de la ejecución del Worker, lidiando con los ciclos de vida del contenedor de inyección de dependencias de ASP.NET Core.

---

### 🤖 Declaración de uso de Inteligencia Artificial

Para el desarrollo de la arquitectura y la documentación de esta entrega utilicé Claude estrictamente como un asistente de apoyo. La IA fue empleada para rebotar ideas y explorar el catálogo de patrones GoF. 

El análisis del problema de acoplamiento, las decisiones de los *trade-offs* arquitectónicos, el descarte justificado de las alternativas y la autoría intelectual del código fuente son de mi completa autoría, desarrollados con base a las prácticas de resiliencia y patrones revisados en clase a partir de los recursos proporcionados por el profesor Jorge Pedrozo.