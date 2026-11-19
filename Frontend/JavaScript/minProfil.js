const profileContent = document.getElementById("profileContent");
const profileMessage = document.getElementById("profileMessage");
const profileAvatar = document.getElementById("profileAvatar");
const profileName = document.getElementById("profileName");
const profileRole = document.getElementById("profileRole");
const profileUsername = document.getElementById("profileUsername");
const profileEmail = document.getElementById("profileEmail");
const profileRoleDetail = document.getElementById("profileRoleDetail");
const retryProfile = document.getElementById("retryProfile");

const profileRoleLabels = {
    Student: "Elev",
    Teacher: "Lærer",
    Admin: "Administrator"
};

function redirectToProfileLogin() {
    clearLogin();
    window.location.replace("login.html");
}

async function loadProfile() {
    if (!hasActiveLogin()) {
        redirectToProfileLogin();
        return;
    }

    profileContent.hidden = true;
    retryProfile.hidden = true;
    retryProfile.disabled = true;
    profileMessage.textContent = "Henter din profil...";

    const token = sessionStorage.getItem("mercBookingToken");

    try {
        const response = await fetch(
            "http://localhost:5265/api/users/CurrentUser",
            {
                headers: {
                    Authorization: `Bearer ${token}`
                }
            }
        );

        // Undgå at vise data fra et login, der er blevet ændret.
        if (sessionStorage.getItem("mercBookingToken") !== token) {
            return;
        }

        if (response.status === 401) {
            redirectToProfileLogin();
            return;
        }

        if (!response.ok) {
            throw new Error(`HTTP ${response.status}`);
        }

        const user = await response.json();

        if (sessionStorage.getItem("mercBookingToken") !== token) {
            return;
        }

        if (
            typeof user.username !== "string" ||
            !Object.hasOwn(profileRoleLabels, user.role)
        ) {
            throw new Error("Backend returnerede uventede brugeroplysninger.");
        }

        const roleLabel = profileRoleLabels[user.role];

        profileName.textContent = user.username;
        profileUsername.textContent = user.username;
        profileRole.textContent = roleLabel;
        profileRoleDetail.textContent = roleLabel;

        profileEmail.textContent =
            typeof user.email === "string" && user.email.trim()
                ? user.email
                : "Ikke tilgængelig endnu";

        // Midlertidig avatar med de første to tegn i brugernavnet.
        profileAvatar.textContent = [...user.username.trim()]
            .slice(0, 2)
            .join("")
            .toLocaleUpperCase("da-DK");

        profileMessage.textContent = "";
        profileContent.hidden = false;
    } catch (error) {
        if (sessionStorage.getItem("mercBookingToken") !== token) {
            return;
        }

        console.error("Kunne ikke hente profil:", error);

        profileMessage.textContent =
            "Din profil kunne ikke hentes. Kontrollér forbindelsen til backend.";

        retryProfile.hidden = false;
    } finally {
        retryProfile.disabled = false;
    }
}

retryProfile.addEventListener("click", loadProfile);

loadProfile();