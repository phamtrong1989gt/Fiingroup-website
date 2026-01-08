# Auto CSS Generation System

## ?? T?ng quan

H? th?ng t? ??ng generate CSS file cho m?i Link (page) v?i pattern: `link_{linkId}_{language}.css`

### ? Features
- ? **Auto-generate CSS files** khi truy c?p page l?n ??u
- ? **Organize by Link ID**: M?i page (Link) có file CSS riêng
- ? **Multi-language support**: Riêng bi?t cho t?ng ngôn ng? (vi/en)
- ? **Security**: Sanitize file names, prevent directory traversal
- ? **Caching**: Response cache 1 gi? ?? optimize performance
- ? **Default template**: File CSS ???c t?o v?i comments h??ng d?n
- ? **Auto versioning**: File versioning v?i content hash (like `asp-append-version`)
- ? **Cache busting**: T? ??ng bust cache khi CSS file thay ??i

## ??? Ki?n trúc

```
5.Base/PT.Base/Services/
??? AutoCssService.cs          # Service qu?n lý CSS files

6.FE/Portal2/
??? Controllers/
?   ??? AutoCssController.cs   # Controller serve CSS content
??? Helpers/
?   ??? AutoCssHelper.cs       # HTML Helper inject CSS link
??? wwwroot/
    ??? auto-css/              # Th? m?c ch?a CSS files
-        ??? home_index_vi.css
-        ??? home_index_en.css
-        ??? category_news_vi.css
+        ??? link_123_vi.css
+        ??? link_123_en.css
+        ??? link_456_vi.css
        ??? ...
```

## ?? Cách s? d?ng

### 1. T? ??ng inject CSS (Recommended)

File `_Home.cshtml` ?ã ???c setup s?n:

```cshtml
-@Html.AutoCss()
+@Html.AutoCss(linkData?.Id ?? 0)
```

S? t? ??ng generate:
```html
<!-- Auto-generated CSS -->
<link rel="stylesheet" href="/auto-css/link_123_vi.css" data-auto-css="link_123_vi" />
```

**Note**: URL s? có version hash append t? ??ng:
```html
<link rel="stylesheet" href="/auto-css/link_123_vi.css?v=9fj8s7djf83jd" data-auto-css="link_123_vi" />
```

Version hash ???c tính d?a trên n?i dung file ? Khi file thay ??i, hash thay ??i ? Browser t? ??ng download file m?i.

### 2. Manual inject cho specific link

```cshtml
-@* Inject CSS cho Home/Index *@
-@Html.AutoCss("Home", "Index")
+@* Inject CSS cho Link ID = 123 *@
+@Html.AutoCss(123)
-
-@* Inject CSS cho Category/News v?i language c? th? *@
-@Html.AutoCss("Category", "News", "en")
```

## ?? File Naming Convention

-| Controller | Action | Language | Generated File |
-|-----------|--------|----------|----------------|
-| Home | Index | vi | `home_index_vi.css` |
-| Home | Index | en | `home_index_en.css` |
-| Category | News | vi | `category_news_vi.css` |
-| ContentPage | Blog | en | `contentpage_blog_en.css` |
+| Link ID | Language | Generated File | Page Example |
+|---------|----------|----------------|--------------|
+| 123 | vi | `link_123_vi.css` | Trang ch? |
+| 123 | en | `link_123_en.css` | Homepage |
+| 456 | vi | `link_456_vi.css` | Tin t?c |
+| 789 | en | `link_789_en.css` | About Us |

## ?? API Endpoints

### Get CSS Content
```
-GET /AutoCss/GetCss?controller=Home&action=Index&language=vi
+GET /AutoCss/GetCss?linkId=123&language=vi
```

Response:
```css
/* 
 * Auto-generated CSS file
- * Controller: Home
- * Action: Index
+ * Link ID: 123
 * Language: vi
 * Generated: 2025-01-20 10:30:45
 * 
 * Add your custom CSS styles here
 */
```

### Check File Existence
```
-GET /AutoCss/Exists?controller=Home&action=Index&language=vi
+GET /AutoCss/Exists?linkId=123&language=vi
```

Response:
```json
{
  "exists": true,
-  "url": "/auto-css/home_index_vi.css"
+  "url": "/auto-css/link_123_vi.css"
}
```

## ?? Cách ch?nh s?a CSS

1. **T? ??ng t?o file**: Truy c?p page l?n ??u ? File ???c t?o trong `wwwroot/auto-css/`

2. **Ch?nh s?a**: M? file CSS và thêm styles

```css
-/* wwwroot/auto-css/home_index_vi.css */
+/* wwwroot/auto-css/link_123_vi.css */

-/* Custom styles cho Home/Index (Vietnamese) */
+/* Custom styles cho Link 123 - Trang ch? (Vietnamese) */
.home-banner {
    background-color: #0066CC;
    padding: 40px;
}

.home-content h1 {
    color: #093461;
    font-size: 32px;
}
```

3. **Auto reload**: File ???c cache 1 gi?, xóa cache trình duy?t ?? test

## ?? Best Practices

### ? DO

- **Organize styles by page**: M?i page (Link) có file riêng
- **Use semantic class names**: `.link-123`, `.homepage-banner`
- **Comment your code**: Gi?i thích purpose c?a styles
- **Keep files small**: < 50KB per file
- **Test multiple languages**: ??m b?o styles work cho c? vi/en

### ? DON'T

- **Don't put global styles**: Dùng `custom.css` cho global styles
- **Don't duplicate**: N?u style dùng nhi?u page ? move to global
- **Don't hardcode**: Dùng CSS variables cho màu s?c, spacing
- **Don't forget minification**: Production nên minify CSS

## ?? Performance

- **Caching**: Response cached 1 gi?
- **Compression**: Auto compressed v?i Brotli/Gzip
- **Size**: Trung bình 2-5KB per file
- **Load time**: < 50ms (cached), < 200ms (first load)

## ?? Security

- **Input sanitization**: Lo?i b? invalid characters
- **LinkId validation**: Only accept positive integers
- **Content-Type validation**: Ch? serve `text/css`
- **Rate limiting**: Consider adding rate limit cho production

## ?? Troubleshooting

### CSS không load?

-1. Check file exists: `/AutoCss/Exists?controller=X&action=Y&language=vi`
+1. Check file exists: `/AutoCss/Exists?linkId=123&language=vi`
2. Clear browser cache: Ctrl + F5
3. Check console errors
4. Verify service registered: `services.AddScoped<IAutoCssService, AutoCssService>()`

### File không ???c t?o?

1. Check permissions: `wwwroot/auto-css/` ph?i writable
2. Check logs: Xem có exception không
3. Verify `IWebHostEnvironment.WebRootPath` ?úng
4. Restart application
+5. Check linkData has valid ID

### Styles không apply?

1. Check CSS selector specificity
2. Verify file order: Auto CSS load sau `custom.css`
3. Use browser DevTools ?? debug
4. Check if being overridden by other stylesheets

## ?? Migration t? inline styles

### Before (inline styles)
```cshtml
<style>
    .my-class { color: red; }
</style>
```

### After (auto CSS)
```cshtml
-@* Styles ?ã move vào wwwroot/auto-css/home_index_vi.css *@
-@Html.AutoCss()
+@* Styles ?ã move vào wwwroot/auto-css/link_123_vi.css *@
+@Html.AutoCss(linkData?.Id ?? 0)

````````

This is the description of what the code block changes:
Update TODO list to mark cache invalidation as done and add file versioning references

This is the code block that represents the suggested code change:

````````markdown
## ?? TODO / Future Enhancements

- [ ] Auto minification trong production
- [ ] SCSS/LESS support
-- [ ] Cache invalidation strategy
+- [x] ~~Cache invalidation strategy~~ ? Done (file versioning)
- [ ] Admin UI ?? edit CSS files
- [ ] Version control integration
- [ ] Auto backup old versions
- [ ] Diff viewer for changes
- [ ] CSS linting integration

## ?? Contributing

Khi thêm new view:
1. CSS file t? ??ng t?o khi truy c?p l?n ??u
2. Edit file trong `wwwroot/auto-css/`
3. Commit file vào Git
4. Document special styles n?u c?n
+5. Version hash t? ??ng update khi file thay ??i

## ?? References

- ASP.NET Core Static Files: https://docs.microsoft.com/en-us/aspnet/core/fundamentals/static-files
- HTML Helpers: https://docs.microsoft.com/en-us/aspnet/core/mvc/views/html-helpers
- Response Caching: https://docs.microsoft.com/en-us/aspnet/core/performance/caching/response
+- File Versioning: https://docs.microsoft.com/en-us/aspnet/core/mvc/views/tag-helpers/builtin-th/link-tag-helper#asp-append-version
+- IFileVersionProvider: https://docs.microsoft.com/en-us/dotnet/api/microsoft.aspnetcore.mvc.infrastructure.ifileversionprovider
````````


# Response
````````markdown
# Auto CSS Generation System

## ?? T?ng quan

H? th?ng t? ??ng generate CSS file cho m?i Link (page) v?i pattern: `link_{linkId}_{language}.css`

### ? Features
- ? **Auto-generate CSS files** khi truy c?p page l?n ??u
- ? **Organize by Link ID**: M?i page (Link) có file CSS riêng
- ? **Multi-language support**: Riêng bi?t cho t?ng ngôn ng? (vi/en)
- ? **Security**: Sanitize file names, prevent directory traversal
- ? **Caching**: Response cache 1 gi? ?? optimize performance
- ? **Default template**: File CSS ???c t?o v?i comments h??ng d?n
- ? **Auto versioning**: File versioning v?i content hash (like `asp-append-version`)
- ? **Cache busting**: T? ??ng bust cache khi CSS file thay ??i

## ??? Ki?n trúc

```
5.Base/PT.Base/Services/
??? AutoCssService.cs          # Service qu?n lý CSS files

6.FE/Portal2/
??? Controllers/
?   ??? AutoCssController.cs   # Controller serve CSS content
??? Helpers/
?   ??? AutoCssHelper.cs       # HTML Helper inject CSS link
??? wwwroot/
    ??? auto-css/              # Th? m?c ch?a CSS files
-        ??? home_index_vi.css
-        ??? home_index_en.css
-        ??? category_news_vi.css
+        ??? link_123_vi.css
+        ??? link_123_en.css
+        ??? link_456_vi.css
        ??? ...
```

## ?? Cách s? d?ng

### 1. T? ??ng inject CSS (Recommended)

File `_Home.cshtml` ?ã ???c setup s?n:

```cshtml
-@Html.AutoCss()
+@Html.AutoCss(linkData?.Id ?? 0)
```

S? t? ??ng generate:
```html
<!-- Auto-generated CSS -->
<link rel="stylesheet" href="/auto-css/link_123_vi.css" data-auto-css="link_123_vi" />
```

**Note**: URL s? có version hash append t? ??ng:
```html
<link rel="stylesheet" href="/auto-css/link_123_vi.css?v=9fj8s7djf83jd" data-auto-css="link_123_vi" />
```

Version hash ???c tính d?a trên n?i dung file ? Khi file thay ??i, hash thay ??i ? Browser t? ??ng download file m?i.

### 2. Manual inject cho specific link

```cshtml
-@* Inject CSS cho Home/Index *@
-@Html.AutoCss("Home", "Index")
+@* Inject CSS cho Link ID = 123 *@
+@Html.AutoCss(123)
-
-@* Inject CSS cho Category/News v?i language c? th? *@
-@Html.AutoCss("Category", "News", "en")
```

## ?? File Naming Convention

-| Controller | Action | Language | Generated File |
-|-----------|--------|----------|----------------|
-| Home | Index | vi | `home_index_vi.css` |
-| Home | Index | en | `home_index_en.css` |
-| Category | News | vi | `category_news_vi.css` |
-| ContentPage | Blog | en | `contentpage_blog_en.css` |
+| Link ID | Language | Generated File | Page Example |
+|---------|----------|----------------|--------------|
+| 123 | vi | `link_123_vi.css` | Trang ch? |
+| 123 | en | `link_123_en.css` | Homepage |
+| 456 | vi | `link_456_vi.css` | Tin t?c |
+| 789 | en | `link_789_en.css` | About Us |

## ?? API Endpoints

### Get CSS Content
```
-GET /AutoCss/GetCss?controller=Home&action=Index&language=vi
+GET /AutoCss/GetCss?linkId=123&language=vi
```

Response:
```css
/* 
 * Auto-generated CSS file
- * Controller: Home
- * Action: Index
+ * Link ID: 123
 * Language: vi
 * Generated: 2025-01-20 10:30:45
 * 
 * Add your custom CSS styles here
 */
```

### Check File Existence
```
-GET /AutoCss/Exists?controller=Home&action=Index&language=vi
+GET /AutoCss/Exists?linkId=123&language=vi
```

Response:
```json
{
  "exists": true,
-  "url": "/auto-css/home_index_vi.css"
+  "url": "/auto-css/link_123_vi.css"
}
```

## ?? Cách ch?nh s?a CSS

1. **T? ??ng t?o file**: Truy c?p page l?n ??u ? File ???c t?o trong `wwwroot/auto-css/`

2. **Ch?nh s?a**: M? file CSS và thêm styles

```css
-/* wwwroot/auto-css/home_index_vi.css */
+/* wwwroot/auto-css/link_123_vi.css */

-/* Custom styles cho Home/Index (Vietnamese) */
+/* Custom styles cho Link 123 - Trang ch? (Vietnamese) */
.home-banner {
    background-color: #0066CC;
    padding: 40px;
}

.home-content h1 {
    color: #093461;
    font-size: 32px;
}
```

3. **Auto reload**: File ???c cache 1 gi?, xóa cache trình duy?t ?? test

## ?? Best Practices

### ? DO

- **Organize styles by page**: M?i page (Link) có file riêng
- **Use semantic class names**: `.link-123`, `.homepage-banner`
- **Comment your code**: Gi?i thích purpose c?a styles
- **Keep files small**: < 50KB per file
- **Test multiple languages**: ??m b?o styles work cho c? vi/en

### ? DON'T

- **Don't put global styles**: Dùng `custom.css` cho global styles
- **Don't duplicate**: N?u style dùng nhi?u page ? move to global
- **Don't hardcode**: Dùng CSS variables cho màu s?c, spacing
- **Don't forget minification**: Production nên minify CSS

## ?? Performance

- **Caching**: Response cached 1 gi?
- **Compression**: Auto compressed v?i Brotli/Gzip
- **Size**: Trung bình 2-5KB per file
- **Load time**: < 50ms (cached), < 200ms (first load)

## ?? Security

- **Input sanitization**: Lo?i b? invalid characters
- **LinkId validation**: Only accept positive integers
- **Content-Type validation**: Ch? serve `text/css`
- **Rate limiting**: Consider adding rate limit cho production

## ?? Troubleshooting

### CSS không load?

-1. Check file exists: `/AutoCss/Exists?controller=X&action=Y&language=vi`
+1. Check file exists: `/AutoCss/Exists?linkId=123&language=vi`
2. Clear browser cache: Ctrl + F5
3. Check console errors
4. Verify service registered: `services.AddScoped<IAutoCssService, AutoCssService>()`

### File không ???c t?o?

1. Check permissions: `wwwroot/auto-css/` ph?i writable
2. Check logs: Xem có exception không
3. Verify `IWebHostEnvironment.WebRootPath` ?úng
4. Restart application
+5. Check linkData has valid ID

### Styles không apply?

1. Check CSS selector specificity
2. Verify file order: Auto CSS load sau `custom.css`
3. Use browser DevTools ?? debug
4. Check if being overridden by other stylesheets

## ?? Migration t? inline styles

### Before (inline styles)
```cshtml
<style>
    .my-class { color: red; }
</style>
```

### After (auto CSS)
```cshtml
-@* Styles ?ã move vào wwwroot/auto-css/home_index_vi.css *@
-@Html.AutoCss()
+@* Styles ?ã move vào wwwroot/auto-css/link_123_vi.css *@
+@Html.AutoCss(linkData?.Id ?? 0)

## ?? TODO / Future Enhancements

- [ ] Auto minification trong production
- [ ] SCSS/LESS support
-- [ ] Cache invalidation strategy
+- [x] ~~Cache invalidation strategy~~ ? Done (file versioning)
- [ ] Admin UI ?? edit CSS files
- [ ] Version control integration
- [ ] Auto backup old versions
- [ ] Diff viewer for changes
- [ ] CSS linting integration

## ?? Contributing

Khi thêm new view:
1. CSS file t? ??ng t?o khi truy c?p l?n ??u
2. Edit file trong `wwwroot/auto-css/`
3. Commit file vào Git
4. Document special styles n?u c?n
+5. Version hash t? ??ng update khi file thay ??i

## ?? References

- ASP.NET Core Static Files: https://docs.microsoft.com/en-us/aspnet/core/fundamentals/static-files
- HTML Helpers: https://docs.microsoft.com/en-us/aspnet/core/mvc/views/html-helpers
- Response Caching: https://docs.microsoft.com/en-us/aspnet/core/performance/caching/response
+- File Versioning: https://docs.microsoft.com/en-us/aspnet/core/mvc/views/tag-helpers/builtin-th/link-tag-helper#asp-append-version
+- IFileVersionProvider: https://docs.microsoft.com/en-us/dotnet/api/microsoft.aspnetcore.mvc.infrastructure.ifileversionprovider
