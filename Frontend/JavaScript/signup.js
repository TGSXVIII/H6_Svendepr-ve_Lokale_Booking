const signupForm = document.getElementById("signupForm");
const usernameInput = document.getElementById("username");
const passwordInput = document.getElementById("password");
const confirmPasswordInput = document.getElementById("confirmPassword");
const signupMessage = document.getElementById("signupMessage");

const lengthRequirement = document.getElementById("passwordLength");
const uppercaseRequirement = document.getElementById("passwordUppercase");
const specialRequirement = document.getElementById("passwordSpecial");

function validateUsername() {
    usernameInput.setCustomValidity(
        usernameInput.value.length > 0 &&
        usernameInput.value.trim().length === 0
            ? "Brugernavnet må ikke kun bestå af mellemrum."
            : ""
    );
}

function validatePassword() {
    const password = passwordInput.value;

    const hasEnoughCharacters = [...password].length >= 8;
    const hasUppercase = /\p{Lu}/u.test(password);
    const hasSpecialCharacter = /[\p{P}\p{S}]/u.test(password);

    lengthRequirement.classList.toggle("is-valid", hasEnoughCharacters);
    uppercaseRequirement.classList.toggle("is-valid", hasUppercase);
    specialRequirement.classList.toggle("is-valid", hasSpecialCharacter);

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
            ? `Adgangskoden skal indeholde ${missingRequirements.join(", ")}.`
            : ""
    );

    validatePasswordConfirmation();
}

function validatePasswordConfirmation() {
    confirmPasswordInput.setCustomValidity(
        confirmPasswordInput.value.length > 0 &&
        confirmPasswordInput.value !== passwordInput.value
            ? "Adgangskoderne skal være ens."
            : ""
    );
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

signupForm.addEventListener("submit", async (event) => {
    event.preventDefault();

    validateUsername();
    validatePassword();

    if (!signupForm.reportValidity()) {
        return;
    }

    const submitButton = signupForm.querySelector('button[type="submit"]');

    if (submitButton.disabled) {
        return;
    }

    const emailInput = document.getElementById("email");

    submitButton.disabled = true;
    signupMessage.textContent = "Opretter din konto...";

    try {
        const response = await fetch("http://localhost:5265/api/users/Register", {
            method: "POST",
            headers: {
                "Content-Type": "application/json"
            },
            body: JSON.stringify({
                username: usernameInput.value.trim(),
                email: emailInput.value.trim(),
                password: passwordInput.value
            })
        });

        if (!response.ok) {
            if (response.status === 409) {
                signupMessage.textContent =
                    "Brugernavnet eller e-mailadressen er allerede i brug.";
            } else if (response.status === 400) {
                signupMessage.textContent =
                    "Oplysningerne blev afvist af serveren. Kontrollér felterne.";
            } else {
                signupMessage.textContent =
                    `Kontoen kunne ikke oprettes (HTTP ${response.status}).`;
            }

            return;
        }

        passwordInput.value = "";
        confirmPasswordInput.value = "";

        window.location.replace("login.html");
    } catch (error) {
        console.error("Fejl under oprettelse:", error);

        signupMessage.textContent =
            "Kunne ikke bekræfte oprettelsen. Kontrollér forbindelsen. " +
            "Prøv at logge ind, før du forsøger at oprette kontoen igen.";
    } finally {
        submitButton.disabled = false;
    }
});