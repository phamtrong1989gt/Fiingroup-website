# ?? TEMPLATE EXAMPLES & USE CASES

## Example 1: Convert Static Hero Banner

### INPUT HTML:
```html
<div class="hero" style="background-image: url('hero.jpg')">
    <h1>Welcome to Event 2025</h1>
    <p>Join us for an amazing experience</p>
    <a href="/register">Register Now</a>
</div>
```

### ANALYSIS:
- Type: Static (no repeating elements)
- Tokens: backgroundImage, title, description, ctaUrl, ctaText
- Loop: None

### OUTPUT:
```javascript
// ============================================
// SECTION TEMPLATES 7 - Hero Simple
// ============================================
(function() {
    'use strict';

    window.SectionTemplates7 = {
        'hero-simple': {
            name: 'Hero Banner (Simple)',
            template: `
<div class="hero" style="background-image: url([backgroundImage])">
    <h1>[title]</h1>
    <p>[description]</p>
    <a href="[ctaUrl]">[ctaText]</a>
</div>`,
            values: {
                "backgroundImage": "hero.jpg",
                "title": "Welcome to Event 2025",
                "description": "Join us for an amazing experience",
                "ctaUrl": "/register",
                "ctaText": "Register Now"
            }
        }
    };

    if (typeof window !== 'undefined') {
        window.SectionTemplates7 = SectionTemplates7;
    }

})();
```

---

## Example 2: Convert Simple Agenda List

### INPUT HTML:
```html
<div class="agenda">
    <h2>Event Agenda</h2>
    <div class="item">
        <span class="time">09:00 - 10:00</span>
        <span class="topic">Opening Ceremony</span>
    </div>
    <div class="item">
        <span class="time">10:00 - 11:00</span>
        <span class="topic">Keynote Speech</span>
    </div>
    <div class="item">
        <span class="time">11:00 - 12:00</span>
        <span class="topic">Panel Discussion</span>
    </div>
</div>
```

### ANALYSIS:
- Type: Simple List
- Repeating: .item (3 times)
- Tokens: sectionTitle, timeSlot, topic
- Loop: For (3 items)

### OUTPUT:
```javascript
// ============================================
// SECTION TEMPLATES 8 - Agenda List
// ============================================
(function() {
    'use strict';

    window.SectionTemplates8 = {
        'agenda-list': {
            name: 'Agenda List',
            template: `
<div class="agenda">
    <h2>[sectionTitle]</h2>
    [For]
    <div class="item">
        <span class="time">[timeSlot]</span>
        <span class="topic">[topic]</span>
    </div>
    [/For]
</div>`,
            values: {
                "sectionTitle": "Event Agenda",
                "For": [
                    {
                        "index": 0,
                        "timeSlot": "09:00 - 10:00",
                        "topic": "Opening Ceremony"
                    },
                    {
                        "index": 1,
                        "timeSlot": "10:00 - 11:00",
                        "topic": "Keynote Speech"
                    },
                    {
                        "index": 2,
                        "timeSlot": "11:00 - 12:00",
                        "topic": "Panel Discussion"
                    }
                ]
            }
        }
    };

    if (typeof window !== 'undefined') {
        window.SectionTemplates8 = SectionTemplates8;
    }

})();
```

---

## Example 3: Convert Mobile Image Carousel

### INPUT HTML:
```html
<div id="carousel-mobile" class="carousel slide">
    <div class="carousel-indicators">
        <button data-bs-slide-to="0" class="active" aria-current="true" aria-label="Slide 1"></button>
        <button data-bs-slide-to="1" aria-label="Slide 2"></button>
        <button data-bs-slide-to="2" aria-label="Slide 3"></button>
    </div>
    <div class="carousel-inner">
        <div class="carousel-item active">
            <img src="image1.jpg" alt="Image 1">
        </div>
        <div class="carousel-item">
            <img src="image2.jpg" alt="Image 2">
        </div>
        <div class="carousel-item">
            <img src="image3.jpg" alt="Image 3">
        </div>
    </div>
</div>
```

### ANALYSIS:
- Type: Carousel with indicators
- Indicators: 3 buttons
- Slides: 3 images
- **KEY**: G?p indicators + slides vào 1 loop!

### OUTPUT:
```javascript
// ============================================
// SECTION TEMPLATES 9 - Mobile Gallery
// ============================================
(function() {
    'use strict';

    window.SectionTemplates9 = {
        'mobile-gallery': {
            name: 'Mobile Image Carousel',
            template: `
<div id="[carouselId]" class="carousel slide">
    <div class="carousel-indicators">
        [ForMobile]
        <button data-bs-slide-to="[index]" class="[activeClass]" aria-current="[ariaCurrent]" aria-label="Slide [slideNumber]"></button>
        [/ForMobile]
    </div>
    <div class="carousel-inner">
        [ForMobile]
        <div class="carousel-item [activeClass]">
            <img src="[imageUrl]" alt="[imageAlt]">
        </div>
        [/ForMobile]
    </div>
</div>`,
            values: {
                "carouselId": "carousel-mobile",
                "ForMobile": [
                    {
                        "index": 0,
                        "activeClass": "active",
                        "ariaCurrent": "true",
                        "slideNumber": 1,
                        "imageUrl": "image1.jpg",
                        "imageAlt": "Image 1"
                    },
                    {
                        "index": 1,
                        "activeClass": "",
                        "ariaCurrent": "",
                        "slideNumber": 2,
                        "imageUrl": "image2.jpg",
                        "imageAlt": "Image 2"
                    },
                    {
                        "index": 2,
                        "activeClass": "",
                        "ariaCurrent": "",
                        "slideNumber": 3,
                        "imageUrl": "image3.jpg",
                        "imageAlt": "Image 3"
                    }
                ]
            }
        }
    };

    if (typeof window !== 'undefined') {
        window.SectionTemplates9 = SectionTemplates9;
    }

})();
```

---

## Example 4: Convert Desktop 3-Column Gallery

### INPUT HTML:
```html
<div class="carousel slide">
    <!-- Slide 1: 3 images -->
    <div class="carousel-item active">
        <div class="row">
            <div class="col-4"><img src="img1.jpg"></div>
            <div class="col-4"><img src="img2.jpg"></div>
            <div class="col-4"><img src="img3.jpg"></div>
        </div>
    </div>
    <!-- Slide 2: 3 images -->
    <div class="carousel-item">
        <div class="row">
            <div class="col-4"><img src="img4.jpg"></div>
            <div class="col-4"><img src="img5.jpg"></div>
            <div class="col-4"><img src="img6.jpg"></div>
        </div>
    </div>
</div>
```

### ANALYSIS:
- Type: Multi-level carousel
- Structure: 2 slides × 3 images/slide
- **KEY**: KHÔNG dùng nested For! Dùng ForPage0, ForPage1

### OUTPUT:
```javascript
// ============================================
// SECTION TEMPLATES 10 - Desktop Gallery
// ============================================
(function() {
    'use strict';

    window.SectionTemplates10 = {
        'desktop-gallery': {
            name: 'Desktop 3-Column Gallery',
            template: `
<div class="carousel slide">
    <!-- Slide 0 -->
    <div class="carousel-item active">
        <div class="row">
            [ForPage0]
            <div class="col-4"><img src="[imageUrl]" alt="[imageAlt]"></div>
            [/ForPage0]
        </div>
    </div>
    <!-- Slide 1 -->
    <div class="carousel-item">
        <div class="row">
            [ForPage1]
            <div class="col-4"><img src="[imageUrl]" alt="[imageAlt]"></div>
            [/ForPage1]
        </div>
    </div>
</div>`,
            values: {
                "ForPage0": [
                    { "imageUrl": "img1.jpg", "imageAlt": "Image 1" },
                    { "imageUrl": "img2.jpg", "imageAlt": "Image 2" },
                    { "imageUrl": "img3.jpg", "imageAlt": "Image 3" }
                ],
                "ForPage1": [
                    { "imageUrl": "img4.jpg", "imageAlt": "Image 4" },
                    { "imageUrl": "img5.jpg", "imageAlt": "Image 5" },
                    { "imageUrl": "img6.jpg", "imageAlt": "Image 6" }
                ]
            }
        }
    };

    if (typeof window !== 'undefined') {
        window.SectionTemplates10 = SectionTemplates10;
    }

})();
```

---

## Example 5: Convert Speaker Grid + Mobile Carousel

### INPUT HTML:
```html
<section class="speakers">
    <h2>Our Speakers</h2>
    
    <!-- Desktop Grid -->
    <div class="row desktop">
        <div class="col-4">
            <img src="speaker1.jpg">
            <h3>John Doe</h3>
            <p>CEO, Company A</p>
        </div>
        <div class="col-4">
            <img src="speaker2.jpg">
            <h3>Jane Smith</h3>
            <p>CTO, Company B</p>
        </div>
        <div class="col-4">
            <img src="speaker3.jpg">
            <h3>Bob Johnson</h3>
            <p>VP, Company C</p>
        </div>
    </div>
    
    <!-- Mobile Carousel -->
    <div class="carousel mobile">
        <div class="carousel-indicators">
            <button data-bs-slide-to="0" class="active"></button>
            <button data-bs-slide-to="1"></button>
            <button data-bs-slide-to="2"></button>
        </div>
        <div class="carousel-inner">
            <div class="carousel-item active">
                <img src="speaker1.jpg">
                <h3>John Doe</h3>
                <p>CEO, Company A</p>
            </div>
            <div class="carousel-item">
                <img src="speaker2.jpg">
                <h3>Jane Smith</h3>
                <p>CTO, Company B</p>
            </div>
            <div class="carousel-item">
                <img src="speaker3.jpg">
                <h3>Bob Johnson</h3>
                <p>VP, Company C</p>
            </div>
        </div>
    </div>
</section>
```

### ANALYSIS:
- Type: Hybrid (Grid + Carousel)
- Desktop: Simple grid (ForDesktop)
- Mobile: Carousel with indicators (ForMobile)

### OUTPUT:
```javascript
// ============================================
// SECTION TEMPLATES 11 - Speaker Section
// ============================================
(function() {
    'use strict';

    window.SectionTemplates11 = {
        'speaker-section': {
            name: 'Speakers (Grid + Carousel)',
            template: `
<section class="speakers">
    <h2>[sectionTitle]</h2>
    
    <!-- Desktop Grid -->
    <div class="row desktop">
        [ForDesktop]
        <div class="col-4">
            <img src="[speakerImage]" alt="[speakerName]">
            <h3>[speakerName]</h3>
            <p>[speakerTitle]</p>
        </div>
        [/ForDesktop]
    </div>
    
    <!-- Mobile Carousel -->
    <div class="carousel mobile">
        <div class="carousel-indicators">
            [ForMobile]
            <button data-bs-slide-to="[index]" class="[activeClass]" aria-label="Slide [slideNumber]"></button>
            [/ForMobile]
        </div>
        <div class="carousel-inner">
            [ForMobile]
            <div class="carousel-item [activeClass]">
                <img src="[speakerImage]" alt="[speakerName]">
                <h3>[speakerName]</h3>
                <p>[speakerTitle]</p>
            </div>
            [/ForMobile]
        </div>
    </div>
</section>`,
            values: {
                "sectionTitle": "Our Speakers",
                
                // Desktop Grid - Simple loop
                "ForDesktop": [
                    {
                        "index": 0,
                        "speakerImage": "speaker1.jpg",
                        "speakerName": "John Doe",
                        "speakerTitle": "CEO, Company A"
                    },
                    {
                        "index": 1,
                        "speakerImage": "speaker2.jpg",
                        "speakerName": "Jane Smith",
                        "speakerTitle": "CTO, Company B"
                    },
                    {
                        "index": 2,
                        "speakerImage": "speaker3.jpg",
                        "speakerName": "Bob Johnson",
                        "speakerTitle": "VP, Company C"
                    }
                ],
                
                // Mobile Carousel - Merged indicators + slides
                "ForMobile": [
                    {
                        "index": 0,
                        "activeClass": "active",
                        "slideNumber": 1,
                        "speakerImage": "speaker1.jpg",
                        "speakerName": "John Doe",
                        "speakerTitle": "CEO, Company A"
                    },
                    {
                        "index": 1,
                        "activeClass": "",
                        "slideNumber": 2,
                        "speakerImage": "speaker2.jpg",
                        "speakerName": "Jane Smith",
                        "speakerTitle": "CTO, Company B"
                    },
                    {
                        "index": 2,
                        "activeClass": "",
                        "slideNumber": 3,
                        "speakerImage": "speaker3.jpg",
                        "speakerName": "Bob Johnson",
                        "speakerTitle": "VP, Company C"
                    }
                ]
            }
        }
    };

    if (typeof window !== 'undefined') {
        window.SectionTemplates11 = SectionTemplates11;
    }

})();
```

---

## Common Mistakes & Fixes

### ? MISTAKE 1: Separate Indicators + Slides
```javascript
// WRONG!
"ForIndicators": [ ... ],
"ForSlides": [ ... ]
```

### ? FIX:
```javascript
// CORRECT!
"ForCarousel": [
    {
        "index": 0,
        "activeClass": "active",
        "ariaCurrent": "true",
        "slideNumber": 1,
        // ... slide data
    }
]
```

---

### ? MISTAKE 2: Nested For Loops
```javascript
// WRONG!
template: `
[ForSlides]
    [ForImages]
    <img src="[url]">
    [/ForImages]
[/ForSlides]
`
```

### ? FIX:
```javascript
// CORRECT!
template: `
<!-- Slide 0 -->
<div class="slide active">
    [ForPage0]
    <img src="[url]">
    [/ForPage0]
</div>
<!-- Slide 1 -->
<div class="slide">
    [ForPage1]
    <img src="[url]">
    [/ForPage1]
</div>
`
```

---

### ? MISTAKE 3: Index from 1
```javascript
// WRONG!
"For": [
    { "index": 1, ... }
]
```

### ? FIX:
```javascript
// CORRECT!
"For": [
    { "index": 0, "slideNumber": 1, ... }
]
```

---

### ? MISTAKE 4: Boolean ariaCurrent
```javascript
// WRONG!
"ariaCurrent": true
```

### ? FIX:
```javascript
// CORRECT!
"ariaCurrent": "true"  // String!
```

---

## Testing Checklist

After generating template:
- [ ] Copy to section-templates-X.js
- [ ] Include in HTML page
- [ ] Test v?i backend C#
- [ ] Validate HTML output
- [ ] Check carousel navigation
- [ ] Test responsive (desktop + mobile)
- [ ] Verify active states
- [ ] Check console for errors

---

**See also:**
- [TEMPLATE_GENERATION_PROMPT.md](./TEMPLATE_GENERATION_PROMPT.md) - Full documentation
- [QUICK_REFERENCE.md](./QUICK_REFERENCE.md) - Quick reference card
