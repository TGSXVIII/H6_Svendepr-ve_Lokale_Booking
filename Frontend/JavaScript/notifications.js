(() => {
    let container;
    let message;
    let timer;

    function initialize() {
        if (container) return;

        container = document.createElement("div");
        container.className = "site-notification";
        container.hidden = true;

        message = document.createElement("p");
        message.className = "site-notification-message";
        message.setAttribute("role", "status");
        message.setAttribute("aria-live", "polite");
        message.setAttribute("aria-atomic", "true");

        const closeButton = document.createElement("button");
        closeButton.type = "button";
        closeButton.className = "site-notification-close";
        closeButton.setAttribute("aria-label", "Luk besked");
        closeButton.textContent = "×";
        closeButton.addEventListener("click", hide);

        container.append(message, closeButton);
        document.body.append(container);
    }

    function hide() {
        window.clearTimeout(timer);

        if (!container) return;

        // Undgå at skjule en knap, mens tastaturbrugeren står på den.
        const hadFocus = container.contains(document.activeElement);

        container.hidden = true;
        message.textContent = "";

        if (hadFocus && previousFocus?.isConnected) {
            previousFocus.focus();
        }
    }

    let previousFocus = null;

    function show(text, { type = "success", duration = 6000 } = {}) {
        initialize();
        window.clearTimeout(timer);

        const allowedTypes = ["success", "error", "info"];

        container.dataset.type = allowedTypes.includes(type)
            ? type
            : "info";

        if (container.hidden) {
            previousFocus = document.activeElement;
        }

        container.hidden = false;
        message.textContent = String(text);

        // duration: 0 betyder, at brugeren selv skal lukke beskeden.
        if (Number.isFinite(duration) && duration > 0) {
            timer = window.setTimeout(() => {
                // Behold beskeden, hvis brugeren betjener lukkeknappen.
                if (!container.contains(document.activeElement)) {
                    hide();
                }
            }, duration);
        }
    }

    window.MercNotifications = Object.freeze({
        show,
        hide
    });
})();