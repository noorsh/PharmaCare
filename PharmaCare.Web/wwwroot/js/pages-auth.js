/**
 * Page Authentication
 */

'use strict';

const formAuthentication = document.querySelector('#formAuthentication');

document.addEventListener('DOMContentLoaded', function (e) {
    (function () {
        // Form validation for login
        if (formAuthentication) {
            const fv = FormValidation.formValidation(formAuthentication, {
                fields: {
                    email: {
                        validators: {
                            notEmpty: {
                                message: 'Please enter your email'
                            },
                            emailAddress: {
                                message: 'Please enter valid email address'
                            }
                        }
                    },
                    password: {
                        validators: {
                            notEmpty: {
                                message: 'Please enter your password'
                            }
                        }
                    }
                },
                plugins: {
                    trigger: new FormValidation.plugins.Trigger(),
                    bootstrap5: new FormValidation.plugins.Bootstrap5({
                        eleValidClass: '',
                        rowSelector: '.mb-3'
                    }),
                    submitButton: new FormValidation.plugins.SubmitButton(),
                    autoFocus: new FormValidation.plugins.AutoFocus()
                }
            });
        }

        // Password toggle
        const passwordToggles = document.querySelectorAll('.form-password-toggle');
        if (passwordToggles) {
            passwordToggles.forEach(toggle => {
                const input = toggle.querySelector('input');
                const icon = toggle.querySelector('.input-group-text i');

                if (icon) {
                    icon.parentElement.addEventListener('click', e => {
                        e.preventDefault();
                        if (input.type === 'password') {
                            input.type = 'text';
                            icon.classList.remove('bx-hide');
                            icon.classList.add('bx-show');
                        } else {
                            input.type = 'password';
                            icon.classList.remove('bx-show');
                            icon.classList.add('bx-hide');
                        }
                    });
                }
            });
        }
    })();
});