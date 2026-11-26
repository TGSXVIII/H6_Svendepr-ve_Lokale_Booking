// Styrer siden: rolletjek og faneskift.
(() => {
    const { redirectToLogin } = MercBooking;

    const bookingViews = document.getElementById("bookingViews");
    const showMyBookings = document.getElementById("showMyBookings");
    const showApprovals = document.getElementById("showApprovals");
    const myBookingsPanel = document.getElementById("myBookingsPanel");
    const approvalsPanel = document.getElementById("approvalsPanel");
    const accessMessage = document.getElementById("bookingAccessMessage");
    const filterContainer = document.getElementById("bookingStatusFilterContainer");
    const statusFilter = document.getElementById("bookingStatusFilter");

    let canReview = false;

    function changeView(showApprovalView) {
        if (showApprovalView && !canReview) {
            return;
        }

        myBookingsPanel.hidden = showApprovalView;
        approvalsPanel.hidden = !showApprovalView;

        showMyBookings.setAttribute("aria-pressed", String(!showApprovalView));
        showApprovals.setAttribute("aria-pressed", String(showApprovalView));
    }

    showMyBookings.addEventListener("click", () => changeView(false));

    showApprovals.addEventListener("click", () => {
        changeView(true);
        MercApprovals.load();
    });

    async function init() {
        if (!hasActiveLogin()) {
            redirectToLogin();
            return;
        }

        const token = sessionStorage.getItem("mercBookingToken");

        try {
            const response = await fetch(
                "http://localhost:5265/api/users/CurrentUser",
                { headers: { Authorization: `Bearer ${token}` } }
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

            if (!user || !["Student", "Teacher", "Admin"].includes(user.role)) {
                throw new Error("Ukendt brugerrolle.");
            }

            canReview = ["Teacher", "Admin"].includes(user.role);

            MercApprovals.setAccess(canReview);

            // Elever ser ingen faner, men har statusfilter.
            bookingViews.hidden = !canReview;
            showApprovals.hidden = !canReview;
            filterContainer.hidden = user.role !== "Student";

            if (user.role !== "Student") {
                statusFilter.value = "all";
            }
        } catch (error) {
            if (sessionStorage.getItem("mercBookingToken") !== token) {
                return;
            }

            console.error("Kunne ikke hente brugerrollen:", error);

            accessMessage.textContent =
                "Brugerrollen kunne ikke kontrolleres. Genindlæs siden for at prøve igen.";
        }
    }

    init();
})();