# 🗑️ Comportamiento Simplificado del Botón "Quitar"

**Chat de referencia:** `798eab68-bb04-4e26-a249-be6a574340c6`  
**Fecha:** 2026-07-01  
**Área:** `Controllers/` (`ProduccionController.cs`, `TrashumanciaController.cs`, `ExportController.cs`), `Views/` (`Produccion/Index.cshtml`, `Trashumancia/Index.cshtml`, `Export/Exportaciones.cshtml`)

---

## Contexto

Originalmente, la eliminación de ciertos registros históricos desencadenaba "efectos secundarios" automáticos en el sistema para intentar revertir los cambios en las entidades. Por ejemplo:
* Eliminar un registro de **Trashumancia** devolvía automáticamente la colmena a su apiario de origen original.
* Eliminar una **Cosecha (Producción)** realizaba cálculos inversos automáticos para ajustar contadores acumulados históricos.

**El problema:** Este comportamiento restrictivo interfería con la flexibilidad que necesita el apicultor, quien muchas veces solo desea corregir un error de transcripción o limpiar el historial visual sin alterar el estado real de sus apiarios y colmenas actuales.

---

## Cambios realizados

### 1. Eliminación sin efectos secundarios (Backend)
Se reescribieron las acciones de borrado (`DeleteConfirmed` / `Delete`) en los controladores de Producción, Exportación y Trashumancia para que únicamente remuevan la fila correspondiente de la base de datos:
* **`ProduccionController.cs`:** Al quitar una extracción, no se altera la colmena asociada.
* **`TrashumanciaController.cs`:** Al quitar un traslado, la colmena permanece en su apiario de destino actual (no se revierte al de origen).
* **`ExportController.cs`:** Se mantiene el borrado directo de la declaración o registro de exportación.

---

### 2. Cambios en la UI: "Eliminar" por "Quitar"
* Se renombraron todas las etiquetas de los botones de `Eliminar` a **`Quitar`**.
* Se actualizaron los textos de confirmación JS (`onsubmit="return confirm(...)"`) para notificar al usuario de forma transparente que la eliminación **únicamente removerá el registro del historial** y no revertirá los traslados físicos de las colmenas ni alterará otras estadísticas del sistema.

```html
<!-- Ejemplo en Trashumancia/Index.cshtml -->
<form asp-action="Delete" asp-route-id="@item.Id" method="post" style="display:inline" 
      onsubmit="return confirm('¿Está seguro de que desea quitar este traslado? El registro se eliminará pero la colmena permanecerá en su apiario actual.');">
    <button type="submit" class="btn btn-sm btn-outline-danger">Quitar</button>
</form>
```

---

## Archivos modificados

| Archivo | Cambio |
|---|---|
| `Controllers/ProduccionController.cs` | Simplificación del método `Delete` para remover la extracción de miel de forma directa. |
| `Controllers/TrashumanciaController.cs` | Remoción de la lógica de reversión de apiario de origen en el borrado. |
| `Views/Produccion/Index.cshtml` | Renombrado a "Quitar" y alerta de advertencia actualizada. |
| `Views/Trashumancia/Index.cshtml` | Renombrado a "Quitar" y alerta de advertencia de no-reversión. |
| `Views/Export/Exportaciones.cshtml` | Renombrado a "Quitar" con diálogo de confirmación modificado. |
