# SIMPLE STATIC METHODS - QUICK REFERENCE

## ?? **M?c ?ích**
Các static methods ??n gi?n ?? render template HTML + JSON **mà không c?n ContentPage object**.

---

## ?? **1. GetHtmlSection() - Render 1 section**

### **Syntax:**
```csharp
string html = ContentPageExtensions.GetHtmlSection(templateHtml, dataJson);
```

### **Parameters:**
- `templateHtml` (string): Template HTML v?i `[For]...[/For]` và `[fieldName]`
- `dataJson` (string): JSON data
- `wrapInDiv` (bool): Wrap trong `<div>` không? Default: `false`
- `sectionNumber` (int): Section number cho class. Default: `0`

### **Examples:**

#### **Example 1: Basic usage**
```csharp
string input1 = "[For]<div>[title]</div>[/For]";
string input2 = "{\"For\":[{\"title\":\"Hello\"}]}";

string html = ContentPageExtensions.GetHtmlSection(input1, input2);
// Output: <div>Hello</div>
```

#### **Example 2: With wrapper**
```csharp
string html = ContentPageExtensions.GetHtmlSection(input1, input2, wrapInDiv: true);
// Output: 
// <div class="section-content">
// <div>Hello</div>
// </div>
```

#### **Example 3: With section number**
```csharp
string html = ContentPageExtensions.GetHtmlSection(input1, input2, wrapInDiv: true, sectionNumber: 1);
// Output:
// <div class="section section-1" data-section="1">
// <div>Hello</div>
// </div>
```

---

## ?? **2. GetHtmlSections() - Render nhi?u sections**

### **Syntax:**
```csharp
string html = ContentPageExtensions.GetHtmlSections(
    (input1, input2),
    (input3, input4),
    (input5, input6)
);
```

### **Parameters:**
- Pairs of `(templateHtml, dataJson)` - Không gi?i h?n s? l??ng

### **Example:**

```csharp
// Section 1: Hero Slider
string input1 = "[For]<div class='slide'>[title]</div>[/For]";
string input2 = "{\"For\":[{\"title\":\"Slide 1\"},{\"title\":\"Slide 2\"}]}";

// Section 2: Reasons
string input3 = "[For]<div class='reason'>[text]</div>[/For]";
string input4 = "{\"For\":[{\"text\":\"Reason 1\"},{\"text\":\"Reason 2\"}]}";

// Section 3: FAQ
string input5 = "[For]<div class='faq'>[question]</div>[/For]";
string input6 = "{\"For\":[{\"question\":\"Q1?\"},{\"question\":\"Q2?\"}]}";

// Render all sections
string html = ContentPageExtensions.GetHtmlSections(
    (input1, input2),
    (input3, input4),
    (input5, input6)
);
```

**Output:**
```html
<div class="section section-1" data-section="1">
<div class='slide'>Slide 1</div>
<div class='slide'>Slide 2</div>
</div>

<div class="section section-2" data-section="2">
<div class='reason'>Reason 1</div>
<div class='reason'>Reason 2</div>
</div>

<div class="section section-3" data-section="3">
<div class='faq'>Q1?</div>
<div class='faq'>Q2?</div>
</div>
```

---

## ?? **3. GetHtmlSectionsWithWrapper() - Render v?i custom wrapper**

### **Syntax:**
```csharp
string html = ContentPageExtensions.GetHtmlSectionsWithWrapper(
    wrapperFormat: "<section>{0}</section>",
    (input1, input2),
    (input3, input4)
);
```

### **Parameters:**
- `wrapperFormat` (string): Format string v?i `{0}` cho content
- Pairs of `(templateHtml, dataJson)`

### **Examples:**

#### **Example 1: HTML5 semantic tags**
```csharp
string html = ContentPageExtensions.GetHtmlSectionsWithWrapper(
    "<section class='event-section'>{0}</section>",
    (input1, input2),
    (input3, input4)
);
```

**Output:**
```html
<section class='event-section'>
<div class='slide'>Slide 1</div>
</section>

<section class='event-section'>
<div class='reason'>Reason 1</div>
</section>
```

#### **Example 2: Bootstrap grid**
```csharp
string html = ContentPageExtensions.GetHtmlSectionsWithWrapper(
    "<div class='col-md-6'>{0}</div>",
    (input1, input2),
    (input3, input4)
);
```

**Output:**
```html
<div class='col-md-6'>
<div class='slide'>Slide 1</div>
</div>

<div class='col-md-6'>
<div class='reason'>Reason 1</div>
</div>
```

---

## ?? **Use Cases trong Controller**

### **Use Case 1: Render sections trong Controller**

```csharp
using PT.Domain.Extensions;

public class EventController : Controller
{
    public IActionResult Details(int id)
    {
        // Get data from database
        var eventData = _repo.GetById(id);
        
        // Render sections
        string html = ContentPageExtensions.GetHtmlSections(
            (eventData.Input1, eventData.Input2),
            (eventData.Input3, eventData.Input4),
            (eventData.Input5, eventData.Input6)
        );
        
        ViewBag.SectionsHtml = html;
        return View();
    }
}
```

**View:**
```razor
@Html.Raw(ViewBag.SectionsHtml)
```

---

### **Use Case 2: Render single section trong API**

```csharp
[HttpGet("api/section/render")]
public IActionResult RenderSection([FromBody] SectionRequest request)
{
    string html = ContentPageExtensions.GetHtmlSection(
        request.TemplateHtml,
        request.DataJson
    );
    
    return Ok(new { html });
}

public class SectionRequest
{
    public string TemplateHtml { get; set; }
    public string DataJson { get; set; }
}
```

**API Usage:**
```javascript
fetch('/api/section/render', {
    method: 'POST',
    headers: { 'Content-Type': 'application/json' },
    body: JSON.stringify({
        templateHtml: '[For]<div>[title]</div>[/For]',
        dataJson: '{"For":[{"title":"Test"}]}'
    })
})
.then(res => res.json())
.then(data => console.log(data.html));
```

---

### **Use Case 3: Preview template trong admin**

```csharp
[HttpPost("preview")]
public IActionResult PreviewTemplate(string templateHtml, string dataJson)
{
    string html = ContentPageExtensions.GetHtmlSection(
        templateHtml,
        dataJson,
        wrapInDiv: true,
        sectionNumber: 1
    );
    
    return Content(html, "text/html");
}
```

**View:**
```razor
<form method="post" action="/preview" target="_blank">
    <textarea name="templateHtml" rows="10">@Model.Input1</textarea>
    <textarea name="dataJson" rows="10">@Model.Input2</textarea>
    <button type="submit">Preview</button>
</form>
```

---

### **Use Case 4: Generate email template**

```csharp
public string GenerateEmailHtml(string recipientName, List<string> reasons)
{
    string template = "[For]<li>[reason]</li>[/For]";
    string data = $"{{\"For\":[{string.Join(",", reasons.Select(r => $"{{\"reason\":\"{r}\"}}"))}]}}";
    
    string reasonsHtml = ContentPageExtensions.GetHtmlSection(template, data);
    
    return $@"
        <html>
            <body>
                <h1>Hello {recipientName}</h1>
                <p>Here are the reasons:</p>
                <ul>{reasonsHtml}</ul>
            </body>
        </html>
    ";
}
```

---

### **Use Case 5: Dynamic page builder**

```csharp
public IActionResult BuildPage(List<PageSection> sections)
{
    var pairs = sections
        .Select(s => (s.TemplateHtml, s.DataJson))
        .ToArray();
    
    string html = ContentPageExtensions.GetHtmlSections(pairs);
    
    return Content(html, "text/html");
}

public class PageSection
{
    public string TemplateHtml { get; set; }
    public string DataJson { get; set; }
}
```

---

## ?? **Performance Comparison**

### **Method 1: ContentPage Extension (requires object)**
```csharp
var contentPage = await _repo.GetById(id);
string html = contentPage.RenderAllSections();
// Pros: Type-safe, auto-maps Input1-10
// Cons: Requires ContentPage object
```

### **Method 2: Static GetHtmlSections (no object required)**
```csharp
string html = ContentPageExtensions.GetHtmlSections(
    (input1, input2),
    (input3, input4)
);
// Pros: Simple, no object needed, flexible
// Cons: Manual passing parameters
```

---

## ? **When to use which method?**

### **Use `GetHtmlSection()` when:**
? You have raw template HTML + JSON strings  
? Building API endpoints  
? Generating emails  
? Preview functionality  
? Testing templates  

### **Use `contentPage.RenderAllSections()` when:**
? You have a ContentPage entity  
? Rendering full event pages  
? Standard CMS workflow  
? Need section ordering logic  

---

## ?? **Quick Examples**

### **1. Simplest possible usage:**
```csharp
string html = ContentPageExtensions.GetHtmlSection(
    "[For]<div>[name]</div>[/For]",
    "{\"For\":[{\"name\":\"John\"}]}"
);
// Output: <div>John</div>
```

### **2. Multiple sections:**
```csharp
string html = ContentPageExtensions.GetHtmlSections(
    ("[For]<h1>[title]</h1>[/For]", "{\"For\":[{\"title\":\"Hero\"}]}"),
    ("[For]<p>[text]</p>[/For]", "{\"For\":[{\"text\":\"Content\"}]}")
);
```

### **3. Custom wrapper:**
```csharp
string html = ContentPageExtensions.GetHtmlSectionsWithWrapper(
    "<article>{0}</article>",
    (template1, data1),
    (template2, data2)
);
```

---

## ?? **Summary**

| Method | Use When | Output |
|--------|----------|--------|
| `GetHtmlSection()` | Single section, no wrapper | Raw HTML |
| `GetHtmlSection(wrap: true)` | Single section with wrapper | `<div class="section section-1">...</div>` |
| `GetHtmlSections()` | Multiple sections | Multiple `<div class="section section-X">...</div>` |
| `GetHtmlSectionsWithWrapper()` | Custom wrapper | Custom wrapper for each section |

**?? Now you can call it simply like this:**
```csharp
var html = ContentPageExtensions.GetHtmlSection(input1, input2);
```

Happy coding! ??
