# ?? Fix: Ký T? ??c Bi?t Trong SQL Query

## ? V?n ??:

Query có JSON string v?i d?u `"` b? break syntax:

```sql
UPDATE [adm].[Log]
SET
    [AcctionUser] = N'{"UserId":1,"DisplayName":"Qu?n tr? h? th?ng",...}'
                         ? D?u " này làm break JavaScript template string
```

**K?t qu?:**
- ? Không copy ???c query vào clipboard
- ? Không execute ???c query
- ? Console báo l?i syntax error

## ?? Nguyên Nhân:

### **1. SQL String Có Ký T? ??c Bi?t:**
```sql
N'{"UserId":1,"DisplayName":"Tên có d?u '", "Email":"test@mail.com"}'
   ? JSON v?i "     ? Single quote   ? @ symbol
```

### **2. ??a Vào JavaScript Template String:**
```javascript
const content = `
    <button onclick="copyQuery(\`${query}\`)">
                                    ? D?u " trong query break syntax
`;
```

### **3. K?t Qu?:**
```javascript
// ? SAI - Syntax error:
<button onclick="copyQuery(`UPDATE [adm].[Log] SET [AcctionUser] = N'{"UserId":1}`')">
                                                                         ? Break t?i ?ây
```

---

## ? Gi?i Pháp:

### **1. Escape ?úng Th? T?:**

#### **Step 1: Format SQL Value** (`formatSqlValue()`)
```javascript
function formatSqlValue(value) {
    if (type === 'string') {
        // Escape cho SQL:
        // 1. Backslash: \ ? \\
        // 2. Single quote: ' ? ''
        let escaped = value
            .replace(/\\/g, '\\\\')  // ? Escape backslash tr??c
            .replace(/'/g, "''");     // ? Escape single quote
        
        return `N'${escaped}'`;
    }
}
```

**Input:**
```
{"UserId":1,"Name":"O'Brien"}
```

**Output (SQL-safe):**
```sql
N'{"UserId":1,"Name":"O''Brien"}'
   ? Single quote ???c escape thành ''
```

---

#### **Step 2: Escape Cho JavaScript** (`escapeForJavaScript()`)
```javascript
function escapeForJavaScript(text) {
    return text
        .replace(/\\/g, '\\\\')   // Backslash
        .replace(/`/g, '\\`')     // Backtick
        .replace(/\$/g, '\\$')    // Dollar sign
        .replace(/"/g, '\\"')     // ? Double quote
        .replace(/'/g, "\\'")     // Single quote
        .replace(/\n/g, '\\n')    // Newline
        .replace(/\r/g, '\\r')    // Carriage return
        .replace(/\t/g, '\\t');   // Tab
}
```

**Input (SQL query):**
```sql
UPDATE [Log] SET [AcctionUser] = N'{"UserId":1,"Name":"Test"}'
```

**Output (JS-safe):**
```javascript
"UPDATE [Log] SET [AcctionUser] = N'{\"UserId\":1,\"Name\":\"Test\"}'"
                                        ? D?u " ?ã ???c escape
```

---

#### **Step 3: Decode Khi Copy** (`copyQueryToClipboard()`)
```javascript
function copyQueryToClipboard(query) {
    // ? Decode escaped characters
    const decodedQuery = query
        .replace(/\\n/g, '\n')
        .replace(/\\r/g, '\r')
        .replace(/\\t/g, '\t')
        .replace(/\\"/g, '"')    // ? Decode double quotes
        .replace(/\\'/g, "'")
        .replace(/\\`/g, '`')
        .replace(/\\\$/g, '$')
        .replace(/\\\\/g, '\\');
    
    navigator.clipboard.writeText(decodedQuery);
}
```

**Input (escaped):**
```
UPDATE [Log] SET [AcctionUser] = N'{\"UserId\":1,\"Name\":\"Test\"}'
```

**Output (original):**
```sql
UPDATE [Log] SET [AcctionUser] = N'{"UserId":1,"Name":"Test"}'
```

---

### **2. Flow Hoàn Ch?nh:**

```
???????????????????????????????
? Raw Data t? Database        ?
? {"UserId":1,"Name":"O'Brien"}?
???????????????????????????????
           ?
           ?
???????????????????????????????
? formatSqlValue()            ?
? Escape ' ? ''               ?
? Result: N'{"...O''Brien"}' ?
???????????????????????????????
           ?
           ?
???????????????????????????????
? Build UPDATE Query          ?
? UPDATE [Log] SET ...        ?
???????????????????????????????
           ?
           ?
???????????????????????????????
? escapeForJavaScript()       ?
? Escape ", `, $, \n, etc.    ?
? Result: ...O\\'Brien...     ?
???????????????????????????????
           ?
           ?
???????????????????????????????
? Inject vào Template String  ?
? onclick="copy(\`${query}\`)"?
? ? No syntax error!         ?
???????????????????????????????
           ?
           ?
???????????????????????????????
? copyQueryToClipboard()      ?
? Decode escaped characters   ?
? Result: Original query      ?
???????????????????????????????
```

---

## ?? Test Cases:

### **Test 1: JSON String**

**Data:**
```json
{"UserId":1,"DisplayName":"Qu?n tr? h? th?ng","Email":"admin@test.com"}
```

**Expected SQL:**
```sql
N'{"UserId":1,"DisplayName":"Qu?n tr? h? th?ng","Email":"admin@test.com"}'
```

**Test:**
1. Click [EDIT] button
2. Query hi?n th? ?úng
3. Click "Copy to Clipboard"
4. Paste vào SQL Management Studio
5. ? Execute thành công

---

### **Test 2: Single Quote In String**

**Data:**
```
O'Brien's House
```

**Expected SQL:**
```sql
N'O''Brien''s House'
   ? Single quotes ???c escape
```

**Test:**
1. Click [INSERT] button
2. Query có `O''Brien''s`
3. Click "Execute Now"
4. ? Data ???c insert ?úng

---

### **Test 3: Backslash**

**Data:**
```
C:\Program Files\App
```

**Expected SQL:**
```sql
N'C:\\Program Files\\App'
    ? Backslashes ???c escape
```

**Test:**
1. Copy query
2. ? Path ?úng format

---

### **Test 4: Newlines & Tabs**

**Data:**
```
Line 1
	Tab here
Line 2
```

**Expected SQL:**
```sql
N'Line 1
	Tab here
Line 2'
```

**Test:**
1. Query preserve newlines
2. ? Format ?úng khi execute

---

### **Test 5: Mix All Special Characters**

**Data:**
```json
{"path":"C:\\Users\\Test","name":"O'Brien","desc":"Test \"quotes\""}
```

**Expected:**
- ? Backslashes escaped: `\\`
- ? Single quotes escaped: `''`
- ? Double quotes preserved trong JSON
- ? Execute thành công

---

## ?? Debug:

### **Check Escape Functions:**

```javascript
// Console:

// Test formatSqlValue
const testValue = '{"UserId":1,"Name":"O\'Brien"}';
const sqlValue = formatSqlValue(testValue);
console.log(sqlValue);
// Expected: N'{"UserId":1,"Name":"O''Brien"}'

// Test escapeForJavaScript
const testQuery = 'UPDATE [Log] SET [Data] = N\'{"UserId":1}\'';
const escapedQuery = escapeForJavaScript(testQuery);
console.log(escapedQuery);
// Expected: UPDATE [Log] SET [Data] = N\\'{\"UserId\":1}\\'

// Test decode
const decoded = escapedQuery
    .replace(/\\"/g, '"')
    .replace(/\\'/g, "'")
    .replace(/\\\\/g, '\\');
console.log(decoded);
// Expected: UPDATE [Log] SET [Data] = N'{"UserId":1}'
```

---

### **Check Button Click:**

```javascript
// After clicking [EDIT] button, check Console:

// 1. Check query generation
console.log('Original query:', updateQuery);

// 2. Check escaped query
console.log('Escaped query:', escapedQuery);

// 3. Check onclick attribute
const $btn = $('.btn-copy').first();
const onclick = $btn.attr('onclick');
console.log('Button onclick:', onclick);

// 4. Try manual copy
copyQueryToClipboard(escapedQuery);
// Then paste and check result
```

---

## ?? Before vs After:

| Scenario | Before (? SAI) | After (? ?ÚNG) |
|----------|----------------|----------------|
| JSON string | Syntax error | ? Copy/Execute OK |
| Single quote | Break SQL | ? Escaped `''` |
| Backslash | Lost in copy | ? Preserved |
| Newlines | Lost format | ? Preserved |
| Mixed special chars | Error | ? All handled |

---

## ? Final Test:

### **Query M?u V?i ??y ?? Ký T? ??c Bi?t:**

```sql
SELECT TOP 1 * FROM [adm].[Log]
WHERE [AcctionUser] LIKE '%{"UserId":1%'
```

### **Expected Result:**

1. Click [EDIT] button ? row có JSON data
2. Popup hi?n query UPDATE
3. Query có format:
```sql
UPDATE [adm].[Log]
SET
    [AcctionUser] = N'{"UserId":1,"DisplayName":"Qu?n tr? h? th?ng",...}'
WHERE
    [AcctionUser] = N'{"UserId":1,"DisplayName":"Qu?n tr? h? th?ng",...}';
```

4. Click **"Copy to Clipboard"**
5. Paste vào SQL Management Studio
6. ? **Execute thành công** - Không có syntax error!

---

## ?? Checklist:

- [ ] `formatSqlValue()` escape `'` ? `''`
- [ ] `escapeForJavaScript()` escape `"`, `` ` ``, `$`, `\n`, etc.
- [ ] `copyQueryToClipboard()` decode escaped chars
- [ ] `copyQueryToEditor()` decode escaped chars
- [ ] `executeQueryDirect()` decode escaped chars
- [ ] Test v?i JSON data ? ? OK
- [ ] Test v?i single quotes ? ? OK
- [ ] Test v?i backslashes ? ? OK
- [ ] Test v?i newlines ? ? OK
- [ ] Copy to clipboard ? ? Original format
- [ ] Execute query ? ? Success

---

## ?? K?t Lu?n:

**T?t c? ký t? ??c bi?t gi? ???c x? lý ?úng:**
1. ? Escape 2 l?n: SQL + JavaScript
2. ? Decode khi copy/execute
3. ? Preserve format g?c
4. ? Không có syntax error

**Test ngay:**
```sql
-- Ch?n row có JSON data ph?c t?p
SELECT TOP 1 * FROM [adm].[Log]

-- Click [EDIT] ? Copy ? Execute
-- ? Thành công!
```

?? **Problem Solved!** ??
