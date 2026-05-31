(function () {
    'use strict';

    var cfg = window.sessionTimeoutConfig;
    if (!cfg) {
        return;
    }

    var modalEl = document.getElementById('sessionTimeoutModal');
    var countdownEl = document.getElementById('sessionTimeoutCountdown');
    var stayBtn = document.getElementById('sessionTimeoutStayBtn');
    var logoutBtn = document.getElementById('sessionTimeoutLogoutBtn');
    var logoutForm = document.getElementById('sessionTimeoutLogoutForm');

    if (!modalEl || !countdownEl || !stayBtn || !logoutBtn || !logoutForm) {
        return;
    }

    var bsModal = bootstrap.Modal.getOrCreateInstance(modalEl);

    var idleTimerId = null;
    var countdownIntervalId = null;
    var modalIsOpen = false;

    function clearIdleTimer() {
        if (idleTimerId !== null) {
            clearTimeout(idleTimerId);
            idleTimerId = null;
        }
    }

    function clearCountdown() {
        if (countdownIntervalId !== null) {
            clearInterval(countdownIntervalId);
            countdownIntervalId = null;
        }
    }

    function startIdleTimer() {
        clearIdleTimer();
        idleTimerId = setTimeout(showWarning, cfg.idleMs);
    }

    function resetIdleTimer() {
        if (modalIsOpen) {
            return;
        }
        startIdleTimer();
    }

    function showWarning() {
        modalIsOpen = true;
        countdownEl.textContent = Math.ceil(cfg.warningMs / 1000);

        bsModal.show();

        var startedAt = Date.now();
        countdownIntervalId = setInterval(function () {
            var elapsed = Date.now() - startedAt;
            var left = Math.max(0, cfg.warningMs - elapsed);
            countdownEl.textContent = Math.ceil(left / 1000);

            if (left <= 0) {
                clearCountdown();
                doLogout();
            }
        }, 250);
    }

    function doLogout() {
        clearIdleTimer();
        clearCountdown();
        logoutForm.submit();
    }

    function stayLoggedIn() {
        clearCountdown();

        fetch(cfg.pingUrl, {
            method: 'POST',
            credentials: 'same-origin',
            headers: {
                'RequestVerificationToken': cfg.antiForgeryToken,
                'Accept': 'application/json'
            }
        }).then(function (resp) {
            if (resp.status === 401 || resp.status === 403) {
                doLogout();
                return;
            }
            bsModal.hide();
            modalIsOpen = false;
            startIdleTimer();
        }).catch(function () {
            doLogout();
        });
    }

    stayBtn.addEventListener('click', stayLoggedIn);
    logoutBtn.addEventListener('click', doLogout);

    var activityEvents = ['mousemove', 'mousedown', 'keydown', 'scroll', 'touchstart', 'wheel'];
    activityEvents.forEach(function (evt) {
        document.addEventListener(evt, resetIdleTimer, { passive: true });
    });

    startIdleTimer();
})();
