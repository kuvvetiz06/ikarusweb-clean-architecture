// /wwwroot/js/core/modal.js (ESM)
// Bootstrap 5 modal helper that lets you build content programmatically (DevExpress inside)
export function openModal({ title, onBuild, submitText = (window.defLangObj?.Save ?? "Kaydet") }) {
  const id = "app-modal-" + Math.random().toString(36).slice(2);
  const html = `
  <div class="modal fade" id="${id}" tabindex="-1">
    <div class="modal-dialog modal-lg modal-dialog-centered">
      <div class="modal-content">
        <div class="modal-header">
          <h5 class="modal-title">${title}</h5>
          <button type="button" class="btn-close" data-bs-dismiss="modal" aria-label="Close"></button>
        </div>
        <div class="modal-body"><div id="${id}-body"></div></div>
        <div class="modal-footer">
          <button class="btn btn-secondary" data-bs-dismiss="modal">${window.defLangObj?.Cancel ?? "İptal"}</button>
          <button class="btn btn-primary" data-modal-submit>${submitText}</button>
        </div>
      </div>
    </div>
  </div>`;

  const $m = $(html).appendTo("body");
  const modal = new bootstrap.Modal(document.getElementById(id));

  const bodySel = `#${id}-body`;
  const api = {
    el: $m,
    body: $(bodySel),
    submitButton: () => $m.find("[data-modal-submit]"),
    close: () => modal.hide()
  };

  if (typeof onBuild === "function") onBuild(api);
  $m.on("hidden.bs.modal", () => $m.remove());
  modal.show();
  return api;
}
