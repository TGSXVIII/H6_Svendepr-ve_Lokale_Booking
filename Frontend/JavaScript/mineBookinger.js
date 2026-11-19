(() => {
    const apiUrl = "http://localhost:5265/api/bookings";

    const list = document.getElementById("bookingsList");
    const message = document.getElementById("bookingsMessage");
    const statusFilter = document.getElementById("bookingStatusFilter");
    const retryButton = document.getElementById("retryBookings");

    let bookings = [];

    function redirectToLogin() {
        clearLogin();
        window.location.replace("login.html");
    }

    // Understøtter både enum-tal og tekst fra API'et.
    function getStatus(value) {
        switch (String(value).toLowerCase()) {
            case "0":
            case "booked":
                return { key: "booked", label: "Godkendt" };

            case "approved":
                return { key: "booked", label: "Godkendt anmodning" };

            case "1":
            case "pending":
                return { key: "pending", label: "Afventer godkendelse" };

            case "2":
            case "cancelled":
                return { key: "cancelled", label: "Annulleret" };

            case "rejected":
                return { key: "rejected", label: "Afvist" };

            default:
                return { key: "unknown", label: "Ukendt status" };
        }
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

    function renderBookings() {
        list.replaceChildren();

        const visibleBookings = bookings.filter(booking => {
            return statusFilter.value === "all" ||
                getStatus(booking.status).key === statusFilter.value;
        });

        if (bookings.length === 0) {
            message.textContent =
                "Du har ingen bookinger endnu. Find et lokale for at komme i gang.";
            return;
        }

        if (visibleBookings.length === 0) {
            message.textContent =
                "Du har ingen bookinger med den valgte status.";
            return;
        }

        message.textContent =
            `Viser ${visibleBookings.length} af dine bookinger.`;

        for (const booking of visibleBookings) {
            const card = document.createElement("article");
            card.className = "room-card booking-card";

            const title = document.createElement("h2");
            title.textContent =
                booking.roomName || `Lokale ${booking.roomId}`;

            const status = getStatus(booking.status);
            const badge = document.createElement("span");

            badge.className =
                `booking-status booking-status-${status.key}`;
            badge.textContent = status.label;

            const details = document.createElement("dl");
            details.className = "booking-details";

            addDetail(
                details,
                "Fra",
                formatDateTime(booking.requestedStart)
            );

            addDetail(
                details,
                "Til",
                formatDateTime(booking.requestedEnd)
            );

            addDetail(
                details,
                "Antal deltagere",
                String(booking.participantCount)
            );

            addDetail(
                details,
                "Formål / kommentar",
                booking.comment?.trim() || "Ingen kommentar"
            );

            addDetail(
                details,
                "Lokaletype",
                booking.roomCategory
            );

            card.append(title, badge, details);

            // Vis annullering for kommende og igangværende bookinger.
            const canCancel =
                (booking.type === "booking" && status.key === "booked") ||
                (booking.type === "request" && status.key === "pending");

            if (
                canCancel &&
                Number.isInteger(booking.id) &&
                booking.id > 0 &&
                new Date(booking.requestedEnd).getTime() > Date.now()
            ) {
                const cancelButton = document.createElement("button");

                cancelButton.type = "button";
                cancelButton.className = "booking-cancel";
                cancelButton.textContent = booking.type === "request"
                    ? "Annuller anmodning"
                    : "Annuller booking";

                cancelButton.addEventListener("click", () => {
                    if (booking.type === "request") {
                        cancelRequest(booking, cancelButton);
                    } else {
                        cancelBooking(booking, cancelButton);
                    }
                });

                card.append(cancelButton);
            }

            list.append(card);
        }
    }

    async function loadBookings() {
        if (!hasActiveLogin()) {
            redirectToLogin();
            return;
        }

        const token = sessionStorage.getItem("mercBookingToken");

        retryButton.hidden = true;
        statusFilter.disabled = true;
        list.replaceChildren();
        list.setAttribute("aria-busy", "true");
        message.textContent = "Henter dine bookinger og anmodninger...";

        try {
            const headers = {
                Authorization: `Bearer ${token}`
            };

            const [bookingResponse, requestResponse] = await Promise.all([
                fetch(`${apiUrl}/CurrentUserBookings`, { headers }),
                fetch(
                    "http://localhost:5265/api/BookingRequests/getCurrentUserRequests",
                    { headers }
                )
            ]);

            if (sessionStorage.getItem("mercBookingToken") !== token) {
                return;
            }

            if (
                bookingResponse.status === 401 ||
                requestResponse.status === 401
            ) {
                redirectToLogin();
                return;
            }

            if (!bookingResponse.ok) {
                throw new Error(
                    `Bookinger kunne ikke hentes (HTTP ${bookingResponse.status}).`
                );
            }

            if (!requestResponse.ok) {
                throw new Error(
                    `Anmodninger kunne ikke hentes (HTTP ${requestResponse.status}).`
                );
            }

            const [bookingData, requestData] = await Promise.all([
                bookingResponse.json(),
                requestResponse.json()
            ]);

            if (sessionStorage.getItem("mercBookingToken") !== token) {
                return;
            }

            if (!Array.isArray(bookingData) || !Array.isArray(requestData)) {
                throw new Error("API'et returnerede ikke de forventede lister.");
            }

            const requestStatuses = {
                "0": "approved",
                "1": "pending",
                "2": "rejected",
                "3": "cancelled"
            };

            const ownBookings = bookingData.map(booking => ({
                id: booking.id,
                type: "booking",
                roomId: booking.room?.id,
                roomName: booking.room?.name || "Ukendt lokale",
                roomCategory: booking.room?.category?.name || "Ukendt lokaletype",
                requestedStart: booking.bookingStart,
                requestedEnd: booking.bookingEnd,
                comment: booking.comment,
                status: booking.status,
                participantCount: booking.participantCount
            }));

            const ownRequests = requestData.map(request => ({
                id: request.id,
                type: "request",
                roomId: request.room?.id,
                roomName: request.room?.name || "Ukendt lokale",
                roomCategory: request.room?.category?.name || "Ukendt lokaletype",
                requestedStart: request.bookedFrom,
                requestedEnd: request.bookedTo,
                comment: request.comment,
                status:
                    requestStatuses[String(request.status)] ??
                    String(request.status).toLowerCase(),
                participantCount: request.participantCount
            }));

            bookings = [...ownBookings, ...ownRequests].sort(
                (a, b) =>
                    new Date(b.requestedStart).getTime() -
                    new Date(a.requestedStart).getTime()
            );

            statusFilter.disabled = false;
            renderBookings();
        } catch (error) {
            if (sessionStorage.getItem("mercBookingToken") !== token) {
                return;
            }

            console.error("Kunne ikke hente reservationer:", error);

            message.textContent =
                "Dine bookinger og anmodninger kunne ikke hentes. Prøv igen.";
            retryButton.hidden = false;
        } finally {
            list.setAttribute("aria-busy", "false");
        }
    }

    async function cancelRequest(request, button) {
        if (!hasActiveLogin()) {
            redirectToLogin();
            return;
        }

        if (!window.confirm("Vil du annullere din bookinganmodning?")) {
            return;
        }

        const token = sessionStorage.getItem("mercBookingToken");

        button.disabled = true;
        button.textContent = "Annullerer...";

        try {
            const response = await fetch(
                "http://localhost:5265/api/BookingRequests/cancelRequest/" +
                encodeURIComponent(request.id),
                {
                    method: "PUT",
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
                throw new Error(
                    `Anmodningen kunne ikke annulleres (HTTP ${response.status}).`
                );
            }

            await loadBookings();
        } catch (error) {
            if (sessionStorage.getItem("mercBookingToken") !== token) {
                return;
            }

            console.error("Kunne ikke annullere anmodningen:", error);
            message.textContent =
                "Annulleringen kunne ikke bekræftes. Genindlæs listen for at kontrollere status.";
        } finally {
            button.disabled = false;
            button.textContent = "Annuller anmodning";
        }
    }

    async function cancelBooking(booking, button) {
        if (!hasActiveLogin()) {
            redirectToLogin();
            return;
        }

        const confirmed = window.confirm(
            `Vil du annullere bookingen af ${
                booking.roomName || `lokale ${booking.roomId}`
            }?\n\nBookingen bliver slettet.`
        );

        if (!confirmed) {
            return;
        }

        const token = sessionStorage.getItem("mercBookingToken");

        button.disabled = true;
        button.textContent = "Annullerer...";

        try {
            const response = await fetch(
                `http://localhost:5265/api/${
                    booking.type === "request" ? "BookingRequests" : "Bookings"
                }/delete/${encodeURIComponent(booking.id)}`,
                {
                    method: "DELETE",
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

            if (response.status === 403) {
                throw new Error(
                    "Du har ikke adgang til at annullere denne booking."
                );
            }

        if (response.status === 404) {
            throw new Error("Bookingen eller slette-endpointet blev ikke fundet. " + "Genindlæs siden for at kontrollere listen.");
        }

        if (!response.ok) {
            throw new Error(`Bookingen kunne ikke annulleres (HTTP ${response.status}).`);
        }

        bookings = bookings.filter(item => !(item.id === booking.id && item.type === booking.type));
        renderBookings();

        message.textContent = "Din booking er annulleret.";
        } catch (error) {
            if (sessionStorage.getItem("mercBookingToken") !== token) {
                return;
            }

            console.error("Kunne ikke annullere booking:", error);

            message.textContent =
                error instanceof TypeError
                    ? "Kunne ikke kontakte backend. Genindlæs listen for at kontrollere bookingens status."
                    : error.message;
        } finally {
            button.disabled = false;
            button.textContent = "Annuller booking";
        }
    }

    // Knapper og områder til de to visninger.
    const showMyBookings = document.getElementById("showMyBookings");
    const showApprovals = document.getElementById("showApprovals");
    const myBookingsPanel = document.getElementById("myBookingsPanel");
    const approvalsPanel = document.getElementById("approvalsPanel");
    const accessMessage = document.getElementById("bookingAccessMessage");

    let canReviewBookings = false;

    const approvalsList = document.getElementById("approvalsList");
    const approvalsMessage = document.getElementById("approvalsMessage");

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
        if (!canReviewBookings || reviewsInProgress.has(request.id)) {
            return;
        }

        if (!hasActiveLogin()) {
            redirectToLogin();
            return;
        }

        const approving = status === 0;

        const confirmed = window.confirm(
            `Vil du ${approving ? "godkende" : "afvise"} anmodningen fra ` +
            `${request.user?.username || "brugeren"} om ` +
            `${request.room?.name || "lokalet"}?`
        );

        if (!confirmed) {
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
                throw new Error(
                    "Du har ikke adgang til at behandle anmodningen."
                );
            }

            if (response.status === 404 || response.status === 409) {
                throw new Error(
                    "Anmodningen findes ikke længere eller kan ikke behandles. " +
                    "Klik på Booking anmodninger for at opdatere listen."
                );
            }

            if (!response.ok) {
                throw new Error(
                    `Beslutningen kunne ikke bekræftes (HTTP ${response.status}). ` +
                    "Opdatér listen, før du prøver igen."
                );
            }

            // Review returnerer 204 uden JSON, så vi læser ingen response body.
            card.remove();

            approvalsMessage.textContent = approving
                ? "Anmodningen er godkendt."
                : "Anmodningen er afvist.";

            // Opdatér også egne reservationer, hvis de er berørt.
            await loadBookings();
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

    async function loadApprovals() {
        if (!canReviewBookings || approvalsLoading) {
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
        approvalsMessage.textContent = "Henter bookinganmodninger...";

        try {
            const response = await fetch(
                "http://localhost:5265/api/BookingRequests/getPendingRequests",
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

            if (response.status === 403) {
                throw new Error(
                    "Du har ikke adgang til at se bookinganmodninger."
                );
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

            for (const request of pendingRequests) {
                const card = document.createElement("article");
                card.className = "room-card booking-card";

                const title = document.createElement("h2");
                title.textContent = request.room?.name || "Ukendt lokale";

                const badge = document.createElement("span");
                badge.className = "booking-status booking-status-pending";
                badge.textContent = "Afventer godkendelse";

                const details = document.createElement("dl");
                details.className = "booking-details";

                addDetail(
                    details,
                    "Ansøger",
                    request.user?.username || "Ukendt bruger"
                );
                addDetail(details, "Fra", formatDateTime(request.bookedFrom));
                addDetail(details, "Til", formatDateTime(request.bookedTo));
                addDetail(
                    details,
                    "Antal deltagere",
                    String(request.participantCount)
                );
                addDetail(
                    details,
                    "Formål",
                    request.comment?.trim() || "Intet formål angivet"
                );

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
                    actionMessage.textContent =
                        "Anmodningens ID mangler. Genindlæs siden.";
                }

                approveButton.addEventListener("click", () => {
                    reviewRequest(
                        request,
                        0,
                        approveButton,
                        rejectButton,
                        actionMessage,
                        card
                    );
                });

                rejectButton.addEventListener("click", () => {
                    reviewRequest(
                        request,
                        2,
                        approveButton,
                        rejectButton,
                        actionMessage,
                        card
                    );
                });

                actions.append(approveButton, rejectButton);
                card.append(title, badge, details, actions, actionMessage);
                approvalsList.append(card);
            }
        } catch (error) {
            if (sessionStorage.getItem("mercBookingToken") !== token) {
                return;
            }

            console.error("Kunne ikke hente bookinganmodninger:", error);

            approvalsMessage.textContent =
                "Bookinganmodningerne kunne ikke hentes. " +
                "Klik på Booking anmodninger for at prøve igen.";
        } finally {
            approvalsLoading = false;
            approvalsList.setAttribute("aria-busy", "false");
        }
    }

    function changeBookingView(showApprovalView) {
        if (showApprovalView && !canReviewBookings) {
            return;
        }

        myBookingsPanel.hidden = showApprovalView;
        approvalsPanel.hidden = !showApprovalView;

        showMyBookings.setAttribute(
            "aria-pressed",
            String(!showApprovalView)
        );

        showApprovals.setAttribute(
            "aria-pressed",
            String(showApprovalView)
        );
    }

    showMyBookings.addEventListener("click", () => {
        changeBookingView(false);
    });

    showApprovals.addEventListener("click", () => {
        changeBookingView(true);
        loadApprovals();
    });

    // Vis kun Godkendelser for lærere og administratorer.
    async function loadBookingAccess() {
        if (!hasActiveLogin()) {
            return;
        }

        const token = sessionStorage.getItem("mercBookingToken");

        try {
            const response = await fetch(
                "http://localhost:5265/api/auth/CurrentUser",
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
                !["Student", "Teacher", "Admin"].includes(user.role)
            ) {
                throw new Error("Ukendt brugerrolle.");
            }

            canReviewBookings =
                ["Teacher", "Admin"].includes(user.role);

            showApprovals.hidden = !canReviewBookings;
            document.getElementById("bookingViews").hidden = !canReviewBookings;
            const filterContainer = document.getElementById("bookingStatusFilterContainer");
            filterContainer.hidden = user.role !== "Student";
            // Lærere og administratorer ser alle deres egne bookinger.
            if (user.role !== "Student") {
                statusFilter.value = "all";
                renderBookings();
            }
        } catch (error) {
            if (sessionStorage.getItem("mercBookingToken") !== token) {
                return;
            }

            console.error("Kunne ikke hente brugerrollen:", error);

            accessMessage.textContent =
                "Adgangen til Godkendelser kunne ikke kontrolleres. " +
                "Genindlæs siden for at prøve igen.";
        }
    }

    statusFilter.addEventListener("change", renderBookings);
    retryButton.addEventListener("click", loadBookings);

    loadBookingAccess();
    loadBookings();
})();