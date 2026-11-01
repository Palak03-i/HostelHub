/* HostelHub motion layer.
   Purely visual progressive enhancement: nothing here reads or changes form
   values, routes, validation or server data. If this file fails to load, every
   page still renders and works (content is only hidden once JS has marked the
   page with .hh-js, and a safety timer reveals everything regardless). */
(function () {
    'use strict';

    var root = document.documentElement;
    root.classList.add('hh-js');

    var reduce = window.matchMedia && window.matchMedia('(prefers-reduced-motion: reduce)').matches;
    var raf = window.requestAnimationFrame || function (fn) { return setTimeout(fn, 16); };

    function ready(fn) {
        if (document.readyState === 'loading') document.addEventListener('DOMContentLoaded', fn);
        else fn();
    }

    /* ---------- scroll progress bar + navbar shadow ---------- */
    function initScroll() {
        var bar = document.createElement('div');
        bar.className = 'hh-scroll-progress';
        bar.setAttribute('aria-hidden', 'true');
        document.body.appendChild(bar);
        var nav = document.querySelector('.app-navbar');
        var ticking = false;

        function update() {
            var max = document.documentElement.scrollHeight - window.innerHeight;
            var y = window.pageYOffset || document.documentElement.scrollTop || 0;
            bar.style.transform = 'scaleX(' + (max > 0 ? Math.min(1, y / max) : 0) + ')';
            if (nav) nav.classList.toggle('is-scrolled', y > 8);
            ticking = false;
        }
        window.addEventListener('scroll', function () {
            if (!ticking) { ticking = true; raf(update); }
        }, { passive: true });
        window.addEventListener('resize', update);
        update();
    }

    /* ---------- scroll reveal (staggered) ---------- */
    var REVEAL_SELECTOR = '.page-header, .card, .summary-card, .kpi-card, .filter-card, .directory-card, ' +
        '.complaints-card, .data-card, .feature-card, .hh-feature, .hh-role, .hh-section-head, .hh-cta-band, .hh-trust > div';

    function initReveal() {
        var nodes = Array.prototype.slice.call(document.querySelectorAll(REVEAL_SELECTOR)).filter(function (el) {
            if (el.closest('.modal')) return false;
            var p = el.parentElement;
            while (p) { if (p.matches && p.matches(REVEAL_SELECTOR)) return false; p = p.parentElement; }
            return true;
        });
        if (!nodes.length) return;

        var groups = new Map();
        nodes.forEach(function (el) {
            var parent = el.parentElement;
            var i = groups.get(parent) || 0;
            groups.set(parent, i + 1);
            el.style.setProperty('--hh-i', Math.min(i, 6));
            el.classList.add('hh-reveal');
        });

        function show(el) {
            if (el.classList.contains('is-visible')) return;
            el.classList.add('is-visible');
            // After the entrance finishes, drop the reveal classes so hover effects take over again.
            setTimeout(function () {
                el.classList.remove('hh-reveal', 'is-visible');
                el.style.removeProperty('--hh-i');
            }, 1300);
        }

        if (reduce || !('IntersectionObserver' in window)) { nodes.forEach(show); return; }

        var io = new IntersectionObserver(function (entries) {
            entries.forEach(function (entry) {
                if (entry.isIntersecting) { show(entry.target); io.unobserve(entry.target); }
            });
        }, { threshold: 0.12, rootMargin: '0px 0px -6% 0px' });
        nodes.forEach(function (el) { io.observe(el); });

        // Safety net: never leave content hidden.
        setTimeout(function () { nodes.forEach(show); }, 3500);
    }

    /* ---------- count-up numbers ---------- */
    function initCountUp() {
        var els = document.querySelectorAll('.summary-number, .kpi-value, [data-count]');
        if (!els.length || reduce) return;

        function run(el) {
            var original = el.textContent;
            var m = /^(\D*)(\d[\d,]*)(\D*)$/.exec(original.trim());
            if (!m) return;
            var target = parseInt(m[2].replace(/,/g, ''), 10);
            if (!isFinite(target) || target === 0 || target > 1e9) return;
            var hasComma = m[2].indexOf(',') > -1;
            var start = null, dur = 1200;
            function fmt(n) { return m[1] + (hasComma ? n.toLocaleString('en-US') : String(n)) + m[3]; }
            function step(ts) {
                if (start === null) start = ts;
                var p = Math.min(1, (ts - start) / dur);
                var eased = 1 - Math.pow(1 - p, 4);
                el.textContent = fmt(Math.round(target * eased));
                if (p < 1) raf(step); else el.textContent = original;
            }
            el.textContent = fmt(0);
            raf(step);
        }

        if (!('IntersectionObserver' in window)) return;
        var io = new IntersectionObserver(function (entries) {
            entries.forEach(function (e) {
                if (e.isIntersecting) { run(e.target); io.unobserve(e.target); }
            });
        }, { threshold: 0.4 });
        els.forEach(function (el) { io.observe(el); });
    }

    /* ---------- progress bars fill in ---------- */
    function initProgress() {
        var bars = document.querySelectorAll('.progress-bar');
        if (!bars.length || reduce) return;
        bars.forEach(function (bar) {
            var w = bar.style.width;
            if (!w) return;
            bar.style.width = '0%';
            var go = function () { bar.style.width = w; };
            if ('IntersectionObserver' in window) {
                var io = new IntersectionObserver(function (entries) {
                    if (entries[0].isIntersecting) { setTimeout(go, 150); io.disconnect(); }
                }, { threshold: 0.2 });
                io.observe(bar);
                setTimeout(go, 3000);
            } else { go(); }
        });
    }

    /* ---------- pointer spotlight on cards ---------- */
    function initSpotlight() {
        if (reduce) return;
        var pending = null, scheduled = false;
        document.addEventListener('pointermove', function (e) {
            if (e.pointerType === 'touch') return;
            var card = e.target.closest && e.target.closest('.card, .summary-card, .kpi-card, .feature-card');
            if (!card) return;
            pending = { card: card, x: e.clientX, y: e.clientY };
            if (scheduled) return;
            scheduled = true;
            raf(function () {
                scheduled = false;
                if (!pending) return;
                var r = pending.card.getBoundingClientRect();
                pending.card.style.setProperty('--mx', (pending.x - r.left) + 'px');
                pending.card.style.setProperty('--my', (pending.y - r.top) + 'px');
                pending = null;
            });
        }, { passive: true });
    }

    /* ---------- button ripple ---------- */
    function initRipple() {
        if (reduce) return;
        document.addEventListener('pointerdown', function (e) {
            var btn = e.target.closest && e.target.closest('.btn');
            if (!btn || btn.disabled || btn.classList.contains('disabled')) return;
            var r = btn.getBoundingClientRect();
            var size = Math.max(r.width, r.height);
            var dot = document.createElement('span');
            dot.className = 'hh-ripple';
            dot.setAttribute('aria-hidden', 'true');
            dot.style.width = dot.style.height = size + 'px';
            dot.style.left = (e.clientX - r.left - size / 2) + 'px';
            dot.style.top = (e.clientY - r.top - size / 2) + 'px';
            btn.appendChild(dot);
            setTimeout(function () { if (dot.parentNode) dot.parentNode.removeChild(dot); }, 700);
        }, { passive: true });
    }

    /* ---------- 3D tilt on the home dashboard mock ---------- */
    function initTilt() {
        var stage = document.querySelector('.hh-stage');
        var mock = stage && stage.querySelector('.hh-mock');
        if (!mock || reduce) return;
        var base = 'rotateY(-9deg) rotateX(5deg)';
        stage.addEventListener('pointermove', function (e) {
            if (e.pointerType === 'touch') return;
            var r = stage.getBoundingClientRect();
            var px = (e.clientX - r.left) / r.width - 0.5;
            var py = (e.clientY - r.top) / r.height - 0.5;
            mock.style.transform = 'rotateY(' + (-9 + px * 14) + 'deg) rotateX(' + (5 - py * 12) + 'deg)';
        });
        stage.addEventListener('pointerleave', function () { mock.style.transform = base; });
    }

    /* ---------- rotating headline word ---------- */
    function initRotator() {
        var host = document.querySelector('.hh-rotator');
        if (!host) return;
        var words = (host.getAttribute('data-words') || '').split(',').map(function (w) { return w.trim(); }).filter(Boolean);
        var word = host.querySelector('.hh-rotator-word');
        if (!word || words.length < 2 || reduce) return;
        var i = 0;
        setInterval(function () {
            word.classList.remove('is-in');
            word.classList.add('is-out');
            setTimeout(function () {
                i = (i + 1) % words.length;
                var label = word.querySelector('.hh-gradient-text') || word;
                label.textContent = words[i];
                word.classList.remove('is-out');
                word.classList.add('is-in');
            }, 360);
        }, 2600);
    }

    /* ---------- fade away success alerts ---------- */
    function initAlerts() {
        document.querySelectorAll('.alert-success').forEach(function (el) {
            setTimeout(function () {
                el.classList.add('hh-dismissing');
                setTimeout(function () { el.style.display = 'none'; }, 550);
            }, 7000);
        });
    }

    ready(function () {
        initScroll();
        initReveal();
        initCountUp();
        initProgress();
        initSpotlight();
        initRipple();
        initTilt();
        initRotator();
        initAlerts();
    });
})();
