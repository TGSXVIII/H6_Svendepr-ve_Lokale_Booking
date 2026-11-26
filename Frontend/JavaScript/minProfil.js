(() => {
    const profileContent = document.getElementById("profileContent");
    const profileMessage = document.getElementById("profileMessage");
    const profileAvatar = document.getElementById("profileAvatar");
    const profileName = document.getElementById("profileName");
    const profileRole = document.getElementById("profileRole");
    const profileUsername = document.getElementById("profileUsername");
    const profileEmail = document.getElementById("profileEmail");
    const profileRoleDetail = document.getElementById("profileRoleDetail");
    const retryProfile = document.getElementById("retryProfile");

    const roleLabels = {
        Student: "Elev",
        Teacher: "Lærer",
        Admin: "Administrator"
    };

    let loading = false;

    function redirectToLogin() {
        clearLogin();
        window.location.replace("login.html");
    }

    async function loadProfile() {
        if (loading) return;

        if (!hasActiveLogin()) {
            redirectToLogin();
            return;
        }

        loading = true;
        profileContent.hidden = true;
        profileContent.setAttribute("aria-busy", "true");
        retryProfile.hidden = true;
        retryProfile.disabled = true;
        profileMessage.textContent = "";

        const token = sessionStorage.getItem("mercBookingToken");
        let stopLoading = () => {};

        try {
            stopLoading = MercLoader.start(
                "#profileLoading",
                "Henter din profil..."
            );

            const response = await fetch(
                "http://localhost:5265/api/users/CurrentUser",
                {
                    headers: {
                        Authorization: `Bearer ${token}`
                    }
                }
            );

            if (sessionStorage.getItem("mercBookingToken") !== token) {
                return;
            }

            if (response.status === 401) {
                redirectToLogin();
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
                !user ||
                typeof user.username !== "string" ||
                !user.username.trim() ||
                !Object.hasOwn(roleLabels, user.role)
            ) {
                throw new Error(
                    "Backend returnerede uventede brugeroplysninger."
                );
            }

            const roleLabel = roleLabels[user.role];

            profileName.textContent = user.username;
            profileUsername.textContent = user.username;
            profileRole.textContent = roleLabel;
            profileRoleDetail.textContent = roleLabel;

            profileEmail.textContent =
                typeof user.email === "string" && user.email.trim()
                    ? user.email
                    : "Ikke angivet";

            profileAvatar.textContent = [...user.username.trim()]
                .slice(0, 2)
                .join("")
                .toLocaleUpperCase("da-DK");

            profileContent.hidden = false;
        } catch (error) {
            if (sessionStorage.getItem("mercBookingToken") !== token) {
                return;
            }

            console.error("Kunne ikke hente profil:", error);

            profileMessage.textContent =
                "Din profil kunne ikke hentes. Prøv igen.";

            retryProfile.hidden = false;
        } finally {
            stopLoading();
            loading = false;
            retryProfile.disabled = false;
            profileContent.setAttribute("aria-busy", "false");
        }
    }

    retryProfile.addEventListener("click", loadProfile);
    loadProfile();
})();