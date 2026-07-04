# 🐛 Corrección de Múltiples Bugs — Apiarios, Tareas, Inspecciones y Colmenas

**Chat de referencia:** `37259ff4-e7e5-412c-9d71-dcc7883ee903`  
**Fecha:** 2026-07-04  
**Área:** Múltiples controladores y vistas — sesión larga de corrección de errores

---

## Contexto

Sesión extensa de corrección de bugs que abarcó: título duplicado en Comparación de Apiarios, filtrado dinámico de colmenas en Tareas, error en Trashumancia, validaciones de inspecciones, restricciones de fechas, y comportamiento de baja de colmenas.

---

## Bug 1 — Título duplicado en Comparación de Apiarios

**Síntoma:** La vista mostraba "Comparación de Apiarios" dos veces (una en el `_Layout` y otra dentro de la vista).

**Fix:** Eliminado el `<h2>` redundante de `Views/Apiarios/Comparacion.cshtml`.

---

## Bug 2 — Tareas: Colmenas no filtraban por Apiario

**Síntoma:** Al registrar una nueva tarea y seleccionar un apiario, el selector de colmenas mostraba **todas las colmenas** del sistema, no solo las del apiario elegido.

**Fix implementado:**

**`Controllers/TareasController.cs`** — Se agregó un endpoint AJAX:
```csharp
[HttpGet]
public async Task<IActionResult> GetColmenasByApiario(int apiarioId)
{
    var colmenas = await _context.Colmenas
        .Where(c => c.ApiarioId == apiarioId)
        .Select(c => new { c.Id, c.Nombre })
        .ToListAsync();
    return Json(colmenas);
}
```

**`Views/Tareas/Create.cshtml`** — Se agregó JS que llama al endpoint al cambiar el apiario seleccionado y repoblada el select de colmenas dinámicamente.

---

## Bug 3 — Trashumancia: Error falso al registrar traslado

**Síntoma:** El formulario mostraba "Error: Por favor, verifique los campos del formulario" aunque todos los campos estaban completos.

**Causa:** El campo `DistanciaKm` es `readonly` y calculado por JS. Si los apiarios no tenían coordenadas configuradas, el JS no calculaba la distancia y el campo quedaba vacío. El model binding de ASP.NET no podía parsear `""` como `double`, invalidando el `ModelState`.

**Fix:**
- `DistanciaKm` pasó a tener `value="0"` por defecto.
- Se corrigió el JS para usar `0` en vez de `''` cuando no hay ambos apiarios seleccionados:

```javascript
// ANTES
input.value = '';
// DESPUÉS
input.value = '0';
```

---

## Bug 4 — Inspecciones (Sanidad): Ajustes de formulario y validaciones

**Cambios implementados tras feedback iterativo:**

| Elemento | Cambio |
|---|---|
| Apiario | **Obligatorio** — muestra `-- Seleccione un Apiario --` |
| Colmena | **Opcional** — se puede inspeccionar un apiario entero sin colmena específica |
| Colmenas en select | Solo se muestran si hay un **apiario seleccionado** (AJAX dinámico) |
| Campo "Enfermedades detectadas" | Movido al lado del campo "Fecha" para optimizar espacio |
| Botón "Eliminar" en índice | Cambiado a **"Quitar"** |

**Modelo `Revision` — cambio de esquema:**
```csharp
// ANTES
[Required]
public int ColmenaId { get; set; }

// DESPUÉS
public int? ColmenaId { get; set; }    // opcional
public int? ApiarioId { get; set; }    // nuevo campo, obligatorio en UI
```
> Se creó y ejecutó una migración EF Core para este cambio de esquema.

---

## Bug 5 — Inspecciones: Tratamiento y fecha de segunda dosis

**Regla de negocio implementada:**
- Si se selecciona un **tratamiento**, el campo "Fecha de segunda dosis" es **obligatorio**.
- Si no se hace ningún tratamiento ("Ninguno"), el campo de segunda dosis queda oculto y no es requerido.
- La "Fecha de próxima dosis" debe ser **mayor a la fecha de hoy** (validación `min` en input date).

**JS implementado:**
```javascript
document.getElementById('tratamiento').addEventListener('change', function () {
    const hayTratamiento = this.value && this.value !== 'Ninguno';
    document.getElementById('segundaDosisSection').style.display = 
        hayTratamiento ? 'block' : 'none';
    document.getElementById('fechaSegundaDosis').required = hayTratamiento;
});
```

---

## Bug 6 — Colmenas: Estado "Perdida" en Edit y flujo Dar de Baja

**Problema:** En el formulario de editar una colmena, aparecía el estado "Perdida" como opción seleccionable, lo cual no tiene sentido — una colmena perdida no puede ser editada.

**Fix en `Views/Colmenas/Edit.cshtml`:**
- Se eliminó "Perdida" del select de estado.

**Flujo "Dar de Baja" rediseñado:**
- Se simplificó `Views/Colmenas/DarDeBaja.cshtml` a una confirmación directa sin selección de motivo.
- El controlador `ColmenasController.cs` siempre asigna estado `Perdida` al confirmar la baja.
- Se implementó `returnUrl` para redirigir al usuario a la sección desde donde inició el proceso.

---

## Bug 7 — Fecha de nacimiento de Reina — validación max date

**Regla:** La fecha de nacimiento de la reina no puede ser en el futuro.

**Fix en `Create.cshtml` y `Edit.cshtml` de Colmenas:**
```html
<input type="date" asp-for="FechaNacimientoReina" 
       max="@DateTime.Today.ToString("yyyy-MM-dd")" />
```

---

## Bug 8 — Ordenamiento de Colmenas por Población

**Problema:** El ordenamiento por el campo "Población" no funcionaba correctamente porque `NivelPoblacion` es un **enum** (`Fuerte`, `Media`, `Débil`), no un string. El orden alfabético no coincide con el orden lógico.

**Fix en `ColmenasController.cs`** — Ordenamiento con CASE personalizado:
```csharp
.OrderBy(c => c.Poblacion == NivelPoblacion.Fuerte ? 0 :
              c.Poblacion == NivelPoblacion.Media   ? 1 :
              c.Poblacion == NivelPoblacion.Debil   ? 2 : 3)
```

---

## Bug 9 — Botones inconsistentes en Apiarios/Create

**Síntoma:** Los botones de "Cancelar" y "Guardar" del formulario de Apiarios tenían un estilo diferente al resto de la app.

**Fix:** Actualizado `Views/Apiarios/Create.cshtml` al patrón estándar:
```html
<div class="d-flex justify-content-between mt-4">
    <a asp-action="Index" class="btn btn-outline-secondary">Cancelar</a>
    <button type="submit" class="btn btn-primary">Guardar Apiario</button>
</div>
```

---

## Archivos modificados (resumen)

| Archivo | Cambios |
|---|---|
| `Views/Apiarios/Comparacion.cshtml` | Eliminado `<h2>` duplicado |
| `Controllers/TareasController.cs` | Endpoint AJAX `GetColmenasByApiario` |
| `Views/Tareas/Create.cshtml` | JS filtrado dinámico de colmenas |
| `Views/Trashumancia/Create.cshtml` + JS | `DistanciaKm` default `0` |
| `Models/Entities/Revision.cs` | `ColmenaId` y `ApiarioId` nullable |
| `Controllers/SanidadController.cs` | Validación ApiarioId obligatorio, colmenas opcionales |
| `Views/Sanidad/Create.cshtml` | Apiario obligatorio, colmena opcional, layout mejorado |
| `Views/Sanidad/Index.cshtml` | "Eliminar" → "Quitar" |
| `Views/Colmenas/Edit.cshtml` | Eliminado estado "Perdida", max date en reina |
| `Views/Colmenas/Create.cshtml` | max date en fecha nacimiento reina |
| `Views/Colmenas/DarDeBaja.cshtml` | Simplificado a confirmación directa |
| `Controllers/ColmenasController.cs` | DarDeBaja con returnUrl, orden de Población por enum |
| `Views/Apiarios/Create.cshtml` | Botones alineados al patrón estándar |
| Migración EF Core | Esquema de `Revision` actualizado |
