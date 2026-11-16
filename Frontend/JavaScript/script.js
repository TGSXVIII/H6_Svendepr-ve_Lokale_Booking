const menuToggle = document.getElementById("menuToggle");
const menuClose = document.getElementById("menuClose");
const navigationMenu = document.getElementById("navigationMenu");

if (menuToggle && menuClose && navigationMenu) {
    menuToggle.addEventListener("click", () => {
        navigationMenu.showModal();
        menuToggle.setAttribute("aria-expanded", "true");
        document.body.classList.add("menu-open");
    });

    menuClose.addEventListener("click", () => {
        navigationMenu.close();
    });

    navigationMenu.addEventListener("click", (event) => {
        const bounds = navigationMenu.getBoundingClientRect();

        const outside =
            event.clientX < bounds.left ||
            event.clientX > bounds.right ||
            event.clientY < bounds.top ||
            event.clientY > bounds.bottom;

        if (event.target === navigationMenu && outside) {
            navigationMenu.close();
        }
    });

    navigationMenu.addEventListener("close", () => {
        menuToggle.setAttribute("aria-expanded", "false");
        document.body.classList.remove("menu-open");
        menuToggle.focus();
    });
}