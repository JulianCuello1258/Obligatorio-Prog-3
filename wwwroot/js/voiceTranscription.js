const VoiceTranscription = {
    recognition: null,
    isListening: false,

    init: function (onResult, onError) {
        const SpeechRecognition = window.SpeechRecognition || window.webkitSpeechRecognition;
        if (!SpeechRecognition) {
            console.error("Browser does not support Speech Recognition");
            return false;
        }

        this.recognition = new SpeechRecognition();
        this.recognition.lang = 'es-ES';
        this.recognition.interimResults = false;
        this.recognition.continuous = false;

        this.recognition.onresult = (event) => {
            const transcript = event.results[0][0].transcript;
            onResult(transcript);
        };

        this.recognition.onerror = (event) => {
            if (onError) onError(event.error);
        };

        this.recognition.onend = () => {
            this.isListening = false;
            document.body.classList.remove('voice-active');
        };

        return true;
    },

    start: function () {
        if (!this.recognition) return;
        this.recognition.start();
        this.isListening = true;
        document.body.classList.add('voice-active');
    },

    stop: function () {
        if (!this.recognition) return;
        this.recognition.stop();
    },

    // Simple parser for structured data extraction
    parseTranscript: function (text) {
        const data = {
            colmena: text.match(/colmena\s+(\d+)/i)?.[1],
            produccion: text.match(/producción\s+(\d+)/i)?.[1],
            tratamiento: text.match(/tratamiento\s+([a-z\s]+)/i)?.[1],
            observaciones: text
        };
        return data;
    }
};
