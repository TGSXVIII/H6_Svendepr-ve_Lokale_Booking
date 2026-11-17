const signupForm = document.getElementById("signupForm");
const usernameInput = document.getElementById("username");
const passwordInput = document.getElementById("password");
const confirmPasswordInput = document.getElementById("confirmPassword");
const signupMessage = document.getElementById("signupMessage");

const lengthRequirement = document.getElementById("passwordLength");
const uppercaseRequirement = document.getElementById("passwordUppercase");
const specialRequirement = document.getElementById("passwordSpecial");

function validateUsername() {
    passwordInput.setCustomValidity(
        password.length === 0
            ? ""
            : missingRequirements.length > 0
                ? "Adgangskoden skal opfylde de tre krav nedenfor."
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
        missingRequirements.length > 0
            ? `Adgangskoden skal indeholde ${missingRequirements.join(", ")}.`
            : ""
    );

    validatePasswordConfirmation();
}

function validatePasswordConfirmation() {
    confirmPasswordInput.setCustomValidity(
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

signupForm.addEventListener("submit", (event) => {
    event.preventDefault();

    validateUsername();
    validatePassword();

    if (!signupForm.reportValidity()) {
        return;
    }

    signupMessage.textContent =
        "Oplysningerne opfylder kravene. Oprettelse er endnu ikke tilsluttet serveren.";
});