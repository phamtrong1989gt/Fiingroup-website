// ============================================
// TEMPLATE AUTO PARSER
// T? ??ng parse template HTML và sinh JSON
// ============================================
(function () {
    'use strict';

    /**
     * Parse template HTML và trích xu?t t?t c? placeholders
     * @param {string} template - Template HTML
     * @returns {object} - { forLoops: {...}, fields: [...] }
     */
    function parseTemplate(template) {
        const result = {
            forLoops: {},  // { "For": [...fields], "ForIndicator": [...fields] }
            globalFields: []  // Fields ngoài For loops
        };

        // 1. Tìm t?t c? For loops: [For], [ForIndicator], [ForSlide], etc.
        const forLoopRegex = /\[([A-Za-z0-9_]+)\]([\s\S]*?)\[\/\1\]/g;
        let forMatch;
        
        while ((forMatch = forLoopRegex.exec(template)) !== null) {
            const loopName = forMatch[1];
            const loopContent = forMatch[2];
            
            // Ch? x? lý các loop b?t ??u b?ng "For"
            if (loopName.startsWith('For') || loopName === 'For') {
                const fields = extractFields(loopContent);
                result.forLoops[loopName] = fields;
            }
        }

        // 2. Tìm global fields (ngoài For loops)
        // Remove all For loop content first
        let templateWithoutLoops = template.replace(/\[For[A-Za-z0-9_]*\][\s\S]*?\[\/For[A-Za-z0-9_]*\]/g, '');
        result.globalFields = extractFields(templateWithoutLoops);

        return result;
    }

    /**
     * Trích xu?t t?t c? field placeholders t? text
     * @param {string} text - Text ch?a placeholders
     * @returns {array} - Danh sách field names
     */
    function extractFields(text) {
        const fields = [];
        const fieldRegex = /\[([a-zA-Z][a-zA-Z0-9_]*)\]/g;
        let match;

        while ((match = fieldRegex.exec(text)) !== null) {
            const fieldName = match[1];
            // Lo?i b? các keywords nh? For, ForIndicator, etc.
            if (!fieldName.startsWith('For') && !fields.includes(fieldName)) {
                fields.push(fieldName);
            }
        }

        return fields;
    }

    /**
     * Sinh JSON structure t? parsed template
     * @param {object} parsedTemplate - K?t qu? t? parseTemplate()
     * @param {number} itemsPerLoop - S? items m?c ??nh cho m?i For loop (default: 2)
     * @returns {object} - JSON structure
     */
    function generateJSON(parsedTemplate, itemsPerLoop = 2) {
        const result = {};

        // 1. Thêm global fields
        parsedTemplate.globalFields.forEach(field => {
            result[field] = '';
        });

        // 2. Thêm For loops
        Object.keys(parsedTemplate.forLoops).forEach(loopName => {
            const fields = parsedTemplate.forLoops[loopName];
            const items = [];

            for (let i = 1; i <= itemsPerLoop; i++) {
                const item = {
                    index: i
                };

                // Thêm t?t c? fields vào item
                fields.forEach(field => {
                    item[field] = '';
                });

                // Set activeClass cho item ??u tiên
                if (i === 1 && fields.includes('activeClass')) {
                    item.activeClass = 'active';
                }

                // Set ariaCurrent cho item ??u tiên
                if (i === 1 && fields.includes('ariaCurrent')) {
                    item.ariaCurrent = 'aria-current="true"';
                }

                // Set slideIndex và slideIndexZero n?u có
                if (fields.includes('slideIndex')) {
                    item.slideIndex = i.toString();
                }
                if (fields.includes('slideIndexZero')) {
                    item.slideIndexZero = (i - 1).toString();
                }

                items.push(item);
            }

            result[loopName] = items;
        });

        return result;
    }

    /**
     * Auto generate JSON from template HTML
     * @param {string} template - Template HTML
     * @param {number} itemsPerLoop - S? items m?c ??nh cho m?i For loop
     * @returns {string} - JSON string (formatted)
     */
    function autoGenerateJSON(template, itemsPerLoop = 2) {
        try {
            const parsed = parseTemplate(template);
            const json = generateJSON(parsed, itemsPerLoop);
            return JSON.stringify(json, null, 2);
        } catch (error) {
            console.error('Error auto-generating JSON:', error);
            return JSON.stringify({ error: error.message }, null, 2);
        }
    }

    /**
     * Validate JSON structure v?i template
     * @param {string} template - Template HTML
     * @param {object} json - JSON object
     * @returns {object} - { valid: boolean, errors: [...], warnings: [...] }
     */
    function validateJSON(template, json) {
        const result = {
            valid: true,
            errors: [],
            warnings: []
        };

        const parsed = parseTemplate(template);

        // 1. Check global fields
        parsed.globalFields.forEach(field => {
            if (!(field in json)) {
                result.warnings.push(`Global field "${field}" not found in JSON`);
            }
        });

        // 2. Check For loops
        Object.keys(parsed.forLoops).forEach(loopName => {
            if (!json[loopName]) {
                result.errors.push(`For loop "${loopName}" not found in JSON`);
                result.valid = false;
                return;
            }

            if (!Array.isArray(json[loopName])) {
                result.errors.push(`"${loopName}" must be an array`);
                result.valid = false;
                return;
            }

            // Check fields trong m?i item
            const requiredFields = parsed.forLoops[loopName];
            json[loopName].forEach((item, index) => {
                requiredFields.forEach(field => {
                    if (!(field in item)) {
                        result.warnings.push(`Field "${field}" not found in ${loopName}[${index}]`);
                    }
                });

                // Check index field
                if (!('index' in item)) {
                    result.warnings.push(`Field "index" recommended in ${loopName}[${index}]`);
                }
            });
        });

        return result;
    }

    /**
     * Show parsed template info (for debugging)
     * @param {string} template - Template HTML
     * @returns {string} - Formatted info string
     */
    function showTemplateInfo(template) {
        const parsed = parseTemplate(template);
        let info = '=== TEMPLATE ANALYSIS ===\n\n';

        info += 'Global Fields:\n';
        if (parsed.globalFields.length === 0) {
            info += '  (none)\n';
        } else {
            parsed.globalFields.forEach(field => {
                info += `  - [${field}]\n`;
            });
        }

        info += '\nFor Loops:\n';
        if (Object.keys(parsed.forLoops).length === 0) {
            info += '  (none)\n';
        } else {
            Object.keys(parsed.forLoops).forEach(loopName => {
                info += `  [${loopName}]\n`;
                parsed.forLoops[loopName].forEach(field => {
                    info += `    - [${field}]\n`;
                });
            });
        }

        return info;
    }

    // =============================================
    // EXPORT PUBLIC API
    // =============================================
    window.TemplateAutoParser = {
        parseTemplate: parseTemplate,
        generateJSON: generateJSON,
        autoGenerateJSON: autoGenerateJSON,
        validateJSON: validateJSON,
        showTemplateInfo: showTemplateInfo
    };

    console.log('? TemplateAutoParser loaded');

})();
