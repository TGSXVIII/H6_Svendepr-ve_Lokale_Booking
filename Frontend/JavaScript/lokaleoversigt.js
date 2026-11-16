const rooms = [
    {
        id: 1,
        name: "A.101",
        floor: 0,
        capacity: 24,
        equipment: ["Projektor", "Whiteboard"]
    },
    {
        id: 2,
        name: "B.201",
        floor: 1,
        capacity: 12,
        equipment: ["Whiteboard"]
    },
    {
        id: 3,
        name: "C.301",
        floor: 2,
        capacity: 30,
        equipment: ["Projektor", "Whiteboard"]
    }
];

const searchForm = document.getElementById("roomSearchForm");
const dateInput = document.getElementById("bookingDate");
const startInput = document.getElementById("startTime");
const endInput = document.getElementById("endTime");
const participantsInput = document.getElementById("participants");
const floorInput = document.getElementById("floor");

const roomsGrid = document.getElementById("roomsGrid");
const searchMessage = document.getElementById("searchMessage");

const bookingDialog = document.getElementById("bookingDialog");
const bookingForm = document.getElementById("bookingForm");
const bookingTitle = document.getElementById("bookingTitle");
const bookingSummary = document.getElementById("bookingSummary");
const bookingPurpose = document.getElementById("bookingPurpose");
const bookingMessage = document.getElementById("bookingMessage");
const closeBooking = document.getElementById("closeBooking");

let currentSearch = null;

const timeStep = 5;
const openingTime = 7 * 60;   // 07:00
const closingTime = 17 * 60;  // 17:00

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

    const previousTimeStillExists = [...endInput.options].some(
        option => option.value === previousEndTime
    );

    if (previousTimeStillExists) {
        endInput.value = previousEndTime;
    }
}

// Starttidspunkter fra 07:00 til 16:55.
startInput.replaceChildren(new Option("Vælg starttidspunkt", ""));

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

function getLocalDate() {
    const now = new Date();

    return [
        now.getFullYear(),
        String(now.getMonth() + 1).padStart(2, "0"),
        String(now.getDate()).padStart(2, "0")
    ].join("-");
}

dateInput.min = getLocalDate();

function getFloorLabel(floor) {
    return floor === 0 ? "Stueetage" : `${floor}. sal`;
}

function validateTimes() {
    dateInput.min = getLocalDate();
    startInput.setCustomValidity("");
    endInput.setCustomValidity("");

    if (startInput.value && endInput.value) {
        if (endInput.value <= startInput.value) {
            endInput.setCustomValidity(
                "Sluttidspunktet skal være efter starttidspunktet."
            );
        }
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

function addText(parent, tag, text, className = "") {
    const element = document.createElement(tag);
    element.textContent = text;

    if (className) {
        element.className = className;
    }

    parent.append(element);
    return element;
}

function renderRooms(list) {
    roomsGrid.replaceChildren();

    for (const room of list) {
        const card = document.createElement("article");
        card.className = "room-card";

        addText(card, "h2", `Lokale ${room.name}`);

        addText(
            card,
            "p",
            `${getFloorLabel(room.floor)} · ${room.capacity} personer`
        );

        addText(card, "p", room.equipment.join(" · "));

        addText(
            card,
            "p",
            "Eksempeldata – tilgængelighed er ikke kontrolleret.",
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

function openBooking(room) {
    if (!currentSearch) {
        return;
    }

    if (!hasActiveLogin()) {
        window.location.href = "auth.html";
        return;
    }

    // Resten af din eksisterende funktion fortsætter her.
    validateTimes();

    if (!searchForm.reportValidity()) {
        return;
    }

    bookingForm.reset();
    bookingMessage.textContent = "";
    bookingPurpose.setCustomValidity("");

    bookingTitle.textContent = `Book lokale ${room.name}`;

    const formattedDate = new Date(
        `${currentSearch.date}T12:00`
    ).toLocaleDateString("da-DK");

    bookingSummary.textContent =
        `${formattedDate}, kl. ${currentSearch.start}–${currentSearch.end}\n` +
        `${currentSearch.participants} deltagere · ${getFloorLabel(room.floor)}`;

    bookingDialog.showModal();
    document.body.classList.add("menu-open");
}

searchForm.addEventListener("input", () => {
    validateTimes();

    currentSearch = null;
    searchMessage.textContent = "Klik på Find lokaler for at anvende filtrene.";
    renderRooms(rooms);
});

searchForm.addEventListener("submit", (event) => {
    event.preventDefault();

    if (!hasActiveLogin()) {
        bookingDialog.close();
        window.location.href = "auth.html";
        return;
    }

    validateTimes();

    if (!searchForm.reportValidity()) {
        return;
    }

    const equipment = [
        ...searchForm.querySelectorAll(
            'input[name="equipment"]:checked'
        )
    ].map((input) => input.value);

    currentSearch = {
        date: dateInput.value,
        start: startInput.value,
        end: endInput.value,
        participants: Number(participantsInput.value)
    };

    const matchingRooms = rooms
        .filter((room) => {
            const matchesCapacity =
                room.capacity >= currentSearch.participants;

            const matchesFloor =
                floorInput.value === "" ||
                room.floor === Number(floorInput.value);

            const matchesEquipment = equipment.every(
                (item) => room.equipment.includes(item)
            );

            return matchesCapacity && matchesFloor && matchesEquipment;
        })
        .sort((a, b) => a.capacity - b.capacity);

    searchMessage.textContent = matchingRooms.length > 0
        ? `${matchingRooms.length} lokaler matcher kapacitet, etage og udstyr. ` +
          "Tilgængelighed er ikke kontrolleret endnu."
        : "Ingen eksempellokaler matcher dine krav.";

    renderRooms(matchingRooms);
});

closeBooking.addEventListener("click", () => {
    bookingDialog.close();
});

bookingDialog.addEventListener("close", () => {
    document.body.classList.remove("menu-open");
});

bookingPurpose.addEventListener("input", () => {
    bookingPurpose.setCustomValidity(
        bookingPurpose.value.trim() ? "" : "Indtast formålet."
    );
});

bookingForm.addEventListener("submit", (event) => {
    event.preventDefault();

    bookingPurpose.setCustomValidity(
        bookingPurpose.value.trim() ? "" : "Indtast formålet."
    );

    if (!bookingForm.reportValidity()) {
        return;
    }

    bookingMessage.textContent =
        "Bookingformularen er udfyldt. Der er ikke gemt en booking, " +
        "da siden endnu ikke er tilsluttet API’et.";
});

renderRooms(rooms);