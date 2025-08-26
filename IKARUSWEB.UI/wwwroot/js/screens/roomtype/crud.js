// /wwwroot/js/screens/odatip/crud.js (ESM)
import { OdaTipApi } from "./api.js";
import { toastSuccess, confirm } from "../../core/notify.js";
import { t } from "../../core/i18n.js";

export const OdaTipCrud = {
  async save(model) {
    const res = model.id ? await OdaTipApi.update(model.id, model) : await OdaTipApi.create(model);
    if (res.success) toastSuccess(t("SavedSuccessfully"));
    return res;
  },
  async del(id) {
    if (!(await confirm(t("AreYouSureToDelete")))) return { success: false, message: "cancelled", data: null };
    const res = await OdaTipApi.remove(id);
    if (res.success) toastSuccess(t("DeletedSuccessfully"));
    return res;
  }
};
