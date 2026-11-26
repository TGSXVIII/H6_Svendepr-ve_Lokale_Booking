(() => {
    const loginForm = document.getElementById("loginForm");
    const usernameInput = document.getElementById("username");
    const passwordInput = document.getElementById("password");
    const loginMessage = document.getElementById("loginMessage");
    const submitButton = loginForm.querySelector('button[type="submit"]');

    let submitting = false;

    function validateUsername() {
        usernameInput.setCustomValidity(
            usernameInput.value.trim().length === 0
                ? "Indtast dit brugernavn."
                : ""
        );
    }

    usernameInput.addEventListener("input", () => {
        validateUsername();
        loginMessage.textContent = "";
    });

    passwordInput.addEventListener("input", () => {
        loginMessage.textContent = "";
    });

    loginForm.addEventListener("submit", async event => {
        event.preventDefault();

        if (submitting) return;

        validateUsername();

        if (!loginForm.reportValidity()) return;

        // Læs oplysningerne, før felterne deaktiveres.
        const credentials = {
            username: usernameInput.value.trim(),
            password: passwordInput.value
        };

        submitting = true;
        submitButton.disabled = true;
        submitButton.textContent = "Logger ind...";
        usernameInput.disabled = true;
        passwordInput.disabled = true;
        loginMessage.textContent = "";

        let redirecting = false;
        let stopLoading = () => {};
        const accountCard = document.querySelector(".account-card");
        accountCard.inert = true;

        try {
            stopLoading = MercLoader.start(
                "#loginLoading",
                "Logger ind..."
            );

            const response = await fetch(
                "http://localhost:5265/api/auth/login",
                {
                    method: "POST",
                    headers: {
                        "Content-Type": "application/json"
                    },
                    body: JSON.stringify(credentials)
                }
            );

            if (!response.ok) {
                loginMessage.textContent = response.status === 401
                    ? "Forkert brugernavn eller adgangskode."
                    : `Login mislykkedes (HTTP ${response.status}).`;

                return;
            }

            const data = await response.json();

            if (
                typeof data.token !== "string" ||
                data.token.trim().length === 0
            ) {
                loginMessage.textContent =
                    "Serveren returnerede ikke et login-token.";
                return;
            }

            try {
                sessionStorage.setItem("mercBookingToken", data.token);
            } catch {
                loginMessage.textContent =
                    "Browseren kunne ikke gemme dit login. " +
                    "Kontrollér browserens indstillinger for lagring.";
                return;
            }

            passwordInput.value = "";
            loginMessage.textContent =
                "Du er logget ind. Åbner lokaleoversigten...";

            window.location.assign("lokaleoversigt.html");
            redirecting = true;
        } catch (error) {
            console.error("Fejl under login:", error);

            loginMessage.textContent = error instanceof TypeError
                ? "Kunne ikke kontakte serveren. Prøv igen."
                : "Login kunne ikke gennemføres. Prøv igen.";
        } finally {
            stopLoading();

            if (!redirecting) {
                accountCard.inert = false;
                submitting = false;
                submitButton.disabled = false;
                submitButton.textContent = "Log ind";
                usernameInput.disabled = false;
                passwordInput.disabled = false;
            }
        }
    });
})();