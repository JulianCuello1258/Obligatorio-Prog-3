# 📦 Registro de Cosecha — Mejoras de UI

**Chat de referencia:** `42d5733c-4682-4bc4-9bd5-c592ed8c03fe`  
**Fecha:** 2026-07-04  
**Área:** `Views/Produccion/Create.cshtml`, `_Layout.cshtml`, CSS del sidebar

---

## Contexto

Se realizaron ajustes de interfaz en el formulario de **Registrar Cosecha** y en el **sidebar de navegación** de la aplicación BeeKeeper.

---

## Cambios realizados

### 1. Formulario "Registrar Cosecha" — Vista por Apiario por defecto

**Problema:** Al ingresar al formulario de registrar cosecha, se mostraba primero el panel "Por Colmena". El usuario debía hacer clic manualmente en "Por Apiario" para cambiar la vista.

**Causa:** El estado inicial estaba definido directamente en el HTML. Aunque el JavaScript al final cambiaba el tipo seleccionado, el panel "Por Colmena" aparecía visiblemente antes (parpadeo).

**Solución aplicada en `Views/Produccion/Create.cshtml`:**
- `panelColmena` → ahora inicia con `style="display:none;"`
- `panelApiario` → ahora inicia visible (sin `display:none`)
- `btnColmena` → inicia con clase `btn-outline-*` (no activo)
- `btnApiario` → inicia con clase activa

```html
<!-- ANTES -->
<div id="panelApiario" style="display:none;">...</div>
<div id="panelColmena">...</div>

<!-- DESPUÉS -->
<div id="panelApiario">...</div>
<div id="panelColmena" style="display:none;">...</div>
```

---

### 2. Sidebar — Reordenamiento de secciones

**Cambio:** El componente **Mapa** fue movido debajo de la sección **Tareas** en el sidebar de navegación lateral (`_Layout.cshtml`).

**Orden resultante del sidebar:**
1. Apiarios
2. Colmenas
3. Tareas
4. 🗺️ Mapa *(movido aquí)*
5. Producción
6. Sanidad
7. Trashumancia

---

### 3. Sidebar — Botón "Cerrar Sesión" accesible y scrollable

**Problema:** El botón de **Cerrar Sesión** quedaba fuera de la vista cuando el sidebar tenía mucho contenido.

**Solución:**
- El botón fue movido a una posición más alta en el sidebar.
- Se añadió `overflow-y: auto` al contenedor del sidebar para permitir scroll.
- La barra de desplazamiento fue ocultada visualmente (manteniendo la funcionalidad) con CSS:

```css
/* Scroll funcional sin barra visible */
.sidebar-nav {
    overflow-y: auto;
    scrollbar-width: none;       /* Firefox */
    -ms-overflow-style: none;    /* IE/Edge */
}
.sidebar-nav::-webkit-scrollbar {
    display: none;               /* Chrome/Safari */
}
```

---

## Archivos modificados

| Archivo | Cambio |
|---|---|
| `Views/Produccion/Create.cshtml` | Estado inicial del panel cambiado a "Por Apiario" |
| `Views/Shared/_Layout.cshtml` | Reordenamiento del sidebar, posición del botón Cerrar Sesión |
| CSS del sidebar | Scroll oculto habilitado |

---

## Notas técnicas

- El cambio en `Create.cshtml` es **HTML puro** — el estado inicial ya no depende de JavaScript para ser correcto.
- El CSS de `scrollbar-width: none` es compatible con todos los navegadores modernos.
