// ============================================
// CODEMIRROR EDITOR MANAGER
// Global utility for creating and managing CodeMirror editors
// ============================================
(function() {
    'use strict';

    /**
     * Create multiple CodeMirror editors with HTML/JSON modes
     * @param {Array} idsHtml - Array of textarea IDs/names for HTML mode
     * @param {Array} idsJson - Array of textarea IDs/names for JSON mode
     * @param {Object} globalOptions - Optional global CodeMirror options
     * @returns {Object} - Editor manager with methods
     */
    window.createEditors = function(idsHtml = [], idsJson = [], globalOptions = {}) {
        const instances = {};

        const defaultOptions = {
            lineNumbers: true,
            matchBrackets: true,
            lineWrapping: true,
            autoRefresh: true,
            theme: "default",
            indentUnit: 2,
            tabSize: 2,
            indentWithTabs: false
        };

        // Helper function to create editor
        function createEditor(id, mode) {
            // Try to get element by ID first
            let ta = document.getElementById(id);
            
            // If not found by ID, try to get by name attribute
            if (!ta) {
                ta = document.querySelector(`textarea[name="${id}"]`);
            }
            
            if (!ta || ta.nodeName.toLowerCase() !== "textarea") {
                console.warn(`createEditors: textarea not found for ${id}`);
                return;
            }
            
            try {
                const opts = Object.assign({}, defaultOptions, globalOptions[id] || {}, { mode: mode });
                instances[id] = CodeMirror.fromTextArea(ta, opts);
                
                // Auto format JSON on init
                if (mode && mode.name === "application/json") {
                    try {
                        const value = instances[id].getValue();
                        if (value.trim()) {
                            const formatted = JSON.stringify(JSON.parse(value), null, 2);
                            instances[id].setValue(formatted);
                        }
                    } catch (e) {
                        console.log('JSON format skipped for ' + id + ': ' + e.message);
                    }
                }
                
                console.log(`? CodeMirror editor created for ${id}`);
            } catch (err) {
                console.error(`createEditors: failed to init editor for ${id}`, err);
            }
        }

        // Create HTML editors
        idsHtml.forEach(id => createEditor(id, "text/html"));
        
        // Create JSON editors
        idsJson.forEach(id => createEditor(id, { name: "application/json", jsonMode: true }));

        return {
            instances,
            
            /**
             * Get editor instance by ID
             */
            get(id) {
                return instances[id];
            },
            
            /**
             * Refresh all editors (useful after showing hidden elements)
             */
            refreshAll() {
                Object.values(instances).forEach(ed => ed && ed.refresh && ed.refresh());
            },
            
            /**
             * Sync all editors back to textareas (CRITICAL for form submit)
             */
            syncAllToTextareas() {
                console.log('?? Syncing all CodeMirror editors to textareas...');
                let syncedCount = 0;
                
                Object.entries(instances).forEach(([id, ed]) => {
                    try {
                        let ta = document.getElementById(id);
                        if (!ta) {
                            ta = document.querySelector(`textarea[name="${id}"]`);
                        }
                        if (ta && ed) {
                            const value = ed.getValue();
                            ta.value = value;
                            syncedCount++;
                            console.log(`  ? Synced ${id} (${value.length} chars)`);
                        }
                    } catch (err) {
                        console.error(`  ? Failed to sync ${id}:`, err);
                    }
                });
                
                console.log(`? Synced ${syncedCount}/${Object.keys(instances).length} editors`);
            },
            
            /**
             * Dispose all editors
             */
            disposeAll() {
                Object.entries(instances).forEach(([id, ed]) => {
                    try {
                        if (ed && ed.toTextArea) ed.toTextArea();
                    } catch (err) { /* ignore */ }
                });
                for (const k in instances) delete instances[k];
            },
            
            /**
             * Format JSON for a specific editor
             */
            formatJson(id) {
                const ed = instances[id];
                if (!ed) return false;
                
                try {
                    const value = ed.getValue();
                    const formatted = JSON.stringify(JSON.parse(value), null, 2);
                    ed.setValue(formatted);
                    return true;
                } catch (e) {
                    console.error('Invalid JSON for editor ' + id + ': ' + e.message);
                    return false;
                }
            }
        };
    };

    console.log('? createEditors() globally available');

})();
