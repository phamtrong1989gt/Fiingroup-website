# ?? Fix: Buttons Ch? Hi?n Th? Text

## ? V?n ??:

C?t Actions ch? hi?n th? **text "add INSERT ..."** thay vì **buttons có màu** v?i icon Material.

## ?? Nguyên Nhân:

1. ? **CSS không ???c apply** ? buttons không có style
2. ? **HTML b? render sai** ? inline `onclick=""` b? escape
3. ? **Browser cache** ? file JS c? v?n ???c s? d?ng
4. ? **Template string b? conflict** ? backticks không escape ?úng

## ? ?ã S?a:

### **1. Dùng jQuery createElement Thay Vì HTML String**

#### **Tr??c (SAI - Render HTML string):**
```javascript
$row.append(`
    <td>
        <button onclick="showInsertQueryForRow(${rowIndex})">INSERT</button>
    </td>
`);
// ? onclick b? escape ho?c không ho?t ??ng
```

#### **Sau (?ÚNG - Dùng jQuery):**
```javascript
// T?o cell
const $actionsCell = $('<td>', {
    style: 'text-align: center; padding: 8px;'
});

// T?o button INSERT
const $btnInsert = $('<button>', {
    class: 'row-action-btn btn-insert',
    title: 'Copy INSERT query',
    html: '<i class="material-icons">add</i> INSERT'
}).on('click', function() {
    showInsertQueryForRow(rowIndex); // ? Event handler tr?c ti?p
});

// Append button vào cell
$actionsCell.append($btnInsert);
$row.append($actionsCell);
```

**?u ?i?m:**
- ? Event handlers attach tr?c ti?p ? Không c?n inline onclick
- ? Không b? escape backticks
- ? DOM manipulation nhanh h?n
- ? D? debug h?n

---

### **2. Force Inject CSS Inline**

**V?n ??:** CSS trong `<style>` tag có th? b? override ho?c ch?a load k?p.

**Fix:** Inject CSS tr?c ti?p vào `<head>` khi `enableRowActions()` ch?y:

```javascript
$('head').append(`
    <style id="row-action-btn-styles">
        .row-action-btn {
            display: inline-flex !important;
            background: #2196F3 !important; /* Xanh d??ng */
            color: white !important;
            padding: 5px 10px !important;
            border-radius: 4px !important;
            /* ... */
        }
    </style>
`);
```

**L?i ích:**
- ? `!important` ??m b?o CSS ???c apply
- ? CSS load ngay khi buttons ???c t?o
- ? Không b? conflict v?i CSS khác

---

### **3. Thêm Logging Chi Ti?t**

```javascript
function enableRowActions() {
    console.log('?? enableRowActions() called');
    console.log('currentResult:', currentResult);
    
    if (!currentResult) {
        console.error('? Missing currentResult data');
        return;
    }
    
    console.log(`? Detected table: [${currentEditSchema}].[${currentEditTable}]`);
    console.log(`? Added ${buttonsAdded} action button sets`);
}
```

**Khi debug, check Console (F12):**
```
?? enableRowActions() called
currentResult: {columns: Array(10), rows: Array(5), ...}
? Detected table: [adm].[User]
? Added Actions header
? Added 5 action button sets (15 buttons total)
```

---

### **4. T?ng Version Cache**

**File:** `Index.cshtml`

```html
<!-- Tr??c: -->
<script src="~/js/database-query-extended.js?v=3"></script>

<!-- Sau: -->
<script src="~/js/database-query-extended.js?v=4"></script>
```

**Lý do:** Browser cache file JS c? ? code m?i không ch?y

---

## ?? Cách Test:

### **B??c 1: Clear Cache Hoàn Toàn**

```
1. Ctrl + Shift + Del
2. Ch?n "All time"
3. Check "Cached images and files"
4. Click "Clear data"
5. Close browser
6. Reopen browser
7. Ctrl + F5 (hard refresh)
```

---

### **B??c 2: Open Console (F12)**

```javascript
// Check jQuery loaded:
typeof $ // Should return "function"

// Check function exists:
typeof enableRowActions // Should return "function"

// Check Material Icons font:
$('link[href*="material-icons"]').length // Should return 1

// Force call enableRowActions:
enableRowActions();

// Check buttons rendered:
$('.row-action-btn').length // Should return s?_dòng × 3
```

---

### **B??c 3: Ch?y Query**

```sql
SELECT TOP 5 * FROM [adm].[User]
```

---

### **B??c 4: Check Console Logs**

**Ph?i th?y:**
```
?? enableRowActions() called
currentResult: {columns: Array(...), rows: Array(...)}
? Detected table: [adm].[User]
? Added Actions header
? Added 5 action button sets (15 buttons total)
? displayResult override installed
```

**KHÔNG ???c th?y:**
```
? Missing currentResult data
? Cannot detect table from query
?? Actions column already exists (2 columns), skipping...
```

---

### **B??c 5: Inspect Buttons**

**Chu?t ph?i vào button ? Inspect:**

**? Ph?i th?y:**
```html
<td style="text-align: center; white-space: nowrap; padding: 8px;">
    <button class="row-action-btn btn-insert" title="Copy INSERT query">
        <i class="material-icons">add</i> INSERT
    </button>
    <button class="row-action-btn btn-edit" title="Edit or gen UPDATE query">
        <i class="material-icons">edit</i> EDIT
    </button>
    <button class="row-action-btn btn-delete" title="Delete or gen DELETE query">
        <i class="material-icons">delete</i> DELETE
    </button>
</td>
```

**CSS computed (Styles tab):**
```css
.row-action-btn.btn-insert {
    background-color: rgb(33, 150, 243); /* ? Xanh d??ng */
    color: rgb(255, 255, 255); /* ? Tr?ng */
    display: inline-flex; /* ? Flex */
    padding: 5px 10px; /* ? Có padding */
}
```

**? KHÔNG ???c th?y:**
```html
<!-- ? SAI: Ch? có text, không có button tag -->
<td>add INSERT ... add INSERT ...</td>

<!-- ? SAI: onclick inline b? escape -->
<button onclick="showInsertQueryForRow(0)">...</button>
```

---

## ?? Troubleshooting:

### **L?i 1: V?n th?y text thay vì button**

**Nguyên nhân:** Browser cache ho?c override b? conflict

**Fix:**
```javascript
// Console:
localStorage.clear();
sessionStorage.clear();
location.reload(true);

// Ho?c m? Incognito mode
```

---

### **L?i 2: Buttons không có màu**

**Nguyên nhân:** CSS không load ho?c b? override

**Fix Console:**
```javascript
// Force inject CSS
$('head').append(`
    <style>
        .row-action-btn.btn-insert {
            background: #2196F3 !important;
            color: white !important;
        }
        .row-action-btn.btn-edit {
            background: #4CAF50 !important;
            color: white !important;
        }
        .row-action-btn.btn-delete {
            background: #f44336 !important;
            color: white !important;
        }
    </style>
`);

// Check CSS applied
$('.row-action-btn.btn-insert').css('background-color');
// Should return: "rgb(33, 150, 243)"
```

---

### **L?i 3: Icon không hi?n th?**

**Nguyên nhân:** Material Icons font ch?a load

**Check:**
```javascript
// Console:
$('link[href*="material-icons"]').length // Should be > 0
```

**Fix:** Thêm vào `_Admin.cshtml`:
```html
<link href="https://fonts.googleapis.com/icon?family=Material+Icons" rel="stylesheet">
```

---

### **L?i 4: Click button không ho?t ??ng**

**Nguyên nhân:** Event handler không attach

**Check Console:**
```javascript
// Click button th? công:
$('.row-action-btn.btn-insert').first().click();
// Ph?i g?i ???c showInsertQueryForRow()

// Ho?c check event:
$._data($('.row-action-btn.btn-insert')[0], 'events');
// Should show: {click: Array(1)}
```

**Fix:**
```javascript
// Re-attach events
$('.row-action-btn.btn-insert').off('click').on('click', function() {
    const rowIndex = $(this).closest('tr').index();
    showInsertQueryForRow(rowIndex);
});
```

---

## ?? Expected Result:

### **Before (? SAI):**
```
???????????????????????????????????????
?...?...?...? Actions                 ?
???????????????????????????????????????
?...?...?...? add INSERT ... ...      ? ? Ch? có text
?...?...?...? add INSERT ... ...      ?
???????????????????????????????????????
```

### **After (? ?ÚNG):**
```
???????????????????????????????????????????????????
?...?...?...? Actions                             ?
???????????????????????????????????????????????????
?...?...?...? [??INSERT] [??EDIT] [??DELETE]    ?
?...?...?...? [??INSERT] [??EDIT] [??DELETE]    ?
???????????????????????????????????????????????????
           ? Buttons có màu, icon, hover effect
```

---

## ?? Quick Test Script:

Paste vào Console (F12):

```javascript
// 1. Check setup
console.log('jQuery:', typeof $);
console.log('enableRowActions:', typeof enableRowActions);
console.log('currentResult:', currentResult);

// 2. Check buttons
const btnCount = $('.row-action-btn').length;
console.log(`Found ${btnCount} buttons`);

// 3. Check CSS
const insertBg = $('.row-action-btn.btn-insert').first().css('background-color');
console.log('INSERT button background:', insertBg);

// 4. Check Material Icons
const hasIcons = $('.row-action-btn i.material-icons').length > 0;
console.log('Has icons:', hasIcons);

// 5. Summary
if (btnCount > 0 && insertBg === 'rgb(33, 150, 243)' && hasIcons) {
    console.log('? ALL GOOD! Buttons rendered correctly!');
} else {
    console.error('? PROBLEMS FOUND!');
    console.log('- Buttons:', btnCount > 0 ? '?' : '?');
    console.log('- CSS:', insertBg === 'rgb(33, 150, 243)' ? '?' : '?');
    console.log('- Icons:', hasIcons ? '?' : '?');
}
```

**Expected output:**
```
jQuery: function
enableRowActions: function
currentResult: {columns: Array(10), rows: Array(5), ...}
Found 15 buttons
INSERT button background: rgb(33, 150, 243)
Has icons: true
? ALL GOOD! Buttons rendered correctly!
```

---

## ? Final Checklist:

- [ ] Ctrl + Shift + Del (clear cache)
- [ ] Ctrl + F5 (hard refresh)
- [ ] Console shows: "? displayResult override installed"
- [ ] Console shows: "? Added X action button sets"
- [ ] Buttons có màu (xanh d??ng/xanh lá/??)
- [ ] Icons Material hi?n th? (add/edit/delete)
- [ ] Hover ? buttons n?i lên + shadow
- [ ] Click INSERT ? Popup xanh d??ng
- [ ] Click EDIT ? Popup xanh lá
- [ ] Click DELETE ? Popup ??
- [ ] Console không có l?i ??

---

## ?? K?t Lu?n:

**Buttons gi? ???c render b?ng jQuery createElement thay vì HTML string:**
- ? Event handlers attach tr?c ti?p
- ? Không b? escape backticks
- ? CSS ???c force inject v?i `!important`
- ? Logging chi ti?t ?? debug

**Test ngay:** Ctrl + F5 ? SELECT query ? Check buttons! ??
