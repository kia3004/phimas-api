const phimasThemeKey = "phimas-theme";

function syncPhimasThemeToggle(toggle, theme) {
    const isDarkTheme = theme === "dark";
    const nextLabel = isDarkTheme ? "Switch to light mode" : "Switch to dark mode";

    toggle.dataset.themeState = isDarkTheme ? "dark" : "light";
    toggle.setAttribute("aria-checked", String(isDarkTheme));
    toggle.setAttribute("aria-label", nextLabel);
    toggle.title = nextLabel;
}

function applyPhimasTheme(theme) {
    document.documentElement.setAttribute("data-theme", theme);
    document.querySelectorAll("[data-theme-toggle]").forEach((toggle) => {
        syncPhimasThemeToggle(toggle, theme);
    });
}

document.addEventListener("DOMContentLoaded", () => {
    const storedTheme = localStorage.getItem(phimasThemeKey) || "light";
    applyPhimasTheme(storedTheme);

    document.querySelectorAll("[data-theme-toggle]").forEach((toggle) => {
        toggle.addEventListener("click", () => {
            const nextTheme = toggle.dataset.themeState === "dark" ? "light" : "dark";
            localStorage.setItem(phimasThemeKey, nextTheme);
            applyPhimasTheme(nextTheme);
        });
    });
});
