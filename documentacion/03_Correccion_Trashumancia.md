# 🚚 Corrección del Registro de Trashumancia

**Chat de referencia:** `86852df2-400b-4e58-8be3-888c9cf6bd77`  
**Fecha:** 2026-07-04  
**Área:** `Models/Entities/Trashumancia.cs`, `Controllers/TrashumanciaController.cs`, `Views/Trashumancia/Create.cshtml`

---

## Contexto

El formulario de **Registrar Traslado (Trashumancia)** fallaba silenciosamente: el usuario completaba todos los campos requeridos (Apiario Origen, Apiario Destino, Fecha, Colmena) y al intentar guardar, el registro no se procesaba.

---

## Diagnóstico

Se identificaron **dos causas raíz** del fallo:

### Causa 1 — Propiedad de navegación no-nullable en el modelo

En el modelo `Trashumancia.cs`, la propiedad de navegación:

```csharp
// ❌ ANTES — causaba el fallo
public Colmena Colmena { get; set; } = null!;
```

Al hacer POST, el **model binder de ASP.NET Core** no puede reconstruir objetos de navegación complejos (como `Colmena`) desde los campos del formulario. Como la propiedad era **no-nullable** (tipo referencia con `= null!`), el `ModelState` la marcaba como inválida, haciendo que la condición `if (ModelState.IsValid)` retornara `false` sin mostrar un error claro al usuario.

**Fix aplicado:**

```csharp
// ✅ DESPUÉS — propiedad de navegación como nullable
public Colmena? Colmena { get; set; }
```

Esto le indica al model binder que el campo es opcional en el POST — el valor real se carga desde la base de datos mediante el `ColmenaId` (FK).

---

### Causa 2 — Inconsistencia `ViewBag` vs `ViewData`

En el controlador:
- El método **GET** usaba `ViewData["Apiarios"]`
- El método **POST** (al devolver la vista tras error) usaba `ViewBag.Apiarios`

En ASP.NET Core, `ViewBag` es una capa dinámica sobre `ViewData`, pero las claves tienen que coincidir exactamente. Esto causaba que la lista de apiarios no estuviera disponible al re-renderizar la vista en caso de error.

**Fix:** Se unificó todo el uso a `ViewData["Apiarios"]` en ambos métodos del controlador.

---

## Archivos modificados

| Archivo | Cambio |
|---|---|
| `Models/Entities/Trashumancia.cs` | `Colmena` cambiado de `Colmena` (non-nullable) a `Colmena?` (nullable) |
| `Controllers/TrashumanciaController.cs` | Unificado uso de `ViewData["Apiarios"]` en GET y POST |

---

## Notas técnicas

- **Patrón correcto en ASP.NET MVC:** Las propiedades de **navegación** en modelos que se usan en formularios POST **siempre deben ser nullable** (`?`). Solo las claves foráneas (`ColmenaId`, `ApiarioId`) son necesarias para que EF Core resuelva la relación.
- El `ModelState.IsValid` falla silenciosamente cuando hay propiedades no-nullable que el model binder no puede poblar — es un error de diagnóstico difícil si no se logea el `ModelState`.

```csharp
// Tip de diagnóstico: para ver qué falla en ModelState
foreach (var error in ModelState.Values.SelectMany(v => v.Errors))
{
    Console.WriteLine(error.ErrorMessage);
}
```
