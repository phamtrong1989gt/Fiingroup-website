# H??NG D?N H? TH?NG TEMPLATE MODULAR

## ?? T?NG QUAN

H? th?ng m?i cho phép:
? **Chia templates thành nhi?u file** - D? qu?n lý, d? maintain
? **T? ??ng generate template buttons** - D?a vào templates có s?n
? **Context menu thông minh** - Ch?n load: C? hai / Ch? Template / Ch? Values
? **Responsive columns** - T? ??ng ?i?u ch?nh theo s? l??ng templates
? **Icon mapping** - Icon ??p cho t?ng lo?i template

---

## ??? C?U TRÚC FILE

```
1.Backend/wwwroot/js/
??? section-template-engine.js          # Engine chính
??? templates/
    ??? section-templates-1.js          # 4 templates: Slider, Carousel, Hero
    ??? section-templates-2.js          # 3 templates: Cards, Grid, Pricing
    ??? section-templates-3.js          # 3 templates: List, Timeline, Testimonials
    ??? section-templates-4.js          # 1 template: Team (Single template example)
    ??? section-templates-5.js          # 2 templates: FAQ, Contact (Dual templates example)
    ??? section-templates-X.js          # Thêm file m?i
```

---

## ?? TÍNH N?NG M?I

### 1. **Auto-Generate Template Buttons**

H? th?ng t? ??ng:
- ??c t?t c? templates t? `templateSource`
- Generate buttons v?i s? c?t phù h?p:
  - **1 template** ? 1 c?t (col-md-12)
  - **2 templates** ? 2 c?t (col-md-6)
  - **3 templates** ? 3 c?t (col-md-4)
  - **4+ templates** ? 4 c?t (col-md-3)

```javascript
// Ví d?: SectionTemplates4 ch? có 1 template
window.SectionTemplates4 = {
    'team': { ... }  // S? hi?n th? 1 button full width
};

// Ví d?: SectionTemplates5 có 2 templates
window.SectionTemplates5 = {
    'faq': { ... },
    'contact': { ... }  // S? hi?n th? 2 buttons, m?i cái 50% width
};
```

### 2. **Context Menu - Load Options**

**Cách s? d?ng:**
- **Click trái** ? Load c? Template & Values (default)
- **Click ph?i** ? Hi?n menu ch?n:
  - ? Load c? Template & Values
  - ?? Ch? load Template HTML
  - ?? Ch? load Values JSON

**Khi nào dùng?**
- Load c? hai: Khi b?t ??u m?i hoàn toàn
- Ch? Template: Khi mu?n gi? values hi?n t?i, ??i template
- Ch? Values: Khi mu?n gi? template hi?n t?i, ??i data

### 3. **Icon Mapping**

Icons t? ??ng d?a vào tên template:

| Template Key | Icon | Color |
|--------------|------|-------|
| `slider` | slideshow | Blue |
| `carousel` | view_carousel | Purple |
| `hero` | panorama | Pink |
| `cards` | view_module | Orange |
| `grid` | grid_on | Red |
| `pricing` | attach_money | Green |
| `list` | list | Green |
| `timeline` | timeline | Cyan |
| `testimonials` | chat_bubble | Purple |
| `team` | people | Grey |
| `portfolio` | work | Brown |
| `gallery` | collections | Pink |
| `video` | play_circle_outline | Red |
| `custom` | code | Grey |

---

## ?? CÁCH T?O FILE TEMPLATE M?I

### B??c 1: T?o file JavaScript m?i

```javascript
// File: 1.Backend/wwwroot/js/templates/section-templates-6.js

(function() {
    'use strict';

    // Có th? có 1, 2, 3 ho?c nhi?u templates
    window.SectionTemplates6 = {
        'features': {
            name: 'Feature List',
            template: `[For]
<div class="feature">
    <i class="[icon]"></i>
    <h4>[title]</h4>
    <p>[description]</p>
</div>
[/For]`,
            values: [
                {
                    icon: "fas fa-rocket",
                    title: "Fast Performance",
                    description: "Lightning fast loading speed"
                }
            ]
        }
        // Có th? thêm nhi?u templates khác ? ?ây
    };

})();
```

### B??c 2: Load file trong EventEdit.cshtml

```razor
<script src="~/js/templates/section-templates-6.js"></script>
```

### B??c 3: S? d?ng trong Partial View

```razor
<partial name="_SectionTemplateBuilder" model="@(new PT.Domain.Model.SectionTemplateBuilderModel 
{ 
    SectionNumber = 6,
    Title = "Features Section",
    TemplateSource = "SectionTemplates6",  // ? T? ??ng generate buttons
    InputTemplate = nameof(Model.Input11),
    InputValue = nameof(Model.Input12),
    TemplateValue = Model.Input11,
    ValueJson = Model.Input12
})" />
```

**MAGIC:** Template buttons s? t? ??ng xu?t hi?n! ??

---

## ?? VÍ D? TH?C T?

### Ví d? 1: Section v?i 1 Template (Full Width)

```razor
<partial name="_SectionTemplateBuilder" model="@(new PT.Domain.Model.SectionTemplateBuilderModel 
{ 
    SectionNumber = 4,
    Title = "Team Section",
    Icon = "people",
    TemplateSource = "SectionTemplates4",  // Ch? có 1 template: team
    InputTemplate = nameof(Model.Input7),
    InputValue = nameof(Model.Input8),
    TemplateValue = Model.Input7,
    ValueJson = Model.Input8
})" />
```

**K?t qu?:** 1 button full width v?i icon "people"

### Ví d? 2: Section v?i 2 Templates (50% Width)

```razor
<partial name="_SectionTemplateBuilder" model="@(new PT.Domain.Model.SectionTemplateBuilderModel 
{ 
    SectionNumber = 5,
    Title = "FAQ & Contact",
    Icon = "contact_support",
    TemplateSource = "SectionTemplates5",  // 2 templates: faq, contact
    InputTemplate = nameof(Model.Input9),
    InputValue = nameof(Model.Input10),
    TemplateValue = Model.Input9,
    ValueJson = Model.Input10
})" />
```

**K?t qu?:** 2 buttons, m?i cái chi?m 50% width

### Ví d? 3: Section v?i 3 Templates (33% Width)

```razor
<partial name="_SectionTemplateBuilder" model="@(new PT.Domain.Model.SectionTemplateBuilderModel 
{ 
    SectionNumber = 3,
    Title = "Content Sections",
    TemplateSource = "SectionTemplates3",  // 3 templates: list, timeline, testimonials
    InputTemplate = nameof(Model.Input5),
    InputValue = nameof(Model.Input6),
    TemplateValue = Model.Input5,
    ValueJson = Model.Input6
})" />
```

**K?t qu?:** 3 buttons, m?i cái chi?m 33% width

---

## ?? CUSTOM ICON MAPPING

N?u mu?n thêm icon cho template key m?i:

```javascript
// Trong _SectionTemplateBuilder.cshtml, thêm vào iconMapping:
var iconMapping = {
    // ... existing ...
    'myNewTemplate': { icon: 'rocket_launch', color: '#ff5722' }
};
```

Ho?c template s? dùng icon m?c ??nh: `extension` màu grey

---

## ?? TIPS & BEST PRACTICES

### 1. T? ch?c Templates theo s? l??ng

```
section-templates-1.js  ? 4 templates (Multi-purpose)
section-templates-4.js  ? 1 template  (Specific: Team only)
section-templates-5.js  ? 2 templates (Related: FAQ & Contact)
```

### 2. ??t tên Template Key có ý ngh?a

```javascript
? Good: 'heroSlider', 'pricingCards', 'teamMembers'
? Bad: 'template1', 'temp2', 'abc'
```

### 3. S? d?ng Context Menu hi?u qu?

**Scenario 1:** Mu?n th? các template khác nhau v?i cùng data
? Click ph?i ? "Ch? load Template HTML"

**Scenario 2:** Mu?n th? data khác nhau v?i cùng template
? Click ph?i ? "Ch? load Values JSON"

**Scenario 3:** B?t ??u m?i hoàn toàn
? Click trái (ho?c click ph?i ? "Load c? Template & Values")

### 4. Template Name nên ng?n g?n

```javascript
'slider': {
    name: 'Slider',  // ? Ng?n g?n, v?a v?n
    // NOT: name: 'Awesome Super Responsive Image Slider'  // ? Quá dài
}
```

---

## ?? TROUBLESHOOTING

### L?i: Không có template buttons nào hi?n th?

**Nguyên nhân:** Template source không có template ho?c ch?a load
**Gi?i pháp:**
```javascript
// Check trong browser console:
console.log(window.SectionTemplates4);
// ? Should show: { team: {...} }
```

### L?i: Template buttons b? ch?ng lên nhau

**Nguyên nhân:** Quá nhi?u templates (>12)
**Gi?i pháp:** Chia thành nhi?u file template sources

### L?i: Context menu không xu?t hi?n

**Nguyên nhân:** jQuery ch?a load ho?c browser không h? tr?
**Gi?i pháp:** Check jQuery và dùng click trái thay th?

---

## ?? SO SÁNH TR??C/SAU

### TR??C (Cách c?):

```razor
<!-- Ph?i hard-code 4 buttons, dù ch? có 1 template -->
<div class="col-md-3">
    <div class="template-selector-card" data-template="slider">...</div>
</div>
<div class="col-md-3">
    <div class="template-selector-card" data-template="cards">...</div>
</div>
<div class="col-md-3">
    <div class="template-selector-card" data-template="list">...</div>
</div>
<div class="col-md-3">
    <div class="template-selector-card" data-template="custom">...</div>
</div>
```

? C?ng nh?c
? Lãng phí space
? Hi?n th? buttons không có template

### SAU (Cách m?i):

```razor
<!-- T? ??ng generate d?a vào templates có s?n -->
<div id="quickTemplatesContainer_@Model.SectionNumber">
    <!-- Auto-generated! -->
</div>
```

? Linh ho?t
? T?i ?u space
? Ch? hi?n th? templates th?t s? có

---

## ?? WORKFLOW HOÀN CH?NH

### T?o Section m?i v?i Custom Templates:

1. **T?o template file** v?i s? l??ng templates tùy ý:
   ```javascript
   window.SectionTemplatesX = {
       'template1': { ... },  // 1 template ? Full width
       'template2': { ... }   // 2 templates ? 50% width each
   };
   ```

2. **Load file**:
   ```razor
   <script src="~/js/templates/section-templates-X.js"></script>
   ```

3. **Khai báo section**:
   ```razor
   <partial name="_SectionTemplateBuilder" model="@(...)" />
   ```

4. **DONE!** 
   - Buttons t? ??ng generate
   - Columns t? ??ng responsive
   - Context menu ho?t ??ng ngay

---

## ?? T?NG K?T

### Tính n?ng m?i:

? **Auto-generate buttons** - Không c?n hard-code
? **Responsive columns** - T? ??ng ?i?u ch?nh layout
? **Context menu** - Load riêng template ho?c values
? **Icon mapping** - Icons ??p t? ??ng
? **Flexible** - 1 template c?ng OK, nhi?u templates c?ng OK

### L?i ích:

| Tính n?ng | Tr??c | Sau |
|-----------|-------|-----|
| S? buttons | 4 c? ??nh | T? ??ng theo templates |
| Layout | 4 c?t c? ??nh | Responsive (1-4 c?t) |
| Load options | C? hai | C? hai / Template / Values |
| Maintain | S?a HTML | Không c?n s?a gì |

**YOUR TEMPLATE SYSTEM IS NOW SUPER INTELLIGENT!** ??
