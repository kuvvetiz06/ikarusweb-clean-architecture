// /wwwroot/js/core/dx-helpers.js (ESM)
import { t } from "./i18n.js";

export const buildServerStore = (loadUrl, extraParamsBuilder) => ({
  key: "id",
  load: (loadOptions) => {
    const params = {
      skip: loadOptions.skip ?? 0,
      take: loadOptions.take ?? 25,
      sort: JSON.stringify(loadOptions.sort || []),
      filter: JSON.stringify(loadOptions.filter || []),
      searchExpr: loadOptions.searchExpr || null,
      searchValue: loadOptions.searchValue || null
    };
    if (extraParamsBuilder) Object.assign(params, extraParamsBuilder());
    return $.getJSON(loadUrl, params).then((r) => {
      return { data: r.data ?? r.items ?? [], totalCount: r.totalCount ?? r.total ?? 0 };
    });
  }
});

export const defaultGrid = (element, dataSource, overrides = {}) => {
  return $(element).dxDataGrid({
    dataSource,
    remoteOperations: { paging: true, sorting: true, filtering: true, grouping: true },
    columnAutoWidth: true,
    wordWrapEnabled: true,
    showBorders: true,
    hoverStateEnabled: true,
    pager: { showPageSizeSelector: true, allowedPageSizes: [25, 50, 100], visible: true },
    paging: { pageSize: 25 },
    filterRow: { visible: true, applyFilter: "auto" },
    searchPanel: { visible: true, highlightCaseSensitive: false },
    groupPanel: { visible: false },
    rowAlternationEnabled: true,
    ...overrides
  }).dxDataGrid("instance");
};

export const buildModalForm = (container, formItems, formData = {}, overrides = {}) => {
  return $(container).dxForm({
    formData,
    colCount: 12,
    labelLocation: "top",
    items: formItems,
    ...overrides
  }).dxForm("instance");
};
