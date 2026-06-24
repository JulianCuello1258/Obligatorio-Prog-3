/**
 * Consulta la aptitud climática apícola para una coordenada dada
 * y renderiza el panel de información en el contenedor especificado.
 * 
 * @param {number|string} lat Latitud de la ubicación.
 * @param {number|string} lon Longitud de la ubicación.
 * @param {string} contenedorId ID del elemento HTML contenedor.
 */
function consultarClima(lat, lon, contenedorId) {
    const contenedor = document.getElementById(contenedorId);
    if (!contenedor) return;

    // Convert inputs to numbers
    const latitude = parseFloat(lat);
    const longitude = parseFloat(lon);

    if (isNaN(latitude) || isNaN(longitude)) {
        contenedor.innerHTML = `
            <div class="alert alert-info border-0 shadow-sm rounded-3">
                <i class="me-2">📍</i> Ingrese coordenadas válidas en el mapa o en los campos manuales para calcular la aptitud apícola del clima.
            </div>
        `;
        return;
    }

    // Render loading state
    contenedor.innerHTML = `
        <div class="glass-card p-4 text-center border-0 shadow-sm position-relative overflow-hidden">
            <div class="spinner-border text-warning" role="status" style="width: 3rem; height: 3rem;">
                <span class="visually-hidden">Cargando clima...</span>
            </div>
            <p class="mt-3 mb-0 text-muted">Consultando condiciones climáticas en Open-Meteo...</p>
        </div>
    `;

    fetch(`/Apiario/Clima?lat=${latitude}&lon=${longitude}`)
        .then(response => {
            if (!response.ok) {
                throw new Error("Error en la respuesta del servidor");
            }
            return response.json();
        })
        .then(data => {
            // Mapping color class to Bootstrap text and background badges
            let badgeBg = "bg-success text-white";
            let progressBg = "bg-success";
            let borderStyle = "border-success";
            
            if (data.color === "warning") {
                badgeBg = "bg-warning text-dark";
                progressBg = "bg-warning";
                borderStyle = "border-warning";
            } else if (data.color === "danger") {
                badgeBg = "bg-danger text-white";
                progressBg = "bg-danger";
                borderStyle = "border-danger";
            }

            // Build details list
            let detallesHtml = "";
            if (data.detalles && data.detalles.length > 0) {
                detallesHtml = data.detalles.map(d => `<li class="small text-muted py-1"><span class="me-2 text-warning">◆</span>${d}</li>`).join("");
            }

            // HTML content for the dashboard-like climate panel
            contenedor.innerHTML = `
                <div class="glass-card border-top border-4 ${borderStyle} p-4 shadow-sm rounded-4 mt-3" style="transition: all 0.3s ease;">
                    <div class="d-flex justify-content-between align-items-center mb-3">
                        <h4 class="h5 mb-0 font-weight-bold d-flex align-items-center">
                            <span class="fs-4 me-2">🐝</span> Aptitud Apícola
                        </h4>
                        <span class="badge ${badgeBg} fs-6 px-3 py-2 rounded-pill shadow-sm">${data.aptitud}</span>
                    </div>

                    <div class="row align-items-center mb-4">
                        <div class="col-sm-4 text-center mb-3 mb-sm-0">
                            <div class="position-relative d-inline-block">
                                <div class="d-flex flex-column align-items-center justify-content-center border border-3 rounded-circle shadow-sm" style="width: 100px; height: 100px; background: rgba(255,255,255,0.75);">
                                    <span class="h2 font-weight-bold mb-0 text-dark">${data.puntaje}</span>
                                    <span class="text-muted small" style="font-size: 0.75rem;">/ 100 pts</span>
                                </div>
                            </div>
                        </div>
                        <div class="col-sm-8">
                            <h5 class="small text-uppercase tracking-wider text-muted mb-2 font-weight-bold">Índice de Vuelo y Pecoreo</h5>
                            <div class="progress rounded-pill shadow-inner" style="height: 14px; background-color: rgba(0,0,0,0.06);">
                                <div class="progress-bar progress-bar-striped progress-bar-animated ${progressBg}" 
                                     role="progressbar" 
                                     style="width: ${data.puntaje}%; transition: width 0.8s ease;" 
                                     aria-valuenow="${data.puntaje}" 
                                     aria-valuemin="0" 
                                     aria-valuemax="100">
                                </div>
                            </div>
                            <p class="text-muted small mt-2 mb-0">Calculado a partir de temperatura, viento, precipitación, radiación y humedad.</p>
                        </div>
                    </div>

                    <div class="row row-cols-2 row-cols-md-3 g-3 mb-4">
                        <!-- Temperatura -->
                        <div class="col">
                            <div class="p-3 rounded-3" style="background: rgba(0,0,0,0.02); border: 1px solid rgba(0,0,0,0.04);">
                                <div class="text-muted small mb-1">🌡️ Temperatura</div>
                                <div class="h6 mb-0 font-weight-bold">${data.temperatura.toFixed(1)} °C</div>
                            </div>
                        </div>
                        <!-- Humedad -->
                        <div class="col">
                            <div class="p-3 rounded-3" style="background: rgba(0,0,0,0.02); border: 1px solid rgba(0,0,0,0.04);">
                                <div class="text-muted small mb-1">💧 Humedad</div>
                                <div class="h6 mb-0 font-weight-bold">${data.humedad.toFixed(0)} %</div>
                            </div>
                        </div>
                        <!-- Viento -->
                        <div class="col">
                            <div class="p-3 rounded-3" style="background: rgba(0,0,0,0.02); border: 1px solid rgba(0,0,0,0.04);">
                                <div class="text-muted small mb-1">💨 Viento</div>
                                <div class="h6 mb-0 font-weight-bold">${data.velocidadViento.toFixed(1)} km/h</div>
                                <div class="text-muted small font-size-xs" style="font-size: 0.75rem;">Dir: ${data.direccionVientoCard} (${data.direccionViento.toFixed(0)}°)</div>
                            </div>
                        </div>
                        <!-- Precipitación -->
                        <div class="col">
                            <div class="p-3 rounded-3" style="background: rgba(0,0,0,0.02); border: 1px solid rgba(0,0,0,0.04);">
                                <div class="text-muted small mb-1">🌧️ Precipitación</div>
                                <div class="h6 mb-0 font-weight-bold">${data.precipitacion.toFixed(1)} mm/h</div>
                            </div>
                        </div>
                        <!-- Radiación -->
                        <div class="col">
                            <div class="p-3 rounded-3" style="background: rgba(0,0,0,0.02); border: 1px solid rgba(0,0,0,0.04);">
                                <div class="text-muted small mb-1">☀️ Radiación</div>
                                <div class="h6 mb-0 font-weight-bold">${data.radiacion.toFixed(0)} W/m²</div>
                            </div>
                        </div>
                        <!-- Temperatura Suelo -->
                        <div class="col">
                            <div class="p-3 rounded-3" style="background: rgba(0,0,0,0.02); border: 1px solid rgba(0,0,0,0.04);">
                                <div class="text-muted small mb-1">🌱 Temp. Suelo</div>
                                <div class="h6 mb-0 font-weight-bold">${data.temperaturaSuelo.toFixed(1)} °C</div>
                            </div>
                        </div>
                    </div>

                    ${detallesHtml ? `
                        <div class="pt-3 border-top border-light">
                            <h6 class="small font-weight-bold text-uppercase text-muted mb-2">Evaluación Detallada:</h6>
                            <ul class="list-unstyled mb-0 ps-0">
                                ${detallesHtml}
                            </ul>
                        </div>
                    ` : ""}
                </div>
            `;
        })
        .catch(error => {
            console.error("Error al consultar clima:", error);
            contenedor.innerHTML = `
                <div class="alert alert-danger border-0 shadow-sm rounded-3">
                    <strong>⚠️ Error al consultar clima:</strong> No se pudo conectar con el servicio de meteorología. Por favor verifique su conexión e intente nuevamente.
                </div>
            `;
        });
}
