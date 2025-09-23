// /wwwroot/js/core/http.js (ESM)
// Backend (YARP + Session) injects JWT. No client-side auth headers.
// Centralized SweetAlert + 401 refresh-retry.

const axiosRef = window.axios;
if (!axiosRef) console.error("Axios missing");

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

let authRedirectInFlight = false;

http.interceptors.response.use(
    (response) => normalize(response),
    (error) => {
        const status = error?.response?.status;
        const payload = error?.response?.data;
        const message = payload?.message || payload?.title || error?.message || "Beklenmeyen bir hata oluştu.";

        // Session bitti / yetki yok → sadece yönlendir
        if ((status === 401 || status === 403 || status === 419 || status === 440) && !authRedirectInFlight) {
            authRedirectInFlight = true;
            const returnUrl = encodeURIComponent(location.pathname + location.search);
            // İstersen Swal göstermeden direkt yönlendirebilirsin:
            Swal.fire({ icon: "info", text: i18n.common["swal.session.expired"] })
                .then(() => { window.location.href = `/account/login?returnUrl=${returnUrl}`; });
            // NOT: refresh token denemesi yapmıyoruz.
            return Promise.resolve({ success: false, message, data: null, status });
        }

        // 400 ModelState
        if (status === 400 && payload?.errors) {
            const lines = Object.entries(payload.errors)
                .flatMap(([k, arr]) => (arr || []).map((m) => `${k}: ${m}`));
            Swal.fire({ icon: "error", html: `<pre style="text-align:left;white-space:pre-wrap">${lines.join("\n")}</pre>` });
        } else {
            Swal.fire({ icon: "error", text: message });
        }

        return Promise.resolve({
            success: false,
            message,
            data: null,
            errors: payload?.errors ?? null,
            status
        });
    }
);


export const get = (url, params = {}, config = {}) => http.get(url, { params, ...config }).then(r => r);
export const post = (url, data = {}, config = {}) => http.post(url, data, config).then(r => r);
export const put = (url, data = {}, config = {}) => http.put(url, data, config).then(r => r);
export const del = (url, config = {}) => http.delete(url, config).then(r => r);

export default http;
