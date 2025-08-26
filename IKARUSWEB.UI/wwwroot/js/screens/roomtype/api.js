// /wwwroot/js/screens/odatip/api.js (ESM)
import { http } from "../../core/http.js";

const BASE = "/api/room-types"; // Adjust to your API route

export const OdaTipApi = {
  create: (payload) => http.post(`${BASE}`, payload),
  update: (id, payload) => http.put(`${BASE}/${id}`, payload),
  remove: (id) => http.delete(`${BASE}/${id}`),
  getById: (id) => http.get(`${BASE}/${id}`)
};
