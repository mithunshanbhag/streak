window.streakHeatmap = {
    scrollToLatest(element) {
        if (!element) {
            return;
        }

        requestAnimationFrame(() => {
            element.scrollLeft = element.scrollWidth;
        });
    }
};
