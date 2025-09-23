// /wwwroot/js/screens/roombedtype/component.js
import { buildDxDataStore, createDefaultGrid, attachDxTooltip } from "../../core/dxHelpers.js";
import { openModal } from "../../core/modal.js";
import { bindRoomBedTypeForm } from "./formBindings.js";
import { roomBedTypeService } from "./service.js";

let grid;
const loadUrl = roomBedTypeService.dataUrl;

export function roomBedTypeGrid() {
    const yes = i18n.common["grid.column.bool.true"] ?? "Yes";
    const no = i18n.common["grid.column.bool.false"] ?? "No";
    const ds = new DevExpress.data.DataSource({ store: buildDxDataStore(loadUrl) });
    grid = createDefaultGrid("#grid-roombedtypes", ds, {
        columns: [
            { dataField: "name", caption: i18n.roombedtypes["grid.column.name"] },
            { dataField: "code", caption: i18n.roombedtypes["grid.column.code"] },
            { dataField: "description", caption: i18n.roombedtypes["grid.column.description"] },
            {
                dataField: "isActive", caption: i18n.roombedtypes["grid.column.isactive"], dataType: "boolean",
                customizeText: (cellInfo) => cellInfo.value ? yes : no
            }
        ],
        selection: { mode: "single" },
        export: {
            enabled: true,
            allowExportSelectedData: false,
        },
        onExporting: (e) => {
            const workbook = new ExcelJS.Workbook();
            const worksheet = workbook.addWorksheet("roombedtypes");

            DevExpress.excelExporter.exportDataGrid({
                component: e.component,
                worksheet,
                autoFilterEnabled: true, customizeCell(options) {
                    const { gridCell, excelCell } = options;
                    if (gridCell?.cellType === "data" && gridCell?.column?.dataField === "isActive") {
                        excelCell.value = gridCell.value ? yes : no;
                    }
                }
            }).then(() => {
                const pad = (n) => n.toString().padStart(2, "0");
                const d = new Date();
                const fileName = `roombedtypes_${d.getFullYear()}${pad(d.getMonth() + 1)}${pad(d.getDate())}_${pad(d.getHours())}${pad(d.getMinutes())}.xlsx`;

                return workbook.xlsx.writeBuffer().then((buffer) => {
                    saveAs(new Blob([buffer], { type: "application/octet-stream" }), fileName);
                });
            });
            e.cancel = true;
        },

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
                            attachDxTooltip(
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
                            attachDxTooltip(
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
                            if (r?.success) grid.refresh();
                        },
                        onInitialized: (args) => {
                            attachDxTooltip(
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
                if (res?.success) { close(); grid.refresh(); }
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
                if (res?.success) { close(); grid.refresh(); }
            });
        }
    });
}
