// /wwwroot/js/screens/roombedtype/component.js
import { buildDxDataStore, createDxGrid, createDxTooltip } from "../../core/dxHelpers.js";
import { openModal } from "../../core/modal.js";
import { bindRoomBedTypeForm } from "./formBindings.js";
import { createService } from "../../core/serviceFactory.js";

const roomBedTypeService = createService("roombedtypes");

let grid;
const loadUrl = roomBedTypeService.dataUrl;

export function roomBedTypeGrid() {
    const ds = new DevExpress.data.DataSource({ store: buildDxDataStore(loadUrl) });
    grid = createDxGrid("#grid-roombedtypes", ds, {
        columns: [
            { dataField: "name", caption: i18n.roombedtypes["grid.column.name"] },
            { dataField: "code", caption: i18n.roombedtypes["grid.column.code"] },
            { dataField: "description", caption: i18n.roombedtypes["grid.column.description"] },
            { dataField: "isActive", caption: i18n.roombedtypes["grid.column.isactive"], dataType: "boolean",
                customizeText: (cellInfo) => cellInfo.value ? i18n.common["grid.column.bool.true"] : i18n.common["grid.column.bool.false"]
            }
        ],        
        onToolbarPreparing: (e) => {


            e.toolbarOptions.items.unshift(
                {

                    location: "after",
                    widget: "dxButton",
                    options: {
                        icon: "add",
                        text: "",
                        onClick: () => openCreate(),
                        onInitialized: (args) => {
                            createDxTooltip(
                                args.element,
                                (i18n.common?.["btn.add"])
                            );
                        }
                    }
                },

                {
                    location: "after",
                    widget: "dxButton",
                    options: {
                        icon: "edit",
                        text: "",
                        onClick: () => {
                            const keys = grid.getSelectedRowKeys();
                            if (!keys.length) return Swal.fire({ icon: "info", text: i18n.common["swal.grid.please.select.row"] });
                            openEdit(keys[0]);
                        },
                        onInitialized: (args) => {
                            createDxTooltip(
                                args.element,
                                (i18n.common?.["btn.edit"])
                            );
                        }
                    }
                },
                {
                    location: "after",
                    widget: "dxButton",
                    options: {
                        icon: "trash",
                        text: "",
                        onClick: async () => {
                            const keys = grid.getSelectedRowKeys();
                            if (!keys.length) return Swal.fire({ icon: "info", text: i18n.common["swal.grid.please.select.row"] });
                            const r = await roomBedTypeService.confirmAndDelete(keys[0]);
                            if (r?.succeeded) grid.refresh();
                        },
                        onInitialized: (args) => {
                            createDxTooltip(
                                args.element,
                                (i18n.common?.["btn.remove"])
                            );
                        }
                    }
                },

            );
        }
    });
}

export function openCreate() {
    openModal({
        title: i18n.common["modal.title.add"],
        url: "/roombedtypes/add",
        onReady: ({ body, submitButton, close }) => {
            const form = bindRoomBedTypeForm(body[0], {});
            submitButton().off("click").on("click", async () => {
                const res = await roomBedTypeService.save(form.getValues());
                if (res?.succeeded) { close(); grid.refresh(); }
            });
        }
    });
}

async function openEdit(id) {
    const dto = await roomBedTypeService.getById(id);
    const model = dto?.data ?? dto ?? {};
    openModal({
        title: i18n.common["modal.title.edit"],
        url: `/roombedtypes/edit?id=${encodeURIComponent(id)}`,
        onReady: ({ body, submitButton, close }) => {
            const form = bindRoomBedTypeForm(body[0], model);
            submitButton().off("click").on("click", async () => {
                const res = await roomBedTypeService.save({ id, ...form.getValues() });
                if (res?.succeeded) { close(); grid.refresh(); }
            });
        }
    });
}
