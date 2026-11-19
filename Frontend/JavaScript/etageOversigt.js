const floorButtons = document.querySelectorAll("[data-floor]");
const floorViewer = document.getElementById("floorViewer");

function showFloor(level) {
    if (!Number.isInteger(level) || level < 1 || level > 6) {
        return;
    }
    for (const button of floorButtons) {
        button.setAttribute(
            "aria-pressed",
            String(Number(button.dataset.floor) === level)
        );
    }

    const imagePath =
        `../../Utilities/Images/Etager/niveau-${level}.png`;

    const link = document.createElement("a");
    link.href = imagePath;
    link.target = "_blank";
    link.rel = "noopener";
    link.setAttribute(
        "aria-label",
        `Åbn plantegning for niveau ${level} i fuld størrelse`
    );

    const image = document.createElement("img");
    image.alt = `Plantegning med lokaler på niveau ${level}`;

    image.addEventListener("error", () => {
        const message = document.createElement("p");
        message.setAttribute("role", "alert");
        message.textContent =
            `Plantegningen for niveau ${level} kunne ikke indlæses. ` +
            "Prøv at åbne PDF-oversigten.";

        link.replaceWith(message);
    });

    link.append(image);
    floorViewer.replaceChildren(link);

    image.src = imagePath;
}

for (const button of floorButtons) {
    button.addEventListener("click", () => {
        showFloor(Number(button.dataset.floor));
    });
}

showFloor(1);