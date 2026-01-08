# Auto CSS with File Versioning

## ?? How it works

Khi b?n dùng `@Html.AutoCss(linkId)`, h? th?ng s?:

1. **Generate CSS file** n?u ch?a t?n t?i
2. **Calculate content hash** c?a file
3. **Append version query string**: `?v={hash}`
4. **Output link tag**: `<link href="/auto-css/link_123_vi.css?v=abc123def" />`

## ?? Example Output

### Before (no versioning)
```html
<link rel="stylesheet" href="/auto-css/link_123_vi.css" />
```

### After (with versioning)
```html
<!-- Auto-generated CSS -->
<link rel="stylesheet" href="/auto-css/link_123_vi.css?v=9fj8s7djf83jd" data-auto-css="link_123_vi" />
```

## ? Benefits

### 1. **Auto Cache Busting**
- File content thay ??i ? Hash thay ??i ? Browser download file m?i
- Không c?n manually clear cache
- Không c?n hardcode version numbers

### 2. **Browser Caching**
- File không ??i ? Hash không ??i ? Browser dùng cached version
- Reduce server load
- Faster page load

### 3. **Production Ready**
- Same behavior as `asp-append-version="true"`
- Works with CDN
- No manual intervention needed

## ?? Cache Invalidation Flow

```
Developer edits: link_123_vi.css
       ?
File content changes
       ?
Hash recalculated: abc123 ? xyz789
       ?
New URL: link_123_vi.css?v=xyz789
       ?
Browser sees new URL ? Downloads fresh file
```

## ?? Testing

### Step 1: View Page Source

```bash
# Access homepage
http://localhost:5000/vi

# View source (Ctrl+U)
# Find line:
<link rel="stylesheet" href="/auto-css/link_7139_vi.css?v=..." />
```

### Step 2: Note the version hash

```
Current: ?v=9fj8s7djf83jd
```

### Step 3: Edit CSS file

```css
/* Add new style */
.test-class {
    color: red;
}
```

### Step 4: Refresh page & view source

```
New: ?v=3jd9sk2ld8sjf  ? Hash changed!
```

### Step 5: Verify browser downloaded new file

- Open DevTools ? Network tab
- Filter: CSS
- Look for `link_7139_vi.css?v=3jd9sk2ld8sjf`
- Status: 200 (not 304 - cache)

## ?? How Version Hash is Calculated

```csharp
// ASP.NET Core uses SHA256 hash of file content
// Truncated to ~20 characters for URL friendliness

File: link_123_vi.css
Content: "/* CSS styles */"
   ?
SHA256 Hash: d7a8fbb307d7809469ca9abcb0082e4f...
   ?
Truncated: d7a8fbb307d7809469
   ?
URL: /auto-css/link_123_vi.css?v=d7a8fbb307d7809469
```

## ?? Best Practices

### ? DO

**1. Edit CSS files directly**
```bash
# Navigate to file
6.FE/Portal2/wwwroot/auto-css/link_123_vi.css

# Edit and save
# Version auto-updates on next page load
```

**2. Test in Incognito mode**
```
Ctrl + Shift + N (Chrome)
Test without cache interference
```

**3. Commit CSS files to Git**
```bash
git add wwwroot/auto-css/*.css
git commit -m "Update page styles"
```

### ? DON'T

**1. Don't manually add ?v= to URL**
```html
<!-- ? Wrong -->
<link href="/auto-css/link_123_vi.css?v=1" />

<!-- ? Correct -->
@Html.AutoCss(123)
```

**2. Don't cache CSS files with long expiration**
```
Cache-Control: max-age=31536000  ? OK (versioned URL)
Cache-Control: no-cache          ? Not needed (version handles it)
```

**3. Don't forget to build/restart after code changes**
```bash
# After changing AutoCssHelper.cs
dotnet build
# Restart application
```

## ?? Troubleshooting

### Version không thay ??i sau khi edit CSS?

**Check 1: File saved?**
```
- Ctrl+S to save
- Check file timestamp
- Verify content changed
```

**Check 2: Hard refresh browser**
```
Ctrl + F5 (Windows)
Cmd + Shift + R (Mac)
```

**Check 3: Clear Output Cache**
```csharp
// In production, may need to clear output cache
// Restart application if needed
```

**Check 4: Check file permissions**
```
- File must be readable by application
- WebRootPath must be correct
```

### CSS không load (404)?

**Check 1: File exists?**
```bash
dir 6.FE\Portal2\wwwroot\auto-css\link_123_vi.css
```

**Check 2: URL correct?**
```
View source ? Check href attribute
Should be: /auto-css/link_123_vi.css?v=...
```

**Check 3: Static Files middleware enabled?**
```csharp
// In Startup.cs
app.UseStaticFiles();
```

## ?? Performance Impact

### Before (no versioning)
```
Problem: Users stuck with old CSS until cache expires
         OR need to manually clear cache
         OR use Ctrl+F5
```

### After (with versioning)
```
Solution: New CSS auto-downloaded when content changes
          Old CSS cached indefinitely (good performance)
          No user intervention needed
```

### Metrics
- **Hash calculation**: ~1ms per file
- **Memory overhead**: Negligible (hash cached internally)
- **Network impact**: None (query string doesn't increase file size)

## ?? Production Deployment

### Step 1: Build project
```bash
dotnet publish -c Release
```

### Step 2: Deploy to server
```
- Copy wwwroot/auto-css/ folder
- Ensure IIS/nginx can read files
- Verify permissions
```

### Step 3: Verify versioning works
```bash
# Access production site
curl -I https://yoursite.com/auto-css/link_123_vi.css?v=...

# Should return:
Cache-Control: public,max-age=...
ETag: "..."
```

### Step 4: Monitor
```
- Check browser console for 404s
- Verify CSS applies correctly
- Test cache behavior
```

## ?? Advanced: Custom Version Strategy

If you need custom versioning (e.g., based on build number):

```csharp
// Option 1: Use build timestamp
var version = DateTime.Now.Ticks.ToString();
var url = $"/auto-css/link_{linkId}_{language}.css?v={version}";

// Option 2: Use assembly version
var version = Assembly.GetExecutingAssembly().GetName().Version;
var url = $"/auto-css/link_{linkId}_{language}.css?v={version}";

// Option 3: Use custom config
var version = Configuration["CssVersion"];
var url = $"/auto-css/link_{linkId}_{language}.css?v={version}";
```

But **IFileVersionProvider is recommended** because:
- ? Content-based (only changes when file changes)
- ? Built-in to ASP.NET Core
- ? Same behavior as `asp-append-version`
- ? No manual version management

## ?? Summary

| Feature | Before | After |
|---------|--------|-------|
| Cache busting | Manual | ? Automatic |
| User experience | Must clear cache | ? Seamless |
| Performance | Suboptimal | ? Optimized |
| Maintenance | High | ? Low |
| Production ready | ? No | ? Yes |

---

**Your CSS files now have the same cache-busting power as built-in ASP.NET Core assets!** ??
