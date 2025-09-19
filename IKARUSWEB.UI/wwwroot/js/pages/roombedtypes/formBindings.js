// /wwwroot/js/screens/roombedtype/formBindings.js (ESM)
// Explicit, predictable bindings (no auto-scan).
// Server view must include placeholders with fixed IDs: #name, #code, #description (and any others).
export function bindRoomBedTypeForm(rootEl, initial = {}) {
    const $root = $(rootEl);

    const $id = $root.find("#id");
    const id = $id.val(initial.id);

    const $name = $root.find("#name");
    const name = $name.dxTextBox({ value: initial.name ?? "" }).dxTextBox("instance");

    const $code = $root.find("#code");
    const code = $code.dxTextBox({ value: initial.code ?? "" }).dxTextBox("instance");

    const $description = $root.find("#description");
    const description = $description.dxTextArea({ value: initial.description ?? "", minHeight: 80 }).dxTextArea("instance");

    const $isActive = $root.find("#isActive");
    let isActive = null;
    if ($isActive.length) {
        isActive = $isActive.dxCheckBox({ value: !!(initial.isActive ?? true), text: 'Aktif', iconSize: 20 }).dxCheckBox("instance");
    }

    function getValues() {
        return {
            name: name.option("value"),
            code: code.option("value"),
            description: description.option("value"),
            ...(isActive ? { isActive: !!isActive.option("value") } : {})
        };
    }

    function setValues(model) {
        if (model?.name !== undefined) name.option("value", model.name);
        if (model?.code !== undefined) code.option("value", model.code);
        if (model?.description !== undefined) description.option("value", model.description);
        if (isActive && model?.isActive !== undefined) isActive.option("value", !!model.isActive);
    }

    function validate() {
        const n = name.option("value")?.toString().trim();
        if (!n) return { isValid: false, message: "Ad zorunludur." };
        return { isValid: true };
    }

    return { getValues, setValues, validate };
}
