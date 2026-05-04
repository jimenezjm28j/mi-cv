(function () {
    "use strict";

    function reindexAdminTables(form) {
        form.querySelectorAll("tbody[data-prefix]").forEach(function (tbody) {
            var prefix = tbody.getAttribute("data-prefix");
            if (!prefix) return;
            var re = new RegExp(
                prefix.replace(/[.*+?^${}()|[\]\\]/g, "\\$&") + "\\[\\d+\\]",
                "g"
            );
            tbody.querySelectorAll(":scope > tr").forEach(function (tr, i) {
                tr.querySelectorAll("[name]").forEach(function (el) {
                    el.name = el.name.replace(re, prefix + "[" + i + "]");
                });
            });
        });
    }

    function feedbackEl() {
        return document.getElementById("admin-wizard-feedback");
    }

    function clearFeedback() {
        var fb = feedbackEl();
        if (!fb) return;
        fb.textContent = "";
        fb.classList.remove("text-danger", "text-success");
    }

    var form = document.getElementById("admin-cv-form");
    if (!form) return;

    document.querySelectorAll("[data-admin-add]").forEach(function (btn) {
        btn.addEventListener("click", function () {
            var sel = btn.getAttribute("data-admin-add");
            var tbody = document.querySelector(sel);
            if (!tbody) return;
            var row = tbody.querySelector("tr:last-child");
            if (!row) return;
            var clone = row.cloneNode(true);
            clone.querySelectorAll("input, textarea, select").forEach(function (el) {
                if (el.type === "checkbox") el.checked = false;
                else el.value = "";
            });
            tbody.appendChild(clone);
        });
    });

    form.addEventListener("submit", function () {
        reindexAdminTables(form);
    });

    var root = form.querySelector("[data-admin-wizard-root]");
    if (!root) return;

    var stepCount = parseInt(root.getAttribute("data-step-count") || "8", 10);
    var mode = root.getAttribute("data-mode") || "edit";
    var initialStep = parseInt(root.getAttribute("data-initial-step") || "0", 10);
    if (initialStep < 0) initialStep = 0;
    if (initialStep >= stepCount) initialStep = stepCount - 1;

    /** Hasta este índice (inclusive) puede saltar con la barra; crece solo al guardar y avanzar. */
    var maxVisitedStep = initialStep;
    var wizardBusy = false;
    var currentStep = initialStep;

    function stepPanels() {
        return Array.from(form.querySelectorAll("[data-admin-wizard-step]"));
    }

    function markers() {
        return Array.from(root.querySelectorAll("[data-wizard-marker]"));
    }

    /** Muestra un paso: paneles + barra lateral + estado de botones (sin mensajes). */
    function applyStep(step) {
        currentStep = step;

        stepPanels().forEach(function (el) {
            var s = parseInt(el.getAttribute("data-admin-wizard-step"), 10);
            el.classList.toggle("is-active", s === step);
        });

        markers().forEach(function (li) {
            var m = parseInt(li.getAttribute("data-wizard-marker"), 10);
            li.classList.toggle("admin-wizard-progress__step--current", m === step);
            li.classList.toggle("admin-wizard-progress__step--done", m < step);

            var canNav = m <= maxVisitedStep;
            li.classList.toggle("admin-wizard-progress__step--navigable", canNav);
            li.classList.toggle("admin-wizard-progress__step--locked", !canNav);

            var labelEl = li.querySelector(".admin-wizard-progress__label");
            var labelText = labelEl ? labelEl.textContent.trim() : "Paso " + (m + 1);

            li.removeAttribute("title");
            if (canNav) {
                li.setAttribute("role", "button");
                li.tabIndex = 0;
                li.removeAttribute("aria-disabled");
                if (m === step) li.setAttribute("aria-current", "step");
                else li.removeAttribute("aria-current");
                li.setAttribute("aria-label", "Ir al paso " + labelText);
                li.setAttribute("title", m === step ? "Paso actual" : "Ir a " + labelText);
            } else {
                li.removeAttribute("role");
                li.tabIndex = -1;
                li.removeAttribute("aria-current");
                li.setAttribute(
                    "title",
                    "Guarda cada paso con «Siguiente y guardar» para llegar hasta aquí."
                );
                li.removeAttribute("aria-label");
                li.removeAttribute("aria-disabled");
            }
        });

        var prevBtn = form.querySelector("[data-admin-wizard-prev]");
        var skipBtn = form.querySelector("[data-admin-wizard-skip]");
        var nextBtn = form.querySelector("[data-admin-wizard-next]");
        var finalBtn = form.querySelector("[data-admin-wizard-final-submit]");

        if (prevBtn) {
            prevBtn.disabled = step === 0;
            prevBtn.classList.toggle("d-none", step === 0);
        }
        if (skipBtn) {
            skipBtn.style.display = step === 0 ? "none" : "";
        }

        var isLast = step === stepCount - 1;
        if (nextBtn) nextBtn.classList.toggle("d-none", isLast);
        if (finalBtn) finalBtn.classList.toggle("d-none", !isLast);

        root.scrollIntoView({ behavior: "smooth", block: "start" });
    }

    function setWizardBusy(busy) {
        wizardBusy = busy;
        var ctrls = form.querySelectorAll(
            "[data-admin-wizard-next],[data-admin-wizard-skip],[data-admin-wizard-prev],[data-admin-wizard-final-submit]"
        );
        ctrls.forEach(function (b) {
            if (busy) {
                if (!b.classList.contains("d-none")) b.setAttribute("disabled", "disabled");
            } else {
                b.removeAttribute("disabled");
            }
        });
        if (!busy) applyStep(currentStep);
    }

    async function tryParseJson(resp) {
        var text = await resp.text();
        if (!text) return null;
        try {
            return JSON.parse(text);
        } catch {
            return null;
        }
    }

    async function wizardPostAndAdvance(targetStep, errorFallback) {
        reindexAdminTables(form);
        clearFeedback();

        var fd = new FormData(form);

        try {
            setWizardBusy(true);

            var resp = await fetch(form.action, {
                method: "POST",
                headers: {
                    "X-Admin-Wizard": "1",
                    Accept: "application/json",
                },
                body: fd,
                credentials: "same-origin",
            });

            var data = await tryParseJson(resp);

            if (data && data.redirectUrl && data.ok === true) {
                window.location.href = data.redirectUrl;
                return;
            }

            if (!resp.ok || !data || data.ok !== true) {
                var lines = [];
                if (errorFallback) lines.push(errorFallback);
                if (data && Array.isArray(data.errors)) {
                    data.errors.forEach(function (e) {
                        if (e && e.message) lines.push(String(e.message));
                    });
                } else if (data && typeof data.message === "string") {
                    lines.push(data.message);
                } else if (resp.status === 401) {
                    lines.push("Sesión expirada. Recarga e inicia sesión de nuevo.");
                } else {
                    lines.push(
                        resp.status >= 400
                            ? "Revisa datos obligatorios (identidad y enlace público)."
                            : "Respuesta no válida del servidor."
                    );
                }
                var fb = feedbackEl();
                if (fb) {
                    fb.classList.add("text-danger");
                    fb.textContent = lines.filter(Boolean).join(" · ") || "Error al guardar.";
                }
                return;
            }

            var landed = Math.min(Math.max(targetStep, 0), stepCount - 1);
            maxVisitedStep = Math.max(maxVisitedStep, landed);
            applyStep(landed);
            var okFb = feedbackEl();
            if (okFb) {
                okFb.classList.remove("text-danger");
                okFb.classList.add("text-success");
                okFb.textContent = "Guardado.";
                window.setTimeout(function () {
                    if (okFb.textContent === "Guardado.") {
                        okFb.textContent = "";
                        okFb.classList.remove("text-success");
                    }
                }, 3800);
            }
        } catch (_e) {
            var ef = feedbackEl();
            if (ef) {
                ef.classList.add("text-danger");
                ef.textContent = "Sin conexión o error de red.";
            }
        } finally {
            setWizardBusy(false);
        }
    }

    var prevBtn = form.querySelector("[data-admin-wizard-prev]");
    var skipBtn = form.querySelector("[data-admin-wizard-skip]");
    var nextBtn = form.querySelector("[data-admin-wizard-next]");

    if (prevBtn) {
        prevBtn.addEventListener("click", function () {
            clearFeedback();
            applyStep(Math.max(currentStep - 1, 0));
        });
    }

    function advanceFrom(step) {
        if (mode === "create" && step === 0) {
            wizardPostAndAdvance(1, null);
            return;
        }
        wizardPostAndAdvance(step + 1, null);
    }

    if (nextBtn) {
        nextBtn.addEventListener("click", function (e) {
            e.preventDefault();
            advanceFrom(currentStep);
        });
    }

    if (skipBtn) {
        skipBtn.addEventListener("click", function (e) {
            e.preventDefault();
            advanceFrom(currentStep);
        });
    }

    var progress = root.querySelector(".admin-wizard-progress");
    if (progress) {
        progress.addEventListener("click", function (e) {
            if (wizardBusy) return;
            var li = e.target.closest("[data-wizard-marker]");
            if (!li || !progress.contains(li)) return;
            if (!li.classList.contains("admin-wizard-progress__step--navigable")) return;
            var m = parseInt(li.getAttribute("data-wizard-marker"), 10);
            if (Number.isNaN(m) || m === currentStep) return;
            clearFeedback();
            applyStep(m);
        });

        progress.addEventListener("keydown", function (e) {
            if (wizardBusy) return;
            if (e.key !== "Enter" && e.key !== " ") return;
            var li = e.target.closest("[data-wizard-marker]");
            if (!li || !progress.contains(li)) return;
            if (!li.classList.contains("admin-wizard-progress__step--navigable")) return;
            var m = parseInt(li.getAttribute("data-wizard-marker"), 10);
            if (Number.isNaN(m) || m === currentStep) return;
            e.preventDefault();
            clearFeedback();
            applyStep(m);
        });
    }

    applyStep(initialStep);
})();
