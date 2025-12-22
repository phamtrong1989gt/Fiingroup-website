# ?? TEMPLATE GENERATION PROMPT

## ?? M?C ?ÍCH
File này ch?a prompt chu?n ?? t? ??ng generate JavaScript template t? HTML input theo chu?n Fiingroup CMS.

---

## ?? PROMPT CHÍNH - PHÂN TÍCH & GENERATE

```
B?n là chuyên gia thi?t k? template cho Fiingroup CMS. Hãy phân tích HTML và t?o JavaScript template file.

## INPUT HTML:
```html
[PASTE YOUR HTML HERE]
```

## YÊU C?U PHÂN TÍCH:

### B??c 1: Phân tích c?u trúc HTML
Xác ??nh:
- [ ] Lo?i template: Static / Dynamic List / Carousel / Multi-level
- [ ] Các ph?n t? l?p l?i (c?n For loop)
- [ ] Các ph?n t? ??c l?p (không c?n For)
- [ ] Carousel có indicators không?
- [ ] Có c?u trúc nested không?

### B??c 2: Trích xu?t tokens
Li?t kê t?t c?:
- Global fields (ngoài loop): [field1, field2, ...]
- Loop fields (trong For): [field1, field2, ...]
- Attributes c?n dynamic: [class, src, href, ...]

### B??c 3: Xác ??nh data structure
```javascript
values: {
    // Global settings
    "globalField1": "value",
    
    // Loop data (n?u có)
    "For": [
        {
            "index": 0,
            "field1": "value1"
        }
    ]
}
```

## QUY T?C B?T BU?C:

### ? PH?I TUÂN TH?:
1. **Index Convention:**
   - `index`: B?t ??u t? 0 (backend processing)
   - `slideNumber`: B?t ??u t? 1 (user display)

2. **Carousel Rules:**
   - G?P indicators và slides vào 1 loop duy nh?t
   - Không t?o ForIndicators riêng và ForSlides riêng
   - Fields b?t bu?c: index, activeClass, ariaCurrent, slideNumber

3. **Nested Structure:**
   - KHÔNG dùng nested For loops
   - Dùng ForPageX (ForPage0, ForPage1, ...) ?? flatten
   - Hardcode carousel-item wrapper, loop ch? cho images

4. **Active State:**
   - Item ??u tiên: `"activeClass": "active"`, `"ariaCurrent": "true"`
   - Items khác: `"activeClass": ""`, `"ariaCurrent": ""`
   - ariaCurrent ph?i là string, không ph?i boolean

5. **Naming Convention:**
   - Loop name: For, ForDesktop, ForMobile, ForPage0, ForPage1
   - Field name: camelCase (imageUrl, speakerName, ...)
   - No spaces, no special chars

### ? TRÁNH:
- ? Nested For loops (For trong For)
- ? Indicators và slides riêng bi?t
- ? Index b?t ??u t? 1
- ? ariaCurrent là boolean true/false
- ? Thi?u activeClass trong items

## OUTPUT YÊU C?U:

### 1. Phân tích Structure:
```
TEMPLATE TYPE: [Static/Dynamic/Carousel/Multi-level]

GLOBAL FIELDS:
- sectionTitle: "..."
- carouselId: "..."

LOOP STRUCTURE:
- For[Name]: X items
  Fields: index, activeClass, field1, field2...

NESTED: [Yes/No]
If Yes: Use ForPage0, ForPage1...
```

### 2. Complete JavaScript File:
```javascript
// ============================================
// SECTION TEMPLATES X - [SECTION NAME]
// ============================================
(function() {
    'use strict';

    window.SectionTemplatesX = {
        '[template-key]': {
            name: '[Template Display Name]',
            description: '[Short description]',
            template: `[HTML WITH TOKENS]`,
            values: {
                // Global settings
                "field1": "value1",
                
                // Loop data
                "For": [
                    {
                        "index": 0,
                        "activeClass": "active",
                        "ariaCurrent": "true",
                        "slideNumber": 1,
                        "field1": "value1"
                    }
                ]
            }
        }
    };

    if (typeof window !== 'undefined') {
        window.SectionTemplatesX = SectionTemplatesX;
    }

})();
```

### 3. Usage Documentation:
```
## Cách s? d?ng:

1. Include file trong HTML:
   <script src="js/templates/section-templates-X.js"></script>

2. Access template:
   var template = window.SectionTemplatesX['template-key'];

3. Render v?i backend C#:
   - Backend s? Replace [token] v?i values
   - For loops ???c x? lý t? ??ng
```

## EXAMPLES BY TYPE:

### Example 1: Static Template (No Loop)
```javascript
// Hero banner, static content
values: {
    "backgroundImage": "url...",
    "mainTitle": "Title",
    "paragraph1": "Text"
}
```

### Example 2: Simple List
```javascript
// Agenda, FAQ items
values: {
    "For": [
        { "index": 0, "timeSlot": "09:00", "content": "..." },
        { "index": 1, "timeSlot": "10:00", "content": "..." }
    ]
}
```

### Example 3: Carousel (Indicators + Slides)
```javascript
// Mobile carousel - G?P CHUNG
values: {
    "ForMobile": [
        {
            // Indicator fields
            "index": 0,
            "activeClass": "active",
            "ariaCurrent": "true",
            "slideNumber": 1,
            // Slide fields
            "imageUrl": "...",
            "imageAlt": "..."
        }
    ]
}
```

### Example 4: Multi-Column Carousel
```javascript
// Desktop 3-column gallery - FLATTEN
values: {
    "ForPage0": [ // Slide 0
        { "imageUrl": "img1.jpg", "columnClass": "ps-0 pr-16" },
        { "imageUrl": "img2.jpg", "columnClass": "px-8" },
        { "imageUrl": "img3.jpg", "columnClass": "pl-16 pe-0" }
    ],
    "ForPage1": [ // Slide 1
        { "imageUrl": "img4.jpg", "columnClass": "ps-0 pr-16" },
        { "imageUrl": "img5.jpg", "columnClass": "px-8" },
        { "imageUrl": "img6.jpg", "columnClass": "pl-16 pe-0" }
    ]
}
```

## VALIDATION CHECKLIST:

Tr??c khi output, validate:
- [ ] ? Index starts from 0
- [ ] ? slideNumber starts from 1
- [ ] ? No duplicate loops (indicators merged)
- [ ] ? No nested For loops
- [ ] ? activeClass in all carousel items
- [ ] ? ariaCurrent is string "true"/""
- [ ] ? Comments clear and helpful
- [ ] ? IIFE wrapper correct
- [ ] ? window export present
- [ ] ? HTML well-formed (tags closed)

Bây gi? hãy phân tích HTML input và t?o template file hoàn ch?nh.
```

---

## ?? PROMPT PH? - VALIDATE TEMPLATE

```
Validate template JavaScript này theo chu?n Fiingroup CMS:

## TEMPLATE CODE:
```javascript
[PASTE YOUR TEMPLATE CODE HERE]
```

## VALIDATION CRITERIA:

### 1. Structure Check:
- [ ] IIFE wrapper: `(function() { 'use strict'; ... })()`
- [ ] window.SectionTemplatesX defined
- [ ] Export check: `if (typeof window !== 'undefined')`
- [ ] Header comment present

### 2. Template HTML:
- [ ] Tokens format: [fieldName]
- [ ] For loops: [For]...[/For]
- [ ] No nested For loops
- [ ] HTML tags properly closed

### 3. Data Structure:
- [ ] Index starts from 0
- [ ] slideNumber starts from 1
- [ ] activeClass in all items
- [ ] ariaCurrent string format
- [ ] No duplicate loops

### 4. Carousel Specific:
- [ ] Indicators + slides in same loop
- [ ] data-bs-slide-to uses [index]
- [ ] aria-label uses [slideNumber]
- [ ] First item has activeClass="active"

### 5. Nested Structure:
- [ ] Uses ForPageX not nested For
- [ ] Comments for Slide 0, Slide 1
- [ ] activeClass hardcoded in HTML (if needed)

## OUTPUT:

```
STATUS: ? PASS / ? FAIL

SCORE: [X/10]

ISSUES FOUND:
1. [Issue] - Line XX
   Current: [code]
   Expected: [correct code]
   Fix: [solution]

OPTIMIZATIONS:
1. [Suggestion]
2. [Suggestion]

FIXED VERSION:
```javascript
[CORRECTED CODE IF NEEDED]
```
```
```

---

## ?? PROMPT PH? - DEBUG TEMPLATE

```
Template này b? l?i khi render. Debug giúp tôi:

## TEMPLATE CODE:
```javascript
[PASTE TEMPLATE CODE]
```

## ERROR INFO:

### Expected Output:
```html
[PASTE EXPECTED HTML]
```

### Actual Output:
```html
[PASTE ACTUAL WRONG HTML]
```

### Error Message (if any):
```
[PASTE ERROR MESSAGE]
```

## BACKEND CONTEXT:
Backend C# x? lý nh? sau:
```csharp
// Extract For loop content
string _TokenFor = Functions.TrimToken(template, "[For]", "[/For]");

// Loop và replace
foreach (var item in data)
{
    result.Append(_TokenFor
        .Replace("[token1]", item.Value1)
        .Replace("[token2]", item.Value2)
    );
}
```

## DEBUG STEPS:

1. **Token Matching:**
   - Check token names match exactly (case-sensitive)
   - Check no typos in [tokenName]

2. **Loop Structure:**
   - Check For loop properly closed
   - Check no nested For loops

3. **Data Alignment:**
   - Check data structure matches template
   - Check all required fields present

4. **Active State Logic:**
   - Check first item has active
   - Check logic for activeClass/ariaCurrent

## OUTPUT:

```
ROOT CAUSE:
[Xác ??nh chính xác nguyên nhân]

DETAILED EXPLANATION:
[Gi?i thích t?i sao l?i x?y ra]

FIX:
Step 1: [Change description]
Step 2: [Change description]

CORRECTED CODE:
```javascript
[FIXED TEMPLATE CODE]
```

PREVENTION:
[Cách tránh l?i này trong t??ng lai]
```
```

---

## ?? REFERENCE - EXISTING TEMPLATES

### Template 1: Static Hero Banner
```javascript
// No For loop - Object structure
values: {
    "backgroundImage": "...",
    "mainTitle": "...",
    "paragraph1": "..."
}
```

### Template 2: Desktop + Mobile Carousel
```javascript
// 2 separate loops - MERGED indicators + slides
values: {
    "ForDesktop": [
        {
            "index": 0,
            "activeClass": "active",
            "ariaCurrent": 'aria-current="true"',
            "slideNumber": 1,
            "leftImage": "...",
            "rightImage": "..."
        }
    ],
    "ForMobile": [ ... ]
}
```

### Template 3: Simple List (Agenda)
```javascript
// Simple For loop
values: {
    "For": [
        { "index": 0, "timeSlot": "09:00", "content": "..." }
    ]
}
```

### Template 4: Grid + Carousel
```javascript
// Desktop: 2 rows (ForRow0, ForRow1)
// Mobile: 1 carousel (ForMobile with indicators)
values: {
    "ForDesktopRow0": [ ... ],
    "ForDesktopRow1": [ ... ],
    "ForMobile": [
        {
            "index": 0,
            "activeClass": "active",
            "ariaCurrent": 'aria-current="true"',
            "slideNumber": 1,
            "speakerImage": "...",
            "speakerName": "..."
        }
    ]
}
```

### Template 5: Accordion (FAQ)
```javascript
// Simple For loop with collapse IDs
values: {
    "For": [
        {
            "index": 0,
            "question": "...",
            "answer": "...",
            "collapseId": "collapseOne",
            "ariaExpanded": "true",
            "showClass": "show"
        }
    ]
}
```

### Template 6: Multi-Level Gallery
```javascript
// Desktop: FLATTENED with ForPageX
// Mobile: MERGED indicators + slides
values: {
    "ForDesktopIndicators": [
        { "index": 0, "activeClass": "active" }
    ],
    "ForDesktopPage0": [ // 3 images
        { "imageUrl": "...", "columnClass": "..." }
    ],
    "ForDesktopPage1": [ // 3 images
        { "imageUrl": "...", "columnClass": "..." }
    ],
    "ForMobile": [
        {
            "index": 0,
            "activeClass": "active",
            "ariaCurrent": "true",
            "slideNumber": 1,
            "imageUrl": "...",
            "imageAlt": "..."
        }
    ]
}
```

---

## ?? DECISION TREE

```
HTML Input
    ?
    ?? Có ph?n t? l?p?
    ?   ?
    ?   ?? KHÔNG ? Static Template
    ?   ?          Use: Object structure
    ?   ?          Example: Template 1
    ?   ?
    ?   ?? CÓ
    ?       ?
    ?       ?? Có carousel indicators?
    ?       ?   ?
    ?       ?   ?? CÓ ? G?P CHUNG 1 loop
    ?       ?   ?       Fields: index, activeClass, ariaCurrent, slideNumber, ...slideData
    ?       ?   ?       Example: Template 2, 4, 6 (mobile)
    ?       ?   ?
    ?       ?   ?? KHÔNG ? Simple For loop
    ?       ?           Example: Template 3, 5
    ?       ?
    ?       ?? Có nested structure? (nhi?u items/slide)
    ?           ?
    ?           ?? CÓ ? FLATTEN v?i ForPageX
    ?           ?       Example: Template 6 (desktop)
    ?           ?
    ?           ?? KHÔNG ? Simple For loop
    ?                   Example: Template 3, 5
```

---

## ?? TIPS & BEST PRACTICES

### 1. Naming Conventions
```
? GOOD:
- ForDesktop, ForMobile
- ForPage0, ForPage1
- imageUrl, speakerName
- carouselId, sectionTitle

? BAD:
- for, forloop
- Page1, Page2 (không có For prefix)
- image_url, speaker-name
- carousel_id, section-title
```

### 2. Active State Pattern
```javascript
// First item (index 0)
{
    "index": 0,
    "activeClass": "active",
    "ariaCurrent": "true",  // STRING not boolean
    "slideNumber": 1        // Display starts from 1
}

// Other items (index 1+)
{
    "index": 1,
    "activeClass": "",      // Empty string
    "ariaCurrent": "",      // Empty string
    "slideNumber": 2
}
```

### 3. Column Class Pattern (3-column layout)
```javascript
[
    { "columnClass": "ps-0 pr-16" },  // Left
    { "columnClass": "px-8" },        // Middle
    { "columnClass": "pl-16 pe-0" }   // Right
]
```

### 4. Collapse ID Pattern (Accordion)
```javascript
{
    "collapseId": "collapseOne",
    "ariaExpanded": "true",
    "collapsedClass": "",
    "showClass": "show"
}
```

---

## ? QUICK REFERENCE

| Use Case | Template Type | Loop Structure | Example |
|----------|---------------|----------------|---------|
| Hero Banner | Static | No loop | Template 1 |
| Agenda List | Simple List | For | Template 3 |
| FAQ | Accordion | For | Template 5 |
| Mobile Carousel | Carousel | ForMobile (merged) | Template 2, 4, 6 |
| Desktop Carousel | Carousel | ForDesktop (merged) | Template 2 |
| Multi-column Gallery | Multi-level | ForPageX (flat) | Template 6 |
| Speaker Grid | Grid + Carousel | ForRowX + ForMobile | Template 4 |

---

## ?? SUPPORT

N?u g?p v?n ??:
1. Ki?m tra l?i VALIDATION CHECKLIST
2. So sánh v?i REFERENCE TEMPLATES
3. Ch?y DEBUG PROMPT
4. Tham kh?o DECISION TREE

---

**Version:** 1.0  
**Last Updated:** 2025-01-XX  
**Maintainer:** Fiingroup CMS Team
