// =============================================
// DATABASE QUERY TOOL - EXTENDED FUNCTIONS
// Edit Row, Delete Row, Full Backup
// =============================================

let currentEditRow = null;
let currentEditSchema = '';
let currentEditTable = '';
let allTablesForBackup = [];
let currentQueryAction = '';
// ✅ THÊM: Store Primary Key columns
let currentTablePrimaryKeys = [];

// =============================================
// ROW ACTION BUTTONS
// =============================================

function enableRowActions() {
    console.log('🔧 enableRowActions() called');
    console.log('currentResult:', currentResult);
    
    if (!currentResult || !currentResult.columns || !currentResult.rows) {
        console.error('❌ Missing currentResult data');
        return;
    }
    
    // ✅ Kiểm tra đã có cột Actions chưa để tránh duplicate
    const $table = $('#resultTableWrapper table');
    const existingActionsCount = $table.find('thead th:contains("Actions")').length;
    
    if (existingActionsCount > 0) {
        console.log(`⚠️ Actions column already exists (${existingActionsCount} columns), skipping...`);
        return;
    }
    
    // Detect table from query
    const query = $('#queryEditor').val();
    console.log('Query:', query);
    
    const fromMatch = query.match(/FROM\s+\[?(\w+)\]?\.\[?(\w+)\]?/i);
    
    if (!fromMatch) {
        console.error('❌ Cannot detect table from query');
        return;
    }
    
    currentEditSchema = fromMatch[1];
    currentEditTable = fromMatch[2];
    console.log(`✅ Detected table: [${currentEditSchema}].[${currentEditTable}]`);
    
    // ✅ LẤY PRIMARY KEY từ server
    loadTablePrimaryKeys(currentEditSchema, currentEditTable, function() {
        // Add header
        $table.find('thead tr').append('<th style="width: 130px; text-align: center; background: #667eea; color: white;">Actions</th>');
        console.log('✅ Added Actions header');
        
        // Add action buttons for each row
        let buttonsAdded = 0;
        $table.find('tbody tr').each(function(rowIndex) {
            const $row = $(this);
            
            // ✅ QUAN TRỌNG: Dùng jQuery .append() để đảm bảo render đúng
            const $actionsCell = $('<td>', {
                style: 'text-align: center; white-space: nowrap; padding: 8px;'
            });
            
            // Create INSERT button
            const $btnInsert = $('<button>', {
                class: 'row-action-btn btn-insert',
                title: 'Copy INSERT query',
                html: '<i class="material-icons icon-align">add</i>'
            }).on('click', function() {
                showInsertQueryForRow(rowIndex);
            });
            
            // Create EDIT button
            const $btnEdit = $('<button>', {
                class: 'row-action-btn btn-edit',
                title: 'Edit or gen UPDATE query',
                html: '<i class="material-icons icon-align">edit</i>'
            }).on('click', function() {
                showEditQueryForRow(rowIndex);
            });
            
            // Create DELETE button
            const $btnDelete = $('<button>', {
                class: 'row-action-btn btn-delete',
                title: 'Delete or gen DELETE query',
                html: '<i class="material-icons icon-align">delete</i>'
            }).on('click', function() {
                showDeleteQueryForRow(rowIndex);
            });
            
            // Append buttons to cell
            $actionsCell.append($btnInsert).append(' ').append($btnEdit).append(' ').append($btnDelete);
            
            // Append cell to row
            $row.append($actionsCell);
            buttonsAdded++;
        });
        
        console.log(`✅ Added ${buttonsAdded} action button sets (${buttonsAdded * 3} buttons total)`);
    });
    
    // ✅ FORCE re-apply CSS nếu cần
    $('head').append(`
        <style id="row-action-btn-styles">
            .row-action-btn {
                display: inline-flex !important;
                align-items: center !important;
                gap: 4px !important;
                padding: 5px 10px !important;
                border: none !important;
                border-radius: 4px !important;
                font-size: 11px !important;
                font-weight: 600 !important;
                cursor: pointer !important;
                transition: all 0.2s !important;
                margin: 0 2px !important;
                vertical-align: middle !important;
            }
            
            .row-action-btn i.material-icons {
                font-size: 14px !important;
                vertical-align: middle !important;
            }
            
            .row-action-btn.btn-insert {
                background: #2196F3 !important;
                color: white !important;
            }
            
            .row-action-btn.btn-insert:hover {
                background: #1976D2 !important;
                transform: translateY(-1px);
                box-shadow: 0 2px 8px rgba(33, 150, 243, 0.4);
            }
            
            .row-action-btn.btn-edit {
                background: #4CAF50 !important;
                color: white !important;
            }
            
            .row-action-btn.btn-edit:hover {
                background: #388E3C !important;
                transform: translateY(-1px);
                box-shadow: 0 2px 8px rgba(76, 175, 80, 0.4);
            }
            
            .row-action-btn.btn-delete {
                background: #f44336 !important;
                color: white !important;
            }
            
            .row-action-btn.btn-delete:hover {
                background: #d32f2f !important;
                transform: translateY(-1px);
                box-shadow: 0 2px 8px rgba(244, 67, 54, 0.4);
            }
        </style>
    `);
}

// ✅ THÊM HÀM: Load Primary Keys từ server
function loadTablePrimaryKeys(schema, table, callback) {
    $.ajax({
        url: '/Admin/Base/DatabaseQuery/GetTableSchema',
        type: 'GET',
        data: { schemaName: schema, tableName: table },
        success: function(response) {
            if (response.output === 1) {
                // Lấy danh sách columns là Primary Key
                currentTablePrimaryKeys = response.data
                    .filter(col => col.isPrimaryKey)
                    .map(col => col.columnName);
                
                console.log('✅ Primary Keys loaded:', currentTablePrimaryKeys);
                
                if (currentTablePrimaryKeys.length === 0) {
                    console.warn('⚠️ Table has NO primary key - will use ALL columns in WHERE');
                }
                
                if (callback) callback();
            } else {
                console.error('❌ Failed to load table schema');
                if (callback) callback();
            }
        },
        error: function() {
            console.error('❌ Error loading table schema');
            if (callback) callback();
        }
    });
}

// ✅ THÊM HÀM: Build WHERE conditions chỉ với Primary Key
function buildPrimaryKeyConditions(rowData, columns) {
    const conditions = {};
    
    // Nếu có Primary Key, chỉ dùng PK
    if (currentTablePrimaryKeys.length > 0) {
        currentTablePrimaryKeys.forEach(pkColumn => {
            const index = columns.indexOf(pkColumn);
            if (index !== -1) {
                conditions[pkColumn] = rowData[index];
            }
        });
    } else {
        // Fallback: Nếu không có PK, dùng tất cả columns (như cũ)
        columns.forEach((col, index) => {
            conditions[col] = rowData[index];
        });
    }
    
    return conditions;
}

// =============================================
// INSERT QUERY FOR ROW
// =============================================

function showInsertQueryForRow(rowIndex) {
    if (!currentResult || !currentResult.rows[rowIndex]) return;
    
    const rowData = currentResult.rows[rowIndex];
    const columns = currentResult.columns;
    const tableName = `[${currentEditSchema}].[${currentEditTable}]`;
    
    // Generate INSERT query
    let insertQuery = `INSERT INTO ${tableName} (\n`;
    columns.forEach(col => {
        insertQuery += `    [${col}],\n`;
    });
    insertQuery = insertQuery.slice(0, -2) + '\n) VALUES (\n';
    
    columns.forEach((col, index) => {
        const value = rowData[index];
        const formattedValue = formatSqlValue(value);
        insertQuery += `    ${formattedValue}, -- ${col}\n`;
    });
    insertQuery = insertQuery.slice(0, -2) + '\n);';
    
    // ✅ QUAN TRỌNG: Store query in data attribute, không escape!
    // Show modal with options
    const content = `
        <div>
            <div style="background: #e7f3ff; padding: 10px 12px; border-left: 4px solid #2196F3; border-radius: 4px; margin-bottom: 12px;">
                <strong style="display: flex; align-items: center; gap: 6px; font-size: 14px;">
                    <i class="material-icons icon-align" style="font-size: 16px;">info</i> INSERT Query
                </strong>
                <p style="margin: 4px 0 0 0; font-size: 12px;">Copy dữ liệu của row này thành câu INSERT mới</p>
            </div>
            
            <div class="query-preview" style="background: #f5f5f5; padding: 10px; border-radius: 4px; font-family: monospace; font-size: 11px; max-height: 300px; overflow-y: auto;">${escapeHtml(insertQuery)}</div>
            
            <div class="action-buttons" style="margin-top: 12px; display: flex; gap: 8px;">
                <button class="btn btn-primary btn-sm btn-query-action" data-action="copy-clipboard" data-query-index="insert-${rowIndex}">
                    <i class="material-icons icon-align" style="font-size: 14px;">content_copy</i>
                    Copy to Clipboard
                </button>
                <button class="btn btn-info btn-sm btn-query-action" data-action="copy-editor" data-query-index="insert-${rowIndex}">
                    <i class="material-icons icon-align" style="font-size: 14px;">code</i>
                    Copy to Editor
                </button>
            </div>
        </div>
    `;
    
    // ✅ Store query in global object with unique key
    if (!window.queryCache) window.queryCache = {};
    window.queryCache[`insert-${rowIndex}`] = insertQuery;
    
    showQueryActionModal('INSERT Query', content);
}

// =============================================
// EDIT QUERY FOR ROW
// =============================================

function showEditQueryForRow(rowIndex) {
    if (!currentResult || !currentResult.rows[rowIndex]) return;
    
    const rowData = currentResult.rows[rowIndex];
    const columns = currentResult.columns;
    const tableName = `[${currentEditSchema}].[${currentEditTable}]`;
    
    // ✅ THAY ĐỔI: Build WHERE chỉ với Primary Key
    const whereConditions = buildPrimaryKeyConditions(rowData, columns);
    
    // Generate UPDATE query
    let updateQuery = `UPDATE ${tableName}\nSET\n`;
    let whereClause = '\nWHERE\n';
    
    // SET clause: all columns
    columns.forEach((col, index) => {
        const value = rowData[index];
        const formattedValue = formatSqlValue(value);
        updateQuery += `    [${col}] = ${formattedValue},\n`;
    });
    
    // WHERE clause: chỉ Primary Key
    Object.keys(whereConditions).forEach(col => {
        const formattedValue = formatSqlValue(whereConditions[col]);
        whereClause += `    [${col}] = ${formattedValue} AND\n`;
    });
    
    updateQuery = updateQuery.slice(0, -2);
    whereClause = whereClause.slice(0, -5);
    updateQuery += whereClause + ';';
    
    // ✅ Store query in cache
    if (!window.queryCache) window.queryCache = {};
    window.queryCache[`update-${rowIndex}`] = updateQuery;
    
    // Show modal with options
    const content = `
        <div>
            <div style="background: #e7f3ff; padding: 10px 12px; border-left: 4px solid #4CAF50; border-radius: 4px; margin-bottom: 12px;">
                <strong style="display: flex; align-items: center; gap: 6px; font-size: 14px;">
                    <i class="material-icons icon-align" style="font-size: 16px;">edit</i> UPDATE Query
                </strong>
                <p style="margin: 4px 0 0 0; font-size: 12px;">Chỉnh sửa trực tiếp hoặc gen UPDATE query</p>
                ${currentTablePrimaryKeys.length > 0 
                    ? `<p style="margin: 4px 0 0 0; font-size: 11px; color: #4CAF50;"><strong>PK:</strong> ${currentTablePrimaryKeys.join(', ')}</p>` 
                    : `<p style="margin: 4px 0 0 0; font-size: 11px; color: #ff9800;"><strong>⚠️</strong> Không có Primary Key - sử dụng tất cả columns</p>`
                }
            </div>
            
            <div class="query-preview" style="background: #f5f5f5; padding: 10px; border-radius: 4px; font-family: monospace; font-size: 11px; max-height: 300px; overflow-y: auto;">${escapeHtml(updateQuery)}</div>
            
            <div class="action-buttons" style="margin-top: 12px; display: flex; gap: 8px; flex-wrap: wrap;">
                <button class="btn btn-primary btn-sm btn-query-action" data-action="copy-clipboard" data-query-index="update-${rowIndex}">
                    <i class="material-icons icon-align" style="font-size: 14px;">content_copy</i>
                    Copy Query
                </button>
            </div>
        </div>
    `;
    
    showQueryActionModal('UPDATE Query', content);
}

// =============================================
// DELETE QUERY FOR ROW
// =============================================

function showDeleteQueryForRow(rowIndex) {
    if (!currentResult || !currentResult.rows[rowIndex]) return;
    
    const rowData = currentResult.rows[rowIndex];
    const columns = currentResult.columns;
    const tableName = `[${currentEditSchema}].[${currentEditTable}]`;
    
    // ✅ THAY ĐỔI: Build WHERE chỉ với Primary Key
    const whereConditions = buildPrimaryKeyConditions(rowData, columns);
    
    // Generate DELETE query
    let deleteQuery = `DELETE FROM ${tableName}\nWHERE\n`;
    
    Object.keys(whereConditions).forEach(col => {
        const formattedValue = formatSqlValue(whereConditions[col]);
        deleteQuery += `    [${col}] = ${formattedValue} AND\n`;
    });
    
    deleteQuery = deleteQuery.slice(0, -5) + ';';
    
    // ✅ Store query in cache
    if (!window.queryCache) window.queryCache = {};
    window.queryCache[`delete-${rowIndex}`] = deleteQuery;
    
    // Show row preview
    let rowPreview = '<div style="background: #fff3cd; padding: 10px; border-radius: 4px; margin-bottom: 15px;"><strong>Row data:</strong><br/>';
    columns.forEach((col, index) => {
        const displayValue = rowData[index] === null ? 'NULL' : escapeHtml(String(rowData[index]));
        rowPreview += `<div style="padding: 2px 0;"><strong>${col}:</strong> ${displayValue}</div>`;
    });
    rowPreview += '</div>';
    
    // Show modal with options
    const content = `
        <div>
            <div style="background: #ffebee; padding: 10px 12px; border-left: 4px solid #f44336; border-radius: 4px; margin-bottom: 12px;">
                <strong style="display: flex; align-items: center; gap: 6px; font-size: 14px;">
                    <i class="material-icons icon-align" style="font-size: 16px;">warning</i> DELETE Query
                </strong>
                <p style="margin: 4px 0 0 0; font-size: 12px; color: #d32f2f;">Cảnh báo: Thao tác này sẽ XÓA dữ liệu!</p>
                ${currentTablePrimaryKeys.length > 0 
                    ? `<p style="margin: 4px 0 0 0; font-size: 11px; color: #4CAF50;"><strong>PK:</strong> ${currentTablePrimaryKeys.join(', ')}</p>` 
                    : `<p style="margin: 4px 0 0 0; font-size: 11px; color: #ff9800;"><strong>⚠️</strong> Không có Primary Key - sử dụng tất cả columns</p>`
                }
            </div>
            
            ${rowPreview}
            
            <div class="query-preview" style="background: #f5f5f5; padding: 10px; border-radius: 4px; font-family: monospace; font-size: 11px; max-height: 300px; overflow-y: auto;">${escapeHtml(deleteQuery)}</div>
            
            <div class="action-buttons" style="margin-top: 12px; display: flex; gap: 8px;">
                <button class="btn btn-primary btn-sm btn-query-action" data-action="copy-clipboard" data-query-index="delete-${rowIndex}">
                    <i class="material-icons icon-align" style="font-size: 14px;">content_copy</i>
                    Copy Query
                </button>
         
            </div>
        </div>
    `;
    
    showQueryActionModal('DELETE Query', content);
}

// =============================================
// HELPER FUNCTIONS
// =============================================

function formatSqlValue(value) {
    if (value === null || value === undefined) {
        return 'NULL';
    }
    
    const type = typeof value;
    if (type === 'string') {
        // ✅ CHỈ escape single quotes cho SQL, KHÔNG escape thêm gì
        // Vì query được lưu trong queryCache, không qua HTML attributes
        let escaped = value.replace(/'/g, "''");
        return `N'${escaped}'`;
    } else if (type === 'boolean') {
        return value ? '1' : '0';
    } else if (value instanceof Date) {
        return `'${value.toISOString()}'`;
    } else {
        return value.toString();
    }
}

function copyQueryToClipboard(query) {
    // ✅ KHÔNG CẦN decode - query đã ở dạng gốc trong cache
    navigator.clipboard.writeText(query).then(() => {
        showNotification('Đã copy query vào clipboard!', 'success');
    }).catch(() => {
        // Fallback: Dùng textarea
        const $temp = $('<textarea>');
        $('body').append($temp);
        $temp.val(query).select();
        document.execCommand('copy');
        $temp.remove();
        showNotification('Đã copy query vào clipboard!', 'success');
    });
}

function copyQueryToEditor(query) {
    // ✅ KHÔNG CẦN decode
    $('#queryEditor').val(query);
    closeQueryActionModal();
    showNotification('Đã copy query vào editor!', 'success');
}

function executeQueryDirect(query) {
    if (!confirm('Bạn có chắc muốn thực thi query này?')) return;
    
    // ✅ KHÔNG CẦN decode
    closeQueryActionModal();
    $('#queryEditor').val(query);
    
    // Execute after a short delay to allow modal to close
    setTimeout(() => {
        executeQuery();
    }, 300);
}

// =============================================
// QUERY ACTION MODAL
// =============================================

function showQueryActionModal(title, content) {
    $('#queryActionTitle').html(`<i class="material-icons icon-align">code</i> ${title}`);
    $('#queryActionContent').html(content);
    $('#queryActionModal').fadeIn();
    
    // ✅ Setup event delegation sau khi modal hiển thị
    setupQueryActionHandlers();
}

function closeQueryActionModal() {
    $('#queryActionModal').fadeOut();
}

// ✅ QUAN TRỌNG: Event delegation thay vì inline onclick
function setupQueryActionHandlers() {
    // Remove existing handlers để tránh duplicate
    $('#queryActionModal').off('click', '.btn-query-action');
    
    // Attach event delegation
    $('#queryActionModal').on('click', '.btn-query-action', function(e) {
        e.preventDefault();
        
        const $btn = $(this);
        const action = $btn.data('action');
        const queryIndex = $btn.data('query-index');
        const rowIndex = $btn.data('row-index');
        
        // Get query from cache
        const query = window.queryCache ? window.queryCache[queryIndex] : null;
        
        console.log('Button clicked:', { action, queryIndex, rowIndex, hasQuery: !!query });
        
        switch(action) {
            case 'copy-clipboard':
                if (query) {
                    copyQueryToClipboard(query);
                } else {
                    showNotification('Query không tồn tại!', 'danger');
                }
                break;
                
            case 'copy-editor':
                if (query) {
                    copyQueryToEditor(query);
                } else {
                    showNotification('Query không tồn tại!', 'danger');
                }
                break;
                
            case 'execute-query':
                if (query) {
                    executeQueryDirect(query);
                } else {
                    showNotification('Query không tồn tại!', 'danger');
                }
                break;
                
            case 'edit-direct':
                if (rowIndex !== undefined) {
                    openEditRowDirect(rowIndex);
                } else {
                    showNotification('Row index không hợp lệ!', 'danger');
                }
                break;
                
            case 'execute-delete':
                if (query && rowIndex !== undefined) {
                    executeDeleteDirect(rowIndex, query);
                } else {
                    showNotification('Thông tin không đầy đủ!', 'danger');
                }
                break;
                
            default:
                console.warn('Unknown action:', action);
        }
    });
}

// =============================================
// TABLE INFO FUNCTIONS
// =============================================

function showTableInfo(schema, table) {
    $.ajax({
        url: '/Admin/Base/DatabaseQuery/GetTableSchema',
        type: 'GET',
        data: { schemaName: schema, tableName: table },
        success: function(response) {
            if (response.output === 1) {
                displayTableInfo(schema, table, response.data);
            } else {
                showNotification(response.message, response.type);
            }
        },
        error: function() {
            showNotification('Lỗi khi lấy thông tin table!', 'danger');
        }
    });
}

function displayTableInfo(schema, table, columns) {
    let html = `
        <div style="padding: 20px;">
            <h3>[${schema}].[${table}]</h3>
            <hr/>
            <table class="table table-bordered" style="font-size: 13px;">
                <thead>
                    <tr style="background: #667eea; color: white;">
                        <th>Column</th>
                        <th>Type</th>
                        <th>Length</th>
                        <th>Nullable</th>
                        <th>PK</th>
                    </tr>
                </thead>
                <tbody>
    `;
    
    columns.forEach(col => {
        const length = col.maxLength ? col.maxLength : '-';
        const nullable = col.isNullable ? 'YES' : 'NO';
        const pk = col.isPrimaryKey ? '<i class="material-icons" style="color: gold; font-size: 16px;">vpn_key</i>' : '';
        
        html += `
            <tr>
                <td><strong>${col.columnName}</strong></td>
                <td>${col.dataType}</td>
                <td>${length}</td>
                <td>${nullable}</td>
                <td style="text-align: center;">${pk}</td>
            </tr>
        `;
    });
    
    html += `
                </tbody>
            </table>
            <div style="margin-top: 15px;">
                <button class="btn btn-info btn-sm" onclick="generateTableScript('${schema}', '${table}')">
                    <i class="material-icons" style="vertical-align: middle; font-size: 14px;">code</i> CREATE Script
                </button>
            </div>
        </div>
    `;
    
    showQueryActionModal('Table Information', html);
}

// =============================================
// INSERT TEMPLATE FOR NEW ROW
// =============================================

function showInsertOptions() {
    if (!currentEditSchema || !currentEditTable) {
        showNotification('Vui lòng chạy SELECT query trước!', 'warning');
        return;
    }
    
    const tableName = `[${currentEditSchema}].[${currentEditTable}]`;
    const columns = currentResult.columns;
    
    let insertQuery = `INSERT INTO ${tableName} (\n`;
    columns.forEach(col => {
        insertQuery += `    [${col}],\n`;
    });
    insertQuery = insertQuery.slice(0, -2) + '\n) VALUES (\n';
    
    columns.forEach(col => {
        insertQuery += `    NULL, -- ${col}\n`;
    });
    insertQuery = insertQuery.slice(0, -2) + '\n);';
    
    // ✅ Store query in cache
    if (!window.queryCache) window.queryCache = {};
    window.queryCache['insert-template'] = insertQuery;
    
    const content = `
        <div>
            <div style="background: #e7f3ff; padding: 10px 12px; border-left: 4px solid #2196F3; border-radius: 4px; margin-bottom: 12px;">
                <strong style="display: flex; align-items: center; gap: 6px; font-size: 14px;">
                    <i class="material-icons icon-align" style="font-size: 16px;">add</i> INSERT Template
                </strong>
                <p style="margin: 4px 0 0 0; font-size: 12px;">Template INSERT cho table [${currentEditSchema}].[${currentEditTable}]</p>
            </div>
            
            <div class="query-preview" style="background: #f5f5f5; padding: 10px; border-radius: 4px; font-family: monospace; font-size: 11px; max-height: 300px; overflow-y: auto;">${escapeHtml(insertQuery)}</div>
            
            <div class="action-buttons" style="margin-top: 12px; display: flex; gap: 8px;">
                <button class="btn btn-primary btn-sm btn-query-action" data-action="copy-clipboard" data-query-index="insert-template">
                    <i class="material-icons icon-align" style="font-size: 14px;">content_copy</i>
                    Copy to Clipboard
                </button>
                <button class="btn btn-info btn-sm btn-query-action" data-action="copy-editor" data-query-index="insert-template">
                    <i class="material-icons icon-align" style="font-size: 14px;">code</i>
                    Copy to Editor
                </button>
            </div>
        </div>
    `;
    
    showQueryActionModal('INSERT Template', content);
}

// =============================================
// FULL BACKUP FUNCTIONS
// =============================================

function openFullBackupModal() {
    $.ajax({
        url: '/Admin/Base/DatabaseQuery/GetTables',
        type: 'GET',
        success: function(response) {
            if (response.output === 1) {
                allTablesForBackup = response.data;
                renderBackupTablesList();
                $('#fullBackupModal').fadeIn();
            }
        }
    });
}

function closeFullBackupModal() {
    $('#fullBackupModal').fadeOut();
}

function renderBackupTablesList() {
    const searchTerm = ($('#backupSearchTable').val() || '').toLowerCase();
    let html = '';
    let totalCount = 0;
    
    allTablesForBackup.forEach((table, index) => {
        const fullName = `${table.schemaName}.${table.tableName}`.toLowerCase();
        
        if (searchTerm && !fullName.includes(searchTerm)) {
            return;
        }
        
        totalCount++;
        html += `
            <div class="backup-table-item">
                <label>
                    <div>
                        <input type="checkbox" name="IsBackupTable_${index}" id="IsBackupTable_${index}"
                               class="backup-table-checkbox filled-in chk-col-red"
                               data-schema="${table.schemaName}" 
                               data-table="${table.tableName}"
                               data-rows="${table.rowCount}"
                               onchange="updateBackupCount()" />
                        <span for="IsBackupTable_${index}" class="backup-table-name">${table.tableName}</span>
                        <span class="backup-table-schema">[${table.schemaName}]</span>
                    </div>
                    <span class="backup-table-rows">${table.rowCount.toLocaleString()} rows</span>
                </label>
            </div>
        `;
    });
    
    $('#backupTablesList').html(html);
    $('#totalTableCount').text(totalCount);
    updateBackupCount();
}

function filterBackupTables() {
    renderBackupTablesList();
}

function toggleSelectAllTables() {
    const isChecked = $('#selectAllTables').is(':checked');
    $('.backup-table-checkbox:visible').prop('checked', isChecked);
    updateBackupCount();
}

function updateBackupCount() {
    const selectedCount = $('.backup-table-checkbox:checked').length;
    $('#selectedTableCount').text(selectedCount);
}

function generateFullBackup() {
    const selectedTables = [];
    const rowLimit = parseInt($('#backupRowLimit').val()) || 10000;
    
    $('.backup-table-checkbox:checked').each(function() {
        selectedTables.push({
            schemaName: $(this).data('schema'),
            tableName: $(this).data('table'),
            rowLimit: rowLimit
        });
    });
    
    if (selectedTables.length === 0) {
        showNotification('Vui lòng chọn ít nhất 1 table!', 'warning');
        return;
    }
    
    if (!confirm(`Bạn có chắc muốn generate backup script cho ${selectedTables.length} tables?\nĐiều này có thể mất vài phút...`)) {
        return;
    }
    
    showNotification('Đang generate backup script... Vui lòng đợi!', 'info');
    closeFullBackupModal();
    
    $.ajax({
        url: '/Admin/Base/DatabaseQuery/GenerateFullBackupScript',
        type: 'POST',
        contentType: 'application/json',
        data: JSON.stringify({
            tables: selectedTables
        }),
        success: function(response) {
            if (response.output === 1) {
                showScript('Full Database Backup Script', response.data, response.fileName);
                showNotification('Generate backup thành công!', 'success');
            } else {
                showNotification(response.message, response.type);
            }
        },
        error: function() {
            showNotification('Lỗi khi generate backup!', 'danger');
        }
    });
}

// =============================================
// CELL CONTENT VIEWER
// =============================================

function showCellContent(content) {
    const html = `
        <div style="padding: 20px;">
            <div style="background: #f5f5f5; padding: 15px; border-radius: 4px; max-height: 400px; overflow-y: auto; white-space: pre-wrap; word-break: break-all;">
                ${escapeHtml(content)}
            </div>
            <div style="margin-top: 15px;">
                <button class="btn btn-info btn-sm" onclick="copyCellContent(\`${escapeHtml(content).replace(/`/g, '\\`')}\`)">
                    <i class="material-icons" style="vertical-align: middle; font-size: 14px;">content_copy</i> Copy
                </button>
            </div>
        </div>
    `;
    
    showQueryActionModal('Cell Content', html);
}

function copyCellContent(content) {
    navigator.clipboard.writeText(content).then(() => {
        showNotification('Đã copy nội dung!', 'success');
        closeQueryActionModal();
    });
}

// =============================================
// OVERRIDE displayResult TO ADD ROW ACTIONS
// =============================================

// ✅ Đảm bảo chỉ override 1 lần
let isDisplayResultOverridden = false;

$(document).ready(function() {
    // Chờ displayResult được định nghĩa trong Index.cshtml
    setTimeout(function() {
        if (typeof window.displayResult === 'function' && !isDisplayResultOverridden) {
            const originalDisplayResult = window.displayResult;
            isDisplayResultOverridden = true;
            
            window.displayResult = function(data) {
                // Gọi hàm gốc
                originalDisplayResult(data);
                
                // Kiểm tra nếu là SELECT query
                const query = $('#queryEditor').val().toUpperCase();
                if (query.includes('SELECT') && query.includes('FROM')) {
                    setTimeout(() => {
                        enableRowActions();
                        truncateLongCells();
                    }, 200);
                }
            };
            
            console.log('✅ displayResult override installed');
        }
    }, 500);
});

function truncateLongCells() {
    $('#resultTableWrapper table tbody td').each(function() {
        const $cell = $(this);
        const content = $cell.text();
        
        if (content.length > 100 && !$cell.find('.view-content-btn').length) {
            const truncated = content.substring(0, 100);
            $cell.html(`
                <span>${escapeHtml(truncated)}...</span>
                <a href="#" class="view-content-btn" onclick="showCellContent(\`${escapeHtml(content).replace(/`/g, '\\`')}\`); return false;" 
                   style="color: #667eea; text-decoration: none; margin-left: 5px; font-size: 11px;">
                    <i class="material-icons" style="font-size: 12px; vertical-align: middle;">visibility</i> View
                </a>
            `);
        }
    });
}

// =============================================
// OVERRIDE executeDeleteDirect
// =============================================

function executeDeleteDirect(rowIndex, query) {
    const rowData = currentResult.rows[rowIndex];
    const columns = currentResult.columns;
    
    // ✅ THAY ĐỔI: Build WHERE chỉ với Primary Key
    const whereConditions = buildPrimaryKeyConditions(rowData, columns);
    
    let confirmMsg = 'BẠN CÓ CHẮC MUỐN XÓA DÒNG NÀY?\n\nDữ liệu sẽ bị xóa vĩnh viễn!\n\n';
    columns.forEach((col, index) => {
        confirmMsg += `${col}: ${rowData[index]}\n`;
    });
    
    if (!confirm(confirmMsg)) return;
    
    closeQueryActionModal();
    
    $.ajax({
        url: '/Admin/Base/DatabaseQuery/DeleteRow',
        type: 'POST',
        contentType: 'application/json',
        data: JSON.stringify({
            schemaName: currentEditSchema,
            tableName: currentEditTable,
            whereConditions: whereConditions
        }),
        success: function(response) {
            if (response.output === 1) {
                showNotification(response.message, 'success');
                executeQuery();
            } else {
                showNotification(response.message, response.type);
            }
        },
        error: function() {
            showNotification('Lỗi khi xóa!', 'danger');
        }
    });
}
