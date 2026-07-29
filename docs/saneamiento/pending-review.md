# Decisiones pendientes de saneamiento

Ítems que salieron de la auditoría (`DIAGNOSTICO.md`) y de la ejecución de la Fase 2, y que requieren criterio humano del dueño del repo. No es una lista de bugs — es un mapa de "acá hay algo que revisar" cuando haya tiempo/roadmap para decidir.

---

### `Sharply.Application/Services/LinearDecayStrategy.cs`::`LinearDecayStrategy`

- **Qué:** Implementa `IDecayStrategy` con una fórmula de decaimiento lineal (en vez de exponencial como `EbbinghausDecayStrategy`), pero nunca se registra en ningún `Program.cs` y no existe ningún mecanismo para que un usuario o un flujo del sistema elija qué estrategia aplicar. Además, su parámetro `priority` no se usa en el cálculo, a diferencia de `EbbinghausDecayStrategy.Calculate`, que sí aplica un multiplicador según prioridad con la misma firma.
- **Por qué huele mal:** Es una segunda implementación completa del patrón Strategy que hoy es inalcanzable desde cualquier flujo real — o se completa el mecanismo de selección, o se elimina. Tal como está, es código muerto con forma de código vivo.
- **Opciones:**
  - A) Agregar un mecanismo real de selección de estrategia (por usuario, por configuración, por skill) y conectar `LinearDecayStrategy`.
  - B) Eliminarla si `EbbinghausDecayStrategy` es la única que se va a usar en la práctica.
  - C) Dejarla como está — ya quedó documentada con un comentario XML como referencia del patrón (Fase 2, ítem 8).
- **Riesgo si se toca:** Bajo si se elimina (nadie la usa hoy). Medio si se conecta — requiere decidir el mecanismo de selección, que probablemente toca UI/UX y no es un cambio mecánico.
- **Recomendación:** Dejarla como referencia por ahora (ya documentada); decidir su futuro cuando se defina si el usuario va a poder elegir estrategia de decaimiento.

---

### `Sharply.Api/appsettings.json`::asimetría de configuración con `Sharply.Web`

- **Qué:** `Sharply.Api/appsettings.json` no tiene sección `EmailSettings` ni bloque `Logging` propio, a diferencia de `Sharply.Web/appsettings.json` que sí los tiene.
- **Por qué huele mal:** Asimetría de configuración entre los dos hosts de entrada del sistema. Hoy es inocua porque `Sharply.Api` no envía correos y el `DecayWorker` quedó activado en `Sharply.Web` (ver Fase 2, ítem 1), pero si en el futuro `Sharply.Api` necesita loguear con configuración propia o enviar correos, va a faltar esa sección.
- **Opciones:**
  - A) Agregar un bloque `Logging` básico a `Sharply.Api/appsettings.json` por consistencia, aunque no se use `EmailSettings` todavía.
  - B) Dejarlo como está mientras `Sharply.Api` no tenga una necesidad real de esas secciones.
- **Riesgo si se toca:** Bajo — agregar configuración no usada no cambia comportamiento observable.
- **Recomendación:** Dejarlo como está. Es una observación, no un defecto — no hay urgencia.

---

No surgieron ítems `[REVISAR]` nuevos durante la ejecución de la Fase 2 más allá de los ya listados en `RespuestaFase2.md` (todos resueltos con decisión explícita) y estos dos. Las dos discrepancias encontradas entre `RespuestaFase2.md` y el código real durante la ejecución (estilo de namespace en `Sharply.Web`, y el tipo a resolver para `notifier.Attach` en `DecayWorker`) ya quedaron resueltas con decisión del dueño del repo y no requieren revisión adicional.
