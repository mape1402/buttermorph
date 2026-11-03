(function () {
    "use strict";

    const registrationType = "ButterMorphDesignerSessionRegistered";
    const closeRequestType = "ButterMorphDesignerCloseRequested";
    const closeCompletedType = "ButterMorphDesignerSessionClosed";
    let closing = false;

    function getToken() {
        return document.querySelector("input[name='__RequestVerificationToken']");
    }

    function getCloseUrl() {
        const url = new URL(window.location.href);
        url.searchParams.set("handler", "CloseSession");
        return url;
    }

    function getRegistration() {
        const token = getToken();
        if (!token) {
            return null;
        }

        const closeUrl = getCloseUrl();
        return {
            type: registrationType,
            closeUrl: closeUrl.pathname + closeUrl.search,
            requestVerificationToken: token.value
        };
    }

    function notifyHost(message) {
        if (window.parent && window.parent !== window) {
            window.parent.postMessage(message, window.location.origin);
        }

        if (window.opener && !window.opener.closed) {
            window.opener.postMessage(message, window.location.origin);
        }
    }

    function closeSession() {
        if (closing) {
            return;
        }

        const registration = getRegistration();
        if (!registration) {
            return;
        }

        closing = true;
        const body = new URLSearchParams();
        body.set("__RequestVerificationToken", registration.requestVerificationToken);

        fetch(registration.closeUrl, {
            method: "POST",
            body: body,
            credentials: "same-origin",
            keepalive: true
        }).catch(function () {
            const payload = new Blob([body.toString()], { type: "application/x-www-form-urlencoded;charset=UTF-8" });
            navigator.sendBeacon(registration.closeUrl, payload);
        }).finally(function () {
            notifyHost({ type: closeCompletedType });
        });
    }

    function touchSession() {
        const token = getToken();
        if (!token || closing) {
            return;
        }

        const url = new URL(window.location.href);
        url.searchParams.set("handler", "TouchSession");
        const body = new URLSearchParams();
        body.set("__RequestVerificationToken", token.value);
        fetch(url.toString(), { method: "POST", body: body, credentials: "same-origin", keepalive: true });
    }

    window.addEventListener("message", function (event) {
        if (event.origin !== window.location.origin || !event.data || event.data.type !== closeRequestType) {
            return;
        }

        closeSession();
    });

    const registration = getRegistration();
    if (!registration) {
        return;
    }

    notifyHost(registration);
    window.setInterval(touchSession, 60000);
}());
