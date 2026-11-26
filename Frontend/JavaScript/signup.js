(() => {
    const signupForm = document.getElementById("signupForm");
    const accountCard = document.querySelector(".account-card");

    const usernameInput = document.getElementById("username");
    const emailInput = document.getElementById("email");
    const passwordInput = document.getElementById("password");
    const confirmPasswordInput = document.getElementById("confirmPassword");
    const signupMessage = document.getElementById("signupMessage");
    const submitButton = signupForm.querySelector('button[type="submit"]');

    const lengthRequirement = document.getElementById("passwordLength");
    const uppercaseRequirement = document.getElementById("passwordUppercase");
    const specialRequirement = document.getElementById("passwordSpecial");

    let submitting = false;

    function validateUsername() {
        usernameInput.setCustomValidity(
            usernameInput.value.length > 0 &&
            usernameInput.value.trim().length === 0
                ? "Brugernavnet må ikke kun bestå af mellemrum."
                : ""
        );
    }

    function validatePasswordConfirmation() {
        confirmPasswordInput.setCustomValidity(
            confirmPasswordInput.value.length > 0 &&
            confirmPasswordInput.value !== passwordInput.value
                ? "Adgangskoderne skal være ens."
                : ""
        );
    }

    function validatePassword() {
        const password = passwordInput.value;

        const hasEnoughCharacters = [...password].length >= 8;
        const hasUppercase = /\p{Lu}/u.test(password);
        const hasSpecialCharacter = /[\p{P}\p{S}]/u.test(password);

        lengthRequirement.classList.toggle(
            "is-valid",
            hasEnoughCharacters
        );

        uppercaseRequirement.classList.toggle(
            "is-valid",
            hasUppercase
        );

        specialRequirement.classList.toggle(
            "is-valid",
            hasSpecialCharacter
        );

        const missingRequirements = [];

        if (!hasEnoughCharacters) {
            missingRequirements.push("mindst 8 tegn");
        }

        if (!hasUppercase) {
            missingRequirements.push("mindst ét stort bogstav");
        }

        if (!hasSpecialCharacter) {
            missingRequirements.push("mindst ét specialtegn");
        }

        passwordInput.setCustomValidity(
            password.length > 0 && missingRequirements.length > 0
                ? `Adgangskoden skal indeholde ${
                    missingRequirements.join(", ")
                }.`
                : ""
        );

        validatePasswordConfirmation();
    }

    usernameInput.addEventListener("input", validateUsername);
    passwordInput.addEventListener("input", validatePassword);

    confirmPasswordInput.addEventListener(
        "input",
        validatePasswordConfirmation
    );

    signupForm.addEventListener("input", () => {
        signupMessage.textContent = "";
    });

    signupForm.addEventListener("submit", async event => {
        event.preventDefault();

        if (submitting) return;

        validateUsername();
        validatePassword();

        if (!signupForm.reportValidity()) return;

        const body = {
            username: usernameInput.value.trim(),
            email: emailInput.value.trim(),
            password: passwordInput.value
        };

        const previousFocus = document.activeElement;

        submitting = true;
        submitButton.disabled = true;
        submitButton.textContent = "Opretter konto...";
        signupMessage.textContent = "";
        accountCard.inert = true;

        let redirecting = false;
        let stopLoading = () => {};

        try {
            stopLoading = MercLoader.start(
                "#signupLoading",
                "Opretter din konto..."
            );

            const response = await fetch(
                "http://localhost:5265/api/users/Register",
                {
                    method: "POST",
                    headers: {
                        "Content-Type": "application/json"
                    },
                    body: JSON.stringify(body)
                }
            );

            if (!response.ok) {
                if (response.status === 409) {
                    signupMessage.textContent =
                        "Brugernavnet eller e-mailadressen er allerede i brug.";
                } else if (response.status === 400) {
                    signupMessage.textContent =
                        "Oplysningerne blev afvist af serveren. " +
                        "Kontrollér felterne.";
                } else {
                    signupMessage.textContent =
                        `Oprettelsen kunne ikke bekræftes ` +
                        `(HTTP ${response.status}). ` +
                        "Prøv at logge ind, før du opretter kontoen igen.";
                }

                return;
            }

            passwordInput.value = "";
            confirmPasswordInput.value = "";
            validatePassword();

            signupMessage.textContent =
                "Din konto er oprettet. Åbner login...";

            window.location.replace("login.html");
            redirecting = true;
        } catch (error) {
            console.error("Fejl under oprettelse:", error);

            signupMessage.textContent =
                "Oprettelsen kunne ikke bekræftes. " +
                "Prøv at logge ind, før du forsøger at oprette kontoen igen.";
        } finally {
            stopLoading();

            if (!redirecting) {
                submitting = false;
                submitButton.disabled = false;
                submitButton.textContent = "Opret konto";
                accountCard.inert = false;

                if (
                    previousFocus instanceof HTMLElement &&
                    accountCard.contains(previousFocus)
                ) {
                    previousFocus.focus();
                }
            }
        }
    });
})();