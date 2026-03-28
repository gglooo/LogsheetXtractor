const getScrollableParent = (element: Element | null): HTMLElement | null => {
    let parent = element?.parentElement ?? null;

    while (parent) {
        const style = window.getComputedStyle(parent);
        const hasScrollableOverflow =
            /(auto|scroll)/.test(style.overflowY) ||
            /(auto|scroll)/.test(style.overflow);

        if (
            hasScrollableOverflow &&
            parent.scrollHeight > parent.clientHeight
        ) {
            return parent;
        }

        parent = parent.parentElement;
    }

    return null;
};

const tryScroll = (roiId: string) => {
    const element = document.getElementById(`roi-${roiId}`);
    if (!element) {
        return false;
    }

    const scrollableParent = getScrollableParent(element);
    if (!scrollableParent) {
        element.scrollIntoView({ behavior: "smooth", block: "center" });
        return true;
    }

    const elementRect = element.getBoundingClientRect();
    const parentRect = scrollableParent.getBoundingClientRect();
    const offsetTop =
        elementRect.top - parentRect.top + scrollableParent.scrollTop;
    const targetTop = offsetTop - (parentRect.height - elementRect.height) / 2;

    scrollableParent.scrollTo({
        top: Math.max(0, targetTop),
        behavior: "smooth",
    });

    return true;
};

export const scrollRoiIntoView = (roiId: string) => {
    if (!tryScroll(roiId)) {
        requestAnimationFrame(() => {
            tryScroll(roiId);
        });
    }
};
