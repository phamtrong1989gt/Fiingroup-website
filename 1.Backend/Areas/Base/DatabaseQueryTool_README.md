# Database Query Management Tool

## Tổng quan
Database Query Management Tool là công cụ chuyên nghiệp cho phép Super Admin thực thi các câu lệnh SQL trực tiếp trên database production mà không cần remote vào server.

## Tính năng chính

### Bảo mật
- **Xác thực 2 lớp**: Yêu cầu PIN code trước khi sử dụng (PIN cài định trong code)
- **Phân quyền**: Chỉ dành cho Super Admin (`[IsSupperAdminAuthorizePermission]`)
- **Timeout session**: PIN hết hạn sau 2 giờ
- **Log đầy đủ**: Ghi lại mọi thao tác và người thực hiện
- **Chặn lệnh nguy hiểm**: Tự động chặn `DROP DATABASE`, `TRUNCATE`, `sp_*`, `xp_*`

### Giao diện
- **Editor SQL** với syntax highlighting
- **Sidebar Tables**: Hiển thị danh sách bảng và số dòng
- **Lịch sử Query**: Xem và tái sử dụng các query đã thực thi
- **Kết quả dạng bảng**: Hiển thị dữ liệu được truy vấn
- **Export CSV**: Xuất kết quả ra file CSV

### Hiệu năng
- **Timeout tùy chỉnh**: Từ 5s đến 300s (mặc định 30s)
- **Giới hạn kết quả**: Tối đa 5.000 dòng để tránh quá tải
- **Thời gian thực thi**: Hiển thị thời gian chạy query

## Cách sử dụng

### 1. Truy cập
- Đăng nhập với tài khoản **Super Admin**
- Vào menu: **SUPPER ADMIN** → **Database Query Tool**

### 2. Xác thực PIN
- Nhập PIN code (mặc định: `DB_ADMIN_2025`)
- PIN có hiệu lực 2 giờ

### 3. Thực thi Query
```sql
-- Ví dụ: Xem danh sách users
SELECT * FROM [adm].[User] WHERE Status = 1

-- Ví dụ: Thêm user mới
INSERT INTO [adm].[User] (UserName, Email, CreatedDate)
VALUES ('newuser', 'email@example.com', GETDATE())

-- Ví dụ: Update dữ liệu
UPDATE [dbo].[ContentPage]
SET Status = 1
WHERE Id = 123

-- Ví dụ: Xóa dữ liệu
DELETE FROM [dbo].[Contact] WHERE Id = 456
```

### 4. Các thao tác
- **F5** hoặc nút **Thực thi**: Chạy query
- **Xóa**: Xóa nội dung editor
- **Format**: Tự động format SQL
- **Export CSV**: Xuất kết quả ra file
- **Click vào Table**: Tự động tạo SELECT query

## Bảo mật & Best Practices

### Cảnh báo
- **LUÔN LUÔN** kiểm tra kỹ query trước khi thực thi
- **BACKUP** database trước khi chạy UPDATE/DELETE lớn
- **SỬ DỤNG WHERE** clause để tránh ảnh hưởng toàn bộ table
- **TEST** trên môi trường dev trước

### Các lệnh an toàn
- `SELECT` - đọc dữ liệu
- `INSERT` - thêm dữ liệu mới
- `UPDATE` với WHERE cụ thể
- `DELETE` với WHERE cụ thể

### Các lệnh BỊ CHẶN
- `DROP DATABASE` - Xóa database
- `DROP SCHEMA` - Xóa schema
- `TRUNCATE` - Xóa toàn bộ table
- `sp_*` - System stored procedures
- `xp_*` - Extended stored procedures

## Cấu hình

### PIN Code
PIN được cài định trong code tại `DatabaseQueryController.cs`:
```csharp
private const string DEFAULT_PIN = "DB_ADMIN_2025";
```

**Để thay đổi PIN**:
1. Mở file `1.Backend\\Areas\\Base\\Controllers\\DatabaseQueryController.cs`
2. Tìm dòng `private const string DEFAULT_PIN = "DB_ADMIN_2025";`
3. Thay đổi giá trị
4. **Build lại ứng dụng**

### Giới hạn
Các hằng số có thể tùy chỉnh trong controller:
```csharp
private const int MAX_QUERY_TIMEOUT = 300; // 5 phút
private const int MAX_ROWS_RETURN = 5000;  // Giới hạn số dòng
```

### Phân quyền
Sử dụng attribute `[IsSupperAdminAuthorizePermission]` - tự động kiểm tra:
- User phải đăng nhập
- User phải có `IsSuperAdmin = true`
- Tự động redirect nếu không có quyền

## Log và Audit

Mọi thao tác đều được ghi log vào bảng `[adm].[Log]`:
- Thời gian thực thi
- User thực hiện
- Câu lệnh SQL
- Kết quả (thành công/lỗi)

### Xem log
```sql
SELECT * FROM [adm].[Log]
WHERE ObjectType = 'DatabaseQuery'
ORDER BY ActionTime DESC
```

## Troubleshooting

### Lỗi: "PIN không chính xác"
- PIN mặc định: `DB_ADMIN_2025`
- Nếu đã thay đổi, kiểm tra lại trong code
- Phải build lại ứng dụng sau khi thay đổi

### Lỗi: "Timeout"
- Tăng giá trị Timeout (5-300s)
- Tối ưu query (thêm index, WHERE clause)

### Lỗi: "Vui lòng xác thực PIN trước"
- PIN đã hết hạn (2 giờ)
- Đăng xuất và xác thực lại

### Lỗi: "Câu lệnh chứa từ khóa nguy hiểm"
- Hệ thống chặn các lệnh nguy cơ cao
- Liên hệ DBA nếu thực sự cần thiết

### Không thấy menu
- Kiểm tra user có `IsSuperAdmin = true` hay không
- Chỉ Super Admin mới thấy menu

## Use Cases

### 1. Sửa lỗi dữ liệu khẩn cấp
```sql
-- Khách hàng báo email sai
UPDATE [dbo].[Customer]
SET Email = 'correct@email.com'
WHERE Id = 789
```

### 2. Thêm cấu hình mới
```sql
-- Thêm setting mới
INSERT INTO [dbo].[Parameter] (Id, Name, Value, PortalId, Language)
VALUES (NEWID(), 'NewFeature', 'Enabled', 1, 'vi')
```

### 3. Kiểm tra dữ liệu
```sql
-- Đếm số bài viết theo trạng thái
SELECT Status, COUNT(*) as Total
FROM [dbo].[ContentPage]
GROUP BY Status
```

### 4. Dọn dẹp dữ liệu test
```sql
-- Xóa contact test
DELETE FROM [dbo].[Contact]
WHERE Email LIKE '%test%'
AND CreatedDate < DATEADD(day, -7, GETDATE())
```

## Lợi ích

### Cho Developer
- Không cần remote vào server
- Sửa lỗi nhanh chóng
- Debug dễ dàng
- Export dữ liệu để phân tích

### Cho Khách hàng
- Giải quyết vấn đề nhanh
- Downtime tối thiểu
- Không cần chờ deploy

### Cho Hệ thống
- Log đầy đủ cho audit
- Bảo mật cao với attribute phân quyền
- Kiểm soát rủi ro

## Kiến trúc

### Attribute Bảo mật
```csharp
[IsSupperAdminAuthorizePermission]
```
Tự động kiểm tra:
- Authentication (đã đăng nhập)
- Authorization (IsSuperAdmin = true)
- Account status (không bị lock)
- Auto redirect nếu fail

### PIN Authentication
- Layer bảo mật thứ 2
- Stored in memory cache
- Timeout: 2 giờ
- Mỗi user có session PIN riêng

### Query Validation
- Keyword blacklist
- SQL injection prevention
- Timeout protection
- Result size limit

## Hỗ trợ

Nếu gặp vấn đề, vui lòng:
1. Kiểm tra log trong Database Query Tool
2. Xem log hệ thống: `logs/error_*.log`
3. Kiểm tra quyền Super Admin
4. Liên hệ Team Leader hoặc Database Administrator

---

**LƯU Ý QUAN TRỌNG**:
Công cụ này rất mạnh mẽ nhưng cũng rất nguy hiểm nếu sử dụng sai. Luôn thận trọng khi thực thi UPDATE/DELETE trên production database!

**Phát triển bởi**: PT Development Team  
**Phiên bản**: 1.0.0  
**Ngày cập nhật**: 2025  
**PIN mặc định**: `DB_ADMIN_2025` (cài định trong code)  
**Phân quyền**: `[IsSupperAdminAuthorizePermission]`
