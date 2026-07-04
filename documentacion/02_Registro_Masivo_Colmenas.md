# 🐝 Registro Masivo de Colmenas y Validación de Reina

**Chat de referencia:** `e8a3a426-4375-4ffa-a577-3cbdc3b7cb52`  
**Fecha:** 2026-07-04  
**Área:** `Views/Colmenas/Create.cshtml`, `Controllers/ColmenasController.cs`

---

## Contexto

Se implementaron dos mejoras en el formulario de registro de colmenas:

1. **Registro en cantidad** — Permitir registrar varias colmenas a la vez con los mismos parámetros.
2. **Validación de Reina** — Deshabilitar campos relacionados con la reina cuando "Reina Presente" está desmarcado.

---

## Cambios realizados

### 1. Registro en cantidad de colmenas

**Pedido del usuario:**  
> "Que se pueda registrar colmenas en cantidad, ej: en el apiario José, le registro 30 colmenas. Los campos los selecciono como si estuviera registrando 1 colmena, pero en realidad estoy registrando 30."

**Solución implementada:**

**Vista (`Views/Colmenas/Create.cshtml`):**
- El campo **Apiario** fue reducido a 8 columnas.
- Se agregó el campo **"Cantidad de Colmenas"** (tipo número, mín 1, máx 500) en las 4 columnas restantes, en la misma fila.

```html
<div class="col-md-8">
    <!-- Campo Apiario (existente) -->
</div>
<div class="col-md-4">
    <label>Cantidad de Colmenas</label>
    <input type="number" name="cantidad" min="1" max="500" value="1" />
</div>
```

**Controlador (`Controllers/ColmenasController.cs`):**
- El método POST `Create` ahora recibe el parámetro `int cantidad`.
- Itera `cantidad` veces y crea una colmena por iteración con los mismos datos.

```csharp
[HttpPost]
public async Task<IActionResult> Create(Colmena colmena, int cantidad = 1)
{
    for (int i = 0; i < cantidad; i++)
    {
        var nueva = /* clonar colmena */;
        _context.Add(nueva);
    }
    await _context.SaveChangesAsync();
}
```

---

### 2. Validación del campo "Reina Presente"

**Problema:** Al desmarcar el checkbox "Existencia de Reina", los campos **Estado de Salud** y **Fecha de Nacimiento** de la reina seguían habilitados y permitían ingresar datos inválidos.

**Solución — JavaScript en `Create.cshtml`:**
- Al desmarcar "Reina Presente":
  - Los campos `EstadoSalud` y `FechaNacimiento` se **deshabilitan** (`disabled = true`).
  - Sus valores se **limpian** automáticamente.
- Al marcar "Reina Presente":
  - Los campos se **habilitan** de nuevo.

```javascript
document.getElementById('reinaCheckbox').addEventListener('change', function () {
    const activa = this.checked;
    document.getElementById('estadoSalud').disabled = !activa;
    document.getElementById('fechaNacimiento').disabled = !activa;

    if (!activa) {
        document.getElementById('estadoSalud').value = '';
        document.getElementById('fechaNacimiento').value = '';
    }
});
```

---

## Archivos modificados

| Archivo | Cambio |
|---|---|
| `Views/Colmenas/Create.cshtml` | Campo cantidad añadido + JS validación de reina |
| `Controllers/ColmenasController.cs` | Lógica de creación en bucle según cantidad |

---

## Notas técnicas

- El rango máximo de cantidad es **500 colmenas** por operación para evitar saturar la base de datos.
- Los campos de reina deshabilitados con `disabled` no son enviados por el formulario HTML, lo cual asegura que el servidor reciba valores nulos para esos campos.
- La lógica de clonación en el controlador asegura que cada colmena tenga un `Id` independiente generado por la base de datos.
