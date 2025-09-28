// /wwwroot/js/core/dxHelpers.js
import http from "./http.js";

function toLoadParams(loadOptions) {
    const params = {};
    const names = [
        "skip", "take",
        "requireTotalCount", "requireGroupCount",
        "sort", "filter",
        "totalSummary", "group", "groupSummary",
        "searchExpr", "searchOperation", "searchValue"
    ];
    for (const n of names) {
        const v = loadOptions[n];
        if (v !== undefined && v !== null) params[n] = (typeof v === "object") ? JSON.stringify(v) : v;
    }
    return params;
}


export function buildDxDataStore(loadUrl, extraParamsBuilder) {
    return new DevExpress.data.CustomStore({
        key: "id",
        load: (loadOptions) => {
            const params = toLoadParams(loadOptions);
            if (typeof extraParamsBuilder === "function") Object.assign(params, extraParamsBuilder());
            return http.get(loadUrl, { params }).then(r => r);
        }
    });
}
    

export function createDxGrid(selector, dataSource, overrides = {}) {
    return $(selector).dxDataGrid({
        dataSource,
        remoteOperations: { paging: true, sorting: true, filtering: true, grouping: true, summary: true },
        columnAutoWidth: true,
        showBorders: true,
        hoverStateEnabled: true,
        paging: { pageSize: 25 },
        pager: { visible: true, showPageSizeSelector: false }, 
        filterRow: { visible: true, applyFilter: "auto" },
        searchPanel: { visible: true },
        selection: { mode: "single" },
        export: {
            enabled: true,
            allowExportSelectedData: false,
        },
        onExporting: (e) => {
            const tableName = selector.split("-")[1];
            const workbook = new ExcelJS.Workbook();
            const worksheet = workbook.addWorksheet(tableName);

            DevExpress.excelExporter.exportDataGrid({
                component: e.component,
                worksheet,
                autoFilterEnabled: true, customizeCell(options) {
                    const { gridCell, excelCell } = options;
                    if (gridCell?.cellType === "data" && gridCell?.column?.dataField === "isActive") {
                        debugger
                        excelCell.value = gridCell.value ? i18n.common["grid.column.bool.true"] : i18n.common["grid.column.bool.false"];
                    }
                }
            }).then(() => {
                const pad = (n) => n.toString().padStart(2, "0");
                const d = new Date();
                const fileName = `${tableName}_${d.getFullYear()}${pad(d.getMonth() + 1)}${pad(d.getDate())}_${pad(d.getHours())}${pad(d.getMinutes())}.xlsx`;

                return workbook.xlsx.writeBuffer().then((buffer) => {
                    saveAs(new Blob([buffer], { type: "application/octet-stream" }), fileName);
                });
            });
            e.cancel = true;
        },
        ...overrides
    }).dxDataGrid("instance");
}

export function createDxTooltip(targetEl, text, extraOpts) {
    if (!targetEl || !text) return;
    const $target = $(targetEl);

    if ($target.attr("title")) {
        $target.attr("data-tooltip", $target.attr("title"));
        $target.removeAttr("title");
    }

    const opts = Object.assign({
        target: $target,
        contentTemplate: () => $("<div/>").text(text),
        showEvent: "mouseenter",
        hideEvent: "mouseleave",
        position: "bottom",
        animation: { show: { type: "fade", duration: 120 }, hide: { type: "fade", duration: 80 } }
    }, extraOpts || {});

    try {
        const old = $target.data("dxTooltip");
        if (old && old.dispose) old.dispose();
    } catch { /* ignore */ }

    const $tooltip = $("<div/>").appendTo(document.body).dxTooltip(opts);
    const instance = $tooltip.dxTooltip("instance");
    $target.data("dxTooltip", instance);
    return instance;
}