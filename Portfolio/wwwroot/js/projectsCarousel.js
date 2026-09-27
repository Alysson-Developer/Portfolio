export function initialize(track, dotNetReference) {
    if (!track) {
        return;
    }

    const updateState = () => {
        const scrollWidth = track.scrollWidth;
        const clientWidth = track.clientWidth;
        const scrollLeft = track.scrollLeft;

        const maxScroll =
            Math.max(0, scrollWidth - clientWidth);

        /*
         * Tamanho do indicador em relação
         * ao conteúdo total.
         */
        const width =
            scrollWidth <= 0
                ? 100
                : (clientWidth / scrollWidth) * 100;

        /*
         * Espaço disponível para mover
         * o indicador.
         */
        const maxPosition =
            Math.max(0, 100 - width);

        const position =
            maxScroll <= 0
                ? 0
                : (scrollLeft / maxScroll) * maxPosition;

        const canGoPrevious =
            scrollLeft > 2;

        const canGoNext =
            scrollLeft < maxScroll - 2;

        dotNetReference.invokeMethodAsync(
            "UpdateProgress",
            position,
            width,
            canGoPrevious,
            canGoNext
        );
    };

    track.addEventListener(
        "scroll",
        updateState,
        { passive: true }
    );

    window.addEventListener(
        "resize",
        updateState
    );

    updateState();
}


export function scrollNext(track) {
    if (!track) {
        return;
    }

    const card =
        track.querySelector(".project-card");

    if (!card) {
        return;
    }

    const gap =
        parseFloat(
            getComputedStyle(track).gap
        ) || 0;

    const amount =
        card.getBoundingClientRect().width + gap;

    track.scrollBy({
        left: amount,
        behavior: "smooth"
    });
}


export function scrollPrevious(track) {
    if (!track) {
        return;
    }

    const card =
        track.querySelector(".project-card");

    if (!card) {
        return;
    }

    const gap =
        parseFloat(
            getComputedStyle(track).gap
        ) || 0;

    const amount =
        card.getBoundingClientRect().width + gap;

    track.scrollBy({
        left: -amount,
        behavior: "smooth"
    });
}