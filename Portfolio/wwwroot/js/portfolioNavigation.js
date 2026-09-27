window.portfolioSidebar = {
    initialize() {

        const sections = [
            document.getElementById("inicio"),
            document.getElementById("projetos"),
            document.getElementById("experiencia"),
            document.getElementById("educacao"),
            document.getElementById("tecnologias"),
            document.getElementById("sobre"),
            document.getElementById("contato")
        ].filter(Boolean);

        const sidebar =
            document.getElementById("portfolio-sidebar");

        if (!sections.length || !sidebar)
            return;

        const update = () => {

            const viewportCenter =
                window.innerHeight / 2;

            let currentSection = sections[0];
            let closestDistance = Infinity;

            for (const section of sections) {

                const rect =
                    section.getBoundingClientRect();

                const sectionCenter =
                    rect.top + rect.height / 2;

                const distance =
                    Math.abs(
                        sectionCenter - viewportCenter
                    );

                if (distance < closestDistance) {

                    closestDistance = distance;
                    currentSection = section;
                }
            }

            const shouldHide =
                currentSection.id === "inicio" ||
                currentSection.id === "contato";

            sidebar.classList.toggle(
                "is-hidden",
                shouldHide
            );
        };

        window.addEventListener(
            "scroll",
            update,
            { passive: true }
        );

        window.addEventListener(
            "resize",
            update
        );

        update();
    }
};

window.portfolioNextLink = {
    initialize(dotNetReference) {

        const sections = [
            document.getElementById("inicio"),
            document.getElementById("projetos"),
            document.getElementById("experiencia"),
            document.getElementById("educacao"),
            document.getElementById("tecnologias"),
            document.getElementById("sobre"),
            document.getElementById("contato")
        ].filter(Boolean);

        if (!sections.length)
            return;

        const observer = new IntersectionObserver(
            entries => {

                const visibleSections = entries
                    .filter(entry => entry.isIntersecting)
                    .sort(
                        (a, b) =>
                            b.intersectionRatio -
                            a.intersectionRatio
                    );

                if (!visibleSections.length)
                    return;

                dotNetReference.invokeMethodAsync(
                    "SetSection",
                    visibleSections[0].target.id
                );

            },
            {
                threshold: [0.25, 0.5, 0.75],
                rootMargin: "-15% 0px -15% 0px"
            }
        );

        sections.forEach(section =>
            observer.observe(section)
        );
    }
};