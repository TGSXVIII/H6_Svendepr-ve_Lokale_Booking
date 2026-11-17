(() => {
    const storageKey = "mercBookingTheme";
    const systemTheme = window.matchMedia("(prefers-color-scheme: dark)");

    function getSavedTheme() {
        try {
            const value = localStorage.getItem(storageKey);

            return value === "light" || value === "dark"
                ? value
                : null;
        } catch {
            return null;
        }
    }

    let selectedTheme = getSavedTheme();

    function updateThemeButton() {
        const button = document.getElementById("themeToggle");

        if (!button) return;

        const dark = document.documentElement.dataset.theme === "dark";

        button.textContent = dark ? "☀ Lys tilstand" : "☾ Mørk tilstand";
        button.setAttribute("aria-pressed", String(dark));
        button.setAttribute(
            "aria-label",
            dark ? "Skift til lys tilstand" : "Skift til mørk tilstand"
        );
    }

    function applyTheme(theme) {
        document.documentElement.dataset.theme = theme;
        updateThemeButton();
    }

    applyTheme(
        selectedTheme ?? (systemTheme.matches ? "dark" : "light")
    );

    // Virker også, når navigation.html indlæses senere.
    document.addEventListener("click", (event) => {
        if (!(event.target instanceof Element)) return;

        const button = event.target.closest("#themeToggle");

        if (!button) return;

        selectedTheme =
            document.documentElement.dataset.theme === "dark"
                ? "light"
                : "dark";

        try {
            localStorage.setItem(storageKey, selectedTheme);
        } catch {
            // Temaet virker stadig, selvom browseren ikke kan gemme valget.
        }

        applyTheme(selectedTheme);
    });

    document.addEventListener("navigationLoaded", updateThemeButton);
    document.addEventListener("DOMContentLoaded", updateThemeButton);

    systemTheme.addEventListener("change", (event) => {
        if (selectedTheme === null) {
            applyTheme(event.matches ? "dark" : "light");
        }
    });

    window.addEventListener("storage", (event) => {
        if (event.key === storageKey || event.key === null) {
            selectedTheme = getSavedTheme();

            applyTheme(
                selectedTheme ?? (systemTheme.matches ? "dark" : "light")
            );
        }
    });
})();