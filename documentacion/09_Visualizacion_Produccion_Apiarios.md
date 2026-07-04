# 🍯 Visualización de Producción Total por Apiario

**Chat de referencia:** `53de50d3-b4a1-4f69-a385-56fe26c57f85`  
**Fecha:** 2026-06-30  
**Área:** `Controllers/ApiariosController.cs`, `Views/Apiarios/Index.cshtml`, `Views/Apiarios/Details.cshtml`

---

## Contexto

El apicultor necesitaba ver de un vistazo cuánto había producido cada apiario en total (la suma de la miel recolectada en todas sus colmenas), tanto en las tarjetas del menú general como en la vista detallada de cada apiario.

---

## Cambios realizados

### 1. Modificación de Consultas en el Controlador
* **Problema:** El controlador de apiarios no cargaba las extracciones de miel al consultar los apiarios, por lo que no se disponía del dato en la vista.
* **Solución:** En `ApiariosController.cs`, se modificaron las consultas de los métodos `Index` y `Details` para incluir la carga anidada (`ThenInclude`) de la tabla de `Extracciones` a través de las `Colmenas`.

```csharp
// ApiariosController.cs
var apiarios = await _context.Apiarios
    .Include(a => a.Colmenas)
        .ThenInclude(c => c.Extracciones) // Carga anidada añadida
    .ToListAsync();
```

---

### 2. Actualización de Tarjetas en el Listado (Index)
* **Cambio:** En cada tarjeta del listado (`Views/Apiarios/Index.cshtml`), debajo del contador de colmenas activas, se calculó la producción acumulada sumando la cantidad de kilogramos de todas las extracciones de todas las colmenas pertenecientes al apiario, mostrándolo con el formato `Producido: {valor} kg`.

```html
<!-- Cómputo y renderizado en la tarjeta del apiario -->
@{
    var totalProducido = item.Colmenas.SelectMany(c => c.Extracciones).Sum(e => e.CantidadKg);
}
<div class="apiario-prod text-honey-dark mt-2 fw-bold">
    Producido: @totalProducido.ToString("N1") kg
</div>
```

---

### 3. Vista de Detalles del Apiario (Details)
* Se integró el dato de producción total en el panel de información general izquierdo.
* Se agregó la columna "Producido" en la tabla que lista las colmenas individuales del apiario, mostrando la producción específica de cada colmena para poder compararlas de manera directa.

---

## Archivos modificados

| Archivo | Cambio |
|---|---|
| `Controllers/ApiariosController.cs` | Inclusión de `.ThenInclude(c => c.Extracciones)` en consultas de base de datos. |
| `Views/Apiarios/Index.cshtml` | Despliegue de producción total en las tarjetas. |
| `Views/Apiarios/Details.cshtml` | Despliegue de estadísticas generales de producción y columna de producción por colmena. |
