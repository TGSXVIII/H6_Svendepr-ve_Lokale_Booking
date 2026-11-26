(() => {
    const { redirectToLogin, formatDateTime, addDetail } = MercBooking;

    const apiUrl = "http://localhost:5265/api/bookings";

    const list = document.getElementById("bookingsList");
    const message = document.getElementById("bookingsMessage");
    const statusFilter = document.getElementById("bookingStatusFilter");
    const retryButton = document.getElementById("retryBookings");

    let bookings = [];

    // Understøtter både enum-tal og tekst fra API'et.
    function getStatus(value) {
        switch (String(value).toLowerCase()) {
            case "0":
            case "booked":
                return { key: "booked", label: "Godkendt" };

            case "approved":
                return { key: "booked", label: "Godkendt anmodning" };

            case "pending":
                return { key: "pending", label: "Afventer godkendelse" };

            case "1":
            case "cancelled":
                return { key: "cancelled", label: "Annulleret" };

            case "rejected":
                return { key: "rejected", label: "Afvist" };

            default:
                return { key: "unknown", label: "Ukendt status" };
        }
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

            addDetail(details, "Fra", formatDateTime(booking.requestedStart));
            addDetail(details, "Til", formatDateTime(booking.requestedEnd));
            addDetail(details, "Antal deltagere", String(booking.participantCount));
            addDetail(
                details,
                "Formål / kommentar",
                booking.comment?.trim() || "Ingen kommentar"
            );
            addDetail(details, "Lokaletype", booking.roomCategory);

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
        message.textContent = "";
        const stopLoading = MercLoader.start(
            "#bookingsLoading",
            "Henter dine bookinger og anmodninger..."
        );

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
            stopLoading();
        }
    }

    function confirmCancellation(reservation) {
        const isRequest = reservation.type === "request";
        const roomName =
            reservation.roomName || `Lokale ${reservation.roomId}`;

        return MercConfirm.ask({
            title: isRequest
                ? "Annuller anmodning?"
                : "Annuller booking?",

            message:
                `${roomName}\n` +
                `Fra: ${formatDateTime(reservation.requestedStart)}\n` +
                `Til: ${formatDateTime(reservation.requestedEnd)}\n\n` +
                (isRequest
                    ? "Anmodningen bliver annulleret og vil ikke længere afvente godkendelse."
                    : "Bookingen bliver annulleret, og du mister reservationen af lokalet."),

            confirmText: "Ja, annuller",

            cancelText: isRequest
                ? "Behold anmodning"
                : "Behold booking",

            danger: true
        });
    }

    async function cancelRequest(request, button) {
        if (!hasActiveLogin()) {
            redirectToLogin();
            return;
        }

        const confirmed = await confirmCancellation(request);

        if (!confirmed) return;

        // Login kan være udløbet, mens dialogen stod åben.
        if (!hasActiveLogin()) {
            redirectToLogin();
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

        if (button.disabled) return;

        const confirmed = await confirmCancellation(booking);

        if (!confirmed) return;

        if (!hasActiveLogin()) {
            redirectToLogin();
            return;
        }

        const token = sessionStorage.getItem("mercBookingToken");

        button.disabled = true;
        button.textContent = "Annullerer...";

        try {
            const response = await fetch(
                `${apiUrl}/Cancel/${encodeURIComponent(booking.id)}`,
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

            if (response.status === 403) {
                throw new Error(
                    "Du har ikke adgang til at annullere denne booking."
                );
            }

            if (response.status === 404) {
                throw new Error(
                    "Bookingen blev ikke fundet. Opdater listen."
                );
            }

            if (response.status === 409) {
                throw new Error(
                    "Bookingen kan ikke længere annulleres. Opdater listen."
                );
            }

            if (!response.ok) {
                throw new Error(
                    `Annulleringen kunne ikke bekræftes ` +
                    `(HTTP ${response.status}). Opdater listen før et nyt forsøg.`
                );
            }

            // Behold bookingen i listen med den nye status.
            booking.status = "cancelled";
            renderBookings();

            message.textContent = "Din booking er annulleret.";
        } catch (error) {
            if (sessionStorage.getItem("mercBookingToken") !== token) {
                return;
            }

            console.error("Kunne ikke annullere booking:", error);

            message.textContent = error instanceof TypeError
                ? "Forbindelsen blev afbrudt. Opdater listen for at kontrollere status."
                : error.message;
        } finally {
            button.disabled = false;
            button.textContent = "Annuller booking";
        }
    }

    statusFilter.addEventListener("change", renderBookings);
    retryButton.addEventListener("click", loadBookings);

    // Når en anmodning er behandlet i den anden fane, opdateres egne bookinger.
    document.addEventListener("bookings:changed", loadBookings);

    loadBookings();
})();