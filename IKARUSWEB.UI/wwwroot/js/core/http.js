// /wwwroot/js/core/http.js (ESM)
// Backend (YARP + Session) injects JWT/Tenant/UserID/Claims. No client-side headers.
// Centralized SweetAlert error handling.

const axiosRef = window.axios;
if (!axiosRef) console.error("axios missing. Load https://cdn.jsdelivr.net/npm/axios/dist/axios.min.js");

const normalize = (resp) => (resp?.data && typeof resp.data === "object") ? resp.data : ({ success: true, message: null, data: resp?.data ?? null });

export const http = axiosRef.create({
  baseURL: "/api",
  timeout: 30000,
  headers: { "X-Requested-With": "XMLHttpRequest" }
});

http.interceptors.response.use(
  (response) => normalize(response),
  (error) => {
    const status = error?.response?.status;
    const payload = error?.response?.data;
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

export const get  = (url, params = {}, config = {}) => http.get(url, { params, ...config }).then(r => r);
export const post = (url, data = {}, config = {}) => http.post(url, data, config).then(r => r);
export const put  = (url, data = {}, config = {}) => http.put(url, data, config).then(r => r);
export const del  = (url, config = {})          => http.delete(url, config).then(r => r);

export default http;
