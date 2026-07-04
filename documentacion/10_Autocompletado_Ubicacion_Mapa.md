# 🗺️ Autocompletado de Ubicación por Mapa y Coordenadas

**Chat de referencia:** `83206139-7016-4f9e-ad65-5444e7e9d4d8`  
**Fecha:** 2026-06-29  
**Área:** `wwwroot/js/apiario-clima.js`, `Services/WeatherService.cs`, `Views/Apiarios/Create.cshtml`, `Views/Apiarios/Edit.cshtml`

---

## Contexto

Al registrar o editar un apiario, el apicultor debía rellenar manualmente los campos políticos e institucionales requeridos por el Ministerio de Ganadería, Agricultura y Pesca (MGAP): **Departamento**, **Sección Policial**, **Zona** e indicar si el apiario tenía habilitada la **Trashumancia**.

**El problema:** El usuario no siempre conoce de memoria la sección policial o el nombre del paraje de unas coordenadas de campo específicas, lo que resultaba en errores o registros incompletos.

---

## Cambios realizados

### 1. Geolocalización Inversa e Inferencia en Servidor
* En `WeatherService.cs`, se aprovechó la consulta a la API de geocodificación (Nominatim de OpenStreetMap) y una lógica de estimación matemática local para inferir la **Sección Policial** en función de la cercanía a límites departamentales y coordenadas geográficas.
* El objeto de transferencia de datos `ClimaAptitudDto` fue extendido para transportar estos datos hacia la interfaz de usuario:
  * `Departamento` (Ej: Colonia, Tacuarembó, Rivera).
  * `Paraje` (Localidad rural más cercana).
  * `SeccionPolicial` (Identificador de sección policial estimado).
  * `Zona` (Rural / Urbana).
  * `SugerirTrashumanciaHabilitada` (Verdadero si el puntaje de aptitud general del terreno es mayor o igual a 50).

---

### 2. Autocompletado en Tiempo Real (JavaScript)
* En `wwwroot/js/apiario-clima.js`, se vinculó el evento de selección de pin en el mapa (`Map click`) y el cambio manual en los inputs de `Latitud` y `Longitud`.
* Al cambiar la ubicación en el mapa, se gatilla una llamada AJAX al servicio de clima. Los datos geográficos devueltos por el backend se inyectan automáticamente en los campos del formulario de registro y edición de apiario.

```javascript
// Inyección de datos geográficos devueltos por el servicio Clima
document.getElementById('Departamento').value = data.departamento || '';
document.getElementById('SeccionPolicial').value = data.seccionPolicial || '';
document.getElementById('Paraje').value = data.paraje || '';
document.getElementById('Zona').value = data.zona || 'Rural';

// Checkbox de habilitación de trashumancia sugerido
const trashumanciaCheckbox = document.getElementById('TrashumanciaHabilitada');
if (trashumanciaCheckbox) {
    trashumanciaCheckbox.checked = data.sugerirTrashumanciaHabilitada;
}
```

---

### 3. Feedback Visual de Autocompletado (Animación CSS Glow)
* Para que el usuario note que la aplicación ha rellenado los campos por él, se implementó una animación de parpadeo/brillo dorado (`glow-fill`) en los bordes de los campos afectados por el autocompletado.

```css
/* Animación en beekeeper.css */
@keyframes autofill-glow {
    0% { border-color: rgba(212, 163, 89, 0.2); box-shadow: 0 0 0 rgba(212, 163, 89, 0); }
    50% { border-color: #d4a359; box-shadow: 0 0 15px rgba(212, 163, 89, 0.6); }
    100% { border-color: rgba(212, 163, 89, 0.2); box-shadow: 0 0 0 rgba(212, 163, 89, 0); }
}

.autofilled-highlight {
    animation: autofill-glow 1.5s ease-in-out;
}
```

---

## Archivos modificados

| Archivo | Cambio |
|---|---|
| `Services/WeatherService.cs` | Cálculo y asignación de `SeccionPolicial`, `Departamento` y `SugerirTrashumanciaHabilitada` en `ProcessSuitability`. |
| `wwwroot/js/apiario-clima.js` | Recepción de variables en AJAX e inyección en formulario con temporizador para remover la clase de animación. |
| `wwwroot/css/beekeeper.css` | Adición de clase `.autofilled-highlight` y animación keyframe. |

---

## Notas técnicas
* La estimación de la **Sección Policial** utiliza un fallback de distancias euclidianas a centros referenciales cuando la geolocalización inversa no contiene la etiqueta de sección directamente del catastro de OpenStreetMap. Esto asegura que siempre haya una sección policial sugerida al usuario en cualquier coordenada de Uruguay.
