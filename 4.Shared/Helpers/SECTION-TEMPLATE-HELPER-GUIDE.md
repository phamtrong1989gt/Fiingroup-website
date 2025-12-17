# SECTION TEMPLATE HELPER - H??NG D?N S? D?NG

## ?? **M?c l?c**
1. [Gi?i thi?u](#gi?i-thi?u)
2. [Cài ??t](#cài-??t)
3. [Các ph??ng th?c chính](#các-ph??ng-th?c-chính)
4. [Wrapper Div Structure](#wrapper-div-structure)
5. [Ví d? s? d?ng](#ví-d?-s?-d?ng)
6. [Use Cases](#use-cases)

---

## ?? **1. Gi?i thi?u**

`SectionTemplateHelper` là utility class giúp g?p và render t?t c? các sections (Input1-10) c?a `ContentPage` thành m?t HTML string duy nh?t.

### **Tính n?ng:**
? Render template HTML v?i JSON data  
? H? tr? `[For]...[/For]` loops  
? Replace placeholders `[fieldName]`  
? **Auto-wrap m?i section trong `<div>` riêng**  
? Custom section order  
? Custom CSS classes cho m?i section  
? Minify HTML  
? Validate JSON  

---

## ?? **2. Cài ??t**

### **2.1. File structure:**
```
4.Shared/
??? Helpers/
    ??? SectionTemplateHelper.cs

2.Domain/
??? Extensions/
    ??? ContentPageExtensions.cs
```

### **2.2. Namespace:**
```csharp
using PT.Shared.Helpers;
using PT.Domain.Extensions; // Extension methods
```

---

## ?? **3. Wrapper Div Structure**

### **3.1. Default wrapper (wrapInDiv = true):**

M?i section ???c wrap trong m?t `<div>` v?i:
- **Class**: `section section-{number}`
- **Data attribute**: `data-section="{number}"`

```html
<div class="section section-1" data-section="1">
    <!-- Section 1 HTML content here -->
</div>

<div class="section section-2" data-section="2">
    <!-- Section 2 HTML content here -->
</div>

<div class="section section-3" data-section="3">
    <!-- Section 3 HTML content here -->
</div>
```

### **3.2. CSS Styling:**

```css
/* Style t?t c? sections */
.section {
    margin-bottom: 50px;
    padding: 20px;
}

/* Style section c? th? */
.section-1 {
    background: linear-gradient(135deg, #667eea 0%, #764ba2 100%);
    color: white;
}

.section-2 {
    background: #f8f9fa;
}

.section-3 {
    border-left: 4px solid #007bff;
}

/* Responsive */
@media (max-width: 768px) {
    .section {
        margin-bottom: 30px;
        padding: 15px;
    }
}
```

### **3.3. JavaScript targeting:**

```javascript
// Get specific section
const section1 = document.querySelector('.section-1');
const section3 = document.querySelector('[data-section="3"]');

// Get all sections
const allSections = document.querySelectorAll('.section');

// Loop through sections
allSections.forEach((section, index) => {
    const sectionNumber = section.getAttribute('data-section');
    console.log(`Section ${sectionNumber} height:`, section.offsetHeight);
});

// Lazy load sections
const observer = new IntersectionObserver((entries) => {
    entries.forEach(entry => {
        if (entry.isIntersecting) {
            entry.target.classList.add('loaded');
        }
    });
});

allSections.forEach(section => observer.observe(section));
```

---

## ?? **4. Các ph??ng th?c chính**

### **4.1. RenderAllSections() - WITH WRAPPER**

```csharp
// Default: Wrap each section in div
string html = contentPage.RenderAllSections();

// Output:
// <div class="section section-1" data-section="1">...</div>
// <div class="section section-2" data-section="2">...</div>
// <div class="section section-3" data-section="3">...</div>
```

### **4.2. RenderAllSections(wrapInDiv: false) - NO WRAPPER**

```csharp
// No wrapper divs
string html = contentPage.RenderAllSections(wrapInDiv: false);

// Output:
// <!-- Section 1 content -->
// <!-- Section 2 content -->
// <!-- Section 3 content -->
```

### **4.3. RenderAllSections(order, wrapInDiv) - CUSTOM ORDER**

```csharp
// Custom order with wrapper
string html = contentPage.RenderAllSections(
    sectionOrder: new List<int> { 5, 3, 1 },
    wrapInDiv: true
);

// Output:
// <div class="section section-5" data-section="5">...</div>
// <div class="section section-3" data-section="3">...</div>
// <div class="section section-1" data-section="1">...</div>
```

### **4.4. RenderSection(sectionNumber, wrapInDiv)**

```csharp
// Single section WITHOUT wrapper (default)
string html = contentPage.RenderSection(2);

// Single section WITH wrapper
string html = contentPage.RenderSection(2, wrapInDiv: true);
// Output: <div class="section section-2" data-section="2">...</div>
```

### **4.5. RenderWithCssClasses() - CUSTOM CSS**

```csharp
var cssClasses = new Dictionary<int, string>
{
    { 1, "hero-section bg-primary" },
    { 2, "reasons-section text-center" },
    { 3, "agenda-section timeline-style" },
    { 5, "faq-section accordion-style" }
};

string html = contentPage.RenderWithCssClasses(
    sectionOrder: new List<int> { 1, 2, 3, 5 },
    cssClasses: cssClasses
);

// Output:
// <div class="section section-1 hero-section bg-primary" data-section="1">...</div>
// <div class="section section-2 reasons-section text-center" data-section="2">...</div>
// <div class="section section-3 agenda-section timeline-style" data-section="3">...</div>
// <div class="section section-5 faq-section accordion-style" data-section="5">...</div>
```

### **4.6. RenderWithCustomWrapper() - CUSTOM HTML WRAPPER**

```csharp
// Custom wrapper format: {0} = section number, {1} = content
string html = contentPage.RenderWithCustomWrapper(
    sectionOrder: new List<int> { 1, 2, 3 },
    wrapperFormat: "<section id='section-{0}' class='custom-section'>{1}</section>"
);

// Output:
// <section id='section-1' class='custom-section'>...</section>
// <section id='section-2' class='custom-section'>...</section>
// <section id='section-3' class='custom-section'>...</section>
```

---

## ?? **5. Ví d? s? d?ng**

### **Ví d? 1: Event Landing Page v?i wrapper divs**

```csharp
public async Task<IActionResult> EventDetails(int id)
{
    var eventPage = await _repo.SingleOrDefaultAsync(x => x.Id == id);
    
    // Render all sections with default wrapper
    ViewBag.AllSectionsHtml = eventPage.RenderAllSections();
    
    return View(eventPage);
}
```

**View:**
```razor
@model ContentPage

<div class="event-page">
    @Html.Raw(ViewBag.AllSectionsHtml)
</div>
```

**Output HTML:**
```html
<div class="event-page">
    <div class="section section-1" data-section="1">
        <!-- Hero Slider -->
        <div class="hero-slider">...</div>
    </div>
    
    <div class="section section-2" data-section="2">
        <!-- Reasons to Join -->
        <div class="reasons-grid">...</div>
    </div>
    
    <div class="section section-3" data-section="3">
        <!-- Event Agenda -->
        <div class="agenda-timeline">...</div>
    </div>
    
    <div class="section section-4" data-section="4">
        <!-- Speakers -->
        <div class="speakers-grid">...</div>
    </div>
    
    <div class="section section-5" data-section="5">
        <!-- FAQ -->
        <div class="faq-accordion">...</div>
    </div>
</div>
```

---

### **Ví d? 2: Custom CSS classes cho sections**

```csharp
public async Task<IActionResult> StyledEvent(int id)
{
    var eventPage = await _repo.SingleOrDefaultAsync(x => x.Id == id);
    
    var cssClasses = new Dictionary<int, string>
    {
        { 1, "hero-fullscreen parallax-bg" },
        { 2, "reasons-animated fade-in-up" },
        { 3, "agenda-timeline dark-theme" },
        { 4, "speakers-carousel owl-carousel" },
        { 5, "faq-collapsible bootstrap-accordion" }
    };
    
    ViewBag.StyledHtml = eventPage.RenderWithCssClasses(
        sectionOrder: new List<int> { 1, 2, 3, 4, 5 },
        cssClasses: cssClasses
    );
    
    return View(eventPage);
}
```

**Output HTML:**
```html
<div class="section section-1 hero-fullscreen parallax-bg" data-section="1">
    <!-- Hero with parallax background -->
</div>

<div class="section section-2 reasons-animated fade-in-up" data-section="2">
    <!-- Reasons with animation -->
</div>

<div class="section section-3 agenda-timeline dark-theme" data-section="3">
    <!-- Agenda with dark theme -->
</div>

<div class="section section-4 speakers-carousel owl-carousel" data-section="4">
    <!-- Speakers carousel -->
</div>

<div class="section section-5 faq-collapsible bootstrap-accordion" data-section="5">
    <!-- FAQ accordion -->
</div>
```

---

### **Ví d? 3: Custom HTML5 semantic wrapper**

```csharp
public async Task<IActionResult> SemanticView(int id)
{
    var eventPage = await _repo.SingleOrDefaultAsync(x => x.Id == id);
    
    // Use HTML5 semantic tags
    ViewBag.SemanticHtml = eventPage.RenderWithCustomWrapper(
        sectionOrder: new List<int> { 1, 2, 3, 4, 5 },
        wrapperFormat: "<section id='section-{0}' class='event-section' aria-label='Event Section {0}'>{1}</section>"
    );
    
    return View(eventPage);
}
```

**Output HTML:**
```html
<section id='section-1' class='event-section' aria-label='Event Section 1'>
    <!-- Hero content -->
</section>

<section id='section-2' class='event-section' aria-label='Event Section 2'>
    <!-- Reasons content -->
</section>

<section id='section-3' class='event-section' aria-label='Event Section 3'>
    <!-- Agenda content -->
</section>
```

---

### **Ví d? 4: Conditional rendering v?i wrapper**

```csharp
public async Task<IActionResult> ConditionalView(int id)
{
    var contentPage = await _repo.SingleOrDefaultAsync(x => x.Id == id);
    
    // Get active sections
    var activeSections = contentPage.GetActiveSections();
    
    // Render with wrapper
    string html = contentPage.RenderAllSections(
        sectionOrder: activeSections,
        wrapInDiv: true
    );
    
    ViewBag.ConditionalHtml = html;
    return View(contentPage);
}
```

---

### **Ví d? 5: Mobile view - No wrapper for performance**

```csharp
public async Task<IActionResult> MobileView(int id)
{
    var contentPage = await _repo.SingleOrDefaultAsync(x => x.Id == id);
    
    // Mobile: No wrapper divs to reduce HTML size
    ViewBag.MobileHtml = contentPage.RenderAllSections(
        sectionOrder: new List<int> { 2, 3, 5 },
        wrapInDiv: false // No wrapper for mobile
    );
    
    return View(contentPage);
}
```

---

### **Ví d? 6: Progressive loading v?i wrapper divs**

```razor
@model ContentPage

<div class="event-container">
    @foreach (var sectionNum in Model.GetActiveSections())
    {
        var html = Model.RenderSection(sectionNum, wrapInDiv: true);
        @Html.Raw(html)
    }
</div>

<script>
// Lazy load sections
document.querySelectorAll('.section').forEach((section, index) => {
    section.style.opacity = '0';
    section.style.transform = 'translateY(50px)';
    
    setTimeout(() => {
        section.style.transition = 'all 0.6s ease-out';
        section.style.opacity = '1';
        section.style.transform = 'translateY(0)';
    }, index * 200); // Stagger animation
});
</script>
```

---

### **Ví d? 7: Analytics tracking v?i data attributes**

```razor
@{
    var html = Model.RenderAllSections();
}

@Html.Raw(html)

<script>
// Track section views
const observer = new IntersectionObserver((entries) => {
    entries.forEach(entry => {
        if (entry.isIntersecting) {
            const sectionNum = entry.target.getAttribute('data-section');
            
            // Send analytics
            gtag('event', 'section_view', {
                'event_category': 'engagement',
                'event_label': `Section ${sectionNum}`,
                'value': sectionNum
            });
            
            // Mark as tracked
            entry.target.classList.add('tracked');
            observer.unobserve(entry.target);
        }
    });
}, { threshold: 0.5 });

document.querySelectorAll('.section').forEach(section => {
    observer.observe(section);
});
</script>
```

---

## ?? **6. Use Cases**

### **Use Case 1: Scroll-spy navigation**

```razor
@model ContentPage

<!-- Navigation -->
<nav class="sticky-nav">
    <ul>
        @foreach (var sectionNum in Model.GetActiveSections())
        {
            <li><a href="#section-@sectionNum">Section @sectionNum</a></li>
        }
    </ul>
</nav>

<!-- Sections -->
@{
    var cssClasses = new Dictionary<int, string>
    {
        { 1, "scroll-section" },
        { 2, "scroll-section" },
        { 3, "scroll-section" },
        { 4, "scroll-section" },
        { 5, "scroll-section" }
    };
    
    var html = Model.RenderWithCssClasses(Model.GetActiveSections(), cssClasses);
}

@Html.Raw(html)

<script>
// Scroll spy
document.querySelectorAll('.scroll-section').forEach((section, index) => {
    section.id = `section-${section.getAttribute('data-section')}`;
});

window.addEventListener('scroll', () => {
    document.querySelectorAll('.scroll-section').forEach(section => {
        const rect = section.getBoundingClientRect();
        if (rect.top >= 0 && rect.top <= 200) {
            const sectionNum = section.getAttribute('data-section');
            document.querySelector(`.sticky-nav a[href="#section-${sectionNum}"]`)
                ?.classList.add('active');
        }
    });
});
</script>
```

---

### **Use Case 2: Print-friendly view**

```csharp
public async Task<IActionResult> PrintView(int id)
{
    var contentPage = await _repo.SingleOrDefaultAsync(x => x.Id == id);
    
    // Custom wrapper for print
    ViewBag.PrintHtml = contentPage.RenderWithCustomWrapper(
        sectionOrder: contentPage.GetActiveSections(),
        wrapperFormat: "<div class='print-section' style='page-break-after: always;'>{1}</div>"
    );
    
    return View("PrintView", contentPage);
}
```

---

### **Use Case 3: A/B Testing v?i different wrappers**

```csharp
public async Task<IActionResult> ABTest(int id, string variant)
{
    var contentPage = await _repo.SingleOrDefaultAsync(x => x.Id == id);
    
    string html;
    
    if (variant == "B")
    {
        // Variant B: Custom wrapper with animations
        var cssClasses = new Dictionary<int, string>
        {
            { 1, "animate-fade-in" },
            { 2, "animate-slide-up" },
            { 3, "animate-zoom-in" }
        };
        
        html = contentPage.RenderWithCssClasses(
            sectionOrder: new List<int> { 1, 2, 3 },
            cssClasses: cssClasses
        );
    }
    else
    {
        // Variant A: Default wrapper
        html = contentPage.RenderAllSections(
            sectionOrder: new List<int> { 1, 2, 3 }
        );
    }
    
    ViewBag.TestHtml = html;
    return View(contentPage);
}
```

---

## ?? **7. Performance Tips**

### **Tip 1: Use data attributes for JavaScript**
```javascript
// Good: Use data attributes
const section = document.querySelector('[data-section="3"]');

// Bad: Use class selectors with numbers
const section = document.querySelector('.section-3');
```

### **Tip 2: Lazy load images in sections**
```html
<div class="section section-1" data-section="1">
    <img data-src="image.jpg" class="lazy-load" />
</div>

<script>
document.querySelectorAll('.section img.lazy-load').forEach(img => {
    const observer = new IntersectionObserver((entries) => {
        entries.forEach(entry => {
            if (entry.isIntersecting) {
                img.src = img.getAttribute('data-src');
                img.classList.remove('lazy-load');
                observer.unobserve(img);
            }
        });
    });
    observer.observe(img);
});
</script>
```

### **Tip 3: Minify HTML for production**
```csharp
#if RELEASE
    html = contentPage.RenderAllSectionsMinified(wrapInDiv: true);
#else
    html = contentPage.RenderAllSections(wrapInDiv: true);
#endif
```

---

## ? **8. Summary**

### **HTML Output Structure:**
```html
<div class="section section-1" data-section="1">
    {Section 1 rendered content}
</div>

<div class="section section-2" data-section="2">
    {Section 2 rendered content}
</div>

<div class="section section-3" data-section="3">
    {Section 3 rendered content}
</div>
```

### **Benefits:**
? **Separation** - Each section isolated in own div  
? **Styling** - Easy to apply CSS per section  
? **JavaScript** - Easy to target with selectors  
? **Analytics** - Track section views with data attributes  
? **Accessibility** - Can add ARIA labels  
? **Responsive** - Control sections independently  

Happy coding! ??
