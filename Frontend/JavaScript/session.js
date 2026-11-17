function clearLogin() {
    sessionStorage.removeItem("mercBookingToken");
}

function hasActiveLogin() {
    const token = sessionStorage.getItem("mercBookingToken");

    if (!token) {
        return false;
    }

    try {
        const payloadPart = token.split(".")[1];

        const base64 = payloadPart
            .replace(/-/g, "+")
            .replace(/_/g, "/");

        const paddedBase64 = base64.padEnd(
            Math.ceil(base64.length / 4) * 4,
            "="
        );

        const bytes = Uint8Array.from(
            atob(paddedBase64),
            character => character.charCodeAt(0)
        );

        const payload = JSON.parse(new TextDecoder().decode(bytes));

        if (
            typeof payload.exp !== "number" ||
            payload.exp * 1000 <= Date.now()
        ) {
            clearLogin();
            return false;
        }

        return true;
    } catch {
        clearLogin();
        return false;
    }
}