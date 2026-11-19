const loginForm = document.getElementById("loginForm");
const usernameInput = document.getElementById("username");
const passwordInput = document.getElementById("password");
const loginMessage = document.getElementById("loginMessage");
const submitButton = loginForm.querySelector('button[type="submit"]');

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

loginForm.addEventListener("submit", async (event) => {
    event.preventDefault();

    validateUsername();

    if (!loginForm.reportValidity()) {
        return;
    }

    const submitButton = loginForm.querySelector('button[type="submit"]');

    if (submitButton.disabled) {
        return;
    }

    submitButton.disabled = true;
    loginMessage.textContent = "Logger ind...";

    try {
        const response = await fetch("http://localhost:5265/api/auth/login", {
            method: "POST",
            headers: {
                "Content-Type": "application/json"
            },
            body: JSON.stringify({
                username: usernameInput.value.trim(),
                password: passwordInput.value
            })
        });

        if (!response.ok) {
            loginMessage.textContent = response.status === 401
                ? "Forkert brugernavn eller adgangskode."
                : `Login mislykkedes (HTTP ${response.status}).`;

            return;
        }

        const data = await response.json();

        if (typeof data.token !== "string" || !data.token) {
            loginMessage.textContent = "Serveren returnerede ikke et login-token.";
            return;
        }

        sessionStorage.setItem("mercBookingToken", data.token);

        passwordInput.value = "";

        window.location.href = "lokaleoversigt.html";
        } catch (error) {
            console.error("Fejl under login:", error);

            loginMessage.textContent =
                "Login kunne ikke gennemføres. Kontrollér forbindelsen til backend.";
        } finally {
            submitButton.disabled = false;
        }
    }
);