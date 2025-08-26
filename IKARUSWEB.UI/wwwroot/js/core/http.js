// /wwwroot/js/core/http.js (ESM)
// NOTE: JWT/Tenant/UserID/Claims are injected on the backend (YARP + Session).
// We DO NOT add headers here. Centralized error handling only.

import { toastError } from "./notify.js";
import { t } from "./i18n.js";

const axiosRef = window.axios;
if (!axiosRef) {
  console.error("axios not found. Add https://cdn.jsdelivr.net/npm/axios/dist/axios.min.js before this file.");
}

// Normalize API responses into: { success, message, data, errors?, status? }
const normalize = (resp) => {
  if (resp?.data && typeof resp.data === "object") return resp.data;
  return { success: true, message: null, data: resp?.data ?? null };
};

export const http = axiosRef.create({
  baseURL: "/api",
  timeout: 30000,
  headers: { "X-Requested-With": "XMLHttpRequest" }
});

http.interceptors.request.use(
  (config) => config,
  (error) => Promise.reject(error)
);

http.interceptors.response.use(
  (response) => normalize(response),
  (error) => {
    const status = error?.response?.status;
    const payload = error?.response?.data;
    const message =
      payload?.message ||
      payload?.title ||
      error?.message ||
      t("UnexpectedError");

    if (status === 400 && payload?.errors) {
      const lines = Object.entries(payload.errors).flatMap(([k, arr]) =>
        (arr || []).map((m) => `${k}: ${m}`)
      );
      toastError(lines.join("\n"));
    } else {
      toastError(message);
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

export const get  = (url, params = {}, config = {}) => http.get(url, { params, ...config }).then(r => r);
export const post = (url, data = {}, config = {}) => http.post(url, data, config).then(r => r);
export const put  = (url, data = {}, config = {}) => http.put(url, data, config).then(r => r);
export const del  = (url, config = {})          => http.delete(url, config).then(r => r);

export default http;
