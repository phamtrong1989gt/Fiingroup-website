# Quick Start Guide - Database Query Tool

## Đã cài đặt sẵn

Các file sau đã được tạo và sẵn sàng sử dụng:

### 1. Controller
- `1.Backend/Areas/Base/Controllers/DatabaseQueryController.cs`
  - Xử lý toàn bộ logic cho Database Query Tool
  - Bảo mật với PIN authentication (PIN mặc định: `DB_ADMIN_2025`)
  - Sử dụng `[IsSupperAdminAuthorizePermission]` — chỉ Super Admin mới truy cập
  - Ghi log mọi thao tác

### 2. View
- `1.Backend/Areas/Base/Views/DatabaseQuery/Index.cshtml`
  - Giao diện editor SQL với syntax highlighting
  - Sidebar hiển thị bảng và lịch sử query

### 3. Menu Admin
- `1.Backend/Views/Shared/_MenuAdmin.cshtml`
  - Đã thêm link vào menu SUPPER ADMIN
  - Chỉ hiển thị cho Super Admin

## Cách sử dụng ngay

### Bước 1: Chạy ứng dụng
```bash
dotnet run --project 1.Backend/PT.UI.csproj
```

### Bước 2: Đăng nhập
- Đăng nhập bằng tài khoản **Super Admin**
- Vào: **SUPPER ADMIN** → **Database Query Tool**

### Bước 3: Xác thực PIN
- Nhập PIN: `DB_ADMIN_2025` (mặc định)
- Nhấn **Xác thực**

### Bước 4: Thực thi query
```sql
-- Ví dụ: Xem users
SELECT * FROM [adm].[User] WHERE Status = 1;

-- Ví dụ: Thêm contact
INSERT INTO [dbo].[Contact] (Name, Email, CreatedDate)
VALUES ('Test User', 'test@example.com', GETDATE());

-- Ví dụ: Cập nhật
UPDATE [dbo].[ContentPage]
SET Status = 1
WHERE Id = 123;

-- Ví dụ: Xóa
DELETE FROM [dbo].[Contact] WHERE Id = 456;
```

## Tùy chỉnh

### Đổi PIN
PIN được định nghĩa trong `DatabaseQueryController.cs`:
```csharp
private const string DEFAULT_PIN = "DB_ADMIN_2025"; // Thay giá trị này
```
Sau khi đổi PIN cần build lại ứng dụng.

### Giới hạn
Các hằng số có thể tùy chỉnh trong controller:
```csharp
private const int MAX_QUERY_TIMEOUT = 300; // giây (5 phút)
private const int MAX_ROWS_RETURN = 5000;  // số dòng tối đa
```

## Bảo mật
- Xác thực 2 lớp: Login Super Admin + PIN
- Attribute bảo mật: `[IsSupperAdminAuthorizePermission]`
- Hệ thống chặn các lệnh nguy hiểm: `DROP DATABASE`, `TRUNCATE`, `sp_*`, `xp_*`
- Log đầy đủ cho audit
- PIN session timeout: 2 giờ
- Giới hạn kết quả trả về: mặc định 5.000 dòng

### Lưu ý an toàn
- Không chia sẻ PIN
- Luôn kiểm tra kỹ query trước khi thực thi
- Backup database trước khi chạy các lệnh thay đổi lớn
- Test trên môi trường dev/trial trước

## Tính năng chính
- Editor SQL với syntax highlighting
- Sidebar tables và row count
- Lịch sử query để tái sử dụng
- Hiển thị thời gian thực thi và kết quả dạng bảng
- Export kết quả sang CSV
- Chặn lệnh nguy hiểm và giới hạn kích thước kết quả

## Ví dụ sử dụng thực tế
```sql
-- Sửa lỗi dữ liệu khẩn cấp
UPDATE [dbo].[Customer]
SET Email = 'correct@email.com'
WHERE Id = 789;

-- Thêm cấu hình
INSERT INTO [dbo].[Parameter] (Id, Name, Value, PortalId, Language)
VALUES (NEWID(), 'NewFeature', 'Enabled', 1, 'vi');

-- Kiểm tra dữ liệu
SELECT Status, COUNT(*) AS Total
FROM [dbo].[ContentPage]
GROUP BY Status;

-- Dọn dẹp test
DELETE FROM [dbo].[Contact]
WHERE Email LIKE '%test%'
AND CreatedDate < DATEADD(day, -7, GETDATE());
```

## Troubleshooting
| Vấn đề | Giải pháp |
|---|---|
| Không thấy menu | Kiểm tra user có thuộc Super Admin không |
| PIN sai | Kiểm tra PIN trong code (`DEFAULT_PIN`) và build lại |
| Timeout | Tăng giá trị timeout (5–300s) hoặc tối ưu query |
| Không kết nối DB | Kiểm tra connection string trong `appsettings.json` |

---

**LƯU Ý QUAN TRỌNG**: Công cụ này rất mạnh, hãy thận trọng khi thực thi các lệnh thay đổi trên production.

**Hỗ trợ**: Liên hệ Team Leader hoặc Database Administrator nếu cần trợ giúp.
