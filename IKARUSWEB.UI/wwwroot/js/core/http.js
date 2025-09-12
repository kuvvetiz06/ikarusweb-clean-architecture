// /wwwroot/js/core/http.js (ESM)
// Backend (YARP + Session) injects JWT. No client-side auth headers.
// Centralized SweetAlert + 401 refresh-retry.

const axiosRef = window.axios;
if (!axiosRef) console.error("axios missing. Load https://cdn.jsdelivr.net/npm/axios/dist/axios.min.js");

const normalize = (resp) =>
    (resp?.data && typeof resp.data === "object")
        ? resp.data
        : ({ success: true, message: null, data: resp?.data ?? null });

export const http = axiosRef.create({
    baseURL: "/api",
    timeout: 30000,
    headers: { "X-Requested-With": "XMLHttpRequest" },
    withCredentials: true // Session + UI cookie her istekte taşınsın
});

// --- 401 → refresh → retry --- //
let isRefreshing = false;
let waitQueue = [];

function queueWait() {
    return new Promise(resolve => waitQueue.push(resolve));
}
function flushQueue() {
    waitQueue.forEach(res => res());
    waitQueue = [];
}

http.interceptors.response.use(
    (response) => normalize(response),
    async (error) => {
        const cfg = error?.config || {};
        const url = (cfg.url || "");
        const status = error?.response?.status;
        const payload = error?.response?.data;

        // Bu istek login/refresh değilse ve 401 geldiyse: otomatik refresh
        const isAuthPath = url.startsWith("/auth/login") || url.startsWith("/auth/refresh");
        if (status === 401 && !isAuthPath && !cfg.__retry) {
            try {
                if (isRefreshing) {
                    await queueWait();                // başka refresh bitene kadar bekle
                } else {
                    isRefreshing = true;
                    await axiosRef.post("/api/auth/refresh", {}, { withCredentials: true });
                    isRefreshing = false;
                    flushQueue();                     // bekleyenleri uyandır
                }
                cfg.__retry = true;                 // tek seferlik retry guard
                return http.request(cfg);           // orijinal isteği tekrar dene
            } catch (e) {
                isRefreshing = false;
                waitQueue = [];
                try { await fetch("/api/auth/logout", { method: "POST", credentials: "same-origin" }); } catch { }
                window.location.href = "/account/login";
                return Promise.resolve({ success: false, message: "Oturum süresi doldu.", data: null, status: 401 });
            }
        }

        // Diğer hata akışı (mevcut davranış korunur)
        const message = payload?.message || payload?.title || error?.message || "Beklenmeyen bir hata oluştu.";
        if (status === 400 && payload?.errors) {
            const lines = Object.entries(payload.errors).flatMap(([k, arr]) => (arr || []).map((m) => `${k}: ${m}`));
            Swal.fire({ icon: "error", html: `<pre style="text-align:left;white-space:pre-wrap">${lines.join("\n")}</pre>` });
        } else {
            Swal.fire({ icon: "error", text: message });
        }
        return Promise.resolve({ success: false, message, data: null, errors: payload?.errors ?? null, status });
    }
);

export const get = (url, params = {}, config = {}) => http.get(url, { params, ...config }).then(r => r);
export const post = (url, data = {}, config = {}) => http.post(url, data, config).then(r => r);
export const put = (url, data = {}, config = {}) => http.put(url, data, config).then(r => r);
export const del = (url, config = {}) => http.delete(url, config).then(r => r);

export default http;
