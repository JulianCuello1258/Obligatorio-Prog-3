# 🎨 Personalización Estética — Cambio de Fondo de Pantalla

**Chat de referencia:** `2f13af27-1706-4820-af39-09ff93865783`  
**Fecha:** 2026-06-22  
**Área:** `wwwroot/css/beekeeper.css`

---

## Contexto

Se modificó la identidad visual del sitio web actualizando la imagen de fondo principal y la de la pantalla de inicio de sesión (`Login`) por una imagen provista por el apicultor, con el fin de mejorar la ambientación temática y el contraste del panel de control de vidrio templado (`glass-card`).

---

## Cambios realizados

### 1. Actualización de la imagen de fondo central (Global y Login)
* Se sustituyó la antigua imagen de fondo `Acceder-fondo-de-pantalla.png` por la nueva imagen **`Fondo-Login.jpg`** (alojada en el directorio de recursos estáticos `wwwroot/Images/`).
* De acuerdo con las instrucciones del usuario, no se alteró ningún otro aspecto de la maquetación CSS (como el centrado, la opacidad o los efectos de desenfoque del cuerpo).

**CSS modificado en `wwwroot/css/beekeeper.css`:**

```css
/* Estilo del cuerpo de la aplicación (Línea 11) */
body {
    background-image: url('../Images/Fondo-Login.jpg');
    background-size: cover;
    background-position: center;
    background-attachment: fixed;
}

/* Estilo de la pantalla de inicio de sesión (Línea 106) */
.login-body {
    background-image: url('../Images/Fondo-Login.jpg');
    background-size: cover;
    background-position: center;
}
```

---

## Archivos modificados

| Archivo | Cambio |
|---|---|
| `wwwroot/css/beekeeper.css` | Modificación de rutas de imagen de fondo en `body` y `.login-body`. |

---

## Notas técnicas
* Al utilizar imágenes de fondo de gran tamaño, es importante que estén optimizadas en compresión y resolución (como en formato `.jpg` progresivo) para reducir los tiempos de carga inicial de la aplicación.
