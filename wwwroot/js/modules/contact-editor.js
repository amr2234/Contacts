class ContactEditor {
    constructor(signalRHandler) {
        this.signalRHandler = signalRHandler;
        this.lockedContacts = new Map();
        this.validationConfig = {
            rules: {
                name: { required: true, maxlength: 50 },
                phone: { required: true, minlength: 11, maxlength: 11, digits: true },
                address: { required: true, maxlength: 200 },
                notes: { maxlength: 500 }
            },
            messages: {
                name: { required: "Name is required", maxlength: "Name cannot exceed 50 characters" },
                phone: { required: "Phone is required", minlength: "Phone number must be exactly 11 digits", maxlength: "Phone number must be exactly 11 digits", digits: "Phone number must contain only digits" },
                address: { required: "Address is required", maxlength: "Address cannot exceed 200 characters" },
                notes: { maxlength: "Notes cannot exceed 500 characters" }
            }
        };

        this.initSignalRHandlers();
    }

    initSignalRHandlers() {
        this.signalRHandler.onLockUpdate = (contactId, isLocked, lockedBy) => {
            this.handleLockUpdate(contactId, isLocked, lockedBy);
        };

        this.signalRHandler.onContactUpdate = (contactId, field, value) => {
            const $row = $(`tr[data-contact-id="${contactId}"]`);
            const $cell = $row.find(`[data-field="${field}"]`);
            if ($cell.length && !$cell.hasClass('locked-by-me')) {
                $cell.text(value);
            }
        };

        this.signalRHandler.onError = (message) => {
            this.showNotification('Error', message, 'error');
        };
    }

    setupEditableFields() {
        $(".editable").off('click');
        
        $(document).on('click', '.editable', (e) => {
            this.handleEditableClick(e);
        });
    }

    async handleEditableClick(e) {
        const $cell = $(e.target);
        
        if ($cell.find('input, textarea').length > 0) {
            return;
        }
        
        const currentValue = $cell.text().trim();
        const field = $cell.data("field");
        const contactId = $cell.closest("tr").data("contact-id");

        if (!contactId) return;

        if (this.lockedContacts.has(contactId)) {
            const lockInfo = this.lockedContacts.get(contactId);
            if (lockInfo.connectionId === this.signalRHandler.connectionId) {
                $cell.addClass("locked-by-me");
                this.createInputField($cell, field, currentValue, contactId);
            } else {
                this.showNotification('Contact Locked', 'This contact is currently being edited by another user. Please try again later.', 'warning');
            }
            return;
        }

        $('#inlineErrorAlert').addClass('d-none');
        
        await this.signalRHandler.lockContact(contactId);
        this.lockedContacts.set(contactId, { connectionId: this.signalRHandler.connectionId });
        $cell.addClass("locked-by-me");
        this.createInputField($cell, field, currentValue, contactId);
    }

    createInputField($cell, field, currentValue, contactId) {
        const isTextarea = field === "notes";
        const $input = isTextarea
            ? $("<textarea>", { class: "form-control", name: field }).text(currentValue)
            : $("<input>", { type: "text", class: "form-control", value: currentValue, name: field });

        if (field === "phone") {
            this.setupPhoneValidation($input);
        }

        $cell.html($input);
        $input.focus();

        this.setupInputHandlers($input, $cell, currentValue, field, contactId);
    }

    setupPhoneValidation($input) {
        const validateDigitOnly = (e) => {
            if (e.which < 48 || e.which > 57 || e.currentTarget.value.length >= 11) {
                e.preventDefault();
                return false;
            }
        };

        $input.on("keypress", validateDigitOnly);

        $input.on("input", function() {
            this.value = this.value.replace(/[^\d]/g, '').slice(0, 11);
        });

        $input.on("paste", function(e) {
            const pastedData = e.originalEvent.clipboardData.getData('text');
            if (!/^\d*$/.test(pastedData)) {
                e.preventDefault();
            }
        });
    }

    setupInputHandlers($input, $cell, currentValue, field, contactId) {
        const handleCancel = () => this.handleCancel($cell, currentValue, contactId);
        const handleSave = () => this.handleSave($input, $cell, field, contactId);

        $input.on("blur", () => {
            if ($input.val() === currentValue) {
                handleCancel();
            } else {
                handleSave();
            }
        });

        $input.on("keydown", (e) => {
            if (e.key === "Escape") {
                if ($input.val() !== currentValue) {
                    this.showConfirmation('Discard Changes?', 'Are you sure you want to discard your changes?')
                        .then((result) => {
                            if (result.isConfirmed) {
                                handleCancel();
                            }
                        });
                } else {
                    handleCancel();
                }
            }
        });

        $input.on("keypress", (e) => {
            if (e.which === 13 && field !== "notes") {
                e.preventDefault();
                handleSave();
            }
        });
    }

    async handleCancel($cell, currentValue, contactId) {
        $cell.text(currentValue);
        $cell.removeClass("locked-by-me");
        this.lockedContacts.delete(contactId);
        await this.signalRHandler.unlockContact(contactId);
    }

    async handleSave($input, $cell, field, contactId) {
        const newValue = $input.val();
        
        if (!this.validateInput(field, newValue)) {
            return;
        }
        
        const $row = $cell.closest("tr");
        const updateData = this.getUpdateData($row, field, newValue);
        
        await this.updateContact(contactId, updateData);
        $cell.text(newValue);
        await this.signalRHandler.updateContact(contactId, field, newValue);
        $cell.removeClass("locked-by-me");
        this.lockedContacts.delete(contactId);
        await this.signalRHandler.unlockContact(contactId);
    }

    validateInput(field, value) {
        if (field === "phone") {
            if (!/^\d+$/.test(value)) {
                this.showNotification('Invalid Phone Number', 'Phone number can only contain digits (0-9)', 'error');
                return false;
            }
            
            if (value.length !== 11) {
                this.showNotification('Invalid Phone Number', 'Phone number must be exactly 11 digits', 'error');
                return false;
            }
        }
        
        return this.validateField(field, value);
    }

    validateField(field, value) {
        const $tempForm = $("<form>").append($("<input>", { name: field, value: value }));
        $tempForm.validate({
            rules: { [field]: this.validationConfig.rules[field] },
            messages: { [field]: this.validationConfig.messages[field] }
        });

        if (!$tempForm.valid()) {
            const errorList = $tempForm.validate().errorList;
            if (errorList.length > 0) {
                this.showNotification('Validation Error', errorList[0].message, 'error');
            }
            return false;
        }
        
        return true;
    }

    getUpdateData($row, field, newValue) {
        const data = {};
        ['name', 'phone', 'address', 'notes'].forEach(fieldName => {
            const selector = fieldName === field ? undefined : `[data-field="${fieldName}"]`;
            data[fieldName] = selector ? $row.find(selector).text().trim() : newValue;
        });
        return data;
    }

    async updateContact(contactId, data) {
        return $.ajax({
            url: `/api/Contact/${contactId}`,
            method: "PUT",
            contentType: "application/json",
            data: JSON.stringify(data)
        });
    }

    showNotification(title, message, icon) {
        Swal.fire({
            title,
            text: message,
            icon,
            confirmButtonText: 'OK'
        });
    }

    showConfirmation(title, message) {
        return Swal.fire({
            title,
            text: message,
            icon: 'warning',
            showCancelButton: true,
            confirmButtonText: 'Yes, discard it!',
            cancelButtonText: 'No, keep editing'
        });
    }

    handleLockUpdate(contactId, isLocked, lockedBy) {
        if (!contactId) return;

        const $row = $(`tr[data-contact-id="${contactId}"]`);
        const $cells = $row.find('.editable');
        
        if (isLocked) {
            this.lockedContacts.set(contactId, { connectionId: lockedBy });
            $cells.addClass('locked');
            
            const isOwnedByMe = lockedBy === this.signalRHandler.connectionId;
            if (isOwnedByMe) {
                $cells.addClass('locked-by-me');
            } else {
                $cells.addClass('locked-by-others');
                $row.addClass('bg-light locked-row');
            }
        } else {
            this.lockedContacts.delete(contactId);
            $cells.removeClass('locked locked-by-others locked-by-me');
            $row.removeClass('bg-light locked-row');
        }
    }
}