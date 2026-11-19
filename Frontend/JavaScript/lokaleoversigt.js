let rooms = [];
let roomsLoaded = false;
let currentSearch = null;
let selectedRoom = null;
let bookingIsSubmitting = false;

const searchForm = document.getElementById("roomSearchForm");
const dateInput = document.getElementById("bookingDate");
const startInput = document.getElementById("startTime");
const endInput = document.getElementById("endTime");
const participantsInput = document.getElementById("participants");
const floorInput = document.getElementById("floor");
const categoryInput = document.getElementById("roomCategory");
const searchButton = searchForm.querySelector('button[type="submit"]');
const roomsGrid = document.getElementById("roomsGrid");
const searchMessage = document.getElementById("searchMessage");
const roomsLoading = document.getElementById("roomsLoading");
const bookingDialog = document.getElementById("bookingDialog");
const bookingForm = document.getElementById("bookingForm");
const bookingTitle = document.getElementById("bookingTitle");
const bookingSummary = document.getElementById("bookingSummary");
const bookingPurpose = document.getElementById("bookingPurpose");
const bookingPurposeCounter = document.getElementById("bookingPurposeCounter");
const bookingMessage = document.getElementById("bookingMessage");
const closeBooking = document.getElementById("closeBooking");

const timeStep = 5;
const openingTime = 7 * 60;
const closingTime = 17 * 60;

// Udstyr findes endnu ikke i API-svaret.
// Deaktivér filteret, så vi ikke filtrerer ud fra opdigtede data.
const equipmentFieldset = searchForm.querySelector(".equipment-filter");

if (equipmentFieldset) {
    equipmentFieldset.disabled = true;

    for (const checkbox of equipmentFieldset.querySelectorAll("input")) {
        checkbox.checked = false;
    }

    const note = document.createElement("p");
    note.className = "equipment-note";
    equipmentFieldset.append(note);
}

function getLocalDate() {
    const now = new Date();

    return [
        now.getFullYear(),
        String(now.getMonth() + 1).padStart(2, "0"),
        String(now.getDate()).padStart(2, "0")
    ].join("-");
}

function formatTime(totalMinutes) {
    const hours = String(Math.floor(totalMinutes / 60)).padStart(2, "0");
    const minutes = String(totalMinutes % 60).padStart(2, "0");

    return `${hours}:${minutes}`;
}

function updateEndTimeOptions() {
    const previousEndTime = endInput.value;

    endInput.replaceChildren();
    endInput.setCustomValidity("");

    if (!startInput.value) {
        endInput.add(new Option("Vælg starttidspunkt først", ""));
        endInput.disabled = true;
        return;
    }

    const [hours, minutes] = startInput.value.split(":").map(Number);
    const startMinutes = hours * 60 + minutes;

    endInput.add(new Option("Vælg sluttidspunkt", ""));
    endInput.disabled = false;

    for (
        let total = startMinutes + timeStep;
        total <= closingTime;
        total += timeStep
    ) {
        const time = formatTime(total);
        endInput.add(new Option(time, time));
    }

    if (
        [...endInput.options].some(
            option => option.value === previousEndTime
        )
    ) {
        endInput.value = previousEndTime;
    }
}

function validateTimes() {
    dateInput.min = getLocalDate();

    startInput.setCustomValidity("");
    endInput.setCustomValidity("");

    if (
        startInput.value &&
        endInput.value &&
        endInput.value <= startInput.value
    ) {
        endInput.setCustomValidity(
            "Sluttidspunktet skal være efter starttidspunktet."
        );
    }

    if (dateInput.value && startInput.value) {
        const start = new Date(
            `${dateInput.value}T${startInput.value}`
        );

        if (start < new Date()) {
            startInput.setCustomValidity(
                "Starttidspunktet må ikke være i fortiden."
            );
        }
    }
}

// Etager vises som teksten fra databasen.
// Vi antager ikke længere, at 0 betyder stueetage.
function getFloorLabel(floor) {
    return floor || "Etage ikke angivet";
}

function getRoomTitle(room) {
    if (room.number && room.name && room.number !== room.name) {
        return `${room.number} – ${room.name}`;
    }

    return room.number || room.name || `Lokale ${room.id}`;
}

function addText(parent, tag, text, className = "") {
    const element = document.createElement(tag);
    element.textContent = text;

    if (className) {
        element.className = className;
    }

    parent.append(element);
    return element;
}

function populateFilters() {
    floorInput.replaceChildren(new Option("Alle etager", ""));
    categoryInput.replaceChildren(new Option("Alle lokaletyper", ""));

    const floors = [...new Set(
        rooms.map(room => room.floor).filter(Boolean)
    )].sort((a, b) =>
        a.localeCompare(b, "da", { numeric: true })
    );

    for (const floor of floors) {
        floorInput.add(new Option(getFloorLabel(floor), floor));
    }

    const categories = new Map();

    for (const room of rooms) {
        if (room.categoryId !== null) {
            categories.set(room.categoryId, room.categoryName);
        }
    }

    const sortedCategories = [...categories.entries()]
        .sort((a, b) => a[1].localeCompare(b[1], "da"));

    for (const [id, name] of sortedCategories) {
        categoryInput.add(new Option(name, String(id)));
    }

    floorInput.disabled = floors.length === 0;
    categoryInput.disabled = categories.size === 0;
}

function renderRooms(list) {
    roomsGrid.replaceChildren();

    for (const room of list) {
        const card = document.createElement("article");
        card.className = "room-card";

        addText(card, "h2", getRoomTitle(room));

        addText(
            card,
            "p",
            `${room.categoryName} · ${getFloorLabel(room.floor)}`
        );

        addText(card, "p", `${room.capacity} personer`);

        addText(
            card,
            "p",
            "Ledighed i det valgte tidsrum er ikke kontrolleret.",
            "room-status"
        );

        const button = addText(
            card,
            "button",
            currentSearch ? "Se booking" : "Vælg tidsrum først",
            "button"
        );

        button.type = "button";
        button.disabled = currentSearch === null;
        button.addEventListener("click", () => openBooking(room));

        roomsGrid.append(card);
    }
}

async function loadRooms() {
    roomsLoaded = false;
    searchButton.disabled = true;
    floorInput.disabled = true;
    categoryInput.disabled = true;

    searchMessage.textContent = "Henter lokaler...";
    roomsGrid.setAttribute("aria-busy", "true");

    // Undgå et hurtigt blink, hvis API'et svarer med det samme.
    const loadingTimer = setTimeout(() => {
        roomsLoading.hidden = false;
    }, 150);

    try {
        if (!hasActiveLogin()) {
            window.location.replace("login.html");
            return;
        }

        const token = sessionStorage.getItem("mercBookingToken");

        const headers = {
            Authorization: `Bearer ${token}`
        };

        const userResponse = await fetch(
            "http://localhost:5265/api/auth/CurrentUser",
            { headers }
        );

        if (sessionStorage.getItem("mercBookingToken") !== token) {
            return;
        }

        if (userResponse.status === 401) {
            clearLogin();
            window.location.replace("login.html");
            return;
        }

        if (!userResponse.ok) {
            throw new Error(
                `Brugeren kunne ikke hentes (HTTP ${userResponse.status}).`
            );
        }

        const user = await userResponse.json();

        if (sessionStorage.getItem("mercBookingToken") !== token) {
            return;
        }

        if (!["Student", "Teacher", "Admin"].includes(user.role)) {
            throw new Error("Ukendt brugerrolle.");
        }

        const endpoint = user.role === "Admin" ? "getAll" : "getActive";

        const response = await fetch(
            `http://localhost:5265/api/rooms/${endpoint}`,
            { headers }
        );

        if (response.status === 401) {
            clearLogin();
            window.location.replace("login.html");
            return;
        }

        if (!response.ok) {
            throw new Error(`HTTP ${response.status}`);
        }

        const data = await response.json();

        if (!Array.isArray(data)) {
            throw new Error("API’et returnerede ikke en liste af lokaler.");
        }

        rooms = data
            .filter(room => room.isActive === true)
            .map(room => ({
                id: room.id,
                name: String(room.name ?? "").trim(),
                number: String(room.number ?? "").trim(),
                floor: String(room.floor ?? "").trim(),
                capacity: Number(room.capacity),

                // Del kategoriobjektet op i ID og navn til frontend.
                categoryId: room.category?.id ?? null,
                categoryName:
                    String(room.category?.name ?? "").trim() ||
                    "Uden kategori"
            }));

        currentSearch = null;
        roomsLoaded = true;

        populateFilters();
        renderRooms(rooms);

        searchButton.disabled = rooms.length === 0;

        searchMessage.textContent = rooms.length > 0
            ? `${rooms.length} aktive lokaler hentet. ` +
              "Vælg tidsrum og filtre, og klik på Find lokaler."
            : "Der er ingen aktive lokaler i databasen.";
    } catch (error) {
        console.error("Kunne ikke hente lokaler:", error);

        rooms = [];
        currentSearch = null;
        renderRooms([]);

        searchMessage.textContent =
            "Lokalerne kunne ikke hentes. Kontrollér backend, " +
            "og genindlæs siden.";

        floorInput.replaceChildren(
            new Option("Kunne ikke hente etager", "")
        );

        categoryInput.replaceChildren(
            new Option("Kunne ikke hente lokaletyper", "")
        );
    } finally {
        clearTimeout(loadingTimer);
        roomsLoading.hidden = true;
        roomsGrid.setAttribute("aria-busy", "false");
    }
}

function openBooking(room) {
    if (!currentSearch) {
        return;
    }

    if (!hasActiveLogin()) {
        window.location.href = "login.html";
        return;
    }

    validateTimes();

    if (!searchForm.reportValidity()) {
        return;
    }

    selectedRoom = room;

    bookingForm.reset();
    const submitButton = bookingForm.querySelector('button[type="submit"]');
    submitButton.disabled = false;
    submitButton.textContent = "Send booking";
    updatePurposeCounter();
    bookingMessage.textContent = "";
    bookingPurpose.setCustomValidity("");

    bookingTitle.textContent = `Book ${getRoomTitle(room)}`;

    const formattedDate = new Date(
        `${currentSearch.date}T12:00`
    ).toLocaleDateString("da-DK");

    bookingSummary.textContent =
        `${formattedDate}, kl. ${currentSearch.start}–${currentSearch.end}\n` +
        `${currentSearch.participants} deltagere\n` +
        `${room.categoryName} · ${getFloorLabel(room.floor)}`;

    bookingDialog.showModal();
    document.body.classList.add("menu-open");
}

// Opsæt dato og tidsvalg.
dateInput.min = getLocalDate();

startInput.replaceChildren(
    new Option("Vælg starttidspunkt", "")
);

for (
    let total = openingTime;
    total < closingTime;
    total += timeStep
) {
    const time = formatTime(total);
    startInput.add(new Option(time, time));
}

startInput.addEventListener("input", updateEndTimeOptions);
updateEndTimeOptions();

// Ændring af filtre kræver en ny søgning.
searchForm.addEventListener("input", () => {
    validateTimes();

    currentSearch = null;

    if (!roomsLoaded || rooms.length === 0) {
        return;
    }

    searchMessage.textContent =
        "Klik på Find lokaler for at anvende filtrene.";

    renderRooms(rooms);
});

searchForm.addEventListener("submit", (event) => {
    event.preventDefault();

    if (!roomsLoaded || rooms.length === 0) {
        return;
    }

    validateTimes();

    if (!searchForm.reportValidity()) {
        return;
    }

    currentSearch = {
        date: dateInput.value,
        start: startInput.value,
        end: endInput.value,
        participants: Number(participantsInput.value),
        floor: floorInput.value,
        categoryId: categoryInput.value
    };

    const matchingRooms = rooms
        .filter(room => {
            const matchesCapacity =
                room.capacity >= currentSearch.participants;

            const matchesFloor =
                currentSearch.floor === "" ||
                room.floor === currentSearch.floor;

            const matchesCategory =
                currentSearch.categoryId === "" ||
                String(room.categoryId) === currentSearch.categoryId;

            return matchesCapacity && matchesFloor && matchesCategory;
        })
        .sort((a, b) => a.capacity - b.capacity);

    searchMessage.textContent = matchingRooms.length > 0
        ? `${matchingRooms.length} lokaler matcher kapacitet, etage og lokaletype. ` +
          "Ledighed er ikke kontrolleret endnu."
        : "Ingen lokaler matcher dine filtre.";

    renderRooms(matchingRooms);
});

closeBooking.addEventListener("click", () => {
    if (!bookingIsSubmitting) {
        bookingDialog.close();
    }
});

bookingDialog.addEventListener("cancel", (event) => {
    if (bookingIsSubmitting) {
        event.preventDefault();
    }
});

bookingDialog.addEventListener("close", () => {
    selectedRoom = null;
    document.body.classList.remove("menu-open");
});

function updatePurposeCounter() {
    bookingPurposeCounter.textContent =
        `${bookingPurpose.value.length} / 300 tegn`;
}

bookingPurpose.addEventListener("input", () => {
    bookingPurpose.setCustomValidity(
        bookingPurpose.value.trim() ? "" : "Indtast formålet."
    );

    updatePurposeCounter();
});

updatePurposeCounter();

const bookingNotification =
    document.getElementById("bookingNotification");

let bookingNotificationTimer;

function showBookingNotification(text) {
    clearTimeout(bookingNotificationTimer);

    bookingNotification.textContent = text;

    bookingNotificationTimer = setTimeout(() => {
        bookingNotification.textContent = "";
    }, 6000);
}

bookingForm.addEventListener("submit", async (event) => {
    event.preventDefault();

    const submitButton =
        bookingForm.querySelector('button[type="submit"]');

    if (bookingIsSubmitting || submitButton.disabled) {
        return;
    }

    if (!hasActiveLogin()) {
        bookingDialog.close();
        window.location.replace("login.html");
        return;
    }

    if (!selectedRoom || !currentSearch) {
        return;
    }

    validateTimes();

    if (!searchForm.checkValidity()) {
        bookingMessage.textContent =
            "Tidsrummet er ikke længere gyldigt. " +
            "Luk dialogen, og vælg et nyt tidsrum.";
        return;
    }

    bookingPurpose.setCustomValidity(
        !bookingPurpose.value.trim()
            ? "Indtast formålet."
            : bookingPurpose.value.length > 300
                ? "Formålet må højst være 300 tegn."
                : ""
    );

    if (!bookingForm.reportValidity()) {
        return;
    }

    // Gem formularens værdier inden de asynkrone kald.
    const roomId = selectedRoom.id;
    const participantCount = currentSearch.participants;
    const comment = bookingPurpose.value.trim();

    // Forudsætter, at browserens tidszone er dansk tid.
    const start = new Date(
        `${currentSearch.date}T${currentSearch.start}`
    );
    const end = new Date(
        `${currentSearch.date}T${currentSearch.end}`
    );

    const token = sessionStorage.getItem("mercBookingToken");
    const originalButtonText = submitButton.textContent;
    let succeeded = false;

    bookingIsSubmitting = true;
    closeBooking.disabled = true;

    submitButton.disabled = true;
    bookingMessage.textContent = "Sender...";

    try {
        const userResponse = await fetch(
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

        if (userResponse.status === 401) {
            clearLogin();
            window.location.replace("login.html");
            return;
        }

        if (!userResponse.ok) {
            throw new Error(
                `Din bruger kunne ikke hentes (HTTP ${userResponse.status}).`
            );
        }

        const user = await userResponse.json();

        if (sessionStorage.getItem("mercBookingToken") !== token) {
            return;
        }

        if (!["Student", "Teacher", "Admin"].includes(user.role)) {
            throw new Error("Din bruger har en ukendt rolle.");
        }

        const isStudent = user.role === "Student";

        const endpoint = isStudent
            ? "http://localhost:5265/api/BookingRequests/create"
            : "http://localhost:5265/api/Bookings/create";

        const body = {
            roomId,
            participantCount,
            comment,
            ...(isStudent
                ? {
                    bookedFrom: start.toISOString(),
                    bookedTo: end.toISOString()
                }
                : {
                    bookingStart: start.toISOString(),
                    bookingEnd: end.toISOString()
                })
        };

        const response = await fetch(endpoint, {
            method: "POST",
            headers: {
                "Content-Type": "application/json",
                Authorization: `Bearer ${token}`
            },
            body: JSON.stringify(body)
        });

        if (sessionStorage.getItem("mercBookingToken") !== token) {
            return;
        }

        if (response.status === 401) {
            clearLogin();
            window.location.replace("login.html");
            return;
        }

        if (response.status === 403) {
            throw new Error("Du har ikke adgang til denne handling.");
        }

        if (response.status === 409) {
            throw new Error(
                "Reservationen kunne ikke oprettes på grund af en konflikt. " +
                "Prøv et andet tidsrum."
            );
        }

        if (!response.ok) {
            throw new Error(
                `Serveren meldte en fejl (HTTP ${response.status}). ` +
                "Kontrollér, om reservationen blev gemt, før du sender igen."
            );
        }

        succeeded = true;

        bookingDialog.close();

        const confirmation = isStudent
            ? "Din bookinganmodning er sendt til godkendelse."
            : "Din booking er gemt. Du kan se den under Mine bookinger.";

        searchMessage.textContent = confirmation;
        showBookingNotification(confirmation);

    } catch (error) {
        console.error("Kunne ikke sende reservation:", error);

        bookingMessage.textContent = error instanceof TypeError
            ? "Forbindelsen blev afbrudt. Kontrollér, om reservationen blev gemt, før du sender igen."
            : error.message;
    } finally {
        bookingIsSubmitting = false;
        closeBooking.disabled = false;

        if (!succeeded) {
            submitButton.disabled = false;
            submitButton.textContent = originalButtonText;
        }
    }
});

loadRooms();