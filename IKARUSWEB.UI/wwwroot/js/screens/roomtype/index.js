// /wwwroot/js/screens/odatip/index.js (ESM)
import { mountOdaTipGrid, openCreate } from "./component.js";

document.addEventListener("DOMContentLoaded", () => {
  window.__R = window.__R || ((k) => (window.defLangObj?.[k] ?? k));
  mountOdaTipGrid();

  const btn = document.getElementById("btn-odatip-new");
  if (btn) btn.addEventListener("click", () => openCreate());
});
