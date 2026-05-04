(function () {
    const form = document.getElementById("admin-cv-form");
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
        document.querySelectorAll("tbody[data-prefix]").forEach(function (tbody) {
            var prefix = tbody.getAttribute("data-prefix");
            if (!prefix) return;
            var re = new RegExp(prefix.replace(/[.*+?^${}()|[\]\\]/g, "\\$&") + "\\[\\d+\\]", "g");
            var rows = tbody.querySelectorAll(":scope > tr");
            rows.forEach(function (tr, i) {
                tr.querySelectorAll("[name]").forEach(function (el) {
                    el.name = el.name.replace(re, prefix + "[" + i + "]");
                });
            });
        });
    });
})();
