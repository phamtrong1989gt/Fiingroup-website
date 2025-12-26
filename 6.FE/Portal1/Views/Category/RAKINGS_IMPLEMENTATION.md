# Rakings Implementation - Traditional MVC Pattern

## Overview
Rakings page s? d?ng pattern truy?n th?ng c?a MVC v?i ViewData và HTML select thu?n, không dùng AJAX ?? load dropdown.

## Architecture

### 1. Controller (`CategoryController.cs`)
```csharp
public async Task<IActionResult> Rakings(string linkData, int portalId, string language)
{
    // Load dropdown data from API
    var industries = await _iNewsAPIService.GetReportIndustriesAsync(language);
    ViewData["Industries"] = industries?.Data?.Select(...).ToList();
    
    var scores = await _iNewsAPIService.GetReportScoresAsync();
    ViewData["Scores"] = scores?.Data?.Select(...).ToList();
    
    var outlooks = await _iNewsAPIService.GetReportOutlooksAsync(language);
    ViewData["Outlooks"] = outlooks?.Data?.Select(...).ToList();
    
    return View("Rakings");
}
```

### 2. Main View (`Rakings.cshtml`)
- Nh?n SelectListItem t? ViewData
- Render HTML `<select>` thu?n
- AJAX ch? dùng cho load table data
- JavaScript ??n gi?n h?n (không c?n load dropdown)

### 3. AJAX Partial View (`RakingsAjax.cshtml`)
- Layout = null
- Render table + pagination
- Model: `RatingResultsResponse`

### 4. Services (`INewsAPIService.cs`)
- `GetReportScoresAsync()` - Cache 1h
- `GetReportIndustriesAsync(lang)` - Cache 1h
- `GetReportOutlooksAsync(lang)` - Cache 1h

## Flow Diagram

```
Page Load
    ?
CategoryController.Rakings()
    ?
Load Dropdown Data from API (with 1h cache)
    ??? GetReportScoresAsync()
    ??? GetReportIndustriesAsync(language)
    ??? GetReportOutlooksAsync(language)
    ?
ViewData["Industries/Scores/Outlooks"]
    ?
Rakings.cshtml (render <select> elements)
    ?
JavaScript: getRakingsAjax(1) - load initial data
    ?
CategoryController.RakingsAjax()
    ?
RakingsAjax.cshtml (render table)
```

## Benefits

### ? Traditional MVC Pattern
- D? hi?u, d? maintain
- Không ph?c t?p v?i AJAX dropdowns
- Team quen thu?c v?i pattern này

### ? Performance
- Dropdown data ???c cache 1h
- Không c?n g?i API m?i l?n load page
- HTML select native (fast rendering)

### ? SEO Friendly
- Full server-side rendering
- No client-side API calls for dropdowns
- Better for search engines

### ? Mobile Friendly
- Native select dropdown
- Better UX on mobile devices
- Auto-hide filters on mobile with toggle button

## Code Example

### View (Rakings.cshtml)
```razor
@{
    var industries = ViewData["Industries"] as List<SelectListItem>;
    var scores = ViewData["Scores"] as List<SelectListItem>;
    var outlooks = ViewData["Outlooks"] as List<SelectListItem>;
}

<select id="industryTypeId" name="industryTypeId" class="find-input">
    <option value="">T?t c? ngành</option>
    @foreach (var item in industries)
    {
        <option value="@item.Value">@item.Text</option>
    }
</select>

<select id="scoreId" name="scoreId" class="find-input">
    <option value="">T?t c? ?i?m</option>
    @foreach (var item in scores)
    {
        <option value="@item.Value">@item.Text</option>
    }
</select>

<select id="prospectsId" name="prospectsId" class="find-input">
    <option value="">T?t c? tri?n v?ng</option>
    @foreach (var item in outlooks)
    {
        <option value="@item.Value">@item.Text</option>
    }
</select>
```

### JavaScript (Simplified)
```javascript
function buildParams(page) {
    return {
        companyName: $('#companyName').val() || '',
        industryTypeId: $('#industryTypeId').val() || '',  // Native select
        scoreId: $('#scoreId').val() || '',                 // Native select
        prospectsId: $('#prospectsId').val() || '',         // Native select
        page: page || 1,
        pageSize: 10,
        lang: currentLanguage
    };
}

// Clear filters - simple!
$('.refresh').on('click', function() {
    $('#companyName').val('');
    $('#industryTypeId').val('');
    $('#scoreId').val('');
    $('#prospectsId').val('');
    getRakingsAjax(1);
});
```

## Files Structure

```
6.FE\Portal1\
??? Controllers\
?   ??? CategoryController.cs
?       ??? Rakings() - Load ViewData + return View
?       ??? RakingsAjax() - Return partial view
??? Views\
?   ??? Category\
?       ??? Rakings.cshtml - Main view with <select>
?       ??? RakingsAjax.cshtml - Partial view (table)
??? Services\
    ??? INewsAPIService.cs
        ??? GetReportScoresAsync() (cache 1h)
        ??? GetReportIndustriesAsync(lang) (cache 1h)
        ??? GetReportOutlooksAsync(lang) (cache 1h)
```

## API Caching Strategy

| API | Cache Key | Cache Duration | Language Specific |
|-----|-----------|----------------|-------------------|
| ReportScores | `ReportAPI_Scores` | 1 hour | No |
| ReportIndustries | `ReportAPI_Industries_{lang}` | 1 hour | Yes |
| ReportOutlooks | `ReportAPI_Outlooks_{lang}` | 1 hour | Yes |

## Mobile Responsive

```javascript
// Auto-hide filters on mobile, show on desktop
function initMobileFilter() {
    $('.btn-filter').on('click', function() {
        filterOpen = !filterOpen;
        if (window.innerWidth <= 768) {
            if (filterOpen) {
                $('.custom-dropdown-select').show();
                $('.refresh').show();
            } else {
                $('.custom-dropdown-select').hide();
                $('.refresh').hide();
            }
        }
    });
    
    // Initial state for mobile
    if (window.innerWidth <= 768) {
        $('.custom-dropdown-select').hide();
        $('.refresh').hide();
    }
}
```

## Comparison: Custom Dropdowns vs Native Select

| Feature | Custom Dropdowns | Native Select ? |
|---------|------------------|------------------|
| Load Method | Client-side AJAX | Server-side ViewData |
| Render Speed | Slower (API call) | Faster (already in HTML) |
| Caching | Client-side only | Server-side (1h) |
| Mobile UX | Custom UI | Native mobile picker |
| SEO | Client-rendered | Server-rendered ? |
| Code Complexity | High | Low ? |
| Accessibility | Need custom logic | Built-in ? |
| Browser Support | All modern | All browsers ? |

## Next Steps: Connect Real API

### 1. Configure endpoints in `appsettings.json`
```json
{
  "BaseSettings": {
    "NewAPI": {
      "ReportScoresEndpoint": "http://113.160.94.133:5050/FGRA/apiReport/scores",
      "ReportIndustriesEndpoint": "http://113.160.94.133:5050/FGRA/apiReport/industries",
      "ReportOutlooksEndpoint": "http://113.160.94.133:5050/FGRA/apiReport/outlooks",
      "RatingResultsEndpoint": "http://113.160.94.133:5050/FGRA/apiReport/rating-results"
    }
  }
}
```

### 2. Add RatingResults API method to `INewsAPIService.cs`
```csharp
Task<RatingResultsResponse> GetRatingResultsAsync(RatingResultsQueryParameters parameters);
```

### 3. Implement in `NewsAPIService.cs`
```csharp
public async Task<RatingResultsResponse> GetRatingResultsAsync(RatingResultsQueryParameters parameters)
{
    var endpoint = _baseSettings.Value.NewAPI.RatingResultsEndpoint;
    var queryString = parameters.ToQueryString();
    var url = $"{endpoint}?{queryString}";
    
    var token = await GetAccessTokenAsync();
    return await CallRatingAPIAsync<RatingResultsResponse>(url, token);
}
```

### 4. Update `CategoryController.RakingsAjax`
```csharp
public async Task<ActionResult> RakingsAjax([FromQuery] RatingResultsQueryParameters prs)
{
    prs.PageSize = prs.PageSize ?? 10;
    prs.Page = prs.Page ?? 1;
    prs.Lang = prs.Lang ?? "vi";

    var ratingResults = await _iNewsAPIService.GetRatingResultsAsync(prs);
    return View("RakingsAjax", ratingResults);
}
```

## Testing

### 1. Page Load
- ? Verify dropdowns populated from ViewData
- ? Check cache (should not call API on refresh within 1h)
- ? Test language switching

### 2. Search & Filter
- ? Select industry ? Search ? Verify AJAX call
- ? Select score ? Search ? Verify parameters
- ? Select outlook ? Search ? Verify results
- ? Clear filters ? Verify all reset

### 3. Mobile
- ? Resize to mobile width
- ? Verify filter toggle button appears
- ? Click toggle ? filters show/hide
- ? Native select picker works

### 4. Performance
- ? First load: 3 API calls (industries, scores, outlooks)
- ? Subsequent loads: 0 API calls (cached)
- ? Cache expires after 1 hour

## Current Status

- ? Build successful
- ? Traditional MVC pattern
- ? Native HTML selects
- ? ViewData for dropdown data
- ? Server-side caching (1h)
- ? Mobile responsive
- ? Ready for real API integration

## Key Differences from Previous Approach

| Aspect | Previous (AJAX) | Current (MVC) ? |
|--------|----------------|------------------|
| Dropdown Load | Client AJAX call | Server ViewData |
| Data Source | `/api/rating/*` | `ViewData["*"]` |
| HTML Element | Custom `<div>` dropdown | Native `<select>` |
| JavaScript | Complex dropdown logic | Simple `.val()` |
| Mobile UX | Custom implementation | Native picker |
| SEO | Client-rendered | Server-rendered ? |
| Performance | 3 API calls on load | Cached in ViewData ? |
| Code Lines | ~200 lines JS | ~50 lines JS ? |

This approach is simpler, more maintainable, and follows standard MVC patterns! ??
