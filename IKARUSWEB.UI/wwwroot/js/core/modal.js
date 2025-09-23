// /wwwroot/js/core/modal.js (ESM)
export function openModal({ title, url, onReady }) {
    const id = "m-" + Math.random().toString(36).slice(2);
    const html = `
  <div class="modal fade" id="${id}" tabindex="-1">
    <div class="modal-dialog modal-lg modal-dialog-centered">
      <div class="modal-content">
        <div class="modal-header">
          <h5 class="modal-title">${title}</h5>
          <button type="button" class="btn-close" data-bs-dismiss="modal"></button>
        </div>
        <div class="modal-body position-relative"><div id="${id}-body" class="p-1"></div></div>
        <div class="modal-footer">
          <button class="btn btn-secondary" data-bs-dismiss="modal">${i18n.common["btn.cancel"]}</button>
          <button class="btn btn-primary" data-modal-submit>${i18n.common["btn.ok"]}</button>
        </div>
      </div>
    </div>
  </div>`;

    const $m = $(html).appendTo("body");
    const modal = new bootstrap.Modal(document.getElementById(id));
    const $body = $m.find("#" + id + "-body");

    const $loader = $(`
    <div class="position-absolute top-0 start-0 w-100 h-100 d-flex align-items-center justify-content-center bg-white bg-opacity-75" style="z-index:5;">
      <div class="spinner-border text-primary" style="width:2rem; height:2rem;"" role="status"><span class="visually-hidden">Yükleniyor...</span></div>
    </div>
  `).appendTo($m.find(".modal-body"));

    const api = { el: $m, body: $body, submitButton: () => $m.find("[data-modal-submit]"), close: () => modal.hide() };

    $m.on("shown.bs.modal", () => {
        $body.load(url, async () => {
            try { await onReady?.(api); }
            finally { $loader.remove(); } 
        });
    });

    $m.on("hidden.bs.modal", () => $m.remove());
    modal.show();
    return api;
}
