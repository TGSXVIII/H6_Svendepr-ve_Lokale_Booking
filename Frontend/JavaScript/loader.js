(() => {
    const activeLoaders = new WeakMap();

    // Stien beregnes ud fra loader.js.
    const logoUrl = new URL(
        "../../Utilities/Images/Merc_Loading_Logo.png",
        document.currentScript.src
    ).href;

    function start(target, text = "Henter oplysninger...") {
        const host = typeof target === "string"
            ? document.querySelector(target)
            : target;

        if (!(host instanceof HTMLElement)) {
            throw new Error("Loaderens placering blev ikke fundet.");
        }

        let state = activeLoaders.get(host);

        if (!state) {
            const element = document.createElement("div");
            element.className = "merc-loader";
            element.hidden = true;

            const symbol = document.createElement("div");
            symbol.className = "merc-loader-symbol";
            symbol.setAttribute("aria-hidden", "true");

            const logo = document.createElement("img");
            logo.src = logoUrl;
            logo.alt = "";
            logo.width = 64;
            logo.height = 64;
            logo.loading = "eager";

            // Ringen vises stadig, hvis billedet ikke kan hentes.
            logo.addEventListener("error", () => {
                logo.hidden = true;
            });

            const label = document.createElement("p");
            label.className = "merc-loader-text";
            label.setAttribute("role", "status");
            label.setAttribute("aria-live", "polite");

            symbol.append(logo);
            element.append(symbol, label);
            host.append(element);

            state = {
                count: 0,
                element,
                label,
                timer: null,
                previousBusy: null
            };

            activeLoaders.set(host, state);
        }

        if (state.count === 0) {
            state.previousBusy = host.getAttribute("aria-busy");
            host.setAttribute("aria-busy", "true");

            state.label.textContent = "";

            // Undgå et kort blink ved hurtige svar.
            state.timer = window.setTimeout(() => {
                state.element.hidden = false;
                state.label.textContent = text;
            }, 150);
        }

        state.count += 1;
        let stopped = false;

        // Hvert start-kald får sin egen stopfunktion.
        return function stop() {
            if (stopped) return;
            stopped = true;

            state.count -= 1;

            if (state.count > 0) return;

            window.clearTimeout(state.timer);
            state.element.hidden = true;
            state.label.textContent = "";

            if (state.previousBusy === null) {
                host.removeAttribute("aria-busy");
            } else {
                host.setAttribute("aria-busy", state.previousBusy);
            }
        };
    }

    window.MercLoader = Object.freeze({ start });
})();