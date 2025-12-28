# ?? Quick Start Guide - Database Query Tool

## ? ?ã cài ??t s?n

Các file sau ?ã ???c t?o và s?n sàng s? d?ng:

### 1. Controller
?? `1.Backend\Areas\Base\Controllers\DatabaseQueryController.cs`
- X? lý t?t c? logic cho Database Query Tool
- B?o m?t v?i PIN authentication (PIN c? ??nh: `DB_ADMIN_2025`)
- S? d?ng `[IsSupperAdminAuthorizePermission]` - ch? Super Admin m?i truy c?p
- Log t?t c? các thao tác

### 2. View
?? `1.Backend\Areas\Base\Views\DatabaseQuery\Index.cshtml`
- Giao di?n ng??i dùng hi?n ??i
- SQL Editor v?i syntax highlighting
- Sidebar hi?n th? tables và l?ch s?

### 3. Menu Admin
?? `1.Backend\Views\Shared\_MenuAdmin.cshtml`
- ?ã thêm link vào menu SUPPER ADMIN
- Ch? hi?n th? cho Super Admin

## ?? Cách s? d?ng ngay

### B??c 1: Ch?y ?ng d?ng
```bash
dotnet run --project 1.Backend\PT.UI.csproj
```

### B??c 2: ??ng nh?p
- ??ng nh?p v?i tài kho?n **Super Admin**
- Tìm menu: **SUPPER ADMIN** ? **Database Query Tool**

### B??c 3: Xác th?c PIN
- Nh?p PIN: `DB_ADMIN_2025`
- Click **Xác th?c**

### B??c 4: Th?c thi query
```sql
-- Xem danh sách users
SELECT * FROM [adm].[User] WHERE Status = 1

-- Thêm d? li?u
INSERT INTO [dbo].[Contact] (Name, Email, CreatedDate)
VALUES ('Test User', 'test@example.com', GETDATE())

-- C?p nh?t d? li?u
UPDATE [dbo].[ContentPage] 
SET Status = 1 
WHERE Id = 123

-- Xóa d? li?u
DELETE FROM [dbo].[Contact] WHERE Id = 456
```

## ?? Tùy ch?nh

### ??i PIN
PIN ???c c? ??nh trong code t?i file `DatabaseQueryController.cs`:
```csharp
private const string DEFAULT_PIN = "DB_ADMIN_2025"; // Thay ??i giá tr? này
```

**L?u ý**: Sau khi ??i PIN, c?n **build l?i ?ng d?ng**.

### Thay ??i gi?i h?n
Các h?ng s? có th? tùy ch?nh trong controller:
```csharp
private const int MAX_QUERY_TIMEOUT = 300; // Timeout t?i ?a (giây)
private const int MAX_ROWS_RETURN = 5000;  // S? dòng t?i ?a
```

## ??? B?o m?t

### ? ?ã có
- **Xác th?c 2 l?p** (Login Super Admin + PIN)
- **Attribute b?o m?t** (`[IsSupperAdminAuthorizePermission]`)
- **Ch?n l?nh nguy hi?m** (DROP, TRUNCATE, sp_, xp_)
- **Log t?t c? thao tác**
- **Timeout session** (2 gi?)
- **Gi?i h?n k?t qu?** (5,000 rows)

### ?? L?u ý
- **Không chia s? PIN** v?i ng??i không có quy?n
- **Luôn ki?m tra query** tr??c khi th?c thi
- **Backup database** tr??c khi ch?y UPDATE/DELETE l?n
- **S? d?ng WHERE clause** ?? tránh ?nh h??ng toàn b? table

## ?? Tính n?ng chính

| Tính n?ng | Mô t? |
|-----------|-------|
| ?? PIN Authentication | Xác th?c 2 l?p v?i PIN code c? ??nh |
| ?? Super Admin Only | Ch? Super Admin truy c?p (attribute `[IsSupperAdminAuthorizePermission]`) |
| ?? SQL Editor | Editor v?i syntax highlighting |
| ?? Tables Sidebar | Danh sách t?t c? tables và row count |
| ?? Execution Time | Hi?n th? th?i gian th?c thi |
| ?? Result Table | Hi?n th? k?t qu? d?ng b?ng |
| ?? Export CSV | Xu?t k?t qu? ra file CSV |
| ?? Query History | Xem và tái s? d?ng query c? |
| ??? Security | Ch?n l?nh nguy hi?m |
| ?? Logging | Ghi log t?t c? thao tác |

## ?? Ví d? s? d?ng th?c t?

### 1. S?a l?i d? li?u kh?n c?p
```sql
-- Khách hàng báo email sai
UPDATE [dbo].[Customer] 
SET Email = 'correct@email.com' 
WHERE Id = 789
```

### 2. Thêm c?u hình m?i
```sql
-- Thêm setting m?i
INSERT INTO [dbo].[Parameter] (Id, Name, Value, PortalId, Language)
VALUES (NEWID(), 'NewFeature', 'Enabled', 1, 'vi')
```

### 3. Ki?m tra d? li?u
```sql
-- ??m s? bài vi?t theo tr?ng thái
SELECT Status, COUNT(*) as Total
FROM [dbo].[ContentPage]
GROUP BY Status
```

### 4. D?n d?p d? li?u test
```sql
-- Xóa contact test
DELETE FROM [dbo].[Contact] 
WHERE Email LIKE '%test%' 
AND CreatedDate < DATEADD(day, -7, GETDATE())
```

## ?? Tài li?u ??y ??

Xem file: `1.Backend\Areas\Base\DatabaseQueryTool_README.md`

## ? Troubleshooting

| V?n ?? | Gi?i pháp |
|--------|-----------|
| Không th?y menu | Ki?m tra role, ph?i là SuperAdmin |
| PIN sai | PIN c? ??nh: `DB_ADMIN_2025` - ki?m tra l?i ho?c liên h? dev ?? thay ??i |
| Timeout | T?ng giá tr? Timeout (5-300s) ho?c t?i ?u query |
| L?i k?t n?i | Ki?m tra connection string trong `appsettings.json` |

## ?? Hoàn t?t!

Chúc b?n s? d?ng Database Query Tool hi?u qu?! 

**L?u ý**: Công c? này r?t m?nh m?, hãy s? d?ng c?n th?n! ??

---
?? **H? tr?**: Liên h? Team Leader n?u c?n h? tr?  
?? **B?o m?t**: Không chia s? PIN v?i ng??i khác  
?? **Backup**: Luôn backup tr??c khi thao tác quan tr?ng  
?? **PIN m?c ??nh**: `DB_ADMIN_2025` (c? ??nh trong code)
