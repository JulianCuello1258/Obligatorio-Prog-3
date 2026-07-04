# Plan de Implementación de Mejoras (Actualizado con Feedback 2)

Plan de diseño e implementación de las modificaciones solicitadas, corregido en base al feedback del usuario.

## User Review Required

> [!IMPORTANT]
> **Sin Eliminación Física de Colmenas:**
> Respetando el historial de la colmena, **no** se implementará la acción `Delete` física para las colmenas. Se utilizará únicamente borrado lógico ("Dar de baja" a través de los estados `Inactiva` o `Perdida`).
>
> **Dar de Baja Flexible:**
> Al dar de baja una colmena, el usuario podrá elegir a qué estado pasarla (`Inactiva` o `Perdida`), en lugar de forzar siempre el estado `Perdida`.
>
> **Extracción por Apiario:**
> Al registrar una extracción por apiario completo, el peso total se dividirá únicamente entre las colmenas con estado `Activa` en dicho apiario.
>
> **Relación Obligatoria en Trashumancia:**
> El campo `ColmenaId` en `Trashumancia` será obligatorio (`int`), asegurando que cada traslado esté estrictamente vinculado a una colmena.
>
> **Reversión de Producción al Eliminar Extracción:**
> Si se elimina una extracción, se restaurará el peso extraído sumándolo de vuelta a la `ProduccionAcumulada` de la colmena correspondiente.
>
> **Filtrado de Colmenas en Dropdowns:**
> Se excluirán las colmenas en estado `Perdida` de todos los dropdowns de selección de colmena (en los flujos de Cosechas, Revisiones y Trashumancia).
>
> **Lista Base de Enfermedades y Tratamientos:**
> Los dropdowns en Revisiones se inicializarán con una lista predefinida de enfermedades (Varroasis, Loque Americana, Loque Europea, Nosemosis) y tratamientos conocidos (Ácido Oxálico, Ácido Fórmico, Timol, Amitraz), complementada dinámicamente con cualquier nuevo valor registrado por el usuario.

## Open Questions

*No hay preguntas de negocio pendientes tras la incorporación del feedback.*

## Proposed Changes

---

### 1. Modelos y Configuración de Datos

#### [MODIFY] [Colmena.cs](file:///c:/Users/Usuario/Documents/GitHub/Obligatorio/Models/Entities/Colmena.cs)
- Agregar la propiedad `public double ProduccionAcumulada { get; set; } = 0.0;` para registrar la producción acumulada de miel antes de su extracción.

#### [MODIFY] [Trashumancia.cs](file:///c:/Users/Usuario/Documents/GitHub/Obligatorio/Models/Entities/Trashumancia.cs)
- Agregar `public int ColmenaId { get; set; }` (requerido) y la relación `public Colmena Colmena { get; set; } = null!;`.

#### [MODIFY] [ApplicationDbContext.cs](file:///c:/Users/Usuario/Documents/GitHub/Obligatorio/Data/ApplicationDbContext.cs)
- Configurar la relación restrictiva para `Trashumancia.Colmena` en `OnModelCreating`.

---

### 2. Controladores (Lógica de Negocio)

#### [MODIFY] [ColmenasController.cs](file:///c:/Users/Usuario/Documents/GitHub/Obligatorio/Controllers/ColmenasController.cs)
- En `Create` [POST]:
  - Validar que todos los campos del alta sean obligatorios.
  - Validar que la fecha de nacimiento de la reina no sea superior al día de hoy.
  - Asegurar que el estado inicial solo pueda ser `Activa` o `Inactiva`.
- En `Edit` [POST]:
  - Validar fecha de nacimiento de la reina.
- Agregar acciones de baja lógica:
  - `GET: DarDeBaja(int id)`: Muestra el formulario para elegir el nuevo estado (`Inactiva` o `Perdida`).
  - `POST: DarDeBaja(int id, EstadoColmena nuevoEstado)`: Aplica el cambio de estado.
- **Nota:** No se agregará una acción `Delete` física para las colmenas.

#### [MODIFY] [ProduccionController.cs](file:///c:/Users/Usuario/Documents/GitHub/Obligatorio/Controllers/ProduccionController.cs)
- Modificar `Create` para recibir un ViewModel que permita registrar:
  - Cosecha por colmena (seleccionando apiario y luego colmena, excluyendo colmenas `Perdida`).
  - Cosecha por apiario (generalizada, dividiendo el peso total únicamente entre las colmenas `Activa` del apiario).
- En el registro de cosecha:
  - Reiniciar `ProduccionAcumulada` de la(s) colmena(s) extraídas a 0.
- Agregar acción `[HttpPost] Delete(int id)` para eliminar físicamente una extracción.
  - Al borrar una extracción, sumar su `CantidadKg` de vuelta a la `ProduccionAcumulada` de la colmena respectiva.
- Implementar soporte de ordenación (`sortBy`, `descending`) en la acción `Index`.

#### [MODIFY] [ExportController.cs](file:///c:/Users/Usuario/Documents/GitHub/Obligatorio/Controllers/ExportController.cs)
- En `CreateExportacion` [POST]:
  - Validar que la cantidad de barriles a exportar (multiplicada por 300 Kg) no exceda el stock total disponible.
  - El stock disponible se calcula como `Sum(Extracciones.CantidadKg) - Sum(Exportaciones.CantidadBarriles * 300)`.
- Agregar acción `[HttpPost] Delete(int id)` para eliminar exportaciones.
- Implementar soporte de ordenación (`sortBy`, `descending`) en la acción `Exportaciones`.

#### [MODIFY] [TrashumanciaController.cs](file:///c:/Users/Usuario/Documents/GitHub/Obligatorio/Controllers/TrashumanciaController.cs)
- En `Create` [POST]:
  - Recibir la colmena a trasladar (obligatoria, excluyendo colmenas `Perdida`) y actualizar su `ApiarioId` al `ApiarioDestinoId` inmediatamente tras el guardado.
- Agregar acción `[HttpPost] Delete(int id)` para eliminar movimientos de trashumancia.
- Implementar soporte de ordenación (`sortBy`, `descending`) en la acción `Index`.

#### [MODIFY] [TareasController.cs](file:///c:/Users/Usuario/Documents/GitHub/Obligatorio/Controllers/TareasController.cs)
- Agregar acción `[HttpPost] Delete(int id)` para eliminar tareas.
- Implementar soporte de ordenación (`sortBy`, `descending`) en la acción `Index`.

#### [MODIFY] [ApiariosController.cs](file:///c:/Users/Usuario/Documents/GitHub/Obligatorio/Controllers/ApiariosController.cs)
- Refinar la acción `Index` para dar soporte consistente a la ordenación por nombre, fecha (Id) y producción total.

#### [MODIFY] [SanidadController.cs](file:///c:/Users/Usuario/Documents/GitHub/Obligatorio/Controllers/SanidadController.cs)
- En `Create` [GET]:
  - Obtener la lista de enfermedades y tratamientos registrados en la BD, combinarlos con la lista base apícola (Varroasis, Loque, Oxálico, etc.) y pasarlos a la vista.
  - Filtrar colmenas para excluir las que estén en estado `Perdida`.
- Agregar acción `[HttpPost] Delete(int id)` para eliminar revisiones.
- Implementar soporte de ordenación (`sortBy`, `descending`) en la acción `Index`.

---

### 3. Vistas y Diseños de Interfaz (CSS & HTML)

#### [MODIFY] [beekeeper.css](file:///c:/Users/Usuario/Documents/GitHub/Obligatorio/wwwroot/css/beekeeper.css)
- Definir la clase `.btn-honey` para que tenga un recuadro/borde claro y estilo acorde al diseño.
- Asegurar que todos los botones tengan bordes y frames claramente identificables.

#### [MODIFY] [Home/Index.cshtml](file:///c:/Users/Usuario/Documents/GitHub/Obligatorio/Views/Home/Index.cshtml)
- Asegurar que todos los botones de "Acciones Rápidas" sigan la misma tipología y tengan recuadro.

#### [MODIFY] [Apiarios/Index.cshtml](file:///c:/Users/Usuario/Documents/GitHub/Obligatorio/Views/Apiarios/Index.cshtml)
- Unificar botones a la nueva tipología: "Detalles", "Editar" y "Eliminar".
- Reemplazar dropdown de ordenación por botones interactivos en línea con flechas ▲ / ▼.

#### [MODIFY] [Colmenas/Index.cshtml](file:///c:/Users/Usuario/Documents/GitHub/Obligatorio/Views/Colmenas/Index.cshtml)
- Hacer que las columnas ID, Fecha de Creación y Producción sean ordenables de forma ascendente/descendente con flechas interactivas.

#### [MODIFY] [Colmenas/Details.cshtml](file:///c:/Users/Usuario/Documents/GitHub/Obligatorio/Views/Colmenas/Details.cshtml)
- Mostrar estado de la colmena (destacando si está dada de baja/inactiva/perdida).
- Añadir botón de "Dar de Baja" si la colmena sigue activa o inactiva.

#### [NEW] [Colmenas/DarDeBaja.cshtml](file:///c:/Users/Usuario/Documents/GitHub/Obligatorio/Views/Colmenas/DarDeBaja.cshtml)
- Formulario para seleccionar el nuevo estado de baja (`Inactiva` o `Perdida`) con confirmación.

#### [MODIFY] [Colmenas/Create.cshtml](file:///c:/Users/Usuario/Documents/GitHub/Obligatorio/Views/Colmenas/Create.cshtml)
- Eliminar la opción "Perdida" del estado inicial.
- Hacer que todos los dropdowns tengan la primera opción como `-- Seleccionar --`.
- Agregar alertas generales de validación en la parte superior y spans de error específicos en español abajo de cada campo.
- Reordenar botones de pie: Cancelar a la izquierda, Guardar a la derecha.

#### [MODIFY] [Colmenas/Edit.cshtml](file:///c:/Users/Usuario/Documents/GitHub/Obligatorio/Views/Colmenas/Edit.cshtml)
- Asegurar que la tipología de botones y cancelaciones coincida con el formulario de Create.

#### [MODIFY] [Sanidad/Create.cshtml](file:///c:/Users/Usuario/Documents/GitHub/Obligatorio/Views/Sanidad/Create.cshtml)
- Convertir "Enfermedades detectadas" y "Tratamiento" en dropdowns con la opción "Otro" que revele dinámicamente un campo de texto.
- Agregar placeholders en todos los inputs.
- Unificar botones (Cancelar a la izquierda, Guardar a la derecha).

#### [MODIFY] [Produccion/Index.cshtml](file:///c:/Users/Usuario/Documents/GitHub/Obligatorio/Views/Produccion/Index.cshtml)
- Mover el botón "Completar Formulario del Ministerio de Ganadería" arriba a la derecha, en la misma línea que "Registrar Cosecha".
- Añadir botón de eliminar para cada extracción.
- Permitir ordenar por Fecha, Cantidad y Colmena.

#### [MODIFY] [Produccion/Create.cshtml](file:///c:/Users/Usuario/Documents/GitHub/Obligatorio/Views/Produccion/Create.cshtml)
- Permitir seleccionar "Tipo de Registro" (Por Colmena / Por Apiario).
- Añadir dropdown de apiarios y filtrar colmenas vía AJAX.
- Asegurar `-- Seleccionar --` en dropdowns y alinear botones.

#### [MODIFY] [Export/Exportaciones.cshtml](file:///c:/Users/Usuario/Documents/GitHub/Obligatorio/Views/Export/Exportaciones.cshtml)
- Eliminar el título duplicado "Exportaciones de Miel" (2) y colocar el botón "+ Nueva Exportación" al lado del título principal (o en su lugar correspondiente).
- Añadir botón para eliminar exportaciones individuales con alerta de confirmación.
- Permitir ordenación de columnas.

#### [MODIFY] [Export/CreateExportacion.cshtml](file:///c:/Users/Usuario/Documents/GitHub/Obligatorio/Views/Export/CreateExportacion.cshtml)
- Alinear botones y agregar alertas generales.

#### [MODIFY] [Trashumancia/Index.cshtml](file:///c:/Users/Usuario/Documents/GitHub/Obligatorio/Views/Trashumancia/Index.cshtml)
- Mostrar la colmena trasladada en la tabla.
- Añadir columna de acciones con botón "Eliminar".
- Permitir ordenación de columnas.

#### [MODIFY] [Trashumancia/Create.cshtml](file:///c:/Users/Usuario/Documents/GitHub/Obligatorio/Views/Trashumancia/Create.cshtml)
- Agregar selector de colmena (filtrado por el apiario de origen seleccionado).
- Alinear botones y agregar alertas.

#### [MODIFY] [Tareas/Index.cshtml](file:///c:/Users/Usuario/Documents/GitHub/Obligatorio/Views/Tareas/Index.cshtml)
- Agregar botón "Eliminar" para tareas.
- Permitir ordenación de columnas.

#### [MODIFY] [Tareas/Create.cshtml](file:///c:/Users/Usuario/Documents/GitHub/Obligatorio/Views/Tareas/Create.cshtml)
- Agregar placeholders en Título y Descripción.
- Alinear botones.

---

## Verification Plan

### Automated Tests
- Ejecutar `dotnet build` para comprobar que compile correctamente tras los cambios.
- Realizar las migraciones con `dotnet ef migrations add` y `dotnet ef database update` para validar la base de datos local.

### Manual Verification
- **Alta Colmena**: Intentar registrar sin fecha de reina; verificar que muestre error descriptivo abajo y alerta arriba.
- **Dar de baja Colmena**: Verificar que permita elegir entre Inactiva y Perdida y se refleje en los detalles conservando el historial.
- **Extracción de Miel**: Registrar extracción por colmena y por apiario entero. Verificar que se divida el peso únicamente entre las colmenas activas y se resetee su `ProduccionAcumulada`.
- **Eliminar Extracción**: Borrar una extracción y verificar que la `ProduccionAcumulada` de la colmena se incremente con el peso devuelto.
- **Dropdowns**: Verificar que ninguna colmena en estado `Perdida` aparezca en los dropdowns de Cosecha, Sanidad ni Trashumancia.
- **Exportación**: Intentar exportar más Kg de los disponibles. Validar que la alerta de error bloquee la operación.
- **Trashumancia**: Registrar un traslado de colmena, verificar que se muestre en el historial y que la colmena cambie de apiario.
- **Enfermedades y Tratamientos**: Agregar uno con la opción "Otro" y verificar que en la siguiente inspección aparezca listado en el dropdown.
- **Ordenación**: Hacer clic en los encabezados de tabla de las distintas pantallas y validar que las flechas ▲ / ▼ se ordenen y muestren correctamente.
- **Eliminación**: Probar borrar registros (tarea, exportación, etc.) y validar el diálogo de confirmación.

---

# Plan de Implementación — Iteración 3 (02/07/2026)

## Descripción

Segunda ronda de ajustes sobre la aplicación, solicitada por el usuario. Incluye correcciones visuales de botones, mejoras de accesibilidad en tablas, un sistema unificado de ordenamiento por dropdown más flecha, y corrección de la validación de exportación.

## Cambios Implementados

### 1. Botones `btn-honey` — Fondo Transparente

#### [MODIFY] [beekeeper.css](file:///c:/Users/Usuario/Documents/GitHub/Obligatorio/wwwroot/css/beekeeper.css)

- `background-color: transparent` aplicado a todos los botones `.btn-honey` (excepto en `.login-body`).
- `:hover` cambia a `background-color: rgba(242, 169, 0, 0.15)` — efecto sutil de amarillo translúcido.
- Se conservan el borde `2px solid var(--honey-dark)` y el `border-radius: 10px`.
- Botones afectados: `+ Registrar Apiario`, `+ Nueva Colmena`, `+ Nueva Tarea` (y "Completar"), `+ Nueva Inspección Sanitaria`, `+ Registrar Cosecha`, `+ Nueva Exportación`, `+ Registrar Traslado`.

### 2. Mejoras de Tablas

#### [MODIFY] [Colmenas/Index.cshtml](file:///c:/Users/Usuario/Documents/GitHub/Obligatorio/Views/Colmenas/Index.cshtml)

- Tabla envuelta en `<div class="table-responsive">`.
- Clase `align-middle` en `<table>` para centrado vertical de celdas.
- Clase `text-nowrap` en todos los `<th>` y `<td>` no narrativos.
- Columna Población muestra `"-"` cuando el valor es `null`.
- Columna Acciones con `text-end` y `justify-content-end`.

#### [MODIFY] [Tareas/Index.cshtml](file:///c:/Users/Usuario/Documents/GitHub/Obligatorio/Views/Tareas/Index.cshtml)

- Tabla envuelta en `<div class="table-responsive">`.
- Clase `align-middle` en `<table>`.
- Clase `text-nowrap` en cabeceras y celdas no narrativas.
- Columna Título muestra `"-"` cuando el campo está vacío.

### 3. Sistema de Ordenamiento Unificado (Dropdown + Flecha)

Se reemplazaron los botones de cabecera en las tablas por un control único en la barra superior, alineado con el botón "+ Nuevo...":

- `<select name="sortBy">` con opciones específicas para cada sección.
- Botón `▲` / `▼` que alterna la dirección (`descending=true/false`) sin tocar el campo de ordenación.
- El `<select>` usa `onchange="this.form.submit()"` para aplicar el criterio inmediatamente.
- La flecha muestra `▲` cuando `Descending=true` (mayor a menor) y `▼` cuando `Descending=false` (menor a mayor).

#### Vistas actualizadas:

| Sección          | Criterios de Orden               | Archivo                                  |
|------------------|----------------------------------|------------------------------------------|
| Apiarios         | Nombre, Fecha de Creación, Producción | [Index.cshtml](file:///c:/Users/Usuario/Documents/GitHub/Obligatorio/Views/Apiarios/Index.cshtml) |
| Colmenas         | ID, Fecha Creación, Producido, Población, Tipo | [Index.cshtml](file:///c:/Users/Usuario/Documents/GitHub/Obligatorio/Views/Colmenas/Index.cshtml) |
| Inspecciones     | Fecha, Colmena, Enfermedad       | [Index.cshtml](file:///c:/Users/Usuario/Documents/GitHub/Obligatorio/Views/Sanidad/Index.cshtml) |
| Producción       | Fecha, Colmena, Cantidad         | [Index.cshtml](file:///c:/Users/Usuario/Documents/GitHub/Obligatorio/Views/Produccion/Index.cshtml) |
| Exportaciones    | Fecha, Destino, Cantidad         | [Exportaciones.cshtml](file:///c:/Users/Usuario/Documents/GitHub/Obligatorio/Views/Export/Exportaciones.cshtml) |
| Trashumancia     | Fecha, Distancia                 | [Index.cshtml](file:///c:/Users/Usuario/Documents/GitHub/Obligatorio/Views/Trashumancia/Index.cshtml) |
| Tareas           | Fecha Programada, Título, Estado | [Index.cshtml](file:///c:/Users/Usuario/Documents/GitHub/Obligatorio/Views/Tareas/Index.cshtml) |

#### Controladores actualizados:

| Controlador           | Cambio                                              |
|-----------------------|-----------------------------------------------------|
| [ApiariosController.cs](file:///c:/Users/Usuario/Documents/GitHub/Obligatorio/Controllers/ApiariosController.cs) | Migrado de `sortOrder` a `sortBy + descending`; soporte para Nombre, Fecha y Producción |
| [ColmenasController.cs](file:///c:/Users/Usuario/Documents/GitHub/Obligatorio/Controllers/ColmenasController.cs) | Agregados casos `Poblacion` y `Tipo` al `switch` |
| [SanidadController.cs](file:///c:/Users/Usuario/Documents/GitHub/Obligatorio/Controllers/SanidadController.cs) | Agregado caso `Enfermedad` al `switch` |

### 4. Validación de Exportación — Corrección

#### [MODIFY] [ExportController.cs](file:///c:/Users/Usuario/Documents/GitHub/Obligatorio/Controllers/ExportController.cs)

- Cuando `cantidadKg` es negativa o excede el stock disponible, ahora también se llama `ModelState.AddModelError("cantidadKg", ...)` para generar un mensaje de error visible en el campo del formulario.
- Se guarda `ViewBag.CantidadKg = cantidadKg` al retornar la vista en casos de error, para que el campo muestre el valor que el usuario había ingresado.

#### [MODIFY] [CreateExportacion.cshtml](file:///c:/Users/Usuario/Documents/GitHub/Obligatorio/Views/Export/CreateExportacion.cshtml)

- Input `cantidadKg` ahora enlaza `value="@ViewBag.CantidadKg"` para recuperar el valor al volver del servidor.
- Clase `is-invalid` se aplica si hay un error de ModelState para ese campo.
- `<div class="invalid-feedback">` muestra el mensaje de error del servidor directamente bajo el input.
- Se agregó la función JavaScript `validateQuantity()` que:
  - Bloquea el submit del formulario si `kg <= 0` o `kg > stockMaxKg`.
  - Agrega la clase `is-invalid` y el mensaje de error en tiempo real.
  - Se llama tanto en el evento `input` como en el `submit`.
  - Al cargar la página, si ya hay un valor precompletado, recalcula barriles y re-valida.

