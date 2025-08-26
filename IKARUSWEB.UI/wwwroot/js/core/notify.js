// /wwwroot/js/core/notify.js (ESM) - SweetAlert wrappers
export const toastSuccess = (text) =>
  Swal.fire({ text, icon: "success", confirmButtonText: "OK" });

export const toastError = (text) =>
  Swal.fire({ text, icon: "error", confirmButtonText: "OK" });

export const toastInfo = (text) =>
  Swal.fire({ text, icon: "info", confirmButtonText: "OK" });

export const confirm = async (text) => {
  const res = await Swal.fire({
    text,
    icon: "warning",
    showCancelButton: true,
    confirmButtonText: "OK",
    cancelButtonText: "Cancel"
  });
  return res.isConfirmed === true;
};
