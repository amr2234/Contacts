class ContactManager {
    constructor(signalRHandler) {
        this.signalRHandler = signalRHandler;
        this.currentPage = 1;
        this.pageSize = 5;
        this.searchTimeout = null;
        this.setupEventHandlers();
    }

    setupEventHandlers() {
        $('#searchInput').on('input', () => {
            this.debounce(() => {
                this.currentPage = 1;
                this.loadContacts();
            });
        });

        $(document).on('click', '.page-link', (e) => {
            e.preventDefault();
            const page = $(e.target).data('page');
            if (page) {
                this.currentPage = page;
                this.loadContacts();
            }
        });

    }

    debounce(callback, delay = 300) {
        clearTimeout(this.searchTimeout);
        this.searchTimeout = setTimeout(callback, delay);
    }

    async loadContacts() {
        const searchTerm = $('#searchInput').val();
        const response = await $.ajax({
            url: '/api/Contact/api',
            method: 'GET',
            data: {
                search: searchTerm,
                pageNumber: this.currentPage,
                pageSize: this.pageSize
            }
        });

        this.updateContactsTable(response.contacts);
        this.updatePagination(response.totalCount);
    }

    updateContactsTable(contacts) {
        const $tbody = $('#contactsTableBody');
        $tbody.empty();

        if (contacts.length === 0) {
            $tbody.append('<tr><td colspan="5" class="text-center">No contacts found</td></tr>');
            return;
        }

        contacts.forEach(contact => {
            const row = this.createContactRow(contact);
            $tbody.append(row);
        });

        if (window.contactEditor) {
            window.contactEditor.setupEditableFields();
        }
    }

    createContactRow(contact) {
        return `
            <tr data-contact-id="${contact.id}">
                <td class="editable" data-field="name">${contact.name}</td>
                <td class="editable" data-field="phone">${contact.phone}</td>
                <td class="editable" data-field="address">${contact.address}</td>
                <td class="editable" data-field="notes">${contact.notes}</td>
                <td class="text-end">
                    <button class="btn btn-sm btn-outline-danger delete-contact" data-contact-id="${contact.id}">
                        <i class="bi bi-trash"></i>
                    </button>
                </td>
            </tr>
        `;
    }

    updatePagination(totalCount) {
        const $pagination = $('.pagination');
        $pagination.empty();

        const totalPages = Math.ceil(totalCount / this.pageSize);
        const maxPagesToShow = 5;
        let startPage = Math.max(1, this.currentPage - Math.floor(maxPagesToShow / 2));
        let endPage = Math.min(totalPages, startPage + maxPagesToShow - 1);

        if (endPage - startPage + 1 < maxPagesToShow) {
            startPage = Math.max(1, endPage - maxPagesToShow + 1);
        }

        this.appendPaginationItem($pagination, 'Previous', this.currentPage === 1, this.currentPage - 1);

        if (startPage > 1) {
            this.appendPaginationItem($pagination, '1', false, 1);
            if (startPage > 2) {
                this.appendEllipsis($pagination);
            }
        }

        for (let i = startPage; i <= endPage; i++) {
            this.appendPaginationItem($pagination, i.toString(), i === this.currentPage, i, i === this.currentPage);
        }

        if (endPage < totalPages) {
            if (endPage < totalPages - 1) {
                this.appendEllipsis($pagination);
            }
            this.appendPaginationItem($pagination, totalPages.toString(), false, totalPages);
        }

        this.appendPaginationItem($pagination, 'Next', this.currentPage === totalPages, this.currentPage + 1);

        const firstItem = (this.currentPage - 1) * this.pageSize + 1;
        const lastItem = Math.min(this.currentPage * this.pageSize, totalCount);
        $('#paginationInfo').text(`Showing ${firstItem} to ${lastItem} of ${totalCount} contacts`);
    }

    appendPaginationItem($pagination, text, isDisabled, page, isActive = false) {
        const activeClass = isActive ? 'active' : '';
        const disabledClass = isDisabled ? 'disabled' : '';

        $pagination.append(`
            <li class="page-item ${activeClass} ${disabledClass}">
                <a class="page-link" href="#" data-page="${page}">${text}</a>
            </li>
        `);
    }

    appendEllipsis($pagination) {
        $pagination.append('<li class="page-item disabled"><span class="page-link">...</span></li>');
    }
}