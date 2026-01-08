# Auto CSS Troubleshooting Steps

## ?? Step 1: Check Debug Output

### Open Output Window
```
Visual Studio:
View ? Output ? Select "Debug" from dropdown

VS Code:
View ? Debug Console
```

### What to look for:
```
[AutoCssHelper] Called with linkId=123
[AutoCssHelper] Language=vi
[AutoCssHelper] Creating CSS file for linkId=123, language=vi
[AutoCssService] WebRootPath: D:\...\wwwroot
[AutoCssService] Checking directory: D:\...\wwwroot\auto-css
[AutoCssService] File created successfully
[AutoCssHelper] Generated CSS URL: /auto-css/link_123_vi.css
[AutoCssHelper] Versioned URL: /auto-css/link_123_vi.css?v=...
[AutoCssHelper] Generated HTML: <link .../>
```

## ?? Common Errors & Solutions

### Error 1: "AutoCss ERROR: Service not registered"

**In HTML:**
```html
<!-- AutoCss ERROR: Service not registered for linkId=123 -->
```

**Solution:**
```csharp
// Check Startup.cs line ~328
services.AddScoped<IAutoCssService, AutoCssService>();
```

**Verify:**
1. Line exists in Startup.cs
2. Restart application
3. Check Output window for errors

---

### Error 2: "ERROR: AutoCssService is NULL"

**In Debug Output:**
```
[AutoCssHelper] ERROR: AutoCssService is NULL! Service not registered in DI?
```

**Solution:**
```bash
# Rebuild solution
dotnet clean
dotnet build

# Check for build errors
# Restart application
```

---

### Error 3: "ERROR creating CSS file: Access denied"

**In Debug Output:**
```
[AutoCssService] ERROR creating file: Access to the path is denied
```

**Solution:**
```bash
# Check folder permissions
Right-click wwwroot folder ? Properties ? Security
Ensure Users / IIS_IUSRS have Write permission

# Or run Visual Studio as Administrator
```

---

### Error 4: "File does not exist" after creation

**In Debug Output:**
```
[AutoCssService] File created successfully: D:\...\link_123_vi.css
[AutoCssHelper] ERROR in AddFileVersionToPath: File not found
```

**Solution:**
```csharp
// Possible WebRootPath mismatch
// Check Startup.cs Configure method:
app.UseStaticFiles();  // Must be present
```

---

### Error 5: No output in Debug window

**Symptoms:**
- No logs appear
- HTML shows nothing

**Solution:**
```
1. Check Output window is set to "Debug" (not "Build")
2. Rebuild solution
3. Hard refresh browser (Ctrl + F5)
4. Check if @Html.AutoCss() is being called
```

---

## ? Quick Diagnostic Test

### Test 1: Check Service Registration

**Run this in a controller action:**
```csharp
public IActionResult TestAutoCss()
{
    var service = HttpContext.RequestServices
        .GetService(typeof(IAutoCssService));
    
    if (service == null)
    {
        return Content("ERROR: Service NOT registered!");
    }
    
    return Content("SUCCESS: Service is registered");
}
```

**Access:** `http://localhost:5000/home/testautocss`

**Expected:** `SUCCESS: Service is registered`

---

### Test 2: Check File Creation

**Access test page:**
```
http://localhost:5000/vi/home/autocsstest
```

**Click buttons:**
1. "Check File Exists" ? Should return JSON
2. "Force Create File" ? Should create file
3. "Open CSS File" ? Should show CSS content

---

### Test 3: Check HTML Output

**View page source:** `Ctrl + U`

**Look for:**
```html
<!-- Auto-generated CSS -->
<link rel="stylesheet" href="/auto-css/link_123_vi.css?v=..." />
```

**If you see:**
```html
<!-- AutoCss ERROR: ... -->
```
? Check error message and follow solutions above

---

### Test 4: Manual File Check

**Navigate to folder:**
```
D:\CongViec\Job\2025\5.Fiingroup-website\6.FE\Portal2\wwwroot\auto-css\
```

**Check:**
1. Folder exists?
2. Files inside?
3. File names match pattern `link_{id}_{lang}.css`?
4. Files not empty?

---

## ?? Manual Fix Steps

### If all else fails:

**Step 1: Create folder manually**
```bash
mkdir D:\CongViec\Job\2025\5.Fiingroup-website\6.FE\Portal2\wwwroot\auto-css
```

**Step 2: Create test file**
```bash
# Create: wwwroot/auto-css/link_0_vi.css
/* Test file */
.test { color: red; }
```

**Step 3: Test access**
```
http://localhost:5000/auto-css/link_0_vi.css
```

**Expected:** CSS content displayed

**If 404:**
- Check UseStaticFiles() in Startup.cs
- Check file path is correct
- Restart application

---

## ?? Checklist

Before asking for help, verify:

- [ ] Service registered in Startup.cs
- [ ] Project builds without errors
- [ ] Application running (not crashed)
- [ ] linkData?.Id has valid value (> 0)
- [ ] wwwroot/auto-css folder exists
- [ ] Folder has write permissions
- [ ] Static files middleware enabled
- [ ] Output window shows debug logs
- [ ] Browser not caching old HTML (Ctrl+F5)
- [ ] Antivirus not blocking file creation

---

## ?? Get Help

### Provide this info:

1. **Error message** from HTML source
2. **Debug logs** from Output window
3. **File structure**: Does wwwroot/auto-css exist?
4. **URL** you're accessing
5. **linkData.Id** value
6. **Build output**: Any warnings/errors?

### Example good report:

```
Problem: CSS link not appearing in HTML

HTML source shows:
<!-- AutoCss ERROR: Service not registered for linkId=123 -->

Debug logs:
[AutoCssHelper] Called with linkId=123
[AutoCssHelper] ERROR: AutoCssService is NULL!

Checked:
? Startup.cs has services.AddScoped<IAutoCssService, AutoCssService>()
? Build successful (no errors)
? Application running
? linkData.Id = 123
? No debug logs from AutoCssService

What I tried:
1. Rebuild solution
2. Restart VS
3. Clear browser cache
```

---

**Good luck debugging! ????**
