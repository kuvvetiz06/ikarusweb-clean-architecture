// /wwwroot/js/screens/roombedtype/component.js
import { buildDxDataStore, createDefaultGrid } from "../../core/dxHelpers.js";
import { openModal } from "../../core/modal.js";
import { bindRoomBedTypeForm } from "./formBindings.js";
import { roomBedTypeService } from "./service.js";

let grid;
const loadUrl = roomBedTypeService.dataUrl;

export function mountRoomBedTypeGrid() {
    const ds = new DevExpress.data.DataSource({ store: buildDxDataStore(loadUrl) });
    grid = createDefaultGrid("#grid-roombedtype", ds, {
        columns: [
            { dataField: "name", caption: "Ad" },
            { dataField: "code", caption: "Kod" },
            { dataField: "description", caption: "Açıklama" }
        ],
        selection: { mode: "single" },
        onToolbarPreparing: (e) => {
            e.toolbarOptions.items.unshift(
                { location: "before", widget: "dxButton", options: { icon: "add", text: "Yeni", onClick: () => openCreate() } },
                {
                    location: "before", widget: "dxButton", options: {
                        icon: "edit", text: "Düzenle", onClick: () => {
                            const keys = grid.getSelectedRowKeys();
                            if (!keys.length) return Swal.fire({ icon: "info", text: "Lütfen bir satır seçin." });
                            openEdit(keys[0]);
                        }
                    }
                },
                {
                    location: "before", widget: "dxButton", options: {
                        icon: "trash", text: "Sil", onClick: async () => {
                            const keys = grid.getSelectedRowKeys();
                            if (!keys.length) return Swal.fire({ icon: "info", text: "Lütfen bir satır seçin." });
                            const r = await roomBedTypeService.confirmAndDelete(keys[0]);
                            if (r?.success) grid.refresh();
                        }
                    }
                },
              
            );
        }
    });
}

export function openCreate() {
    openModal({
        title: "Oda Yatak Tipi Oluştur",
        url: "/RoomBedType/Add",
        onReady: ({ body, submitButton, close }) => {
            const form = bindRoomBedTypeForm(body[0], {});
            submitButton().off("click").on("click", async () => {
                const res = await roomBedTypeService.save(form.getValues());
                if (res?.success) { close(); grid.refresh(); }
            });
        }
    });
}

async function openEdit(id) {
    const dto = await roomBedTypeService.getById(id);
    const model = dto?.data ?? dto ?? {};
    openModal({
        title: "Oda Yatak Tipi Düzenle",
        url: `/RoomBedType/Edit?id=${encodeURIComponent(id)}`,
        onReady: ({ body, submitButton, close }) => {
            const form = bindRoomBedTypeForm(body[0], model);
            submitButton().off("click").on("click", async () => {
                const res = await roomBedTypeService.save({ id, ...form.getValues() });
                if (res?.success) { close(); grid.refresh(); }
            });
        }
    });
}
