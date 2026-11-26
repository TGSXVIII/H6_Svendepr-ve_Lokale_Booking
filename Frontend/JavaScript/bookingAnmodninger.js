(() => {
    const { redirectToLogin, formatDateTime, addDetail } = MercBooking;

    const approvalsList = document.getElementById("approvalsList");
    const approvalsMessage = document.getElementById("approvalsMessage");

    let canReview = false;
    let approvalsLoading = false;
    const reviewsInProgress = new Set();

    async function reviewRequest(
        request,
        status,
        approveButton,
        rejectButton,
        actionMessage,
        card
    ) {
        if (!canReview || reviewsInProgress.has(request.id)) {
            return;
        }

        if (!hasActiveLogin()) {
            redirectToLogin();
            return;
        }

        const approving = status === 0;

        const confirmed = await MercConfirm.ask({
            title: approving
                ? "Godkend bookinganmodning?"
                : "Afvis bookinganmodning?",

            message:
                `Ansøger: ${request.user?.username || "Ukendt bruger"}\n` +
                `Lokale: ${request.room?.name || "Ukendt lokale"}\n` +
                `Fra: ${formatDateTime(request.bookedFrom)}\n` +
                `Til: ${formatDateTime(request.bookedTo)}\n\n` +
                `Formål: ${request.comment?.trim() || "Intet formål angivet"}`,

            confirmText: approving ? "Godkend" : "Afvis",
            cancelText: "Fortryd",
            danger: !approving
        });

        if (!confirmed) {
            return;
        }

        if (!hasActiveLogin()) {
            redirectToLogin();
            return;
        }

        if (reviewsInProgress.has(request.id)) {
            return;
        }

        const token = sessionStorage.getItem("mercBookingToken");

        reviewsInProgress.add(request.id);
        approveButton.disabled = true;
        rejectButton.disabled = true;
        actionMessage.textContent = "Gemmer beslutningen...";

        try {
            const response = await fetch(
                "http://localhost:5265/api/BookingRequests/review/" +
                encodeURIComponent(request.id),
                {
                    method: "PUT",
                    headers: {
                        "Content-Type": "application/json",
                        Authorization: `Bearer ${token}`
                    },
                    body: JSON.stringify({ status })
                }
            );

            if (sessionStorage.getItem("mercBookingToken") !== token) {
                return;
            }

            if (response.status === 401) {
                redirectToLogin();
                return;
            }

            if (response.status === 403) {
                throw new Error("Du har ikke adgang til at behandle anmodningen.");
            }

            if (response.status === 404 || response.status === 409) {
                throw new Error(
                    "Anmodningen findes ikke længere eller kan ikke behandles. " +
                    "Klik på Bookinganmodninger for at opdatere listen."
                );
            }

            if (!response.ok) {
                throw new Error(
                    `Beslutningen kunne ikke bekræftes (HTTP ${response.status}). ` +
                    "Opdatér listen, før du prøver igen."
                );
            }

            // Review returnerer 204 uden JSON.
            card.remove();

            approvalsMessage.textContent = approving
                ? "Anmodningen er godkendt."
                : "Anmodningen er afvist.";

            // Fortæl "Mine bookinger", at listen skal opdateres.
            document.dispatchEvent(new CustomEvent("bookings:changed"));
        } catch (error) {
            if (sessionStorage.getItem("mercBookingToken") !== token) {
                return;
            }

            console.error("Kunne ikke behandle anmodningen:", error);

            actionMessage.textContent = error instanceof TypeError
                ? "Forbindelsen blev afbrudt. Opdatér listen for at kontrollere status."
                : error.message;
        } finally {
            reviewsInProgress.delete(request.id);
            approveButton.disabled = false;
            rejectButton.disabled = false;
        }
    }

    function renderRequest(request) {
        const card = document.createElement("article");
        card.className = "room-card booking-card";

        const title = document.createElement("h2");
        title.textContent = request.room?.name || "Ukendt lokale";

        const badge = document.createElement("span");
        badge.className = "booking-status booking-status-pending";
        badge.textContent = "Afventer godkendelse";

        const details = document.createElement("dl");
        details.className = "booking-details";

        addDetail(details, "Ansøger", request.user?.username || "Ukendt bruger");
        addDetail(details, "Fra", formatDateTime(request.bookedFrom));
        addDetail(details, "Til", formatDateTime(request.bookedTo));
        addDetail(details, "Antal deltagere", String(request.participantCount));
        addDetail(details, "Formål", request.comment?.trim() || "Intet formål angivet");

        const actions = document.createElement("div");
        actions.className = "request-actions";

        const approveButton = document.createElement("button");
        approveButton.type = "button";
        approveButton.className = "button";
        approveButton.textContent = "Godkend";

        const rejectButton = document.createElement("button");
        rejectButton.type = "button";
        rejectButton.className = "button button-secondary";
        rejectButton.textContent = "Afvis";

        const actionMessage = document.createElement("p");
        actionMessage.setAttribute("role", "status");

        const validId = Number.isInteger(request.id) && request.id > 0;

        approveButton.disabled = !validId;
        rejectButton.disabled = !validId;

        if (!validId) {
            actionMessage.textContent = "Anmodningens ID mangler. Genindlæs siden.";
        }

        approveButton.addEventListener("click", () => {
            reviewRequest(request, 0, approveButton, rejectButton, actionMessage, card);
        });

        rejectButton.addEventListener("click", () => {
            reviewRequest(request, 2, approveButton, rejectButton, actionMessage, card);
        });

        actions.append(approveButton, rejectButton);
        card.append(title, badge, details, actions, actionMessage);
        approvalsList.append(card);
    }

    async function loadApprovals() {
        if (!canReview || approvalsLoading || reviewsInProgress.size > 0) {
            return;
        }

        if (!hasActiveLogin()) {
            redirectToLogin();
            return;
        }

        const token = sessionStorage.getItem("mercBookingToken");

        approvalsLoading = true;
        approvalsList.replaceChildren();
        approvalsList.setAttribute("aria-busy", "true");
        approvalsMessage.textContent = "";
        const stopLoading = MercLoader.start(
            "#approvalsLoading",
            "Henter bookinganmodninger..."
        );

        try {
            const response = await fetch(
                "http://localhost:5265/api/BookingRequests/getPendingRequests",
                { headers: { Authorization: `Bearer ${token}` } }
            );

            if (sessionStorage.getItem("mercBookingToken") !== token) {
                return;
            }

            if (response.status === 401) {
                redirectToLogin();
                return;
            }

            if (response.status === 403) {
                throw new Error("Du har ikke adgang til at se bookinganmodninger.");
            }

            if (!response.ok) {
                throw new Error(`HTTP ${response.status}`);
            }

            const requests = await response.json();

            if (sessionStorage.getItem("mercBookingToken") !== token) {
                return;
            }

            if (!Array.isArray(requests)) {
                throw new Error("API'et returnerede ikke en liste.");
            }

            const pendingRequests = requests
                .filter(request => {
                    const status = String(request.status).toLowerCase();
                    return status === "1" || status === "pending";
                })
                .sort(
                    (a, b) =>
                        new Date(a.bookedFrom).getTime() -
                        new Date(b.bookedFrom).getTime()
                );

            approvalsMessage.textContent = pendingRequests.length
                ? `${pendingRequests.length} anmodninger afventer behandling.`
                : "Der er ingen afventende bookinganmodninger.";

            pendingRequests.forEach(renderRequest);
        } catch (error) {
            if (sessionStorage.getItem("mercBookingToken") !== token) {
                return;
            }

            console.error("Kunne ikke hente bookinganmodninger:", error);

            approvalsMessage.textContent =
                "Bookinganmodningerne kunne ikke hentes. " +
                "Klik på Bookinganmodninger for at prøve igen.";
        } finally {
            approvalsLoading = false;
            approvalsList.setAttribute("aria-busy", "false");
            stopLoading();
        }
    }

    // Bruges af booking.js.
    window.MercApprovals = {
        setAccess(value) {
            canReview = value;
        },
        load: loadApprovals
    };
})();