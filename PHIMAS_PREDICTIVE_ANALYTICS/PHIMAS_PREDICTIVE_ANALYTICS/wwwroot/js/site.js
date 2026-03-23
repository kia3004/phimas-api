const phimasThemeKey = "phimas-theme";

function applyPhimasTheme(theme) {
    document.documentElement.setAttribute("data-theme", theme);
    document.querySelectorAll("[data-theme-toggle]").forEach((toggle) => {
        toggle.checked = theme === "dark";
    });
}

document.addEventListener("DOMContentLoaded", () => {
    const storedTheme = localStorage.getItem(phimasThemeKey) || "light";
    applyPhimasTheme(storedTheme);

    document.querySelectorAll("[data-theme-toggle]").forEach((toggle) => {
        toggle.addEventListener("change", () => {
            const nextTheme = toggle.checked ? "dark" : "light";
            localStorage.setItem(phimasThemeKey, nextTheme);
            applyPhimasTheme(nextTheme);
        });
    });
});
