// /wwwroot/js/core/utils.js (ESM)
export const isoToLocal = (iso) => {
  if (!iso) return "";
  const d = new Date(iso);
  if (Number.isNaN(d.getTime())) return "";
  return d.toLocaleString("tr-TR");
};

export const toISODate = (d) => {
  if (!d) return null;
  const dt = typeof d === "string" ? new Date(d) : d;
  return dt.toISOString();
};
