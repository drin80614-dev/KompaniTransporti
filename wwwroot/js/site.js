document.addEventListener("DOMContentLoaded", () => {
    const paymentSelect = document.querySelector("[data-card-toggle='true']");
    const cardFields = document.getElementById("card-fields");

    const updateCardFields = () => {
        if (!paymentSelect || !cardFields) return;
        const showCardFields = paymentSelect.value === "Card" || paymentSelect.value === "1";
        cardFields.style.display = showCardFields ? "grid" : "none";
    };

    if (paymentSelect && cardFields) {
        paymentSelect.addEventListener("change", updateCardFields);
        updateCardFields();
    }

    document.querySelectorAll("[data-seat-picker-form]").forEach((form) => {
        const tripSelect = form.querySelector("[data-seat-trip-select]");
        const seatCountInput = form.querySelector("[data-seat-count]");
        const selectedInput = form.querySelector("[data-selected-seats]");
        const maps = Array.from(form.querySelectorAll(".seat-map"));
        let selectedSeats = new Set((selectedInput?.value || "").split(",").map((x) => x.trim()).filter(Boolean));

        const sync = () => {
            const activeTripId = tripSelect?.value || "";
            maps.forEach((map) => {
                const isActive = map.dataset.tripId === activeTripId;
                map.style.display = isActive ? "grid" : "none";
                if (!isActive) {
                    map.querySelectorAll(".seat-button.selected").forEach((button) => button.classList.remove("selected"));
                }
            });

            const activeMap = maps.find((map) => map.dataset.tripId === activeTripId);
            if (!activeMap) {
                selectedSeats = new Set();
            }

            if (activeMap) {
                activeMap.querySelectorAll(".seat-button.free").forEach((button) => {
                    button.classList.toggle("selected", selectedSeats.has(button.dataset.seat));
                });
            }

            const ordered = Array.from(selectedSeats).sort((a, b) => Number(a) - Number(b));
            if (selectedInput) selectedInput.value = ordered.join(",");
            if (seatCountInput && ordered.length > 0) seatCountInput.value = ordered.length;
        };

        maps.forEach((map) => {
            map.addEventListener("click", (event) => {
                const button = event.target.closest(".seat-button.free");
                if (!button || map.dataset.tripId !== tripSelect?.value) return;
                const number = button.dataset.seat;
                if (selectedSeats.has(number)) {
                    selectedSeats.delete(number);
                } else {
                    selectedSeats.add(number);
                }
                sync();
            });
        });

        tripSelect?.addEventListener("change", () => {
            selectedSeats = new Set();
            sync();
        });

        seatCountInput?.addEventListener("change", () => {
            if (selectedSeats.size > 0 && Number(seatCountInput.value) !== selectedSeats.size) {
                selectedSeats = new Set();
                sync();
            }
        });

        sync();
    });
});
