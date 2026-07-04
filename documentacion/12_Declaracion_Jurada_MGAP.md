# 📝 Formulario de Declaración Jurada (MGAP) y Sección D

**Chat de referencia:** `6e3c6d15-7913-4d92-a208-8b504ea0ede2`  
**Fecha:** 2026-06-29  
**Área:** `Views/Export/DeclaracionJurada.cshtml`

---

## Contexto

Se ajustó el comportamiento y formato del formulario interactivo para la **Declaración Jurada del Registro Nacional de Apicultores** que se presenta ante el Ministerio de Ganadería, Agricultura y Pesca (MGAP). 

---

## Cambios realizados

### 1. Restricción estricta de caracteres numéricos
* **Problema:** Los campos de **Documento de Identidad** y **Cantidad de Núcleos** permitían ingresar texto o caracteres especiales, lo que provocaba errores de formato y rechazo de los datos.
* **Solución:** Se limitaron ambos inputs para que acepten únicamente caracteres numéricos utilizando atributos HTML y expresiones regulares de JavaScript en tiempo real.

```html
<!-- Configuración de input numérico estricto -->
<input type="text" 
       inputmode="numeric" 
       pattern="[0-9]*" 
       oninput="this.value = this.value.replace(/[^0-9]/g, '')" />
```

---

### 2. Remoción de valores por defecto forzados
* Se removió el valor predeterminado `"Colonia"` del campo localidad, permitiendo que el apicultor sea quien digite de manera flexible su propia ubicación en el formulario sin tener que borrar el texto previo.

---

### 3. Incorporación de la Sección D en página independiente
* **Cambio:** Se integró el apartado oficial **Sección D** de la declaración jurada en una segunda página separada de impresión.
* Esta sección incluye la advertencia y constancia legal oficial de firmas requerida por el ministerio:
  > *"La presente declaración jurada está exonerada del pago del timbre de la Caja de Jubilaciones y Pensiones de Profesionales Universitarios, según Ley 19.535 del 25 de setiembre de 2017, Art. 93."*

---

## Archivos modificados

| Archivo | Cambio |
|---|---|
| `Views/Export/DeclaracionJurada.cshtml` | Validación de inputs numéricos en JS, remoción de localidad preestablecida e inclusión de texto de exoneración en Sección D. |

---

## Notas técnicas
* El uso de `inputmode="numeric"` mejora significativamente la experiencia del usuario en dispositivos móviles al levantar directamente el teclado numérico en lugar del alfanumérico.
* La expresión regular `/^[^0-9]/g` previene incluso la acción de copiar y pegar caracteres de texto no válidos dentro del campo.
