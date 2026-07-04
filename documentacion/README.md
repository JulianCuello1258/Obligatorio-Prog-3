# 📚 Documentación del Proyecto BeeKeeperApp

Registro de todas las sesiones de desarrollo, organizadas por tema.  
Cada archivo documenta los cambios realizados, el problema original, la causa y la solución aplicada.

---

## Índice de documentos

| # | Archivo | Tema | Fecha |
|---|---|---|---|
| 01 | [01_Registro_Cosecha_UI.md](./01_Registro_Cosecha_UI.md) | UI del formulario "Registrar Cosecha" y sidebar | 2026-07-04 |
| 02 | [02_Registro_Masivo_Colmenas.md](./02_Registro_Masivo_Colmenas.md) | Registro en cantidad de colmenas + validación de reina | 2026-07-04 |
| 03 | [03_Correccion_Trashumancia.md](./03_Correccion_Trashumancia.md) | Bug crítico en registro de trashumancia | 2026-07-04 |
| 04 | [04_Refinamiento_Formularios_Apiarios.md](./04_Refinamiento_Formularios_Apiarios.md) | Consistencia UI entre formularios Crear y Editar | 2026-07-04 |
| 05 | [05_Correccion_Bugs_Generales.md](./05_Correccion_Bugs_Generales.md) | Corrección masiva: tareas, inspecciones, colmenas, fechas | 2026-07-04 |
| 06 | [06_Declaracion_Jurada_Impresion.md](./06_Declaracion_Jurada_Impresion.md) | Formato e impresión de la Declaración Jurada | 2026-06-30 |
| 07 | [07_Evaluacion_Aptitud_Historica.md](./07_Evaluacion_Aptitud_Historica.md) | Aptitud geográfica basada en clima de 180 días | 2026-06-30 |
| 08 | [08_Limpieza_Buscadores_Filtros.md](./08_Limpieza_Buscadores_Filtros.md) | Ordenar por producción y limpieza de barras vacías | 2026-06-29 |
| 09 | [09_Visualizacion_Produccion_Apiarios.md](./09_Visualizacion_Produccion_Apiarios.md) | Carga total producida de miel por cada apiario | 2026-06-30 |
| 10 | [10_Autocompletado_Ubicacion_Mapa.md](./10_Autocompletado_Ubicacion_Mapa.md) | Autocompletar datos MGAP desde marcador Leaflet | 2026-06-29 |
| 11 | [11_Comportamiento_Boton_Quitar.md](./11_Comportamiento_Boton_Quitar.md) | Botón "Quitar" sin efectos secundarios destructivos | 2026-07-01 |
| 12 | [12_Declaracion_Jurada_MGAP.md](./12_Declaracion_Jurada_MGAP.md) | Restricción numérica y Sección D (exoneración) | 2026-06-29 |
| 13 | [13_Fondo_Pantalla_Web.md](./13_Fondo_Pantalla_Web.md) | Sustitución de imagen de fondo principal e inicio | 2026-06-22 |
| 14 | [14_Estilos_Botones_Tablas_Ordenamiento.md](./14_Estilos_Botones_Tablas_Ordenamiento.md) | Botones translúcidos, tablas centradas y orden en 7 vistas | 2026-07-02 |
| 15 | [15_Servicio_Clima_OpenMeteo.md](./15_Servicio_Clima_OpenMeteo.md) | Integración inicial y backend del servicio Open-Meteo | 2026-06-16 |
| 16 | [16_Seguridad_APIs.md](./16_Seguridad_APIs.md) | Auditoría de API Keys y seguridad de base de datos | 2026-06-30 |

---

## Resumen de áreas cubiertas

### 🗂️ Formularios y UI
- Consistencia visual entre formularios Crear/Editar
- Eliminación de títulos duplicados
- Patrón estándar de botones Cancelar/Guardar
- Sidebar scrollable con botón Cerrar Sesión accesible
- Panel inicial "Por Apiario" en Cosecha

### 🐛 Bugs de lógica
- Trashumancia: `ModelState` inválido por propiedad de navegación non-nullable
- `ViewBag` vs `ViewData` inconsistentes en controladores
- `ExtraccionCreateViewModel` vs `Extraccion` como modelo de vista
- `DistanciaKm` vacío causando fallo silencioso en formulario

### ⚙️ Funcionalidades nuevas
- Registro masivo de colmenas (campo "Cantidad")
- Filtrado dinámico de colmenas por apiario (AJAX) en Tareas
- Inspecciones: apiario obligatorio, colmena opcional
- Tratamiento: segunda dosis obligatoria solo si hay tratamiento seleccionado
- Dar de Baja simplificado con `returnUrl`

### 🗄️ Base de datos / Migraciones
- `Revision.ColmenaId` y `ApiarioId` cambiados a nullable
- Migración EF Core generada y aplicada

### 🔒 Validaciones de datos
- Fecha de nacimiento de reina: máximo hoy
- Segunda dosis: mayor al día de hoy
- Fechas de producciones, trashumancias e inspecciones históricas: **permitidas** (el usuario puede registrar eventos pasados)
- Validación JS de "Reina Presente": deshabilita campos al desmarcar

---

> **Nota:** Este índice se actualiza continuamente. Los chats futuros serán documentados con número correlativo.
