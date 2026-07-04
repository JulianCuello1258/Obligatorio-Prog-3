# 🔒 Seguridad, APIs Utilizadas y Conectividad de Datos

**Chat de referencia:** `5288baa6-0bba-478b-923e-3ba6b79707cc`  
**Fecha:** 2026-06-30  
**Área:** `appsettings.json`, `Services/WeatherService.cs`

---

## Contexto

Se llevó a cabo una auditoría de seguridad para verificar que el código del sistema no expusiera claves de acceso (API Keys), credenciales de base de datos o información confidencial que pudiera comprometer la aplicación.

---

## Diagnóstico y Resoluciones

### 1. Auditoría de API Keys (Claves de API Externas)
Se comprobó que la aplicación no expone ninguna clave de API en el código fuente. Esto se debe a que todas las interfaces y servicios utilizados operan bajo protocolos abiertos, públicos y gratuitos:

| API | Proveedor / URL | Propósito | ¿Requiere API Key? | Origen del Servidor |
|---|---|---|---|---|
| **Open-Meteo Forecast** | `api.open-meteo.com` | Clima en tiempo real | **No** (Gratuita/Abierta) | 🇨🇭 Suiza |
| **Open-Meteo Archive** | `archive-api.open-meteo.com` | Historial climático | **No** (Gratuita/Abierta) | 🇨🇭 Suiza |
| **Overpass API (OSM)** | `overpass-api.de` | Flora y recursos hídricos | **No** (Gratuita/Abierta) | 🇩🇪 Alemania |
| **Nominatim (OSM)** | `nominatim.openstreetmap.org` | Inferencia de parajes y límites | **No** (Gratuita/Abierta) | 🇬🇧 Reino Unido |

---

### 2. Seguridad de la Cadena de Conexión a Base de Datos
* **Intento Inicial (User Secrets):** Se intentó mover la cadena de conexión de desarrollo del archivo `appsettings.json` al almacenamiento seguro de secretos de usuario de .NET (`dotnet user-secrets`).
* **Incidente:** Se observó un fallo de inicialización de la conexión (`ConnectionString property has not been initialized`) al desplegar o ejecutar la aplicación en entornos donde la configuración local no cargaba los secretos del perfil del programador.
* **Resolución definitiva:** Se restauró la cadena de conexión en el archivo `appsettings.json`.
* **Análisis de riesgo:** La conexión utiliza **Autenticación Integrada de Windows** (`Trusted_Connection=True` y `Integrated Security=True`), lo que significa que el servidor SQL delega el acceso al sistema operativo de la máquina local. **No contiene contraseñas ni nombres de usuario** escritos en texto claro en el archivo de configuración.

```json
// Configuración segura en appsettings.json
"ConnectionStrings": {
  "DefaultConnection": "Server=.\\SQLEXPRESS;Database=BeeKeeperDB;Trusted_Connection=True;TrustServerCertificate=True;MultipleActiveResultSets=true"
}
```

---

## Archivos modificados

| Archivo | Cambio |
|---|---|
| `appsettings.json` | Unificación y restauración segura de la cadena de conexión por Windows Authentication. |

---

## Notas técnicas
* En caso de migrar a un entorno de producción en la nube (como Azure SQL o AWS RDS), se recomienda utilizar **Managed Identities** (identidades administradas) o inyectar las credenciales a través de **Variables de Entorno** del contenedor, evitando siempre guardar contraseñas en el repositorio de Git.
