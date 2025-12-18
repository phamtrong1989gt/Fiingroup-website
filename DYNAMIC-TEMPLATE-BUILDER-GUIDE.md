# ?? DYNAMIC TEMPLATE BUILDER - H??NG D?N TOÀN DI?N

## ?? T?NG QUAN

H? th?ng **Dynamic Template Builder** cho phép:
- ? Qu?n lý nhi?u sections v?i templates HTML ??ng
- ? Load/edit templates và JSON values d? dàng
- ? Thêm field/item vào JSON tr?c ti?p trong UI
- ? S? d?ng l?i cho nhi?u màn hình khác nhau
- ? Không ph? thu?c external engine (Standalone)

---

## ?? C?U TRÚC FILES

```
1.Backend/
??? Areas/Manager/
?   ??? Controllers/
?   ?   ??? BlogManagerController.cs
?   ??? Views/
?       ??? BlogManager/
?       ?   ??? Index.cshtml
?       ?   ??? EventEdit.cshtml  ? EXAMPLE
?       ?   ??? Create.cshtml
?       ??? Shared/
?           ??? _SectionTemplateBuilder.cshtml  ? CORE COMPONENT
??? wwwroot/js/
?   ??? templates/
?   ?   ??? section-templates-1.js  (Slider, Carousel, Hero)
?   ?   ??? section-templates-2.js  (Cards, Grid, Pricing)
?   ?   ??? section-templates-3.js  (List, Timeline, Testimonials)
?   ?   ??? section-templates-4.js  (Team, Portfolio, Gallery)
?   ?   ??? section-templates-5.js  (FAQ, Contact)
?   ??? section-template-engine.js  (Optional)
?   ??? HOW-TO-ADD-SECTION.js
??? 2.Domain/Model/
    ??? BlogModel.cs
    ??? SectionTemplateBuilderModel.cs  ? MODEL
```

---

## ?? 1. MODEL CLASS

**File:** `2.Domain/Model/SectionTemplateBuilderModel.cs`

```csharp
namespace PT.Domain.Model
{
    /// <summary>
    /// Model cho Dynamic Template Builder Component
    /// S? d?ng cho Partial View _SectionTemplateBuilder.cshtml
    /// </summary>
    public class SectionTemplateBuilderModel
    {
        // ===============================================
        // SECTION CONFIGURATION
        // ===============================================
        
        /// <summary>
        /// S? th? t? c?a section (1, 2, 3, ...)
        /// Dùng ?? generate unique IDs cho các elements
        /// </summary>
        public int SectionNumber { get; set; }
        
        /// <summary>
        /// Tiêu ?? hi?n th? c?a section
        /// VD: "Section I: Hero Slider"
        /// </summary>
        public string Title { get; set; }
        
        /// <summary>
        /// Material Icon name
        /// VD: "slideshow", "view_module", "timeline"
        /// </summary>
        public string Icon { get; set; } = "view_carousel";
        
        /// <summary>
        /// Section có m? r?ng m?c ??nh hay không
        /// true = m?, false = ?óng
        /// </summary>
        public bool IsExpanded { get; set; } = false;
        
        // ===============================================
        // INPUT BINDING (asp-for)
        // ===============================================
        
        /// <summary>
        /// Tên property binding cho Template HTML
        /// VD: nameof(Model.Input1) ? "Input1"
        /// </summary>
        public string InputTemplate { get; set; }
        
        /// <summary>
        /// Tên property binding cho Values JSON
        /// VD: nameof(Model.Input2) ? "Input2"
        /// </summary>
        public string InputValue { get; set; }
        
        // ===============================================
        // LABELS
        // ===============================================
        
        /// <summary>
        /// Label cho Template HTML textarea
        /// </summary>
        public string TemplateLabel { get; set; } = "Template HTML";
        
        /// <summary>
        /// Label cho Values JSON textarea
        /// </summary>
        public string ValueLabel { get; set; } = "Values (JSON)";
        
        // ===============================================
        // CURRENT VALUES (for Edit Mode)
        // ===============================================
        
        /// <summary>
        /// Giá tr? hi?n t?i c?a Template HTML
        /// L?y t? Model.Input1, Model.Input3, ...
        /// </summary>
        public string TemplateValue { get; set; }
        
        /// <summary>
        /// Giá tr? hi?n t?i c?a Values JSON
        /// L?y t? Model.Input2, Model.Input4, ...
        /// </summary>
        public string ValueJson { get; set; }
        
        // ===============================================
        // TEMPLATE SOURCE
        // ===============================================
        
        /// <summary>
        /// Tên bi?n JavaScript global ch?a templates
        /// VD: "SectionTemplates1", "SectionTemplates2"
        /// T??ng ?ng v?i file: section-templates-1.js
        /// </summary>
        public string TemplateSource { get; set; } = "SectionTemplates1";
        
        // ===============================================
        // BUTTON NAMES (Optional - Auto-generate if empty)
        // ===============================================
        
        /// <summary>
        /// Custom ID cho button "Thêm field"
        /// N?u null ? auto: "btnAddField{SectionNumber}"
        /// </summary>
        public string ButtonAddField { get; set; }
        
        /// <summary>
        /// Custom ID cho button "Thêm item"
        /// N?u null ? auto: "btnAddItem{SectionNumber}"
        /// </summary>
        public string ButtonAddItem { get; set; }
        
        // ===============================================
        // AUTO-GENERATED IDs (ReadOnly Properties)
        // ===============================================
        
        /// <summary>
        /// ID c?a panel heading
        /// Format: "headingSection_{SectionNumber}"
        /// </summary>
        public string SectionId => $"headingSection_{SectionNumber}";
        
        /// <summary>
        /// ID c?a collapsible panel body
        /// Format: "collapseSection_{SectionNumber}"
        /// </summary>
        public string CollapseId => $"collapseSection_{SectionNumber}";
        
        /// <summary>
        /// ID c?a Template HTML textarea
        /// Format: "templateInput{SectionNumber}"
        /// </summary>
        public string TemplateInputId => $"templateInput{SectionNumber}";
        
        /// <summary>
        /// ID c?a Values JSON textarea
        /// Format: "valuesInput{SectionNumber}"
        /// </summary>
        public string ValueInputId => $"valuesInput{SectionNumber}";
        
        /// <summary>
        /// ID c?a button "Thêm field"
        /// Custom ho?c auto: "btnAddField{SectionNumber}"
        /// </summary>
        public string AddFieldBtnId => 
            !string.IsNullOrEmpty(ButtonAddField) 
                ? ButtonAddField 
                : $"btnAddField{SectionNumber}";
        
        /// <summary>
        /// ID c?a button "Thêm item"
        /// Custom ho?c auto: "btnAddItem{SectionNumber}"
        /// </summary>
        public string AddItemBtnId => 
            !string.IsNullOrEmpty(ButtonAddItem) 
                ? ButtonAddItem 
                : $"btnAddItem{SectionNumber}";
        
        /// <summary>
        /// ID c?a input nh?p tên field
        /// Format: "quickFieldName{SectionNumber}"
        /// </summary>
        public string FieldNameInputId => $"quickFieldName{SectionNumber}";
    }
}
```

---

## ?? 2. DATA MODEL (BlogModel.cs)

**File:** `2.Domain/Model/BlogModel.cs`

```csharp
public class BlogModel
{
    // ... existing properties ...
    
    // ===============================================
    // DYNAMIC TEMPLATE BUILDER INPUTS
    // ===============================================
    
    // Section 1: Hero Slider (SectionTemplates1)
    [Display(Name = "Section 1 - Template HTML")]
    public string Input1 { get; set; }
    
    [Display(Name = "Section 1 - Values JSON")]
    public string Input2 { get; set; }
    
    // Section 2: Product Cards (SectionTemplates2)
    [Display(Name = "Section 2 - Template HTML")]
    public string Input3 { get; set; }
    
    [Display(Name = "Section 2 - Values JSON")]
    public string Input4 { get; set; }
    
    // Section 3: Timeline (SectionTemplates3)
    [Display(Name = "Section 3 - Template HTML")]
    public string Input5 { get; set; }
    
    [Display(Name = "Section 3 - Values JSON")]
    public string Input6 { get; set; }
    
    // Section 4: Gallery (SectionTemplates4)
    [Display(Name = "Section 4 - Template HTML")]
    public string Input7 { get; set; }
    
    [Display(Name = "Section 4 - Values JSON")]
    public string Input8 { get; set; }
    
    // Section 5: FAQ (SectionTemplates5)
    [Display(Name = "Section 5 - Template HTML")]
    public string Input9 { get; set; }
    
    [Display(Name = "Section 5 - Values JSON")]
    public string Input10 { get; set; }
    
    // Có th? thêm ??n Input15 n?u c?n
}
```

---

## ?? 3. VIEW USAGE (EventEdit.cshtml)

**File:** `1.Backend/Areas/Manager/Views/BlogManager/EventEdit.cshtml`

```razor
@model PT.Domain.Model.BlogModel

<form form-edit asp-action="Edit">
    @Html.AntiForgeryToken()
    
    @* Thông tin chung *@
    <div class="panel panel-default">
        <div class="panel-body">
            <input type="text" asp-for="Name" />
            <textarea asp-for="Summary"></textarea>
        </div>
    </div>
    
    @* =========================================
       SECTION 1 - Hero Slider
       ========================================= *@
    <partial name="_SectionTemplateBuilder" model="@(new PT.Domain.Model.SectionTemplateBuilderModel 
    { 
        SectionNumber = 1,
        Title = "Section I: Hero Slider",
        Icon = "slideshow",
        IsExpanded = true,
        InputTemplate = nameof(Model.Input1),
        InputValue = nameof(Model.Input2),
        TemplateLabel = "Input 1 - Template HTML",
        ValueLabel = "Input 2 - Values (JSON)",
        TemplateValue = Model.Input1,
        ValueJson = Model.Input2,
        TemplateSource = "SectionTemplates1",
        ButtonAddField = "btnAddField1",
        ButtonAddItem = "btnAddItem1"
    })" />
    
    @* =========================================
       SECTION 2 - Product Cards
       ========================================= *@
    <partial name="_SectionTemplateBuilder" model="@(new PT.Domain.Model.SectionTemplateBuilderModel 
    { 
        SectionNumber = 2,
        Title = "Section II: Product Cards",
        Icon = "view_module",
        IsExpanded = false,
        InputTemplate = nameof(Model.Input3),
        InputValue = nameof(Model.Input4),
        TemplateLabel = "Input 3 - Template HTML",
        ValueLabel = "Input 4 - Values (JSON)",
        TemplateValue = Model.Input3,
        ValueJson = Model.Input4,
        TemplateSource = "SectionTemplates2",
        ButtonAddField = "btnAddField2",
        ButtonAddItem = "btnAddItem2"
    })" />
    
    @* =========================================
       SECTION 3 - Timeline (Optional)
       ========================================= *@
    <partial name="_SectionTemplateBuilder" model="@(new PT.Domain.Model.SectionTemplateBuilderModel 
    { 
        SectionNumber = 3,
        Title = "Section III: Timeline",
        Icon = "timeline",
        IsExpanded = false,
        InputTemplate = nameof(Model.Input5),
        InputValue = nameof(Model.Input6),
        TemplateValue = Model.Input5,
        ValueJson = Model.Input6,
        TemplateSource = "SectionTemplates3"
    })" />
    
    <div class="modal-footer">
        <button type="submit">L?u thay ??i</button>
    </div>
</form>

@* ===== LOAD TEMPLATE FILES ? CU?I ===== *@
<script src="~/js/templates/section-templates-1.js"></script>
<script src="~/js/templates/section-templates-2.js"></script>
<script src="~/js/templates/section-templates-3.js"></script>
<script src="~/js/templates/section-templates-4.js"></script>
<script src="~/js/templates/section-templates-5.js"></script>

<script>
    // Optional: Initialize CodeMirror for syntax highlighting
    setTimeout(function () {
        pageEditors = createEditors(["Input1", "Input2", "Input3", "Input4"]);
    }, 200);
</script>
```

---

## ?? 4. TEMPLATE FILE FORMAT

**File:** `1.Backend/wwwroot/js/templates/section-templates-1.js`

```javascript
// ============================================
// SECTION TEMPLATES 1 - Slider & Carousel
// ============================================
(function() {
    'use strict';

    // Global variable - ph?i match v?i TemplateSource trong Model
    window.SectionTemplates1 = {
        
        // Template Key (ph?i unique trong file này)
        'slider': {
            // Tên hi?n th? trong UI
            name: 'Basic Slider',
            
            // HTML Template v?i placeholders
            // [For]...[/For] = vòng l?p
            // [field1], [field2] = placeholders
            template: `[For]
<div class="swiper-slide">
    <img src="[field1]" alt="[field2]" class="img-fluid" />
    <div class="slide-caption">
        <h3>[field2]</h3>
        <p>[field3]</p>
        <a href="[field4]" class="btn btn-primary">[field5]</a>
    </div>
</div>
[/For]`,
            
            // Sample data (JSON array of objects)
            values: [
                {
                    field1: "https://via.placeholder.com/1200x500/3f51b5/ffffff?text=Slide+1",
                    field2: "Welcome to Our Website",
                    field3: "Discover amazing products and services",
                    field4: "/products",
                    field5: "Shop Now"
                },
                {
                    field1: "https://via.placeholder.com/1200x500/f44336/ffffff?text=Slide+2",
                    field2: "Summer Sale 2024",
                    field3: "Up to 50% off on selected items",
                    field4: "/sale",
                    field5: "View Deals"
                }
            ]
        },
        
        'carousel': {
            name: 'Image Carousel',
            template: `[For]
<div class="carousel-item">
    <img src="[imageUrl]" alt="[title]" />
    <h4>[title]</h4>
    <p>[description]</p>
</div>
[/For]`,
            values: [
                {
                    imageUrl: "https://via.placeholder.com/800x400",
                    title: "Item 1",
                    description: "Description for item 1"
                }
            ]
        },
        
        'hero': {
            name: 'Hero Banner',
            template: `[For]
<section class="hero-banner" style="background-image: url('[backgroundImage]')">
    <div class="container">
        <h1>[heading]</h1>
        <p>[subheading]</p>
        <a href="[ctaUrl]" class="btn btn-lg">[ctaText]</a>
    </div>
</section>
[/For]`,
            values: [
                {
                    backgroundImage: "https://via.placeholder.com/1920x600",
                    heading: "Your Hero Heading",
                    subheading: "Powerful subheading text goes here",
                    ctaUrl: "/contact",
                    ctaText: "Get Started"
                }
            ]
        }
    };

})();
```

**File:** `1.Backend/wwwroot/js/templates/section-templates-2.js`

```javascript
// ============================================
// SECTION TEMPLATES 2 - Cards & Grid
// ============================================
(function() {
    'use strict';

    window.SectionTemplates2 = {
        'cards': {
            name: 'Product Cards',
            template: `[For]
<div class="col-md-4">
    <div class="card">
        <img src="[image]" class="card-img-top" alt="[title]" />
        <div class="card-body">
            <h5 class="card-title">[title]</h5>
            <p class="card-text">[description]</p>
            <p class="price">[price]</p>
            <a href="[url]" class="btn btn-primary">View Details</a>
        </div>
    </div>
</div>
[/For]`,
            values: [
                {
                    image: "https://via.placeholder.com/400x300",
                    title: "Product 1",
                    description: "Short description of product 1",
                    price: "$99.99",
                    url: "/product/1"
                },
                {
                    image: "https://via.placeholder.com/400x300",
                    title: "Product 2",
                    description: "Short description of product 2",
                    price: "$149.99",
                    url: "/product/2"
                }
            ]
        },
        
        'grid': {
            name: 'Image Grid',
            template: `[For]
<div class="grid-item">
    <img src="[imageUrl]" alt="[caption]" />
    <div class="overlay">
        <h4>[caption]</h4>
    </div>
</div>
[/For]`,
            values: [
                {
                    imageUrl: "https://via.placeholder.com/400x400",
                    caption: "Gallery Image 1"
                }
            ]
        },
        
        'pricing': {
            name: 'Pricing Table',
            template: `[For]
<div class="pricing-card">
    <h3>[planName]</h3>
    <div class="price">[price]<span>/month</span></div>
    <ul class="features">
        <li>[feature1]</li>
        <li>[feature2]</li>
        <li>[feature3]</li>
    </ul>
    <a href="[signupUrl]" class="btn">[ctaText]</a>
</div>
[/For]`,
            values: [
                {
                    planName: "Basic",
                    price: "$9",
                    feature1: "Feature 1",
                    feature2: "Feature 2",
                    feature3: "Feature 3",
                    signupUrl: "/signup/basic",
                    ctaText: "Get Started"
                }
            ]
        }
    };

})();
```

---

## ?? CÁCH S? D?NG

### **Scenario 1: Thêm Section m?i vào màn hình hi?n có**

#### B??c 1: Thêm properties vào Model

```csharp
// File: 2.Domain/Model/BlogModel.cs
public class BlogModel 
{
    // ... existing properties ...
    
    // Section 4 m?i
    public string Input7 { get; set; }  // Template HTML
    public string Input8 { get; set; }  // Values JSON
}
```

#### B??c 2: Thêm Partial View vào View

```razor
@* File: EventEdit.cshtml *@
<partial name="_SectionTemplateBuilder" model="@(new SectionTemplateBuilderModel 
{ 
    SectionNumber = 4,
    Title = "Section IV: Gallery",
    Icon = "collections",
    IsExpanded = false,
    InputTemplate = nameof(Model.Input7),
    InputValue = nameof(Model.Input8),
    TemplateValue = Model.Input7,
    ValueJson = Model.Input8,
    TemplateSource = "SectionTemplates4",
    ButtonAddField = "btnAddField4",
    ButtonAddItem = "btnAddItem4"
})" />
```

#### B??c 3: Load template script

```html
<script src="~/js/templates/section-templates-4.js"></script>
```

---

### **Scenario 2: T?o Template file m?i**

#### File: `section-templates-6.js`

```javascript
(function() {
    'use strict';
    
    window.SectionTemplates6 = {
        'video': {
            name: 'Video Embed',
            template: `[For]
<div class="video-wrapper">
    <iframe src="[videoUrl]" frameborder="0" allowfullscreen></iframe>
    <h4>[title]</h4>
    <p>[description]</p>
</div>
[/For]`,
            values: [
                {
                    videoUrl: "https://www.youtube.com/embed/dQw4w9WgXcQ",
                    title: "Sample Video",
                    description: "This is a sample video description"
                }
            ]
        },
        
        'custom': {
            name: 'Custom Template',
            template: `[For]
<div class="custom-section">
    <h3>[heading]</h3>
    <div class="content">[content]</div>
</div>
[/For]`,
            values: [
                {
                    heading: "Custom Heading",
                    content: "Your custom content here"
                }
            ]
        }
    };
})();
```

---

### **Scenario 3: Áp d?ng cho màn hình khác**

#### Controller

```csharp
// File: PageFlowManagerController.cs
public class PageFlowManagerController : Controller
{
    [HttpGet]
    public async Task<IActionResult> Edit(int id)
    {
        var model = await _repository.GetByIdAsync(id);
        return View(model);
    }
    
    [HttpPost]
    public async Task<IActionResult> Edit(PageFlowModel model)
    {
        if (ModelState.IsValid)
        {
            await _repository.UpdateAsync(model);
            return Json(new { type = "success", message = "C?p nh?t thành công!" });
        }
        return Json(new { type = "error", message = "Có l?i x?y ra!" });
    }
}
```

#### Model

```csharp
// File: PageFlowModel.cs
public class PageFlowModel
{
    public int Id { get; set; }
    public string Name { get; set; }
    
    // Dynamic Template Builder Inputs
    public string Input1 { get; set; }  // Section 1 Template
    public string Input2 { get; set; }  // Section 1 Values
    public string Input3 { get; set; }  // Section 2 Template
    public string Input4 { get; set; }  // Section 2 Values
}
```

#### View

```razor
@model PT.Domain.Model.PageFlowModel

<form form-edit asp-action="Edit">
    
    <partial name="_SectionTemplateBuilder" model="@(new SectionTemplateBuilderModel 
    { 
        SectionNumber = 1,
        Title = "Hero Banner",
        Icon = "panorama",
        IsExpanded = true,
        InputTemplate = nameof(Model.Input1),
        InputValue = nameof(Model.Input2),
        TemplateValue = Model.Input1,
        ValueJson = Model.Input2,
        TemplateSource = "SectionTemplates1"
    })" />
    
    <partial name="_SectionTemplateBuilder" model="@(new SectionTemplateBuilderModel 
    { 
        SectionNumber = 2,
        Title = "Content Blocks",
        Icon = "view_module",
        IsExpanded = false,
        InputTemplate = nameof(Model.Input3),
        InputValue = nameof(Model.Input4),
        TemplateValue = Model.Input3,
        ValueJson = Model.Input4,
        TemplateSource = "SectionTemplates2"
    })" />
    
</form>

<script src="~/js/templates/section-templates-1.js"></script>
<script src="~/js/templates/section-templates-2.js"></script>
```

---

## ?? PROMPTS ?? S? D?NG

### **Prompt 1: Thêm Section m?i**

```
Tôi mu?n thêm Section 3 m?i vào màn hình EventEdit.cshtml:

**Yêu c?u:**
- M?c ?ích: Qu?n lý Timeline & Events
- Templates: SectionTemplates3
- Inputs: Input5 (Template), Input6 (Values)
- Title: "Section III: Timeline & Events"
- Icon: timeline
- IsExpanded: false

**Công vi?c:**
1. Thêm Input5, Input6 vào BlogModel.cs
2. Thêm Partial View vào EventEdit.cshtml
3. Load script section-templates-3.js

S? d?ng c?u trúc ?ã có trong workspace (_SectionTemplateBuilder.cshtml).
```

### **Prompt 2: T?o Template m?i**

```
T?o template "Video Embed" trong section-templates-4.js:

**Fields:**
- videoUrl: YouTube/Vimeo URL
- title: Tiêu ?? video
- description: Mô t?
- thumbnail: ?nh thumbnail

**HTML:**
```
<div class="video-wrapper">
    <iframe src="[videoUrl]"></iframe>
    <h4>[title]</h4>
    <p>[description]</p>
</div>
```

**Sample:**
- videoUrl: "https://www.youtube.com/embed/dQw4w9WgXcQ"
- title: "Sample Video"
- description: "Video description"

Thêm vào section-templates-4.js theo format ?ang có.
```

### **Prompt 3: Áp d?ng cho màn hình m?i**

```
Áp d?ng Dynamic Template Builder cho màn hình "PageFlowManager/Edit.cshtml":

**Config:**
- Model: PageFlowModel
- Section 1: Hero (SectionTemplates1) - Input1, Input2
- Section 2: Content (SectionTemplates2) - Input3, Input4

**Tasks:**
1. Thêm properties vào PageFlowModel.cs
2. T?o PageFlowManager/Edit.cshtml
3. S? d?ng _SectionTemplateBuilder.cshtml
4. Load templates scripts

Follow structure t? BlogManager.
```

---

## ??? TROUBLESHOOTING

| V?n ?? | Nguyên nhân | Gi?i pháp |
|--------|-------------|-----------|
| **Templates không hi?n** | Script ch?a load | Check `<script src="...">` ??t CU?I form |
| **Button không work** | Button ID sai | Verify `AddFieldBtnId`, `AddItemBtnId` |
| **JSON parse error** | Invalid JSON | Validate t?i JSONLint.com |
| **Template không t?n t?i** | TemplateSource sai | Check `window.SectionTemplates1` exists |
| **Click không ph?n ?ng** | jQuery ch?a load | Ensure jQuery loaded before scripts |

---

## ?? FEATURES

### ? ?ã hoàn thành

- ? Dynamic Sections (unlimited)
- ? Template Management
- ? JSON Editor (add field/item)
- ? Context Menu (right-click)
- ? Standalone Mode (no external dependency)
- ? Retry Logic
- ? Notification System
- ? Responsive UI

### ?? Có th? m? r?ng

- ?? Template Preview
- ?? Drag & Drop (reorder)
- ?? Import/Export templates
- ?? JSON Validation
- ?? Undo/Redo
- ?? Advanced placeholders
- ?? Multi-language
- ?? AI-powered generation

---

## ?? TÀI LI?U THAM KH?O

- **Core Model**: `2.Domain/Model/SectionTemplateBuilderModel.cs`
- **Partial View**: `1.Backend/Areas/Manager/Views/Shared/_SectionTemplateBuilder.cshtml`
- **Example Usage**: `1.Backend/Areas/Manager/Views/BlogManager/EventEdit.cshtml`
- **Template Format**: `1.Backend/wwwroot/js/templates/section-templates-1.js`

---

## ? K?T LU?N

H? th?ng **Dynamic Template Builder** cung c?p:

1. ? **D? s? d?ng** - Copy/paste config là xong
2. ? **Self-contained** - Không ph? thu?c external
3. ? **Reusable** - Dùng cho nhi?u màn hình
4. ? **Extensible** - D? m? r?ng
5. ? **Production-ready** - Stable & tested

**S? d?ng các Prompts trên ?? t?o m?i ho?c m? r?ng!** ??

---

## ?? SUPPORT

N?u g?p v?n ??, hãy:
1. Check console log (F12)
2. Verify file paths
3. Validate JSON format
4. Check button IDs match Model properties

Happy coding! ??
