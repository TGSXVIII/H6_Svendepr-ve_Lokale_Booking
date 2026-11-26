(() => {
    let dialog;
    let titleElement;
    let messageElement;
    let cancelButton;
    let confirmButton;
    let pending = false;

    function initialize() {
        if (dialog) return;

        dialog = document.createElement("dialog");
        dialog.className = "confirmation-dialog";
        dialog.setAttribute("aria-labelledby", "confirmationTitle");
        dialog.setAttribute("aria-describedby", "confirmationMessage");

        const form = document.createElement("form");
        form.method = "dialog";

        titleElement = document.createElement("h2");
        titleElement.id = "confirmationTitle";

        messageElement = document.createElement("p");
        messageElement.id = "confirmationMessage";

        const actions = document.createElement("div");
        actions.className = "confirmation-actions";

        cancelButton = document.createElement("button");
        cancelButton.type = "submit";
        cancelButton.value = "cancel";
        cancelButton.className = "confirmation-button confirmation-back";
        cancelButton.autofocus = true;

        confirmButton = document.createElement("button");
        confirmButton.type = "submit";
        confirmButton.value = "confirm";
        confirmButton.className =
            "confirmation-button confirmation-accept";

        actions.append(cancelButton, confirmButton);
        form.append(titleElement, messageElement, actions);
        dialog.append(form);
        document.body.append(dialog);
    }

    function ask({
        title = "Bekræft handling",
        message = "",
        confirmText = "Bekræft",
        cancelText = "Fortryd",
        danger = false
    } = {}) {
        // Åbn ikke flere bekræftelser oven i hinanden.
        if (pending) {
            return Promise.resolve(false);
        }

        initialize();
        pending = true;

        titleElement.textContent = title;
        messageElement.textContent = message;
        cancelButton.textContent = cancelText;
        confirmButton.textContent = confirmText;

        dialog.classList.toggle("confirmation-danger", danger);
        dialog.returnValue = "cancel";

        const previousFocus = document.activeElement;
        const hadScrollLock =
            document.body.classList.contains("menu-open");

        return new Promise(resolve => {
            function handleCancel() {
                // Escape beholder altid den eksisterende tilstand.
                dialog.returnValue = "cancel";
            }

            function handleClose() {
                const confirmed = dialog.returnValue === "confirm";

                dialog.removeEventListener("cancel", handleCancel);
                pending = false;

                if (!hadScrollLock) {
                    document.body.classList.remove("menu-open");
                }

                if (
                    previousFocus instanceof HTMLElement &&
                    previousFocus.isConnected
                ) {
                    previousFocus.focus();
                }

                resolve(confirmed);
            }

            dialog.addEventListener("cancel", handleCancel);
            dialog.addEventListener("close", handleClose, { once: true });

            try {
                dialog.showModal();
                document.body.classList.add("menu-open");
                cancelButton.focus();
            } catch (error) {
                dialog.removeEventListener("cancel", handleCancel);
                dialog.removeEventListener("close", handleClose);
                pending = false;

                console.error("Bekræftelsen kunne ikke åbnes:", error);
                resolve(false);
            }
        });
    }

    window.MercConfirm = Object.freeze({ ask });
})();