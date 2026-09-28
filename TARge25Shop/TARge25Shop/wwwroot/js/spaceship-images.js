"use strict";

// Iga pildi kustutamise vorm saadetakse eraldi, põhivormi andmeid ei muudeta.
document.querySelectorAll("[data-image-delete-form]").forEach(form => {
    form.addEventListener("submit", async event => {
        event.preventDefault();

        const button = form.querySelector("button[type='submit']");
        const errorElement = form.querySelector("[data-image-delete-error]");
        const statusElement = document.querySelector("[data-image-delete-status]");

        // Korduv klõps ei tohi käivitada sama pildi teist kustutamist.
        if (button.disabled) {
            return;
        }

        // Kasutaja peab kinnitama just selle ühe pildi kustutamise.
        if (!window.confirm("Kas soovid selle pildi kustutada?")) {
            return;
        }

        button.disabled = true;
        errorElement.textContent = "";
        if (statusElement) {
            statusElement.textContent = "";
        }

        try {
            // FormData saadab ID-d ja Razor vormi loodud antiforgery tokeni.
            const response = await fetch(form.action, {
                method: "POST",
                credentials: "same-origin",
                headers: {
                    "X-Requested-With": "XMLHttpRequest",
                    "Accept": "application/json"
                },
                body: new FormData(form)
            });

            // HTML vealeht või võrguviga ei tohi jätta muljet edukast kustutamisest.
            const result = await response.json().catch(() => null);
            if (!response.ok || result?.success !== true) {
                const message = response.status === 400
                    ? "Kustutamine ebaõnnestus. Salvesta oma muudatused ja laadi leht uuesti."
                    : "Pilti ei õnnestunud kustutada. Kontrolli ühendust või serveri veateadet.";
                throw new Error(result?.message || message);
            }

            // Eemaldame ainult selle kaardi; teksti- ja failiväljad jäävad alles.
            form.closest("[data-image-card]").remove();
            if (statusElement) {
                statusElement.textContent = "Pilt kustutatud.";
            }
        } catch (error) {
            // textContent kuvab serveri teate tekstina, mitte käivitatava HTML-ina.
            errorElement.textContent = error instanceof Error
                ? error.message
                : "Pilti ei õnnestunud kustutada.";
        } finally {
            // Ebaõnnestunud päringu järel saab kasutaja uuesti proovida.
            button.disabled = false;
        }
    });
});
