const VoiceTranscription = {
    recognition: null,
    isListening: false,
    fullTranscript: "",   // acumula TODO lo que se dice (para continuous: true)

    manualStop: false,
    lastError: "",

    init: function (onResult, onError) {
        const SpeechRecognition = window.SpeechRecognition || window.webkitSpeechRecognition;
        if (!SpeechRecognition) return false;

        this.recognition = new SpeechRecognition();
        this.recognition.lang = 'es-ES';
        this.recognition.interimResults = true;   // muestra texto en tiempo real
        this.recognition.continuous = true;       // NO corta al silencio

        this.recognition.onresult = (event) => {
            let interim = "";
            for (let i = event.resultIndex; i < event.results.length; i++) {
                const t = event.results[i][0].transcript;
                if (event.results[i].isFinal) {
                    this.fullTranscript += t + " ";
                } else {
                    interim = t;
                }
            }
            console.log("🎤 Transcript acumulado:", this.fullTranscript);
            console.log("🎤 Interim:", interim);
            if (typeof window.onInterim === 'function') window.onInterim(this.fullTranscript + interim);
        };

        this.recognition.onerror = (event) => {
            console.error("❌ Error de reconocimiento:", event.error);
            this.lastError = event.error;
            if (event.error === 'no-speech') return;
            if (onError) onError(event.error);
        };

        this.recognition.onend = () => {
            console.log("🛑 onend disparado. Transcript:", this.fullTranscript);

            // Si se cortó solo (por silencio, etc) y no fue un error grave, lo reiniciamos
            if (!this.manualStop && this.lastError !== 'network' && this.lastError !== 'not-allowed' && this.lastError !== 'aborted') {
                console.log("🔄 Reiniciando reconocimiento automáticamente para seguir escuchando...");
                try {
                    this.recognition.start();
                    return; // Evita cerrar la UI
                } catch (e) {
                    console.error("No se pudo reiniciar", e);
                }
            }

            this.isListening = false;
            onResult(this.fullTranscript.trim());
        };

        return true;
    },

    start: function () {
        if (!this.recognition) return;
        this.fullTranscript = "";  // resetea para nueva grabación
        this.manualStop = false;
        this.lastError = "";
        try {
            this.recognition.start();
            this.isListening = true;
        } catch (e) { }
    },

    stop: function () {
        if (!this.recognition) return;
        this.manualStop = true;
        this.recognition.stop();  // dispara onend → onResult
    },

    // Llama al endpoint C# que usa Claude API
    extractWithClaude: async function (text) {
        console.log("📡 Enviando a Claude:", text);
        const response = await fetch('/Sanidad/ExtractVoiceData', {
            method: 'POST',
            headers: { 'Content-Type': 'application/json' },
            body: JSON.stringify({ text })
        });
        console.log("📡 Respuesta status:", response.status);
        const json = await response.json();
        console.log("📡 JSON recibido:", json);
        if (!response.ok) throw new Error('Error al procesar con IA');
        return json;
    }
};