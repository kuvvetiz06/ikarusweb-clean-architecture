// /wwwroot/js/screens/roombedtype/index.js (ESM)
import { mountRoomBedTypeGrid, openCreate } from "./component.js";

document.addEventListener("DOMContentLoaded", () => {
  mountRoomBedTypeGrid();
  const btn = document.getElementById("btn-roombedtype-new");
  if (btn) btn.addEventListener("click", () => openCreate());
});
