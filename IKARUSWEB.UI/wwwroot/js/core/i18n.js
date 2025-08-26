// /wwwroot/js/core/i18n.js (ESM)
export const t = (key) => {
  try { return (window.__R && window.__R(key)) || key; }
  catch { return key; }
};
