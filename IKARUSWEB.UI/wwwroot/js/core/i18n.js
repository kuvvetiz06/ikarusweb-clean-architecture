export function t(key, vars) {
    debugger
    const dict = (window.i18n && window.i18n.dict) || {};
    let s = dict[key] || key;               // fallback: key
    if (!vars) return s;
    // basit değişken yerleştirme: "Merhaba {name}"
    Object.entries(vars).forEach(([k, v]) => {
        s = s.replaceAll(`{${k}}`, String(v));
    });
    return s;
}
