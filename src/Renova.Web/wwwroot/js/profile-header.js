(function () {
    const maxFileSize = 5 * 1024 * 1024;
    const allowedTypes = ["image/jpeg", "image/png", "image/webp"];

    function setError(root, message) {
        const error = root.querySelector("[data-profile-error]");
        if (error) {
            error.textContent = message || "";
        }
    }

    function setPreview(root, dataUrl) {
        const preview = root.querySelector("[data-profile-preview]");
        const placeholder = root.querySelector("[data-profile-placeholder]");

        if (preview) {
            preview.src = dataUrl;
            preview.classList.remove("is-hidden");
        }

        if (placeholder) {
            placeholder.classList.add("is-hidden");
        }
    }

    function clearPreview(root) {
        const preview = root.querySelector("[data-profile-preview]");
        const placeholder = root.querySelector("[data-profile-placeholder]");
        const input = root.querySelector("[data-profile-input]");
        const removeInput = root.querySelector("[data-profile-remove-input]");

        if (input) {
            input.value = "";
        }

        if (preview) {
            preview.removeAttribute("src");
            preview.classList.add("is-hidden");
        }

        if (placeholder) {
            placeholder.classList.remove("is-hidden");
        }

        if (removeInput) {
            removeInput.value = "true";
        }

        setError(root, "");
    }

    document.addEventListener("change", function (event) {
        const input = event.target.closest("[data-profile-input]");
        if (!input) {
            return;
        }

        const root = input.closest("[data-profile-header]");
        const file = input.files && input.files[0];
        if (!root || !file) {
            return;
        }

        if (!allowedTypes.includes(file.type)) {
            input.value = "";
            setError(root, "Use uma imagem JPG, PNG ou WEBP.");
            return;
        }

        if (file.size > maxFileSize) {
            input.value = "";
            setError(root, "A imagem deve ter no máximo 5 MB.");
            return;
        }

        const removeInput = root.querySelector("[data-profile-remove-input]");
        if (removeInput) {
            removeInput.value = "false";
        }

        const reader = new FileReader();
        reader.onload = function () {
            setPreview(root, reader.result);
            setError(root, "");
        };
        reader.readAsDataURL(file);
    });

    document.addEventListener("click", function (event) {
        const button = event.target.closest("[data-profile-remove]");
        if (!button) {
            return;
        }

        const root = button.closest("[data-profile-header]");
        if (root) {
            clearPreview(root);
        }
    });
})();
