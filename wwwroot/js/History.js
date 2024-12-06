const sortDirections = [true, true, true, true, true]; // Sorting directions for columns
const rowsPerPage = 9;
let currentPage = 1;
let allRows = [];
let filteredRows = [];
const table = document.getElementById('historyTable');
let tbody = table ? table.querySelector('tbody') : null;

window.onload = function () {
    defaultFunction();
    // Check if tbody exists and has rows
    if (tbody && tbody.querySelectorAll('tr').length > 0) {
        allRows = Array.from(tbody.querySelectorAll('tr'));
        filteredRows = [...allRows];
        const totalPages = Math.ceil(filteredRows.length / rowsPerPage);
        displayPage(currentPage, totalPages);
    } else {
        // Handle the case where no records exist
        if (tbody) {
            tbody.innerHTML = '<tr><td colspan="100%" style="text-align: center;">No records found.</td></tr>';
        } else if (table) {
            table.innerHTML = '<p>No records found.</p>'; // For cases where <tbody> is completely missing
        }
    }
};

// Define the default function
function defaultFunction() {
    var getHeadtab = document.getElementById('history-tab');

    // Apply both styles using cssText
    getHeadtab.style.cssText = 'background-color: #d55401; color: white; border-radius: 10px;';
}

// Display rows for the current page
function displayPage(page, totalPages) {
    tbody.innerHTML = ''; // Clear existing rows
    if (page < 1) page = 1;
    if (page > totalPages) page = totalPages;

    currentPage = page; // Update the current page
    const start = (page - 1) * rowsPerPage;
    const end = start + rowsPerPage;
    const pageRows = filteredRows.slice(start, end);

    pageRows.forEach(row => tbody.appendChild(row));
    document.getElementById('paginationControls').innerHTML = generatePaginationControls(page, totalPages);
}

// Generate pagination controls
function generatePaginationControls(current, total) {
    let controls = '';
    for (let i = 1; i <= total; i++) {
        controls += `<button class="btn ${i === current ? 'btn-primary' : 'btn-light'}" onclick="displayPage(${i}, ${total})">${i}</button> `;
    }
    return controls;
}

// Sort the table by a specific column index
function sortTable(colIndex) {
    const isNumericColumn = [2, 3].includes(colIndex); // Assuming columns 2 and 3 are numeric/date columns
    sortDirections[colIndex] = !sortDirections[colIndex];

    // Sort only the filtered rows
    filteredRows.sort((rowA, rowB) => {
        const cellA = rowA.cells[colIndex].textContent.trim();
        const cellB = rowB.cells[colIndex].textContent.trim();

        // Numeric/date sorting
        if (isNumericColumn) {
            const dateA = new Date(cellA);
            const dateB = new Date(cellB);
            return sortDirections[colIndex] ? dateA - dateB : dateB - dateA;
        } else {
            // Text sorting
            return sortDirections[colIndex] ? cellA.localeCompare(cellB) : cellB.localeCompare(cellA);
        }
    });

    // Recalculate total pages after sorting
    const totalPages = Math.ceil(filteredRows.length / rowsPerPage);
    displayPage(currentPage, totalPages); // Show the current page after sorting
    updateSortIcons(colIndex);
}

// Update sort icons based on the column and sort direction
function updateSortIcons(colIndex) {
    const ths = table.querySelectorAll('th');
    ths.forEach((th, index) => {
        const icon = th.querySelector('i');
        if (index === colIndex) {
            icon.classList.remove('fa-sort', 'fa-sort-up', 'fa-sort-down');
            icon.classList.add(sortDirections[colIndex] ? 'fa-sort-up' : 'fa-sort-down');
        } else {
            icon.classList.remove('fa-sort', 'fa-sort-up', 'fa-sort-down');
            icon.classList.add('fa-sort');
        }
    });
}

function clearInput() {
    document.getElementById('filterInput').value = ''; // Clear the input field
    filterTable(); // Optional: Call the filterTable function to reset the filter
    document.getElementById("clearIcon").style.display = "none";
}

// Filter the table rows based on input
function filterTable() {
    const input = document.getElementById('filterInput').value.toLowerCase();

    if (input.length > 0) {
        document.getElementById("clearIcon").style.display = "block";
    }
    else {
        document.getElementById("clearIcon").style.display = "none";
    }

    // Filter all rows based on the input
    filteredRows = allRows.filter(row => {
        const cells = row.getElementsByTagName('td');
        let match = false;

        Array.from(cells).forEach(cell => {
            if (cell.textContent.toLowerCase().includes(input)) {
                match = true;
            }
        });

        return match;
    });

    if (tbody) {
        // Handle the case when no rows match the filter
        if (filteredRows.length === 0) {
            // Clear the tbody content
            tbody.innerHTML = '';

            // Directly add the "No results found" message inside the tbody
            tbody.innerHTML = '<span style="width:97vw;display: flex;justify-content: center;align-items: center;">No results found</span>';
        }
        else {
            const totalPages = Math.ceil(filteredRows.length / rowsPerPage);
            displayPage(1, totalPages); // Always show the first page after filtering
        }
    }

    // Update pagination controls
    const totalPages = Math.ceil(filteredRows.length / rowsPerPage);
    document.getElementById('paginationControls').innerHTML = generatePaginationControls(currentPage, totalPages);
}

// Event listener for the filter input
document.getElementById('filterInput').addEventListener('input', filterTable);
