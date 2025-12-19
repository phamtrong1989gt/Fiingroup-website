# ?? QUICK PROMPT - Section Template Generator

## ?? Copy & Paste This:

```
Hãy phân tích HTML này và generate ra file section-templates-X.js:

**HTML INPUT:**
```html
[PASTE HTML HERE]
```

**YÊU C?U:**
1. Tìm pattern l?p ? Dùng [For] loops
2. Tìm giá tr? ??ng ? Dùng [fieldName]
3. T?o JSON data v?i 2-3 examples
4. ??t tên template theo n?i dung (camelCase)
5. Include description và comments

**OUTPUT FORMAT:**
```javascript
const SectionTemplatesX = {
    templateName: {
        name: "Display Name",
        description: "What this template does",
        template: `HTML with [placeholders] and [For] loops`,
        values: {
            // JSON data structure
        }
    }
};
```

**File location:** `1.Backend/wwwroot/js/templates/section-templates-X.js`
```

---

## ?? Examples:

### Example 1: Simple
```html
<div class="item">Item 1</div>
<div class="item">Item 2</div>
```

? Output:
```javascript
template: `[For]<div class="item">[name]</div>[/For]`,
values: { For: [{"name":"Item 1"}, {"name":"Item 2"}] }
```

---

### Example 2: Carousel
```html
<div class="carousel">
    <button class="active"></button>
    <button></button>
    <div class="slide active">Slide 1</div>
    <div class="slide">Slide 2</div>
</div>
```

? Output:
```javascript
template: `<div class="carousel">
    [ForIndicator]<button class="[activeClass]"></button>[/ForIndicator]
    [For]<div class="slide [activeClass]">[content]</div>[/For]
</div>`,
values: {
    ForIndicator: [
        {activeClass: "active"},
        {activeClass: ""}
    ],
    For: [
        {activeClass: "active", content: "Slide 1"},
        {activeClass: "", content: "Slide 2"}
    ]
}
```

---

## ?? Template Names:

| Type | Name |
|------|------|
| Slider | `heroSlider`, `carouselBasic` |
| Grid | `reasonsGrid`, `cardsGrid` |
| Timeline | `agendaTimeline`, `eventSchedule` |
| List | `speakersList`, `faqList` |
| Accordion | `faqAccordion`, `collapsible` |

---

## ? Checklist:

- [ ] Template name is camelCase
- [ ] Description explains purpose
- [ ] [For] loops for repeating items
- [ ] [fieldName] for dynamic values
- [ ] JSON has 2-3 realistic examples
- [ ] activeClass handled correctly
- [ ] URLs are realistic (cdn.fiingroup.vn)

---

**?? That's it! Just paste HTML and get JavaScript template!**
