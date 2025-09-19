// /wwwroot/js/screens/roombedtype/index.js (ESM)
import { roomBedTypeGrid, openCreate } from "./component.js";

document.addEventListener("DOMContentLoaded", () => {
  roomBedTypeGrid();
  const btn = document.getElementById("btn-roombedtype-new");
  if (btn) btn.addEventListener("click", () => openCreate());
});
