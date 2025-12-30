# ?? Fix Cu?i Cùng: Event Delegation + Data Attributes

## ? V?n ?? Tr??c ?ây:

### **Cách 1: Inline onclick v?i Escape** (FAILED)
```javascript
const escapedQuery = escapeForJavaScript(query);
const html = `<button onclick="copyQuery(\`${escapedQuery}\`)">Copy</button>`;
```

**T?i sao fail:**
1. Browser **t? ??ng decode HTML entities** tr??c khi parse JavaScript
2. Double-escaping gây ph?c t?p và d? l?i
3. Special characters trong SQL (??c bi?t JSON) v?n break syntax

---

## ? Gi?i Pháp M?i: Event Delegation + Data Attributes

### **Ki?n Trúc M?i:**

```
???????????????????????????????????
? 1. Generate Query (Raw)         ?
?    - No escaping needed         ?
?    - Store in queryCache object ?
???????????????????????????????????
           ?
           ?
???????????????????????????????????
? 2. Create Button v?i Data Attrs?
?    <button                      ?
?      data-action="copy"         ?
?      data-query-index="ins-1">  ?
???????????????????????????????????
           ?
           ?
???????????????????????????????????
? 3. Event Delegation             ?
?    $(modal).on('click',         ?
?      '.btn-query-action')       ?
???????????????????????????????????
           ?
           ?
???????????????????????????????????
? 4. Get Query from Cache         ?
?    const query =                ?
?      queryCache[queryIndex]     ?
???????????????????????????????????
           ?
           ?
???????????????????????????????????
? 5. Use Query Directly           ?
?    - Copy to clipboard          ?
?    - Execute query              ?
?    - No decoding needed!        ?
???????????????????????????????????
```

---

## ?? Implementation:

### **1. Store Query in Global Cache**

```javascript
function showInsertQueryForRow(rowIndex) {
    // ... generate insertQuery ...
    
    // ? Store raw query, NO ESCAPE!
    if (!window.queryCache) window.queryCache = {};
    window.queryCache[`insert-${rowIndex}`] = insertQuery;
    
    // Button with data attributes only
    const content = `
        <button 
            class="btn-copy btn-query-action" 
            data-action="copy-clipboard" 
            data-query-index="insert-${rowIndex}">
            Copy
        </button>
    `;
    
    showQueryActionModal('INSERT Query', content);
}
```

**Key points:**
- ? Query stored as-is (no escaping)
- ? Button có `data-*` attributes
- ? Không có inline `onclick`
- ? Query index as identifier

---

### **2. Event Delegation Setup**

```javascript
function setupQueryActionHandlers() {
    // Remove existing handlers
    $('#queryActionModal').off('click', '.btn-query-action');
    
    // Attach event delegation
    $('#queryActionModal').on('click', '.btn-query-action', function(e) {
        e.preventDefault();
        
        const $btn = $(this);
        const action = $btn.data('action');         // 'copy-clipboard'
        const queryIndex = $btn.data('query-index'); // 'insert-1'
        
        // Get query from cache
        const query = window.queryCache[queryIndex];
        
        // Execute action
        switch(action) {
            case 'copy-clipboard':
                copyQueryToClipboard(query);
                break;
            case 'copy-editor':
                copyQueryToEditor(query);
                break;
            case 'execute-query':
                executeQueryDirect(query);
                break;
        }
    });
}
```

**Advantages:**
- ? Single event listener for all buttons
- ? Easy to add new actions
- ? No inline JavaScript
- ? Works with dynamically added buttons

---

### **3. Simplified Helper Functions**

```javascript
function copyQueryToClipboard(query) {
    // ? Use query directly, NO DECODE!
    navigator.clipboard.writeText(query).then(() => {
        showNotification('?ã copy query vào clipboard!', 'success');
    });
}

function copyQueryToEditor(query) {
    // ? Use query directly
    $('#queryEditor').val(query);
    closeQueryActionModal();
}

function executeQueryDirect(query) {
    // ? Use query directly
    $('#queryEditor').val(query);
    executeQuery();
}
```

**Simplified:**
- ? NO `escapeForJavaScript()`
- ? NO decode functions
- ? NO double-escaping
- ? Just use raw query!

---

## ?? Before vs After:

### **Before (With Inline onclick):**

```javascript
// Generate query
let query = `UPDATE [Log] SET [Data] = N'{"UserId":1,"Name":"O'Brien"}'`;

// ? Step 1: Escape for SQL
query = query.replace(/'/g, "''");

// ? Step 2: Escape for JavaScript
query = query.replace(/\\/g, '\\\\')
             .replace(/`/g, '\\`')
             .replace(/"/g, '\\"')
             .replace(/'/g, "\\'");

// ? Step 3: Inject into onclick
const html = `<button onclick="copy(\`${query}\`)">Copy</button>`;
// Result: Syntax error ho?c query b? break!

// ? Step 4: Must decode when using
const decodedQuery = query.replace(/\\"/g, '"')...
```

**Problems:**
- ?? Quá ph?c t?p
- ?? D? có bugs
- ?? V?n break v?i special characters
- ?? Ph?i decode l?i khi dùng

---

### **After (With Event Delegation):**

```javascript
// Generate query
let query = `UPDATE [Log] SET [Data] = N'{"UserId":1,"Name":"O'Brien"}'`;

// ? Step 1: Escape ONLY for SQL
query = formatSqlValue(value); // Ch? replace ' ? ''

// ? Step 2: Store in cache
window.queryCache['update-1'] = query;

// ? Step 3: Create button
const html = `<button class="btn-query-action" data-query-index="update-1">Copy</button>`;

// ? Step 4: Use directly
$('#modal').on('click', '.btn-query-action', function() {
    const query = window.queryCache[$(this).data('query-index')];
    copyToClipboard(query); // Use as-is!
});
```

**Benefits:**
- ? ??n gi?n, d? hi?u
- ? Không có bugs
- ? Works v?i m?i special characters
- ? Không c?n decode

---

## ?? Test Cases:

### **Test 1: JSON v?i Double Quotes**

**Data:**
```json
{"UserId":1,"DisplayName":"Qu?n tr? h? th?ng","Email":"admin@test.com"}
```

**Expected:**
- ? Store in cache: `window.queryCache['insert-1']`
- ? Button: `<button data-query-index="insert-1">Copy</button>`
- ? Click ? Get from cache ? Copy directly
- ? Paste ? Query ?úng format
- ? Execute ? Success!

---

### **Test 2: Single Quote + JSON**

**Data:**
```
{"Name":"O'Brien","Path":"C:\\Users\\Test"}
```

**Expected SQL:**
```sql
N'{"Name":"O''Brien","Path":"C:\\Users\\Test"}'
   ? Ch? single quote ???c escape
```

**Test:**
1. Click [EDIT]
2. Check `window.queryCache['update-1']`
3. Click "Copy to Clipboard"
4. Paste ? ? Query ?úng
5. Execute ? ? Success!

---

### **Test 3: Complex JSON Array**

**Data:**
```json
[{"id":1,"tags":["test","dev"]},{"id":2,"data":"<html>"}]
```

**Expected:**
- ? NO escape cho `[`, `]`, `<`, `>`
- ? CH? escape `'` trong SQL string
- ? Copy & execute OK

---

## ?? Debug Commands:

```javascript
// Console:

// 1. Check query cache
console.log(window.queryCache);
// Expected: {
//   'insert-1': 'INSERT INTO...',
//   'update-2': 'UPDATE...',
//   ...
// }

// 2. Check button data
$('.btn-query-action').first().data();
// Expected: {action: 'copy-clipboard', queryIndex: 'insert-1'}

// 3. Test get query
const queryIndex = $('.btn-query-action').first().data('query-index');
const query = window.queryCache[queryIndex];
console.log(query);
// Expected: Raw query string

// 4. Test copy
copyQueryToClipboard(query);
// Paste and check ? Should be exact same as query

// 5. Check event handler
$._data($('#queryActionModal')[0], 'events');
// Expected: {click: Array(1)} with '.btn-query-action' selector
```

---

## ? Final Checklist:

- [ ] `window.queryCache` object created
- [ ] Queries stored v?i unique keys (insert-1, update-2, etc.)
- [ ] Buttons có `data-action` và `data-query-index`
- [ ] Event delegation setup v?i `setupQueryActionHandlers()`
- [ ] `copyQueryToClipboard()` không decode
- [ ] `copyQueryToEditor()` không decode
- [ ] `executeQueryDirect()` không decode
- [ ] Test v?i JSON data ? ? OK
- [ ] Test v?i single quotes ? ? OK
- [ ] Test v?i backslashes ? ? OK
- [ ] Test v?i HTML tags ? ? OK
- [ ] Copy to clipboard ? ? Exact same
- [ ] Execute query ? ? Success

---

## ?? Why This Works:

### **Key Insight:**

```
Inline onclick:
  Query ? Escape ? HTML attribute ? Browser decode ? Parse JS ? Break ?

Data attributes:
  Query ? Store in JS object ? Get from object ? Use directly ? Success ?
```

**The problem was:**
- HTML attributes go through **HTML parser**
- HTML parser **decodes entities** before passing to JS parser
- This breaks escaped quotes and special characters

**The solution:**
- Store query in **JavaScript object** (not HTML)
- Pass only **identifier** through HTML (data-query-index)
- Retrieve query from object when needed
- **No parsing/decoding needed!**

---

## ?? Test Ngay:

1. **Ctrl + F5** (hard refresh)
2. Ch?y query có JSON:
```sql
SELECT TOP 5 * FROM [adm].[Log]
WHERE [AcctionUser] LIKE '%{%'
```
3. Click **[EDIT]** button
4. Console check:
```javascript
window.queryCache['update-0']
// Should show raw UPDATE query
```
5. Click **"Copy to Clipboard"**
6. Paste vào SQL Management Studio
7. ? **Execute thành công!** Không l?i!

---

## ?? K?t Lu?n:

**Gi?i pháp cu?i cùng:**
- ? Event delegation + data attributes
- ? Query stored in JavaScript object
- ? NO inline onclick
- ? NO escape hell
- ? NO decode needed
- ? Works v?i m?i ký t? ??c bi?t

**This is the RIGHT way!** ??

---

## ?? References:

**Event Delegation Pattern:**
- jQuery: https://learn.jquery.com/events/event-delegation/
- MDN: https://developer.mozilla.org/en-US/docs/Learn/JavaScript/Building_blocks/Events#event_delegation

**Data Attributes:**
- HTML5: https://developer.mozilla.org/en-US/docs/Learn/HTML/Howto/Use_data_attributes
- jQuery: https://api.jquery.com/data/

**Why Inline onclick is Bad:**
- https://developer.mozilla.org/en-US/docs/Learn/JavaScript/Building_blocks/Events#inline_event_handlers_%E2%80%94_dont_use_these
- Content Security Policy issues
- Hard to maintain
- Escaping hell

**Best Practice: Unobtrusive JavaScript**
- Separate behavior (JS) from structure (HTML)
- Use event delegation
- Store data in JS objects, not HTML attributes
