# Resumen del Trabajo Realizado (Walkthrough)

Se han completado todas las modificaciones y mejoras planificadas en el sistema de gestión apícola **BeeKeeperApp**. A continuación se detallan los cambios realizados, las pruebas de verificación y los resultados.

## Cambios Implementados

### 1. Gestión de Colmenas (Baja Lógica)
- **Eliminación Física Desactivada:** Se quitó la acción `Delete` física para las colmenas.
- **Baja Lógica Adaptable:** Creada la nueva vista [DarDeBaja.cshtml](file:///c:/Users/Usuario/Documents/GitHub/Obligatorio/Views/Colmenas/DarDeBaja.cshtml) y las acciones en [ColmenasController.cs](file:///c:/Users/Usuario/Documents/GitHub/Obligatorio/Controllers/ColmenasController.cs), permitiendo al usuario elegir el motivo de baja (`Inactiva` o `Perdida`).
- **Validación del Registro:** El formulario de alta en [Create.cshtml](file:///c:/Users/Usuario/Documents/GitHub/Obligatorio/Views/Colmenas/Create.cshtml) ahora exige todos los campos obligatorios, valida que la fecha de nacimiento de la reina no supere la fecha de hoy, muestra alertas de error en la cabecera y spans descriptivos individuales en español.
- **Visualización en Detalles:** El estado lógico se destaca con badges de colores en [Details.cshtml](file:///c:/Users/Usuario/Documents/GitHub/Obligatorio/Views/Colmenas/Details.cshtml), con acceso directo al botón de dar de baja.

### 2. Producción y Extracciones (Cosecha por Apiario o Colmena)
- **Extracción Flexible:** Modificada la vista [Create.cshtml](file:///c:/Users/Usuario/Documents/GitHub/Obligatorio/Views/Produccion/Create.cshtml) y el controlador [ProduccionController.cs](file:///c:/Users/Usuario/Documents/GitHub/Obligatorio/Controllers/ProduccionController.cs) para soportar extracciones:
  - **Por Colmena:** Filtra dinámicamente mediante AJAX las colmenas del apiario seleccionado (excluyendo colmenas en estado `Perdida`).
  - **Por Apiario:** Divide equitativamente el peso total extraído únicamente entre las colmenas con estado `Activa` dentro del apiario.
- **Reinicio y Restauración de Stock:**
  - Al realizar una extracción, se reinicia la `ProduccionAcumulada` de la(s) colmena(s) cosechada(s) a `0`.
  - Se agregó la acción de eliminar extracciones en [Index.cshtml](file:///c:/Users/Usuario/Documents/GitHub/Obligatorio/Views/Produccion/Index.cshtml). Al eliminar una extracción, se **restaura** la producción acumulada de la colmena correspondiente sumando de vuelta la cantidad removida.

### 3. Control de Stock en Exportaciones
- **Validación de Capacidad:** Al registrar una nueva exportación en [CreateExportacion.cshtml](file:///c:/Users/Usuario/Documents/GitHub/Obligatorio/Views/Export/CreateExportacion.cshtml), el sistema valida que el volumen solicitado (multiplicado por 300 Kg por barril) no exceda el stock total disponible (`Sum(Extracciones) - Sum(Exportaciones)`).
- **Control de Eliminación:** Agregada la acción de eliminar exportaciones con confirmación y retroalimentación inmediata sobre el stock.
- **Diseño Limpio:** Se eliminó la repetición del título "Exportaciones de Miel (2)" y se alinearon los botones de control.

### 4. Trashumancia Obligatoria y Actualización Automática
- **Colmena Requerida:** La relación entre Trashumancia y Colmena ahora es estrictamente requerida en el modelo [Trashumancia.cs](file:///c:/Users/Usuario/Documents/GitHub/Obligatorio/Models/Entities/Trashumancia.cs).
- **Actualización Inmediata:** Al guardar un traslado, se actualiza automáticamente el `ApiarioId` de la colmena al `ApiarioDestinoId`.
- **Restauración al Eliminar:** Si se elimina un traslado, la colmena vuelve automáticamente al `ApiarioOrigenId`.

### 5. Control Sanitario (Dropdowns Dinámicos y Dictado)
- **Seeding y dynamic-options:** Los campos de Enfermedad y Tratamiento se convirtieron en dropdowns en [Create.cshtml](file:///c:/Users/Usuario/Documents/GitHub/Obligatorio/Views/Sanidad/Create.cshtml). Se alimentan de una lista base inicial combinada dinámicamente con cualquier valor previamente ingresado en la BD.
- **Opción "Otro":** Al seleccionar "Otro", se revela un input de texto para permitir agregar nuevas enfermedades o tratamientos, persistiendo el dato.

### 6. Ordenación General (▲ / ▼)
- Se implementaron cabeceras de ordenación con flechas indicadoras en todas las pantallas principales:
  - **Apiarios:** Ordenación por Nombre y Fecha.
  - **Colmenas:** Ordenación por ID, Fecha de Creación y Producción Acumulada.
  - **Producción:** Ordenación por Fecha, Cantidad y Colmena.
  - **Exportación:** Ordenación por Fecha, Cantidad y Destino.
  - **Trashumancia:** Ordenación por Fecha y Distancia.
  - **Tareas:** Ordenación por Fecha, Título y Estado.

### 7. Aislamiento Estilístico del Login y Unificaciones de Botones
- **Aislamiento del Login:** Se modificó [beekeeper.css](file:///c:/Users/Usuario/Documents/GitHub/Obligatorio/wwwroot/css/beekeeper.css) para que las reglas de diseño y marcos de los botones excluyan la página de login (`body:not(.login-body)`), restaurando el estilo y colores originales del botón de acceso.
- Los botones principales de acción han sido estandarizados con etiquetas claras ("Detalles", "Editar" y "Eliminar") con diálogos interactivos de confirmación (`confirm(...)`).
- Los formularios ahora tienen el diseño consistente: botón **Cancelar** en color gris/outline a la izquierda, y botón **Guardar** (o acción principal `.btn-honey`) a la derecha.

---

## Verificación Realizada

1. **Compilación Correcta:** Ejecutado `dotnet build` con resultado exitoso: **0 Advertencias y 0 Errores**.
2. **Base de Datos Migrada Exitosamente:** Solucionado el conflicto de clave foránea al aplicar la migración en registros preexistentes de Trashumancia (actualizando el valor por defecto a una colmena válida antes de forzar la restricción FK). La migración se aplicó exitosamente mediante `dotnet ef database update`.

---

# Walkthrough — Iteración 3 (02/07/2026)

## Resumen

Segunda ronda de mejoras aplicadas al sistema de gestión apícola **BeeKeeperApp**, enfocada en refinamiento visual, usabilidad y corrección de validaciones críticas.

## Cambios Realizados

### 1. Botones `btn-honey` Transparentes

- **Archivo:** [beekeeper.css](file:///c:/Users/Usuario/Documents/GitHub/Obligatorio/wwwroot/css/beekeeper.css)
- Se cambió `background-color: var(--honey-primary)` por `transparent` en `.btn-honey`.
- El efecto `:hover` ahora aplica un fondo amarillo translúcido (`rgba(242, 169, 0, 0.15)`).
- Esto afecta a todos los botones de creación (`+ Registrar Apiario`, `+ Nueva Colmena`, `+ Nueva Tarea`, `Completar`, etc.) en toda la aplicación, **sin tocar** el botón de login.

### 2. Tablas Mejoradas (Colmenas y Tareas)

- **Archivos:** [Colmenas/Index.cshtml](file:///c:/Users/Usuario/Documents/GitHub/Obligatorio/Views/Colmenas/Index.cshtml), [Tareas/Index.cshtml](file:///c:/Users/Usuario/Documents/GitHub/Obligatorio/Views/Tareas/Index.cshtml)
- Se envolvieron las tablas en `<div class="table-responsive">` para prevenir desbordamientos horizontales.
- Se agregó `align-middle` en `<table>` para que todo el contenido de las filas esté centrado verticalmente.
- Los encabezados y celdas no narrativas usan `text-nowrap` para evitar saltos de línea indeseados en columnas como "Fecha Creación" o "✅ Presente".
- La columna Población ahora muestra `"-"` en lugar de un espacio vacío cuando el valor no fue asignado.
- La columna Acciones tiene los botones alineados a la derecha con `justify-content-end`.

### 3. Sistema de Ordenamiento Unificado (7 Secciones)

- **Archivos modificados:** Vistas de Apiarios, Colmenas, Inspecciones, Producción, Exportaciones, Trashumancia y Tareas
- **Archivos de controladores:** [ApiariosController.cs](file:///c:/Users/Usuario/Documents/GitHub/Obligatorio/Controllers/ApiariosController.cs), [ColmenasController.cs](file:///c:/Users/Usuario/Documents/GitHub/Obligatorio/Controllers/ColmenasController.cs), [SanidadController.cs](file:///c:/Users/Usuario/Documents/GitHub/Obligatorio/Controllers/SanidadController.cs)

Cada vista ahora tiene en la barra superior (junto al botón "+ Nuevo...") un bloque de controles de ordenamiento:

```
[Ordenar por:] [▼ Dropdown con criterios] [▲/▼ Botón de dirección]
```

- **Dropdown:** Se envía al servidor automáticamente al cambiar con `onchange="this.form.submit()"`.
- **Flecha ▲/▼:** Alterna la dirección del orden (`descending=true/false`) como un link `GET`.
- La flecha muestra `▲` cuando está en modo "mayor a menor" (descending=true) y `▼` cuando está en modo "menor a mayor" (descending=false).

| Sección      | Criterios disponibles |
|--------------|----------------------|
| Apiarios     | Nombre, Fecha de Creación, Producción |
| Colmenas     | ID, Fecha Creación, Producido, Población, Tipo |
| Inspecciones | Fecha, Colmena, Enfermedad |
| Producción   | Fecha, Colmena, Cantidad |
| Exportaciones| Fecha, Destino, Cantidad |
| Trashumancia | Fecha, Distancia |
| Tareas       | Fecha Programada, Título, Estado |

### 4. Validación de Exportación — Corregida

- **Archivos:** [ExportController.cs](file:///c:/Users/Usuario/Documents/GitHub/Obligatorio/Controllers/ExportController.cs), [CreateExportacion.cshtml](file:///c:/Users/Usuario/Documents/GitHub/Obligatorio/Views/Export/CreateExportacion.cshtml)

**Problema:** Al ingresar valores extremos como `10000000000000000000000000`, el formulario se enviaba sin mostrar ningún error.

**Solución aplicada:**

1. **Cliente (JavaScript):** Se creó la función `validateQuantity()` que:
   - Valida en tiempo real en el evento `input`.
   - Bloquea el `submit` del formulario si el valor supera el stock disponible.
   - Muestra clase `is-invalid` y el mensaje de error directamente bajo el campo.

2. **Servidor (C#):** Si la validación del servidor detecta que `cantidadKg > disponibleKg`:
   - Se agrega `ModelState.AddModelError("cantidadKg", ...)` con mensaje descriptivo.
   - Se guarda `ViewBag.CantidadKg = cantidadKg` para que el campo no quede en blanco al volver la vista.
   - El campo del formulario usa `value="@ViewBag.CantidadKg"` para restaurar el valor digitado.

## Verificación

- La aplicación compila y corre con `dotnet watch run` sin errores.
- Los botones `btn-honey` aparecen transparentes con borde oscuro en todas las páginas excepto el login.
- Las tablas de Colmenas y Tareas no presentan el problema de desalineación vertical.
- El ordenamiento por dropdown funciona en todas las 7 secciones; la flecha cambia correctamente de dirección.
- Al intentar exportar `10000000000000000000000000` kg, el formulario bloquea el envío con un mensaje de error en rojo bajo el campo.

