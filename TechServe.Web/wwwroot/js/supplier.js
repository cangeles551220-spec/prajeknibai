(function () {
    const supplierRows = Array.from(document.querySelectorAll('[data-supplier-row]'));
    const detailName = document.getElementById('detailName');
    const detailInitials = document.getElementById('detailInitials');
    const detailContact = document.getElementById('detailContact');
    const detailAddress = document.getElementById('detailAddress');
    const detailPayment = document.getElementById('detailPayment');
    const detailLead = document.getElementById('detailLead');
    const detailPhone = document.getElementById('detailPhone');
    const detailStatusBadge = document.getElementById('detailStatusBadge');
    const searchInput = document.getElementById('supplierSearch');
    const categoryFilter = document.getElementById('categoryFilter');
    const statusFilter = document.getElementById('statusFilter');
    const exportBtn = document.getElementById('exportSuppliersBtn');
    const prevBtn = document.getElementById('prevPageBtn');
    const nextBtn = document.getElementById('nextPageBtn');
    const rangeLabel = document.getElementById('supplierRangeLabel');
    const supplierModal = document.getElementById('supplierModal');
    const supplierForm = document.getElementById('supplierForm');
    const supplierIdInput = document.getElementById('supplierId');
    const supplierModalTitle = document.getElementById('supplierModalTitle');
    const purchaseModal = document.getElementById('purchaseModal');
    const purchaseForm = document.getElementById('purchaseForm');
    const orderList = document.getElementById('orderList');
    const ordersModal = document.getElementById('ordersModal');
    const allOrdersList = document.getElementById('allOrdersList');
    const selectedSupplierState = { selectedId: null, page: 0, pageSize: 6, search: '', category: 'all', status: 'all' };

    const getInitials = (name) => {
        const sources = (name || 'Supplier').split(/\s+/).filter(Boolean).slice(0, 2);
        return (sources.map(item => item[0]).join('').toUpperCase() || 'S');
    };

    const getRows = () => {
        const rows = supplierRows.filter((row) => {
            const name = (row.dataset.name || '').toLowerCase();
            const category = (row.dataset.category || '').toLowerCase();
            const email = (row.dataset.email || '').toLowerCase();
            const status = (row.dataset.status || '').toLowerCase();
            const search = selectedSupplierState.search.trim().toLowerCase();
            const matchesSearch = !search || name.includes(search) || category.includes(search) || email.includes(search);
            const matchesCategory = selectedSupplierState.category === 'all' || (row.dataset.category || '') === selectedSupplierState.category;
            const matchesStatus = selectedSupplierState.status === 'all' || status === selectedSupplierState.status.toLowerCase();
            return matchesSearch && matchesCategory && matchesStatus;
        });

        selectedSupplierState.page = Math.max(0, Math.min(selectedSupplierState.page, Math.max(0, Math.ceil(rows.length / selectedSupplierState.pageSize) - 1)));
        return rows;
    };

    const renderTable = () => {
        const rows = getRows();
        const startIndex = selectedSupplierState.page * selectedSupplierState.pageSize;
        const visibleRows = rows.slice(startIndex, startIndex + selectedSupplierState.pageSize);

        supplierRows.forEach((row) => {
            const show = visibleRows.includes(row);
            row.style.display = show ? '' : 'none';
        });

        if (rangeLabel) {
            const total = rows.length || supplierRows.length;
            const start = total === 0 ? 0 : startIndex + 1;
            const end = Math.min(startIndex + selectedSupplierState.pageSize, total);
            rangeLabel.textContent = total === 0 ? 'Showing 0 suppliers' : `Showing ${start}-${end} of ${total} suppliers`;
        }

        const pageCount = Math.max(1, Math.ceil(rows.length / selectedSupplierState.pageSize));
        prevBtn.disabled = selectedSupplierState.page === 0;
        nextBtn.disabled = selectedSupplierState.page >= pageCount - 1 || rows.length === 0;

        if (!rows.length) {
            return;
        }

        const selected = rows.find((row) => String(row.dataset.id) === String(selectedSupplierState.selectedId)) || visibleRows[0] || rows[0];
        updateDetail(selected);
    };

    const updateDetail = (row) => {
        if (!row || !detailName || !detailInitials || !detailAddress || !detailPayment || !detailLead || !detailPhone || !detailStatusBadge) {
            return;
        }

        const name = row.dataset.name || 'Supplier';
        detailName.textContent = name;
        detailInitials.textContent = getInitials(name);
        detailContact.textContent = row.dataset.contact || '—';
        detailAddress.textContent = row.dataset.address || '—';
        detailPayment.textContent = row.dataset.payment || '—';
        detailLead.textContent = row.dataset.lead || '—';
        detailPhone.textContent = row.dataset.phone || '—';
        detailStatusBadge.textContent = `${row.dataset.status || 'Active'} Supplier`;
        detailStatusBadge.style.background = (row.dataset.status || 'Active') === 'Active' ? '#ebf8f3' : '#f3f5fa';
        detailStatusBadge.style.color = (row.dataset.status || 'Active') === 'Active' ? '#1b9d68' : '#6b7a90';
        selectedSupplierState.selectedId = row.dataset.id || null;

        supplierRows.forEach((item) => item.classList.toggle('is-selected', item === row));
    };

    const openModal = (modal) => {
        if (!modal) return;
        modal.classList.add('visible');
        modal.setAttribute('aria-hidden', 'false');
    };

    const closeModal = (modal) => {
        if (!modal) return;
        if (modal.contains(document.activeElement)) {
            document.activeElement.blur();
        }
        modal.classList.remove('visible');
        modal.setAttribute('aria-hidden', 'true');
    };

    const clearForm = () => {
        supplierForm.reset();
        supplierIdInput.value = '';
        supplierModalTitle.textContent = 'Add Supplier';
    };

    const openAddSupplierModal = () => {
        clearForm();
        document.getElementById('supplierCategory').value = 'Motherboards';
        document.getElementById('supplierStatus').value = 'Active';
        document.getElementById('supplierPaymentTerms').value = 'Net 30';
        document.getElementById('supplierLeadTime').value = '3-5 Days';
        openModal(supplierModal);
    };

    const openEditSupplierModal = () => {
        const selected = supplierRows.find((row) => String(row.dataset.id) === String(selectedSupplierState.selectedId));
        if (!selected) return;
        supplierModalTitle.textContent = 'Edit Supplier';
        supplierIdInput.value = selected.dataset.id || '';
        document.getElementById('supplierName').value = selected.dataset.name || '';
        document.getElementById('supplierCategory').value = selected.dataset.category || 'Motherboards';
        document.getElementById('supplierStatus').value = selected.dataset.status || 'Active';
        document.getElementById('supplierContactPerson').value = selected.dataset.contact || '';
        document.getElementById('supplierContactEmail').value = selected.dataset.email || '';
        document.getElementById('supplierPhone').value = selected.dataset.phone || '';
        document.getElementById('supplierPaymentTerms').value = selected.dataset.payment || 'Net 30';
        document.getElementById('supplierLeadTime').value = selected.dataset.lead || '3-5 Days';
        document.getElementById('supplierAddress').value = selected.dataset.address || '';
        openModal(supplierModal);
    };

    const openPurchaseModal = () => {
        const dateValue = new Date().toISOString().split('T')[0];
        document.getElementById('poDate').value = dateValue;
        document.getElementById('poNumber').value = `PO-${new Date().getFullYear()}-${String(Math.floor(Math.random() * 900) + 100)}`;
        openModal(purchaseModal);
    };

    const exportCsv = () => {
        const rows = getRows();
        if (!rows.length) {
            return;
        }

        const header = ['Supplier','Category','Email','Phone','Status'];
        const csvBody = rows.map((row) => [
            row.dataset.name || '',
            row.dataset.category || '',
            row.dataset.email || '',
            row.dataset.phone || '',
            row.dataset.status || ''
        ].map((value) => `"${String(value).replace(/"/g, '""')}"`).join(','));
        const csv = [header.join(','), ...csvBody].join('\n');
        const blob = new Blob([csv], { type: 'text/csv;charset=utf-8;' });
        const link = document.createElement('a');
        const url = URL.createObjectURL(blob);
        link.href = url;
        link.download = 'suppliers.csv';
        document.body.appendChild(link);
        link.click();
        document.body.removeChild(link);
        URL.revokeObjectURL(url);
    };

    const handleSupplierSubmit = (event) => {
        event.preventDefault();

        const formData = new FormData(supplierForm);
        const payload = Object.fromEntries(formData.entries());
        const isEdit = Boolean(payload.SupplierId);
        const route = isEdit ? '/Supplier/Update' : '/Supplier/Create';

        const body = new URLSearchParams();
        Object.entries(payload).forEach(([key, value]) => {
            body.append(key, String(value));
        });

        fetch(route, {
            method: 'POST',
            headers: {
                'Content-Type': 'application/x-www-form-urlencoded; charset=UTF-8',
                'RequestVerificationToken': document.querySelector('input[name="__RequestVerificationToken"]')?.value || ''
            },
            body: body.toString()
        }).then((response) => {
            if (response.redirected || response.ok) {
                window.location.reload();
                return;
            }
            alert('Supplier could not be saved. Please check the entered information and try again.');
        }).catch(() => {
            alert('Unable to save the supplier. Please check your connection and try again.');
        });
    };

    const handlePurchaseSubmit = (event) => {
        event.preventDefault();
        const selected = supplierRows.find((row) => String(row.dataset.id) === String(selectedSupplierState.selectedId));
        const item = document.getElementById('poItem').value.trim();
        const qty = document.getElementById('poQty').value;
        const amount = document.getElementById('poAmount').value;

        const orderTemplate = `
            <div class="order-item">
                <div>
                    <strong>${document.getElementById('poNumber').value || 'PO-New'}</strong>
                    <small>${document.getElementById('poDate').value || new Date().toISOString().split('T')[0]} · ${qty} units</small>
                </div>
                <div class="amount">$${Number(amount || 0).toFixed(2)}</div>
            </div>
        `;

        const selectedOrderList = orderList || document.getElementById('orderList');
        if (selectedOrderList) {
            selectedOrderList.insertAdjacentHTML('afterbegin', orderTemplate);
        }

        if (selected && selected.dataset.name && allOrdersList) {
            allOrdersList.insertAdjacentHTML('afterbegin', orderTemplate);
        }

        closeModal(purchaseModal);
        purchaseForm.reset();
    };

    const openAllOrders = () => {
        if (!allOrdersList || !orderList) return;
        allOrdersList.innerHTML = orderList.innerHTML || '<p>No orders found for this supplier.</p>';
        openModal(ordersModal);
    };

    supplierRows.forEach((row) => {
        row.addEventListener('click', () => updateDetail(row));
    });

    searchInput?.addEventListener('input', (event) => {
        selectedSupplierState.search = event.target.value;
        selectedSupplierState.page = 0;
        renderTable();
    });

    categoryFilter?.addEventListener('change', (event) => {
        selectedSupplierState.category = event.target.value;
        selectedSupplierState.page = 0;
        renderTable();
    });

    statusFilter?.addEventListener('change', (event) => {
        selectedSupplierState.status = event.target.value;
        selectedSupplierState.page = 0;
        renderTable();
    });

    prevBtn?.addEventListener('click', () => {
        if (selectedSupplierState.page > 0) {
            selectedSupplierState.page -= 1;
            renderTable();
        }
    });

    nextBtn?.addEventListener('click', () => {
        const rows = getRows();
        const maxPage = Math.max(0, Math.ceil(rows.length / selectedSupplierState.pageSize) - 1);
        if (selectedSupplierState.page < maxPage) {
            selectedSupplierState.page += 1;
            renderTable();
        }
    });

    exportBtn?.addEventListener('click', exportCsv);
    document.querySelector('.btn-add')?.addEventListener('click', openAddSupplierModal);
    document.getElementById('editSupplierBtn')?.addEventListener('click', openEditSupplierModal);
    document.getElementById('newPurchaseBtn')?.addEventListener('click', openPurchaseModal);
    document.getElementById('backToSupplierListBtn')?.addEventListener('click', () => {
        document.querySelector('.supplier-table')?.scrollIntoView({ behavior: 'smooth', block: 'center' });
        document.getElementById('supplierSearch')?.focus();
    });
    document.getElementById('viewAllOrdersBtn')?.addEventListener('click', openAllOrders);
    document.querySelectorAll('.modal-close, .modal-close-secondary').forEach((button) => {
        button.addEventListener('click', () => {
            closeModal(supplierModal);
            closeModal(purchaseModal);
            closeModal(ordersModal);
        });
    });

    supplierForm?.addEventListener('submit', handleSupplierSubmit);
    purchaseForm?.addEventListener('submit', handlePurchaseSubmit);

    if (supplierRows.length) {
        selectedSupplierState.selectedId = supplierRows[0].dataset.id || null;
        renderTable();
    }
})();
