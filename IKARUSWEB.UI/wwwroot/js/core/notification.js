
export function collectErrors(errorsObj) {
    return Object.entries(errorsObj || {})
        .flatMap(([k, arr]) => (arr || []).map((m) => `${k}: ${m}`));
}

// Tüm sayfalar için tek karar noktası
export async function showResult(res, opts = {}) {
    const { successText = "Başarılı", errorText = "Hata", onOk } = opts;
    const lines = collectErrors(res?.errors);

    if (lines.length) {
        await Swal.fire({
            icon: "error",
            html: `<pre style="text-align:left;white-space:pre-wrap;margin:0">${lines.join("\n")}</pre>`
        });
        return false; // başarısız
    }

    await Swal.fire({
        icon: res?.succeeded ? "success" : "error",
        text: res?.message || (res?.succeeded ? successText : errorText)
    });

    if (res?.succeeded && typeof onOk === "function") onOk(res);
    return !!res?.succeeded;
}
