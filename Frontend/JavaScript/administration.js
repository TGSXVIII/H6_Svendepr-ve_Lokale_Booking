(() => {
    const apiUrl = "http://localhost:5265/api";
    const get = id => document.getElementById(id);

    const content = get("adminContent");
    const message = get("adminMessage");
    const retry = get("retryAdmin");
    const search = get("roomSearch");
    const statusFilter = get("roomStatus");
    const count = get("roomCount");
    const tableBody = get("roomsTableBody");

    const editor = get("roomEditor");
    const form = get("roomEditorForm");
    const editorTitle = get("editorTitle");
    const editorMessage = get("editorMessage");
    const saveButton = get("saveRoom");

    const nameInput = get("roomName");
    const numberInput = get("roomNumber");
    const floorInput = get("roomFloor");
    const capacityInput = get("roomCapacity");
    const categoryInput = get("roomCategory");
    const activeInput = get("roomActive");
    const activeField = get("activeField");

    const deleteDialog = get("deleteDialog");
    const deleteMessage = get("deleteMessage");

    let rooms = [];
    let categories = [];
    let editingId = null;
    let deletingRoom = null;
    let busy = false;
    let loading = false;
    let isAdmin = false;

    function redirectToLogin() {
        clearLogin();
        window.location.replace("login.html");
    }

    async function api(path, method = "GET", body) {
        if (!hasActiveLogin()) {
            redirectToLogin();
            throw new Error("Du skal logge ind igen.");
        }

        const token = sessionStorage.getItem("mercBookingToken");
        const headers = {
            Authorization: `Bearer ${token}`
        };

        if (body !== undefined) {
            headers["Content-Type"] = "application/json";
        }

        const response = await fetch(`${apiUrl}${path}`, {
            method,
            headers,
            body: body === undefined ? undefined : JSON.stringify(body)
        });

        if (sessionStorage.getItem("mercBookingToken") !== token) {
            throw new Error("Dit login er ændret. Genindlæs siden.");
        }

        if (response.status === 401) {
            redirectToLogin();
            throw new Error("Din session er udløbet.");
        }

        if (response.status === 403) {
            throw new Error("Du har ikke adgang til denne handling.");
        }

        if (response.status === 409) {
            throw new Error(
                "Handlingen er i konflikt med eksisterende data. " +
                "Lokalet kan eksempelvis være knyttet til bookinger."
            );
        }

        if (!response.ok) {
            throw new Error(`Handlingen mislykkedes (HTTP ${response.status}).`);
        }

        // Oprettelse, ændring og sletning kræver ikke JSON-svaret.
        if (method !== "GET") {
            return;
        }

        const data = await response.json();

        if (sessionStorage.getItem("mercBookingToken") !== token) {
            throw new Error("Dit login er ændret. Genindlæs siden.");
        }

        return data;
    }

    function errorText(error) {
        return error instanceof TypeError
            ? "Kunne ikke kontakte serveren."
            : error.message;
    }

    function addCell(row, text) {
        const cell = document.createElement("td");
        cell.textContent = text;
        row.append(cell);
        return cell;
    }

    function renderRooms() {
        tableBody.replaceChildren();

        const query = search.value.trim().toLocaleLowerCase("da-DK");

        const visible = rooms
            .filter(room => {
                const matchesSearch = [
                    room.name,
                    room.number,
                    room.floor
                ].some(value =>
                    String(value ?? "")
                        .toLocaleLowerCase("da-DK")
                        .includes(query)
                );

                const matchesStatus =
                    statusFilter.value === "all" ||
                    (statusFilter.value === "active" && room.isActive) ||
                    (statusFilter.value === "inactive" && !room.isActive);

                return matchesSearch && matchesStatus;
            })
            .sort((a, b) =>
                String(a.name).localeCompare(
                    String(b.name),
                    "da",
                    { numeric: true }
                )
            );

        count.textContent = `Viser ${visible.length} af ${rooms.length} lokaler.`;

        if (visible.length === 0) {
            const row = document.createElement("tr");
            const cell = addCell(
                row,
                rooms.length
                    ? "Ingen lokaler matcher dine filtre."
                    : "Der er ingen lokaler endnu."
            );
            cell.colSpan = 6;
            tableBody.append(row);
            return;
        }

        for (const room of visible) {
            const row = document.createElement("tr");
            const nameCell = document.createElement("td");

            const name = document.createElement("strong");
            name.className = "admin-room-name";
            name.textContent = room.name;

            const number = document.createElement("span");
            number.className = "admin-room-number";
            number.textContent = `Nr. ${room.number || "—"}`;

            nameCell.append(name, number);
            row.append(nameCell);

            addCell(row, room.floor || "—");
            addCell(row, `${room.capacity} pladser`);
            addCell(row, room.category?.name || "Uden kategori");

            const statusCell = addCell(row, "");
            const badge = document.createElement("span");
            badge.className = room.isActive
                ? "admin-status admin-status-active"
                : "admin-status admin-status-inactive";
            badge.textContent = room.isActive ? "Aktiv" : "Inaktiv";
            statusCell.append(badge);

            const actionCell = addCell(row, "");
            const actions = document.createElement("div");
            actions.className = "admin-row-actions";

            const edit = document.createElement("button");
            edit.type = "button";
            edit.textContent = "Rediger";
            edit.setAttribute("aria-label", `Rediger ${room.name}`);
            edit.addEventListener("click", () => openEditor(room));

            const remove = document.createElement("button");
            remove.type = "button";
            remove.textContent = "Slet";
            remove.setAttribute("aria-label", `Slet ${room.name}`);
            remove.addEventListener("click", () => openDelete(room));

            actions.append(edit, remove);
            actionCell.append(actions);
            tableBody.append(row);
        }
    }

    async function loadAdmin(successMessage = "") {
        if (loading) return;

        loading = true;
        isAdmin = false;
        content.hidden = true;
        retry.hidden = true;
        message.textContent = successMessage
            ? `${successMessage} Opdaterer listen...`
            : "Henter administration...";
        content.setAttribute("aria-busy", "true");
        const stopLoading = MercLoader.start(
            "#adminLoading",
            "Henter administration..."
        );

        try {
            const user = await api("/users/CurrentUser");

            if (user.role !== "Admin") {
                message.textContent = "Kun administratorer har adgang til siden.";
                return;
            }

            isAdmin = true;

            const [roomData, categoryData] = await Promise.all([
                api("/rooms/getAll"),
                api("/Categories/getAll")
            ]);

            if (!Array.isArray(roomData) || !Array.isArray(categoryData)) {
                throw new Error("Serveren returnerede ikke de forventede lister.");
            }

            rooms = roomData;
            categories = categoryData;

            renderRooms();
            content.hidden = false;

            const categoryNotice = categories.length === 0
                ? "Der mangler lokaletyper. Et lokale kræver en lokaletype."
                : "";

            message.textContent = [successMessage, categoryNotice]
                .filter(Boolean)
                .join(" ");

            get("createRoom").disabled = categories.length === 0;
        } catch (error) {
            console.error("Kunne ikke hente administration:", error);

            message.textContent = successMessage
                ? `${successMessage} Listen kunne dog ikke opdateres. Prøv igen.`
                : errorText(error);

            retry.hidden = false;
        } finally {
            loading = false;
            content.setAttribute("aria-busy", "false");
            stopLoading();
        }
    }

    function openEditor(room = null) {
        if (!isAdmin || busy) return;

        form.reset();
        editorMessage.textContent = "";
        editingId = room?.id ?? null;

        for (const input of [nameInput, numberInput, floorInput]) {
            input.setCustomValidity("");
        }

        categoryInput.replaceChildren(new Option("Vælg lokaletype", ""));

        for (const category of categories) {
            categoryInput.add(new Option(category.name, String(category.id)));
        }

        editorTitle.textContent = room ? "Rediger lokale" : "Opret lokale";
        saveButton.textContent = room ? "Gem ændringer" : "Opret lokale";
        activeField.hidden = !room;

        nameInput.value = room?.name ?? "";
        numberInput.value = room?.number ?? "";
        populateFloors(room?.floor);
        capacityInput.value = room?.capacity ?? 1;
        categoryInput.value = room?.category?.id == null
            ? ""
            : String(room.category.id);
        activeInput.checked = room?.isActive ?? true;

        saveButton.disabled = categories.length === 0;

        if (!categories.length) {
            editorMessage.textContent =
                "Der skal oprettes en lokaletype, før formularen kan gemmes.";
        }

        editor.showModal();
        document.body.classList.add("menu-open");
        nameInput.focus();
    }
    
    function floorValue(value) {
    const text = String(value ?? "").trim().toLowerCase();

    const aliases = {
        "0": "Stue",
        "stue": "Stue",
        "stueetagen": "Stue",
        "ground floor": "Stue",
        "first floor": "1",
        "second floor": "2",
        "third floor": "3",
        "fourth floor": "4",
        "fifth floor": "5"
    };

    if (aliases[text] !== undefined) {
        return aliases[text];
    }

    const match = text.match(/^([1-5])(?:\.?\s*etage)?$/);
    return match ? match[1] : "";
}

    function populateFloors(selectedFloor) {
        const labels = [
            ["Stue", "Stueetagen"],
            ["1", "1. etage"],
            ["2", "2. etage"],
            ["3", "3. etage"],
            ["4", "4. etage"],
            ["5", "5. etage"]
        ];

        floorInput.replaceChildren(new Option("Vælg etage", ""));

        for (const [key, label] of labels) {
            // Genbrug den eksisterende databasebetegnelse for etagen.
            const existing = rooms.find(room => floorValue(room.floor) === key);
            const value = existing ? String(existing.floor) : key;

            floorInput.add(new Option(label, value));
        }

        const selectedKey = floorValue(selectedFloor);
        const index = labels.findIndex(([key]) => key === selectedKey);

        floorInput.selectedIndex = index >= 0 ? index + 1 : 0;
    }

    function setEditorBusy(value) {
        busy = value;

        for (const control of form.querySelectorAll("input, select, button")) {
            control.disabled = value;
        }
    }

    function closeEditor() {
        if (!busy) editor.close();
    }

    form.addEventListener("input", event => {
        if (
            event.target instanceof HTMLInputElement ||
            event.target instanceof HTMLSelectElement
        ) {
            event.target.setCustomValidity("");
        }
        editorMessage.textContent = "";
    });

    form.addEventListener("submit", async event => {
        event.preventDefault();

        if (busy || !isAdmin) return;

        for (const input of [nameInput, numberInput, floorInput]) {
            input.setCustomValidity(
                input.value.trim() ? "" : "Udfyld feltet."
            );
        }

        if (!form.reportValidity()) return;

        const updating = editingId !== null;

        const body = {
            name: nameInput.value.trim(),
            number: numberInput.value.trim(),
            floor: floorInput.value.trim(),
            capacity: Number(capacityInput.value),
            categoryId: Number(categoryInput.value)
        };

        if (updating) {
            body.isActive = activeInput.checked;
        }

        setEditorBusy(true);
        editorMessage.textContent = "Gemmer lokale...";

        try {
            await api(
                updating
                    ? `/rooms/update/${encodeURIComponent(editingId)}`
                    : "/rooms/Create",
                updating ? "PUT" : "POST",
                body
            );

            editor.close();

            await loadAdmin(
                updating ? "Lokalet er opdateret." : "Lokalet er oprettet."
            );
        } catch (error) {
            console.error("Kunne ikke gemme lokale:", error);

            editorMessage.textContent =
                `${errorText(error)} Kontrollér listen, før du forsøger igen, ` +
                "hvis du er i tvivl om, hvorvidt ændringen blev gemt.";
        } finally {
            setEditorBusy(false);
        }
    });

    function openDelete(room) {
        if (!isAdmin || busy) return;

        deletingRoom = room;
        deleteMessage.textContent = "";
        get("deleteSummary").textContent =
            `Du er ved at slette lokalet ${room.name} (${room.number}).`;

        deleteDialog.showModal();
        document.body.classList.add("menu-open");
    }

    get("confirmDelete").addEventListener("click", async () => {
        if (busy || !isAdmin || !deletingRoom) return;

        const room = deletingRoom;
        busy = true;
        get("confirmDelete").disabled = true;
        get("cancelDelete").disabled = true;
        deleteMessage.textContent = "Sletter lokale...";

        try {
            await api(
                `/rooms/Delete/${encodeURIComponent(room.id)}`,
                "DELETE"
            );

            deleteDialog.close();
            await loadAdmin(`Lokalet ${room.name} er slettet.`);
        } catch (error) {
            console.error("Kunne ikke slette lokale:", error);

            deleteMessage.textContent =
                `${errorText(error)} Hvis lokalet bruges af bookinger, ` +
                "kan du i stedet gøre det inaktivt via Rediger.";
        } finally {
            busy = false;
            get("confirmDelete").disabled = false;
            get("cancelDelete").disabled = false;
        }
    });

    get("createRoom").addEventListener("click", () => openEditor());
    get("closeEditor").addEventListener("click", closeEditor);
    get("cancelEditor").addEventListener("click", closeEditor);

    get("cancelDelete").addEventListener("click", () => {
        if (!busy) deleteDialog.close();
    });

    for (const dialog of [editor, deleteDialog]) {
        dialog.addEventListener("cancel", event => {
            if (busy) event.preventDefault();
        });

        dialog.addEventListener("close", () => {
            document.body.classList.remove("menu-open");
        });
    }

    search.addEventListener("input", renderRooms);
    statusFilter.addEventListener("change", renderRooms);
    retry.addEventListener("click", () => loadAdmin());

    const usersContent = get("usersContent");
    const usersMessage = get("usersMessage");
    const usersTableBody = get("usersTableBody");
    const userSearch = get("userSearch");
    const reloadUsers = get("reloadUsers");

    let users = [];
    let currentAdminId = null;
    let usersBusy = false;

    const roleLabels = {
        Student: "Elev",
        Teacher: "Lærer",
        Admin: "Administrator"
    };

    function renderUsers() {
        usersTableBody.replaceChildren();

        const query = userSearch.value.trim().toLocaleLowerCase("da-DK");

        const visibleUsers = users
            .filter(user =>
                [user.username, user.email].some(value =>
                    String(value ?? "")
                        .toLocaleLowerCase("da-DK")
                        .includes(query)
                )
            )
            .sort((a, b) =>
                String(a.username).localeCompare(String(b.username), "da")
            );

        if (!visibleUsers.length) {
            const row = document.createElement("tr");
            const cell = addCell(row, "Ingen brugere matcher din søgning.");
            cell.colSpan = 4;
            usersTableBody.append(row);
            return;
        }

        for (const user of visibleUsers) {
            const row = document.createElement("tr");

            addCell(row, user.username);
            addCell(row, user.email);

            const roleCell = addCell(row, "");
            const field = document.createElement("div");
            field.className = "form-field";

            const select = document.createElement("select");
            select.setAttribute("aria-label", `Rolle for ${user.username}`);

            for (const [value, label] of Object.entries(roleLabels)) {
                select.add(new Option(label, value));
            }

            const knownRole = Object.hasOwn(roleLabels, user.role);
            select.value = knownRole ? user.role : "";

            field.append(select);
            roleCell.append(field);

            const actionCell = addCell(row, "");
            const actions = document.createElement("div");
            actions.className = "admin-row-actions";

            const save = document.createElement("button");
            save.type = "button";
            save.textContent = "Gem rolle";
            save.setAttribute("aria-label", `Gem rolle for ${user.username}`);

            const isSelf = Number(user.id) === currentAdminId;

            select.disabled = usersBusy || isSelf || !knownRole;
            save.disabled = true;

            select.addEventListener("change", () => {
                save.disabled = usersBusy || select.value === user.role;
            });

            save.addEventListener("click", () => {
                updateUserRole(user, select.value);
            });

            actions.append(save);
            actionCell.append(actions);

            if (isSelf) {
                const note = document.createElement("small");
                note.textContent = "Din egen konto";
                actionCell.append(note);
            }

            usersTableBody.append(row);
        }
    }

    async function loadUsers() {
        if (usersBusy) return;

        usersBusy = true;
        reloadUsers.disabled = true;
        usersMessage.textContent = "Henter brugere...";
        usersContent.setAttribute("aria-busy", "true");
       renderUsers();

        const stopLoading = MercLoader.start(
            "#adminLoading",
            "Henter administration..."
        );

        try {
            const currentUser = await api("/users/CurrentUser");

            if (currentUser.role !== "Admin") {
                usersContent.hidden = true;
                return;
            }

            currentAdminId = Number(currentUser.id);
            usersContent.hidden = false;

            const data = await api("/users/getAll");

            if (!Array.isArray(data)) {
                throw new Error("Serveren returnerede ikke en brugerliste.");
            }

            users = data;
            usersMessage.textContent = `Der er ${users.length} brugere.`;
       } catch (error) {
            usersContent.hidden = false;
            usersMessage.textContent = errorText(error);
        } finally {
            usersBusy = false;
            reloadUsers.disabled = false;
            usersContent.setAttribute("aria-busy", "false");
            renderUsers();
            stopLoading();
        }
    }

    async function updateUserRole(user, newRole) {
        if (
            usersBusy ||
            !Object.hasOwn(roleLabels, newRole) ||
            newRole === user.role ||
            Number(user.id) === currentAdminId
        ) {
            return;
        }

        const confirmed = await MercConfirm.ask({
            title: "Skift brugerrolle?",
            message:
                `Bruger: ${user.username}\n` +
                `Nuværende rolle: ${roleLabels[user.role] || user.role}\n` +
                `Ny rolle: ${roleLabels[newRole]}\n\n` +
                "Ændringen påvirker brugerens adgang til funktioner på hjemmesiden.",
            confirmText: "Gem rolle",
            cancelText: "Fortryd"
        });

        if (!confirmed) return;

        usersBusy = true;
        reloadUsers.disabled = true;
        renderUsers();
        usersMessage.textContent = "Gemmer rolle...";

        try {
            await api(
                `/users/updateRole/${encodeURIComponent(user.id)}` +
                `?newRole=${encodeURIComponent(newRole)}`,
                "PUT"
            );

            user.role = newRole;

            usersMessage.textContent =
                `Rollen for ${user.username} er ændret til ${roleLabels[newRole]}. ` +
                "Brugeren skal logge ud og ind igen for at få rollen i sit login-token.";
        } catch (error) {
            usersMessage.textContent =
                `${errorText(error)} Opdater brugerlisten for at kontrollere rollen.`;
        } finally {
            usersBusy = false;
            reloadUsers.disabled = false;
            renderUsers();
        }
    }

    userSearch.addEventListener("input", renderUsers);
    reloadUsers.addEventListener("click", loadUsers);

    function changeAdminView(view) {
    const showRooms = view === "rooms";

    get("roomsView").hidden = !showRooms;
    get("usersView").hidden = showRooms;

    get("showRooms").setAttribute("aria-pressed", String(showRooms));
    get("showUsers").setAttribute("aria-pressed", String(!showRooms));
    }

    get("showRooms").addEventListener("click", () => {
        changeAdminView("rooms");
    });

    get("showUsers").addEventListener("click", () => {
        changeAdminView("users");
    });

    changeAdminView("rooms");

    loadUsers();
    loadAdmin();
})();