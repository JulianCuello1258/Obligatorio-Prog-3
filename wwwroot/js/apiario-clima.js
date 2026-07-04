/**
 * Consulta la aptitud climática y de entorno apícola para una coordenada dada
 * y renderiza el panel de información resumido y jerarquizado en el contenedor especificado.
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
                <i class="me-2">📍</i> Ingrese coordenadas válidas en el mapa o en los campos manuales para calcular la aptitud apícola completa del lugar.
            </div>
        `;
        return;
    }

    // Render loading state
    contenedor.innerHTML = `
        <div class="glass-card p-4 text-center border-0 shadow-sm position-relative overflow-hidden">
            <div class="spinner-border text-warning" role="status" style="width: 3rem; height: 3rem;">
                <span class="visually-hidden">Cargando evaluación...</span>
            </div>
            <p class="mt-3 mb-0 text-muted">Realizando análisis satelital, histórico y de entorno geográfico...</p>
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

            // Simplified water display
            let waterText = "No detectada";
            let waterBadgeStyle = "bg-danger";
            if (data.tieneAguaCercana) {
                const distKm = (data.distanciaAguaMasCercana / 1000).toFixed(1);
                waterText = `${distKm} km`;
                waterBadgeStyle = data.distanciaAguaMasCercana < 1000 ? "bg-success" : "bg-warning text-dark";
            }
            let waterLabel = `<span class="badge ${waterBadgeStyle}">${waterText}</span>`;
            if (data.aguaOrigen === "OSM") {
                waterLabel += ` <span class="text-muted small" style="font-size: 0.7rem;">(Satelital/OSM)</span>`;
            } else if (data.aguaOrigen === "Estimado") {
                waterLabel += ` <span class="text-info small" style="font-size: 0.7rem;">(Estimación Regional)</span>`;
            }

            // Simplified crops display
            let cropsText = "No detectados";
            if (data.cultivosDetectados && data.cultivosDetectados.length > 0) {
                cropsText = data.cultivosDetectados.join(", ");
            }
            let cropsLabel = `<span>${cropsText}</span>`;
            if (data.cultivosOrigen === "OSM") {
                cropsLabel += ` <span class="text-muted small" style="font-size: 0.7rem;">(Satelital/OSM)</span>`;
            } else if (data.cultivosOrigen === "Estimado") {
                cropsLabel += ` <span class="text-info small" style="font-size: 0.7rem;">(Estimación Regional)</span>`;
            }

            // Competition display removed per user request
            let apiariesHtml = "";

            // Build detailed reasons lists (Por qué sí / Por qué no)
            let razonesSiHtml = "";
            if (data.razonesSi && data.razonesSi.length > 0) {
                razonesSiHtml = data.razonesSi.map(r => `
                    <li class="d-flex align-items-start small text-muted py-1">
                        <span class="text-success me-2 font-weight-bold">✓</span>
                        <span>${r}</span>
                    </li>
                `).join("");
            }

            let razonesNoHtml = "";
            if (data.razonesNo && data.razonesNo.length > 0) {
                razonesNoHtml = data.razonesNo.map(r => `
                    <li class="d-flex align-items-start small text-muted py-1">
                        <span class="text-danger me-2 font-weight-bold">⚠️</span>
                        <span>${r}</span>
                    </li>
                `).join("");
            }

            // HTML content for the dashboard-like climate panel (Highly Structured and Simplified)
            contenedor.innerHTML = `
                <div class="glass-card border-top border-4 ${borderStyle} p-4 shadow-sm rounded-4 mt-3" style="transition: all 0.3s ease;">
                    
                    <!-- 1. HEADER (APTITUD) -->
                    <div class="d-flex justify-content-between align-items-center mb-3">
                        <h4 class="h5 mb-0 font-weight-bold d-flex align-items-center">
                            <span class="fs-4 me-2"></span> Aptitud de Ubicación
                        </h4>
                        <span class="badge ${badgeBg} fs-6 px-3 py-2 rounded-pill shadow-sm">${data.aptitud}</span>
                    </div>

                    <!-- Score and progress bar row -->
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
                            <h5 class="small text-uppercase tracking-wider text-muted mb-2 font-weight-bold">Índice de Aptitud General</h5>
                            <div class="progress rounded-pill shadow-inner" style="height: 14px; background-color: rgba(0,0,0,0.06);">
                                <div class="progress-bar progress-bar-striped progress-bar-animated ${progressBg}" 
                                     role="progressbar" 
                                     style="width: ${data.puntaje}%; transition: width 0.8s ease;" 
                                     aria-valuenow="${data.puntaje}" 
                                     aria-valuemin="0" 
                                     aria-valuemax="100">
                                </div>
                            </div>
                        </div>
                    </div>


                    <!-- 3. MAIN ANALYSIS BODY (WHY YES / WHY NO) -->
                    <div class="row">
                        <!-- Factores a favor -->
                        <div class="col-md-6 mb-3 mb-md-0">
                            <h6 class="small font-weight-bold text-uppercase text-success mb-2">✅ Factores a Favor (${data.razonesSi.length}):</h6>
                            ${razonesSiHtml ? `
                                <ul class="list-unstyled mb-0 ps-0">
                                    ${razonesSiHtml}
                                </ul>
                            ` : `<p class="text-muted small">No se detectaron factores sobresalientes a favor en este momento.</p>`}
                        </div>
                        <!-- Advertencias -->
                        <div class="col-md-6">
                            <h6 class="small font-weight-bold text-uppercase text-danger mb-2">⚠️ Advertencias y Recomendaciones (${data.razonesNo.length}):</h6>
                            ${razonesNoHtml ? `
                                <ul class="list-unstyled mb-0 ps-0">
                                    ${razonesNoHtml}
                                </ul>
                            ` : `<p class="text-muted small">Condiciones excelentes sin advertencias significativas.</p>`}
                        </div>
                    </div>

                    <!-- 4. COLLAPSIBLE TECHNICAL DETAIL -->
                    <div class="text-center mt-4">
                        <button class="btn btn-link btn-sm text-decoration-none text-muted" type="button" data-bs-toggle="collapse" data-bs-target="#detalleTecnico" aria-expanded="false" aria-controls="detalleTecnico">
                            🔍 Ver Detalles
                        </button>
                    </div>

                    <div class="collapse mt-3" id="detalleTecnico">
                        <div class="p-3 rounded-4" style="background: rgba(0,0,0,0.02); border: 1px solid rgba(0,0,0,0.04);">
                            
                            <!-- Entorno Geográfico -->
                            <div class="mb-3">
                                <h6 class="small text-uppercase tracking-wider text-muted font-weight-bold mb-2">🌍 Entorno Geográfico (Radio 3km)</h6>
                                <ul class="list-unstyled mb-0 ps-0" style="font-size: 0.8rem; line-height: 1.6;">
                                    <li class="py-1">
                                        <span class="fw-bold text-dark me-2">💧 Cercanía a Agua:</span>
                                        ${waterLabel}
                                    </li>
                                    <li class="py-1">
                                        <span class="fw-bold text-dark me-2">🌱 Vegetación/Cultivos:</span>
                                        ${cropsLabel}
                                    </li>
                                    ${apiariesHtml}
                                </ul>
                            </div>

                            <!-- Clima Actual -->
                            <div class="mb-3 pt-3 border-top border-light">
                                <h6 class="small text-uppercase tracking-wider text-muted font-weight-bold mb-2">🌤️ Clima del Momento</h6>
                                <div class="row row-cols-2 row-cols-sm-3 g-2" style="font-size: 0.8rem;">
                                    <div class="col"><span class="text-muted">🌡️ Temp:</span> <strong>${data.temperatura.toFixed(1)} °C</strong></div>
                                    <div class="col"><span class="text-muted">💧 Humedad:</span> <strong>${data.humedad.toFixed(0)}%</strong></div>
                                    <div class="col"><span class="text-muted">💨 Viento:</span> <strong>${data.velocidadViento.toFixed(1)} km/h</strong> (${data.direccionVientoCard})</div>
                                    <div class="col"><span class="text-muted">🌧️ Lluvia:</span> <strong>${data.precipitacion.toFixed(1)} mm/h</strong></div>
                                    <div class="col"><span class="text-muted">☀️ Radiación:</span> <strong>${data.radiacion.toFixed(0)} W/m²</strong></div>
                                    <div class="col"><span class="text-muted">🌱 Temp. Suelo:</span> <strong>${data.temperaturaSuelo.toFixed(1)} °C</strong></div>
                                </div>
                            </div>

                            <!-- Historial 180 días -->
                            ${data.tieneDatosHistoricos ? `
                                <div class="pt-3 border-top border-light">
                                    <h6 class="small text-uppercase tracking-wider text-muted font-weight-bold mb-2">📅 Historial Reciente (Últimos 180 días)</h6>
                                    <div class="row row-cols-1 row-cols-sm-3 g-2" style="font-size: 0.8rem;">
                                        <div class="col"><span class="text-muted">🌧️ Lluvia diaria:</span> <strong>${data.lluviaPromedioDiaria180Dias.toFixed(2)} mm</strong> (Acum: ${data.lluviaAcumulada180Dias.toFixed(0)} mm)</div>
                                        <div class="col"><span class="text-muted">💨 Vientos Fuertes:</span> <strong>${data.diasVientoFuerte180Dias} días</strong> (${Math.round(data.porcentajeDiasVientoFuerte)}%)</div>
                                        <div class="col"><span class="text-muted">❄️ Heladas (<5°C):</span> <strong>${data.diasHeladas180Dias} días</strong></div>
                                    </div>
                                </div>
                            ` : ""}

                        </div>
                    </div>

                </div>
            `;

            // Autocompletado de campos del formulario con animación de resalte premium
            function autoCompletarYAnimar(id, valor) {
                const el = document.getElementById(id);
                if (el && valor !== undefined && valor !== null && valor !== '') {
                    if (el.value !== valor.toString()) {
                        el.value = valor;
                        el.dispatchEvent(new Event('change', { bubbles: true }));
                        
                        // Agregar animación
                        el.classList.add('autofill-highlight');
                        setTimeout(() => el.classList.remove('autofill-highlight'), 1800);
                    }
                }
            }

            autoCompletarYAnimar('Departamento', data.departamento);
            autoCompletarYAnimar('SeccionPolicial', data.seccionPolicial);
            
            // Si existe Paraje (como en Edit), completamos Paraje y Zona por separado.
            // Si no existe Paraje (como en Create), completamos Zona / Paraje (id="Zona") con el paraje.
            const parajeEl = document.getElementById('Paraje');
            const zonaEl = document.getElementById('Zona');
            
            if (parajeEl) {
                autoCompletarYAnimar('Paraje', data.paraje);
                if (zonaEl && zonaEl.tagName.toLowerCase() === 'select') {
                    autoCompletarYAnimar('Zona', data.zona);
                }
            } else if (zonaEl) {
                // En Create.cshtml, Zona es el campo para Zona / Paraje.
                autoCompletarYAnimar('Zona', data.paraje);
            }

            // Casilla de Habilitar Trashumancia
            const trashumanciaCheck = document.getElementById('TrashumanciaHabilitada');
            if (trashumanciaCheck && data.sugerirTrashumanciaHabilitada !== undefined) {
                if (trashumanciaCheck.checked !== data.sugerirTrashumanciaHabilitada) {
                    trashumanciaCheck.checked = data.sugerirTrashumanciaHabilitada;
                    trashumanciaCheck.dispatchEvent(new Event('change', { bubbles: true }));
                    
                    const formCheckParent = trashumanciaCheck.closest('.form-check') || trashumanciaCheck.parentElement;
                    if (formCheckParent) {
                        formCheckParent.classList.add('autofill-highlight');
                        setTimeout(() => formCheckParent.classList.remove('autofill-highlight'), 1800);
                    }
                }
            }
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
