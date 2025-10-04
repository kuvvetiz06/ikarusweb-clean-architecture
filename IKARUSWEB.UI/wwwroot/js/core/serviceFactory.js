// /wwwroot/js/core/serviceFactory.js
import { get, post, put, del } from "./http.js";
import { showResult } from "./notification.js";

export function createService(baseUrl) {
    const base = baseUrl.replace(/^\/+|\/+$/g, ""); // trim slashes

    return {
        dataUrl: `${base}/data`,
        getById: (id) => get(`${base}/${id}`),
        create: (payload) => post(`${base}`, payload),
        update: (id, payload) => put(`${base}/${id}`, payload),
        remove: (id) => del(`${base}/${id}`),

        // Ortak save (create/update)
        async save(model, texts = {}) {
            const isUpdate = !!model.id;
            const res = isUpdate
                ? await this.update(model.id, model)
                : await this.create(model);

            await showResult(res, {
                successText: isUpdate ? (texts.updateOk || "Güncellendi") : (texts.createOk || "Kaydedildi"),
                errorText: texts.error || "Hata"
            });

            return res;
        },

        // Ortak confirm + delete
        async confirmAndDelete(id, texts = {}) {
            const ok = (await Swal.fire({
                icon: "warning",
                text: texts.confirm || "Silmek istediğinize emin misiniz?",
                showCancelButton: true
            })).isConfirmed;
            if (!ok) return { succeeded: false, message: "cancelled" };

            const res = await this.remove(id);
            await showResult(res, {
                successText: texts.deleteOk || "Silindi",
                errorText: texts.error || "Hata"
            });

            return res;
        }
    };
}
