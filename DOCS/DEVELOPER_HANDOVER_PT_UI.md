# Tài liệu bàn giao — PT.UI (Admin)

Mục đích: Tài liệu này chỉ tập trung hướng dẫn cho phần Admin UI của hệ thống (project `1.Backend`). Mục tiêu giúp lập trình viên khác nhanh nắm cấu trúc, cấu hình, cách chạy và cấu hình các file JSON liên quan.

---

## 1. Phạm vi tài liệu
- Chỉ mô tả project Admin (Admin UI) nằm trong `1.Backend`.
- Không đi sâu vào các Portal FE (Portal1/Portal2) trừ khi có liên quan đến cấu hình chung (appsettings).

## 2. Vị trí chính của project Admin
- Project chính: `1.Backend` (đường dẫn workspace: `1.Backend/`)
- Các file khởi động: `Program.cs`, `Startup.cs` trong `1.Backend`.
- DbContext: `3.Infrastructure/ApplicationContext.cs` (sử dụng chung).
- Custom routing & common helpers: `5.Base/` (ví dụ `CustomRouter`, `UrlRequestCultureProvider`).

## 3. Cách chạy local (Admin)
1. Cài đặt prerequisites: `.NET 10 SDK`, SQL Server (hoặc SQL Server Express).
2. Mở solution trong Visual Studio hoặc dùng CLI.
3. Cập nhật kết nối DB trong `1.Backend/appsettings.json` (key `ConnectionStrings:DefaultConnection`).
4. Tạo hoặc chạy migrations:
   - Tạo migration: `dotnet ef migrations add Init -p 3.Infrastructure -s 1.Backend`
   - Áp dụng migration: `dotnet ef database update -p 3.Infrastructure -s 1.Backend`
5. Chạy project `1.Backend` (Set as startup) hoặc `dotnet run --project 1.Backend`.
6. Truy cập Admin: theo route area `Admin`, ví dụ `/Admin` (login page nằm trong Views Shared `_Login`).

## 4. Các file cấu hình JSON và mô tả (quan trọng)
Project Admin/Program.cs và các frontend Program.cs đều load một số file JSON bổ sung. Dưới đây là danh sách file thường xuất hiện và mô tả các khóa quan trọng cần chú ý.

- `appsettings.json` (bắt buộc)
  - `ConnectionStrings:DefaultConnection` — chuỗi kết nối SQL Server.
  - `Logging` — cấu hình logging cơ bản (khi không dùng Serilog).
  - `AllowedHosts` — host cho môi trường.

- `appsettings.Base.json`
  - `BaseSettings` (POCO `BaseSettings` trong `PT.Domain.Model`):
    - `PortalId` — id cổng mặc định.
    - `DefaultLanguage` — `vi` / `en`.
    - `MultipleLanguage` — bật/tắt đa ngôn ngữ.
    - `DataPath` — đường dẫn tới thư mục dữ liệu dùng chung (dùng trong `Startup` để map `PhysicalFileProvider` tới `/Data`). Nếu rỗng, mặc định `SharedData/Data` trong `ContentRoot`.
    - Các khóa cấu hình khác dùng bởi app (đọc POCO để biết chi tiết).

- `appsettings.Authorize.json`
  - Cấu hình liên quan tới phân quyền/Authorization (role mapping, các setting custom cho permission module).

- `appsettings.Email.json` (hoặc `EmailSettings` trong `appsettings.json`)
  - `EmailSettings` (POCO `EmailSettings`):
    - `SmtpHost`, `Port`, `EnableSsl`, `UserName`, `Password`, `FromAddress`, `FromName`.
  - Dùng bởi `IEmailSettingRepository` và email sending service.

- `appsettings.Socket.json`
  - `SocketSettings`: host, port, timeout, health-check setting.
  - Ảnh hưởng tới các service socket trong `4.Shared` hoặc `Base`.

- `appsettings.Paypal.json`
  - `PaypalSettings`: client id, secret, sandbox/production flags.

- `appsettings.Log.json`
  - `LogSettings` (POCO): bật/tắt logging, `IsUseMongo` (nếu log vào MongoDB), cấu hình mức độ.
  - Lưu ý: Serilog trong `Startup` viết file log vào thư mục `logs/` theo level; `appsettings.Log.json` có thể điều khiển lưu trong DB/Mongo nếu code hỗ trợ.

- `appsettings.RedirectLink.json` / `appsettings.AdvertisingHomepage.json` / `appsettings.AsyncNews.json` / `appsettings.Misa.json` (nếu có)
  - Các file này chứa cấu hình từng tính năng modular: redirect rules, cấu hình quảng cáo, cấu hình xử lý tin tức bất đồng bộ, cấu hình tích hợp MISA,...
  - Kiểm tra `Program.cs` để biết project có load file nào (mỗi project có danh sách file khác nhau).

Lưu ý: `Program.cs` trong `1.Backend` có đoạn `config.AddJsonFile("appsettings.Base.json", optional: true, reloadOnChange: true)` — nếu file optional nhưng mã code phụ thuộc vào các key trong file này thì phải đảm bảo file tồn hoặc có giá trị thay thế mặc định.

## 5. Mẫu appsettings (không chứa secrets) — ví dụ minimal
- `appsettings.json` (ví dụ):

```
{
  "ConnectionStrings": {
    "DefaultConnection": "Server=localhost;Database=PT_DB;Trusted_Connection=True;TrustServerCertificate=True;"
  },
  "Logging": {
    "LogLevel": {
      "Default": "Information",
      "Microsoft": "Warning"
    }
  },
  "AllowedHosts": "*"
}
```

- `appsettings.Base.json` (ví dụ):

```
{
  "BaseSettings": {
    "PortalId": 1,
    "DefaultLanguage": "vi",
    "MultipleLanguage": true,
    "DataPath": "SharedData/Data"
  }
}
```

- `appsettings.Email.json` (ví dụ):

```
{
  "EmailSettings": {
    "SmtpHost": "smtp.example.com",
    "Port": 587,
    "EnableSsl": true,
    "UserName": "no-reply@example.com",
    "Password": "(secrets-not-in-repo)",
    "FromAddress": "no-reply@example.com",
    "FromName": "PT Admin"
  }
}
```

Ghi chú: không commit mật khẩu/secret. Dùng user-secrets hoặc biến môi trường trên CI/CD.

## 6. Cấu hình logging (Serilog)
- `Startup` khởi tạo Serilog để ghi file log per-level vào `logs/`.
- `loggerFactory.AddSerilog();` được gọi trong `Configure`.
- Đảm bảo thư mục `logs/` có quyền ghi hoặc cấu hình Serilog để ghi vào nơi khác.

## 7. Static files và Shared Data
- `app.UseStaticFiles()` phục vụ nội dung `wwwroot`.
- `Startup.Configure` ánh xạ `PhysicalFileProvider` cho `DataPath`:
  - `RequestPath = "/Data"` — các file dữ liệu có thể được truy cập qua `/Data/...`.
  - Nếu `BaseSettings:DataPath` là một đường dẫn tuyệt đối, `Startup` dùng nó; nếu rỗng dùng `ContentRoot/SharedData/Data`.
- Khi deploy, đảm bảo `DataPath` trỏ tới nơi chứa ảnh/tài nguyên và có quyền đọc/ghi nếu upload.

## 8. Authentication / Identity (Admin)
- Identity cấu hình bằng EF store (`ApplicationContext`) trong `Startup.ConfigureServices`.
- Cookie settings:
  - `LoginPath = "/Login"`
  - `AccessDeniedPath = "/Admin/AccessDenied"`
  - `ExpireTimeSpan` thường 2 giờ (xem Startup).
- Để seed admin user: tạo script seed SQL hoặc EF seed (nên nằm trong dự án `3.Infrastructure` hoặc một migration seed).

## 9. DI & Repositories
- `Startup` đăng ký các repository bằng `AddScoped`.
- Quy trình thêm repository: interface -> implementation -> đăng ký trong `Startup`.

## 10. Debug & lỗi phổ biến (Admin)
- Lỗi kết nối DB: kiểm tra `ConnectionStrings:DefaultConnection`, quyền user.
- Thiếu key `BaseSettings`: kiểm tra `appsettings.Base.json` và `Program.cs` load files.
- File Data không tồn tại: kiểm tra `BaseSettings:DataPath` và quyền folder.
- Lỗi phân quyền: kiểm tra bảng Role/RoleDetail và attribute `Authorize`/`IsSupperAdminAuthorizePermission`.

## 11. Checklist bàn giao cho Admin UI
- [ ] Cung cấp file `appsettings.json` và các `appsettings.*.json` môi trường (Dev/Staging/Prod). Không commit secrets.
- [ ] Cung cấp chuỗi kết nối DB hoặc DB dump + user credential.
- [ ] Script / mã seed admin user + role.
- [ ] Ghi chú giá trị `BaseSettings:DataPath` và quyền thư mục.
- [ ] Hướng dẫn rebuild static assets (nếu admin UI có bước build Node/NPM) — kiểm tra `wwwroot/Content/Admin`.
- [ ] Hướng dẫn cấu hình Serilog nếu cần lưu ở nơi khác.

---

Tôi có thể tiếp tục thực hiện một trong các việc sau (chọn 1):
1. Tạo các file template `appsettings.Development.json`, `appsettings.Production.json` (không chứa secrets).
2. Viết script EF Core seed để tạo admin user + role.
3. Tách checklist deploy thành hướng dẫn CI/CD (IIS hoặc Docker).

Cho biết lựa chọn bạn muốn tôi làm tiếp.