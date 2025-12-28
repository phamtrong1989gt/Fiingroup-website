# ?? Database Query Management Tool

## ?? T?ng quan
Database Query Management Tool là công c? chuyên nghi?p cho phép Super Admin th?c thi các câu l?nh SQL tr?c ti?p trên database production mà không c?n remote vào server.

## ? Tính n?ng chính

### ?? B?o m?t
- **Xác th?c 2 l?p**: Yêu c?u PIN code tr??c khi s? d?ng (PIN c? ??nh trong code)
- **Phân quy?n**: Ch? dành cho Super Admin (`[IsSupperAdminAuthorizePermission]`)
- **Timeout session**: PIN h?t h?n sau 2 gi?
- **Log ??y ??**: Ghi l?i m?i thao tác và ng??i th?c hi?n
- **Ch?n l?nh nguy hi?m**: T? ??ng ch?n DROP DATABASE, TRUNCATE, sp_, xp_

### ?? Giao di?n
- **Editor SQL** v?i syntax highlighting
- **Sidebar Tables**: Hi?n th? danh sách b?ng và s? dòng
- **L?ch s? Query**: Xem và tái s? d?ng các query ?ã th?c thi
- **K?t qu? d?ng b?ng**: Hi?n th? d? li?u d? ??c
- **Export CSV**: Xu?t k?t qu? ra file CSV

### ? Hi?u n?ng
- **Timeout tùy ch?nh**: T? 5s ??n 300s (m?c ??nh 30s)
- **Gi?i h?n k?t qu?**: T?i ?a 5,000 dòng ?? tránh quá t?i
- **Th?i gian th?c thi**: Hi?n th? th?i gian ch?y query

## ?? Cách s? d?ng

### 1. Truy c?p
- ??ng nh?p v?i tài kho?n **Super Admin**
- Vào menu: **SUPPER ADMIN** ? **Database Query Tool**

### 2. Xác th?c PIN
- Nh?p PIN code (m?c ??nh: `DB_ADMIN_2025`)
- PIN có hi?u l?c 2 gi?

### 3. Th?c thi Query
```sql
-- Ví d?: Xem danh sách users
SELECT * FROM [adm].[User] WHERE Status = 1

-- Ví d?: Thêm user m?i
INSERT INTO [adm].[User] (UserName, Email, CreatedDate)
VALUES ('newuser', 'email@example.com', GETDATE())

-- Ví d?: Update d? li?u
UPDATE [dbo].[ContentPage] 
SET Status = 1 
WHERE Id = 123

-- Ví d?: Xóa d? li?u
DELETE FROM [dbo].[Contact] WHERE Id = 456
```

### 4. Các thao tác
- **F5** ho?c nút **Th?c thi**: Ch?y query
- **Xóa**: Xóa n?i dung editor
- **Format**: T? ??ng format SQL
- **Export CSV**: Xu?t k?t qu? ra file
- **Click vào Table**: T? ??ng t?o SELECT query

## ??? B?o m?t & Best Practices

### ?? C?nh báo
- **LUÔN LUÔN** ki?m tra k? query tr??c khi th?c thi
- **BACKUP** database tr??c khi ch?y UPDATE/DELETE l?n
- **S? D?NG WHERE** clause ?? tránh ?nh h??ng toàn b? table
- **TEST** trên môi tr??ng dev tr??c

### ? Các l?nh an toàn
- `SELECT` - ??c d? li?u
- `INSERT` - Thêm d? li?u m?i
- `UPDATE` v?i WHERE c? th?
- `DELETE` v?i WHERE c? th?

### ? Các l?nh B? CH?N
- `DROP DATABASE` - Xóa database
- `DROP SCHEMA` - Xóa schema
- `TRUNCATE` - Xóa toàn b? table
- `sp_*` - System stored procedures
- `xp_*` - Extended stored procedures

## ?? C?u hình

### PIN Code
PIN ???c c? ??nh trong code t?i `DatabaseQueryController.cs`:
```csharp
private const string DEFAULT_PIN = "DB_ADMIN_2025";
```

**?? thay ??i PIN**:
1. M? file `1.Backend\Areas\Base\Controllers\DatabaseQueryController.cs`
2. Tìm dòng `private const string DEFAULT_PIN = "DB_ADMIN_2025";`
3. Thay ??i giá tr?
4. **Build l?i ?ng d?ng**

### Gi?i h?n
Các h?ng s? có th? tùy ch?nh trong controller:
```csharp
private const int MAX_QUERY_TIMEOUT = 300; // 5 phút
private const int MAX_ROWS_RETURN = 5000;  // Gi?i h?n s? dòng
```

### Phân quy?n
S? d?ng attribute `[IsSupperAdminAuthorizePermission]` - t? ??ng ki?m tra:
- User ph?i ??ng nh?p
- User ph?i có `IsSuperAdmin = true`
- T? ??ng redirect n?u không có quy?n

## ?? Log và Audit

M?i thao tác ??u ???c ghi log vào b?ng `[adm].[Log]`:
- Th?i gian th?c thi
- User th?c hi?n
- Câu l?nh SQL
- K?t qu? (thành công/l?i)

### Xem log
```sql
SELECT * FROM [adm].[Log] 
WHERE ObjectType = 'DatabaseQuery'
ORDER BY ActionTime DESC
```

## ?? Troubleshooting

### L?i: "PIN không chính xác"
- PIN m?c ??nh: `DB_ADMIN_2025`
- N?u ?ã thay ??i, ki?m tra l?i trong code
- Ph?i build l?i ?ng d?ng sau khi thay ??i

### L?i: "Timeout"
- T?ng giá tr? Timeout (5-300s)
- T?i ?u query (thêm index, WHERE clause)

### L?i: "Vui lòng xác th?c PIN tr??c"
- PIN ?ã h?t h?n (2 gi?)
- Nh?n **??ng xu?t** và xác th?c l?i

### L?i: "Câu l?nh ch?a t? khóa nguy hi?m"
- H? th?ng ch?n các l?nh có nguy c? cao
- Liên h? Database Administrator n?u th?c s? c?n thi?t

### Không th?y menu
- Ki?m tra user có `IsSuperAdmin = true` không
- Ch? Super Admin m?i th?y menu này

## ?? Use Cases

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

## ?? L?i ích

### ? Cho Developer
- Không c?n remote vào server
- S?a l?i nhanh chóng
- Debug d? dàng
- Export d? li?u ?? phân tích

### ? Cho Khách hàng
- Gi?i quy?t v?n ?? nhanh
- Downtime t?i thi?u
- Không c?n ch? ??i

### ? Cho H? th?ng
- Log ??y ?? cho audit
- B?o m?t cao v?i attribute phân quy?n
- Ki?m soát ???c r?i ro
- Không c?n c?u hình external

## ??? Ki?n trúc

### Attribute B?o m?t
```csharp
[IsSupperAdminAuthorizePermission]
```
T? ??ng ki?m tra:
- Authentication (?ã ??ng nh?p)
- Authorization (IsSuperAdmin = true)
- Account status (không b? lock)
- Auto redirect n?u fail

### PIN Authentication
- Layer b?o m?t th? 2
- Stored in memory cache
- Timeout: 2 gi?
- M?i user có PIN session riêng

### Query Validation
- Keyword blacklist
- SQL injection prevention
- Timeout protection
- Result size limit

## ?? H? tr?

N?u g?p v?n ??, vui lòng:
1. Ki?m tra log trong Database Query Tool
2. Xem log h? th?ng: `logs/error_*.log`
3. Ki?m tra quy?n Super Admin
4. Liên h? Team Leader ho?c Database Administrator

---

**?? L?U Ý QUAN TR?NG**: 
Công c? này r?t m?nh m? nh?ng c?ng r?t nguy hi?m n?u s? d?ng sai. 
Luôn th?n tr?ng khi th?c thi UPDATE/DELETE trên production database!

**Phát tri?n b?i**: PT Development Team  
**Phiên b?n**: 1.0.0  
**Ngày c?p nh?t**: 2025  
**PIN m?c ??nh**: `DB_ADMIN_2025` (c? ??nh trong code)  
**Phân quy?n**: `[IsSupperAdminAuthorizePermission]`
