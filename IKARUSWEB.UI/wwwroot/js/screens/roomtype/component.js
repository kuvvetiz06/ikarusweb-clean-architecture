// /wwwroot/js/screens/odatip/component.js (ESM)
import { buildServerStore, defaultGrid, buildModalForm } from "../../core/dx-helpers.js";
import { OdaTipCrud } from "./crud.js";
import { OdaTipApi } from "./api.js";
import { openModal } from "../../core/modal.js";
import { t } from "../../core/i18n.js";

let grid, form, modalApi;
let formState = { id: null, name: "", description: "", isActive: true };

const store = buildServerStore("/api/room-types/list", null);

const columns = [
  { dataField: "name", caption: t("Name") },
  { dataField: "description", caption: t("Description") },
  { dataField: "isActive", caption: t("IsActive"), dataType: "boolean", width: 110 },
  {
    caption: t("Actions"), width: 150, alignment: "center",
    cellTemplate: (container, { data }) => {
      $("<div>").appendTo(container)
        .append($("<a>", { href: "javascript:void(0)", text: t("Edit") }).on("click", () => openEdit(data.id)))
        .append(" | ")
        .append($("<a>", { href: "javascript:void(0)", text: t("Delete") }).on("click", async () => {
          const res = await OdaTipCrud.del(data.id);
          if (res.success) grid.refresh();
        }));
    }
  }
];

const toolbar = {
  items: [
    { widget: "dxButton", options: { icon: "add", text: t("New"), onClick: () => openCreate() }, location: "after" },
    "searchPanel"
  ]
};

const formItems = [
  { dataField: "name", label: { text: t("Name") }, colSpan: 12, isRequired: true, editorType: "dxTextBox",
    validationRules: [{ type: "required", message: t("NameRequired") }, { type: "stringLength", min: 2, max: 100 }] },
  { dataField: "description", label: { text: t("Description") }, colSpan: 12, editorType: "dxTextArea",
    editorOptions: { minHeight: 80 } },
  { dataField: "isActive", label: { text: t("IsActive") }, colSpan: 12, editorType: "dxSwitch" }
];

export function mountOdaTipGrid() {
  grid = defaultGrid("#grid-odatip", new DevExpress.data.DataSource({
    store: new DevExpress.data.CustomStore(store)
  }), {
    columns,
    toolbar
  });
}

export function openCreate() {
  formState = { id: null, name: "", description: "", isActive: true };
  showModal(t("CreateRoomType"));
}

async function openEdit(id) {
  const res = await OdaTipApi.getById(id);
  if (res.success && res.data) {
    formState = { ...res.data };
    showModal(t("EditRoomType"));
  }
}

function showModal(title) {
  modalApi = openModal({
    title,
    onBuild: ({ body, submitButton, close }) => {
      body.append('<div id="odatip-form"></div>');
      form = buildModalForm("#odatip-form", formItems, formState, { colCount: 12 });

      submitButton().on("click", async () => {
        const validate = form.validate();
        if (!validate.isValid) return;
        const model = form.option("formData");
        const res = await OdaTipCrud.save(model);
        if (res.success) {
          close();
          grid.refresh();
        }
      });
    }
  });
}
