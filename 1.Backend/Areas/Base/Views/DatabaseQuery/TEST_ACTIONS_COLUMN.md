# ?? Test Actions Column - Step by Step

## ? V?n ?? ?ã S?a:

### **Tr??c ?ây:**
? Hàm `enableRowActions()` ???c ??nh ngh?a nh?ng **KHÔNG ???C G?I**
? B?ng k?t qu? không có c?t Actions
? Không có nút INSERT/EDIT/DELETE

### **Bây gi?:**
? `displayResult()` g?i `enableRowActions()` sau khi render xong
? C?t "Actions" t? ??ng xu?t hi?n
? M?i row có 3 nút: [INSERT] [EDIT] [DELETE]

---

## ?? Cách Test:

### **B??c 1: Clear Cache & Refresh**
```
Ctrl + Shift + Del (Clear browser cache)
Ctrl + F5 (Hard refresh)
```

### **B??c 2: M? Database Query Tool**
```
URL: /Admin/Base/DatabaseQuery/Index
Nh?p PIN (n?u c?n)
```

### **B??c 3: Ch?y SELECT Query**
```sql
-- Query ??n gi?n
SELECT TOP 10 * FROM [adm].[User]

-- Ho?c query ph?c t?p
SELECT 
    Id, 
    Name, 
    Email, 
    Status 
FROM [adm].[User] 
WHERE Status = 1
ORDER BY Id DESC
```

### **B??c 4: Ki?m Tra K?t Qu?**

B?ng k?t qu? ph?i có d?ng:

```
????????????????????????????????????????????????????????
? Id ? Name ? Email         ? Actions                  ?
????????????????????????????????????????????????????????
? 1  ? John ? john@test.com ? [INSERT] [EDIT] [DELETE] ?
? 2  ? Jane ? jane@test.com ? [INSERT] [EDIT] [DELETE] ?
? 3  ? Bob  ? bob@test.com  ? [INSERT] [EDIT] [DELETE] ?
????????????????????????????????????????????????????????
```

#### ? Checklist:
- [ ] C?t "Actions" xu?t hi?n ? **cu?i cùng** (bên ph?i nh?t)
- [ ] Header có text "Actions" màu tr?ng trên n?n xanh
- [ ] M?i row có **3 nút màu khác nhau**:
  - ?? **INSERT** (màu xanh d??ng) - `background: #2196F3`
  - ?? **EDIT** (màu xanh lá) - `background: #4CAF50`
  - ?? **DELETE** (màu ??) - `background: #f44336`
- [ ] Hover vào nút ? nút sáng lên và n?i lên (transform: translateY(-1px))

---

## ?? Test T?ng Nút:

### **Test 1: INSERT Button**

1. Click nút **[INSERT]** ? row b?t k?
2. **K? v?ng:**
   - Popup modal hi?n ra v?i title "INSERT Query"
   - Có khung màu xanh d??ng "Copy d? li?u c?a row này thành INSERT m?i"
   - SQL query hi?n th? trong khung ?en (background: #282c34)
   - Query có format:
     ```sql
     INSERT INTO [schema].[table] (
         [Column1],
         [Column2],
         ...
     ) VALUES (
         'Value1', -- Column1
         'Value2', -- Column2
         ...
     );
     ```
   - Có 2 nút:
     - ?? **Copy to Clipboard**
     - ?? **Copy to Editor**

3. **Test Copy to Clipboard:**
   - Click "Copy to Clipboard"
   - Notification "?ã copy query vào clipboard!" xu?t hi?n
   - Paste vào notepad ? Query ?úng format

4. **Test Copy to Editor:**
   - Click "Copy to Editor"
   - Popup ?óng l?i
   - Query xu?t hi?n trong #queryEditor
   - Notification "?ã copy query vào editor!" xu?t hi?n

---

### **Test 2: EDIT Button**

1. Click nút **[EDIT]** ? row b?t k?
2. **K? v?ng:**
   - Popup modal hi?n ra v?i title "UPDATE Query"
   - Có khung màu xanh lá "Ch?nh s?a tr?c ti?p ho?c gen UPDATE query"
   - SQL query UPDATE v?i WHERE ??y ??
   - Query có format:
     ```sql
     UPDATE [schema].[table]
     SET
         [Column1] = 'Value1',
         [Column2] = 'Value2',
         ...
     WHERE
         [Column1] = 'Value1' AND
         [Column2] = 'Value2' AND
         ...;
     ```
   - Có 3 nút:
     - ?? **Edit Tr?c Ti?p** (màu xanh lá)
     - ?? **Copy Query** (màu xanh d??ng)
     - ?? **Execute Now** (màu gradient tím)

3. **Test Edit Tr?c Ti?p:**
   - Click "Edit Tr?c Ti?p"
   - Popup UPDATE ?óng l?i
   - Popup "Ch?nh s?a dòng d? li?u" hi?n ra
   - Form có input cho T?T C? các columns
   - M?i input có:
     - Label: Tên column
     - Value: Giá tr? hi?n t?i
     - Info text: "Original: [giá tr? g?c]"
   - Có 2 nút:
     - ? **H?y**
     - ? **L?u**

4. **Test L?u Thay ??i:**
   - S?a 1 field (VD: Email)
   - Click "L?u"
   - Confirm dialog xu?t hi?n: "B?n có ch?c mu?n l?u thay ??i?"
   - Click OK
   - AJAX call t?i `/Admin/Base/DatabaseQuery/UpdateRow`
   - Notification "C?p nh?t thành công!"
   - Query t? ??ng ch?y l?i
   - Data m?i hi?n th?

5. **Test Copy Query & Execute Now:**
   - T??ng t? INSERT test

---

### **Test 3: DELETE Button**

1. Click nút **[DELETE]** ? row b?t k?
2. **K? v?ng:**
   - Popup modal hi?n ra v?i title "DELETE Query"
   - Có khung màu **??** "C?nh báo: Thao tác này s? XÓA d? li?u!"
   - Có khung màu vàng "Row data:" hi?n th? T?T C? columns và values
   - SQL query DELETE v?i WHERE ??y ??:
     ```sql
     DELETE FROM [schema].[table]
     WHERE
         [Column1] = 'Value1' AND
         [Column2] = 'Value2' AND
         ...;
     ```
   - Có 2 nút:
     - ?? **Copy Query** (màu xanh d??ng)
     - ??? **Execute Delete** (màu ??)

3. **Test Execute Delete:**
   - Click "Execute Delete"
   - **First confirm:** Browser confirm dialog
     ```
     B?N CÓ CH?C MU?N XÓA DÒNG NÀY?
     
     D? li?u s? b? xóa v?nh vi?n!
     
     Column1: Value1
     Column2: Value2
     ...
     ```
   - Click OK
   - AJAX call t?i `/Admin/Base/DatabaseQuery/DeleteRow`
   - Notification "Xóa thành công!"
   - Query t? ??ng ch?y l?i
   - Row ?ã bi?n m?t

---

## ?? Troubleshooting:

### **V?n ?? 1: C?t Actions KHÔNG xu?t hi?n**

**Nguyên nhân:**
- File JS ch?a ???c load
- Hàm `enableRowActions()` không ???c g?i
- Query không ph?i SELECT

**Gi?i pháp:**
```javascript
// M? Console (F12) và check:
console.log('currentResult:', currentResult);
console.log('typeof enableRowActions:', typeof enableRowActions);

// N?u undefined ? file JS ch?a load
// Ki?m tra:
<script src="~/js/database-query-extended.js?v=1"></script>

// G?i th? công:
enableRowActions();
```

---

### **V?n ?? 2: Nút không ho?t ??ng (onclick không ch?y)**

**Nguyên nhân:**
- Hàm `showInsertQueryForRow()` không t?n t?i
- currentResult b? null
- Schema/Table không detect ???c

**Gi?i pháp:**
```javascript
// Check trong Console:
console.log('currentEditSchema:', currentEditSchema);
console.log('currentEditTable:', currentEditTable);

// N?u empty string ? Query không có FROM [schema].[table]
// Fix query:
SELECT * FROM [adm].[User]  // ? ?ÚNG
SELECT * FROM User          // ? SAI (thi?u schema)
```

---

### **V?n ?? 3: Modal không hi?n / không ?óng**

**Nguyên nhân:**
- `#queryActionModal` không t?n t?i trong DOM
- fadeIn/fadeOut không ho?t ??ng

**Gi?i pháp:**
```javascript
// Check modal t?n t?i:
console.log('Modal exists:', $('#queryActionModal').length);

// N?u = 0 ? _EditRowModal.cshtml ch?a ???c load
// Ki?m tra:
@Html.Partial("_EditRowModal")  // Ph?i có dòng này
```

---

### **V?n ?? 4: API tr? v? l?i 404/500**

**Endpoints c?n có:**
```csharp
// DatabaseQueryController.cs
[HttpPost]
public async Task<IActionResult> UpdateRow([FromBody] UpdateRowRequest request)

[HttpPost]
public async Task<IActionResult> DeleteRow([FromBody] DeleteRowRequest request)

[HttpGet]
public async Task<IActionResult> GetTableSchema(string schemaName, string tableName)
```

**Check trong Network tab (F12):**
- Status 404 ? Controller/Action thi?u
- Status 500 ? L?i backend
- Status 200 nh?ng output = 0 ? Business logic l?i

---

## ?? Expected vs Actual Result:

| Feature | Expected | Check |
|---------|----------|-------|
| C?t Actions xu?t hi?n | ? Có | [ ] |
| Header "Actions" có màu xanh | ? Có | [ ] |
| 3 nút m?i row | ? Có | [ ] |
| INSERT button màu xanh d??ng | ? Có | [ ] |
| EDIT button màu xanh lá | ? Có | [ ] |
| DELETE button màu ?? | ? Có | [ ] |
| Click INSERT ? Popup hi?n | ? Có | [ ] |
| Click EDIT ? Popup hi?n | ? Có | [ ] |
| Click DELETE ? Popup ?? hi?n | ? Có | [ ] |
| Edit form hi?n th? ??y ?? columns | ? Có | [ ] |
| L?u thành công ? Query refresh | ? Có | [ ] |
| Xóa thành công ? Row bi?n m?t | ? Có | [ ] |

---

## ?? Quick Test Command:

```sql
-- Test query m?u
SELECT TOP 5 
    Id, 
    Name, 
    Email, 
    Status,
    DatePosted
FROM [adm].[User]
WHERE Status = 1
ORDER BY Id DESC
```

**Sau khi ch?y, ph?i th?y:**
```
????????????????????????????????????????????????????????????????????????????????
? Id ? Name ? Email          ? Status ? DatePosted  ? Actions                  ?
????????????????????????????????????????????????????????????????????????????????
? 5  ? Eve  ? eve@test.com   ? 1      ? 2024-01-05  ? [INSERT] [EDIT] [DELETE] ?
? 4  ? Dave ? dave@test.com  ? 1      ? 2024-01-04  ? [INSERT] [EDIT] [DELETE] ?
? 3  ? Bob  ? bob@test.com   ? 1      ? 2024-01-03  ? [INSERT] [EDIT] [DELETE] ?
? 2  ? Jane ? jane@test.com  ? 1      ? 2024-01-02  ? [INSERT] [EDIT] [DELETE] ?
? 1  ? John ? john@test.com  ? 1      ? 2024-01-01  ? [INSERT] [EDIT] [DELETE] ?
????????????????????????????????????????????????????????????????????????????????
```

---

## ? Final Checklist:

- [ ] File `database-query-extended.js?v=1` ???c load (check Network tab)
- [ ] `enableRowActions()` ???c g?i sau `displayResult()`
- [ ] C?t "Actions" xu?t hi?n bên ph?i nh?t
- [ ] Click INSERT ? Modal xanh d??ng
- [ ] Click EDIT ? Modal xanh lá ? Edit form
- [ ] Click DELETE ? Modal ?? ? Double confirm
- [ ] Copy to Clipboard ho?t ??ng
- [ ] Copy to Editor ho?t ??ng
- [ ] Execute Now ho?t ??ng
- [ ] Edit & Save ho?t ??ng
- [ ] Delete ho?t ??ng

---

## ?? N?u T?t C? ??u Pass:

**Congratulations!** C?t Actions ?ã ho?t ??ng hoàn h?o! ??

Gi? b?n có th?:
- ? INSERT copy row nhanh
- ? EDIT tr?c ti?p ho?c gen UPDATE query
- ? DELETE an toàn v?i double confirm

**Database Query Tool ?ã hoàn ch?nh!** ??
