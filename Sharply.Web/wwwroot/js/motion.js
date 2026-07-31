// Animaciones sutiles compartidas por toda la app: fade-in al hacer scroll / cargar la pagina.
// Se carga una sola vez por layout (_DashboardLayout, _AuthLayout, _SkillFormLayout) y no requiere
// que cada vista marque sus elementos a mano: taggea automaticamente los bloques repetidos (cards, etc).
(function () {
    var prefersReducedMotion = window.matchMedia && window.matchMedia('(prefers-reduced-motion: reduce)').matches;
    if (prefersReducedMotion) return;

    var autoSelectors = '.card, .skill-card, .achievement-badge, .landing-about-section, .landing-about-highlight, .landing-about-feature';
    var groupedContainers = document.querySelectorAll('.skills-grid, .stats-row, .achievements-grid, .community-panel-grid');

    document.querySelectorAll(autoSelectors).forEach(function (el) {
        if (el.hasAttribute('data-reveal') || el.closest('.modal-overlay')) return;
        el.setAttribute('data-reveal', '');
    });

    groupedContainers.forEach(function (group) {
        Array.prototype.forEach.call(group.children, function (child, index) {
            if (child.hasAttribute('data-reveal')) {
                child.style.transitionDelay = (Math.min(index, 5) * 0.06) + 's';
            }
        });
    });

    var targets = document.querySelectorAll('[data-reveal]');
    if (!targets.length) return;

    if (!('IntersectionObserver' in window)) {
        targets.forEach(function (el) { el.classList.add('is-visible'); });
        return;
    }

    var observer = new IntersectionObserver(function (entries) {
        entries.forEach(function (entry) {
            if (entry.isIntersecting) {
                entry.target.classList.add('is-visible');
                observer.unobserve(entry.target);
            }
        });
    }, { threshold: 0.1, rootMargin: '0px 0px -30px 0px' });

    targets.forEach(function (el) { observer.observe(el); });
})();
