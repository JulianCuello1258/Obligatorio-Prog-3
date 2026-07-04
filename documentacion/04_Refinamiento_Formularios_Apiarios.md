# 🏗️ Refinamiento de Formularios de Apiarios y Consistencia de UI

**Chat de referencia:** `01c698a9-9457-4cd3-91cd-9925ae46b493`  
**Fecha:** 2026-07-04  
**Área:** `Views/Apiarios/`, `Views/Colmenas/`, `Views/Produccion/`, `Views/Sanidad/`, `Views/Tareas/`, `Views/Trashumancia/`, `Services/WeatherService.cs`, `wwwroot/js/apiario-clima.js`

---

## Contexto

Sesión de correcciones masivas de UI con múltiples objetivos:
1. Eliminar información de "Competencia de Pecoreo" de apiarios.
2. Hacer los formularios de Crear y Editar **visualmente idénticos** (mismos campos y botones).
3. Eliminar títulos duplicados en todos los formularios.
4. Corregir error `InvalidOperationException` al registrar una Producción.

---

## Cambios realizados

### 1. Eliminación de "Competencia de Pecoreo"

**Pedido:**  
> "En la ubicación no me importa la competencia como en este ejemplo que lo dice. Quita ambas cosas."

**Dos lugares donde aparecía:**

**`Services/WeatherService.cs`:**
- Se eliminaron los mensajes `RazonesNo.Add(...)` para "Alta competencia por pecoreo" y "Competencia de pecoreo".
- El cálculo de puntaje geográfico interno sigue funcionando, pero ya no genera avisos visibles al usuario.

**`wwwroot/js/apiario-clima.js`:**
- Se eliminó la sección de JS que renderizaba el badge de "Competencia por Pecoreo" en el panel de detalles del apiario.

---

### 2. Formularios Crear y Editar — Campos idénticos

**Problema:** Los formularios de Apiarios `Create.cshtml` y `Edit.cshtml` tenían **campos diferentes** (diferente orden, diferentes controles para el campo `Tipo`).

**Fix en `Views/Apiarios/Edit.cshtml`:**
- Se alineó el orden de campos al mismo que `Create.cshtml`:
  - Fila 1: `Tipo` + `TrashumanciaHabilitada`
  - Fila 2: `Departamento` + `Paraje`
  - Fila 3: `Zona` + `SeccionPolicial`
- `Tipo` pasó a usar `asp-items` (enum select) igual que en Create.
- Botones de "Cancelar" y "Guardar" alineados con el patrón `d-flex justify-content-between`.

**Mismo fix aplicado a:** `Views/Colmenas/Edit.cshtml`

---

### 3. Eliminación de títulos duplicados en formularios

**Problema:** El `_Layout.cshtml` ya renderizaba `@ViewData["Title"]` como `<h1>` en el header global. Cada vista tenía además un `<h2>` con el mismo título dentro del formulario.

**Fix:** Se eliminó el `<h2>` redundante de todas las vistas afectadas:

| Vista | Título duplicado eliminado |
|---|---|
| `Views/Apiarios/Create.cshtml` | "Registrar Apiario" |
| `Views/Colmenas/Create.cshtml` | "Registrar Colmena" |
| `Views/Tareas/Create.cshtml` | "Nueva Tarea" |
| `Views/Sanidad/Create.cshtml` | "Nueva Revisión" |
| `Views/Produccion/Create.cshtml` | "Registrar Cosecha" |
| `Views/Trashumancia/Create.cshtml` | "Nuevo Traslado" |

---

### 4. Corrección — `InvalidOperationException` en Producción

**Error:**
```
InvalidOperationException: The model item passed into the ViewDataDictionary is of type 
'BeeKeeperApp.Models.ExtraccionCreateViewModel', but this ViewDataDictionary instance 
requires a model item of type 'BeeKeeperApp.Models.Entities.Extraccion'.
```

**Causa:** La vista `Produccion/Create.cshtml` declaraba `@model Extraccion` pero el controlador le pasaba un `ExtraccionCreateViewModel`.

**Fix:** Se reescribió `Produccion/Create.cshtml` para:
- Usar `@model BeeKeeperApp.Models.ExtraccionCreateViewModel`
- Exponer el campo `TipoRegistro` con un toggle de botones entre **"Por Colmena"** y **"Por Apiario"**
- Mostrar los campos correctos según el tipo seleccionado

---

## Archivos modificados

| Archivo | Cambio |
|---|---|
| `Services/WeatherService.cs` | Eliminados mensajes de competencia de pecoreo |
| `wwwroot/js/apiario-clima.js` | Eliminado badge de competencia en detalles |
| `Views/Apiarios/Edit.cshtml` | Campos reordenados, `Tipo` con `asp-items`, botones corregidos |
| `Views/Colmenas/Edit.cshtml` | Botones y estructura alineados con Create |
| `Views/Produccion/Create.cshtml` | Modelo cambiado a `ExtraccionCreateViewModel`, reescritura |
| Todas las vistas de Create | Eliminado `<h2>` interno duplicado |

---

## Patrón de botones estándar establecido

A partir de este chat se establece el patrón uniforme de botones para todos los formularios:

```html
<div class="d-flex justify-content-between mt-4">
    <a asp-action="Index" class="btn btn-outline-secondary">Cancelar</a>
    <button type="submit" class="btn btn-primary">Guardar</button>
</div>
```
