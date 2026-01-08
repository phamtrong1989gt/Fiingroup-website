# ?? Auto CSS Quick Start Guide

## B??c 1: Build & Run Project

```bash
# Build solution
dotnet build

# Run Portal2
cd 6.FE/Portal2
dotnet run
```

## B??c 2: Test Auto CSS System

### A. Truy c?p Test Page

```
http://localhost:5000/vi/home/autocsstest
```

Test page s? hi?n th?:
- ? Current route info (controller/action/language)
- ? Current link info (ID/name/slug/language)
- ? Expected CSS file name
- ? File status (exists/not found)
- ? Actions: Check/Create/Open CSS file
- ? CSS content preview

### B. Test v?i Homepage

```
http://localhost:5000/vi
```

Check HTML source ? Tìm dòng:
```html
<!-- Auto-generated CSS -->
<link rel="stylesheet" href="/auto-css/link_123_vi.css" data-auto-css="link_123_vi" />
```

### C. Verify File Created

Check th? m?c:
```
6.FE/Portal2/wwwroot/auto-css/
```

S? th?y file: `link_123_vi.css` (123 là LinkId c?a homepage)

## B??c 3: Debug N?u Có L?i

### Check Debug Output

1. **Visual Studio**: M? `Output` window ? Select `Debug`
2. **VS Code**: M? `Debug Console`

S? th?y logs:
```
[AutoCssService] WebRootPath: D:\...\6.FE\Portal2\wwwroot
[AutoCssService] Checking directory: D:\...\6.FE\Portal2\wwwroot\auto-css
[AutoCssService] Creating directory: ...
[AutoCssService] Directory created successfully
[AutoCssService] GetCssFilePathAsync called: linkId=123, language=vi
[AutoCssService] Target file path: D:\...\link_123_vi.css
[AutoCssService] File does not exist, creating: ...
[AutoCssService] File created successfully: ...
```

### Common Issues

#### ? File không ???c t?o

**Check 1: WebRootPath có ?úng không?**
```
Debug log: [AutoCssService] WebRootPath: ???
```
Ph?i là: `D:\...\6.FE\Portal2\wwwroot`

**Check 2: Permission?**
```
Right-click wwwroot folder ? Properties ? Security
? Ensure IIS_IUSRS / NETWORK SERVICE have Write permission
```

**Check 3: Service registered?**
```csharp
// In Startup.cs, check line:
services.AddScoped<IAutoCssService, AutoCssService>();
```

**Check 4: LinkData có valid ID?**
```
Debug: linkData?.Id = ??? (ph?i > 0)
```

#### ? CSS không load

**Check 1: File exists?**
```
-GET /AutoCss/Exists?controller=Home&action=Index&language=vi
+GET /AutoCss/Exists?linkId=123&language=vi
```

Response should be:
```json
{
  "exists": true,
-  "url": "/auto-css/home_index_vi.css"
+  "url": "/auto-css/link_123_vi.css"
}
```

**Check 2: Try force create:**
```
-GET /AutoCss/Create?controller=Home&action=Index&language=vi
+GET /AutoCss/Create?linkId=123&language=vi
```

**Check 3: Access CSS directly:**
```
-http://localhost:5000/auto-css/home_index_vi.css
+http://localhost:5000/auto-css/link_123_vi.css
```

#### ? Exception in logs

```
[AutoCssService] ERROR creating file: Access to the path is denied
```

**Solution:**
1. Run Visual Studio as Administrator
2. Check folder permissions
3. Check antivirus blocking

## B??c 4: Edit CSS File

1. **Locate file:**
```
-6.FE/Portal2/wwwroot/auto-css/home_index_vi.css
+6.FE/Portal2/wwwroot/auto-css/link_123_vi.css
```

2. **Edit content:**
```css
/* 
 * Auto-generated CSS file
- * Controller: Home
- * Action: Index
+ * Link ID: 123
 * Language: vi
 * Generated: 2025-01-20 14:30:00
 * 
 * Add your custom CSS styles here
 */

/* Your custom styles */
.home-banner {
    background: linear-gradient(135deg, #0066CC 0%, #034ea2 100%);
    padding: 60px 0;
    color: white;
}

.home-features {
    margin-top: 40px;
}

.home-features h2 {
    color: #093461;
    font-size: 32px;
    margin-bottom: 20px;
}
```

3. **Refresh browser:**
```
Ctrl + F5 (hard refresh)
```

## B??c 5: Test Multiple Views

### Create CSS for different pages:

```
# Tin t?c page - Vietnamese (LinkId = 456)
http://localhost:5000/vi/tin-tuc.html

# About page - English (LinkId = 789)
http://localhost:5000/en/about-us.html

# Contact page - Vietnamese (LinkId = 101)
http://localhost:5000/vi/lien-he.html
```

Check `wwwroot/auto-css/` folder:
```
??? link_123_vi.css  # Homepage Vietnamese
??? link_123_en.css  # Homepage English
??? link_456_vi.css  # News page Vietnamese
??? link_789_en.css  # About page English
??? link_101_vi.css  # Contact page Vietnamese
```

## ?? Success Checklist

- [ ] Build project successfully
- [ ] Run project without errors
- [ ] Access test page `/home/autocsstest`
- [ ] See debug logs in Output window
- [ ] File created in `wwwroot/auto-css/`
- [ ] CSS link tag in HTML source
- [ ] CSS content loads in browser
- [ ] Edit CSS file ? see changes
- [ ] Multiple pages create multiple files (by LinkId)

## ?? Need Help?

### Check these files:

1. **Service Registration**
```
6.FE/Portal2/Startup.cs
Line: services.AddScoped<IAutoCssService, AutoCssService>();
```

2. **Helper Import**
```
6.FE/Portal2/Views/_ViewImports.cshtml
Line: @using PT.UI.Helpers
```

3. **Layout Injection**
```
6.FE/Portal2/Views/Shared/_Home.cshtml
-Line: @Html.AutoCss()
+Line: @Html.AutoCss(linkData?.Id ?? 0)
```

### Diagnostic Endpoints

```
# Check file exists
-GET /AutoCss/Exists?controller=Home&action=Index&language=vi
+GET /AutoCss/Exists?linkId=123&language=vi

# Force create file
-GET /AutoCss/Create?controller=Home&action=Index&language=vi
+GET /AutoCss/Create?linkId=123&language=vi

# Get CSS content
-GET /AutoCss/GetCss?controller=Home&action=Index&language=vi
+GET /AutoCss/GetCss?linkId=123&language=vi
```

### Debug Commands

```bash
# Check wwwroot path
dir 6.FE\Portal2\wwwroot

# Check auto-css folder
dir 6.FE\Portal2\wwwroot\auto-css

# Create folder manually if needed
mkdir 6.FE\Portal2\wwwroot\auto-css
