// /wwwroot/js/core/http.js
const axiosRef = window.axios;
const normalize = (resp) =>
    (resp?.data && typeof resp.data === "object")
        ? resp.data
        : ({ succeeded: true, message: null, data: resp?.data ?? null });

// succeeded ismini standardize ettik
export const http = axiosRef.create({
    baseURL: "/api",
    timeout: 30000,
    headers: { "X-Requested-With": "XMLHttpRequest" },
    withCredentials: true
});

let authRedirectInFlight = false;

http.interceptors.response.use(
    (response) => normalize(response),
    (error) => {
        const status = error?.response?.status;
        const payload = error?.response?.data;
        const message = payload?.message || payload?.title || error?.message || "Beklenmeyen bir hata oluştu.";

        // 401/403 vs → yönlendir (istersen burada da Swal kullanma)
        if ((status === 401 || status === 403 || status === 419 || status === 440) && !authRedirectInFlight) {
            authRedirectInFlight = true;
            const returnUrl = encodeURIComponent(location.pathname + location.search);
            window.location.href = `/account/login?returnUrl=${returnUrl}`;
        }

        // Sadece anlamlı bir obje döndür: service.js karar versin
        return Promise.resolve({
            succeeded: false,
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