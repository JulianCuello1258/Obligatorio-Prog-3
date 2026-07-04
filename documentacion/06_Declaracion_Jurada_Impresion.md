# 📄 Declaración Jurada — Mejoras en el Diseño de Impresión (PDF)

**Chat de referencia:** `e93c289e-f93e-418e-bbfa-f5724e3c87ce`  
**Fecha:** 2026-06-30  
**Área:** `Views/Export/DeclaracionJurada.cshtml`, CSS de impresión en `wwwroot/css/beekeeper.css`

---

## Contexto

Se realizaron múltiples iteraciones para corregir el comportamiento y diseño visual al imprimir la **Declaración Jurada** (diseñada para ser exportada a PDF o papel). El objetivo era cumplir con los estrictos estándares requeridos por los organismos públicos receptores del documento.

---

## Cambios realizados

### 1. Reubicación del pie de página legal (Texto Rojo)
* **Problema:** El bloque de texto legal inferior (`footer-box`) se superponía con la tabla o aparecía flotando a mitad de la página 2 en lugar de mantenerse al pie del papel.
* **Solución:** Se transformó el contenedor de la página de impresión en un flexbox con orientación vertical (`flex-direction: column`) y se insertó un divisor espaciador dinámico con `flex: 1` justo antes del footer. Esto empuja el pie de página de forma limpia hacia el extremo inferior del espacio físico de la hoja.

```html
<!-- Estructura en la vista -->
<div class="print-page">
    <!-- Contenido de la página (Tablas, Info) -->
    
    <div class="print-page-spacer"></div> <!-- Empuja el footer al fondo -->
    
    <div class="footer-box text-danger">
        <!-- Texto legal y firmas -->
    </div>
</div>
```

```css
/* Reglas CSS aplicadas */
.print-page {
    display: flex;
    flex-direction: column;
    min-height: 297mm; /* Altura estándar A4 */
    box-sizing: border-box;
}
.print-page-spacer {
    flex: 1;
}
```

---

### 2. Eliminación de numeración de página duplicada
* **Problema:** En el PDF resultante aparecía la numeración dos veces (por ejemplo, "2/2" y luego otra vez al pie).
* **Causa:** El navegador web genera de forma automática sus propios pies de página con numeración y fecha, mientras que la vista HTML tenía divs fijos `<div class="page-num">`.
* **Solución:** Se removieron los elementos HTML de numeración fija (`.page-num`) para permitir que la numeración nativa de la ventana de impresión del navegador sea la única visible, evitando duplicados.

---

### 3. Solución a páginas en blanco y saltos de página incorrectos
* **Problema:** Al intentar imprimir, se generaba una página en blanco intermedia o el encabezado del Ministerio quedaba separado en una página distinta.
* **Causa:** El contenedor `.shared-top` (encabezado con el logotipo del Ministerio y datos del apicultor) estaba fuera de los límites de los contenedores `.print-page`. Al tener estos últimos una altura mínima estricta de `297mm`, el navegador realizaba un salto de página inmediatamente después de imprimir el encabezado flotante.
* **Solución:** 
  1. Se ocultó el `.shared-top` original para el motor de impresión mediante CSS `@media print`.
  2. Se duplicó la estructura del membrete institucional directamente dentro del primer div `.print-page` de la vista, logrando que el encabezado y las tablas de datos comiencen e impriman en el mismo bloque físico.

---

## Archivos modificados

| Archivo | Cambio |
|---|---|
| `Views/Export/DeclaracionJurada.cshtml` | Reestructuración de divs contenedores de impresión, inserción de `print-page-spacer` y duplicación interna del encabezado. |
| `wwwroot/css/beekeeper.css` | Adición de estilos de flexbox para impresión y limitación de saltos de página no deseados. |

---

## Notas técnicas
* Al trabajar con maquetados de impresión web, es de suma importancia que los elementos clave no queden fuera de los contenedores que definen la altura física de la página (`A4`, `Letter`).
* El uso de `flex: 1` junto con `min-height` es el estándar moderno para mantener pies de página fijos en el fondo de documentos impresos dinámicos.
