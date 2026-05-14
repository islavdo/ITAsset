(function () {
    let dotNetRef = null;
    let port = null;
    let reader = null;
    let readLoopActive = false;
    let buffer = "";
    let flushTimer = null;
    let lastReceivedAt = 0;

    const textDecoder = new TextDecoder();
    const defaultOptions = {
        baudRate: 9600,
        dataBits: 8,
        stopBits: 1,
        parity: "none",
        flowControl: "none"
    };

    function isSupported() {
        return "serial" in navigator;
    }

    function initialize(ref) {
        dotNetRef = ref;
        return isSupported();
    }

    async function autoConnect() {
        if (!isSupported()) {
            return false;
        }

        const ports = await navigator.serial.getPorts();
        if (ports.length === 0) {
            return false;
        }

        port = ports[0];
        await openPort();
        return true;
    }

    async function connect() {
        if (!isSupported()) {
            throw new Error("Браузер не поддерживает Web Serial API.");
        }

        port = await navigator.serial.requestPort();
        await openPort();
        return true;
    }

    async function openPort() {
        if (!port) {
            return;
        }

        if (!port.readable) {
            await port.open(defaultOptions);
        }

        if (port.setSignals) {
            await port.setSignals({ dataTerminalReady: true, requestToSend: true });
        }

        if (!readLoopActive) {
            readLoopActive = true;
            readLoop();
        }
    }

    async function readLoop() {
        while (port && port.readable && readLoopActive) {
            try {
                reader = port.readable.getReader();
                while (readLoopActive) {
                    const result = await reader.read();
                    if (result.done) {
                        break;
                    }
                    appendChunk(textDecoder.decode(result.value, { stream: true }));
                }
            } catch {
                break;
            } finally {
                if (reader) {
                    reader.releaseLock();
                    reader = null;
                }
            }
        }
    }

    function appendChunk(text) {
        lastReceivedAt = Date.now();
        for (const char of text) {
            if (char === "\r" || char === "\n" || char === "\u0003") {
                flushBuffer();
            } else if (char === "\u0002" || char === "\u0000") {
                continue;
            } else {
                buffer += char;
                scheduleFlush();
            }
        }
    }

    function scheduleFlush() {
        if (flushTimer) {
            clearTimeout(flushTimer);
        }

        flushTimer = setTimeout(() => {
            if (Date.now() - lastReceivedAt >= 160) {
                flushBuffer();
            }
        }, 180);
    }

    function flushBuffer() {
        if (flushTimer) {
            clearTimeout(flushTimer);
            flushTimer = null;
        }

        const code = buffer.replace(/[\u0000-\u001F\u007F]/g, "").trim();
        buffer = "";

        if (code && dotNetRef) {
            dotNetRef.invokeMethodAsync("OnBarcodeScanned", code);
        }
    }

    async function dispose() {
        readLoopActive = false;
        buffer = "";

        if (flushTimer) {
            clearTimeout(flushTimer);
            flushTimer = null;
        }

        if (reader) {
            try {
                await reader.cancel();
            } catch {
                // Ignore shutdown races.
            }
            try {
                reader.releaseLock();
            } catch {
                // Ignore shutdown races.
            }
            reader = null;
        }
    }

    window.itAssetSerialScanner = {
        initialize,
        autoConnect,
        connect,
        dispose
    };
})();
