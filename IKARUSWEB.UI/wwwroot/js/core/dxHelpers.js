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

export function attachDxTooltip(targetEl, text, extraOpts) {
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