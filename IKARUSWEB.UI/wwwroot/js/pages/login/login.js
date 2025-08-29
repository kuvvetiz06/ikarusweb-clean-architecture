"use strict";
var SigninGeneral = (function () {
    var m, t, e, r, f;


    const signIn = async () => {
        f = $('#Sign_in_form');

        try {
            const r = await axios.post(
                '/api/auth/login',
                f.serializeJSON(),
                {
                    headers: {
                        'Content-Type': 'application/json',
                        'Accept': 'application/json'
                    },
                    withCredentials: true
                }
            );

            if (r.status === 200 || r.status === 204) {
                location.href = '/Home/Index';
                return;

            } else {
                e.removeAttribute("data-kt-indicator");
                e.disabled = false;
                Swal.fire({
                    text: "Login Unsuccessful",
                    icon: "error",
                    buttonsStyling: false,
                    confirmButtonText: "Ok",
                    customClass: { confirmButton: "btn btn-primary" },
                });
            }
        } catch (error) {
            e.removeAttribute("data-kt-indicator");
            e.disabled = false;

            Swal.fire({
                text: error.response?.data?.message || "Login Unsuccessful",
                icon: "error",
            });
        }


    };


    return {
        init: function () {
            (t = document.querySelector("#Sign_in_form")),
                (e = document.querySelector("#Sign_in_submit")),
                (r = FormValidation.formValidation(t, {
                    fields: {
                        UserName: {
                            validators: {
                                notEmpty: { message: "UserName Is Required" }
                            }
                        },
                        Password: {
                            validators: {
                                notEmpty: { message: "Password Is Required" }
                            }
                        },
                        TenantCode: {
                            validators: {
                                notEmpty: { message: "Tenant Code Is Required" }
                            }
                        },
                    },
                    plugins: {
                        trigger: new FormValidation.plugins.Trigger(),
                        bootstrap: new FormValidation.plugins.Bootstrap5({
                            rowSelector: ".fv-row",
                            eleInvalidClass: "is-invalid",
                            eleValidClass: "is-valid"
                        }),
                    },
                })),

                !(function (t) {
                    try {
                        return new URL(t), !0;
                    } catch (t) {
                        return !1;
                    }
                })(e.closest("form").getAttribute("action"));

            e.addEventListener("click", function (i) {
                i.preventDefault();
                r.validate().then(function (r) {
                    if (r === "Valid") {
                        e.setAttribute("data-kt-indicator", "on");
                        e.disabled = true;
                        signIn();
                    } else {
                        Swal.fire({
                            text: "Errors Detected",
                            icon: "error",
                            buttonsStyling: false,
                            confirmButtonText: "Ok",
                            customClass: {
                                confirmButton: "btn btn-primary"
                            },
                        });
                    }
                });
            });
        }
    };
})();
KTUtil.onDOMContentLoaded(function () {
    SigninGeneral.init();
});
