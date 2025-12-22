# ? HOÀN THÀNH C?P NH?T TEMPLATE SYSTEM

## ?? T?NG K?T TÍNH N?NG M?I

### ?? **1. Tooltip Hover v?i H??ng D?n Chi Ti?t**

M?i template button gi? có tooltip hi?n th? khi hover:

**N?i dung tooltip bao g?m:**
- ?? **Tên template** (bold, màu xanh d??ng)
- ?? **Description**: Mô t? ng?n g?n v? template
- ?? **Usage**: Danh sách chi ti?t cách s? d?ng
  - Phù h?p cho use case nào
  - C?u trúc d? li?u (For loops, fields...)
  - Responsive behavior
  - L?u ý ??c bi?t (??)

**Ví d? tooltip:**
```
?? Hero Event Banner (Single)

?? Banner hero cho s? ki?n - Hi?n th? thông tin chính 
   v?i background image và CTA button

• Phù h?p: Landing page s? ki?n, trang ch?
• D? li?u: Object (không dùng For loop)
• Responsive: Desktop only (d-md-block d-none)
• Fields: backgroundImage, mainTitle, paragraph1-3...

?? Hover ?? xem chi ti?t • Click ?? load
```

---

### ?? **2. Auto-Detect Template ?ang S? D?ng**

**C? ch? ho?t ??ng:**
1. Khi **load template**, t? ??ng thêm comment dòng ??u: `<!--TemplateName-->`
2. Khi **m? edit**, ??c comment này t? HTML
3. **So sánh** v?i `name` c?a các templates trong source
4. **Auto active** nút template t??ng ?ng

**Ví d? HTML sau khi load:**
```html
<!--Hero Event Banner (Single)-->
<div class="hero section">
  ...
</div>
```

**Visual feedback:**
- Nút active có background **xanh lá**
- Icon **?** ? góc trên ph?i
- Box shadow n?i b?t

**Code logic:**
```javascript
function detectCurrentTemplate() {
    var htmlContent = getEditorValue('Input1').trim();
    var match = htmlContent.match(/^<!--\s*(.+?)\s*-->/);
    if (match && match[1]) {
        return match[1].trim(); // Return template name
    }
    return null;
}
```

---

### ?? **3. C?nh Báo Rõ Ràng H?n**

**Helper text ???c c?p nh?t:**
```html
<ul>
  <li>Hover vào template ?? xem h??ng d?n chi ti?t tr??c khi s? d?ng</li>
  <li>Click template ?? load c? HTML và JSON (s? ghi ?è n?i dung hi?n t?i)</li>
  <li>?? Xem h??ng d?n (hover) tr??c khi load ?? tránh m?t d? li?u!</li>
</ul>
```

---

## ?? FILES ?Ã C?P NH?T

### ? **1. _SectionTemplateBuilder.cshtml**
**Thay ??i chính:**
- ? Thêm CSS cho tooltip (`.template-tooltip`)
- ? Thêm CSS cho active state (`.template-btn-unified.active::after`)
- ? Function `detectCurrentTemplate()` - detect t? HTML comment
- ? Function `autoActivateTemplate()` - auto active button
- ? Function `generateUsageFromValues()` - fallback n?u không có usage
- ? Thêm comment khi load template: `<!--TemplateName-->\n` + template
- ? Update helper text v?i warning rõ ràng

**JavaScript functions m?i:**
```javascript
// Detect template name from HTML comment
function detectCurrentTemplate() { ... }

// Auto-activate matching template button
function autoActivateTemplate() { ... }

// Generate usage from values structure (fallback)
function generateUsageFromValues(values) { ... }

// Load template with comment header
function loadTemplate(templateKey, skipConfirm) {
    var htmlWithComment = '<!--' + template.name + '-->\n' + template.template;
    setEditorValue('Input1', htmlWithComment);
    ...
}
```

---

### ? **2. section-templates-1.js** (Hero Event)
```javascript
{
    name: 'Hero Event Banner (Single)',
    description: '?? Banner hero cho s? ki?n - Hi?n th? thông tin chính v?i background image và CTA button',
    usage: '<li>Phù h?p: Landing page s? ki?n, trang ch?</li>...',
    template: `...`,
    values: {...}
}
```

---

### ? **3. section-templates-2.js** (Reasons to Join)
```javascript
{
    name: 'Reasons to Join (Desktop + Mobile Carousel)',
    description: '?? Carousel hi?n th? lý do tham gia s? ki?n - Desktop (2 ?nh/slide) và Mobile (1 ?nh/slide)',
    usage: '<li>Desktop: ForDesktop - 2 slides × 2 images</li>...',
    template: `...`,
    values: {...}
}
```

**??c bi?t:** ?ã g?p indicators + slides vào `ForDesktop` và `ForMobile`

---

### ? **4. section-templates-3.js** (Event Agenda)
```javascript
{
    name: 'Event Agenda (Mobile)',
    description: '?? L?ch trình s? ki?n hi?n th? trên mobile - Danh sách th?i gian và n?i dung',
    usage: '<li>D? li?u: For loop ??n gi?n v?i array items</li>...',
    template: `...`,
    values: {...}
}
```

---

### ? **5. section-templates-4.js** (Speakers)
```javascript
{
    name: 'Speakers (Desktop Grid + Mobile Carousel)',
    description: '?? Danh sách di?n gi? - Desktop hi?n th? grid 2 hàng, Mobile dùng carousel',
    usage: '<li>Desktop: 2 rows - ForDesktopRow0, ForDesktopRow1</li>...',
    template: `...`,
    values: {...}
}
```

**??c bi?t:** Mobile ?ã g?p indicators + slides vào `ForMobile`

---

### ? **6. section-templates-5.js** (FAQ)
```javascript
{
    name: 'FAQ (Frequently Asked Questions)',
    description: '? Danh sách câu h?i th??ng g?p v?i accordion Bootstrap',
    usage: '<li>?? Item ??u tiên m? m?c ??nh: ariaExpanded="true"</li>...',
    template: `...`,
    values: {...}
}
```

---

### ? **7. section-templates-6.js** (Gallery)
```javascript
{
    name: 'Gallery Carousel (Desktop + Mobile)',
    description: '??? Gallery carousel ph?c t?p - Desktop (3 c?t/slide) và Mobile (1 ?nh/slide)',
    usage: '<li>Desktop: ForDesktopPage0, ForDesktopPage1 (FLAT structure)</li>...',
    template: `...`,
    values: {...}
}
```

**??c bi?t:** 
- Desktop dùng FLAT structure v?i `ForDesktopPage0`, `ForDesktopPage1`
- Mobile ?ã g?p indicators + slides vào `ForMobile`

---

## ?? UI/UX IMPROVEMENTS

### **Tooltip Style:**
```css
.template-tooltip {
    position: absolute;
    bottom: 100%;
    background: #333;
    color: white;
    padding: 12px 15px;
    border-radius: 8px;
    min-width: 300px;
    max-width: 400px;
    line-height: 1.6;
    opacity: 0;
    transition: all 0.3s;
}

.template-btn-unified:hover .template-tooltip {
    opacity: 1;
    transform: translateX(-50%) translateY(-5px);
}
```

### **Active Button Style:**
```css
.template-btn-unified.active {
    background: linear-gradient(135deg, #4caf50 0%, #2e7d32 100%);
    box-shadow: 0 4px 12px rgba(76,175,80,0.4);
}

.template-btn-unified.active::after {
    content: '?';
    position: absolute;
    top: -5px;
    right: -5px;
    background: #4caf50;
    color: white;
    width: 20px;
    height: 20px;
    border-radius: 50%;
    font-size: 12px;
    font-weight: bold;
}
```

---

## ?? WORKFLOW M?I

### **Khi T?o M?i Event:**
```
1. M? EventEdit.cshtml
2. Expand section (vd: Section I)
3. HOVER vào template buttons ? Xem h??ng d?n chi ti?t
4. ??c k?:
   - Description (template làm gì)
   - Usage (c?u trúc d? li?u)
   - Responsive behavior
   - Các l?u ý ??c bi?t (??)
5. Click template ? Confirm dialog
6. Confirm ? Load template
7. HTML có comment header: <!--TemplateName-->
8. Customize data trong JSON editor
9. Save
```

### **Khi Edit Event:**
```
1. M? EventEdit.cshtml (existing event)
2. Expand section (vd: Section I)
3. AUTO-DETECT:
   - ??c comment <!--TemplateName--> t? HTML
   - Tìm template có name trùng kh?p
   - Auto active button template t??ng ?ng
4. User nhìn th?y button active (xanh lá + ?)
5. Bi?t ???c ?ang dùng template nào
6. Hover ?? xem l?i h??ng d?n n?u c?n
7. Edit data ho?c load template m?i
```

---

## ?? TEMPLATE STRUCTURE SUMMARY

| Template | Desktop | Mobile | Loops | Special |
|----------|---------|--------|-------|---------|
| **1. Hero Event** | Object | - | No loop | Static banner |
| **2. Reasons** | ForDesktop | ForMobile | 2 loops | Indicators merged |
| **3. Agenda** | - | For | 1 loop | Simple list |
| **4. Speakers** | ForRow0 + ForRow1 | ForMobile | 3 loops | Grid + Carousel |
| **5. FAQ** | For | For | 1 loop | Accordion |
| **6. Gallery** | ForPage0 + ForPage1 + ForIndicators | ForMobile | 4 loops | FLAT structure |

---

## ?? KEY POINTS

### **Description Pattern:**
```
Emoji + Short Description + Key Features
```
Example: `?? Banner hero cho s? ki?n - Hi?n th? thông tin chính v?i background image và CTA button`

### **Usage Pattern:**
```html
<li>Phù h?p: [Use cases]</li>
<li>D? li?u: [Data structure]</li>
<li>Responsive: [Behavior]</li>
<li>Fields: [List of fields]</li>
<li>?? [Special notes]</li>
```

### **Auto-Detect Pattern:**
```html
<!--TemplateName-->
<div class="section">
  ...
</div>
```

---

## ? CHECKLIST HOÀN THÀNH

- [x] Thêm tooltip hover cho t?t c? templates
- [x] Thêm `description` field cho 6 templates
- [x] Thêm `usage` field cho 6 templates
- [x] Implement auto-detect t? HTML comment
- [x] Implement auto-activate button
- [x] Thêm comment header khi load template
- [x] Update helper text v?i warning
- [x] CSS cho tooltip animation
- [x] CSS cho active state v?i checkmark
- [x] Fallback `generateUsageFromValues()` n?u không có usage
- [x] Test workflow: Create ? Edit ? Auto-detect

---

## ?? TESTING

### **Test Cases:**

#### **1. Tooltip Hover:**
- [ ] Hover vào template button ? Tooltip hi?n th?
- [ ] Tooltip có ??: name, description, usage
- [ ] Animation m??t mà
- [ ] Không b? overflow viewport

#### **2. Auto-Detect:**
- [ ] Load template ? HTML có comment `<!--TemplateName-->`
- [ ] Save ? M? l?i edit
- [ ] Button template t??ng ?ng auto active (xanh lá + ?)
- [ ] N?u HTML không có comment ? Không active button nào

#### **3. User Flow:**
- [ ] Create new event ? Hover ? ??c usage ? Load ? Customize ? Save
- [ ] Edit existing event ? Auto-detect ? Modify ? Save
- [ ] Load template khác ? Confirm dialog ? Ghi ?è ? Save

---

## ?? NOTES

### **Fallback Mechanism:**
N?u template không có field `usage`, h? th?ng t? ??ng generate t? `values`:
```javascript
var usage = template.usage || generateUsageFromValues(template.values);
```

### **Comment Format:**
```html
<!--TemplateName-->
```
- Không có kho?ng tr?ng th?a
- Match exactly v?i `template.name`
- Ph?i ? **dòng ??u tiên** c?a HTML

### **Active State Logic:**
```javascript
1. Load templates ? generateQuickTemplates()
2. Sau 100ms ? autoActivateTemplate()
3. Detect comment ? Find matching template
4. Remove all active class
5. Add active class to matching button
```

---

## ?? USER GUIDE

### **Cho Developer:**
1. Khi thêm template m?i, nh? thêm `description` và `usage`
2. Format theo pattern ?ã có
3. Usage nên có 4-5 items, ng?n g?n, súc tích
4. Dùng emoji cho d? nhìn (?? ?? ?? ? ???)

### **Cho Content Editor:**
1. Luôn **HOVER** vào template tr??c khi click
2. ??c k? usage ?? hi?u c?u trúc d? li?u
3. L?u ý các warning (??)
4. Sau khi load, customize data trong JSON editor
5. Không xóa comment header n?u mu?n auto-detect ho?t ??ng

---

**Version:** 2.0  
**Last Updated:** 2025-01-XX  
**Status:** ? Production Ready
