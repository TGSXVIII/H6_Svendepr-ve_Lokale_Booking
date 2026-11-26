(() => {
    const floorButtons = document.querySelectorAll("[data-floor]");
    const floorViewer = document.getElementById("floorViewer");

    const floorNames = [
        "Stueetagen",
        "1. etage",
        "2. etage",
        "3. etage",
        "4. etage",
        "5. etage"
    ];

    let currentLoad = null;
    let selectedLevel = null;
    let floorLoaded = false;

    function showFloor(level) {
        if (!Number.isInteger(level) || level < 1 || level > 6) {
            return;
        }

        if (
            level === selectedLevel &&
            (currentLoad !== null || floorLoaded)
        ) {
            return;
        }

        if (currentLoad) {
            currentLoad.dispose();
            currentLoad = null;
        }

        selectedLevel = level;
        floorLoaded = false;

        const floorName = floorNames[level - 1];

        for (const button of floorButtons) {
            button.setAttribute(
                "aria-pressed",
                String(Number(button.dataset.floor) === level)
            );
        }

        floorViewer.replaceChildren();
        floorViewer.hidden = true;
        floorViewer.setAttribute("aria-busy", "true");
        floorViewer.setAttribute(
            "aria-label",
            `Plantegning: ${floorName}`
        );

        const stopLoading = MercLoader.start(
            "#floorLoading",
            `Henter plantegning: ${floorName}...`
        );

        const imagePath =
            `../../Utilities/Images/Etager/niveau-${level}.png`;

        const link = document.createElement("a");
        link.href = imagePath;
        link.target = "_blank";
        link.rel = "noopener";
        link.setAttribute(
            "aria-label",
            `Åbn plantegning for ${floorName.toLowerCase()} i fuld størrelse`
        );

        const image = document.createElement("img");
        image.alt =
            `Plantegning med lokaler på ${floorName.toLowerCase()}`;

        function dispose() {
            image.removeEventListener("load", handleLoad);
            image.removeEventListener("error", handleError);
            stopLoading();
        }

        const load = { dispose };
        currentLoad = load;

        function finish() {
            dispose();
            currentLoad = null;
            floorViewer.hidden = false;
            floorViewer.setAttribute("aria-busy", "false");
        }

        function handleLoad() {
            if (currentLoad !== load) return;

            floorLoaded = true;
            floorViewer.replaceChildren(link);
            finish();
        }

        function handleError() {
            if (currentLoad !== load) return;

            const message = document.createElement("p");
            message.className = "floor-error";
            message.setAttribute("role", "alert");
            message.textContent =
                `Plantegningen for ${floorName.toLowerCase()} ` +
                "kunne ikke indlæses. Tryk på etagen igen for at " +
                "prøve igen, eller åbn PDF-oversigten.";

            floorViewer.replaceChildren(message);
            finish();
        }

        image.addEventListener("load", handleLoad);
        image.addEventListener("error", handleError);

        link.append(image);
        image.src = imagePath;
    }

    for (const button of floorButtons) {
        button.setAttribute("aria-controls", "floorViewer");

        button.addEventListener("click", () => {
            showFloor(Number(button.dataset.floor));
        });
    }

    showFloor(1);
})();