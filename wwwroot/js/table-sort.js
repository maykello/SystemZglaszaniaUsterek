(function () {
    'use strict';

    function parseCellValue(text) {
        text = text.trim();

        if (text === '' || text === '-') {
            return { type: 'empty', value: '' };
        }

        var dateMatch = text.match(/^(\d{2})\.(\d{2})\.(\d{4})(?:\s+(\d{2}):(\d{2}))?$/);
        if (dateMatch) {
            var date = new Date(
                parseInt(dateMatch[3], 10),
                parseInt(dateMatch[2], 10) - 1,
                parseInt(dateMatch[1], 10),
                dateMatch[4] ? parseInt(dateMatch[4], 10) : 0,
                dateMatch[5] ? parseInt(dateMatch[5], 10) : 0
            );
            return { type: 'number', value: date.getTime() };
        }

        var numericText = text.replace(/\s/g, '').replace(',', '.');
        if (/^-?\d+(\.\d+)?$/.test(numericText)) {
            return { type: 'number', value: parseFloat(numericText) };
        }

        return { type: 'string', value: text.toLowerCase() };
    }

    function compareValues(a, b) {
        if (a.type === 'empty' && b.type === 'empty') return 0;
        if (a.type === 'empty') return -1;
        if (b.type === 'empty') return 1;

        if (a.type === 'number' && b.type === 'number') {
            return a.value - b.value;
        }

        return String(a.value).localeCompare(String(b.value), 'pl');
    }

    function sortTable(table, columnIndex, ascending) {
        var tbody = table.tBodies[0];
        if (!tbody) return;

        var rows = Array.prototype.slice.call(tbody.rows);
        rows.sort(function (rowA, rowB) {
            var cellA = rowA.cells[columnIndex];
            var cellB = rowB.cells[columnIndex];
            var valueA = parseCellValue(cellA ? cellA.textContent : '');
            var valueB = parseCellValue(cellB ? cellB.textContent : '');
            var result = compareValues(valueA, valueB);
            return ascending ? result : -result;
        });

        rows.forEach(function (row) { tbody.appendChild(row); });
    }

    function initSortableTable(table) {
        var headerRow = table.tHead && table.tHead.rows[0];
        if (!headerRow) return;

        Array.prototype.forEach.call(headerRow.cells, function (th, columnIndex) {
            if (th.hasAttribute('data-no-sort') || th.textContent.trim() === '') {
                return;
            }

            th.classList.add('sortable-th');

            var arrow = document.createElement('span');
            arrow.className = 'sort-arrow';
            th.appendChild(arrow);

            th.addEventListener('click', function () {
                var ascending = !th.classList.contains('sort-asc');

                Array.prototype.forEach.call(headerRow.cells, function (otherTh) {
                    otherTh.classList.remove('sort-asc', 'sort-desc');
                });
                th.classList.add(ascending ? 'sort-asc' : 'sort-desc');

                sortTable(table, columnIndex, ascending);
            });
        });
    }

    document.querySelectorAll('table[data-sortable="true"]').forEach(initSortableTable);
})();
