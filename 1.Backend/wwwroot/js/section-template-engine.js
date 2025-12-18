(function () {
    'use strict';

    // ===========================================
    // CONFIGURATION
    // ===========================================
    const sectionConfigs = {};
    const boundSections = new Set(); // Track đã bind chưa

    // ===========================================
    // KHỞI TẠO - Bind events cho từng section riêng lẻ
    // ===========================================
    function bindSectionEvents(sectionKey, config) {
        // Nếu đã bind rồi thì skip
        if (boundSections.has(sectionKey)) {
            console.log('Section already bound: ' + sectionKey);
            return;
        }
        
        console.log('Binding events for section: ' + sectionKey, config);
        
        // Add Field Button - Unbind trước để tránh duplicate
        $(`#${config.addFieldBtnId}`).off('click').on('click', function () {
            console.log('Add field clicked for section: ' + sectionKey);
            addFieldToAllItems(config);
        });
        
        // Add Item Button - Unbind trước để tránh duplicate
        $(`#${config.addItemBtnId}`).off('click').on('click', function () {
            console.log('Add item clicked for section: ' + sectionKey);
            addNewItem(config);
        });
        
        // Mark as bound
        boundSections.add(sectionKey);
    }

    // ===========================================
    // CHỨC NĂNG: Thêm field mới vào tất cả items
    // ===========================================
    function addFieldToAllItems(config) {
        const fieldName = $(`#${config.fieldNameInputId}`).val().trim();
        
        if (!fieldName) {
            showNotification('warning', 'Vui lòng nhập tên field!');
            return;
        }
        
        const valuesStr = $(`#${config.valuesId}`).val().trim();
        
        if (!valuesStr) {
            showNotification('warning', 'Chưa có dữ liệu JSON!');
            return;
        }
        
        try {
            const values = JSON.parse(valuesStr);
            
            // Kiểm tra cấu trúc: object với For loops hay array
            if (typeof values === 'object' && !Array.isArray(values)) {
                // Cấu trúc mới: { "For": [...], "ForIndicator": [...] }
                const forLoopName = $(`#${config.forLoopSelectId}`).val() || 'For';
                
                if (!values[forLoopName] || !Array.isArray(values[forLoopName])) {
                    showNotification('error', `"${forLoopName}" không tồn tại hoặc không phải array!`);
                    return;
                }
                
                // Add field vào tất cả items của For loop được chọn
                values[forLoopName].forEach(item => {
                    if (!item[fieldName]) {
                        item[fieldName] = '';
                    }
                });
                
                $(`#${config.valuesId}`).val(JSON.stringify(values, null, 2));
                $(`#${config.fieldNameInputId}`).val('');
                showNotification('success', `Đã thêm field "${fieldName}" vào tất cả items trong "${forLoopName}"!`);
                
            } else if (Array.isArray(values)) {
                // Cấu trúc cũ: array trực tiếp
                values.forEach(item => {
                    if (!item[fieldName]) {
                        item[fieldName] = '';
                    }
                });
                
                $(`#${config.valuesId}`).val(JSON.stringify(values, null, 2));
                $(`#${config.fieldNameInputId}`).val('');
                showNotification('success', `Đã thêm field "${fieldName}" vào tất cả items!`);
                
            } else {
                showNotification('error', 'JSON phải là array hoặc object chứa các For loops!');
            }
            
        } catch (e) {
            showNotification('error', 'Lỗi: ' + e.message);
        }
    }

    // ===========================================
    // CHỨC NĂNG: Thêm item mới vào JSON
    // ===========================================
    function addNewItem(config) {
        const valuesStr = $(`#${config.valuesId}`).val().trim();
        
        if (!valuesStr) {
            showNotification('warning', 'Chưa có dữ liệu JSON! Tạo cấu trúc đầu tiên...');
            // Tạo cấu trúc đầu tiên với For loop
            const newStructure = {
                "For": [
                    {
                        "index": 1,
                        "field1": "",
                        "field2": ""
                    }
                ]
            };
            $(`#${config.valuesId}`).val(JSON.stringify(newStructure, null, 2));
            showNotification('success', 'Đã tạo cấu trúc đầu tiên với For loop!');
            return;
        }
        
        try {
            const values = JSON.parse(valuesStr);
            
            // Kiểm tra cấu trúc: object với For loops hay array
            if (typeof values === 'object' && !Array.isArray(values)) {
                // Cấu trúc mới: { "For": [...], "ForIndicator": [...] }
                const forLoopName = $(`#${config.forLoopSelectId}`).val() || 'For';
                
                if (!values[forLoopName]) {
                    // Tạo For loop mới nếu chưa tồn tại
                    values[forLoopName] = [];
                    showNotification('info', `Đã tạo For loop mới: "${forLoopName}"`);
                }
                
                if (!Array.isArray(values[forLoopName])) {
                    showNotification('error', `"${forLoopName}" phải là một array!`);
                    return;
                }
                
                // Tạo item mới
                let newItem = {
                    "index": values[forLoopName].length + 1
                };
                
                // Clone structure từ item đầu tiên (nếu có)
                if (values[forLoopName].length > 0) {
                    Object.keys(values[forLoopName][0]).forEach(key => {
                        if (key !== 'index') {
                            newItem[key] = '';
                        }
                    });
                } else {
                    // Item đầu tiên, tạo structure mặc định
                    newItem.field1 = '';
                    newItem.field2 = '';
                }
                
                values[forLoopName].push(newItem);
                $(`#${config.valuesId}`).val(JSON.stringify(values, null, 2));
                showNotification('success', `Đã thêm item mới vào "${forLoopName}" (tổng: ${values[forLoopName].length} items)`);
                
            } else if (Array.isArray(values)) {
                // Cấu trúc cũ: array trực tiếp (backward compatible)
                let newItem = {};
                
                if (values.length > 0) {
                    // Clone structure từ item đầu tiên
                    Object.keys(values[0]).forEach(key => {
                        newItem[key] = '';
                    });
                } else {
                    // Nếu chưa có item nào, tạo item mặc định
                    newItem = {
                        field1: '',
                        field2: ''
                    };
                }
                
                values.push(newItem);
                $(`#${config.valuesId}`).val(JSON.stringify(values, null, 2));
                showNotification('success', `Đã thêm item mới (tổng: ${values.length} items)`);
                
            } else {
                showNotification('error', 'JSON phải là array hoặc object chứa các For loops!');
            }
            
        } catch (e) {
            showNotification('error', 'Lỗi: ' + e.message);
        }
    }

    // ===========================================
    // CHỨC NĂNG: Parse For loops từ template
    // ===========================================
    function parseForLoopsFromTemplate(template) {
        const forLoops = [];
        // Match [For], [ForIndicator], [ForSlide], etc.
        const regex = /\[([A-Za-z0-9_]+)\]/g;
        let match;
        
        while ((match = regex.exec(template)) !== null) {
            const loopName = match[1];
            // Kiểm tra xem có phải là For loop không (bắt đầu bằng "For")
            if (loopName.startsWith('For') && !forLoops.includes(loopName)) {
                forLoops.push(loopName);
            }
        }
        
        return forLoops;
    }

    // ===========================================
    // CHỨC NĂNG: Auto-detect For loops và tạo dropdown selector
    // ===========================================
    function createForLoopSelector(config, template) {
        const forLoops = parseForLoopsFromTemplate(template);
        
        if (forLoops.length > 1) {
            // Có nhiều For loops, tạo dropdown selector
            const selectorHtml = `
                <div class="form-group mt-2">
                    <label>Chọn For Loop:</label>
                    <select id="${config.forLoopSelectId}" class="form-control">
                        ${forLoops.map(loop => `<option value="${loop}">${loop}</option>`).join('')}
                    </select>
                </div>
            `;
            
            // Insert selector trước Add Field button
            $(`#${config.addFieldBtnId}`).closest('.form-group').before(selectorHtml);
            showNotification('info', `Phát hiện ${forLoops.length} For loops: ${forLoops.join(', ')}`);
        }
    }

    // ===========================================
    // HELPER: Show notification
    // ===========================================
    function showNotification(type, message) {
        const colors = {
            success: '#4caf50',
            error: '#f44336',
            warning: '#ff9800',
            info: '#2196f3'
        };
        
        const color = colors[type] || '#333';
        
        const notification = $(`
            <div style="position: fixed; top: 20px; right: 20px; background: ${color}; color: white; padding: 15px 20px; border-radius: 4px; z-index: 9999; box-shadow: 0 4px 6px rgba(0,0,0,0.2); max-width: 400px;">
                <strong>${type.toUpperCase()}:</strong> ${message}
            </div>
        `);
        
        $('body').append(notification);
        
        setTimeout(() => {
            notification.fadeOut(300, function () {
                $(this).remove();
            });
        }, 3000);
    }

    // ===========================================
    // EXPORT
    // ===========================================
    window.SectionTemplateEngine = {
        /**
         * Thêm section mới
         * @param {string} sectionKey - Unique key cho section
         * @param {object} config - Cấu hình section
         * @param {string} config.templateId - ID của textarea template
         * @param {string} config.valuesId - ID của textarea values
         * @param {string} config.quickTemplateSelector - Selector cho quick template buttons
         * @param {string} config.addFieldBtnId - ID của button thêm field
         * @param {string} config.addItemBtnId - ID của button thêm item
         * @param {string} config.fieldNameInputId - ID của input nhập tên field
         * @param {string} config.forLoopSelectId - ID của dropdown chọn For loop
         * @param {string} [config.templateSource='SectionTemplates1'] - Tên biến global chứa templates
         */
        addSection: function(sectionKey, config) {
            // Set default template source nếu không có
            if (!config.templateSource) {
                config.templateSource = 'SectionTemplates1';
            }
            
            // Set default forLoopSelectId
            if (!config.forLoopSelectId) {
                config.forLoopSelectId = `forLoopSelect_${sectionKey}`;
            }
            
            sectionConfigs[sectionKey] = config;
            
            // Bind events cho section này
            bindSectionEvents(sectionKey, config);
            
            console.log('✓ Section registered:', sectionKey);
        },
        
        /**
         * Lấy danh sách templates từ một source
         * @param {string} sourceName - Tên biến global (e.g., 'SectionTemplates1')
         */
        getTemplates: function(sourceName) {
            return window[sourceName] || {};
        },
        
        /**
         * Liệt kê tất cả template sources có sẵn
         */
        listTemplateSources: function() {
            const sources = [];
            for (let key in window) {
                if (key.startsWith('SectionTemplates')) {
                    sources.push(key);
                }
            }
            return sources;
        },
        
        /**
         * Parse For loops từ template và tạo selector (nếu cần)
         * @param {string} sectionKey - Key của section
         * @param {string} template - Template HTML
         */
        setupForLoopSelector: function(sectionKey, template) {
            const config = sectionConfigs[sectionKey];
            if (config) {
                createForLoopSelector(config, template);
            }
        }
    };

})();
