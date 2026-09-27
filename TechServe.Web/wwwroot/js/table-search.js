document.addEventListener('DOMContentLoaded', () => {
    document.querySelectorAll('.table-search').forEach((input) => {
        input.addEventListener('input', () => {
            const table = input.closest('.panel')?.querySelector('.searchable-table');
            if (!table) return;

            const query = input.value.trim().toLowerCase();
            table.querySelectorAll('tbody tr').forEach((row) => {
                const text = row.textContent.toLowerCase();
                row.classList.toggle('hidden-row', query && !text.includes(query));
                if (!query) row.classList.remove('hidden-row');
            });
        });
    });
});
