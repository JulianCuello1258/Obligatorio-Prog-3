function showNotification(title, message) {
    const area = document.getElementById('notification-area');
    if (!area) return;

    const notif = document.createElement('div');
    notif.className = 'notification';
    notif.innerHTML = `
        <div class="fw-bold">${title}</div>
        <div class="small">${message}</div>
    `;

    area.appendChild(notif);

    setTimeout(() => {
        notif.style.opacity = '0';
        setTimeout(() => notif.remove(), 500);
    }, 5000);
}

// Check for overdue tasks on load
$(document).ready(function() {
    function refreshNotifications() {
        $.get('/Tareas/GetPendingTasks', function(data) {
            const list = $('#notification-list');
            const badge = $('#notif-badge');
            list.empty();
            
            if (data.length === 0) {
                list.append('<li class="p-2 text-center text-muted small">No hay notificaciones pendientes</li>');
                badge.hide();
            } else {
                badge.text(data.length).show();
                data.forEach(item => {
                    list.append(`
                        <li>
                            <a href="/Tareas/Details/${item.id}" class="dropdown-item p-2 border-bottom border-light" style="white-space: normal;">
                                <div class="fw-bold small text-honey-dark">${item.titulo || item.descripcion}</div>
                                <div class="text-muted" style="font-size: 10px;">Para: ${item.fecha}</div>
                            </a>
                        </li>
                    `);
                });
            }
        });
    }

    $('#notification-trigger').on('show.bs.dropdown', refreshNotifications);
    refreshNotifications();

    // Loading animation on navigation
    $('a').on('click', function(e) {
        const href = $(this).attr('href');
        if (href && href !== '#' && !href.startsWith('javascript') && !$(this).attr('target')) {
            e.preventDefault();
            BeeAnimations.showLoading(800, () => {
                window.location.href = href;
            });
        }
    });
});
