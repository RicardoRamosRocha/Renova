(function () {
    const root = document.documentElement;

    document.querySelectorAll("[data-tooltip]").forEach((element) => {
        if (!element.getAttribute("aria-label") && !element.getAttribute("title")) {
            element.setAttribute("aria-label", element.getAttribute("data-tooltip"));
        }
    });

    document.addEventListener("keydown", (event) => {
        if (event.key !== "Escape") {
            return;
        }

        if (window.location.hash && document.querySelector(`${window.location.hash}.admin-delete-modal, ${window.location.hash}.rn-premium-drawer`)) {
            history.pushState("", document.title, window.location.pathname + window.location.search);
        }
    });

    document.querySelectorAll("form").forEach((form) => {
        form.addEventListener("submit", () => {
            const submitter = form.querySelector("button[type='submit'], input[type='submit']");
            if (!submitter || submitter.dataset.noLoading === "true") {
                return;
            }

            submitter.classList.add("is-loading");
            submitter.setAttribute("aria-busy", "true");
        });
    });

    root.classList.add("rn-interactions-ready");
})();
