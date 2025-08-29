// /wwwroot/js/screens/roombedtype/service.js (ESM)
// api + crud + notify in one place. Messages come from API result.message.
import { get, post, put, del } from "../../core/http.js";

const base = "roombedtypes";

export const roomBedTypeService = {
    dataUrl: `${base}/data`,
    getById: (id) => get(`${base}/${id}`),
    create: (payload) => post(`${base}`, payload),
    update: (id, payload) => put(`${base}/${id}`, payload),
    remove: (id) => del(`${base}/${id}`),

    async save(model) {
        const isUpdate = !!model.id;
        const res = isUpdate ? await this.update(model.id, model) : await this.create(model);
        debugger
        Swal.fire({ icon: res?.success ? "success" : "error", text: res?.message || (res?.success ? "Başarılı" : "Hata") });
        return res;
    },

    async confirmAndDelete(id) {
        const ok = (await Swal.fire({ icon: "warning", text: "Silmek istediğinize emin misiniz?", showCancelButton: true })).isConfirmed;
        if (!ok) return { success: false, message: "cancelled" };
        const res = await this.remove(id);
        Swal.fire({ icon: res?.success ? "success" : "error", text: res?.message || (res?.success ? "Silindi" : "Hata") });
        return res;
    }
};
