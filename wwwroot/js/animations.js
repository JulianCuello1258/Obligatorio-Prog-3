const BeeAnimations = {
    showSuccess: function (callback) {
        this.overlay('/Images/Animaciones/verificado.mp4', 2000, callback);
    },

    showLoading: function (duration, callback) {
        this.overlay('/Images/Animaciones/Cargando.mp4', duration, callback);
    },

    showCalendar: function () {
        this.overlay('/Images/Animaciones/calendario.mp4', 3000);
    },

    overlay: function (src, duration, callback) {
        const overlay = document.createElement('div');
        overlay.style = `
            position: fixed; top: 0; left: 0; width: 100vw; height: 100vh;
            background: rgba(0,0,0,0.8); z-index: 9999;
            display: flex; align-items: center; justify-content: center;
        `;

        const video = document.createElement('video');
        video.src = src;
        video.autoplay = true;
        video.muted = true;
        video.style = "max-width: 300px; border-radius: 50%;";

        overlay.appendChild(video);
        document.body.appendChild(overlay);

        setTimeout(() => {
            overlay.remove();
            if (callback) callback();
        }, duration);
    }
};

// Auto-show success animation if a success flag is in URL or TempData (handled by controller)
$(document).ready(function() {
    if (window.location.search.includes('success=true')) {
        BeeAnimations.showSuccess();
    }
});
