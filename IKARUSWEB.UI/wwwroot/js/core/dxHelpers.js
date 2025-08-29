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
function debugLog(loadOptions, params, url) {
    if (!window.DEBUG_DX) return;
    console.groupCollapsed("[DX] load →", url);
    try { console.log("loadOptions:", JSON.parse(JSON.stringify(loadOptions))); } catch { }
    console.log("query params:", params);
    console.groupEnd();
}

export function buildDxDataStore(loadUrl, extraParamsBuilder) {
    return new DevExpress.data.CustomStore({
        key: "id",
        load: (loadOptions) => {
            const params = toLoadParams(loadOptions);
            if (typeof extraParamsBuilder === "function") Object.assign(params, extraParamsBuilder());
            debugLog(loadOptions, params, loadUrl);
            return http.get(loadUrl, { params }).then(r => r);
        }
    });
}

export function createDefaultGrid(selector, dataSource, overrides = {}) {
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
        ...overrides
    }).dxDataGrid("instance");
}