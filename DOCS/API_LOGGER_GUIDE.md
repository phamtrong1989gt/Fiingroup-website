# ?? H??ng D?n S? D?ng API Logger Service

## ?? T?ng Quan

H? th?ng API Logger Service ?ã ???c tích h?p vào `NewsAPIService` ??:
- ? **Theo dõi hi?u n?ng**: Ghi l?i th?i gian th?c thi m?i API call
- ? **Phát hi?n spam**: D? dàng tra c?u s? l?n g?i API trong 1 kho?ng th?i gian
- ? **Debug l?i**: Log chi ti?t các l?i 401, 500, timeout...
- ? **Phân tích**: Xác ??nh API nào ch?m, API nào l?i nhi?u
- ? **Multi-portal tracking**: Theo dõi riêng bi?t cho t?ng portal (Portal1, Portal2...)

---

## ?? Multi-Portal Support

**T?T C?** các method trong `INewsAPIService` ?ã ???c thêm parameter `portalId`:

```csharp
// Ví d?: G?i API t? Portal 1
var news = await _newsAPIService.GetNewsAsync(parameters, language: "vi", portalId: 1);

// Ví d?: G?i API t? Portal 2
var news = await _newsAPIService.GetNewsAsync(parameters, language: "vi", portalId: 2);
```

### Format Log M?i:
```
? [P1][VI] GET /api/news/Gets - 245ms
? [P2][EN] GET /api/news/Get?id=123 - 180ms
? [P1][VI] POST /token - 5200ms - Connection timeout
```

**Legend:**
- `[P1]` = Portal 1
- `[P2]` = Portal 2
- `[VI]` = Vietnamese
- `[EN]` = English

---

## ?? Các LogType M?i

?ã thêm vào `PT.Domain.Model.LogType`:

```csharp
// API Success
[Display(Name = "API - L?y Token")]
API_GetToken = 100,

[Display(Name = "API - Tin t?c (danh sách)")]
API_GetNews = 101,

[Display(Name = "API - Tin t?c (chi ti?t)")]
API_GetNewsDetail = 102,

[Display(Name = "API - ?i?m x?p h?ng")]
API_GetReportScores = 103,

[Display(Name = "API - Ngành ngh?")]
API_GetIndustries = 104,

[Display(Name = "API - Tri?n v?ng")]
API_GetOutlooks = 105,

[Display(Name = "API - Tài chính b?n v?ng")]
API_GetSustainableFinance = 106,

[Display(Name = "API - K?t qu? x?p h?ng")]
API_GetRatingResults = 107,

// API Errors
[Display(Name = "API - L?i 401")]
API_Error_Unauthorized = 200,

[Display(Name = "API - L?i khác")]
API_Error_Other = 201
```

---

## ?? Cách Tra C?u Log

### 1?? Xem T?t C? API Calls Theo Portal

```sql
-- Xem t?t c? API calls c?a Portal 1 hôm nay
SELECT 
    Type,
    Name,
    ActionTime,
    ObjectId AS StatusCode
FROM Log
WHERE 
    Type >= 100 AND Type <= 201
    AND Name LIKE '%[P1]%'
    AND CAST(ActionTime AS DATE) = CAST(GETDATE() AS DATE)
ORDER BY ActionTime DESC;
```

### 2?? So Sánh Hi?u N?ng Gi?a Các Portal

```sql
-- So sánh s? l?n g?i API gi?a Portal 1 và Portal 2 trong 24h qua
SELECT 
    CASE 
        WHEN Name LIKE '%[P1]%' THEN 'Portal 1'
        WHEN Name LIKE '%[P2]%' THEN 'Portal 2'
        ELSE 'Unknown'
    END AS Portal,
    Type,
    COUNT(*) AS SoLanGoi,
    AVG(CAST(SUBSTRING(Name, CHARINDEX('ms', Name) - 5, 4) AS INT)) AS ThoiGianTrungBinh_ms
FROM Log
WHERE 
    Type >= 100 AND Type <= 199
    AND ActionTime >= DATEADD(HOUR, -24, GETDATE())
GROUP BY 
    CASE 
        WHEN Name LIKE '%[P1]%' THEN 'Portal 1'
        WHEN Name LIKE '%[P2]%' THEN 'Portal 2'
        ELSE 'Unknown'
    END,
    Type
ORDER BY Portal, SoLanGoi DESC;
```

### 3?? Phát Hi?n Spam API Theo Portal

```sql
-- Portal nào ?ang g?i API nhi?u nh?t trong 1 gi? qua?
SELECT 
    CASE 
        WHEN Name LIKE '%[P1]%' THEN 'Portal 1'
        WHEN Name LIKE '%[P2]%' THEN 'Portal 2'
        ELSE 'Unknown'
    END AS Portal,
    Type,
    COUNT(*) AS SoLanGoi
FROM Log
WHERE 
    Type >= 100 AND Type <= 199
    AND ActionTime >= DATEADD(HOUR, -1, GETDATE())
GROUP BY 
    CASE 
        WHEN Name LIKE '%[P1]%' THEN 'Portal 1'
        WHEN Name LIKE '%[P2]%' THEN 'Portal 2'
        ELSE 'Unknown'
    END,
    Type
HAVING COUNT(*) > 100 -- C?nh báo n?u > 100 calls trong 1 gi?
ORDER BY SoLanGoi DESC;
```

### 4?? Tìm API Ch?m Nh?t Theo Portal

```sql
-- Top 10 API call ch?m nh?t c?a t?ng Portal
SELECT TOP 10
    CASE 
        WHEN Name LIKE '%[P1]%' THEN 'Portal 1'
        WHEN Name LIKE '%[P2]%' THEN 'Portal 2'
        ELSE 'Unknown'
    END AS Portal,
    Type,
    Name,
    ActionTime,
    CAST(SUBSTRING(Name, CHARINDEX('ms', Name) - 5, 4) AS INT) AS Duration_ms
FROM Log
WHERE 
    Type >= 100 AND Type <= 199
    AND ActionTime >= DATEADD(DAY, -1, GETDATE())
ORDER BY Duration_ms DESC;
```

### 5?? Phát Hi?n L?i 401 (Token H?t H?n) Theo Portal

```sql
-- Portal nào g?p nhi?u l?i 401 nh?t?
SELECT 
    CASE 
        WHEN Name LIKE '%[P1]%' THEN 'Portal 1'
        WHEN Name LIKE '%[P2]%' THEN 'Portal 2'
        ELSE 'Unknown'
    END AS Portal,
    COUNT(*) AS SoLanLoi401
FROM Log
WHERE 
    Type = 200 -- API_Error_Unauthorized
    AND ActionTime >= DATEADD(DAY, -1, GETDATE())
GROUP BY 
    CASE 
        WHEN Name LIKE '%[P1]%' THEN 'Portal 1'
        WHEN Name LIKE '%[P2]%' THEN 'Portal 2'
        ELSE 'Unknown'
    END
ORDER BY SoLanLoi401 DESC;
```

### 6?? Th?ng Kê Theo Portal + Language

```sql
-- API usage theo Portal và Language
SELECT 
    CASE 
        WHEN Name LIKE '%[P1]%' THEN 'Portal 1'
        WHEN Name LIKE '%[P2]%' THEN 'Portal 2'
        ELSE 'Unknown'
    END AS Portal,
    CASE 
        WHEN Name LIKE '%[VI]%' THEN 'Vietnamese'
        WHEN Name LIKE '%[EN]%' THEN 'English'
        ELSE 'Unknown'
    END AS Language,
    Type,
    COUNT(*) AS SoLanGoi
FROM Log
WHERE 
    Type >= 100 AND Type <= 199
    AND ActionTime >= DATEADD(DAY, -7, GETDATE())
GROUP BY 
    CASE 
        WHEN Name LIKE '%[P1]%' THEN 'Portal 1'
        WHEN Name LIKE '%[P2]%' THEN 'Portal 2'
        ELSE 'Unknown'
    END,
    CASE 
        WHEN Name LIKE '%[VI]%' THEN 'Vietnamese'
        WHEN Name LIKE '%[EN]%' THEN 'English'
        ELSE 'Unknown'
    END,
    Type
ORDER BY SoLanGoi DESC;
```

### 7?? T? L? Thành Công / Th?t B?i Theo Portal

```sql
-- Success rate c?a t?ng Portal
SELECT 
    CASE 
        WHEN Name LIKE '%[P1]%' THEN 'Portal 1'
        WHEN Name LIKE '%[P2]%' THEN 'Portal 2'
        ELSE 'Unknown'
    END AS Portal,
    Type,
    COUNT(*) AS TongSo,
    SUM(CASE WHEN Name LIKE '?%' THEN 1 ELSE 0 END) AS ThanhCong,
    SUM(CASE WHEN Name LIKE '?%' THEN 1 ELSE 0 END) AS ThatBai,
    CAST(SUM(CASE WHEN Name LIKE '?%' THEN 1 ELSE 0 END) * 100.0 / COUNT(*) AS DECIMAL(5,2)) AS TyLeThanhCong_Percent
FROM Log
WHERE 
    Type >= 100 AND Type <= 201
    AND ActionTime >= DATEADD(DAY, -7, GETDATE())
GROUP BY 
    CASE 
        WHEN Name LIKE '%[P1]%' THEN 'Portal 1'
        WHEN Name LIKE '%[P2]%' THEN 'Portal 2'
        ELSE 'Unknown'
    END,
    Type
ORDER BY Portal, Type;
```

---

## ?? Ví D? Log Entry

### ? API Call Thành Công (Portal 1)
```
Type: API_GetNews (101)
Name: ? [P1][VI] GET .../Gets - 245ms
ObjectId: 200
ActionTime: 2025-01-XX 14:30:15
Object: ExternalAPI
ObjectType: NewsAPIService
```

### ? API Call Thành Công (Portal 2)
```
Type: API_GetNewsDetail (102)
Name: ? [P2][EN] GET .../Get?id=123 - 180ms
ObjectId: 200
ActionTime: 2025-01-XX 14:30:20
Object: ExternalAPI
ObjectType: NewsAPIService
```

### ? API Call Th?t B?i (401) - Portal 1
```
Type: API_Error_Unauthorized (200)
Name: ? [P1][EN] GET .../Get?id=123 - 180ms - Token expired, retrying with new token
ObjectId: 401
ActionTime: 2025-01-XX 14:31:20
Object: ExternalAPI
ObjectType: NewsAPIService
```

### ? API Call L?i Khác - Portal 2
```
Type: API_Error_Other (201)
Name: ? [P2][VI] POST /token - 5200ms - Failed to get access token: Connection timeout
ObjectId: 500
ActionTime: 2025-01-XX 14:32:45
Object: ExternalAPI
ObjectType: NewsAPIService
```

---

## ?? Dashboard Queries (Multi-Portal)

### Query 1: Real-time Dashboard - All Portals
```sql
SELECT 
    CASE 
        WHEN Name LIKE '%[P1]%' THEN 'Portal 1'
        WHEN Name LIKE '%[P2]%' THEN 'Portal 2'
        ELSE 'Unknown'
    END AS Portal,
    Type,
    COUNT(*) AS Calls_Last5Min,
    AVG(CAST(SUBSTRING(Name, CHARINDEX('ms', Name) - 5, 4) AS INT)) AS AvgDuration_ms,
    MAX(CAST(SUBSTRING(Name, CHARINDEX('ms', Name) - 5, 4) AS INT)) AS MaxDuration_ms
FROM Log
WHERE 
    Type >= 100 AND Type <= 199
    AND ActionTime >= DATEADD(MINUTE, -5, GETDATE())
GROUP BY 
    CASE 
        WHEN Name LIKE '%[P1]%' THEN 'Portal 1'
        WHEN Name LIKE '%[P2]%' THEN 'Portal 2'
        ELSE 'Unknown'
    END,
    Type;
```

### Query 2: Portal Performance Comparison
```sql
-- So sánh hi?u n?ng gi?a các Portal trong 24h
SELECT 
    CASE 
        WHEN Name LIKE '%[P1]%' THEN 'Portal 1'
        WHEN Name LIKE '%[P2]%' THEN 'Portal 2'
        ELSE 'Unknown'
    END AS Portal,
    COUNT(*) AS TotalCalls,
    AVG(CAST(SUBSTRING(Name, CHARINDEX('ms', Name) - 5, 4) AS INT)) AS AvgDuration_ms,
    MAX(CAST(SUBSTRING(Name, CHARINDEX('ms', Name) - 5, 4) AS INT)) AS MaxDuration_ms,
    MIN(CAST(SUBSTRING(Name, CHARINDEX('ms', Name) - 5, 4) AS INT)) AS MinDuration_ms,
    SUM(CASE WHEN Name LIKE '?%' THEN 1 ELSE 0 END) AS SuccessCount,
    SUM(CASE WHEN Name LIKE '?%' THEN 1 ELSE 0 END) AS FailureCount
FROM Log
WHERE 
    Type >= 100 AND Type <= 199
    AND ActionTime >= DATEADD(HOUR, -24, GETDATE())
GROUP BY 
    CASE 
        WHEN Name LIKE '%[P1]%' THEN 'Portal 1'
        WHEN Name LIKE '%[P2]%' THEN 'Portal 2'
        ELSE 'Unknown'
    END;
```

---

## ?? L?u Ý Quan Tr?ng

### 1. **Hi?u N?ng**
- Log ???c ghi **async** b?ng `Task.Run()` ? **KHÔNG ?nh h??ng** ??n performance API call chính
- N?u ghi log th?t b?i ? **b? qua** (không throw exception)

### 2. **Dung L??ng Database**
- M?i API call = 1 log record
- **Multi-portal** có th? t?ng l??ng log g?p 2-3 l?n
- **Nên xóa log c? ??nh k?**:
```sql
-- Xóa log c? h?n 30 ngày
DELETE FROM Log
WHERE Type >= 100 AND Type <= 201
AND ActionTime < DATEADD(DAY, -30, GETDATE());
```

### 3. **Phát Hi?n Spam Theo Portal**
- N?u th?y **1 portal** có **h?n 1000 calls trong 1 phút** ? Có v?n ??!
- Ki?m tra:
  - Portal nào ?ang b? spam?
  - Có vòng l?p g?i API không?
  - Có bot/crawler targeting portal c? th? không?

---

## ??? Cách S? D?ng Trong Code

### Ví D? 1: G?i API T? Controller (Portal 1)
```csharp
public class NewsController : Controller
{
    private readonly INewsAPIService _newsAPI;
    private readonly IOptions<BaseSettings> _settings;

    public async Task<IActionResult> Index()
    {
        var portalId = _settings.Value.PortalId; // Portal 1 = 1, Portal 2 = 2
        
        var news = await _newsAPI.GetNewsAsync(
            new NewsQueryParameters { Page = 1, Limit = 10 },
            language: "vi",
            portalId: portalId
        );
        
        return View(news);
    }
}
```

### Ví D? 2: G?i API T? Background Service
```csharp
public class NewsBackgroundService : BackgroundService
{
    private readonly INewsAPIService _newsAPI;

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        while (!stoppingToken.IsCancellationRequested)
        {
            // Fetch news for Portal 1
            var newsP1 = await _newsAPI.GetNewsAsync(
                new NewsQueryParameters { Page = 1, Limit = 5 },
                language: "vi",
                portalId: 1
            );
            
            // Fetch news for Portal 2
            var newsP2 = await _newsAPI.GetNewsAsync(
                new NewsQueryParameters { Page = 1, Limit = 5 },
                language: "en",
                portalId: 2
            );
            
            await Task.Delay(TimeSpan.FromMinutes(5), stoppingToken);
        }
    }
}
```

---

## ? Checklist Khi Tri?n Khai

- [x] ?ã thêm `LogType` enum m?i
- [x] ?ã t?o `APILogModel` class v?i `PortalId`
- [x] ?ã t?o `IAPILoggerService` và `APILoggerService`
- [x] ?ã tích h?p vào `NewsAPIService`
- [x] ?ã thêm `portalId` parameter vào T?T C? methods
- [x] ?ã update `BuildLogName` ?? hi?n th? `[P1]`, `[P2]`...
- [x] ?ã ??ng ký DI trong `Startup.cs` (Portal1, Portal2, Backend)
- [x] Build thành công
- [ ] Test trên Development environment
- [ ] Ki?m tra log trong database v?i portalId
- [ ] T?o SQL queries ?? tra c?u theo portal
- [ ] T?o dashboard hi?n th? metrics theo portal

---

**Tác gi?:** PT.Base API Logger System  
**Ngày c?p nh?t:** 2025-01-XX  
**Version:** 2.0 (Multi-Portal Support)
