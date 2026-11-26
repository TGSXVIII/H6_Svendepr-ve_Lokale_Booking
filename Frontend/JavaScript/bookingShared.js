// Fælles hjælpefunktioner til bookinger og bookinganmodninger.
const MercBooking = (() => {
    function redirectToLogin() {
        clearLogin();
        window.location.replace("login.html");
    }

    function formatDateTime(value) {
        const date = new Date(value);

        if (Number.isNaN(date.getTime())) {
            return "Tidspunkt ikke tilgængeligt";
        }

        return date.toLocaleString("da-DK", {
            timeZone: "Europe/Copenhagen",
            day: "2-digit",
            month: "2-digit",
            year: "numeric",
            hour: "2-digit",
            minute: "2-digit"
        });
    }

    function addDetail(parent, label, value) {
        const row = document.createElement("div");
        const term = document.createElement("dt");
        const description = document.createElement("dd");

        term.textContent = label;
        description.textContent = value;

        row.append(term, description);
        parent.append(row);
    }

    return { redirectToLogin, formatDateTime, addDetail };
})();