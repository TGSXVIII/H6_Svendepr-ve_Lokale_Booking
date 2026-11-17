function initializeNavigation() {
    const menuToggle = document.getElementById("menuToggle");
    const menuClose = document.getElementById("menuClose");
    const navigationMenu = document.getElementById("navigationMenu");

    if (menuToggle && menuClose && navigationMenu) {
        menuToggle.addEventListener("click", () => {
            navigationMenu.showModal();
            menuToggle.setAttribute("aria-expanded", "true");
            document.body.classList.add("menu-open");
        });

        menuClose.addEventListener("click", () => {
            navigationMenu.close();
        });

        navigationMenu.addEventListener("click", (event) => {
            const bounds = navigationMenu.getBoundingClientRect();

            const outside =
                event.clientX < bounds.left ||
                event.clientX > bounds.right ||
                event.clientY < bounds.top ||
                event.clientY > bounds.bottom;

            if (event.target === navigationMenu && outside) {
                navigationMenu.close();
            }
        });

        navigationMenu.addEventListener("close", () => {
            menuToggle.setAttribute("aria-expanded", "false");
            document.body.classList.remove("menu-open");
            menuToggle.focus();
        });
    }

    const guestActions = document.getElementById("guestActions");
    const headerUsername = document.getElementById("headerUsername");
    const logoutButton = document.getElementById("logoutButton");
    const navigationStatus = document.getElementById("navigationStatus");
    const roleItems = document.querySelectorAll("[data-roles]");

    function updateNavigation(user) {
        const loggedIn = user !== null;

        if (guestActions) {
            guestActions.hidden = loggedIn;
        }

        if (headerUsername) {
            headerUsername.hidden = !loggedIn;
            headerUsername.textContent = user?.username ?? "";
        }

        if (logoutButton) {
            logoutButton.hidden = !loggedIn;
        }

        for (const item of roleItems) {
            const allowedRoles = item.dataset.roles.split(" ");

            item.hidden =
                !loggedIn || !allowedRoles.includes(user.role);
        }
    }

    async function loadNavigation() {
        if (navigationStatus) {
            navigationStatus.textContent = "";
        }

        if (!hasActiveLogin()) {
            updateNavigation(null);
            return;
        }

        // Skjul rollebestemte punkter, indtil backend har svaret.
        for (const item of roleItems) {
            item.hidden = true;
        }

        if (guestActions) guestActions.hidden = true;
        if (logoutButton) logoutButton.hidden = false;

        if (headerUsername) {
            headerUsername.hidden = false;
            headerUsername.textContent = "Henter profil...";
        }

        const token = sessionStorage.getItem("mercBookingToken");

        try {
            const response = await fetch(
                "http://localhost:5265/api/auth/me",
                {
                    headers: {
                        Authorization: `Bearer ${token}`
                    }
                }
            );

            // Ignorer svaret, hvis brugeren har logget ud imens.
            if (sessionStorage.getItem("mercBookingToken") !== token) {
                return;
            }

            if (response.status === 401) {
                clearLogin();
                updateNavigation(null);
                return;
            }

            if (!response.ok) {
                throw new Error(`HTTP ${response.status}`);
            }

            const user = await response.json();

            if (
                typeof user.username !== "string" ||
                !["Student", "Teacher", "Admin"].includes(user.role)
            ) {
                throw new Error("Uventede brugeroplysninger.");
            }

            if (sessionStorage.getItem("mercBookingToken") !== token) {
                return;
            }

            updateNavigation(user);
        } catch (error) {
            if (sessionStorage.getItem("mercBookingToken") !== token) {
                return;
            }

            console.error("Kunne ikke hente menuens brugeroplysninger:", error);

            if (headerUsername) {
                headerUsername.textContent = "Profil utilgængelig";
            }

            if (navigationStatus) {
                navigationStatus.textContent =
                    "Kunne ikke hente din profil. Kontrollér backend, og genindlæs siden.";
            }
        }
    }

    logoutButton?.addEventListener("click", () => {
        clearLogin();
        window.location.replace("dashboard.html");
    });

    loadNavigation();
}

async function insertNavigation() {
    const container = document.getElementById("sharedNavigation");

    // Sider uden navigation behøver ikke indlæse menuen.
    if (!container) {
        return;
    }

    try {
        const response = await fetch("navigation.html");

        if (!response.ok) {
            throw new Error(
                `Navigation kunne ikke indlæses: HTTP ${response.status}`
            );
        }

        const html = await response.text();

        // HTML kommer fra vores egen fælles navigationsfil.
        container.innerHTML = html;

        // Markér automatisk den aktuelle side.
        const links = container.querySelectorAll("nav a[href]");

        for (const link of links) {
            const linkUrl = new URL(link.href);

            if (linkUrl.pathname === window.location.pathname) {
                link.setAttribute("aria-current", "page");
            } else {
                link.removeAttribute("aria-current");
            }
        }

        // Find elementer og tilføj events, efter HTML er indsat.
        initializeNavigation();
        document.dispatchEvent(new Event("navigationLoaded"));
    } catch (error) {
        console.error("Fejl ved indlæsning af navigation:", error);

        const message = document.createElement("p");
        message.setAttribute("role", "alert");
        message.textContent =
            "Menuen kunne ikke indlæses. Prøv at genindlæse siden.";

        container.replaceChildren(message);
    }
}

insertNavigation();