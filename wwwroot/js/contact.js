$(function () {
    window.signalRHandler = new SignalRHandler();
    window.contactManager = new ContactManager(window.signalRHandler);
    window.contactEditor = new ContactEditor(window.signalRHandler);

    window.signalRHandler.start().then(() => {
        window.contactEditor.setupEditableFields();
        setupDeleteHandlers();
        setupAddContactForm();     
        const initialSearch = $("#searchInput").val();
        window.contactManager.loadContacts(initialSearch, 1);
    }).catch(error => {
        console.error("Error starting SignalR:", error);
    });

    function setupDeleteHandlers() {
        $(document).on('click', '.delete-contact', function () {
            const $button = $(this);
            const contactId = $button.closest("tr").data("contact-id");
            const contactName = $button.closest("tr").find('[data-field="name"]').text().trim();
            
            if (!contactId) {
                Swal.fire({
                    title: 'Error',
                    text: 'Invalid contact ID',
                    icon: 'error',
                    confirmButtonText: 'OK'
                });
                return;
            }
            
            Swal.fire({
                title: 'Delete Contact',
                text: `Are you sure you want to delete ${contactName}?`,
                icon: 'warning',
                showCancelButton: true,
                confirmButtonColor: '#d33',
                cancelButtonColor: '#3085d6',
                confirmButtonText: 'Yes, delete it!',
                cancelButtonText: 'Cancel'
            }).then((result) => {
                if (result.isConfirmed) {
                    $.ajax({
                        url: `/api/Contact/${contactId}`,
                        method: 'DELETE',
                        success: () => {
                            Swal.fire({
                                title: 'Deleted!',
                                text: 'Contact has been deleted.',
                                icon: 'success',
                                timer: 1500,
                                showConfirmButton: false
                            }).then(() => {
                                window.contactManager.loadContacts();
                            });
                        },
                        error: (xhr) => {
                            Swal.fire({
                                title: 'Error',
                                text: xhr.responseJSON?.message || 'Failed to delete contact',
                                icon: 'error',
                                confirmButtonText: 'OK'
                            });
                        }
                    });
                }
            });
        });
    }

    function setupAddContactForm() {
        const $addContactForm = $("#addContactForm");
        const $saveButton = $("#saveContact");
        const $addContactModal = $("#addContactModal");

        // Form validation
        $addContactForm.validate({
            rules: {
                name: { required: true, maxlength: 50 },
                phone: { 
                    required: true, 
                    minlength: 11, 
                    maxlength: 11, 
                    digits: true
                },
                address: { required: true, maxlength: 200 },
                notes: { maxlength: 500 }
            },
            messages: {
                name: { required: "Name is required", maxlength: "Name cannot exceed 50 characters" },
                phone: { 
                    required: "Phone is required", 
                    minlength: "Phone number must be exactly 11 digits", 
                    maxlength: "Phone number must be exactly 11 digits", 
                    digits: "Phone number must contain only digits"
                },
                address: { required: "Address is required", maxlength: "Address cannot exceed 200 characters" },
                notes: { maxlength: "Notes cannot exceed 500 characters" }
            },
            errorElement: "div",
            errorClass: "invalid-feedback",
            validClass: "valid-feedback",
            highlight: function(element) {
                $(element).addClass("is-invalid").removeClass("is-valid");
                updateSaveButtonState();
            },
            unhighlight: function(element) {
                $(element).removeClass("is-invalid").addClass("is-valid");
                updateSaveButtonState();
            },
            errorPlacement: function(error, element) {
                error.insertAfter(element);
            },
            onfocusout: function(element) {
                if ($(element).val() !== '') {
                    this.element(element);
                }
            },
            onkeyup: function(element) {
                this.element(element);
                updateSaveButtonState();
            }
        });

        function updateSaveButtonState() {
            const name = $("#name").val().trim();
            const phone = $("#phone").val().trim();
            const address = $("#address").val().trim();
            
            const hasName = name.length > 0;
            const hasPhone = phone.length === 11 && /^\d+$/.test(phone);
            const hasAddress = address.length > 0;
            
            const isValid = hasName && hasPhone && hasAddress;
            $saveButton.prop('disabled', !isValid);
        }

        $saveButton.on("click", () => {
            if ($addContactForm.valid()) {
                const formData = {
                    name: $("#name").val(),
                    phone: $("#phone").val(),
                    address: $("#address").val(),
                    notes: $("#notes").val()
                };

                $.ajax({
                    url: "/api/Contact",
                    method: "POST",
                    contentType: "application/json",
                    data: JSON.stringify(formData),
                    success: (response) => {
                        $addContactModal.modal("hide");
                        $addContactForm[0].reset();
                        
                        Swal.fire({
                            title: 'Success!',
                            text: 'Contact has been added.',
                            icon: 'success',
                            timer: 1500,
                            showConfirmButton: false
                        }).then(() => {
                            window.contactManager.loadContacts();
                        });
                    },
                    error: (xhr) => {
                        const errors = xhr.responseJSON;
                        if (errors) {
                            Object.keys(errors).forEach(key => {
                                const input = $(`#${key.toLowerCase()}`);
                                input.addClass("is-invalid");
                                input.siblings(".invalid-feedback").text(errors[key][0]);
                            });
                        }
                    }
                });
            }
        });

        $addContactModal.on("hidden.bs.modal", () => {
            $addContactForm[0].reset();
            $addContactForm.find(".is-invalid").removeClass("is-invalid");
            $addContactForm.find(".invalid-feedback").empty();
            $saveButton.prop('disabled', true);
            $("#addContactBtn").focus();
        });

        $addContactModal.on("show.bs.modal", () => {
            $addContactForm[0].reset();
            $addContactForm.find(".is-invalid").removeClass("is-invalid");
            $addContactForm.find(".invalid-feedback").empty();
            $saveButton.prop('disabled', true);
            setTimeout(() => $("#name").focus(), 500);
        });

        updateSaveButtonState();
    }
});