# ?? FIINGROUP - Section Template Generator Prompt

## ?? PROJECT CONTEXT

**Project:** Fiingroup Website CMS
**Technology Stack:**
- Backend: .NET 10, Razor Pages, C#
- Frontend: JavaScript (ES6), Bootstrap 5, jQuery
- Template Engine: Custom parser with `[fieldName]` and `[For]...[/For]` syntax
- Domain: cdn.fiingroup.vn

**Workspace Structure:**
```
D:\CongViec\Job\2025\5.Fiingroup-website\
??? 1.Backend (PT.UI) - Admin CMS
?   ??? wwwroot/js/templates/
?       ??? section-templates-1.js (Hero Slider, Carousel)
?       ??? section-templates-2.js (Reasons Grid, Cards)
?       ??? section-templates-3.js (Event Agenda, Timeline)
?       ??? section-templates-4.js (Speakers, Team)
?       ??? section-templates-5.js (FAQ, Accordion)
??? 2.Domain (PT.Domain) - Models & Extensions
??? 3.Infrastructure (PT.Infrastructure) - Data Access
??? 4.Shared (PT.Share) - Helpers
?   ??? Helpers/
?       ??? SectionTemplateHelper.cs
??? 6.FE (PT.UI) - Frontend Portals
```

---

## ?? QUICK COPY-PASTE PROMPT

```
**CONTEXT:** Fiingroup Website Event Page Builder
**TASK:** Convert HTML to JavaScript template for section-templates-X.js

**HTML INPUT:**
```html
[PASTE YOUR HTML HERE]
```

**REQUIREMENTS:**
1. **Analyze patterns:**
   - Identify repeating elements ? Use `[For]...[/For]` loops
   - Identify dynamic content ? Use `[fieldName]` placeholders
   - Identify desktop/mobile splits ? Use `[ForDesktop]` / `[ForMobile]`
   - Identify carousel indicators ? Use `[ForIndicator]`

2. **Template syntax:**
   - Placeholders: `[fieldName]` (camelCase)
   - Simple loop: `[For]...[/For]`
   - Named loops: `[ForDesktop]`, `[ForMobile]`, `[ForIndicator]`, etc.
   - Active state: `[activeClass]` (first = "active", rest = "")
   - Indexes: `[slideIndex]` (1-based), `[slideIndexZero]` (0-based)

3. **JSON data:**
   - Use realistic Fiingroup content
   - URLs: `https://cdn.fiingroup.vn/medialib/...`
   - Min 2-3 examples per array
   - First item has `activeClass: "active"`
   - Complete all fields with real-looking data

4. **Template naming:**
   - camelCase: `heroSlider`, `carouselReasons`, `agendaTimeline`
   - Descriptive: Explains what template does
   - Consistent: Follow existing patterns

**OUTPUT FORMAT:**
```javascript
const SectionTemplatesX = {
    templateName: {
        name: "Display Name",
        description: "Clear description of template purpose and usage",
        template: `
<!-- HTML with [placeholders] and [For] loops -->
<div class="section">
    [For]
    <div class="item [activeClass]">
        <h3>[title]</h3>
        <p>[description]</p>
    </div>
    [/For]
</div>
        `,
        values: {
            // JSON data structure
            For: [
                {
                    activeClass: "active",
                    title: "Realistic Fiingroup Title",
                    description: "Realistic description about Fiingroup services"
                },
                {
                    activeClass: "",
                    title: "Second Item Title",
                    description: "Second item description"
                }
            ]
        }
    }
};

// Export for browser
if (typeof window !== 'undefined') {
    window.SectionTemplatesX = SectionTemplatesX;
}
```

**FILE LOCATION:**
`1.Backend/wwwroot/js/templates/section-templates-X.js`

**USAGE IN RAZOR:**
```razor
<script src="~/js/templates/section-templates-X.js"></script>
```
```

---

## ?? FIINGROUP-SPECIFIC EXAMPLES

### Example 1: Event Hero Slider

**Input HTML:**
```html
<div class="hero-section">
    <div id="heroCarousel" class="carousel slide">
        <div class="carousel-indicators">
            <button class="active" data-bs-slide-to="0"></button>
            <button data-bs-slide-to="1"></button>
        </div>
        <div class="carousel-inner">
            <div class="carousel-item active">
                <img src="https://cdn.fiingroup.vn/medialib/245453/I/2025/03/19/hero1.jpg">
                <div class="carousel-caption">
                    <h1>FiinGroup Annual Conference 2025</h1>
                    <p>Join us for insights on Vietnam's financial market</p>
                </div>
            </div>
            <div class="carousel-item">
                <img src="https://cdn.fiingroup.vn/medialib/245453/I/2025/03/19/hero2.jpg">
                <div class="carousel-caption">
                    <h1>Trade Credit Insurance Seminar</h1>
                    <p>Protecting your export business</p>
                </div>
            </div>
        </div>
    </div>
</div>
```

**Expected Output:**
```javascript
const SectionTemplates1 = {
    heroSliderEvent: {
        name: "Event Hero Slider",
        description: "Full-width hero carousel for event landing pages with captions",
        template: `<div class="hero-section">
    <div id="[carouselId]" class="carousel slide">
        <div class="carousel-indicators">
            [ForIndicator]
            <button class="[activeClass]" data-bs-slide-to="[slideIndexZero]"></button>
            [/ForIndicator]
        </div>
        <div class="carousel-inner">
            [For]
            <div class="carousel-item [activeClass]">
                <img src="[imageUrl]">
                <div class="carousel-caption">
                    <h1>[title]</h1>
                    <p>[description]</p>
                </div>
            </div>
            [/For]
        </div>
    </div>
</div>`,
        values: {
            carouselId: "heroCarousel",
            ForIndicator: [
                { activeClass: "active", slideIndexZero: "0" },
                { activeClass: "", slideIndexZero: "1" }
            ],
            For: [
                {
                    activeClass: "active",
                    imageUrl: "https://cdn.fiingroup.vn/medialib/245453/I/2025/03/19/hero1.jpg",
                    title: "FiinGroup Annual Conference 2025",
                    description: "Join us for insights on Vietnam's financial market"
                },
                {
                    activeClass: "",
                    imageUrl: "https://cdn.fiingroup.vn/medialib/245453/I/2025/03/19/hero2.jpg",
                    title: "Trade Credit Insurance Seminar",
                    description: "Protecting your export business"
                }
            ]
        }
    }
};

if (typeof window !== 'undefined') {
    window.SectionTemplates1 = SectionTemplates1;
}
```

---

### Example 2: Desktop/Mobile Split (Real Fiingroup Use Case)

**Input HTML:**
```html
<div class="reasons-section">
    <!-- Desktop: 2-column layout -->
    <div class="carousel slide d-none d-md-block" id="carousel-desktop">
        <div class="carousel-inner">
            <div class="carousel-item active">
                <div class="row">
                    <div class="col-6">
                        <img src="https://cdn.fiingroup.vn/medialib/245453/I/2025/03/19/reason1.png">
                        <p>Leveraging trade credit insurance to reduce risk</p>
                    </div>
                    <div class="col-6">
                        <img src="https://cdn.fiingroup.vn/medialib/245453/I/2025/03/19/reason2.png">
                        <p>Digitalization trends in trade insurance</p>
                    </div>
                </div>
            </div>
        </div>
    </div>
    
    <!-- Mobile: Stacked layout -->
    <div class="carousel slide d-md-none" id="carousel-mobile">
        <div class="carousel-inner">
            <div class="carousel-item active">
                <img src="https://cdn.fiingroup.vn/medialib/245453/I/2025/03/19/reason1.png">
                <p>Leveraging trade credit insurance</p>
            </div>
            <div class="carousel-item">
                <img src="https://cdn.fiingroup.vn/medialib/245453/I/2025/03/19/reason2.png">
                <p>Digitalization trends</p>
            </div>
        </div>
    </div>
</div>
```

**Expected Output:**
```javascript
const SectionTemplates2 = {
    reasonsCarouselResponsive: {
        name: "Reasons Carousel - Desktop/Mobile",
        description: "2-column desktop carousel, single-column mobile carousel for event reasons",
        template: `<div class="reasons-section">
    <!-- Desktop: 2-column layout -->
    <div class="carousel slide d-none d-md-block" id="[carouselIdDesktop]">
        <div class="carousel-inner">
            [ForDesktop]
            <div class="carousel-item [activeClass]">
                <div class="row">
                    <div class="col-6">
                        <img src="[leftImage]">
                        <p>[leftContent]</p>
                    </div>
                    <div class="col-6">
                        <img src="[rightImage]">
                        <p>[rightContent]</p>
                    </div>
                </div>
            </div>
            [/ForDesktop]
        </div>
    </div>
    
    <!-- Mobile: Stacked layout -->
    <div class="carousel slide d-md-none" id="[carouselIdMobile]">
        <div class="carousel-inner">
            [ForMobile]
            <div class="carousel-item [activeClass]">
                <img src="[imageUrl]">
                <p>[content]</p>
            </div>
            [/ForMobile]
        </div>
    </div>
</div>`,
        values: {
            carouselIdDesktop: "carousel-desktop",
            carouselIdMobile: "carousel-mobile",
            ForDesktop: [
                {
                    activeClass: "active",
                    leftImage: "https://cdn.fiingroup.vn/medialib/245453/I/2025/03/19/reason1.png",
                    leftContent: "Leveraging trade credit insurance to reduce risk and optimize cash flow",
                    rightImage: "https://cdn.fiingroup.vn/medialib/245453/I/2025/03/19/reason2.png",
                    rightContent: "Digitalization and data analytics trends in trade insurance"
                }
            ],
            ForMobile: [
                {
                    activeClass: "active",
                    imageUrl: "https://cdn.fiingroup.vn/medialib/245453/I/2025/03/19/reason1.png",
                    content: "Leveraging trade credit insurance to reduce risk"
                },
                {
                    activeClass: "",
                    imageUrl: "https://cdn.fiingroup.vn/medialib/245453/I/2025/03/19/reason2.png",
                    content: "Digitalization and data analytics trends"
                }
            ]
        }
    }
};

if (typeof window !== 'undefined') {
    window.SectionTemplates2 = SectionTemplates2;
}
```

---

## ?? FIINGROUP TEMPLATE NAMING GUIDE

### Section 1: Hero & Sliders (section-templates-1.js)
- `heroSliderEvent` - Event landing page hero
- `heroSliderFull` - Full-screen hero slider
- `carouselBasic` - Basic Bootstrap carousel
- `carouselWithCaptions` - Carousel with text overlays

### Section 2: Reasons & Features (section-templates-2.js)
- `reasonsGrid` - 3-column reasons grid
- `reasonsCards` - Card-based reasons layout
- `reasonsCarouselResponsive` - Desktop 2-col / Mobile stacked
- `featuresGrid` - Feature highlights grid

### Section 3: Event Agenda (section-templates-3.js)
- `agendaTimeline` - Vertical timeline layout
- `agendaDesktop` - Desktop detailed schedule
- `agendaMobile` - Mobile-friendly schedule
- `agendaTabs` - Tabbed multi-day agenda

### Section 4: Speakers & Team (section-templates-4.js)
- `speakersGrid` - 4-column speakers grid
- `speakersCarousel` - Speakers in carousel
- `speakersWithBio` - Speakers with detailed bio
- `teamGrid` - Team members grid

### Section 5: FAQ & Contact (section-templates-5.js)
- `faqAccordion` - Bootstrap accordion FAQ
- `faqCollapsible` - Custom collapsible FAQ
- `contactForm` - Contact form with map
- `registrationForm` - Event registration form

---

## ?? FIINGROUP CONTENT PATTERNS

### Financial Services Content
```javascript
{
    title: "Trade Credit Insurance Solutions",
    description: "Protect your export business from partner defaults and optimize cash flow",
    imageUrl: "https://cdn.fiingroup.vn/medialib/245453/I/2025/03/19/trade-insurance.jpg"
}
```

### Event Content
```javascript
{
    eventName: "FiinGroup Annual Conference 2025",
    date: "March 25, 2025",
    location: "Hanoi, Vietnam",
    description: "Join industry leaders for insights on Vietnam's financial market"
}
```

### Speakers Content
```javascript
{
    name: "Dr. Nguyen Van A",
    title: "Chief Economist, FiinGroup",
    bio: "20+ years experience in financial analysis and market research",
    imageUrl: "https://cdn.fiingroup.vn/medialib/245453/I/2025/03/19/speaker1.jpg"
}
```

---

## ? QUALITY CHECKLIST

Before submitting, verify:

- [ ] Template name is camelCase and descriptive
- [ ] Description explains when to use template
- [ ] All repeating patterns use `[For]` loops
- [ ] All dynamic content uses `[fieldName]` placeholders
- [ ] First item has `activeClass: "active"`, rest have `""`
- [ ] JSON has 2-3 realistic Fiingroup examples
- [ ] All URLs use `cdn.fiingroup.vn` domain
- [ ] Field names are camelCase and consistent
- [ ] No hardcoded IDs (use `[carouselId]` placeholder)
- [ ] File exports: `window.SectionTemplatesX = SectionTemplatesX`
- [ ] Bootstrap 5 classes used correctly
- [ ] Responsive classes: `d-none d-md-block`, `d-md-none`

---

## ?? USAGE IN FIINGROUP CMS

### 1. Generate Template
```
[Paste HTML] ? AI generates section-templates-X.js
```

### 2. Save File
```
1.Backend/wwwroot/js/templates/section-templates-X.js
```

### 3. Reference in View
```razor
@* EventEdit.cshtml *@
<script src="~/js/templates/section-templates-1.js"></script>
<script src="~/js/templates/section-templates-2.js"></script>
```

### 4. Use in Template Builder
```razor
<partial name="_SectionTemplateBuilder" model="@(new SectionTemplateBuilderModel 
{ 
    SectionNumber = 1,
    Title = "Section I: Hero Slider",
    TemplateSource = "SectionTemplates1",
    InputTemplate = nameof(Model.Input1),
    InputValue = nameof(Model.Input2)
})" />
```

### 5. Render in Controller
```csharp
using PT.Domain.Extensions;
using PT.Shared.Helpers;

// Method 1: Simple
string html = ContentPageExtensions.GetHtmlSection(input1, input2);

// Method 2: Multiple sections
string html = ContentPageExtensions.GetHtmlSections(
    (input1, input2),
    (input3, input4)
);

// Method 3: From ContentPage entity
string html = contentPage.RenderAllSections();
```

---

## ?? COMPLETE FILE PATHS

```
Project Root: D:\CongViec\Job\2025\5.Fiingroup-website\

Templates (JavaScript):
- 1.Backend\wwwroot\js\templates\section-templates-1.js
- 1.Backend\wwwroot\js\templates\section-templates-2.js
- 1.Backend\wwwroot\js\templates\section-templates-3.js
- 1.Backend\wwwroot\js\templates\section-templates-4.js
- 1.Backend\wwwroot\js\templates\section-templates-5.js

Helpers (C#):
- 4.Shared\Helpers\SectionTemplateHelper.cs

Extensions (C#):
- 2.Domain\Extensions\ContentPageExtensions.cs

Views (Razor):
- 1.Backend\Areas\Manager\Views\BlogManager\EventEdit.cshtml
- 1.Backend\Areas\Manager\Views\Shared\_SectionTemplateBuilder.cshtml

Documentation:
- DOCS\PROMPT-TEMPLATE-GENERATOR.md
- DOCS\QUICK-PROMPT.md
- DOCS\SYSTEM-PROMPT.md
```

---

## ?? NEXT STEPS

1. **Copy prompt above**
2. **Paste your HTML**
3. **Get JavaScript template**
4. **Save to correct file**
5. **Test in EventEdit page**
6. **Verify rendering in C#**

---

**?? Ready to generate Fiingroup templates!**
