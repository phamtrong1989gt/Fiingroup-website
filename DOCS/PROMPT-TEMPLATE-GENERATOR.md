# ?? PROMPT TEMPLATE - AUTO GENERATE SECTION TEMPLATES

## ?? M?C ?ÍCH
Prompt này giúp Copilot t? ??ng phân tích HTML string và generate ra JavaScript template file (section-templates-X.js) v?i c?u trúc chu?n.

---

## ?? PROMPT TEMPLATE (Copy & Paste khi c?n)

```
Tôi có m?t ?o?n HTML template c?n convert thành JavaScript template file cho system Section Template Builder.

**YÊU C?U:**
1. Phân tích HTML và tìm t?t c? các pattern l?p l?i
2. T?o ra template HTML v?i placeholder `[fieldName]` và `[For]...[/For]` loops
3. Generate JSON data structure t??ng ?ng
4. T?o file JavaScript theo c?u trúc: `section-templates-X.js`
5. ??t tên template d?a trên n?i dung HTML (ví d?: heroSlider, carouselReasons, eventAgenda, etc.)

**C?U TRÚC FILE OUTPUT:**
- File location: `1.Backend/wwwroot/js/templates/section-templates-X.js`
- Format: ES6 JavaScript
- Structure:
  ```javascript
  const SectionTemplatesX = {
      templateName1: {
          name: "Template Display Name",
          description: "Description of template",
          template: `HTML v?i [placeholders] và [For] loops`,
          values: { JSON data structure }
      },
      templateName2: { ... }
  };
  ```

**HTML INPUT:**
```html
[PASTE HTML STRING HERE]
```

**H??NG D?N PHÂN TÍCH:**
1. Tìm các ph?n t? l?p l?i ? Dùng `[For]...[/For]`
2. Tìm các giá tr? ??ng ? Dùng `[fieldName]`
3. Gi? nguyên structure HTML và CSS classes
4. T?o JSON data v?i ít nh?t 2-3 items m?u cho m?i array
5. ??t tên fields rõ ràng, d? hi?u (camelCase)

**OUTPUT M?U:**
Generate ra:
- File `section-templates-X.js` hoàn ch?nh
- Có ít nh?t 2-3 template variants khác nhau
- JSON data ??y ?? và realistic
- Comments gi?i thích cách s? d?ng
```

---

## ?? CÁCH S? D?NG

### B??c 1: Copy Prompt Template
Copy toàn b? n?i dung trong khung "PROMPT TEMPLATE" ? trên.

### B??c 2: Thay th? HTML String
Thay th? `[PASTE HTML STRING HERE]` b?ng HTML th?c t? c?a b?n.

### B??c 3: G?i cho Copilot
Paste vào chat và Copilot s? t? ??ng generate file JavaScript.

---

## ?? VÍ D? C? TH?

### Example 1: Hero Slider

**INPUT HTML:**
```html
<div class="hero-section">
    <div class="hero-slider">
        <div class="slide active">
            <img src="https://example.com/slide1.jpg" alt="Slide 1">
            <h2>Slide 1 Title</h2>
            <p>Slide 1 Description</p>
        </div>
        <div class="slide">
            <img src="https://example.com/slide2.jpg" alt="Slide 2">
            <h2>Slide 2 Title</h2>
            <p>Slide 2 Description</p>
        </div>
    </div>
</div>
```

**EXPECTED OUTPUT:**
```javascript
const SectionTemplates1 = {
    heroSlider: {
        name: "Hero Slider",
        description: "Full-width hero slider with images and text",
        template: `<div class="hero-section">
    <div class="hero-slider">
        [For]
        <div class="slide [activeClass]">
            <img src="[imageUrl]" alt="[altText]">
            <h2>[title]</h2>
            <p>[description]</p>
        </div>
        [/For]
    </div>
</div>`,
        values: {
            For: [
                {
                    activeClass: "active",
                    imageUrl: "https://example.com/slide1.jpg",
                    altText: "Slide 1",
                    title: "Slide 1 Title",
                    description: "Slide 1 Description"
                },
                {
                    activeClass: "",
                    imageUrl: "https://example.com/slide2.jpg",
                    altText: "Slide 2",
                    title: "Slide 2 Title",
                    description: "Slide 2 Description"
                }
            ]
        }
    }
};
```

---

### Example 2: Bootstrap Carousel v?i Multiple For Loops

**INPUT HTML:**
```html
<div class="carousel slide" id="myCarousel">
    <div class="carousel-indicators">
        <button class="active" data-bs-slide-to="0" aria-current="true"></button>
        <button data-bs-slide-to="1"></button>
    </div>
    <div class="carousel-inner">
        <div class="carousel-item active">
            <img src="image1.jpg" alt="Slide 1">
            <div class="carousel-caption">
                <h3>Title 1</h3>
                <p>Description 1</p>
            </div>
        </div>
        <div class="carousel-item">
            <img src="image2.jpg" alt="Slide 2">
            <div class="carousel-caption">
                <h3>Title 2</h3>
                <p>Description 2</p>
            </div>
        </div>
    </div>
</div>
```

**EXPECTED OUTPUT:**
```javascript
const SectionTemplates2 = {
    bootstrapCarousel: {
        name: "Bootstrap Carousel",
        description: "Bootstrap 5 carousel with indicators and captions",
        template: `<div class="carousel slide" id="[carouselId]">
    <div class="carousel-indicators">
        [ForIndicator]
        <button class="[activeClass]" data-bs-slide-to="[slideIndex]" [ariaCurrent]></button>
        [/ForIndicator]
    </div>
    <div class="carousel-inner">
        [For]
        <div class="carousel-item [activeClass]">
            <img src="[imageUrl]" alt="[altText]">
            <div class="carousel-caption">
                <h3>[title]</h3>
                <p>[description]</p>
            </div>
        </div>
        [/For]
    </div>
</div>`,
        values: {
            carouselId: "myCarousel",
            ForIndicator: [
                {
                    activeClass: "active",
                    slideIndex: "0",
                    ariaCurrent: 'aria-current="true"'
                },
                {
                    activeClass: "",
                    slideIndex: "1",
                    ariaCurrent: ""
                }
            ],
            For: [
                {
                    activeClass: "active",
                    imageUrl: "https://example.com/image1.jpg",
                    altText: "Slide 1",
                    title: "Title 1",
                    description: "Description 1"
                },
                {
                    activeClass: "",
                    imageUrl: "https://example.com/image2.jpg",
                    altText: "Slide 2",
                    title: "Title 2",
                    description: "Description 2"
                }
            ]
        }
    }
};
```

---

## ?? TEMPLATE NAMING CONVENTIONS

| Template Type | Template Name | File |
|--------------|---------------|------|
| Hero Slider | `heroSlider` | section-templates-1.js |
| Carousel | `carouselBasic`, `carouselReasons` | section-templates-1.js |
| Reasons Grid | `reasonsGrid`, `reasonsCards` | section-templates-2.js |
| Event Agenda | `agendaTimeline`, `agendaDesktop` | section-templates-3.js |
| Speakers | `speakersGrid`, `speakersCarousel` | section-templates-4.js |
| FAQ | `faqAccordion`, `faqCollapsible` | section-templates-5.js |

---

## ?? PHÂN TÍCH PATTERNS

### Pattern 1: Simple Loop
```html
<!-- HTML có pattern l?p -->
<div class="item">Item 1</div>
<div class="item">Item 2</div>
<div class="item">Item 3</div>

<!-- Template output -->
[For]
<div class="item">[itemName]</div>
[/For]

<!-- JSON -->
{
  "For": [
    {"itemName": "Item 1"},
    {"itemName": "Item 2"},
    {"itemName": "Item 3"}
  ]
}
```

### Pattern 2: Nested Structure
```html
<!-- HTML có c?u trúc ph?c t?p -->
<div class="row">
    <div class="col-6">
        <img src="left.jpg">
        <p>Left content</p>
    </div>
    <div class="col-6">
        <img src="right.jpg">
        <p>Right content</p>
    </div>
</div>

<!-- Template output -->
[For]
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
[/For]
```

### Pattern 3: Multiple For Loops
```html
<!-- HTML có nhi?u sections -->
<div class="indicators">
    <button class="active"></button>
    <button></button>
</div>
<div class="slides">
    <div class="slide active">Slide 1</div>
    <div class="slide">Slide 2</div>
</div>

<!-- Template output -->
<div class="indicators">
    [ForIndicator]
    <button class="[activeClass]"></button>
    [/ForIndicator]
</div>
<div class="slides">
    [ForSlide]
    <div class="slide [activeClass]">[content]</div>
    [/ForSlide]
</div>
```

---

## ? CHECKLIST

Khi generate template, ??m b?o:

- [ ] Template name rõ ràng, mô t? ?úng ch?c n?ng
- [ ] Description gi?i thích cách s? d?ng
- [ ] Template HTML có ??y ?? placeholders `[fieldName]`
- [ ] For loops ???c ??t tên rõ ràng: `[For]`, `[ForDesktop]`, `[ForMobile]`
- [ ] JSON values có ít nh?t 2-3 items ?? test
- [ ] activeClass ???c handle ?úng (item ??u = "active", các item khác = "")
- [ ] All URLs và paths là realistic (https://cdn.fiingroup.vn/...)
- [ ] Comments gi?i thích các ph?n ph?c t?p
- [ ] File ???c save ?úng location: `1.Backend/wwwroot/js/templates/`

---

## ?? QUICK START PROMPT

Copy prompt này và thay HTML:

```
Generate section template JavaScript file from this HTML:

HTML INPUT:
```html
[PASTE YOUR HTML HERE]
```

Requirements:
- Identify repeating patterns ? Use [For] loops
- Extract dynamic values ? Use [fieldName] placeholders
- Create realistic JSON data with 2-3 examples
- Follow naming convention (camelCase)
- Include comments and description
- Save to: section-templates-X.js

Structure:
```javascript
const SectionTemplatesX = {
    templateName: {
        name: "Display Name",
        description: "Description",
        template: `HTML with [placeholders]`,
        values: { JSON data }
    }
};
```
```

---

## ?? SUPPORT

N?u output không ?úng, hãy:
1. Check HTML structure có consistent không
2. Xác ??nh rõ ph?n nào l?p, ph?n nào static
3. Ch? ??nh tên For loop c? th? (ForDesktop, ForMobile, etc.)
4. Cung c?p JSON data m?u n?u có

---

## ?? DONE!

Save file này vào project và s? d?ng m?i khi c?n generate template m?i!

**File location:** `DOCS/PROMPT-TEMPLATE-GENERATOR.md`

---

## ?? VERSION HISTORY

| Version | Date | Changes |
|---------|------|---------|
| 1.0 | 2025-03-20 | Initial prompt template |

---

**?? Happy Template Building!**
